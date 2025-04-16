package com.catan.catanbackend.repository;

import com.catan.catanbackend.model.GameSave;
import com.catan.catanbackend.model.User;
import org.springframework.data.jpa.repository.JpaRepository;

import java.util.List;

public interface GameSaveRepository extends JpaRepository<GameSave, Long> {
    List<GameSave> findByUser(User user);
}
