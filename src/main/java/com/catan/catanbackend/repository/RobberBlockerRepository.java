package com.catan.catanbackend.repository;

import com.catan.catanbackend.model.RobberDebtBlocker;
import org.springframework.data.jpa.repository.JpaRepository;

import java.util.Optional;

public interface RobberBlockerRepository extends JpaRepository<RobberDebtBlocker, Long> {
    Optional<RobberDebtBlocker> findBySessionPlayerId(Long sessionPlayerId);
}
