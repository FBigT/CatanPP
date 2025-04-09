package com.catan.catanbackend.repository;

import com.catan.catanbackend.model.Session;
import org.springframework.data.jpa.repository.JpaRepository;

import java.util.List;

public interface SessionRepository extends JpaRepository<Session, Long> {
    public List<Session> findByHostId(Long hostId);
}
