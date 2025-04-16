package com.catan.catanbackend.service;

import com.catan.catanbackend.model.GameSave;
import com.catan.catanbackend.model.User;
import com.catan.catanbackend.model.dto.GameSaveDto;
import com.catan.catanbackend.repository.GameSaveRepository;
import com.catan.catanbackend.repository.UserRepository;
import org.springframework.stereotype.Service;

import java.util.List;

@Service
public class GameSaveService {

    private final GameSaveRepository gameSaveRepository;
    private final UserRepository userRepository;

    public GameSaveService(GameSaveRepository gameSaveRepository, UserRepository userRepository) {
        this.gameSaveRepository = gameSaveRepository;
        this.userRepository = userRepository;
    }

    public GameSave saveGame(String username, GameSaveDto dto) {
        User user = userRepository.findByUsername(username)
                .orElseThrow(() -> new IllegalArgumentException("User not found"));

        GameSave save = new GameSave();
        save.setUser(user);
        save.setSaveName(dto.getSaveName());
        save.setGameStateJson(dto.getGameStateJson());
        return gameSaveRepository.save(save);
    }

    public List<GameSave> getSavesByUser(String username) {
        User user = userRepository.findByUsername(username)
                .orElseThrow(() -> new IllegalArgumentException("User not found"));
        return gameSaveRepository.findByUser(user);
    }
}
