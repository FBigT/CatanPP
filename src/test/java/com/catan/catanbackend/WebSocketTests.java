package com.catan.catanbackend;

import com.catan.catanbackend.config.EncryptionTestConfig;
import com.catan.catanbackend.model.*;
import com.catan.catanbackend.model.dto.*;
import com.catan.catanbackend.model.dto.move_dtos.*;
import com.catan.catanbackend.model.dto.move_dtos.responses.*;
import com.catan.catanbackend.model.helper.GameMoveTypeEnum;
import com.catan.catanbackend.model.helper.StructureTypeEnum;
import com.catan.catanbackend.model.tile.Structure;
import com.catan.catanbackend.model.tile.Tile;
import com.catan.catanbackend.model.tile.TileCorner;
import com.catan.catanbackend.repository.RobberBlockerRepository;
import com.catan.catanbackend.repository.RobberMoveBlockerRepository;
import com.catan.catanbackend.repository.tiles.RoadRepository;
import com.catan.catanbackend.repository.tiles.StructureRepository;
import com.catan.catanbackend.repository.tiles.TileCornerRepository;
import com.catan.catanbackend.service.*;
import com.fasterxml.jackson.core.type.TypeReference;
import com.fasterxml.jackson.databind.ObjectMapper;
import jakarta.persistence.EntityManager;
import jakarta.persistence.PersistenceContext;
import org.junit.jupiter.api.Assertions;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.springframework.boot.test.autoconfigure.web.servlet.AutoConfigureMockMvc;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.boot.test.web.server.LocalServerPort;
import org.springframework.context.annotation.Import;
import org.springframework.http.MediaType;
import org.springframework.jdbc.core.JdbcTemplate;
import org.springframework.lang.NonNull;
import org.springframework.messaging.converter.MappingJackson2MessageConverter;
import org.springframework.messaging.simp.stomp.*;
import org.springframework.test.context.ActiveProfiles;
import org.springframework.test.context.TestConstructor;
import org.springframework.test.web.servlet.MockMvc;
import org.springframework.test.web.servlet.MvcResult;
import org.springframework.web.socket.WebSocketHttpHeaders;
import org.springframework.web.socket.client.standard.StandardWebSocketClient;
import org.springframework.web.socket.messaging.WebSocketStompClient;
import org.springframework.messaging.simp.stomp.StompSessionHandlerAdapter;
import org.springframework.web.socket.sockjs.client.SockJsClient;
import org.springframework.web.socket.sockjs.client.Transport;
import org.springframework.web.socket.sockjs.client.WebSocketTransport;

import java.lang.reflect.Type;
import java.util.*;
import java.util.concurrent.BlockingQueue;
import java.util.concurrent.CompletableFuture;
import java.util.concurrent.LinkedBlockingQueue;
import java.util.concurrent.TimeUnit;

import static org.assertj.core.api.Assertions.assertThat;
import static org.assertj.core.api.Assertions.fail;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.post;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.status;

@SpringBootTest(webEnvironment = SpringBootTest.WebEnvironment.RANDOM_PORT)
@ActiveProfiles("test")
@AutoConfigureMockMvc
@Import(EncryptionTestConfig.class)
@TestConstructor(autowireMode = TestConstructor.AutowireMode.ALL)
class WebSocketTests {
    @LocalServerPort
    private int port;

    private StompSession stompSession1;
    private StompSession stompSession2;
    private StompSession stompSession3;

    @PersistenceContext
    private EntityManager entityManager;
    private final RobberBlockerRepository blockerRepository;
    private final RobberMoveBlockerRepository robberMoveBlockerRepository;
    private final UserService userService;
    private final Mapper mapper;
    private final MockMvc mockMvc;
    private final ObjectMapper objectMapper;
    private final SessionService sessionService;
    private final JdbcTemplate jdbc;
    private final RoadRepository roadRepository;
    private final TileService tileService;
    private final StructureRepository structureRepository;
    private final SessionPlayerService sessionPlayerService;
    private final TileCornerRepository tileCornerRepository;
    private final SessionSaveService sessionSaveService;
    private final DevCardService devCardService;
    private final EncryptionUtils encryptionUtils;

    private final RegisterForm userForm1 = new RegisterForm("user1", "123", "test1@gmail.com");
    private final RegisterForm userForm2 = new RegisterForm("user2", "123", "test2@gmail.com");
    private final RegisterForm userForm3 = new RegisterForm("user3", "123", "test3@gmail.com");

    private Session session;
    private User user1;
    private User user2;
    private User user3;
    private LogInResponse logInResponse1;
    private LogInResponse logInResponse2;
    private LogInResponse logInResponse3;

    private final List<TileDto> tileDtos = Arrays.asList(
            new TileDto(0, 0, 0, "DESERT", 1, false),
            new TileDto(1, 0, 0, "WOOD", 1, false),
            new TileDto(0, 1, 0, "DESERT", 1, false),
            new TileDto(-1, 0, 0, "SAND", 1, false),
            new TileDto(0, -1, 0, "MOUNTAIN", 1, false),
            new TileDto(1, 1, 0, "PASTURE", 1, false),
            new TileDto(-1, -1, 0, "MOUNTAIN", 1, false),

            new TileDto(-1, 1, 0, "CLAYPIT", 1, false),
            new TileDto(1, -1, 0, "CLAYPIT", 1, false)
    );

    private SessionCode sessionCode;

