package com.catan.catanbackend;

import com.catan.catanbackend.config.EncryptionTestConfig;
import com.catan.catanbackend.model.User;
import com.catan.catanbackend.model.dto.*;
import com.catan.catanbackend.service.EncryptionUtils;
import com.catan.catanbackend.service.Mapper;
import com.catan.catanbackend.service.UserService;
import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.ObjectMapper;
import jakarta.persistence.EntityManager;
import jakarta.persistence.PersistenceContext;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.springframework.boot.test.autoconfigure.web.servlet.AutoConfigureMockMvc;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.context.annotation.Import;
import org.springframework.http.HttpHeaders;
import org.springframework.http.MediaType;
import org.springframework.jdbc.core.JdbcTemplate;
import org.springframework.test.context.ActiveProfiles;
import org.springframework.test.context.TestConstructor;
import org.springframework.test.web.servlet.MockMvc;
import org.springframework.test.web.servlet.MvcResult;

import static org.assertj.core.api.Assertions.assertThat;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.*;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.status;

@ActiveProfiles("test")
@SpringBootTest
@Import(EncryptionTestConfig.class)
@AutoConfigureMockMvc
@TestConstructor(autowireMode = TestConstructor.AutowireMode.ALL)
class UserTests {
    @PersistenceContext
    private EntityManager entityManager;
    private final MockMvc mockMvc;
    private final UserService userService;
    private final Mapper mapper;
    private final ObjectMapper objectMapper;
    private final EncryptionUtils encryptionUtils;
    private final JdbcTemplate jdbc;

    private static final String DEFAULT_USERNAME = "test";
    private static final String DEFAULT_PASSWORD = "123";

    private static final String NEW_USERNAME = "newUsername";
    private static final String NEW_PASSWORD = "newPassword";
    private static final String MAIL = "newEmail@test.com";

    public UserTests(UserService userService, MockMvc mockMvc, Mapper mapper, ObjectMapper objectMapper, EncryptionUtils encryptionUtils, JdbcTemplate jdbc) {
        this.userService = userService;
        this.mockMvc = mockMvc;
        this.mapper = mapper;
        this.objectMapper = objectMapper;
        this.encryptionUtils = encryptionUtils;
        this.jdbc = jdbc;
    }

    @BeforeEach
    void setup() {
        User test = mapper.mapRegisterFormToUser(new RegisterForm(DEFAULT_USERNAME, DEFAULT_PASSWORD, "test@test.com"));

        userService.deleteAllUsers();
        userService.createUser(test);
    }

    @BeforeEach
    void cleanDatabase() {
        jdbc.execute("SET REFERENTIAL_INTEGRITY to FALSE");
        jdbc.queryForList(
                "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='PUBLIC'",
                String.class
        ).forEach(table ->
                jdbc.execute("TRUNCATE TABLE " + table)
        );
        jdbc.execute("SET REFERENTIAL_INTEGRITY to TRUE");
        entityManager.clear();
    }

    LogInResponse registerAndLogin(RegisterForm registerForm) throws Exception {
        mockMvc.perform(post("/api/users/register")
                        .content(objectMapper.writeValueAsString(mapper.mapToEncryptedMessage(registerForm).getEncryptedMessage()))
                        .contentType(MediaType.APPLICATION_JSON));

        LogInForm logInForm = new LogInForm(NEW_USERNAME, NEW_PASSWORD);
        EncryptedMessageWithKey encryptedMessageWithKey = mapper.mapToEncryptedMessage(logInForm);
        MvcResult mvcResult = mockMvc.perform(post("/api/users/login")
                        .content(objectMapper.writeValueAsString(encryptedMessageWithKey.getEncryptedMessage()))
                        .contentType(MediaType.APPLICATION_JSON))
                .andExpect(status().isOk()).andReturn();
        String contentAsString = mvcResult.getResponse().getContentAsString();
        EncryptedResponse encryptedMessage = objectMapper.readValue(contentAsString, EncryptedResponse.class);

        return (LogInResponse) mapper.mapFromEncryptedResponse(encryptedMessage, encryptedMessageWithKey.getKey(), LogInResponse.class);
    }

