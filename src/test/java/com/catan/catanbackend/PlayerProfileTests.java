package com.catan.catanbackend;

import com.catan.catanbackend.config.EncryptionTestConfig;
import com.catan.catanbackend.model.PlayerProfile;
import com.catan.catanbackend.model.ResourceGroup;
import com.catan.catanbackend.model.dto.*;
import com.catan.catanbackend.repository.PlayerProfileRepository;
import com.catan.catanbackend.service.Mapper;
import com.catan.catanbackend.service.UserService;
import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.ObjectMapper;
import jakarta.persistence.EntityManager;
import jakarta.persistence.PersistenceContext;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.autoconfigure.web.servlet.AutoConfigureMockMvc;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.context.annotation.Import;
import org.springframework.http.HttpHeaders;
import org.springframework.http.MediaType;
import org.springframework.jdbc.core.JdbcTemplate;
import org.springframework.test.context.ActiveProfiles;
import org.springframework.test.web.servlet.MockMvc;
import org.springframework.test.web.servlet.MvcResult;

import java.util.UUID;

import static org.assertj.core.api.Assertions.assertThat;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.*;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.status;

@ActiveProfiles("test")
@SpringBootTest
@Import(EncryptionTestConfig.class)
@AutoConfigureMockMvc
class PlayerProfileTests {

    @PersistenceContext
    private EntityManager entityManager;
    private final MockMvc mockMvc;
    private final ObjectMapper objectMapper;
    private final UserService userService;
    private final PlayerProfileRepository playerProfileRepo;
    private final Mapper mapper;
    private final JdbcTemplate jdbc;

    @Autowired
    public PlayerProfileTests(MockMvc mockMvc, ObjectMapper objectMapper, UserService userService, PlayerProfileRepository playerProfileRepo, Mapper mapper, JdbcTemplate jdbc) {
        this.mockMvc = mockMvc;
        this.objectMapper = objectMapper;
        this.userService = userService;
        this.playerProfileRepo = playerProfileRepo;
        this.mapper = mapper;
        this.jdbc = jdbc;
    }

    @BeforeEach
    void setup() {
        userService.deleteAllUsers();
        playerProfileRepo.deleteAll();
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

    private RegisterForm createUniqueRegisterForm() {
        String uniqueSuffix = UUID.randomUUID().toString().substring(0, 8);
        String uniqueUsername = "testUser" + uniqueSuffix;
        String uniqueEmail = uniqueSuffix + "@test.com";
        return new RegisterForm(uniqueUsername, "password123", uniqueEmail);
    }

    LogInResponse registerAndLogin(RegisterForm registerForm) throws Exception {
        EncryptedMessageWithKey encryptedMessageWithKey = mapper.mapToEncryptedMessage(registerForm);
        mockMvc.perform(post("/api/users/register")
                        .content(objectMapper.writeValueAsString(encryptedMessageWithKey.getEncryptedMessage()))
                        .contentType(MediaType.APPLICATION_JSON))
                .andExpect(status().isCreated());

        EncryptedMessageWithKey encryptedMessageWithKey1 = mapper.mapToEncryptedMessage(new LogInForm(registerForm.getUsername(), registerForm.getPassword()));
        MvcResult mvcResult = mockMvc.perform(post("/api/users/login")
                        .content(objectMapper.writeValueAsString(encryptedMessageWithKey1.getEncryptedMessage()))
                        .contentType(MediaType.APPLICATION_JSON))
                .andReturn();

        String contentAsString = mvcResult.getResponse().getContentAsString();
        EncryptedResponse encryptedMessage = objectMapper.readValue(contentAsString, EncryptedResponse.class);
        return (LogInResponse) mapper.mapFromEncryptedResponse(encryptedMessage, encryptedMessageWithKey1.getKey(), LogInResponse.class);
    }

    @Test
    void testGetAllProfiles() throws Exception {
        RegisterForm registerForm = createUniqueRegisterForm();
        LogInResponse logInResponse = registerAndLogin(registerForm);

        MvcResult mvcResult = mockMvc.perform(get("/api/playerProfiles")
                        .header(HttpHeaders.AUTHORIZATION, logInResponse.getFullToken()))
                .andExpect(status().isOk())
                .andReturn();

        String contentAsString = mvcResult.getResponse().getContentAsString();
        PlayerProfile[] profiles = objectMapper.readValue(contentAsString, PlayerProfile[].class);
        assertThat(profiles).isNotNull();
        assertThat(profiles).hasSizeGreaterThanOrEqualTo(0);
    }

    @Test
    void testGetProfileById() throws Exception {
        RegisterForm registerForm = createUniqueRegisterForm();
        LogInResponse logInResponse = registerAndLogin(registerForm);

        MvcResult mvcResult = mockMvc.perform(get("/api/playerProfiles/{id}", logInResponse.getUserId())
                        .header(HttpHeaders.AUTHORIZATION, logInResponse.getFullToken()))
                .andExpect(status().isOk())
                .andReturn();

        String contentAsString = mvcResult.getResponse().getContentAsString();
        JsonNode jsonNode = objectMapper.readTree(contentAsString);
        String username = jsonNode.get("username").asText();
        PlayerProfile profile = objectMapper.readValue(contentAsString, PlayerProfile.class);
        assertThat(profile).isNotNull();
        assertThat(username).isEqualTo(logInResponse.getUsername());
    }

    @Test
    void testUpdateResources() throws Exception {
        RegisterForm registerForm = createUniqueRegisterForm();
        LogInResponse logInResponse = registerAndLogin(registerForm);

        ResourceGroup newResources = new ResourceGroup();
        newResources.setBrick(5);
        newResources.setCrystal(3);
        newResources.setOre(2);
        newResources.setRice(4);
        newResources.setSheep(1);
        newResources.setSilver(0);
        newResources.setGold(0);
        newResources.setWood(6);

        MvcResult mvcResult = mockMvc.perform(put("/api/playerProfiles/{id}/resources", logInResponse.getUserId())
                        .content(objectMapper.writeValueAsString(newResources))
                        .contentType(MediaType.APPLICATION_JSON)
                        .header(HttpHeaders.AUTHORIZATION, logInResponse.getFullToken()))
                .andExpect(status().isOk())
                .andReturn();

        String contentAsString = mvcResult.getResponse().getContentAsString();
        PlayerProfile updatedProfile = objectMapper.readValue(contentAsString, PlayerProfile.class);
        assertThat(updatedProfile).isNotNull();
        assertThat(updatedProfile.getResources()).isNotNull();
        assertThat(updatedProfile.getResources().getBrick()).isEqualTo(5);
        assertThat(updatedProfile.getResources().getCrystal()).isEqualTo(3);
        assertThat(updatedProfile.getResources().getOre()).isEqualTo(2);
        assertThat(updatedProfile.getResources().getRice()).isEqualTo(4);
        assertThat(updatedProfile.getResources().getSheep()).isEqualTo(1);
        assertThat(updatedProfile.getResources().getSilver()).isEqualTo(0);
        assertThat(updatedProfile.getResources().getGold()).isEqualTo(0);
        assertThat(updatedProfile.getResources().getWood()).isEqualTo(6);
    }
}