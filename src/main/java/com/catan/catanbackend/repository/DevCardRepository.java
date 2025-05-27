package com.catan.catanbackend.repository;

import com.catan.catanbackend.model.DevCard;
import com.catan.catanbackend.model.SessionPlayer;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import java.util.List;

@Repository
public interface DevCardRepository extends JpaRepository<DevCard, Long> {
    List<DevCard> findByOwner(SessionPlayer owner);
    List<DevCard> findByOwnerIsNull();            // cards still in deck
    List<DevCard> findByOwnerIsNullOrderById();   // simple top-of-deck ordering
    long countByOwnerIsNull();
    long countByOwner(SessionPlayer owner);
    List<DevCard> findBySessionId(Long sessionId);
}