    @Test
    void testLogin() throws Exception {
        EncryptedMessageWithKey encryptedMessageWithKey = mapper.mapToEncryptedMessage(new LogInForm(DEFAULT_USERNAME, DEFAULT_PASSWORD));
        MvcResult mvcResult = mockMvc.perform(post("/api/users/login")
                        .content(objectMapper.writeValueAsString(encryptedMessageWithKey.getEncryptedMessage()))
                        .contentType(MediaType.APPLICATION_JSON))
                .andExpect(status().isOk()).andReturn();
        String contentAsString = mvcResult.getResponse().getContentAsString();
        EncryptedResponse encryptedMessage = objectMapper.readValue(contentAsString, EncryptedResponse.class);

        LogInResponse logInResponse = (LogInResponse) mapper.mapFromEncryptedResponse(encryptedMessage, encryptedMessageWithKey.getKey(), LogInResponse.class);
        assertThat(logInResponse.getToken()).isNotNull().isNotBlank().isNotEmpty();
        assertThat(logInResponse.getUsername()).isEqualTo(DEFAULT_USERNAME);

        EncryptedMessageWithKey wrongPasswordMessage = mapper.mapToEncryptedMessage(new LogInForm(DEFAULT_USERNAME, "wrongPassword"));

        mockMvc.perform(post("/api/users/login")
                        .content(objectMapper.writeValueAsString(wrongPasswordMessage.getEncryptedMessage()))
                        .contentType(MediaType.APPLICATION_JSON))
                .andExpect(status().isUnauthorized());
    }

    @Test
    void testRegister() throws Exception {
        mockMvc.perform(post("/api/users/register")
                        .content(objectMapper.writeValueAsString(mapper.mapToEncryptedMessage(new RegisterForm(DEFAULT_USERNAME, DEFAULT_PASSWORD, "test@test.com")).getEncryptedMessage()))
                        .contentType(MediaType.APPLICATION_JSON))
                .andExpect(status().isConflict());

        EncryptedMessageWithKey encryptedMessageWithKey = mapper.mapToEncryptedMessage(new RegisterForm(NEW_USERNAME, DEFAULT_PASSWORD, "test@test.com"));
        MvcResult mvcResult = mockMvc.perform(post("/api/users/register")
                        .content(objectMapper.writeValueAsString(encryptedMessageWithKey.getEncryptedMessage()))
                        .contentType(MediaType.APPLICATION_JSON))
                .andExpect(status().isCreated()).andReturn();

        String contentAsString = mvcResult.getResponse().getContentAsString();
        EncryptedResponse encryptedMessage = objectMapper.readValue(contentAsString, EncryptedResponse.class);
        UserDto userDto = (UserDto) mapper.mapFromEncryptedResponse(encryptedMessage, encryptedMessageWithKey.getKey(), UserDto.class);
        assertThat(userDto.getUsername()).isEqualTo(NEW_USERNAME);
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
        EncryptedMessageWithKey encryptedMessageWithKey = mapper.mapToEncryptedMessage(null);
        MvcResult mvcResult = mockMvc.perform(post("/api/users/register/guest")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(objectMapper.writeValueAsString(encryptedMessageWithKey.getEncryptedMessage())))
                .andExpect(status().isCreated()).andReturn();

        String profileJson = mvcResult.getResponse().getContentAsString();
        EncryptedResponse encryptedResponse = objectMapper.readValue(profileJson, EncryptedResponse.class);
        GuestRegisterResponse registerResponse = (GuestRegisterResponse) mapper.mapFromEncryptedResponse(encryptedResponse, encryptedMessageWithKey.getKey(), GuestRegisterResponse.class);

        assertThat(registerResponse).isNotNull();
        assertThat(registerResponse.getUsername()).isNotBlank().startsWith("Guest");
        assertThat(registerResponse.getGuestKey()).isNotNull().isNotBlank();

        encryptedMessageWithKey = mapper.mapToEncryptedMessage(new RefreshRequest(registerResponse.getGuestKey()));

        MvcResult loginResult = mockMvc.perform(post("/api/users/login/guest")
                        .content(objectMapper.writeValueAsString(encryptedMessageWithKey.getEncryptedMessage()))
                        .contentType(MediaType.APPLICATION_JSON))
                .andExpect(status().isOk()).andReturn();

        String contentAsString = loginResult.getResponse().getContentAsString();

        encryptedResponse = objectMapper.readValue(contentAsString, EncryptedResponse.class);
        LogInResponse logInResponse = (LogInResponse) mapper.mapFromEncryptedResponse(encryptedResponse, encryptedMessageWithKey.getKey(), LogInResponse.class);
        assertThat(logInResponse.getToken()).isNotNull().isNotBlank().isNotEmpty();
        assertThat(logInResponse.getUsername()).isEqualTo(registerResponse.getUsername());
    }

