package com.catan.catanbackend.repository.tiles;

import com.catan.catanbackend.model.tile.Structure;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Query;
import org.springframework.data.repository.query.Param;
import org.springframework.stereotype.Repository;

import java.util.List;
import java.util.Objects;

@Repository
public interface StructureRepository extends JpaRepository<Structure, Long> {
    // Sve strukture nekog igrača
    List<Structure> findByOwnerId(Long ownerId);

    @Query("""
    SELECT s FROM Structure s
    JOIN s.corner c
    JOIN c.tileCornerMaps map
    JOIN map.tile t
    WHERE t.id = :tileId AND map.cornerIndex = :cornerIndex
""")
    Structure findByTileIdAndCornerIndex(@Param("tileId") Long tileId, @Param("cornerIndex") int cornerIndex);

    // Sve strukture u sesiji
    List<Structure> findByOwnerSessionId(Long sessionId);
}
