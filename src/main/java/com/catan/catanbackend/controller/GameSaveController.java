package com.catan.catanbackend.controller;

import com.catan.catanbackend.model.GameSave;
import com.catan.catanbackend.model.dto.GameSaveDto;
import com.catan.catanbackend.service.GameSaveService;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.util.List;

@RestController
@RequestMapping("/api/saves")
@CrossOrigin
public class GameSaveController {

    private final GameSaveService gameSaveService;

    public GameSaveController(GameSaveService gameSaveService) {
        this.gameSaveService = gameSaveService;
    }

    @PostMapping("/{username}")
    public ResponseEntity<GameSave> saveGame(@PathVariable String username, @RequestBody GameSaveDto dto) {
        return ResponseEntity.ok(gameSaveService.saveGame(username, dto));
    }

    @GetMapping("/{username}")
    public ResponseEntity<List<GameSave>> getSaves(@PathVariable String username) {
        return ResponseEntity.ok(gameSaveService.getSavesByUser(username));
    }
}
