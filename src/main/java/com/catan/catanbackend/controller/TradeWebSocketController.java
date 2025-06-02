package com.catan.catanbackend.controller;

import com.catan.catanbackend.model.dto.move_dtos.responses.TradeResponseDto;
import com.catan.catanbackend.model.dto.move_dtos.GameMoveDto;
import com.catan.catanbackend.service.TradeService;
import lombok.RequiredArgsConstructor;
import org.springframework.messaging.handler.annotation.DestinationVariable;
import org.springframework.messaging.handler.annotation.MessageMapping;
import org.springframework.messaging.simp.SimpMessagingTemplate;
import org.springframework.stereotype.Controller;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.fasterxml.jackson.core.type.TypeReference;

import java.util.Map;


@Controller
@RequiredArgsConstructor
public class TradeWebSocketController {

    private final SimpMessagingTemplate messagingTemplate;
    private final TradeService tradeService;

    @MessageMapping("/moves/{sessionCode}")
    public void handleTradeResponse(@DestinationVariable String sessionCode, TradeResponseDto message) {
        if (message.isAccepted()) {
            tradeService.tradeBetweenPlayers(
                    message.getSessionId(),
                    message.getFromUser(),
                    message.getToUser(),
                    message.getOffered(),
                    message.getRequested()
            );
        }

        GameMoveDto response = new GameMoveDto("TRADE_RESPONSE", convertToMap(message));

        messagingTemplate.convertAndSend(
                "/topic/moves/" + sessionCode,
                response
        );
    }
    private Map<String, Object> convertToMap(Object obj) {
        ObjectMapper mapper = new ObjectMapper();
        return mapper.convertValue(obj, new TypeReference<>() {});
    }


}
