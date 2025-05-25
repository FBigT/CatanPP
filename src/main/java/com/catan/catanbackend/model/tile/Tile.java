package com.catan.catanbackend.model.tile;

import com.catan.catanbackend.model.Session;
import jakarta.persistence.*;
import lombok.*;

import java.util.ArrayList;
 import java.util.List;
import java.util.Optional;

@Entity
@Table(name = "tiles")
@Getter
@Setter
@Builder
@AllArgsConstructor
@NoArgsConstructor
public class Tile {
    public Tile(int x, int y, int z, Session session, int number, TileType tileType) {
        this.x = x;
        this.y = y;
        this.z = z;
        this.session = session;
        this.number = number;
        this.tileType = tileType;
        hasRobber = false;
    }

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;

    private int x;
    private int y;
    private int z;

    private int number;
    private boolean hasRobber;

    @ManyToOne
    @JoinColumn(name = "tile_type_id")
    private TileType tileType;

    @OneToMany(mappedBy = "tile", cascade = CascadeType.ALL, orphanRemoval = true, fetch = FetchType.EAGER)
    @Builder.Default
    private List<TileCornerMap> tileCornerMaps = new ArrayList<>();

    @OneToMany(mappedBy = "tile", cascade = CascadeType.ALL, orphanRemoval = true, fetch = FetchType.EAGER)
    @Builder.Default
    private List<TileEdgeMap> tileEdgeMaps = new ArrayList<>();

    @ManyToOne(fetch = FetchType.LAZY)
    @JoinColumn(name = "session_id", nullable = false)
    private Session session;

    public Optional<TileCorner> getTileCorner(int cornerIndex) {
        Optional<TileCornerMap> map = tileCornerMaps.stream().filter(tileCornerMap ->
                tileCornerMap.getCornerIndex() == cornerIndex).findAny();
        return map.map(TileCornerMap::getCorner);
    }

    public Optional<TileEdge> getTileEdge(int edgeIndex) {
        Optional<TileEdgeMap> map = tileEdgeMaps.stream().filter(tileEdgeMap ->
                tileEdgeMap.getEdgeIndex() == edgeIndex).findAny();
        return map.map(TileEdgeMap::getEdge);
    }
}
