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

    /**
     * Player A sends a trade offer to Player B.  We only broadcast the
     * TradeOfferMessage here—do NOT execute the swap yet.
     */
    @PostMapping("/player")
    public ResponseEntity<Void> tradePlayer(@RequestBody PlayerTradeDto dto) {
        // ─── Remove this line (it was prematurely swapping resources) ───
        // tradeService.tradeBetweenPlayers(
        //         dto.getSessionId(),
        //         dto.getFromUser(),
        //         dto.getToUser(),
        //         dto.getOffered(),
        //         dto.getRequested()
        // );

        // Build the TradeOfferMessage to broadcast over STOMP:
        TradeOfferMessage offerMsg = new TradeOfferMessage(
                dto.getFromUser(),
                dto.getToUser(),
                dto.getOffered(),
                dto.getRequested()
        );

        // Convert to Map<String,Object> for GameMoveDto.moveData
        Map<String, Object> moveData = objectMapper.convertValue(
                offerMsg, new TypeReference<Map<String,Object>>() {}
        );

        // Wrap in GameMoveDto with type = "TRADE_OFFER"
        GameMoveDto gameMove = new GameMoveDto(
                GameMoveTypeEnum.TRADE_OFFER.name(),
                moveData
        );

        // Broadcast to /topic/moves/{sessionId}
        String destination = "/topic/moves/" + dto.getSessionId();
        messagingTemplate.convertAndSend(destination, gameMove);

        return ResponseEntity.ok().build();
    }

    /**
     * When Player B accepts or declines, Unity POSTs a TradeResponseMessage here.
     * If accepted == true, we now apply the database swap exactly once, and then
     * broadcast both TRADE_RESPONSE & TRADE_EXECUTED.
     */
    @PostMapping("/response")
    public ResponseEntity<Void> handleTradeResponse(@RequestBody TradeResponseMessage resp) {
        Long sessionId = resp.getSessionId();
        String fromUser = resp.getFromUser();  // This is Player B
        String toUser   = resp.getToUser();    // This is Player A

        String destination = "/topic/moves/" + sessionId;

        if (resp.isAccepted()) {
            // 1) Broadcast TRADE_RESPONSE so Player A sees “Trade Accepted!”
            Map<String,Object> respMoveData = objectMapper.convertValue(
                    resp, new TypeReference<Map<String,Object>>() {}
            );
            GameMoveDto acceptanceMove = new GameMoveDto(
                    GameMoveTypeEnum.TRADE_RESPONSE.name(),
                    respMoveData
            );
            messagingTemplate.convertAndSend(destination, acceptanceMove);

            // 2) Now actually perform the swap in the database (only once!)
            //    NOTE: resp.getOffered() == originalOffer.offered  (what A was giving)
            //    resp.getRequested() == originalOffer.requested (what A wanted from B)
            //    Because “fromUser” is B and “toUser” is A in the response, we invert:
            tradeService.tradeBetweenPlayers(
                    sessionId,
                    toUser,     // “fromUser” in service = Player A
                    fromUser,   // “toUser”   in service = Player B
                    resp.getOffered(),
                    resp.getRequested()
            );

            // 3) Broadcast TRADE_EXECUTED so both clients update inventories
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
            // If declined, just broadcast TRADE_RESPONSE (accepted=false)
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
