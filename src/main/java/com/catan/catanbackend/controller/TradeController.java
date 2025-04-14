package com.catan.catanbackend.controller;

import com.catan.catanbackend.model.ResourceGroup;
import com.catan.catanbackend.service.TradeService;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

@RestController
@RequestMapping("/api/trade")
@CrossOrigin
public class TradeController {

    private final TradeService tradeService;

    public TradeController(TradeService tradeService) {
        this.tradeService = tradeService;
    }

    @PostMapping("/playerToPlayer")
    public ResponseEntity<String> tradePlayerToPlayer(@RequestParam String fromUser,
                                                      @RequestParam String toUser,
                                                      @RequestBody TradeRequest request) {
        tradeService.tradeBetweenPlayers(fromUser, toUser, request.getOffered(), request.getRequested());
        return ResponseEntity.ok("Player-to-player trade successful!");
    }

    @PostMapping("/bank")
    public ResponseEntity<String> tradeWithBank(@RequestParam String fromUser,
                                                @RequestParam(required = false) String portType,
                                                @RequestParam(defaultValue = "4") int portRatio,
                                                @RequestBody TradeRequest request) {
        tradeService.tradeWithBank(fromUser, request.getOffered(), request.getRequested(), portType, portRatio);
        return ResponseEntity.ok("Trade with bank successful!");
    }

    public static class TradeRequest {
        private ResourceGroup offered;
        private ResourceGroup requested;

        public ResourceGroup getOffered() {
            return offered;
        }
        public ResourceGroup getRequested() {
            return requested;
        }
    }
}
