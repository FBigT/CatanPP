package com.catan.catanbackend.repository;

import com.catan.catanbackend.model.Structure;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import java.util.List;

@Repository
public interface StructureRepository extends JpaRepository<Structure, Long> {
    // Sve strukture nekog igrača
    List<Structure> findByOwner(String owner);

    // Sve strukture na jednom tileu
    List<Structure> findByTileId(Long tileId);

    // Pronađi strukturu na određenom tileu i corneru
    Structure findByTileIdAndCornerIndex(Long tileId, int cornerIndex);
}