    @Test
    void testRefresh() throws Exception {
        LogInResponse logInResponse = registerAndLogin(new RegisterForm(NEW_USERNAME, NEW_PASSWORD, MAIL));

        EncryptedMessageWithKey encryptedMessageWithKey = mapper.mapToEncryptedMessage(new RefreshRequest(logInResponse.getRefreshToken()));
        MvcResult loginResult = mockMvc.perform(post("/api/users/refresh")
                        .content(objectMapper.writeValueAsString(encryptedMessageWithKey.getEncryptedMessage()))
                        .contentType(MediaType.APPLICATION_JSON))
                .andExpect(status().isOk()).andReturn();
        String contentAsString = loginResult.getResponse().getContentAsString();
        EncryptedResponse encryptedMessage = objectMapper.readValue(contentAsString, EncryptedResponse.class);

        LogInResponse refreshResponse = (LogInResponse) mapper.mapFromEncryptedResponse(encryptedMessage, encryptedMessageWithKey.getKey(), LogInResponse.class);
        assertThat(refreshResponse).isNotNull();
        assertThat(refreshResponse.getToken()).isNotNull().isNotBlank().isNotEmpty();
        assertThat(refreshResponse.getUsername()).isEqualTo(logInResponse.getUsername());
        assertThat(refreshResponse.getFullToken()).isNotEqualTo(logInResponse.getFullToken());
    }

    @Test
    void testDeactivate() throws Exception {
        LogInResponse logInResponse = registerAndLogin(new RegisterForm(NEW_USERNAME, NEW_PASSWORD, MAIL));

        mockMvc.perform(get("/api/users/" + logInResponse.getUserId())
                        .header(HttpHeaders.AUTHORIZATION, logInResponse.getFullToken()))
                .andExpect(status().isOk());


        mockMvc.perform(delete("/api/users/deactivate/" + logInResponse.getUserId())
                        .header(HttpHeaders.AUTHORIZATION, logInResponse.getFullToken()))
                .andExpect(status().isOk());

        mockMvc.perform(get("/api/users/" + logInResponse.getUserId())
                        .header(HttpHeaders.AUTHORIZATION, logInResponse.getFullToken()))
                .andExpect(status().isNotFound());
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

        EncryptedMessageWithKey encryptedMessageWithKey = mapper.mapToEncryptedMessage(new LogInForm(updatedUsername, NEW_PASSWORD));
        MvcResult mvcResult = mockMvc.perform(post("/api/users/login")
                        .content(objectMapper.writeValueAsString(encryptedMessageWithKey.getEncryptedMessage()))
                        .contentType(MediaType.APPLICATION_JSON))
                .andExpect(status().isOk()).andReturn();
        String contentAsString = mvcResult.getResponse().getContentAsString();
        EncryptedResponse encryptedMessage = objectMapper.readValue(contentAsString, EncryptedResponse.class);

        LogInResponse updatedLoginResponse = (LogInResponse) mapper.mapFromEncryptedResponse(encryptedMessage, encryptedMessageWithKey.getKey(), LogInResponse.class);
        mockMvc.perform(get("/api/users/username/" + updatedUsername)
                        .header(HttpHeaders.AUTHORIZATION, updatedLoginResponse.getFullToken()))
                .andExpect(status().isOk());
    }
}
