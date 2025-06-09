package com.catan.catanbackend.repository.tiles;

import com.catan.catanbackend.model.tile.TileCorner;
import com.catan.catanbackend.model.tile.TileEdge;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Query;
import org.springframework.data.repository.query.Param;

import java.util.ArrayList;
import java.util.List;

public interface TileEdgeRepository extends JpaRepository<TileEdge, Integer> {
    @Query("SELECT e FROM TileEdge e WHERE e.cornerA = :corner OR e.cornerB = :corner")
    List<TileEdge> findAllConnectedToCorner(@Param("corner") TileCorner corner);

    List<TileEdge> findBySessionId(Long sessionId);
}
