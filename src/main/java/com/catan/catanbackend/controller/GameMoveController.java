package com.catan.catanbackend.controller;

import com.catan.catanbackend.model.dto.move_dtos.TradeOfferDto;
import com.catan.catanbackend.model.dto.move_dtos.responses.TradeResponseDto;
import com.catan.catanbackend.model.dto.move_dtos.responses.GameMoveResponseDto;
import com.catan.catanbackend.model.helper.GameMoveTypeEnum;
import com.catan.catanbackend.service.TradeService;
import org.springframework.messaging.handler.annotation.DestinationVariable;
import org.springframework.messaging.handler.annotation.MessageMapping;
import org.springframework.messaging.simp.SimpMessagingTemplate;
import org.springframework.stereotype.Controller;

@Controller
public class GameMoveController {

    private final TradeService tradeService;
    private final SimpMessagingTemplate messagingTemplate;

    public GameMoveController(TradeService tradeService,
                              SimpMessagingTemplate messagingTemplate) {
        this.tradeService      = tradeService;
        this.messagingTemplate = messagingTemplate;
    }

    /**
     * Handles incoming trade offers.
     * Clients send to "/send/moves/{sessionId}" (because applicationDestinationPrefixes is "/send").
     */
    @MessageMapping("/moves/{sessionId}")
    public void handleTradeOffer(@DestinationVariable Long sessionId,
                                 TradeOfferDto offer) {
        // 1) apply the trade on the backend
        tradeService.tradeBetweenPlayers(
                sessionId,
                offer.getFromUser(),
                offer.getToUser(),
                offer.getOffered(),
                offer.getRequested()
        );

        // 2) wrap in a generic GameMoveResponseDto
        GameMoveResponseDto wrapper = new GameMoveResponseDto(
                GameMoveTypeEnum.TRADE_OFFER,
                offer
        );

        // 3a) broadcast to everyone subscribed to "/game/moves/{sessionId}"
        messagingTemplate.convertAndSend(
                "/game/moves/" + sessionId,
                wrapper
        );

        // 3b) send a personal notification to the target user on "/user/queue/moves"
        messagingTemplate.convertAndSendToUser(
                offer.getToUser(),
                "/queue/moves",
                wrapper
        );
    }

    @MessageMapping("/moves/{sessionId}/response")
    public void handleTradeResponse(@DestinationVariable Long sessionId,
                                    TradeResponseDto response) {
        // If they accepted, finalize the swap
        if (response.isAccepted()) {
            tradeService.tradeBetweenPlayers(
                    sessionId,
                    response.getToUser(),   // original offerer
                    response.getFromUser(), // responder
                    response.getOffered(),
                    response.getRequested()
            );
        }

        // Wrap and notify the original offerer
        GameMoveResponseDto wrapper = new GameMoveResponseDto(
                GameMoveTypeEnum.TRADE_RESPONSE,
                response
        );

        messagingTemplate.convertAndSendToUser(
                response.getToUser(),
                "/queue/moves",
                wrapper
        );
    }
}
