package com.catan.catanbackend.model.tile;

import com.catan.catanbackend.model.Session;
import jakarta.persistence.*;
import lombok.Getter;
import lombok.NoArgsConstructor;
import lombok.Setter;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;
import java.util.Optional;

@Entity
@Table(name = "tiles")
@Getter
@Setter
@NoArgsConstructor
public class Tile {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;

    private int x;
    private int y;
    private int z;

    private int number;
    private boolean hasRobber;

    @ManyToOne(fetch = FetchType.LAZY)
    @JoinColumn(name = "resource_id", nullable = false)
    private Resource resource;

    @OneToMany(mappedBy = "tile", cascade = CascadeType.ALL, orphanRemoval = true)
    private List<TileCornerMap> tileCornerMaps = new ArrayList<>();

    @OneToMany(mappedBy = "tile", cascade = CascadeType.ALL, orphanRemoval = true)
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
