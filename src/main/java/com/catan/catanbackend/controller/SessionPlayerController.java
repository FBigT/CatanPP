// src/main/java/com/catan/catanbackend/controller/SessionPlayerController.java
package com.catan.catanbackend.controller;

import com.catan.catanbackend.model.SessionPlayer;
import com.catan.catanbackend.model.dto.SessionPlayerDto;
import com.catan.catanbackend.service.SessionPlayerService;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.util.List;

@RestController
@RequestMapping("/api/session-players")
@CrossOrigin(origins = "*")
public class SessionPlayerController {

    private final SessionPlayerService sessionPlayerService;

    public SessionPlayerController(SessionPlayerService sessionPlayerService) {
        this.sessionPlayerService = sessionPlayerService;
    }

    @GetMapping("/session/{sessionId}")
    public ResponseEntity<List<SessionPlayerDto>> getPlayersBySessionId(@PathVariable Long sessionId) {
        List<SessionPlayer> players = sessionPlayerService.findPlayerBySessionId(sessionId);
        List<SessionPlayerDto> dtos = players.stream()
                .map(p -> new SessionPlayerDto(
                        p.getId(),
                        p.getSession().getId(),
                        p.getUser() != null ? p.getUser().getId() : null,
                        p.getUser() != null ? p.getUser().getUsername() : null,
                        p.getPlayerScore(),
                        p.getActive(),
                        p.getIsAi(),
                        p.getName(),
                        p.getBrick(),
                        p.getCrystal(),
                        p.getOre(),
                        p.getRice(),
                        p.getSheep(),
                        p.getSilver(),
                        p.getGold(),
                        p.getWood()
                ))
                .toList();
        return ResponseEntity.ok(dtos);
    }
}