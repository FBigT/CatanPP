package com.catan.catanbackend.model;

import jakarta.persistence.*;
import lombok.Getter;
import lombok.NoArgsConstructor;
import lombok.Setter;

@NoArgsConstructor
@Entity
@Table(name = "trading_ports")
@Getter
@Setter
public class TradingPort {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;

    @Column(nullable = false)
    private String type;

    @Column(nullable = false)
    private int tradeRatio;

    @Column(nullable = false)
    private boolean isPlaced = false;

    @ManyToOne(fetch = FetchType.LAZY)
    @JoinColumn(name = "player_id")
    private PlayerProfile owner;

    public TradingPort(String type, int tradeRatio, PlayerProfile owner) {
        this.type = type;
        this.tradeRatio = tradeRatio;
        this.owner = owner;
        this.isPlaced = false;
    }
}
