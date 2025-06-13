package com.catan.catanbackend.model.tile;

import com.catan.catanbackend.model.Session;
import com.fasterxml.jackson.annotation.JsonIgnore;
import com.fasterxml.jackson.annotation.JsonIgnoreProperties;
import com.fasterxml.jackson.annotation.JsonProperty;
import jakarta.persistence.*;
import lombok.*;

import java.util.ArrayList;
import java.util.List;

@Entity
@Data
@NoArgsConstructor
@AllArgsConstructor
@Table(name = "tile_edges")
@JsonIgnoreProperties({ "hibernateLazyInitializer", "handler" })
public class TileEdge {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;

    @JsonIgnore
    @ManyToOne
    @JoinColumn(name = "corner_a_id", nullable = false)
    private TileCorner cornerA;

    @JsonIgnore
    @ManyToOne
    @JoinColumn(name = "corner_b_id", nullable = false)
    private TileCorner cornerB;

    @ToString.Exclude @EqualsAndHashCode.Exclude
    @JsonIgnore
    @ManyToOne(fetch = FetchType.LAZY)
    @JoinColumn(name = "session_id", nullable = false)
    private Session session;

    @JsonIgnore
    @OneToOne(fetch = FetchType.EAGER)
    @JoinColumn(name = "road_id")
    private Road road;

    @JsonProperty("cornerAId")
    public Long extractCornerAId() {
        return cornerA.getId();
    }

    @JsonProperty("cornerBId")
    public Long extractCornerBId() {
        return cornerB.getId();
    }

    @JsonProperty("roadId")
    public Long extractRoadId() {
        if (road == null) {
            return null;
        }
        return road.getId();
    }

    @JsonIgnore
    @OneToMany(mappedBy = "edge", cascade = CascadeType.ALL, orphanRemoval = true)
    private List<TileEdgeMap> tileEdgeMaps = new ArrayList<>();
}
