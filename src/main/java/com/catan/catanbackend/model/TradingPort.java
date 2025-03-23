package com.catan.catanbackend.model;


import jakarta.persistence.*;
import lombok.Getter;

@Entity
@Table(name = "trading_ports")
public class TradingPort {

    @Getter
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;

    @Getter
    @Column(nullable = false)
    private String type; // "Generic", "Brick", "Wood", etc.

    @Getter
    @Column(nullable = false)
    private int tradeRatio; // 3:1, 2:1, etc.

    @Column(nullable = false)
    private boolean isPlaced; // False until map generation is done

    public TradingPort() {}

    public TradingPort(String type, int tradeRatio) {
        this.type = type;
        this.tradeRatio = tradeRatio;
        this.isPlaced = false;
    }

    public boolean isPlaced() { return isPlaced; }

    public void setPlaced(boolean placed) { this.isPlaced = placed; }
}
