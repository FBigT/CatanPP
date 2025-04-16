package com.catan.catanbackend.controller;

import com.catan.catanbackend.model.TradingPort;
import com.catan.catanbackend.service.TradingPortService;
import org.springframework.web.bind.annotation.*;

import java.util.List;

@CrossOrigin
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

    @GetMapping("/{username}")
    public List<TradingPort> getPortsByUsername(@PathVariable String username) {
        return tradingPortService.getPortsByUsername(username);
    }

    @PostMapping
    public TradingPort createTradingPort(@RequestParam String type,
                                         @RequestParam int tradeRatio,
                                         @RequestParam String username) {
        return tradingPortService.createTradingPort(type, tradeRatio, username);
    }

    @PutMapping("/{id}/place")
    public TradingPort updatePortPlacement(@PathVariable Long id, @RequestParam boolean isPlaced) {
        return tradingPortService.updatePortPlacement(id, isPlaced);
    }
}
