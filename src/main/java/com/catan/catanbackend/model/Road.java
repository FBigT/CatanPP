package com.catan.catanbackend.model;

import jakarta.persistence.*;
import lombok.Getter;
import lombok.NoArgsConstructor;
import lombok.Setter;

@Entity
@Table(name = "roads")
@Getter
@Setter
@NoArgsConstructor
public class Road {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;

    private String owner;

    private int edgeIndex;

    @ManyToOne
    @JoinColumn(name = "tile_id")
    private Tile tile;
}
