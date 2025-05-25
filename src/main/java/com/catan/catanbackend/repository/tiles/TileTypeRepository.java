package com.catan.catanbackend.repository.tiles;

import com.catan.catanbackend.model.tile.TileType;
import org.springframework.data.jpa.repository.JpaRepository;

import java.util.Optional;

public interface TileTypeRepository extends JpaRepository<TileType, Long> {
    Optional<TileType> findByName(String name);
    Optional<TileType> findByResourceId(Long id);
}
