package com.catan.catanbackend;

import com.catan.catanbackend.config.EncryptionTestConfig;
import com.catan.catanbackend.model.Session;
import com.catan.catanbackend.model.SessionPlayer;
import com.catan.catanbackend.model.dto.*;
import com.catan.catanbackend.service.*;
import com.fasterxml.jackson.databind.ObjectMapper;
import jakarta.persistence.EntityManager;
import jakarta.persistence.PersistenceContext;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.springframework.boot.test.autoconfigure.web.servlet.AutoConfigureMockMvc;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.context.annotation.Import;
import org.springframework.http.MediaType;
import org.springframework.jdbc.core.JdbcTemplate;
import org.springframework.test.context.ActiveProfiles;
import org.springframework.test.context.TestConstructor;
import org.springframework.test.web.servlet.MockMvc;
import org.springframework.test.web.servlet.MvcResult;

import java.util.Optional;

import static org.assertj.core.api.Assertions.fail;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.post;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.status;
import static org.assertj.core.api.Assertions.assertThat;

@ActiveProfiles("test")
@SpringBootTest
@Import(EncryptionTestConfig.class)
@AutoConfigureMockMvc
@TestConstructor(autowireMode = TestConstructor.AutowireMode.ALL)
class OtherSessionTests {
    @PersistenceContext
    private EntityManager entityManager;
    private final JdbcTemplate jdbc;
    private final MockMvc mockMvc;
    private final Mapper mapper;
    private final UserService userService;
    private final ObjectMapper objectMapper;
    private final SessionService sessionService;
    private final SessionPlayerService sessionPlayerService;
    private final DevCardService devCardService;

    private static final String DEFAULT_USERNAME = "test";
    private static final String DEFAULT_PASSWORD = "123";
    private static final String DEFAULT_EMAIL = "test@test.com";
    private static Long defaultId;

    private static final String NEW_USERNAME = "newUsername";
    private static final String NEW_PASSWORD = "newPassword";
    private static final String NEW_MAIL = "newEmail@test.com";

    private static final String AUTH_HEADER = "Authorization";

    private LogInResponse logInResponse1;
    private LogInResponse logInResponse2;
    private SessionCodeDto sessionCodeDto;

    public OtherSessionTests(JdbcTemplate jdbc, MockMvc mockMvc, Mapper mapper, UserService userService, ObjectMapper objectMapper, SessionService sessionService, SessionPlayerService sessionPlayerService, DevCardService devCardService) {
        this.jdbc = jdbc;
        this.mockMvc = mockMvc;
        this.mapper = mapper;
        this.userService = userService;
        this.objectMapper = objectMapper;
        this.sessionService = sessionService;
        this.sessionPlayerService = sessionPlayerService;
        this.devCardService = devCardService;
    }

