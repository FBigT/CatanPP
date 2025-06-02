package com.catan.catanbackend.controller;

import com.catan.catanbackend.model.dto.BankTradeDto;
import com.catan.catanbackend.model.dto.PlayerTradeDto;
import com.catan.catanbackend.service.TradeService;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

@RestController
@RequestMapping("/api/trade")
@CrossOrigin(origins = "*")
public class TradeController {

    private final TradeService tradeService;

    public TradeController(TradeService tradeService) {
        this.tradeService = tradeService;
    }

    @PostMapping("/player")
    public ResponseEntity<Void> tradePlayer(@RequestBody PlayerTradeDto dto) {
        return ResponseEntity.ok().build();
    }


    @PostMapping("/bank")
    public ResponseEntity<Void> tradeBank(@RequestBody BankTradeDto dto) {
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
