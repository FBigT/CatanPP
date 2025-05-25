package com.catan.catanbackend;

import com.catan.catanbackend.model.User;
import com.catan.catanbackend.model.dto.*;
import com.catan.catanbackend.service.Mapper;
import com.catan.catanbackend.service.UserService;
import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.ObjectMapper;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.springframework.boot.test.autoconfigure.web.servlet.AutoConfigureMockMvc;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.http.HttpHeaders;
import org.springframework.http.MediaType;
import org.springframework.test.context.ActiveProfiles;
import org.springframework.test.context.TestConstructor;
import org.springframework.test.web.servlet.MockMvc;
import org.springframework.test.web.servlet.MvcResult;

import static org.assertj.core.api.Assertions.assertThat;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.*;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.status;

@ActiveProfiles("test")
@SpringBootTest
@AutoConfigureMockMvc
@TestConstructor(autowireMode = TestConstructor.AutowireMode.ALL)
class UserTests {
    private final MockMvc mockMvc;
    private final UserService userService;
    private final Mapper mapper;
    private final ObjectMapper objectMapper;

    private static final String DEFAULT_USERNAME = "test";
    private static final String DEFAULT_PASSWORD = "123";
    private static Long defaultId;

    private static final String NEW_USERNAME = "newUsername";
    private static final String NEW_PASSWORD = "newPassword";
    private static final String MAIL = "newEmail@test.com";

    public UserTests(UserService userService, MockMvc mockMvc, Mapper mapper, ObjectMapper objectMapper) {
        this.userService = userService;
        this.mockMvc = mockMvc;
        this.mapper = mapper;
        this.objectMapper = objectMapper;
    }

    @BeforeEach
    void setup() {
        User test = mapper.mapRegisterFormToUser(new RegisterForm(DEFAULT_USERNAME, DEFAULT_PASSWORD, "test@test.com"));

        userService.deleteAllUsers();
        defaultId = userService.createUser(test).getId();
    }

    LogInResponse registerAndLogin(RegisterForm registerForm) throws Exception {
        mockMvc.perform(post("/api/users/register")
                        .content(objectMapper.writeValueAsString(registerForm))
                        .contentType(MediaType.APPLICATION_JSON));

        MvcResult mvcResult = mockMvc.perform(post("/api/users/login")
                        .content(objectMapper.writeValueAsString(new LogInForm(NEW_USERNAME, NEW_PASSWORD)))
                        .contentType(MediaType.APPLICATION_JSON)).andReturn();
        String contentAsString = mvcResult.getResponse().getContentAsString();
        return objectMapper.readValue(contentAsString, LogInResponse.class);
    }

    @Test
    void testLogin() throws Exception {
        MvcResult mvcResult = mockMvc.perform(post("/api/users/login")
                        .content(objectMapper.writeValueAsString(new LogInForm(DEFAULT_USERNAME, DEFAULT_PASSWORD)))
                        .contentType(MediaType.APPLICATION_JSON))
                .andExpect(status().isOk()).andReturn();
        String contentAsString = mvcResult.getResponse().getContentAsString();
        LogInResponse logInResponse = objectMapper.readValue(contentAsString, LogInResponse.class);
        assertThat(logInResponse.getToken()).isNotNull().isNotBlank().isNotEmpty();
        assertThat(logInResponse.getUsername()).isEqualTo(DEFAULT_USERNAME);

        mockMvc.perform(post("/api/users/login")
                        .content(objectMapper.writeValueAsString(new LogInForm(DEFAULT_USERNAME, "wrongPassword")))
                        .contentType(MediaType.APPLICATION_JSON))
                .andExpect(status().isUnauthorized());
    }

