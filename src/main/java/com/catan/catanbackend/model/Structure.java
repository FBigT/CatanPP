package com.catan.catanbackend.model;

import jakarta.persistence.*;
import lombok.Getter;
import lombok.NoArgsConstructor;
import lombok.Setter;

@Entity
@Table(name = "structures")
@NoArgsConstructor
public class Structure {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;

    @Getter
    @Column(nullable = false)
    private String owner;

    @Getter
    @ManyToOne
    @JoinColumn(name = "tile_id")
    private Tile tile;

    @Getter
    @Column(nullable = false)
    private int cornerIndex;

    @Getter
    @Setter
    private String type = "SETTLEMENT";

    public Structure(String owner, Tile tile, int cornerIndex) {
        this.owner = owner;
        this.tile = tile;
        this.cornerIndex = cornerIndex;
    }
}
