package com.catan.catanbackend.controller;

import com.catan.catanbackend.model.ResourceGroup;
import com.catan.catanbackend.model.RobberDebtBlocker;
import com.catan.catanbackend.model.SessionPlayer;
import com.catan.catanbackend.service.*;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.util.Objects;
import java.util.Optional;

@CrossOrigin
@RestController
@RequestMapping("/api/game")
public class GameController {

    private final TokenService tokenService;
    private final GameService gameService;
    private final SessionPlayerService sessionPlayerService;
    private final Mapper mapper;

    // Use constructor injection for all required services
    public GameController(TokenService tokenService,
                          GameService gameService,
                          SessionPlayerService sessionPlayerService,
                          Mapper mapper) {
        this.tokenService = tokenService;
        this.gameService = gameService;
        this.sessionPlayerService = sessionPlayerService;
        this.mapper = mapper;
    }

    @GetMapping("/resources/{sessioncode}/{username}")
    public ResponseEntity<ResourceGroup> getCurrentPlayerResources(@RequestHeader(name="Authorization") String token, @PathVariable String sessioncode, @PathVariable String username) {
        if (!token.startsWith("Bearer ")) {
            return new ResponseEntity<>(HttpStatus.UNAUTHORIZED);
        }

        Optional<SessionPlayer> first = sessionPlayerService.findPlayersBySessionCode(sessioncode).stream().filter(x -> Objects.equals(x.getName(), username)).findFirst();

        if (first.isPresent()) {
            ResourceGroup rg = mapper.mapSessionPlayerToResource(first.get());
            return new ResponseEntity<>(rg, HttpStatus.OK);
        }
        return new ResponseEntity<>(HttpStatus.NOT_FOUND);
    }
}
