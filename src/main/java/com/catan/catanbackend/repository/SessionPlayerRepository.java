package com.catan.catanbackend.repository;

import com.catan.catanbackend.model.SessionPlayer;
import org.springframework.data.jpa.repository.JpaRepository;

import java.util.List;

public interface SessionPlayerRepository extends JpaRepository<SessionPlayer, Long> {
    List<SessionPlayer> findSessionPlayerBySessionId(Long sessionId);
    List<SessionPlayer> findSessionPlayerByUserId(Long sessionId);
}