    @Test
    void testRegister() throws Exception {
        mockMvc.perform(post("/api/users/register")
                        .content(objectMapper.writeValueAsString(new RegisterForm(DEFAULT_USERNAME, DEFAULT_PASSWORD, "test@test.com")))
                        .contentType(MediaType.APPLICATION_JSON))
                .andExpect(status().isConflict());

        MvcResult mvcResult = mockMvc.perform(post("/api/users/register")
                        .content(objectMapper.writeValueAsString(new RegisterForm(NEW_USERNAME, DEFAULT_PASSWORD, "test@test.com")))
                        .contentType(MediaType.APPLICATION_JSON))
                .andExpect(status().isCreated()).andReturn();

        String contentAsString = mvcResult.getResponse().getContentAsString();
        UserDto registerResponse = objectMapper.readValue(contentAsString, UserDto.class);
        assertThat(registerResponse.getUsername()).isEqualTo(NEW_USERNAME);
    }

    @Test
    void testProfile() throws Exception {
        LogInResponse logInResponse = registerAndLogin(new RegisterForm(NEW_USERNAME, NEW_PASSWORD, MAIL));

        MvcResult byUsernameResult = mockMvc.perform(get("/api/users/profile/username/" + NEW_USERNAME)
                        .header(HttpHeaders.AUTHORIZATION, logInResponse.getFullToken()))
                .andExpect(status().isOk()).andReturn();

        String profileJson = byUsernameResult.getResponse().getContentAsString();
        JsonNode jsonNode = objectMapper.readTree(profileJson);
        assertThat(jsonNode).isNotNull();
        assertThat(jsonNode.get("username").asText()).isEqualTo(NEW_USERNAME);

        MvcResult byIdResult = mockMvc.perform(get("/api/users/profile/" + logInResponse.getUserId())
                        .header(HttpHeaders.AUTHORIZATION, logInResponse.getFullToken()))
                .andExpect(status().isOk()).andReturn();

        profileJson = byIdResult.getResponse().getContentAsString();
        JsonNode idJsonNode = objectMapper.readTree(profileJson);
        assertThat(idJsonNode).isNotNull();
        assertThat(idJsonNode.get("username").asText()).isEqualTo(NEW_USERNAME);

        MvcResult byJwtResult = mockMvc.perform(get("/api/users/profile")
                        .header(HttpHeaders.AUTHORIZATION, logInResponse.getFullToken()))
                .andExpect(status().isOk()).andReturn();

        profileJson = byJwtResult.getResponse().getContentAsString();
        JsonNode jwtJsonNode = objectMapper.readTree(profileJson);
        assertThat(jwtJsonNode).isNotNull();
        assertThat(jwtJsonNode.get("username").asText()).isEqualTo(NEW_USERNAME);
    }

    @Test
    void testGuest() throws Exception {
        MvcResult mvcResult = mockMvc.perform(post("/api/users/register/guest"))
                .andExpect(status().isCreated()).andReturn();

        String profileJson = mvcResult.getResponse().getContentAsString();
        GuestRegisterResponse registerResponse = objectMapper.readValue(profileJson, GuestRegisterResponse.class);
        assertThat(registerResponse).isNotNull();
        assertThat(registerResponse.getUsername()).isNotBlank().startsWith("Guest");
        assertThat(registerResponse.getGuestKey()).isNotNull().isNotBlank();

        MvcResult loginResult = mockMvc.perform(post("/api/users/login/guest")
                        .content(registerResponse.getGuestKey())
                        .contentType(MediaType.APPLICATION_JSON))
                .andExpect(status().isOk()).andReturn();

        String contentAsString = loginResult.getResponse().getContentAsString();
        LogInResponse logInResponse = objectMapper.readValue(contentAsString, LogInResponse.class);
        assertThat(logInResponse.getToken()).isNotNull().isNotBlank().isNotEmpty();
        assertThat(logInResponse.getUsername()).isEqualTo(registerResponse.getUsername());
    }

    @Test
    void testRefresh() throws Exception {
        LogInResponse logInResponse = registerAndLogin(new RegisterForm(NEW_USERNAME, NEW_PASSWORD, MAIL));

        MvcResult loginResult = mockMvc.perform(post("/api/users/refresh")
                        .content(logInResponse.getRefreshToken())
                        .contentType(MediaType.APPLICATION_JSON))
                .andExpect(status().isOk()).andReturn();
        String contentAsString = loginResult.getResponse().getContentAsString();
        LogInResponse refreshResponse = objectMapper.readValue(contentAsString, LogInResponse.class);
        assertThat(refreshResponse).isNotNull();
        assertThat(refreshResponse.getToken()).isNotNull().isNotBlank().isNotEmpty();
        assertThat(refreshResponse.getUsername()).isEqualTo(logInResponse.getUsername());
        assertThat(refreshResponse.getFullToken()).isNotEqualTo(logInResponse.getFullToken());
    }

