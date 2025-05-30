package com.catan.catanbackend.model;

import com.fasterxml.jackson.annotation.JsonIgnore;
import jakarta.persistence.*;
import jakarta.validation.constraints.NotNull;
import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;
import org.hibernate.annotations.ColumnDefault;

import java.util.Objects;

@Data
@AllArgsConstructor
@NoArgsConstructor
@Entity
public class TradeOffer {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;

    // Source
    @JsonIgnore
    @NotNull
    @OneToOne(fetch = FetchType.LAZY, optional = false)
    @JoinColumn(name = "from_player_id", nullable = false)
    private SessionPlayer fromPlayer;

    @NotNull
    @Column(name = "offer_brick", nullable = false)
    @ColumnDefault("0")
    private Integer offerBrick = 0;

    @NotNull
    @Column(name = "offer_crystal", nullable = false)
    @ColumnDefault("0")
    private Integer offerCrystal = 0;

    @NotNull
    @Column(name = "offer_ore", nullable = false)
    @ColumnDefault("0")
    private Integer offerOre = 0;

    @NotNull
    @Column(name = "offer_rice", nullable = false)
    @ColumnDefault("0")
    private Integer offerRice = 0;

    @NotNull
    @Column(name = "offer_sheep", nullable = false)
    @ColumnDefault("0")
    private Integer offerSheep = 0;

    @NotNull
    @Column(name = "offer_silver", nullable = false)
    @ColumnDefault("0")
    private Integer offerSilver = 0;

    @NotNull
    @Column(name = "offer_gold", nullable = false)
    @ColumnDefault("0")
    private Integer offerGold = 0;

    @NotNull
    @Column(name = "offer_wood", nullable = false)
    @ColumnDefault("0")
    private Integer offerWood = 0;

    // Target
    @JsonIgnore
    @NotNull
    @OneToOne(fetch = FetchType.LAZY, optional = false)
    @JoinColumn(name = "to_player_id", nullable = false)
    private SessionPlayer toPlayer;

    @NotNull
    @Column(name = "request_brick", nullable = false)
    @ColumnDefault("0")
    private Integer requestBrick = 0;

    @NotNull
    @Column(name = "request_crystal", nullable = false)
    @ColumnDefault("0")
    private Integer requestCrystal = 0;

    @NotNull
    @Column(name = "request_ore", nullable = false)
    @ColumnDefault("0")
    private Integer requestOre = 0;

    @NotNull
    @Column(name = "request_rice", nullable = false)
    @ColumnDefault("0")
    private Integer requestRice = 0;

    @NotNull
    @Column(name = "request_sheep", nullable = false)
    @ColumnDefault("0")
    private Integer requestSheep = 0;

    @NotNull
    @Column(name = "request_silver", nullable = false)
    @ColumnDefault("0")
    private Integer requestSilver = 0;

    @NotNull
    @Column(name = "request_gold", nullable = false)
    @ColumnDefault("0")
    private Integer requestGold = 0;

    @NotNull
    @Column(name = "request_wood", nullable = false)
    @ColumnDefault("0")
    private Integer requestWood = 0;

    public void setOfferResources(ResourceGroup resourceGroup) {
        offerBrick = resourceGroup.getBrick();
        offerCrystal = resourceGroup.getCrystal();
        offerOre = resourceGroup.getOre();
        offerRice = resourceGroup.getRice();
        offerSheep = resourceGroup.getSheep();
        offerSilver = resourceGroup.getSilver();
        offerGold = resourceGroup.getGold();
        offerWood = resourceGroup.getWood();
    }

    public void setRequestResources(ResourceGroup resourceGroup) {
        requestBrick = resourceGroup.getBrick();
        requestCrystal = resourceGroup.getCrystal();
        requestOre = resourceGroup.getOre();
        requestRice = resourceGroup.getRice();
        requestSheep = resourceGroup.getSheep();
        requestSilver = resourceGroup.getSilver();
        requestGold = resourceGroup.getGold();
        requestWood = resourceGroup.getWood();
    }

    public ResourceGroup getOfferResources() {
        return ResourceGroup.builder()
                .brick(offerBrick)
                .crystal(offerCrystal)
                .ore(offerOre)
                .rice(offerRice)
                .sheep(offerSheep)
                .silver(offerSilver)
                .gold(offerGold)
                .wood(offerWood)
                .build();
    }

    public ResourceGroup getRequestResources() {
        return ResourceGroup.builder()
                .brick(requestBrick)
                .crystal(requestCrystal)
                .ore(requestOre)
                .rice(requestRice)
                .sheep(requestSheep)
                .silver(requestSilver)
                .gold(requestGold)
                .wood(requestWood)
                .build();
    }
}
