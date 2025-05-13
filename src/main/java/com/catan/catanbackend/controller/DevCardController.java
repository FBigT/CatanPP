package com.catan.catanbackend.controller;

import com.catan.catanbackend.model.DevCard;
import com.catan.catanbackend.service.DevCardService;
import com.catan.catanbackend.service.TokenService;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.util.List;

@RestController
@RequestMapping("/api/devcards")
@CrossOrigin(origins = "*")
public class DevCardController {
    private final DevCardService devCardService;
    private final TokenService tokenService;

    public DevCardController(DevCardService devCardService, TokenService tokenService) {
        this.devCardService = devCardService;
        this.tokenService = tokenService;
    }

    /** Buy a new dev card */
    @PostMapping("/buy")
    public ResponseEntity<DevCard> buy(@RequestHeader("Authorization") String auth) {
        String token = auth.replace("Bearer ","");
        Long userId = tokenService.getUserIdFromJwtToken(token);
        DevCard card = devCardService.buyDevCard(userId);
        return ResponseEntity.ok(card);
    }

    /** List my dev cards */
    @GetMapping("/player/{playerId}")
    public ResponseEntity<List<DevCard>> list(@PathVariable Long playerId) {
        List<DevCard> cards = devCardService.getPlayerCards(playerId);
        return ResponseEntity.ok(cards);
    }

    /** Use/activate a dev card */
    @PostMapping("/use/{cardId}")
    public ResponseEntity<DevCard> use(@PathVariable Long cardId,
                                       @RequestHeader("Authorization") String auth) {
        String token = auth.replace("Bearer ","");
        Long userId = tokenService.getUserIdFromJwtToken(token);
        DevCard used = devCardService.useCard(cardId, userId);
        return ResponseEntity.ok(used);
    }
}
