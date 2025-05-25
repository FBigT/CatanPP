package com.catan.catanbackend.repository.tiles;

import com.catan.catanbackend.model.tile.Tile;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import java.util.List;
import java.util.Optional;

@Repository
public interface TileRepository extends JpaRepository<Tile, Long> {
    // Nađi tile prema poziciji na mapi (korisno ako koristiš (x, y) koordinatni sustav)
    Tile findByXAndY(int x, int y);

    // Ako budeš radio učitavanje svih tile-ova u određenom području (npr. svijet generacije)
    List<Tile> findByXBetweenAndYBetween(int xMin, int xMax, int yMin, int yMax);

    Optional<Tile> findTileByXAndYAndSessionId(Integer x, Integer y, Long sessionId);

    List<Tile> findBySessionId(Long sessionId);
}
