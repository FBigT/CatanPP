package com.catan.catanbackend.repository;

import com.catan.catanbackend.model.Tile;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import java.util.List;

@Repository
public interface TileRepository extends JpaRepository<Tile, Long> {
    // Nađi tile prema poziciji na mapi (korisno ako koristiš (x, y) koordinatni sustav)
    Tile findByXAndY(int x, int y);

    // Ako budeš radio učitavanje svih tile-ova u određenom području (npr. svijet generacije)
    List<Tile> findByXBetweenAndYBetween(int xMin, int xMax, int yMin, int yMax);
}
