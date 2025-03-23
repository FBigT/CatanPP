package com.catan.catanbackend.controller;


import com.catan.catanbackend.model.TradingPort;
import com.catan.catanbackend.service.TradingPortService;
import org.springframework.web.bind.annotation.*;

import java.util.List;

@RestController
@RequestMapping("/api/trading-ports")
public class TradingPortController {

    private final TradingPortService tradingPortService;

    public TradingPortController(TradingPortService tradingPortService) {
        this.tradingPortService = tradingPortService;
    }

    @GetMapping
    public List<TradingPort> getAllTradingPorts() {
        return tradingPortService.getAllTradingPorts();
    }

    @PostMapping
    public TradingPort createTradingPort(@RequestParam String type, @RequestParam int tradeRatio) {
        return tradingPortService.createTradingPort(type, tradeRatio);
    }

    @PutMapping("/{id}/place")
    public TradingPort updatePortPlacement(@PathVariable Long id, @RequestParam boolean isPlaced) {
        return tradingPortService.updatePortPlacement(id, isPlaced);
    }
}
