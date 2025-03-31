package com.catan.catanbackend.repository;

import com.catan.catanbackend.model.RobberBlocker;
import org.springframework.data.jpa.repository.JpaRepository;

import java.util.Optional;

public interface RobberBlockerRepository extends JpaRepository<RobberBlocker, Long> {
    Optional<RobberBlocker> findBySessionPlayerId(Long sessionPlayerId);
}
