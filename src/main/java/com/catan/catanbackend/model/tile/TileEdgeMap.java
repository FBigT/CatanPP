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

    @ManyToOne(cascade = { CascadeType.PERSIST, CascadeType.MERGE })
    @JoinColumn(name = "tile_id", nullable = false)
    private Tile tile;

    @ManyToOne(cascade =  { CascadeType.PERSIST, CascadeType.MERGE })
    @JoinColumn(name = "edge_id", nullable = false)
    private TileEdge edge;

    private int edgeIndex;
}
