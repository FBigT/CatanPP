package com.catan.catanbackend.repository.tiles;

import com.catan.catanbackend.model.tile.Road;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import java.util.List;

@Repository
public interface RoadRepository extends JpaRepository<Road, Long> {

    // Sve ceste nekog igrača
    List<Road> findByOwnerId(Long ownerId);

    // Sve ceste u sesiji
    List<Road> findByOwnerSessionId(Long sessionId);
}
