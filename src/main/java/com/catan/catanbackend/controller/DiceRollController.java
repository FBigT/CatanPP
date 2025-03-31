package com.catan.catanbackend.controller;


import com.catan.catanbackend.model.Session;
import com.catan.catanbackend.model.SessionPlayer;
import com.catan.catanbackend.service.*;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;
import org.springframework.web.client.HttpClientErrorException;

import java.util.Optional;

@CrossOrigin
@RestController
@RequestMapping("/api/dice")
public class DiceRollController {
    private final TokenService tokenService;
    private final SessionService sessionService;
    private final DiceRollService diceRollService;
    private final GameService gameService;
    private final SessionPlayerService sessionPlayerService;

    public DiceRollController(DiceRollService diceRollService, TokenService tokenService, SessionService sessionService, GameService gameService, SessionPlayerService sessionPlayerService) {
        this.diceRollService = diceRollService;
        this.tokenService = tokenService;
        this.sessionService = sessionService;
        this.gameService = gameService;
        this.sessionPlayerService = sessionPlayerService;
    }

    @GetMapping("/roll")
    public ResponseEntity<Integer> rollDice(@RequestHeader(name="Authorization") String token) {
        if (!token.startsWith("Bearer")) {
            return new ResponseEntity<>(HttpStatus.UNAUTHORIZED);
        }
        Long userId = tokenService.getUserIdFromJwtToken(token.split(" ")[1]);
        Optional<SessionPlayer> player = sessionPlayerService.findCurrentSessionPlayerByUserId(userId);
        if (player.isEmpty()) {
            return new ResponseEntity<>(HttpStatus.BAD_REQUEST);
        }

        int result = diceRollService.rollDice();
        if (result == 7 && !gameService.activateRobber(player.get().getSession().getId(), player.get()))
                return new ResponseEntity<>(HttpStatus.BAD_REQUEST);

        return ResponseEntity.ok(result);
    }
}
