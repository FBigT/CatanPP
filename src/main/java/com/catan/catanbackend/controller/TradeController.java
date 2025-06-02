// src/main/java/com/catan/catanbackend/controller/TradeController.java
package com.catan.catanbackend.controller;

import com.catan.catanbackend.model.dto.PlayerTradeDto;
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
    private final ObjectMapper          objectMapper; // auto‐configured by Spring Boot

    @PostMapping("/player")
    public ResponseEntity<Void> tradePlayer(@RequestBody PlayerTradeDto dto) {
        // 1) Apply the trade (will throw if invalid)
        tradeService.tradeBetweenPlayers(
                dto.getSessionId(),
                dto.getFromUser(),
                dto.getToUser(),
                dto.getOffered(),
                dto.getRequested()
        );

        // 2) Build a simple DTO that your Unity client expects
        TradeOfferMessage offerMsg = new TradeOfferMessage(
                dto.getFromUser(),
                dto.getToUser(),
                dto.getOffered(),
                dto.getRequested()
        );

        // 3) Convert that DTO into a Map<String,Object> so it fits into GameMoveDto.moveData
        Map<String, Object> moveData = objectMapper.convertValue(
                offerMsg, new TypeReference<Map<String,Object>>() {}
        );

        // 4) Wrap in GameMoveDto with gameMoveType = "TRADE_OFFER"
        GameMoveDto gameMove = new GameMoveDto(
                GameMoveTypeEnum.TRADE_OFFER.name(),
                moveData
        );

        // 5) Broadcast via STOMP to /topic/moves/{sessionId}
        String destination = "/topic/moves/" + dto.getSessionId();
        messagingTemplate.convertAndSend(destination, gameMove);

        return ResponseEntity.ok().build();
    }

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
