package com.catan.catanbackend.controller;

import com.catan.catanbackend.model.ResourceGroup;
import com.catan.catanbackend.model.RobberBlocker;
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
    private final SessionService sessionService;
    private final SessionPlayerService sessionPlayerService;
    private final Mapper mapper;

    // Use constructor injection for all required services
    public GameController(TokenService tokenService,
                          GameService gameService,
                          SessionService sessionService,
                          SessionPlayerService sessionPlayerService,
                          Mapper mapper) {
        this.tokenService = tokenService;
        this.gameService = gameService;
        this.sessionService = sessionService;
        this.sessionPlayerService = sessionPlayerService;
        this.mapper = mapper;
    }

    @PostMapping("/deposit")
    public ResponseEntity<Void> depositResources(@RequestBody ResourceGroup resourceGroup,
                                                 @RequestHeader(name="Authorization") String token) {
        if (!token.startsWith("Bearer")) {
            return new ResponseEntity<>(HttpStatus.UNAUTHORIZED);
        }
        Long userId = tokenService.getUserIdFromJwtToken(token.split(" ")[1]);

        Optional<RobberBlocker> debt = gameService.findDebtByUserId(userId);
        if (debt.isEmpty()) {
            return new ResponseEntity<>(HttpStatus.NOT_FOUND);
        }

        // Validate the resourceGroup + check it matches the debt amount.
        // If everything is okay, proceed; otherwise return BAD_REQUEST.
        if (!resourceGroup.validate()
                || !Objects.equals(debt.get().getAmount(), resourceGroup.getSum())
                || !gameService.settleDebtByUserId(debt.get(), userId, resourceGroup)) {
            return new ResponseEntity<>(HttpStatus.BAD_REQUEST);
        }

        return new ResponseEntity<>(HttpStatus.OK);
    }

    @GetMapping("/resources")
    public ResponseEntity<ResourceGroup> getCurrentPlayerResources(@RequestHeader(name="Authorization") String token) {
        if (!token.startsWith("Bearer ")) {
            return new ResponseEntity<>(HttpStatus.UNAUTHORIZED);
        }

        String parsedToken = token.substring("Bearer ".length());
        Long userId = tokenService.getUserIdFromJwtToken(parsedToken);

        Optional<SessionPlayer> currentPlayer = sessionPlayerService.findCurrentSessionPlayerByUserId(userId);
        if (currentPlayer.isEmpty()) {
            return new ResponseEntity<>(HttpStatus.NOT_FOUND);
        }

        ResourceGroup rg = mapper.mapSessionPlayerToResource(currentPlayer.get());
        return new ResponseEntity<>(rg, HttpStatus.OK);
    }
}
