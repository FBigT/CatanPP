package com.catan.catanbackend.repository;

import com.catan.catanbackend.model.SessionRecord;
import org.springframework.data.jpa.repository.JpaRepository;

public interface SessionRecordRepository extends JpaRepository<SessionRecord, Long> {
}
