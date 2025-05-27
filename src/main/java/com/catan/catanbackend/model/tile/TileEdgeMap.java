package com.catan.catanbackend.model.tile;

import com.fasterxml.jackson.annotation.JsonIgnore;
import com.fasterxml.jackson.annotation.JsonIgnoreProperties;
import com.fasterxml.jackson.annotation.JsonProperty;
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
@JsonIgnoreProperties({ "hibernateLazyInitializer", "handler" })
@Table(name = "tile_edge_map",
        uniqueConstraints = {
                @UniqueConstraint(columnNames = {"tile_id", "edge_index"})
        })
public class TileEdgeMap {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;

    @JsonIgnore
    @ManyToOne(cascade = { CascadeType.PERSIST, CascadeType.MERGE })
    @JoinColumn(name = "tile_id", nullable = false)
    private Tile tile;

    @JsonIgnore
    @ManyToOne(cascade =  { CascadeType.PERSIST, CascadeType.MERGE })
    @JoinColumn(name = "edge_id", nullable = false)
    private TileEdge edge;

    @JsonProperty("tileId")
    public Long extractTileId() {
        return tile.getId();
    }

    @JsonProperty("edgeId")
    public Long extractEdgeId(){
         return edge.getId();
    }

    private int edgeIndex;
}
