/*package com.catan.catanbackend.model;



import jakarta.persistence.*;
import lombok.Getter;

@Entity
@Table(name = "cities")
public class City {

    @Getter
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;

    @Getter
    @Column(nullable = false)
    private String owner; // Player ID or Name

    @Column(nullable = false)
    private boolean isUpgraded; // False = Settlement, True = City

    @Getter
    @Column(nullable = false)
    private int x; // Hex tile X coordinate
    @Getter
    @Column(nullable = false)
    private int y; // Hex tile Y coordinate

    public City() {}

    public City(String owner, int x, int y) {
        this.owner = owner;
        this.x = x;
        this.y = y;
        this.isUpgraded = false; // Starts as a settlement
    }

    public boolean isUpgraded() { return isUpgraded; }

    public void upgradeToCity() { this.isUpgraded = true; }
}
*/