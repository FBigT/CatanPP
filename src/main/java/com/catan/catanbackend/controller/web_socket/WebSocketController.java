package com.catan.catanbackend.controller.web_socket;

import com.catan.catanbackend.model.SessionPlayer;
import com.catan.catanbackend.model.User;
import com.catan.catanbackend.model.UserDetailsImpl;
import com.catan.catanbackend.model.dto.*;
import com.catan.catanbackend.model.dto.move_dtos.GameMoveDto;
import com.catan.catanbackend.model.dto.move_dtos.TradeOfferDto;
import com.catan.catanbackend.model.dto.move_dtos.responses.TradeResponseDto;
import com.catan.catanbackend.model.dto.move_dtos.responses.VictoryDto;
import com.catan.catanbackend.model.helper.GameMoveTypeEnum;
import com.catan.catanbackend.service.*;
import com.fasterxml.jackson.databind.ObjectMapper;
import org.springframework.messaging.MessageDeliveryException;
import org.springframework.messaging.handler.annotation.DestinationVariable;
import org.springframework.messaging.handler.annotation.MessageMapping;
import org.springframework.messaging.handler.annotation.Payload;
import org.springframework.messaging.simp.SimpMessagingTemplate;
import org.springframework.security.authentication.UsernamePasswordAuthenticationToken;
import org.springframework.stereotype.Controller;
import org.springframework.web.bind.annotation.CrossOrigin;

import java.security.Principal;
import java.util.*;

@Controller
@CrossOrigin
public class WebSocketController {
    private final SimpMessagingTemplate messagingTemplate;
    private final GameMoveHandler gameMoveHandler;
    private final SessionPlayerService sessionPlayerService;
    private final GameService gameService;
    private final ObjectMapper objectMapper;

    private static final String GAME_MOVE_DESTINATION = "/game/move/";
    private static final String USER_QUEUE_PATH = "/queue/";

    public WebSocketController(SimpMessagingTemplate messagingTemplate, GameMoveHandler gameMoveHandler, SessionPlayerService sessionPlayerService, GameService gameService, ObjectMapper objectMapper) {
        this.messagingTemplate = messagingTemplate;
        this.gameMoveHandler = gameMoveHandler;
        this.sessionPlayerService = sessionPlayerService;
        this.gameService = gameService;
        this.objectMapper = objectMapper;
    }

    @MessageMapping("/chat/{sessionCode}")
    public void sendChatMessage(@DestinationVariable String sessionCode, RawChatMessage message, Principal principal) {
        String username = principal.getName();
        messagingTemplate.convertAndSend("/game/chat/" + sessionCode, new ChatMessage(username, message));
    }

    public static void sendJoinSessionUpdate(SimpMessagingTemplate messagingTemplate, String sessionCode, List<User> users) {
        messagingTemplate.convertAndSend("/game/players/" + sessionCode, new JoinSessionNotification(users));
    }

