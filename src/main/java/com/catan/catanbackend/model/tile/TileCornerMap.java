package com.catan.catanbackend.model.tile;

import com.fasterxml.jackson.annotation.JsonIgnore;
import com.fasterxml.jackson.annotation.JsonIgnoreProperties;
import com.fasterxml.jackson.annotation.JsonProperty;
import jakarta.persistence.*;
import lombok.*;

@Entity
@Data
@NoArgsConstructor
@AllArgsConstructor
@Builder
@JsonIgnoreProperties({ "hibernateLazyInitializer", "handler" })
@Table(name = "tile_corner_map"
        //uniqueConstraints = {
            //@UniqueConstraint(columnNames = {"tile_id", "corner_index"})
        //}
)
public class TileCornerMap {
    @Id
    @GeneratedValue
    private Long id;

    @ToString.Exclude @EqualsAndHashCode.Exclude
    @JsonIgnore
    @ManyToOne(cascade = { CascadeType.PERSIST, CascadeType.MERGE })
    @JoinColumn(name = "tile_id", nullable = false)
    private Tile tile;

    @ToString.Exclude @EqualsAndHashCode.Exclude
    @JsonIgnore
    @ManyToOne(cascade = { CascadeType.PERSIST, CascadeType.MERGE })
    @JoinColumn(name = "corner_id", nullable = false)
    private TileCorner corner;

    @JsonProperty("tileId")
    public Long extractTileId() {
        return tile.getId();
    }

    @JsonProperty("cornerId")
    public Long extractCornerId() {
        return corner.getId();
    }

    private int cornerIndex;
}
