package com.catan.catanbackend.service;

import com.catan.catanbackend.model.PlayerProfile;
import com.catan.catanbackend.model.TradingPort;
import com.catan.catanbackend.repository.PlayerProfileRepository;
import com.catan.catanbackend.repository.TradingPortRepository;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.util.List;

@Service
@Transactional
public class TradingPortService {

    private final TradingPortRepository tradingPortRepository;
    private final PlayerProfileRepository playerProfileRepository;

    public TradingPortService(TradingPortRepository tradingPortRepository, PlayerProfileRepository playerProfileRepository) {
        this.tradingPortRepository = tradingPortRepository;
        this.playerProfileRepository = playerProfileRepository;
    }

    public List<TradingPort> getAllTradingPorts() {
        return tradingPortRepository.findAll();
    }

    public List<TradingPort> getPortsByUsername(String username) {
        PlayerProfile profile = playerProfileRepository.findByUserUsername(username)
                .orElseThrow(() -> new IllegalArgumentException("User not found"));
        return tradingPortRepository.findAllByOwner(profile);
    }

    public TradingPort createTradingPort(String type, int tradeRatio, String username) {
        PlayerProfile profile = playerProfileRepository.findByUserUsername(username)
                .orElseThrow(() -> new IllegalArgumentException("User not found"));

        TradingPort port = new TradingPort(type, tradeRatio, profile);
        return tradingPortRepository.save(port);
    }

    public TradingPort updatePortPlacement(Long id, boolean isPlaced) {
        TradingPort port = tradingPortRepository.findById(id).orElseThrow(
                () -> new RuntimeException("Trading Port not found"));
        port.setPlaced(isPlaced);
        return tradingPortRepository.save(port);
    }
}
