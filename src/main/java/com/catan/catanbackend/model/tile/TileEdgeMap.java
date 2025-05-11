package com.catan.catanbackend.model.tile;

import jakarta.persistence.*;
import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Data;
import lombok.NoArgsConstructor;

@Entity
@Data
@NoArgsConstructor
@AllArgsConstructor
@Builder
@Table(name = "tile_edge_map",
        uniqueConstraints = {
                @UniqueConstraint(columnNames = {"tile_id", "edge_index"})
        })
public class TileEdgeMap {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;

    @ManyToOne
    @JoinColumn(name = "tile_id", nullable = false)
    private Tile tile;

    @ManyToOne
    @JoinColumn(name = "edge_id", nullable = false)
    private TileEdge edge;

    private int edgeIndex;
}