    @BeforeEach
    void setup() {
        sessionPlayerService.deleteAll();
        devCardService.deleteAll();
        sessionService.deleteAllSessions();
        userService.deleteAllUsers();
        try {
            registerAndLogin();
        } catch (Exception e) {
            fail();
        }
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

    private void registerAndLogin() throws Exception {
        mockMvc.perform(post("/api/users/register")
                .content(objectMapper.writeValueAsString(mapper.mapToEncryptedMessage(new RegisterForm(DEFAULT_USERNAME, DEFAULT_PASSWORD, DEFAULT_EMAIL)).getEncryptedMessage()))
                .contentType(MediaType.APPLICATION_JSON));

        EncryptedMessageWithKey encryptedMessageWithKey = mapper.mapToEncryptedMessage(new LogInForm(DEFAULT_USERNAME, DEFAULT_PASSWORD));
        MvcResult mvcResult = mockMvc.perform(post("/api/users/login")
                .content(objectMapper.writeValueAsString(encryptedMessageWithKey.getEncryptedMessage()))
                .contentType(MediaType.APPLICATION_JSON)).andReturn();

        String contentAsString = mvcResult.getResponse().getContentAsString();
        EncryptedResponse encryptedMessage = objectMapper.readValue(contentAsString, EncryptedResponse.class);
        logInResponse1 = (LogInResponse) mapper.mapFromEncryptedResponse(encryptedMessage, encryptedMessageWithKey.getKey(), LogInResponse.class);

        mockMvc.perform(post("/api/users/register")
                .content(objectMapper.writeValueAsString(mapper.mapToEncryptedMessage(new RegisterForm(NEW_USERNAME, NEW_PASSWORD, NEW_MAIL)).getEncryptedMessage()))
                .contentType(MediaType.APPLICATION_JSON));

        encryptedMessageWithKey = mapper.mapToEncryptedMessage(new LogInForm(NEW_USERNAME, NEW_PASSWORD));
        mvcResult = mockMvc.perform(post("/api/users/login")
                .content(objectMapper.writeValueAsString(encryptedMessageWithKey.getEncryptedMessage()))
                .contentType(MediaType.APPLICATION_JSON)).andReturn();

        contentAsString = mvcResult.getResponse().getContentAsString();
        encryptedMessage = objectMapper.readValue(contentAsString, EncryptedResponse.class);
        logInResponse2 = (LogInResponse) mapper.mapFromEncryptedResponse(encryptedMessage, encryptedMessageWithKey.getKey(), LogInResponse.class);
    }

    private void setupSession() throws Exception {
        MvcResult mvcResult = mockMvc.perform(post("/api/sessions/4")
                .header(AUTH_HEADER, logInResponse1.getFullToken())
                .contentType(MediaType.APPLICATION_JSON)).andExpect(status().isCreated()).andReturn();

        String contentAsString = mvcResult.getResponse().getContentAsString();
        sessionCodeDto = objectMapper.readValue(contentAsString, SessionCodeDto.class);
    }

    @Test
    void testSessionCreate() throws Exception {
        MvcResult mvcResult = mockMvc.perform(post("/api/sessions/4")
                .header(AUTH_HEADER, logInResponse1.getFullToken())
                .contentType(MediaType.APPLICATION_JSON)).andExpect(status().isCreated()).andReturn();

        String contentAsString = mvcResult.getResponse().getContentAsString();
        SessionCodeDto sessionCode = objectMapper.readValue(contentAsString, SessionCodeDto.class);
        Optional<Session> session = sessionService.getSessionBySessionCode(sessionCode.getCode());

        assertThat(session).isPresent();
        assertThat(session.get().getId()).isEqualTo(sessionCode.getId());
        assertThat(sessionService.getPlayers(sessionCode.getId())).hasSize(1);
    }

    @Test
    void testSessionJoin() throws Exception {
        setupSession();

        MvcResult mvcResult = mockMvc.perform(post("/api/sessions/join/"+sessionCodeDto.getCode())
                .header(AUTH_HEADER, logInResponse2.getFullToken())
                .contentType(MediaType.APPLICATION_JSON)).andExpect(status().isOk()).andReturn();

        String contentAsString = mvcResult.getResponse().getContentAsString();
        SessionCodeDto sessionCode = objectMapper.readValue(contentAsString, SessionCodeDto.class);
        assertThat(sessionCode).isNotNull();
        assertThat(sessionCode.getId()).isEqualTo(sessionCodeDto.getId());
        assertThat(sessionCode.getCode()).isEqualTo(sessionCodeDto.getCode());

        assertThat(sessionService.getPlayers(sessionCode.getId())).hasSize(2);
    }

    @Test
    void testLeaveSession() throws Exception {
        setupSession();

        MvcResult mvcResult = mockMvc.perform(post("/api/sessions/join/"+sessionCodeDto.getCode())
                .header(AUTH_HEADER, logInResponse2.getFullToken())
                .contentType(MediaType.APPLICATION_JSON)).andExpect(status().isOk()).andReturn();

        String contentAsString = mvcResult.getResponse().getContentAsString();
        SessionCodeDto sessionCode = objectMapper.readValue(contentAsString, SessionCodeDto.class);
        assertThat(sessionCode).isNotNull();
        assertThat(sessionCode.getId()).isEqualTo(sessionCodeDto.getId());
        assertThat(sessionCode.getCode()).isEqualTo(sessionCodeDto.getCode());

        assertThat(sessionService.getPlayers(sessionCode.getId())).hasSize(2);

        mockMvc.perform(post("/api/sessions/leave/"+sessionCodeDto.getCode())
                .header(AUTH_HEADER, logInResponse2.getFullToken())
                .contentType(MediaType.APPLICATION_JSON)).andExpect(status().isOk());

        assertThat(sessionService.getPlayers(sessionCode.getId()).stream().filter(SessionPlayer::getActive).toList()).hasSize(1);
    }

    @Test
    void testCloseSession() throws Exception {
        setupSession();
        Optional<Session> sessionById = sessionService.getSessionById(sessionCodeDto.getId());
        if (sessionById.isEmpty()) {
            fail("Session not found");
            return;
        }

        Session session = sessionById.get();
        session.setActive(true);
        sessionService.save(session);

        mockMvc.perform(post("/api/sessions/close")
                .header(AUTH_HEADER, logInResponse1.getFullToken())
                .contentType(MediaType.APPLICATION_JSON)).andExpect(status().isOk());

        Optional<SessionPlayer> first = sessionPlayerService.findPlayersBySessionCode(sessionCodeDto.getCode()).stream().filter(SessionPlayer::getActive).findFirst();
        assertThat(first).isEmpty();
        assertThat(sessionService.getPlayers(sessionCodeDto.getId())).hasSize(1);

        Optional<Session> updatedSession = sessionService.getSessionById(sessionCodeDto.getId());
        assertThat(updatedSession).isPresent();
        assertThat(updatedSession.get().getActive()).isFalse();
    }
}
