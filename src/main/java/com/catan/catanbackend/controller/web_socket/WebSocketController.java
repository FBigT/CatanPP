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
        Object payload;
        Optional<SessionPlayer> winner = Optional.empty();
        SessionPlayer sessionPlayer;
        GameMoveTypeEnum gameMoveType = GameMoveTypeEnum.valueOf(gameMoveDto.getGameMoveType());
        try {
            if (principal instanceof UsernamePasswordAuthenticationToken token &&
                    token.getPrincipal() instanceof UserDetailsImpl userDetails){
                Optional<SessionPlayer> player = sessionPlayerService.findPlayerBySessionCodeAndUserId(sessionCode, userDetails.getId());

                if (player.isEmpty()) {
                    throw new MessageDeliveryException("Player not found");
                }

                sessionPlayer = player.get();

                payload = gameMoveHandler.handleGameMove(gameMoveType, gameMoveDto, sessionPlayer);

                winner = gameService.checkForWinner(sessionPlayer.getSession().getId());
            } else {
                throw new MessageDeliveryException("Unsupported principal type");
            }
        } catch (Exception ignored) {
            return;
        }
        if (gameMoveType == GameMoveTypeEnum.TRADE_OFFER) {
            TradeOfferDto offer = objectMapper.convertValue(payload, TradeOfferDto.class);
            messagingTemplate.convertAndSendToUser(
                    offer.getToUser(),
                    USER_QUEUE_PATH + sessionCode,
                    new GameMoveDto(GameMoveTypeEnum.TRADE_OFFER.name(),
                            objectMapper.convertValue(offer, Map.class))
            );
            return;
        }

        if (gameMoveType == GameMoveTypeEnum.TRADE_RESPONSE) {
            TradeResponseDto resp = objectMapper.convertValue(payload, TradeResponseDto.class);
            messagingTemplate.convertAndSendToUser(
                    resp.getToUser(),
                    USER_QUEUE_PATH + sessionCode,
                    new GameMoveDto(GameMoveTypeEnum.TRADE_RESPONSE.name(),
                            objectMapper.convertValue(resp, Map.class))
            );
            return;
        }

        if (gameMoveType == GameMoveTypeEnum.BUY_CARD){
            messagingTemplate.convertAndSendToUser(sessionPlayer.getUser().getUsername(), USER_QUEUE_PATH + sessionCode, new GameMoveDto(GameMoveTypeEnum.PRIVATE_BUY_CARD.name(), objectMapper.convertValue(((List<?>) payload).get(0), Map.class)));
            payload = ((List<?>) payload).get(1);
        }

        if (sessionPlayer.getSession().getInSetup() && gameMoveType == GameMoveTypeEnum.PLACE_ROAD) {
            Object movePayload = ((List<?>) payload).get(0);
            Object endTurnPayload = ((List<?>) payload).get(1);
            messagingTemplate.convertAndSend(GAME_MOVE_DESTINATION + sessionCode, new GameMoveDto(gameMoveType.name(), objectMapper.convertValue(movePayload, Map.class)));
            messagingTemplate.convertAndSend(GAME_MOVE_DESTINATION + sessionCode, new GameMoveDto(GameMoveTypeEnum.END_TURN.name(), objectMapper.convertValue(endTurnPayload, Map.class)));
        } else {
            messagingTemplate.convertAndSend(GAME_MOVE_DESTINATION + sessionCode, new GameMoveDto(gameMoveType.name(), objectMapper.convertValue(payload, Map.class)));
        }

        if (winner.isPresent()) {
            VictoryDto victoryPayload = new VictoryDto(sessionPlayerService.findPlayersBySessionCode(sessionCode).stream()
                    .sorted(Comparator.comparingInt(SessionPlayer::getPlayerScore)).map(x
                            -> new PlayerScoreDto(x.getName(), x.getPlayerScore())).toList());
            messagingTemplate.convertAndSend(GAME_MOVE_DESTINATION + sessionCode, new GameMoveDto(GameMoveTypeEnum.VICTORY.name(), objectMapper.convertValue(victoryPayload, Map.class)));
        }
    }
}
