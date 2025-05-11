package com.catan.catanbackend.repository.tiles;

import com.catan.catanbackend.model.tile.Structure;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import java.util.List;
import java.util.Objects;

@Repository
public interface StructureRepository extends JpaRepository<Structure, Long> {
    // Sve strukture nekog igrača
    List<Structure> findByOwnerId(Long ownerId);

    // Pronađi strukturu na određenom tileu i corneru
    default Structure findByTileIdAndCornerIndex(Long tileId, int cornerIndex){
        List<Structure> all = findAll();
        return all.stream().filter(x ->
                x.getCorner().getTileCornerMaps().stream().anyMatch(y ->
                        y.getCornerIndex() == cornerIndex && Objects.equals(y.getTile().getId(), tileId))).findFirst().orElse(null);
    }

    // Sve strukture u sesiji
    List<Structure> findByOwnerSessionId(Long sessionId);
}
