package com.catan.catanbackend.repository;

import com.catan.catanbackend.model.Session;
import org.springframework.data.jpa.repository.JpaRepository;

public interface SessionRepository extends JpaRepository<Session, Long> {
}
