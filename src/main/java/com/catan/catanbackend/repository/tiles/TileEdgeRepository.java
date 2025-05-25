package com.catan.catanbackend.repository.tiles;

import com.catan.catanbackend.model.tile.TileCorner;
import com.catan.catanbackend.model.tile.TileEdge;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.repository.query.Param;

import java.util.ArrayList;
import java.util.List;

public interface TileEdgeRepository extends JpaRepository<TileEdge, Integer> {
    default List<TileEdge> findByCorner(@Param("corner") TileCorner corner){
        List<TileEdge> tileEdges = new ArrayList<>();
        for (TileEdge tileEdge : findAll()) {
            if (tileEdge.getCornerB().equals(corner) || tileEdge.getCornerA().equals(corner)) {
                tileEdges.add(tileEdge);
            }
        }
        return tileEdges;
    }
}
