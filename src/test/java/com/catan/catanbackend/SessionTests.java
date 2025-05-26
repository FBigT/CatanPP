package com.catan.catanbackend;

import com.catan.catanbackend.model.dto.*;
import com.fasterxml.jackson.databind.ObjectMapper;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.springframework.boot.test.autoconfigure.web.servlet.AutoConfigureMockMvc;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.http.HttpHeaders;
import org.springframework.http.MediaType;
import org.springframework.jdbc.core.JdbcTemplate;
import org.springframework.test.context.ActiveProfiles;
import org.springframework.test.context.TestConstructor;
import org.springframework.test.web.servlet.MockMvc;
import org.springframework.test.web.servlet.MvcResult;

import java.util.List;

import static org.assertj.core.api.Assertions.assertThat;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.*;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.status;

@SpringBootTest
@AutoConfigureMockMvc
@ActiveProfiles("test")
@TestConstructor(autowireMode = TestConstructor.AutowireMode.ALL)
class SessionTests {
    private final MockMvc mockMvc;
    private final ObjectMapper objectMapper;
    private final JdbcTemplate jdbc;

    private String hostToken;
    private String playerToken;

    SessionTests(MockMvc mockMvc, ObjectMapper objectMapper, JdbcTemplate jdbc) {
        this.mockMvc = mockMvc;
        this.objectMapper = objectMapper;
        this.jdbc = jdbc;
    }

    @BeforeEach
    void setUp() throws Exception {
        jdbc.execute("SET REFERENTIAL_INTEGRITY FALSE");
        jdbc.queryForList(
                "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='PUBLIC'",
                String.class
        ).forEach(tbl -> jdbc.execute("TRUNCATE TABLE " + tbl));
        jdbc.execute("SET REFERENTIAL_INTEGRITY TRUE");

        hostToken   = registerAndLogin("host",   "pw", "host@test.com").getFullToken();
        playerToken = registerAndLogin("player", "pw", "player@test.com").getFullToken();
    }

    private LogInResponse registerAndLogin(String username, String password, String email) throws Exception {
        mockMvc.perform(post("/api/users/register")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(objectMapper.writeValueAsString(new RegisterForm(username, password, email)))
                )
                .andExpect(status().isCreated());

        MvcResult login = mockMvc.perform(post("/api/users/login")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(objectMapper.writeValueAsString(new LogInForm(username, password)))
                )
                .andExpect(status().isOk())
                .andReturn();

        return objectMapper.readValue(login.getResponse().getContentAsString(), LogInResponse.class);
    }

    @Test
    void testSessionControllerAndSessionSaves() throws Exception {
        MvcResult create = mockMvc.perform(post("/api/sessions/4")
                        .header(HttpHeaders.AUTHORIZATION, hostToken)
                )
                .andExpect(status().isCreated())
                .andReturn();

        SessionCodeDto codeDto = objectMapper.readValue(
                create.getResponse().getContentAsString(),
                SessionCodeDto.class
        );
        assertThat(codeDto.getCode()).isNotBlank();

        MvcResult join = mockMvc.perform(post("/api/sessions/join/" + codeDto.getCode())
                        .header(HttpHeaders.AUTHORIZATION, playerToken)
                )
                .andExpect(status().isOk())
                .andReturn();

        SessionCodeDto joined = objectMapper.readValue(
                join.getResponse().getContentAsString(),
                SessionCodeDto.class
        );
        assertThat(joined.getCode()).isEqualTo(codeDto.getCode());

        MvcResult empty = mockMvc.perform(get("/api/sessions/saves")
                        .header(HttpHeaders.AUTHORIZATION, hostToken)
                )
                .andExpect(status().isOk())
                .andReturn();

        List<SessionSaveSimpleDto> emptyList = objectMapper.readValue(
                empty.getResponse().getContentAsString(),
                objectMapper.getTypeFactory()
                        .constructCollectionType(List.class, SessionSaveSimpleDto.class)
        );
        assertThat(emptyList).isEmpty();

        MvcResult makeSave = mockMvc.perform(post("/api/sessions/save")
                        .param("name", "Save1")
                        .header(HttpHeaders.AUTHORIZATION, playerToken)
                )
                .andExpect(status().isCreated())
                .andReturn();

        SessionSaveSimpleDto saveDto = objectMapper.readValue(
                makeSave.getResponse().getContentAsString(),
                SessionSaveSimpleDto.class
        );
        assertThat(saveDto.getName()).isEqualTo("Save1");

        MvcResult after = mockMvc.perform(get("/api/sessions/saves")
                        .header(HttpHeaders.AUTHORIZATION, hostToken)
                )
                .andExpect(status().isOk())
                .andReturn();

        List<SessionSaveSimpleDto> afterList = objectMapper.readValue(
                after.getResponse().getContentAsString(),
                objectMapper.getTypeFactory()
                        .constructCollectionType(List.class, SessionSaveSimpleDto.class)
        );
        assertThat(afterList).hasSize(1);

        Long saveId = afterList.get(0).getId();
        mockMvc.perform(delete("/api/sessions/save/" + saveId)
                        .header(HttpHeaders.AUTHORIZATION, hostToken)
                )
                .andExpect(status().isOk());

        MvcResult finalList = mockMvc.perform(get("/api/sessions/saves")
                        .header(HttpHeaders.AUTHORIZATION, hostToken)
                )
                .andExpect(status().isOk())
                .andReturn();

        List<SessionSaveSimpleDto> finalSaves = objectMapper.readValue(
                finalList.getResponse().getContentAsString(),
                objectMapper.getTypeFactory()
                        .constructCollectionType(List.class, SessionSaveSimpleDto.class)
        );
        assertThat(finalSaves).isEmpty();
    }
}