    @Test
    void testDeactivate() throws Exception {
        LogInResponse logInResponse = registerAndLogin(new RegisterForm(NEW_USERNAME, NEW_PASSWORD, MAIL));

        MvcResult byIdResult = mockMvc.perform(get("/api/users/" + logInResponse.getUserId())
                        .header(HttpHeaders.AUTHORIZATION, logInResponse.getFullToken()))
                .andExpect(status().isOk()).andReturn();

        String contentAsString = byIdResult.getResponse().getContentAsString();
        UserDto getResponse = objectMapper.readValue(contentAsString, UserDto.class);

        MvcResult deactivateResult = mockMvc.perform(delete("/api/users/deactivate/" + logInResponse.getUserId())
                        .header(HttpHeaders.AUTHORIZATION, logInResponse.getFullToken()))
                .andExpect(status().isOk()).andReturn();
        contentAsString = deactivateResult.getResponse().getContentAsString();
        UserDto deactivateResponse = objectMapper.readValue(contentAsString, UserDto.class);

        assertThat(deactivateResponse).isNotNull();
        assertThat(deactivateResponse.getUsername()).isEqualTo(logInResponse.getUsername());
        assertThat(getResponse.getActive()).isTrue();
        assertThat(deactivateResponse.getActive()).isFalse();
    }

    @Test
    void testDelete() throws Exception {
        LogInResponse logInResponse = registerAndLogin(new RegisterForm(NEW_USERNAME, NEW_PASSWORD, MAIL));

        mockMvc.perform(get("/api/users/" + logInResponse.getUserId())
                        .header(HttpHeaders.AUTHORIZATION, logInResponse.getFullToken()))
                .andExpect(status().isOk());

        //We cannot delete other user (jwt holder id has to match parameter id)
        mockMvc.perform(delete("/api/users/forget/" + logInResponse.getUserId())
                        .header(HttpHeaders.AUTHORIZATION, logInResponse.getFullToken()))
                .andExpect(status().isOk());

        //Not found because we deleted our authenticated user
        mockMvc.perform(get("/api/users/" + logInResponse.getUserId())
                        .header(HttpHeaders.AUTHORIZATION, logInResponse.getFullToken()))
                .andExpect(status().isUnauthorized());
    }

    @Test
    void testUpdate() throws Exception {
        LogInResponse logInResponse = registerAndLogin(new RegisterForm(NEW_USERNAME, NEW_PASSWORD, MAIL));

        String updatedUsername = "updated";
        UserDto userDto = new UserDto();
        userDto.setUsername(updatedUsername);

        mockMvc.perform(put("/api/users/" + logInResponse.getUserId())
                        .content(objectMapper.writeValueAsString(userDto))
                        .contentType(MediaType.APPLICATION_JSON)
                        .header(HttpHeaders.AUTHORIZATION, logInResponse.getFullToken()))
                .andExpect(status().isOk());

        MvcResult mvcResult = mockMvc.perform(post("/api/users/login")
                        .content(objectMapper.writeValueAsString(new LogInForm(updatedUsername, NEW_PASSWORD)))
                        .contentType(MediaType.APPLICATION_JSON))
                .andExpect(status().isOk()).andReturn();
        String contentAsString = mvcResult.getResponse().getContentAsString();
        LogInResponse updatedLoginResponse = objectMapper.readValue(contentAsString, LogInResponse.class);

        mockMvc.perform(get("/api/users/username/" + updatedUsername)
                        .header(HttpHeaders.AUTHORIZATION, updatedLoginResponse.getFullToken()))
                .andExpect(status().isOk());
    }
}
