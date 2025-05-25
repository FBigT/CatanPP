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
@Table(name = "tile_corner_map",
        uniqueConstraints = {
            @UniqueConstraint(columnNames = {"tile_id", "corner_index"})
        })
public class TileCornerMap {
    @Id
    @GeneratedValue
    private Long id;

    @ManyToOne(cascade = { CascadeType.PERSIST, CascadeType.MERGE })
    @JoinColumn(name = "tile_id", nullable = false)
    private Tile tile;

    @ManyToOne(cascade = { CascadeType.PERSIST, CascadeType.MERGE })
    @JoinColumn(name = "corner_id", nullable = false)
    private TileCorner corner;

    private int cornerIndex;
}
