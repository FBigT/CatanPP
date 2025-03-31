package com.catan.catanbackend.service;



import com.catan.catanbackend.model.TradingPort;
import com.catan.catanbackend.repository.TradingPortRepository;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.util.List;

@Service
@Transactional
public class TradingPortService {

    private final TradingPortRepository tradingPortRepository;

    public TradingPortService(TradingPortRepository tradingPortRepository) {
        this.tradingPortRepository = tradingPortRepository;
    }

    public List<TradingPort> getAllTradingPorts() {
        return tradingPortRepository.findAll();
    }

    public TradingPort createTradingPort(String type, int tradeRatio) {
        TradingPort port = new TradingPort(type, tradeRatio);
        return tradingPortRepository.save(port);
    }

    public TradingPort updatePortPlacement(Long id, boolean isPlaced) {
        TradingPort port = tradingPortRepository.findById(id).orElseThrow(
                () -> new RuntimeException("Trading Port not found"));
        port.setPlaced(isPlaced);
        return tradingPortRepository.save(port);
    }
}
