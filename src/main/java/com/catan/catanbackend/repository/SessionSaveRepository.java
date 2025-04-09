package com.catan.catanbackend.repository;

import com.catan.catanbackend.model.SessionSave;
import org.springframework.data.jpa.repository.JpaRepository;

import java.util.List;

public interface SessionSaveRepository extends JpaRepository<SessionSave, Long> {
    List<SessionSave> findBySessionHostId(Long hostId);
}
