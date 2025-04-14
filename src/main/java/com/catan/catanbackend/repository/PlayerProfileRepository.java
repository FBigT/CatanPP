package com.catan.catanbackend.repository;

import com.catan.catanbackend.model.PlayerProfile;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import java.util.Optional;

@Repository
public interface PlayerProfileRepository extends JpaRepository<PlayerProfile, Long> {
    Optional<PlayerProfile> findByUserUsername(String username);
    Optional<PlayerProfile> findByUserId(Long id);
}
