package com.catan.catanbackend.repository.tiles;

import com.catan.catanbackend.model.tile.TileCorner;
import org.springframework.data.repository.CrudRepository;

import java.util.List;

public interface TileCornerRepository extends CrudRepository<TileCorner, Integer> {
    List<TileCorner> findBySessionId(Long sessionId);
}
