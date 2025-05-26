package com.catan.catanbackend;

import com.catan.catanbackend.model.PlayerProfile;
import com.catan.catanbackend.model.ResourceGroup;
import com.catan.catanbackend.model.dto.LogInForm;
import com.catan.catanbackend.model.dto.LogInResponse;
import com.catan.catanbackend.model.dto.RegisterForm;
import com.catan.catanbackend.repository.PlayerProfileRepository;
import com.catan.catanbackend.service.UserService;
import com.fasterxml.jackson.databind.ObjectMapper;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.autoconfigure.web.servlet.AutoConfigureMockMvc;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.http.HttpHeaders;
import org.springframework.http.MediaType;
import org.springframework.test.context.ActiveProfiles;
import org.springframework.test.web.servlet.MockMvc;
import org.springframework.test.web.servlet.MvcResult;

import java.util.UUID;

import static org.assertj.core.api.Assertions.assertThat;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.*;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.status;

@ActiveProfiles("test")
@SpringBootTest
@AutoConfigureMockMvc
class PlayerProfileTests {

    private final MockMvc mockMvc;
    private final ObjectMapper objectMapper;
    private final UserService userService;
    private final PlayerProfileRepository playerProfileRepo;

    @Autowired
    public PlayerProfileTests(MockMvc mockMvc, ObjectMapper objectMapper, UserService userService, PlayerProfileRepository playerProfileRepo) {
        this.mockMvc = mockMvc;
        this.objectMapper = objectMapper;
        this.userService = userService;
        this.playerProfileRepo = playerProfileRepo;
    }

    @BeforeEach
    void setup() {
        userService.deleteAllUsers();
        playerProfileRepo.deleteAll();
    }

    private RegisterForm createUniqueRegisterForm() {
        String uniqueSuffix = UUID.randomUUID().toString().substring(0, 8);
        String uniqueUsername = "testUser" + uniqueSuffix;
        String uniqueEmail = uniqueSuffix + "@test.com";
        return new RegisterForm(uniqueUsername, "password123", uniqueEmail);
    }

    LogInResponse registerAndLogin(RegisterForm registerForm) throws Exception {
        mockMvc.perform(post("/api/users/register")
                        .content(objectMapper.writeValueAsString(registerForm))
                        .contentType(MediaType.APPLICATION_JSON))
                .andExpect(status().isCreated());

        MvcResult mvcResult = mockMvc.perform(post("/api/users/login")
                        .content(objectMapper.writeValueAsString(new LogInForm(registerForm.getUsername(), registerForm.getPassword())))
                        .contentType(MediaType.APPLICATION_JSON))
                .andReturn();

        String contentAsString = mvcResult.getResponse().getContentAsString();
        return objectMapper.readValue(contentAsString, LogInResponse.class);
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

        Long playerId = 1L;
        MvcResult mvcResult = mockMvc.perform(get("/api/playerProfiles/{id}", playerId)
                        .header(HttpHeaders.AUTHORIZATION, logInResponse.getFullToken()))
                .andExpect(status().isOk())
                .andReturn();

        String contentAsString = mvcResult.getResponse().getContentAsString();
        PlayerProfile profile = objectMapper.readValue(contentAsString, PlayerProfile.class);
        assertThat(profile).isNotNull();
        assertThat(profile.getId()).isEqualTo(playerId);
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

        Long playerId = 1L;
        MvcResult mvcResult = mockMvc.perform(put("/api/playerProfiles/{id}/resources", playerId)
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