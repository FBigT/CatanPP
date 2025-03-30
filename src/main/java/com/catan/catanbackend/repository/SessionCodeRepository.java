package com.catan.catanbackend.repository;

import com.catan.catanbackend.model.SessionCode;
import org.springframework.data.jpa.repository.JpaRepository;

import java.util.Optional;

public interface SessionCodeRepository extends JpaRepository<SessionCode, Long> {
    Optional<SessionCode> findByCode(String code);
}
