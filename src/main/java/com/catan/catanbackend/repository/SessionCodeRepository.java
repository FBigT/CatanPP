package com.catan.catanbackend.repository;

import com.catan.catanbackend.model.SessionCode;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Query;
import org.springframework.data.repository.query.Param;

import java.util.Optional;

public interface SessionCodeRepository extends JpaRepository<SessionCode, Long> {
    Optional<SessionCode> findByCode(@Param("sessionCode") String code);
    Optional<SessionCode> findBySessionId(Long sessionId);

}
