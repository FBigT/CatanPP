package com.catan.catanbackend.model;

import jakarta.persistence.*;
import lombok.Getter;
import lombok.NoArgsConstructor;
import lombok.Setter;

@Entity
@Table(name = "structures")
@Getter
@Setter
@NoArgsConstructor
public class Structure {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;

    private String owner;
    private String type; // "settlement" or "city"
    private int cornerIndex;

    @ManyToOne
    @JoinColumn(name = "tile_id")
    private Tile tile;
}
