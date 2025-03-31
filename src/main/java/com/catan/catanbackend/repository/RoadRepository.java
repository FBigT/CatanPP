package com.catan.catanbackend.repository;

import com.catan.catanbackend.model.Road;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import java.util.List;

@Repository
public interface RoadRepository extends JpaRepository<Road, Long> {

    // Sve ceste nekog igrača
    List<Road> findByOwner(String owner);

    // Ceste na određenom tileu
    List<Road> findByTileId(Long tileId);

    // Cesta na specifičnom edgeu određenog tilea
    Road findByTileIdAndEdgeIndex(Long tileId, int edgeIndex);
}