    WebSocketTests(RobberBlockerRepository blockerRepository, RobberMoveBlockerRepository robberMoveBlockerRepository, UserService userService, Mapper mapper, MockMvc mockMvc, ObjectMapper objectMapper, SessionService sessionService, JdbcTemplate jdbc, RoadRepository roadRepository, TileService tileService, StructureRepository structureRepository, SessionPlayerService sessionPlayerService, TileCornerRepository tileCornerRepository, SessionSaveService sessionSaveService, DevCardService devCardService, EncryptionUtils encryptionUtils) {
        this.blockerRepository = blockerRepository;
        this.robberMoveBlockerRepository = robberMoveBlockerRepository;
        this.userService = userService;
        this.mapper = mapper;
        this.mockMvc = mockMvc;
        this.objectMapper = objectMapper;
        this.sessionService = sessionService;
        this.jdbc = jdbc;
        this.roadRepository = roadRepository;
        this.tileService = tileService;
        this.structureRepository = structureRepository;
        this.sessionPlayerService = sessionPlayerService;
        this.tileCornerRepository = tileCornerRepository;
        this.sessionSaveService = sessionSaveService;
        this.devCardService = devCardService;
        this.encryptionUtils = encryptionUtils;
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

    void cleanDatabaseKeepUsers() {
        jdbc.execute("SET REFERENTIAL_INTEGRITY to FALSE");

        List<String> tables = jdbc.queryForList(
                "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='PUBLIC'",
                String.class
        );

        for (String table : tables) {
            if (!"USERS".equalsIgnoreCase(table)) {
                jdbc.execute("TRUNCATE TABLE " + table);
            }
        }

        jdbc.execute("SET REFERENTIAL_INTEGRITY to TRUE");

        entityManager.clear();
    }

    @BeforeEach
    void setup() throws Exception {
        userService.deleteAllUsers();
        user1 = userService.createUser(mapper.mapRegisterFormToUser(userForm1));
        user2 = userService.createUser(mapper.mapRegisterFormToUser(userForm2));
        user3 = userService.createUser(mapper.mapRegisterFormToUser(userForm3));

        sessionService.createSession(user1.getId(), 4).ifPresent(code -> sessionCode = code);

        Optional<Session> dbSession = sessionService.getSessionBySessionCode(sessionCode.getCode());

        assertThat(dbSession).isPresent();
        session = dbSession.get();
        sessionService.joinSession(user2.getId(), sessionCode.getCode());

        LogInForm loginForm = new LogInForm(user1.getUsername(), userForm1.getPassword());
        EncryptedMessageWithKey encryptedMessageWithKey = mapper.mapToEncryptedMessage(loginForm);
        MvcResult result = mockMvc.perform(post("/api/users/login")
                        .content(objectMapper.writeValueAsString(encryptedMessageWithKey.getEncryptedMessage()))
                        .contentType(MediaType.APPLICATION_JSON))
                .andExpect(status().isOk())
                .andReturn();

        String responseBody = result.getResponse().getContentAsString();
        EncryptedResponse encryptedMessage = objectMapper.readValue(responseBody, EncryptedResponse.class);
        logInResponse1 = (LogInResponse) mapper.mapFromEncryptedResponse(encryptedMessage, encryptedMessageWithKey.getKey(), LogInResponse.class);

        loginForm = new LogInForm(user2.getUsername(), userForm2.getPassword());
        encryptedMessageWithKey = mapper.mapToEncryptedMessage(loginForm);
        result = mockMvc.perform(post("/api/users/login")
                        .content(objectMapper.writeValueAsString(encryptedMessageWithKey.getEncryptedMessage()))
                        .contentType(MediaType.APPLICATION_JSON))
                .andExpect(status().isOk())
                .andReturn();

        responseBody = result.getResponse().getContentAsString();
        encryptedMessage = objectMapper.readValue(responseBody, EncryptedResponse.class);
        logInResponse2 = (LogInResponse) mapper.mapFromEncryptedResponse(encryptedMessage, encryptedMessageWithKey.getKey(), LogInResponse.class);

        loginForm = new LogInForm(user3.getUsername(), userForm3.getPassword());
        encryptedMessageWithKey = mapper.mapToEncryptedMessage(loginForm);
        result = mockMvc.perform(post("/api/users/login")
                        .content(objectMapper.writeValueAsString(encryptedMessageWithKey.getEncryptedMessage()))
                        .contentType(MediaType.APPLICATION_JSON))
                .andExpect(status().isOk())
                .andReturn();

        responseBody = result.getResponse().getContentAsString();
        encryptedMessage = objectMapper.readValue(responseBody, EncryptedResponse.class);
        logInResponse3 = (LogInResponse) mapper.mapFromEncryptedResponse(encryptedMessage, encryptedMessageWithKey.getKey(), LogInResponse.class);

        List<Transport> transports = List.of(new WebSocketTransport(new StandardWebSocketClient()));
        SockJsClient sockJsClient = new SockJsClient(transports);
        MappingJackson2MessageConverter converter = new MappingJackson2MessageConverter();
        converter.getObjectMapper().registerModule(new com.fasterxml.jackson.datatype.jsr310.JavaTimeModule());
        WebSocketStompClient stompClient = new WebSocketStompClient(sockJsClient);
        stompClient.setMessageConverter(converter);

        StompSessionHandler handler = new StompSessionHandlerAdapter() {
            @Override
            public void afterConnected(StompSession session, @NonNull StompHeaders headers) {
                System.out.println("Connected session " + session.getSessionId());
            }
            @Override
            public void handleTransportError(StompSession session, @NonNull Throwable ex) {
                System.err.println("Transport error on " + session.getSessionId());
            }
        };

        String url = "ws://localhost:" + port + "/catan";

        WebSocketHttpHeaders httpHeaders1 = new WebSocketHttpHeaders();
        StompHeaders connectHeaders1 = new StompHeaders();
        connectHeaders1.add("Authorization", logInResponse1.getFullToken());
        stompSession1 = stompClient
                .connectAsync(url, httpHeaders1, connectHeaders1, handler)
                .get(5, TimeUnit.SECONDS);

        WebSocketHttpHeaders httpHeaders2 = new WebSocketHttpHeaders();
        StompHeaders connectHeaders2 = new StompHeaders();
        connectHeaders2.add("Authorization", logInResponse2.getFullToken());
        stompSession2 = stompClient
                .connectAsync(url, httpHeaders2, connectHeaders2, handler)
                .get(5, TimeUnit.SECONDS);

        WebSocketHttpHeaders httpHeaders3 = new WebSocketHttpHeaders();
        StompHeaders connectHeaders3 = new StompHeaders();
        connectHeaders3.add("Authorization", logInResponse3.getFullToken());
        stompSession3 = stompClient
                .connectAsync(url, httpHeaders3, connectHeaders3, handler)
                .get(5, TimeUnit.SECONDS);

        assertThat(stompSession1.isConnected()).isTrue();
        assertThat(stompSession2.isConnected()).isTrue();
        assertThat(stompSession3.isConnected()).isTrue();
    }

    @Test
    void testChat() throws Exception {
        CompletableFuture<ChatMessage> chatFuture1 = new CompletableFuture<>();
        CompletableFuture<ChatMessage> chatFuture2 = new CompletableFuture<>();

        String gameTopic = "/game/chat/" + sessionCode.getCode();
        String sendDest = "/send/chat/" + sessionCode.getCode();

        //Join chat
        stompSession1.subscribe(gameTopic, new StompFrameHandler() {
            @Override @NonNull public Type getPayloadType(@NonNull StompHeaders headers) {
                return ChatMessage.class;
            }
            @Override public void handleFrame(@NonNull StompHeaders headers, Object payload) {
                chatFuture1.complete((ChatMessage) payload);
            }
        });
        stompSession2.subscribe(gameTopic, new StompFrameHandler() {
            @Override @NonNull public Type getPayloadType(@NonNull StompHeaders headers) {
                return ChatMessage.class;
            }
            @Override public void handleFrame(@NonNull StompHeaders headers, Object payload) {
                chatFuture2.complete((ChatMessage) payload);
            }
        });

        Thread.sleep(500);

        stompSession1.send(sendDest, new RawChatMessage("Hello World"));

        ChatMessage received1 = chatFuture1.get(3, TimeUnit.SECONDS);
        assertThat(received1.getText()).isEqualTo("Hello World");

        ChatMessage received2 = chatFuture2.get(3, TimeUnit.SECONDS);

        assertThat(received2.getText()).isEqualTo("Hello World");
    }

    @Test
    void testJoin() throws Exception {
        CompletableFuture<JoinSessionNotification> joinFuture1 = new CompletableFuture<>();
        CompletableFuture<JoinSessionNotification> joinFuture2 = new CompletableFuture<>();

        String joinTopic = "/game/players/" + sessionCode.getCode();

        stompSession1.subscribe(getStompHeaders(joinTopic, logInResponse1), new StompFrameHandler() {
            @Override @NonNull public Type getPayloadType(@NonNull StompHeaders headers) {
                return JoinSessionNotification.class;
            }
            @Override public void handleFrame(@NonNull StompHeaders headers, Object payload) {
                joinFuture1.complete((JoinSessionNotification) payload);
            }
        });

        Thread.sleep(500);

        stompSession2.subscribe(getStompHeaders(joinTopic, logInResponse2), new StompFrameHandler() {
            @Override @NonNull public Type getPayloadType(@NonNull StompHeaders headers) {
                return JoinSessionNotification.class;
            }
            @Override public void handleFrame(@NonNull StompHeaders headers, Object payload) {
                joinFuture2.complete((JoinSessionNotification) payload);
            }
        });

        Thread.sleep(500);

        JoinSessionNotification joinReceived1 = joinFuture1.get(5, TimeUnit.SECONDS);

        assertThat(joinReceived1).isNotNull();
        assertThat(joinReceived1.getUsernames()).hasSize(2);
        assertThat(joinReceived1.getUsernames()).contains(user1.getUsername());
        assertThat(joinReceived1.getUsernames()).contains(user2.getUsername());
    }

    @Test
    void testMapGen() throws Exception {
        CompletableFuture<GameMoveDto> gameFuture1 = new CompletableFuture<>();
        CompletableFuture<GameMoveDto> gameFuture2 = new CompletableFuture<>();

        String gameTopic = "/game/move/" + sessionCode.getCode();
        String sendGameTopic = "/send/move/" + sessionCode.getCode();

        stompSession1.subscribe(getStompHeaders(gameTopic, logInResponse1), new StompFrameHandler() {
            @Override @NonNull public Type getPayloadType(@NonNull StompHeaders headers) {
                return GameMoveDto.class;
            }
            @Override public void handleFrame(@NonNull StompHeaders headers, Object payload) {
                gameFuture1.complete((GameMoveDto) payload);
            }
        });
        stompSession2.subscribe(getStompHeaders(gameTopic, logInResponse2), new StompFrameHandler() {
            @Override @NonNull public Type getPayloadType(@NonNull StompHeaders headers) {
                return GameMoveDto.class;
            }
            @Override public void handleFrame(@NonNull StompHeaders headers, Object payload) {
                gameFuture2.complete((GameMoveDto) payload);
            }
        });

        Thread.sleep(500);

        Map<String, Object> map = objectMapper.convertValue(new MapGenerationDto(tileDtos), new TypeReference<>() {
        });
        stompSession1.send(getStompHeaders(sendGameTopic, logInResponse1), new GameMoveDto(GameMoveTypeEnum.MAP_GEN.name(), map));
        GameMoveDto received1 = gameFuture1.get(5, TimeUnit.SECONDS);

        Thread.sleep(500);
        stompSession1.send(getStompHeaders(sendGameTopic, logInResponse1), new GameMoveDto(GameMoveTypeEnum.START_GAME.name(), null));

        turnOffSetup();

        stompSession2.send(getStompHeaders(sendGameTopic, logInResponse2), new GameMoveDto(GameMoveTypeEnum.REQUEST_MAP.name(), map));
        GameMoveDto received2 = gameFuture2.get(5, TimeUnit.SECONDS);

        assertThat(received1).isNotNull();
        assertThat(received2).isNotNull().isEqualTo(received1);
    }

    @Test
    void testPlaceSettlement() throws Exception {
        BlockingQueue<GameMoveDto> gameFuture1 = new LinkedBlockingQueue<>();
        BlockingQueue<GameMoveDto> gameFuture2 = new LinkedBlockingQueue<>();

        String gameTopic = "/game/move/" + sessionCode.getCode();
        String sendGameTopic = "/send/move/" + sessionCode.getCode();

        stompSession1.subscribe(getStompHeaders(gameTopic, logInResponse1), new StompFrameHandler() {
            @Override @NonNull public Type getPayloadType(@NonNull StompHeaders headers) {
                return GameMoveDto.class;
            }
            @Override public void handleFrame(@NonNull StompHeaders headers, Object payload) {
                Assertions.assertTrue(gameFuture1.offer((GameMoveDto) payload));
            }
        });
        stompSession2.subscribe(getStompHeaders(gameTopic, logInResponse2), new StompFrameHandler() {
            @Override @NonNull public Type getPayloadType(@NonNull StompHeaders headers) {
                return GameMoveDto.class;
            }
            @Override public void handleFrame(@NonNull StompHeaders headers, Object payload) {
                Assertions.assertTrue(gameFuture2.offer((GameMoveDto) payload));
            }
        });

        Thread.sleep(500);

         generateMap(sendGameTopic, gameFuture1, gameFuture2);

        turnOffSetup();

        GameMoveDto receivedUser1;
        GameMoveDto receivedUser2;

        Thread.sleep(500);

        Map<String, Object> map = objectMapper.convertValue(new PlaceStructureDto(0, 0, 3, StructureTypeEnum.SETTLEMENT.name()), new TypeReference<>() {
        });
        stompSession1.send(getStompHeaders(sendGameTopic, logInResponse1), new GameMoveDto(GameMoveTypeEnum.PLACE_STRUCTURE.name(), map));

        Thread.sleep(500);

        receivedUser1 = gameFuture1.poll(5, TimeUnit.SECONDS);
        receivedUser2 = gameFuture2.poll(5, TimeUnit.SECONDS);

        assertThat(receivedUser1).isNotNull();
        assertThat(receivedUser2).isNotNull().isEqualTo(receivedUser1);

        PlaceStructureResponseDto placeStructureResponseDto = objectMapper.convertValue(receivedUser2.getMoveData(), PlaceStructureResponseDto.class);
        assertThat(placeStructureResponseDto).isNotNull();
        assertThat(placeStructureResponseDto.getStructureType()).isEqualTo(StructureTypeEnum.SETTLEMENT.name());
        assertThat(placeStructureResponseDto.getCornerIndex()).isEqualTo(3);
        assertThat(placeStructureResponseDto.getTileX()).isZero();
        assertThat(placeStructureResponseDto.getTileY()).isZero();

        Tile tile = tileService.findByXAndYAndSession(0, 0, session.getId()).orElseThrow();

        TileCorner tileCorner = tile.getTileCorner(3).orElseThrow();
        tileCorner = tileCornerRepository.findById(tileCorner.getId().intValue()).orElseThrow();

        Structure structure = tileCorner.getStructure();

        assertThat(structure).isNotNull();
        assertThat(structure.getCorner()).isEqualTo(tileCorner);
    }

    @Test
    void testUpgradeSettlement() throws Exception {
        BlockingQueue<GameMoveDto> gameFuture1 = new LinkedBlockingQueue<>();
        BlockingQueue<GameMoveDto> gameFuture2 = new LinkedBlockingQueue<>();

        String gameTopic = "/game/move/" + sessionCode.getCode();
        String sendGameTopic = "/send/move/" + sessionCode.getCode();

        stompSession1.subscribe(getStompHeaders(gameTopic, logInResponse1), new StompFrameHandler() {
            @Override @NonNull public Type getPayloadType(@NonNull StompHeaders headers) {
                return GameMoveDto.class;
            }
            @Override public void handleFrame(@NonNull StompHeaders headers, Object payload) {
                Assertions.assertTrue(gameFuture1.offer((GameMoveDto) payload));
            }
        });
        stompSession2.subscribe(getStompHeaders(gameTopic, logInResponse2), new StompFrameHandler() {
            @Override @NonNull public Type getPayloadType(@NonNull StompHeaders headers) {
                return GameMoveDto.class;
            }
            @Override public void handleFrame(@NonNull StompHeaders headers, Object payload) {
                Assertions.assertTrue(gameFuture2.offer((GameMoveDto) payload));
            }
        });

        int millis = 500;
        Thread.sleep(millis);

        generateMap(sendGameTopic, gameFuture1, gameFuture2);

        turnOffSetup();

        //consume map gen
        GameMoveDto receivedUser1;
        GameMoveDto receivedUser2;

        Thread.sleep(millis);

        Map<String, Object> map = objectMapper.convertValue(new PlaceStructureDto(0, 0, 3, StructureTypeEnum.SETTLEMENT.name()), new TypeReference<>() {
        });
        stompSession1.send(getStompHeaders(sendGameTopic, logInResponse1), new GameMoveDto(GameMoveTypeEnum.PLACE_STRUCTURE.name(), map));

        Thread.sleep(millis);

        receivedUser1 = gameFuture1.poll(5, TimeUnit.SECONDS);
        receivedUser2 = gameFuture2.poll(5, TimeUnit.SECONDS);

        assertThat(receivedUser1).isNotNull().isEqualTo(receivedUser2);

        Thread.sleep(millis);

        map = objectMapper.convertValue(new UpgradeStructureDto(0, 0, 3), new TypeReference<>() {
        });
        stompSession1.send(getStompHeaders(sendGameTopic, logInResponse1), new GameMoveDto(GameMoveTypeEnum.UPGRADE_STRUCTURE.name(), map));

        Thread.sleep(millis);

        receivedUser1 = gameFuture1.poll(5, TimeUnit.SECONDS);
        receivedUser2 = gameFuture2.poll(5, TimeUnit.SECONDS);

        assertThat(receivedUser1).isNotNull().isEqualTo(receivedUser2);

        Tile tile = tileService.findByXAndYAndSession(0, 0, session.getId()).orElseThrow();

        TileCorner tileCorner = tile.getTileCorner(3).orElseThrow();
        tileCorner = tileCornerRepository.findById(tileCorner.getId().intValue()).orElseThrow();

        Structure structure = tileCorner.getStructure();

        assertThat(structure).isNotNull();
        assertThat(structure.getCorner()).isEqualTo(tileCorner);
        assertThat(structureRepository.findAll()).hasSize(1);
        assertThat(structureRepository.findAll().stream().filter(x -> Objects.equals(x.getStructureType().getName(), StructureTypeEnum.CITY.name()))).hasSize(1);
    }

    @Test
    void testEndTurn() throws Exception {
        BlockingQueue<GameMoveDto> gameFuture1 = new LinkedBlockingQueue<>();
        BlockingQueue<GameMoveDto> gameFuture2 = new LinkedBlockingQueue<>();

        String gameTopic = "/game/move/" + sessionCode.getCode();
        String sendGameTopic = "/send/move/" + sessionCode.getCode();

        stompSession1.subscribe(getStompHeaders(gameTopic, logInResponse1), new StompFrameHandler() {
            @Override
            @NonNull
            public Type getPayloadType(@NonNull StompHeaders headers) {
                return GameMoveDto.class;
            }

            @Override
            public void handleFrame(@NonNull StompHeaders headers, Object payload) {
                Assertions.assertTrue(gameFuture1.offer((GameMoveDto) payload));
            }
        });
        stompSession2.subscribe(getStompHeaders(gameTopic, logInResponse2), new StompFrameHandler() {
            @Override
            @NonNull
            public Type getPayloadType(@NonNull StompHeaders headers) {
                return GameMoveDto.class;
            }

            @Override
            public void handleFrame(@NonNull StompHeaders headers, Object payload) {
                Assertions.assertTrue(gameFuture2.offer((GameMoveDto) payload));
            }
        });

        Thread.sleep(500);

        generateMap(sendGameTopic, gameFuture1, gameFuture2);

        turnOffSetup();

        //consume map gen
        GameMoveDto receivedUser1;
        GameMoveDto receivedUser2;

        Thread.sleep(500);

        stompSession1.send(getStompHeaders(sendGameTopic, logInResponse1), new GameMoveDto(GameMoveTypeEnum.END_TURN.name(), null));

        Thread.sleep(500);

        receivedUser1 = gameFuture1.poll(5, TimeUnit.SECONDS);
        receivedUser2 = gameFuture2.poll(5, TimeUnit.SECONDS);

        assertThat(receivedUser1).isNotNull();
        assertThat(receivedUser2).isNotNull().isEqualTo(receivedUser1);
    }

    @Test
    void testBuyCard() throws Exception {
        BlockingQueue<GameMoveDto> gameFuture1 = new LinkedBlockingQueue<>();
        BlockingQueue<GameMoveDto> secretFuture = new LinkedBlockingQueue<>();
        BlockingQueue<GameMoveDto> gameFuture2 = new LinkedBlockingQueue<>();

        String gameTopic = "/game/move/" + sessionCode.getCode();
        String privateTopic = "/user/queue/" + sessionCode.getCode();
        String sendGameTopic = "/send/move/" + sessionCode.getCode();

        stompSession1.subscribe(getStompHeaders(gameTopic, logInResponse1), new StompFrameHandler() {
            @Override @NonNull public Type getPayloadType(@NonNull StompHeaders headers) {
                return GameMoveDto.class;
            }
            @Override public void handleFrame(@NonNull StompHeaders headers, Object payload) {
                Assertions.assertTrue(gameFuture1.offer((GameMoveDto) payload));
            }
        });

        stompSession1.subscribe(getStompHeaders(privateTopic, logInResponse1), new StompFrameHandler() {
            @Override @NonNull public Type getPayloadType(@NonNull StompHeaders headers) {
                return GameMoveDto.class;
            }
            @Override public void handleFrame(@NonNull StompHeaders headers, Object payload) {
                Assertions.assertTrue(secretFuture.offer((GameMoveDto) payload));
            }
        });

        stompSession2.subscribe(getStompHeaders(gameTopic, logInResponse2), new StompFrameHandler() {
            @Override @NonNull public Type getPayloadType(@NonNull StompHeaders headers) {
                return GameMoveDto.class;
            }
            @Override public void handleFrame(@NonNull StompHeaders headers, Object payload) {
                Assertions.assertTrue(gameFuture2.offer((GameMoveDto) payload));
            }
        });

        Thread.sleep(500);

        generateMap(sendGameTopic, gameFuture1, gameFuture2);

        GameMoveDto publicReceive1;
        GameMoveDto receivedUser2;

        turnOffSetup();

        Thread.sleep(500);

        stompSession1.send(getStompHeaders(sendGameTopic, logInResponse1), new GameMoveDto(GameMoveTypeEnum.BUY_CARD.name(), null));

        Thread.sleep(500);

        publicReceive1 = gameFuture1.poll(5, TimeUnit.SECONDS);
        receivedUser2 = gameFuture2.poll(5, TimeUnit.SECONDS);
        GameMoveDto secretReceive = secretFuture.poll(5, TimeUnit.SECONDS);

        assertThat(publicReceive1).isNotNull();
        BuyCardResponseDto buyCardResponseDto = objectMapper.convertValue(publicReceive1.getMoveData(), BuyCardResponseDto.class);

        assertThat(secretReceive).isNotNull();
        assertThat(receivedUser2).isNotNull().isEqualTo(publicReceive1).isNotEqualTo(secretReceive);
        assertThat(buyCardResponseDto).isNotNull();
        assertThat(buyCardResponseDto.getNumberOfCards()).isEqualTo(1);
        PrivateBuyCardResponse privateBuyCardResponse = objectMapper.convertValue(secretReceive.getMoveData(), PrivateBuyCardResponse.class);
        Thread.sleep(500);

        stompSession1.send(getStompHeaders(sendGameTopic, logInResponse1), new GameMoveDto(GameMoveTypeEnum.REQUEST_DEV_CARDS.name(), null));

        Thread.sleep(500);

        publicReceive1 = gameFuture1.poll(5, TimeUnit.SECONDS);
        receivedUser2 = gameFuture2.poll(5, TimeUnit.SECONDS);

        assertThat(publicReceive1).isNotNull().isEqualTo(receivedUser2);
        DevCardsListResponseDto devCardsListResponseDto = objectMapper.convertValue(publicReceive1.getMoveData(), DevCardsListResponseDto.class);
        assertThat(devCardsListResponseDto).isNotNull();
        assertThat(devCardsListResponseDto.getDevCards()).isNotNull();
        assertThat(devCardsListResponseDto.getDevCards()).hasSize(1);
        assertThat(devCardsListResponseDto.getDevCards().stream().findFirst().get().getId()).isEqualTo(privateBuyCardResponse.getCardId());
    }

    @Test
    void testTrade() throws Exception {
        BlockingQueue<GameMoveDto> gameFuture1 = new LinkedBlockingQueue<>();
        BlockingQueue<GameMoveDto> gameFuture2 = new LinkedBlockingQueue<>();

        String gameTopic = "/game/move/" + sessionCode.getCode();
        String sendGameTopic = "/send/move/" + sessionCode.getCode();

        stompSession1.subscribe(getStompHeaders(gameTopic, logInResponse1), new StompFrameHandler() {
            @Override @NonNull public Type getPayloadType(@NonNull StompHeaders headers) {
                return GameMoveDto.class;
            }
            @Override public void handleFrame(@NonNull StompHeaders headers, Object payload) {
                Assertions.assertTrue(gameFuture1.offer((GameMoveDto) payload));
            }
        });

        stompSession2.subscribe(getStompHeaders(gameTopic, logInResponse2), new StompFrameHandler() {
            @Override @NonNull public Type getPayloadType(@NonNull StompHeaders headers) {
                return GameMoveDto.class;
            }
            @Override public void handleFrame(@NonNull StompHeaders headers, Object payload) {
                Assertions.assertTrue(gameFuture2.offer((GameMoveDto) payload));
            }
        });

        Thread.sleep(500);

        generateMap(sendGameTopic, gameFuture1, gameFuture2);

        turnOffSetup();

        Thread.sleep(500);

        TradeOfferDto tradeOfferDto = new TradeOfferDto(user1.getUsername(), user2.getUsername(),
                new ResourceGroup(0, 2, 2, 2, 2, 2, 2, 2),
                new ResourceGroup(2, 0, 0, 0, 0, 0, 0, 0));

        Map<String, Object> map = objectMapper.convertValue(tradeOfferDto, new TypeReference<>() {});

        stompSession1.send(getStompHeaders(sendGameTopic, logInResponse1), new GameMoveDto(GameMoveTypeEnum.TRADE_OFFER.name(), map));

        Thread.sleep(500);

        GameMoveDto secretReceive2 = gameFuture2.poll(5, TimeUnit.SECONDS);

        assertThat(secretReceive2).isNotNull();
        TradeOfferDto receivedOffer = objectMapper.convertValue(secretReceive2.getMoveData(), TradeOfferDto.class);

        map = objectMapper.convertValue(
                new TradeResponseDto(receivedOffer, true, session.getId()),
                new TypeReference<>() {}
        );

        stompSession2.send(getStompHeaders(sendGameTopic, logInResponse2), new GameMoveDto(GameMoveTypeEnum.TRADE_RESPONSE.name(), map));

        Thread.sleep(500);

        GameMoveDto secretReceive1 = gameFuture1.poll(5, TimeUnit.SECONDS);
        assertThat(secretReceive1).isNotNull();
        TradeResponseDto tradeResponseDto = objectMapper.convertValue(secretReceive1.getMoveData(), TradeResponseDto.class);

        Optional<SessionPlayer> sessionPlayer1 = sessionPlayerService.findCurrentSessionPlayerByUserId(user1.getId());
        Optional<SessionPlayer> sessionPlayer2 = sessionPlayerService.findCurrentSessionPlayerByUserId(user2.getId());

        assertThat(tradeResponseDto).isNotNull();
        assertThat(sessionPlayer1).isPresent();
        assertThat(sessionPlayer2).isPresent();
        assertThat(sessionPlayer1.get().getBrick()).isEqualTo(12);
        assertThat(sessionPlayer1.get().getSilver()).isEqualTo(8);
        assertThat(sessionPlayer2.get().getBrick()).isEqualTo(8);
        assertThat(sessionPlayer2.get().getSilver()).isEqualTo(12);
    }

    @Test
    void testPlayCard() throws Exception {
        BlockingQueue<GameMoveDto> gameFuture1 = new LinkedBlockingQueue<>();
        BlockingQueue<GameMoveDto> secretFuture = new LinkedBlockingQueue<>();
        BlockingQueue<GameMoveDto> gameFuture2 = new LinkedBlockingQueue<>();

        String gameTopic = "/game/move/" + sessionCode.getCode();
        String privateTopic = "/user/queue/" + sessionCode.getCode();
        String sendGameTopic = "/send/move/" + sessionCode.getCode();

        stompSession1.subscribe(getStompHeaders(gameTopic, logInResponse1), new StompFrameHandler() {
            @Override @NonNull public Type getPayloadType(@NonNull StompHeaders headers) {
                return GameMoveDto.class;
            }
            @Override public void handleFrame(@NonNull StompHeaders headers, Object payload) {
                Assertions.assertTrue(gameFuture1.offer((GameMoveDto) payload));
            }
        });

        stompSession1.subscribe(getStompHeaders(privateTopic, logInResponse1), new StompFrameHandler() {
            @Override @NonNull public Type getPayloadType(@NonNull StompHeaders headers) {
                return GameMoveDto.class;
            }
            @Override public void handleFrame(@NonNull StompHeaders headers, Object payload) {
                Assertions.assertTrue(secretFuture.offer((GameMoveDto) payload));
            }
        });

        stompSession2.subscribe(getStompHeaders(gameTopic, logInResponse2), new StompFrameHandler() {
            @Override @NonNull public Type getPayloadType(@NonNull StompHeaders headers) {
                return GameMoveDto.class;
            }
            @Override public void handleFrame(@NonNull StompHeaders headers, Object payload) {
                Assertions.assertTrue(gameFuture2.offer((GameMoveDto) payload));
            }
        });

        Thread.sleep(500);

        generateMap(sendGameTopic, gameFuture1, gameFuture2);

        GameMoveDto publicReceive1;
        GameMoveDto receivedUser2;

        turnOffSetup();

        Thread.sleep(500);

        //Buy card
        stompSession1.send(getStompHeaders(sendGameTopic, logInResponse1), new GameMoveDto(GameMoveTypeEnum.BUY_CARD.name(), null));

        Thread.sleep(500);

        publicReceive1 = gameFuture1.poll(5, TimeUnit.SECONDS);
        receivedUser2 = gameFuture2.poll(5, TimeUnit.SECONDS);

        assertThat(publicReceive1).isNotNull().isEqualTo(receivedUser2);
        GameMoveDto secretReceive = secretFuture.poll(5, TimeUnit.SECONDS);
        assertThat(secretReceive).isNotNull();
        PrivateBuyCardResponse privateBuyCardResponse = objectMapper.convertValue(secretReceive.getMoveData(), PrivateBuyCardResponse.class);

        //Place structure for roads later
        Map<String, Object> map = objectMapper.convertValue(new PlaceStructureDto(0, 0, 3, StructureTypeEnum.SETTLEMENT.name()), new TypeReference<>() {
        });
        stompSession1.send(getStompHeaders(sendGameTopic, logInResponse1), new GameMoveDto(GameMoveTypeEnum.PLACE_STRUCTURE.name(), map));

        Thread.sleep(500);

        //Consume place structure
        publicReceive1 = gameFuture1.poll(5, TimeUnit.SECONDS);
        receivedUser2 = gameFuture2.poll(5, TimeUnit.SECONDS);

        assertThat(publicReceive1).isNotNull().isEqualTo(receivedUser2);

        //End turn for user1
        stompSession1.send(getStompHeaders(sendGameTopic, logInResponse1), new GameMoveDto(GameMoveTypeEnum.END_TURN.name(), null));

        //Consume end turn 1
        publicReceive1 = gameFuture1.poll(5, TimeUnit.SECONDS);
        receivedUser2 = gameFuture2.poll(5, TimeUnit.SECONDS);

        assertThat(publicReceive1).isNotNull().isEqualTo(receivedUser2);

        //End turn for user2
        stompSession2.send(getStompHeaders(sendGameTopic, logInResponse2), new GameMoveDto(GameMoveTypeEnum.END_TURN.name(), null));

        //Consume end turn 1
        publicReceive1 = gameFuture1.poll(5, TimeUnit.SECONDS);
        receivedUser2 = gameFuture2.poll(5, TimeUnit.SECONDS);

        assertThat(publicReceive1).isNotNull().isEqualTo(receivedUser2);

        Optional<SessionPlayer> currentSessionPlayerByUserId1 = sessionPlayerService.findCurrentSessionPlayerByUserId(user1.getId());
        assertThat(currentSessionPlayerByUserId1).isPresent();
        SessionPlayer startingSessionPlayer = currentSessionPlayerByUserId1.get();
        //Play card on user1
        switch (privateBuyCardResponse.getCardType()) {
            case KNIGHT -> {
                Optional<Tile> robberTileOp = tileService.getRobberTile(session.getId());
                if (robberTileOp.isEmpty()) {
                    fail();
                    return;
                }
                Tile robberTile = robberTileOp.get();
                Optional<Tile> first = tileService.findBySessionId(session.getId()).stream().filter(x
                        -> robberTile.getX() != x.getX() || robberTile.getY() != x.getY()).findFirst();

                if (first.isEmpty()) {
                    fail();
                    return;
                }
                Tile newTile = first.get();

                Map<String, Object> knightMap = objectMapper.convertValue(new RobberMoveDto(robberTile.getX(), robberTile.getY(), newTile.getX(), newTile.getY()), new TypeReference<>() {});
                map = objectMapper.convertValue(new DevCardPlayDto(privateBuyCardResponse.getCardId(), knightMap), new TypeReference<>() {
                });
                stompSession1.send(getStompHeaders(sendGameTopic, logInResponse1), new GameMoveDto(GameMoveTypeEnum.PLAY_CARD.name(), map));

                Thread.sleep(500);
                publicReceive1 = gameFuture1.poll(5, TimeUnit.SECONDS);
                receivedUser2 = gameFuture2.poll(5, TimeUnit.SECONDS);
                assertThat(publicReceive1).isNotNull();
                assertThat(receivedUser2).isNotNull().isEqualTo(publicReceive1);

                PlayCardResponseDto playCardResponseDto = objectMapper.convertValue(publicReceive1.getMoveData(), PlayCardResponseDto.class);
                RobberMoveResponseDto robberMoveDto = objectMapper.convertValue(playCardResponseDto.getMoveData(), RobberMoveResponseDto.class);
                assertThat(robberMoveDto.getDestinationTileX()).isEqualTo(newTile.getX());
                assertThat(robberMoveDto.getDestinationTileY()).isEqualTo(newTile.getY());
            }
            case VICTORY_POINT -> {
                Integer playerScore = startingSessionPlayer.getPlayerScore();

                map = objectMapper.convertValue(new DevCardPlayDto(privateBuyCardResponse.getCardId(), null), new TypeReference<>() {
                });
                stompSession1.send(getStompHeaders(sendGameTopic, logInResponse1), new GameMoveDto(GameMoveTypeEnum.PLAY_CARD.name(), map));

                Thread.sleep(500);
                publicReceive1 = gameFuture1.poll(5, TimeUnit.SECONDS);
                receivedUser2 = gameFuture2.poll(5, TimeUnit.SECONDS);

                assertThat(publicReceive1).isNotNull();
                assertThat(receivedUser2).isNotNull().isEqualTo(publicReceive1);
                PlayCardResponseDto playCardResponseDto = objectMapper.convertValue(publicReceive1.getMoveData(), PlayCardResponseDto.class);
                PlayerScoreDto playerScoreDto = objectMapper.convertValue(playCardResponseDto.getMoveData(), PlayerScoreDto.class);
                assertThat(playerScoreDto.getScore()).isEqualTo(playerScore+1);
            }
            case ROAD_BUILDING -> {
                Map<String, Object> roadMap = objectMapper.convertValue(new Place2RoadsDto(new PlaceRoadDto(0, 0, 3), new PlaceRoadDto(0, 0, 4)), new TypeReference<>() {});
                map = objectMapper.convertValue(new DevCardPlayDto(privateBuyCardResponse.getCardId(), roadMap), new TypeReference<>() {
                });
                stompSession1.send(getStompHeaders(sendGameTopic, logInResponse1), new GameMoveDto(GameMoveTypeEnum.PLAY_CARD.name(), map));

                Thread.sleep(500);
                publicReceive1 = gameFuture1.poll(5, TimeUnit.SECONDS);
                receivedUser2 = gameFuture2.poll(5, TimeUnit.SECONDS);
                assertThat(publicReceive1).isNotNull();
                assertThat(receivedUser2).isNotNull().isEqualTo(publicReceive1);

                assertThat(roadRepository.findAll()).hasSize(2);
            }
            case YEAR_OF_PLENTY -> {
                Integer silver = startingSessionPlayer.getSilver();
                Integer gold = startingSessionPlayer.getGold();
                ResourceGroup resourceGroup = new ResourceGroup();
                resourceGroup.setGold(1);
                resourceGroup.setSilver(1);
                Map<String, Object> respurceMap = objectMapper.convertValue(resourceGroup, new TypeReference<>() {
                });
                map = objectMapper.convertValue(new DevCardPlayDto(privateBuyCardResponse.getCardId(), respurceMap), new TypeReference<>() {
                });
                stompSession1.send(getStompHeaders(sendGameTopic, logInResponse1), new GameMoveDto(GameMoveTypeEnum.PLAY_CARD.name(), map));

                Thread.sleep(500);
                publicReceive1 = gameFuture1.poll(5, TimeUnit.SECONDS);
                receivedUser2 = gameFuture2.poll(5, TimeUnit.SECONDS);
                assertThat(publicReceive1).isNotNull();
                assertThat(receivedUser2).isNotNull().isEqualTo(publicReceive1);

                Optional<SessionPlayer> currentSessionPlayerOp = sessionPlayerService.findCurrentSessionPlayerByUserId(user1.getId());
                assertThat(currentSessionPlayerOp).isPresent();
                SessionPlayer currentSessionPlayer = currentSessionPlayerOp.get();

                assertThat(currentSessionPlayer.getSilver()).isEqualTo(silver+1);
                assertThat(currentSessionPlayer.getGold()).isEqualTo(gold+1);
            }
        }

    }

    @Test
    void testPlaceRoad() throws Exception {
        BlockingQueue<GameMoveDto> gameFuture1 = new LinkedBlockingQueue<>();
        BlockingQueue<GameMoveDto> gameFuture2 = new LinkedBlockingQueue<>();

        String gameTopic = "/game/move/" + sessionCode.getCode();
        String sendGameTopic = "/send/move/" + sessionCode.getCode();

        stompSession1.subscribe(getStompHeaders(gameTopic, logInResponse1), new StompFrameHandler() {
            @Override @NonNull public Type getPayloadType(@NonNull StompHeaders headers) {
                return GameMoveDto.class;
            }
            @Override public void handleFrame(@NonNull StompHeaders headers, Object payload) {
                Assertions.assertTrue(gameFuture1.offer((GameMoveDto) payload));
            }
        });
        stompSession2.subscribe(getStompHeaders(gameTopic, logInResponse2), new StompFrameHandler() {
            @Override @NonNull public Type getPayloadType(@NonNull StompHeaders headers) {
                return GameMoveDto.class;
            }
            @Override public void handleFrame(@NonNull StompHeaders headers, Object payload) {
                Assertions.assertTrue(gameFuture2.offer((GameMoveDto) payload));
            }
        });

        Thread.sleep(500);

        generateMap(sendGameTopic, gameFuture1, gameFuture2);

        turnOffSetup();

        //consume map gen
        GameMoveDto receivedUser1;
        GameMoveDto receivedUser2;

        Thread.sleep(500);

        Map<String, Object> map = objectMapper.convertValue(new PlaceStructureDto(0, 0, 3, StructureTypeEnum.SETTLEMENT.name()), new TypeReference<>() {});
        stompSession1.send(getStompHeaders(sendGameTopic, logInResponse1), new GameMoveDto(GameMoveTypeEnum.PLACE_STRUCTURE.name(), map));

        Thread.sleep(500);

        //Consume place structure to use for road
        receivedUser1 = gameFuture1.poll(5, TimeUnit.SECONDS);
        receivedUser2 = gameFuture2.poll(5, TimeUnit.SECONDS);

        assertThat(receivedUser1).isNotNull().isEqualTo(receivedUser2);

        map = objectMapper.convertValue(new PlaceRoadDto(0, 0, 3), new TypeReference<>() {});
        stompSession1.send(getStompHeaders(sendGameTopic, logInResponse1), new GameMoveDto(GameMoveTypeEnum.PLACE_ROAD.name(), map));

        Thread.sleep(500);

        receivedUser1 = gameFuture1.poll(5, TimeUnit.SECONDS);
        receivedUser2 = gameFuture2.poll(5, TimeUnit.SECONDS);

        assertThat(receivedUser1).isNotNull();
        assertThat(receivedUser2).isNotNull().isEqualTo(receivedUser1);

        PlaceRoadResponseDto placeRoadResponseDto = objectMapper.convertValue(receivedUser1.getMoveData(), PlaceRoadResponseDto.class);
        assertThat(placeRoadResponseDto).isNotNull();
        assertThat(placeRoadResponseDto.getEdgeIndex()).isEqualTo(3);
        assertThat(placeRoadResponseDto.getTileX()).isZero();
        assertThat(placeRoadResponseDto.getTileY()).isZero();
    }

    @Test
    void testLoadSave() throws Exception {
        BlockingQueue<GameMoveDto> gameFuture1 = new LinkedBlockingQueue<>();
        BlockingQueue<GameMoveDto> gameFuture2 = new LinkedBlockingQueue<>();

        String gameTopic = "/game/move/" + sessionCode.getCode();
        String sendGameTopic = "/send/move/" + sessionCode.getCode();

        stompSession1.subscribe(getStompHeaders(gameTopic, logInResponse1), new StompFrameHandler() {
            @Override
            @NonNull
            public Type getPayloadType(@NonNull StompHeaders headers) {
                return GameMoveDto.class;
            }

            @Override
            public void handleFrame(@NonNull StompHeaders headers, Object payload) {
                Assertions.assertTrue(gameFuture1.offer((GameMoveDto) payload));
            }
        });
        stompSession2.subscribe(getStompHeaders(gameTopic, logInResponse2), new StompFrameHandler() {
            @Override
            @NonNull
            public Type getPayloadType(@NonNull StompHeaders headers) {
                return GameMoveDto.class;
            }

            @Override
            public void handleFrame(@NonNull StompHeaders headers, Object payload) {
                Assertions.assertTrue(gameFuture2.offer((GameMoveDto) payload));
            }
        });

        Thread.sleep(500);

        generateMap(sendGameTopic, gameFuture1, gameFuture2);

        turnOffSetup();

        Thread.sleep(500);

        Map<String, Object> map = objectMapper.convertValue(new PlaceStructureDto(0, 0, 3, StructureTypeEnum.SETTLEMENT.name()), new TypeReference<>() {});
        stompSession1.send(getStompHeaders(sendGameTopic, logInResponse1), new GameMoveDto(GameMoveTypeEnum.PLACE_STRUCTURE.name(), map));
        Thread.sleep(500);
        map = objectMapper.convertValue(new PlaceRoadDto(0, 0, 3), new TypeReference<>() {});
        stompSession1.send(getStompHeaders(sendGameTopic, logInResponse1), new GameMoveDto(GameMoveTypeEnum.PLACE_ROAD.name(), map));

        Thread.sleep(500);

        String saveJson = sessionSaveService.createSaveJson(session.getId());
        assertThat(roadRepository.findAll()).hasSize(1);
        assertThat(structureRepository.findAll()).hasSize(1);

        cleanDatabaseKeepUsers();
        assertThat(roadRepository.findAll()).isEmpty();
        assertThat(structureRepository.findAll()).isEmpty();

        sessionSaveService.loadSave(saveJson);
        assertThat(roadRepository.findAll()).hasSize(1);
        assertThat(structureRepository.findAll()).hasSize(1);
    }

    @Test
    void testDiceRoll() throws Exception {
        BlockingQueue<GameMoveDto> gameFuture1 = new LinkedBlockingQueue<>();
        BlockingQueue<GameMoveDto> gameFuture2 = new LinkedBlockingQueue<>();

        String gameTopic = "/game/move/" + sessionCode.getCode();
        String sendGameTopic = "/send/move/" + sessionCode.getCode();

        stompSession1.subscribe(getStompHeaders(gameTopic, logInResponse1), new StompFrameHandler() {
            @Override
            @NonNull
            public Type getPayloadType(@NonNull StompHeaders headers) {
                return GameMoveDto.class;
            }

            @Override
            public void handleFrame(@NonNull StompHeaders headers, Object payload) {
                Assertions.assertTrue(gameFuture1.offer((GameMoveDto) payload));
            }
        });
        stompSession2.subscribe(getStompHeaders(gameTopic, logInResponse2), new StompFrameHandler() {
            @Override
            @NonNull
            public Type getPayloadType(@NonNull StompHeaders headers) {
                return GameMoveDto.class;
            }

            @Override
            public void handleFrame(@NonNull StompHeaders headers, Object payload) {
                Assertions.assertTrue(gameFuture2.offer((GameMoveDto) payload));
            }
        });

        Thread.sleep(500);

        generateMap(sendGameTopic, gameFuture1, gameFuture2);

        GameMoveDto receivedUser1;
        GameMoveDto receivedUser2;

        turnOffSetup();

        Thread.sleep(500);


        stompSession1.send(getStompHeaders(sendGameTopic, logInResponse1), new GameMoveDto(GameMoveTypeEnum.DICE_ROLL.name(), null));

        Thread.sleep(500);

        receivedUser1 = gameFuture1.poll(5, TimeUnit.SECONDS);
        receivedUser2 = gameFuture2.poll(5, TimeUnit.SECONDS);

        assertThat(receivedUser1).isNotNull();
        assertThat(receivedUser2).isNotNull().isEqualTo(receivedUser1);

        DiceResultDto diceResultDto = objectMapper.convertValue(receivedUser1.getMoveData(), DiceResultDto.class);

        assertThat(diceResultDto).isNotNull();
        assertThat(diceResultDto.getUsername()).isEqualTo(user1.getUsername());
    }

    @Test
    void testRobber() throws Exception {
        BlockingQueue<GameMoveDto> gameFuture1 = new LinkedBlockingQueue<>();
        BlockingQueue<GameMoveDto> gameFuture2 = new LinkedBlockingQueue<>();

        String gameTopic = "/game/move/" + sessionCode.getCode();
        String sendGameTopic = "/send/move/" + sessionCode.getCode();

        stompSession1.subscribe(getStompHeaders(gameTopic, logInResponse1), new StompFrameHandler() {
            @Override
            @NonNull
            public Type getPayloadType(@NonNull StompHeaders headers) {
                return GameMoveDto.class;
            }

            @Override
            public void handleFrame(@NonNull StompHeaders headers, Object payload) {
                Assertions.assertTrue(gameFuture1.offer((GameMoveDto) payload));
            }
        });
        stompSession2.subscribe(getStompHeaders(gameTopic, logInResponse2), new StompFrameHandler() {
            @Override
            @NonNull
            public Type getPayloadType(@NonNull StompHeaders headers) {
                return GameMoveDto.class;
            }

            @Override
            public void handleFrame(@NonNull StompHeaders headers, Object payload) {
                Assertions.assertTrue(gameFuture2.offer((GameMoveDto) payload));
            }
        });

        Thread.sleep(500);

        generateMap(sendGameTopic, gameFuture1, gameFuture2);

        GameMoveDto receivedUser1;
        GameMoveDto receivedUser2;

        turnOffSetup();

        Thread.sleep(500);
        DiceResultDto diceResultDto;
        do {
            stompSession1.send(getStompHeaders(sendGameTopic, logInResponse1), new GameMoveDto(GameMoveTypeEnum.DICE_ROLL.name(), null));

            Thread.sleep(500);

            receivedUser1 = gameFuture1.poll(5, TimeUnit.SECONDS);
            receivedUser2 = gameFuture2.poll(5, TimeUnit.SECONDS);

            assertThat(receivedUser1).isNotNull();
            assertThat(receivedUser2).isNotNull().isEqualTo(receivedUser1);

            diceResultDto = objectMapper.convertValue(receivedUser1.getMoveData(), DiceResultDto.class);

            assertThat(diceResultDto).isNotNull();
            assertThat(diceResultDto.getUsername()).isEqualTo(user1.getUsername());
        } while (diceResultDto.getRollResult() != 7);
        RobberMoveDto robberMoveDto = new RobberMoveDto(0, 0, 1, 0);
        assertThat(robberMoveBlockerRepository.findAll()).hasSize(1);

        Thread.sleep(500);

        Map<String, Object> map = objectMapper.convertValue(robberMoveDto, new TypeReference<>() {});
        stompSession1.send(getStompHeaders(sendGameTopic, logInResponse1), new GameMoveDto(GameMoveTypeEnum.ROBBER_MOVE.name(), map));

        Thread.sleep(500);

        receivedUser1 = gameFuture1.poll(5, TimeUnit.SECONDS);
        receivedUser2 = gameFuture2.poll(5, TimeUnit.SECONDS);

        assertThat(receivedUser1).isNotNull().isEqualTo(receivedUser2);
        assertThat(robberMoveBlockerRepository.findAll()).isEmpty();
        assertThat(blockerRepository.findAll()).hasSize(2);

        ResourceGroup resourceGroup = new ResourceGroup(5, 5, 5, 5, 5, 5, 5, 5);

        map = objectMapper.convertValue(resourceGroup, new TypeReference<>() {});
        stompSession1.send(getStompHeaders(sendGameTopic, logInResponse1), new GameMoveDto(GameMoveTypeEnum.PAY_DEBT.name(), map));

        Thread.sleep(500);

        receivedUser1 = gameFuture1.poll(5, TimeUnit.SECONDS);
        receivedUser2 = gameFuture2.poll(5, TimeUnit.SECONDS);
        assertThat(receivedUser1).isNotNull().isEqualTo(receivedUser2);

        map = objectMapper.convertValue(resourceGroup, new TypeReference<>() {});
        stompSession2.send(getStompHeaders(sendGameTopic, logInResponse1), new GameMoveDto(GameMoveTypeEnum.PAY_DEBT.name(), map));

        Thread.sleep(500);

        receivedUser1 = gameFuture1.poll(5, TimeUnit.SECONDS);
        receivedUser2 = gameFuture2.poll(5, TimeUnit.SECONDS);
        assertThat(receivedUser1).isNotNull().isEqualTo(receivedUser2);
        assertThat(blockerRepository.findAll()).isEmpty();
    }

    @Test
    void testSetupPhase() throws Exception {
        BlockingQueue<GameMoveDto> gameFuture1 = new LinkedBlockingQueue<>();
        BlockingQueue<GameMoveDto> gameFuture2 = new LinkedBlockingQueue<>();
        BlockingQueue<GameMoveDto> gameFuture3 = new LinkedBlockingQueue<>();
        sessionService.joinSession(user3.getId(), sessionCode.getCode());
        sessionService.save(session);

        String gameTopic = "/game/move/" + sessionCode.getCode();
        String sendGameTopic = "/send/move/" + sessionCode.getCode();

        stompSession1.subscribe(getStompHeaders(gameTopic, logInResponse1), new StompFrameHandler() {
            @Override
            @NonNull
            public Type getPayloadType(@NonNull StompHeaders headers) {
                return GameMoveDto.class;
            }

            @Override
            public void handleFrame(@NonNull StompHeaders headers, Object payload) {
                Assertions.assertTrue(gameFuture1.offer((GameMoveDto) payload));
            }
        });
        stompSession2.subscribe(getStompHeaders(gameTopic, logInResponse2), new StompFrameHandler() {
            @Override
            @NonNull
            public Type getPayloadType(@NonNull StompHeaders headers) {
                return GameMoveDto.class;
            }

            @Override
            public void handleFrame(@NonNull StompHeaders headers, Object payload) {
                Assertions.assertTrue(gameFuture2.offer((GameMoveDto) payload));
            }
        });
        stompSession3.subscribe(getStompHeaders(gameTopic, logInResponse3), new StompFrameHandler() {
            @Override
            @NonNull
            public Type getPayloadType(@NonNull StompHeaders headers) {
                return GameMoveDto.class;
            }

            @Override
            public void handleFrame(@NonNull StompHeaders headers, Object payload) {
                Assertions.assertTrue(gameFuture3.offer((GameMoveDto) payload));
            }
        });

        int millis = 1000;
        int timeout = 15;

        Thread.sleep(millis);

        Map<String, Object> map = objectMapper.convertValue(new MapGenerationDto(tileDtos), new TypeReference<>() {});
        stompSession1.send(getStompHeaders(sendGameTopic, logInResponse1), new GameMoveDto(GameMoveTypeEnum.MAP_GEN.name(), map));

        Thread.sleep(millis);

        Map<Long, StompSession> futureMap = new HashMap<>();
        futureMap.put(logInResponse1.getUserId(), stompSession1);
        futureMap.put(logInResponse2.getUserId(), stompSession2);
        futureMap.put(logInResponse3.getUserId(), stompSession3);

        List<SessionPlayer> orderedPlayers = sessionService.getPlayersInTurnOrder(session.getId());

        List<StompSession> orderedSessions = orderedPlayers.stream()
                .map(player -> futureMap.get(player.getUser().getId()))
                .toList();

        //consume map gen
        GameMoveDto receivedUser1 = gameFuture1.poll(timeout, TimeUnit.SECONDS);
        GameMoveDto receivedUser2 = gameFuture2.poll(timeout, TimeUnit.SECONDS);
        GameMoveDto receivedUser3 = gameFuture3.poll(timeout, TimeUnit.SECONDS);

        assertThat(receivedUser1).isNotNull().isEqualTo(receivedUser2).isEqualTo(receivedUser3);

        Thread.sleep(millis);

        /// Player 1
        map = objectMapper.convertValue(new PlaceStructureDto(0, 0, 3, StructureTypeEnum.SETTLEMENT.name()), new TypeReference<>() {});
        StompSession stompSession = orderedSessions.get(0);
        stompSession.send(getStompHeaders(sendGameTopic, logInResponse1), new GameMoveDto(GameMoveTypeEnum.PLACE_STRUCTURE.name(), map));

        Thread.sleep(millis);

        receivedUser1 = gameFuture1.poll(timeout, TimeUnit.SECONDS);
        receivedUser2 = gameFuture2.poll(timeout, TimeUnit.SECONDS);
        receivedUser3 = gameFuture3.poll(timeout, TimeUnit.SECONDS);

        assertThat(receivedUser1).isNotNull().isEqualTo(receivedUser2).isEqualTo(receivedUser3);

        map = objectMapper.convertValue(new PlaceRoadDto(0, 0, 3), new TypeReference<>() {});
        orderedSessions.get(0).send(getStompHeaders(sendGameTopic, logInResponse1), new GameMoveDto(GameMoveTypeEnum.PLACE_ROAD.name(), map));

        Thread.sleep(millis);

        receivedUser1 = gameFuture1.poll(timeout, TimeUnit.SECONDS);
        receivedUser2 = gameFuture2.poll(timeout, TimeUnit.SECONDS);
        receivedUser3 = gameFuture3.poll(timeout, TimeUnit.SECONDS);

        assertThat(receivedUser1).isNotNull().isEqualTo(receivedUser2).isEqualTo(receivedUser3);

        receivedUser1 = gameFuture1.poll(timeout, TimeUnit.SECONDS);
        receivedUser2 = gameFuture2.poll(timeout, TimeUnit.SECONDS);
        receivedUser3 = gameFuture3.poll(timeout, TimeUnit.SECONDS);

        assertThat(receivedUser1).isNotNull().isEqualTo(receivedUser2).isEqualTo(receivedUser3);

        /// Player 2
        map = objectMapper.convertValue(new PlaceStructureDto(1, 1, 2, StructureTypeEnum.SETTLEMENT.name()), new TypeReference<>() {});
        orderedSessions.get(1).send(getStompHeaders(sendGameTopic, logInResponse1), new GameMoveDto(GameMoveTypeEnum.PLACE_STRUCTURE.name(), map));

        Thread.sleep(millis);

        receivedUser1 = gameFuture1.poll(timeout, TimeUnit.SECONDS);
        receivedUser2 = gameFuture2.poll(timeout, TimeUnit.SECONDS);
        receivedUser3 = gameFuture3.poll(timeout, TimeUnit.SECONDS);

        assertThat(receivedUser1).isNotNull().isEqualTo(receivedUser2).isEqualTo(receivedUser3);

        map = objectMapper.convertValue(new PlaceRoadDto(1, 1, 2), new TypeReference<>() {});
        orderedSessions.get(1).send(getStompHeaders(sendGameTopic, logInResponse1), new GameMoveDto(GameMoveTypeEnum.PLACE_ROAD.name(), map));

        Thread.sleep(millis);

        receivedUser1 = gameFuture1.poll(timeout, TimeUnit.SECONDS);
        receivedUser2 = gameFuture2.poll(timeout, TimeUnit.SECONDS);
        receivedUser3 = gameFuture3.poll(timeout, TimeUnit.SECONDS);

        assertThat(receivedUser1).isNotNull().isEqualTo(receivedUser2).isEqualTo(receivedUser3);

        receivedUser1 = gameFuture1.poll(timeout, TimeUnit.SECONDS);
        receivedUser2 = gameFuture2.poll(timeout, TimeUnit.SECONDS);
        receivedUser3 = gameFuture3.poll(timeout, TimeUnit.SECONDS);

        assertThat(receivedUser1).isNotNull().isEqualTo(receivedUser2).isEqualTo(receivedUser3);

        /// Player 3
        map = objectMapper.convertValue(new PlaceStructureDto(-1, -1, 3, StructureTypeEnum.SETTLEMENT.name()), new TypeReference<>() {});
        orderedSessions.get(2).send(getStompHeaders(sendGameTopic, logInResponse1), new GameMoveDto(GameMoveTypeEnum.PLACE_STRUCTURE.name(), map));

        Thread.sleep(millis);

        receivedUser1 = gameFuture1.poll(timeout, TimeUnit.SECONDS);
        receivedUser2 = gameFuture2.poll(timeout, TimeUnit.SECONDS);
        receivedUser3 = gameFuture3.poll(timeout, TimeUnit.SECONDS);

        assertThat(receivedUser1).isNotNull().isEqualTo(receivedUser2).isEqualTo(receivedUser3);

        map = objectMapper.convertValue(new PlaceRoadDto(-1, -1, 3), new TypeReference<>() {});
        orderedSessions.get(2).send(getStompHeaders(sendGameTopic, logInResponse1), new GameMoveDto(GameMoveTypeEnum.PLACE_ROAD.name(), map));

        Thread.sleep(millis);

        receivedUser1 = gameFuture1.poll(timeout, TimeUnit.SECONDS);
        receivedUser2 = gameFuture2.poll(timeout, TimeUnit.SECONDS);
        receivedUser3 = gameFuture3.poll(timeout, TimeUnit.SECONDS);

        assertThat(receivedUser1).isNotNull().isEqualTo(receivedUser2).isEqualTo(receivedUser3);

        receivedUser1 = gameFuture1.poll(timeout, TimeUnit.SECONDS);
        receivedUser2 = gameFuture2.poll(timeout, TimeUnit.SECONDS);
        receivedUser3 = gameFuture3.poll(timeout, TimeUnit.SECONDS);

        assertThat(receivedUser1).isNotNull().isEqualTo(receivedUser2).isEqualTo(receivedUser3);

        /// Player 3
        map = objectMapper.convertValue(new PlaceStructureDto(0, 1, 3, StructureTypeEnum.SETTLEMENT.name()), new TypeReference<>() {});
        orderedSessions.get(2).send(getStompHeaders(sendGameTopic, logInResponse1), new GameMoveDto(GameMoveTypeEnum.PLACE_STRUCTURE.name(), map));

        Thread.sleep(millis);

        receivedUser1 = gameFuture1.poll(timeout, TimeUnit.SECONDS);
        receivedUser2 = gameFuture2.poll(timeout, TimeUnit.SECONDS);
        receivedUser3 = gameFuture3.poll(timeout, TimeUnit.SECONDS);

        assertThat(receivedUser1).isNotNull().isEqualTo(receivedUser2).isEqualTo(receivedUser3);

        map = objectMapper.convertValue(new PlaceRoadDto(0, 1, 3), new TypeReference<>() {});
        orderedSessions.get(2).send(getStompHeaders(sendGameTopic, logInResponse1), new GameMoveDto(GameMoveTypeEnum.PLACE_ROAD.name(), map));

        Thread.sleep(millis);

        receivedUser1 = gameFuture1.poll(timeout, TimeUnit.SECONDS);
        receivedUser2 = gameFuture2.poll(timeout, TimeUnit.SECONDS);
        receivedUser3 = gameFuture3.poll(timeout, TimeUnit.SECONDS);

        assertThat(receivedUser1).isNotNull().isEqualTo(receivedUser2).isEqualTo(receivedUser3);

        receivedUser1 = gameFuture1.poll(timeout, TimeUnit.SECONDS);
        receivedUser2 = gameFuture2.poll(timeout, TimeUnit.SECONDS);
        receivedUser3 = gameFuture3.poll(timeout, TimeUnit.SECONDS);

        assertThat(receivedUser1).isNotNull().isEqualTo(receivedUser2).isEqualTo(receivedUser3);

        /// Player 2
        map = objectMapper.convertValue(new PlaceStructureDto(-1, 0, 3, StructureTypeEnum.SETTLEMENT.name()), new TypeReference<>() {});
        orderedSessions.get(1).send(getStompHeaders(sendGameTopic, logInResponse1), new GameMoveDto(GameMoveTypeEnum.PLACE_STRUCTURE.name(), map));

        Thread.sleep(millis);

        receivedUser1 = gameFuture1.poll(timeout, TimeUnit.SECONDS);
        receivedUser2 = gameFuture2.poll(timeout, TimeUnit.SECONDS);
        receivedUser3 = gameFuture3.poll(timeout, TimeUnit.SECONDS);

        assertThat(receivedUser1).isNotNull().isEqualTo(receivedUser2).isEqualTo(receivedUser3);

        map = objectMapper.convertValue(new PlaceRoadDto(-1, 0, 3), new TypeReference<>() {});
        orderedSessions.get(1).send(getStompHeaders(sendGameTopic, logInResponse1), new GameMoveDto(GameMoveTypeEnum.PLACE_ROAD.name(), map));

        Thread.sleep(millis);

        receivedUser1 = gameFuture1.poll(timeout, TimeUnit.SECONDS);
        receivedUser2 = gameFuture2.poll(timeout, TimeUnit.SECONDS);
        receivedUser3 = gameFuture3.poll(timeout, TimeUnit.SECONDS);
//ovdje
        assertThat(receivedUser1).isNotNull().isEqualTo(receivedUser2).isEqualTo(receivedUser3);

        receivedUser1 = gameFuture1.poll(timeout, TimeUnit.SECONDS);
        receivedUser2 = gameFuture2.poll(timeout, TimeUnit.SECONDS);
        receivedUser3 = gameFuture3.poll(timeout, TimeUnit.SECONDS);

        assertThat(receivedUser1).isNotNull().isEqualTo(receivedUser2).isEqualTo(receivedUser3);

        /// Player 1
        map = objectMapper.convertValue(new PlaceStructureDto(0, -1, 3, StructureTypeEnum.SETTLEMENT.name()), new TypeReference<>() {});
        orderedSessions.get(0).send(getStompHeaders(sendGameTopic, logInResponse1), new GameMoveDto(GameMoveTypeEnum.PLACE_STRUCTURE.name(), map));

        Thread.sleep(millis);

        receivedUser1 = gameFuture1.poll(timeout, TimeUnit.SECONDS);
        receivedUser2 = gameFuture2.poll(timeout, TimeUnit.SECONDS);
        receivedUser3 = gameFuture3.poll(timeout, TimeUnit.SECONDS);

        assertThat(receivedUser1).isNotNull().isEqualTo(receivedUser2).isEqualTo(receivedUser3);

        map = objectMapper.convertValue(new PlaceRoadDto(0, -1, 3), new TypeReference<>() {});
        orderedSessions.get(0).send(getStompHeaders(sendGameTopic, logInResponse1), new GameMoveDto(GameMoveTypeEnum.PLACE_ROAD.name(), map));

        Thread.sleep(millis);

        receivedUser1 = gameFuture1.poll(timeout, TimeUnit.SECONDS);
        receivedUser2 = gameFuture2.poll(timeout, TimeUnit.SECONDS);
        receivedUser3 = gameFuture3.poll(timeout, TimeUnit.SECONDS);

        assertThat(receivedUser1).isNotNull().isEqualTo(receivedUser2).isEqualTo(receivedUser3);

        assertThat(structureRepository.findAll()).hasSize(6);
        assertThat(roadRepository.findAll()).hasSize(6);

        Thread.sleep(millis);
        orderedSessions.get(0).send(getStompHeaders(sendGameTopic, logInResponse1), new GameMoveDto(GameMoveTypeEnum.TURN_ORDER.name(), null));
        Thread.sleep(millis);
        receivedUser1 = gameFuture1.poll(timeout, TimeUnit.SECONDS);
        receivedUser2 = gameFuture2.poll(timeout, TimeUnit.SECONDS);
        receivedUser3 = gameFuture3.poll(timeout, TimeUnit.SECONDS);

        assertThat(receivedUser1).isNotNull().isEqualTo(receivedUser2).isEqualTo(receivedUser3);
        TurnOrderResponseDto dto = objectMapper.convertValue(receivedUser1.getMoveData(), TurnOrderResponseDto.class);
        assertThat(dto.getUsernames()).hasSize(3);
    }

    //Utils
    private StompHeaders getStompHeaders(String gameTopic, LogInResponse logInResponse) {
        StompHeaders headers1 = new StompHeaders();
        headers1.add("Authorization", logInResponse.getFullToken());
        headers1.setDestination(gameTopic);
        return headers1;
    }

    private void turnOffSetup(){
        //Turn off setup for most tests
        Optional<Session> sessionById = sessionService.getSessionById(session.getId());
        assertThat(sessionById).isPresent();
        session = sessionById.get();
        session.setInSetup(false);

        Optional<SessionPlayer> currentSessionPlayerByUserId = sessionPlayerService.findCurrentSessionPlayerByUserId(user1.getId());
        assertThat(currentSessionPlayerByUserId).isPresent();
        session.setCurrentPlayer(currentSessionPlayerByUserId.get());
        sessionService.save(session);
    }

    private void generateMap(String sendGameTopic, BlockingQueue<GameMoveDto> gameFuture1, BlockingQueue<GameMoveDto> gameFuture2) throws InterruptedException {
        Map<String, Object> map = objectMapper.convertValue(new MapGenerationDto(tileDtos), new TypeReference<>() {});
        stompSession1.send(getStompHeaders(sendGameTopic, logInResponse1), new GameMoveDto(GameMoveTypeEnum.MAP_GEN.name(), map));

        Thread.sleep(500);

        GameMoveDto receivedUser1 = gameFuture1.poll(5, TimeUnit.SECONDS);
        GameMoveDto receivedUser2 = gameFuture2.poll(5, TimeUnit.SECONDS);

        assertThat(receivedUser1).isNotNull().isEqualTo(receivedUser2);

        stompSession1.send(getStompHeaders(sendGameTopic, logInResponse1), new GameMoveDto(GameMoveTypeEnum.START_GAME.name(), null));

        Thread.sleep(500);

        receivedUser1 = gameFuture1.poll(5, TimeUnit.SECONDS);
        receivedUser2 = gameFuture2.poll(5, TimeUnit.SECONDS);
        assertThat(receivedUser1).isNotNull().isEqualTo(receivedUser2);
    }
}