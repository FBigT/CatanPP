package com.catan.catanbackend.controller;

import com.catan.catanbackend.model.ResourceGroup;
import com.catan.catanbackend.model.RobberBlocker;
import com.catan.catanbackend.service.GameService;
import com.catan.catanbackend.service.TokenService;
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

    public GameController(GameService gameService, TokenService tokenService) {
        this.gameService = gameService;
        this.tokenService = tokenService;
    }

    @PostMapping("/deposit")
    public ResponseEntity<Void> depositResources(@RequestBody ResourceGroup resourceGroup, @RequestHeader (name="Authorization") String token) {
        if (!token.startsWith("Bearer")) {
            return new ResponseEntity<>(HttpStatus.UNAUTHORIZED);
        }

        Long userId = tokenService.getUserIdFromJwtToken(token.split(" ")[1]);
        Optional<RobberBlocker> debt = gameService.findDebtByUserId(userId);

        if (debt.isEmpty()) {
            return new ResponseEntity<>(HttpStatus.NOT_FOUND);
        }

        if (!resourceGroup.validate() || !Objects.equals(debt.get().getAmount(), resourceGroup.getSum()) || !gameService.settleDebtByUserId(debt.get(), userId, resourceGroup)) {
            return new ResponseEntity<>(HttpStatus.BAD_REQUEST);
        }
        return new ResponseEntity<>(HttpStatus.OK);
    }
}
