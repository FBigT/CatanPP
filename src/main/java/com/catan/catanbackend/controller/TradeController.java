package com.catan.catanbackend.controller;

import com.catan.catanbackend.model.dto.PlayerTradeDto;
import com.catan.catanbackend.model.dto.TradeExecutedDto;
import com.catan.catanbackend.model.dto.move_dtos.responses.TradeResponseMessage;
import com.catan.catanbackend.model.dto.TradeOfferMessage;
import com.catan.catanbackend.model.dto.move_dtos.GameMoveDto;
import com.catan.catanbackend.model.helper.GameMoveTypeEnum;
import com.catan.catanbackend.service.TradeService;
import com.fasterxml.jackson.core.type.TypeReference;
import com.fasterxml.jackson.databind.ObjectMapper;
import lombok.RequiredArgsConstructor;
import org.springframework.http.ResponseEntity;
import org.springframework.messaging.simp.SimpMessagingTemplate;
import org.springframework.web.bind.annotation.*;

import java.util.Map;

@RestController
@RequestMapping("/api/trade")
@CrossOrigin(origins = "*")
@RequiredArgsConstructor
public class TradeController {

    private final TradeService          tradeService;
    private final SimpMessagingTemplate messagingTemplate;
    private final ObjectMapper          objectMapper;


    @PostMapping("/player")
    public ResponseEntity<Void> tradePlayer(@RequestBody PlayerTradeDto dto) {
        tradeService.tradeBetweenPlayers(
                dto.getSessionId(),
                dto.getFromUser(),
                dto.getToUser(),
                dto.getOffered(),
                dto.getRequested()
        );

        TradeOfferMessage offerMsg = new TradeOfferMessage(
                dto.getFromUser(),
                dto.getToUser(),
                dto.getOffered(),
                dto.getRequested()
        );

        // 3) Convert to Map<String,Object> so it fits into GameMoveDto.moveData
        Map<String, Object> moveData = objectMapper.convertValue(
                offerMsg, new TypeReference<Map<String,Object>>() {}
        );

        // 4) Wrap in GameMoveDto with gameMoveType = "TRADE_OFFER"
        GameMoveDto gameMove = new GameMoveDto(
                GameMoveTypeEnum.TRADE_OFFER.name(),
                moveData
        );

        // 5) Broadcast to /topic/moves/{sessionId}
        String destination = "/topic/moves/" + dto.getSessionId();
        messagingTemplate.convertAndSend(destination, gameMove);

        return ResponseEntity.ok().build();
    }

    /**
     * When Player B accepts or declines, Unity should POST here with a TradeResponseMessage.
     * If accepted == true, we apply the actual resource‐swap in the database and then
     * broadcast a “TRADE_EXECUTED” to both players so they can update local UIs.
     */
    @PostMapping("/response")
    public ResponseEntity<Void> handleTradeResponse(@RequestBody TradeResponseMessage resp) {
        Long sessionId = resp.getSessionId();
        String fromUser = resp.getFromUser();  // Player B’s username
        String toUser   = resp.getToUser();    // Player A’s username

        String destination = "/topic/moves/" + sessionId;

        if (resp.isAccepted()) {
            // 1) First: broadcast a TRADE_RESPONSE so Player A’s client sees “Trade Accepted!”
            Map<String,Object> respMoveData = objectMapper.convertValue(
                    resp, new TypeReference<Map<String,Object>>() {}
            );
            GameMoveDto acceptanceMove = new GameMoveDto(
                    GameMoveTypeEnum.TRADE_RESPONSE.name(),
                    respMoveData
            );
            messagingTemplate.convertAndSend(destination, acceptanceMove);

            // 2) Now actually swap resources in the database:
            //    Note: tradeService.tradeBetweenPlayers expects (sessionId, fromUser, toUser, offered, requested)
            //    where “offered” is taken from fromUser, “requested” is taken from toUser.
            //    Our resp.getOffered() == originalOffer.offered  (resources A was giving)
            //    Our resp.getRequested() == originalOffer.requested (resources A wanted from B).
            //
            //    Because fromUser=Player B and toUser=Player A in the incoming `resp`, we must invert them:
            tradeService.tradeBetweenPlayers(
                    sessionId,
                    toUser,     // “fromUser” in service = Player A
                    fromUser,   // “toUser”   in service = Player B
                    resp.getOffered(),
                    resp.getRequested()
            );

            // 3) Finally: broadcast TRADE_EXECUTED with a TradeExecutedDto so both clients update inventories
            TradeExecutedDto executedDto = new TradeExecutedDto(
                    toUser,              // A
                    fromUser,            // B
                    resp.getOffered(),   // what A gave
                    resp.getRequested()  // what A got
            );
            Map<String,Object> execMoveData = objectMapper.convertValue(
                    executedDto, new TypeReference<Map<String,Object>>() {}
            );
            GameMoveDto executedMove = new GameMoveDto(
                    GameMoveTypeEnum.TRADE_EXECUTED.name(),
                    execMoveData
            );
            messagingTemplate.convertAndSend(destination, executedMove);
        }
        else {
            // If declined, broadcast a TRADE_RESPONSE (accepted=false) so Player A sees “Trade Declined!”
            Map<String,Object> respMoveData = objectMapper.convertValue(
                    resp, new TypeReference<Map<String,Object>>() {}
            );
            GameMoveDto declineMove = new GameMoveDto(
                    GameMoveTypeEnum.TRADE_RESPONSE.name(),
                    respMoveData
            );
            messagingTemplate.convertAndSend(destination, declineMove);
        }

        return ResponseEntity.ok().build();
    }

    /**
     * Bank trades remain unchanged.
     */
    @PostMapping("/bank")
    public ResponseEntity<Void> tradeBank(@RequestBody com.catan.catanbackend.model.dto.BankTradeDto dto) {
        tradeService.tradeWithBank(
                dto.getSessionId(),
                dto.getFromUser(),
                dto.getOffered(),
                dto.getRequested(),
                dto.getPortType(),
                dto.getPortRatio()
        );
        return ResponseEntity.ok().build();
    }
}
