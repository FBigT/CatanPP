package com.catan.catanbackend.repository;

import com.catan.catanbackend.model.RobberMoveBlocker;
import org.springframework.data.jpa.repository.JpaRepository;

import java.util.List;

public interface RobberMoveBlockerRepository extends JpaRepository<RobberMoveBlocker, Long> {
    List<RobberMoveBlocker> findBySessionPlayerSessionId(Long sessionId);
}
