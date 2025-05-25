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

    @GetMapping("/session/code/{code}")
    public ResponseEntity<List<SessionPlayerDto>> getPlayersBySessionCode(@PathVariable String code) {
        List<SessionPlayer> players = sessionPlayerService.findPlayersBySessionCode(code);
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

    @GetMapping("/user/{userId}")
    public ResponseEntity<List<SessionPlayerDto>> getPlayersByUserId(@PathVariable Long userId) {
        List<SessionPlayer> players = sessionPlayerService.findPlayersByUserId(userId);
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

    @GetMapping("/user/{userId}/active")
    public ResponseEntity<List<SessionPlayerDto>> getActivePlayersByUserId(@PathVariable Long userId) {
        List<SessionPlayer> players = sessionPlayerService.findActivePlayersByUserId(userId);
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


//    @PutMapping("/{playerId}/resources")
//    public ResponseEntity<SessionPlayerDto> updateResources(
//            @PathVariable Long playerId,
//            @RequestBody ResourceGroup resources
//    ) {
//        return sessionPlayerService.findById(playerId)
//                .map(sp -> {
//                    sp.setResources(resources);
//                    SessionPlayer updated = sessionPlayerService.saveSessionPlayer(sp);
//                    SessionPlayerDto dto = new SessionPlayerDto(
//                            updated.getId(),
//                            updated.getSession().getId(),
//                            updated.getUser() != null ? updated.getUser().getId() : null,
//                            updated.getUser() != null ? updated.getUser().getUsername() : null,
//                            updated.getPlayerScore(),
//                            updated.getActive(),
//                            updated.getIsAi(),
//                            updated.getName(),
//                            updated.getBrick(),
//                            updated.getCrystal(),
//                            updated.getOre(),
//                            updated.getRice(),
//                            updated.getSheep(),
//                            updated.getSilver(),
//                            updated.getGold(),
//                            updated.getWood()
//                    );
//                    return ResponseEntity.ok(dto);
//                })
//                .orElseGet(() -> ResponseEntity.notFound().build());
//    }

}