    @MessageMapping("/move/{sessionCode}")
    public void gameMove(@DestinationVariable String sessionCode, @Payload GameMoveDto gameMoveDto, Principal principal) {
        System.out.println("🎯 [WebSocketController] === GAME MOVE RECEIVED ===");
        System.out.println("🎯 [WebSocketController] Timestamp: " + java.time.LocalDateTime.now());
        System.out.println("🎯 [WebSocketController] Session Code: " + sessionCode);
        System.out.println("🎯 [WebSocketController] Game Move Type: " + gameMoveDto.getGameMoveType());
        System.out.println("🎯 [WebSocketController] Move Data: " + gameMoveDto.getMoveData());
        System.out.println("🎯 [WebSocketController] Principal: " + principal);
        System.out.println("🎯 [WebSocketController] Principal Type: " + (principal != null ? principal.getClass().getSimpleName() : "NULL"));

        Object payload;
        Optional<SessionPlayer> winner = Optional.empty();
        SessionPlayer sessionPlayer;
        GameMoveTypeEnum gameMoveType;

        try {
            gameMoveType = GameMoveTypeEnum.valueOf(gameMoveDto.getGameMoveType());
            System.out.println("🎯 [WebSocketController] Parsed GameMoveType enum: " + gameMoveType);
        } catch (Exception e) {
            System.out.println("❌ [WebSocketController] Failed to parse GameMoveType: " + e.getMessage());
            return;
        }

        try {
            if (principal instanceof UsernamePasswordAuthenticationToken token &&
                    token.getPrincipal() instanceof UserDetailsImpl userDetails){

                System.out.println("🎯 [WebSocketController] ✅ Principal is correct type");
                System.out.println("🎯 [WebSocketController] User ID: " + userDetails.getId());
                System.out.println("🎯 [WebSocketController] Username: " + userDetails.getUsername());

                Optional<SessionPlayer> player = sessionPlayerService.findPlayerBySessionCodeAndUserId(sessionCode, userDetails.getId());

                if (player.isEmpty()) {
                    System.out.println("❌ [WebSocketController] Player not found for session: " + sessionCode + ", userId: " + userDetails.getId());
                    throw new MessageDeliveryException("Player not found");
                }

                sessionPlayer = player.get();
                System.out.println("🎯 [WebSocketController] ✅ Found SessionPlayer: " + sessionPlayer.getId());
                System.out.println("🎯 [WebSocketController] SessionPlayer Name: " + sessionPlayer.getName());
                System.out.println("🎯 [WebSocketController] SessionPlayer Resources: Ore=" + sessionPlayer.getOre() +
                        ", Rice=" + sessionPlayer.getRice() + ", Sheep=" + sessionPlayer.getSheep());

                System.out.println("🎯 [WebSocketController] 🔄 Calling gameMoveHandler.handleGameMove()...");
                payload = gameMoveHandler.handleGameMove(gameMoveType, gameMoveDto, sessionPlayer);
                System.out.println("🎯 [WebSocketController] ✅ gameMoveHandler returned payload: " + payload);
                System.out.println("🎯 [WebSocketController] Payload type: " + (payload != null ? payload.getClass().getSimpleName() : "NULL"));

                winner = gameService.checkForWinner(sessionPlayer.getSession().getId());
                System.out.println("🎯 [WebSocketController] Winner check result: " + winner.isPresent());
            } else {
                System.out.println("❌ [WebSocketController] Unsupported principal type: " + (principal != null ? principal.getClass() : "NULL"));
                throw new MessageDeliveryException("Unsupported principal type");
            }
        } catch (Exception e) {
            System.out.println("❌ [WebSocketController] Exception in gameMove: " + e.getMessage());
            System.out.println("❌ [WebSocketController] Exception type: " + e.getClass().getSimpleName());
            e.printStackTrace();
            return;
        }

        // Handle BUY_CARD specific logic with extensive debugging
        if (gameMoveType == GameMoveTypeEnum.BUY_CARD){
            messagingTemplate.convertAndSendToUser(sessionPlayer.getUser().getUsername(), USER_QUEUE_PATH + sessionCode, new GameMoveDto(GameMoveTypeEnum.PRIVATE_BUY_CARD.name(), objectMapper.convertValue(((List<?>) payload).get(0), Map.class)));
            payload = ((List<?>) payload).get(1);
            System.out.println("🎯 [WebSocketController] 🛒 === PROCESSING BUY_CARD RESPONSE ===");
            System.out.println("🎯 [WebSocketController] Payload type: " + (payload != null ? payload.getClass().getSimpleName() : "NULL"));
            System.out.println("🎯 [WebSocketController] Payload content: " + payload);

            if (payload instanceof List<?> payloadList) {
                System.out.println("🎯 [WebSocketController] Payload is List with size: " + payloadList.size());

                if (payloadList.size() >= 2) {
                    Object privateCardData = payloadList.get(0);
                    Object publicCardData = payloadList.get(1);

                    System.out.println("🎯 [WebSocketController] Private card data: " + privateCardData);
                    System.out.println("🎯 [WebSocketController] Private card data type: " + (privateCardData != null ? privateCardData.getClass().getSimpleName() : "NULL"));
                    System.out.println("🎯 [WebSocketController] Public card data: " + publicCardData);
                    System.out.println("🎯 [WebSocketController] Public card data type: " + (publicCardData != null ? publicCardData.getClass().getSimpleName() : "NULL"));

                    // Send private message to buyer
                    String destination = USER_QUEUE_PATH + sessionCode;
                    String username = sessionPlayer.getUser().getUsername();
                    System.out.println("🎯 [WebSocketController] 📤 Sending private message to: " + username);
                    System.out.println("🎯 [WebSocketController] 📤 Private destination: " + destination);

                    try {
                        GameMoveDto privateMessage = new GameMoveDto(GameMoveTypeEnum.PRIVATE_BUY_CARD.name(), objectMapper.convertValue(privateCardData, Map.class));
                        System.out.println("🎯 [WebSocketController] 📤 Private message content: " + privateMessage);

                        messagingTemplate.convertAndSendToUser(username, destination, privateMessage);
                        System.out.println("🎯 [WebSocketController] ✅ Private message sent successfully");
                    } catch (Exception e) {
                        System.out.println("❌ [WebSocketController] Failed to send private message: " + e.getMessage());
                        e.printStackTrace();
                    }

                    payload = publicCardData;
                    System.out.println("🎯 [WebSocketController] ✅ Using public payload for broadcast: " + payload);
                } else {
                    System.out.println("❌ [WebSocketController] Invalid payload list size: " + payloadList.size());
                }
            } else {
                System.out.println("❌ [WebSocketController] Payload is not a List! Type: " + (payload != null ? payload.getClass().getSimpleName() : "NULL"));
            }
        }
        String publicDestination = GAME_MOVE_DESTINATION + sessionCode;
        if (sessionPlayer.getSession().getInSetup() && gameMoveType == GameMoveTypeEnum.PLACE_ROAD) {

            System.out.println("🎯 [WebSocketController] 📤 Sending public message to: " + publicDestination);
            System.out.println("🎯 [WebSocketController] 📤 Public payload: " + payload);
            System.out.println("🎯 [WebSocketController] 📤 Public payload type: " + (payload != null ? payload.getClass().getSimpleName() : "NULL"));
            Object movePayload = ((List<?>) payload).get(0);
            Object endTurnPayload = ((List<?>) payload).get(1);
            messagingTemplate.convertAndSend(publicDestination, new GameMoveDto(gameMoveType.name(), objectMapper.convertValue(movePayload, Map.class)));
            new Thread(() -> {
                try {
                    Thread.sleep(50);
                    messagingTemplate.convertAndSend(publicDestination, new GameMoveDto(GameMoveTypeEnum.END_TURN.name(), objectMapper.convertValue(endTurnPayload, Map.class)));
                } catch (InterruptedException e) {
                    Thread.currentThread().interrupt();
                }
            }).start();
        } else {
            messagingTemplate.convertAndSend(publicDestination, new GameMoveDto(gameMoveType.name(), objectMapper.convertValue(payload, Map.class)));
        /*if (gameMoveType == GameMoveTypeEnum.TRADE_OFFER) {
            TradeOfferDto offer = objectMapper.convertValue(payload, TradeOfferDto.class);
            messagingTemplate.convertAndSendToUser(
                    offer.getToUser(),
                    USER_QUEUE_PATH + sessionCode,
                    new GameMoveDto(GameMoveTypeEnum.TRADE_OFFER.name(),
                            objectMapper.convertValue(offer, Map.class))
            );
       /* if (gameMoveType == GameMoveTypeEnum.TRADE_RESPONSE) {
            TradeResponseDto resp = objectMapper.convertValue(payload, TradeResponseDto.class);
            messagingTemplate.convertAndSendToUser(
                    resp.getToUser(),
                    USER_QUEUE_PATH + sessionCode,
                    new GameMoveDto(GameMoveTypeEnum.TRADE_RESPONSE.name(),
                            objectMapper.convertValue(resp, Map.class))
            );
            return;
        }*/
        }
        if (winner.isPresent()) {
            System.out.println("🎯 [WebSocketController] 🏆 Processing victory...");
            VictoryDto victoryPayload = new VictoryDto(sessionPlayerService.findPlayersBySessionCode(sessionCode).stream()
                    .sorted(Comparator.comparingInt(SessionPlayer::getPlayerScore)).map(x
                            -> new PlayerScoreDto(x.getName(), x.getPlayerScore())).toList());
            messagingTemplate.convertAndSend(GAME_MOVE_DESTINATION + sessionCode, new GameMoveDto(GameMoveTypeEnum.VICTORY.name(), objectMapper.convertValue(victoryPayload, Map.class)));
        }

        System.out.println("🎯 [WebSocketController] === GAME MOVE PROCESSING COMPLETE ===");
    }

}
