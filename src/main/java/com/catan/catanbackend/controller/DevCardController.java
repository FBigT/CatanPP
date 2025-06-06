package com.catan.catanbackend.controller;

import com.catan.catanbackend.model.DevCard;
import com.catan.catanbackend.model.SessionPlayer;
import com.catan.catanbackend.service.DevCardService;
import com.catan.catanbackend.service.SessionPlayerService;
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
    private final SessionPlayerService sessionPlayerService;

    public DevCardController(DevCardService devCardService,
                             TokenService tokenService,
                             SessionPlayerService sessionPlayerService) {
        this.devCardService = devCardService;
        this.tokenService = tokenService;
        this.sessionPlayerService = sessionPlayerService;
    }

    @PostMapping("/buy")
    public ResponseEntity<DevCard> buy(@RequestHeader("Authorization") String auth) {
        System.out.println("🛒 [DevCardController] === BUY DEV CARD REQUEST ===");
        System.out.println("🛒 [DevCardController] Timestamp: " + java.time.LocalDateTime.now());
        System.out.println("🛒 [DevCardController] Authorization header: " + (auth != null ? auth.substring(0, 20) + "..." : "NULL"));

        try {
            String token = auth.replace("Bearer ", "");
            System.out.println("🛒 [DevCardController] Cleaned token: " + token.substring(0, 20) + "...");

            Long userId = tokenService.getUserIdFromJwtToken(token);
            System.out.println("🛒 [DevCardController] Extracted User ID: " + userId);

            // Convert userId to sessionPlayerId
            SessionPlayer sessionPlayer = sessionPlayerService.findCurrentSessionPlayerByUserId(userId)
                    .orElseThrow(() -> new IllegalArgumentException("Player not in active session"));

            System.out.println("🛒 [DevCardController] Found SessionPlayer: " + sessionPlayer.getId());
            System.out.println("🛒 [DevCardController] SessionPlayer Name: " + sessionPlayer.getName());
            System.out.println("🛒 [DevCardController] SessionPlayer Resources: Ore=" + sessionPlayer.getOre() +
                    ", Rice=" + sessionPlayer.getRice() + ", Sheep=" + sessionPlayer.getSheep());

            DevCard card = devCardService.buyDevCard(sessionPlayer.getId());

            System.out.println("🛒 [DevCardController] ✅ Card purchased successfully!");
            System.out.println("🛒 [DevCardController] Card Type: " + card.getType());
            System.out.println("🛒 [DevCardController] Card ID: " + card.getId());
            System.out.println("🛒 [DevCardController] Card Playable: " + card.isPlayable());
            System.out.println("🛒 [DevCardController] Card Used: " + card.isUsed());

            return ResponseEntity.ok(card);

        } catch (IllegalArgumentException e) {
            System.out.println("❌ [DevCardController] Validation error: " + e.getMessage());
            throw e;
        } catch (Exception e) {
            System.out.println("❌ [DevCardController] Unexpected error: " + e.getMessage());
            System.out.println("❌ [DevCardController] Exception type: " + e.getClass().getSimpleName());
            e.printStackTrace();
            throw e;
        }
    }

    /** List my dev cards */
    @GetMapping("/player/{playerId}")
    public ResponseEntity<List<DevCard>> list(@PathVariable Long playerId) {
        System.out.println("📋 [DevCardController] === LIST DEV CARDS REQUEST ===");
        System.out.println("📋 [DevCardController] Timestamp: " + java.time.LocalDateTime.now());
        System.out.println("📋 [DevCardController] Player ID: " + playerId);

        try {
            List<DevCard> cards = devCardService.getPlayerCards(playerId);

            System.out.println("📋 [DevCardController] ✅ Found " + cards.size() + " cards for player " + playerId);

            if (!cards.isEmpty()) {
                System.out.println("📋 [DevCardController] Card details:");
                for (int i = 0; i < cards.size(); i++) {
                    DevCard card = cards.get(i);
                    System.out.println("📋 [DevCardController]   " + (i + 1) + ". " + card.getType() +
                            " (ID: " + card.getId() + ", playable: " + card.isPlayable() +
                            ", used: " + card.isUsed() + ")");
                }
            } else {
                System.out.println("📋 [DevCardController] No cards found for player " + playerId);
            }

            return ResponseEntity.ok(cards);

        } catch (Exception e) {
            System.out.println("❌ [DevCardController] Error listing cards: " + e.getMessage());
            e.printStackTrace();
            throw e;
        }
    }

    /** Use/activate a dev card */
    @PostMapping("/use/{cardId}")
    public ResponseEntity<DevCard> use(@PathVariable Long cardId,
                                       @RequestHeader("Authorization") String auth) {
        System.out.println("🃏 [DevCardController] === USE DEV CARD REQUEST ===");
        System.out.println("🃏 [DevCardController] Timestamp: " + java.time.LocalDateTime.now());
        System.out.println("🃏 [DevCardController] Card ID: " + cardId);
        System.out.println("🃏 [DevCardController] Authorization header: " + (auth != null ? auth.substring(0, 20) + "..." : "NULL"));

        try {
            String token = auth.replace("Bearer ", "");
            Long userId = tokenService.getUserIdFromJwtToken(token);
            System.out.println("🃏 [DevCardController] User ID: " + userId);

            DevCard used = devCardService.useCard(cardId, userId);

            System.out.println("🃏 [DevCardController] ✅ Card used successfully!");
            System.out.println("🃏 [DevCardController] Card Type: " + used.getType());
            System.out.println("🃏 [DevCardController] Card ID: " + used.getId());
            System.out.println("🃏 [DevCardController] Card Used: " + used.isUsed());

            return ResponseEntity.ok(used);

        } catch (IllegalArgumentException e) {
            System.out.println("❌ [DevCardController] Validation error using card: " + e.getMessage());
            throw e;
        } catch (Exception e) {
            System.out.println("❌ [DevCardController] Unexpected error using card: " + e.getMessage());
            e.printStackTrace();
            throw e;
        }
    }

    /** Debug endpoint to check deck composition */
    @GetMapping("/debug/deck/{sessionId}")
    public ResponseEntity<String> debugDeck(@PathVariable Long sessionId) {
        System.out.println("🔍 [DevCardController] === DEBUG DECK COMPOSITION ===");
        System.out.println("🔍 [DevCardController] Session ID: " + sessionId);

        try {
            List<DevCard> allCards = devCardService.getAllDevCardsBySessionId(sessionId);

            // Count cards by type
            long knightCount = allCards.stream().filter(c -> c.getType().name().equals("KNIGHT")).count();
            long vpCount = allCards.stream().filter(c -> c.getType().name().equals("VICTORY_POINT")).count();
            long roadCount = allCards.stream().filter(c -> c.getType().name().equals("ROAD_BUILDING")).count();
            long yearCount = allCards.stream().filter(c -> c.getType().name().equals("YEAR_OF_PLENTY")).count();

            long ownedCards = allCards.stream().filter(c -> c.getOwner() != null).count();
            long remainingCards = allCards.stream().filter(c -> c.getOwner() == null).count();

            System.out.println("🔍 [DevCardController] Total cards: " + allCards.size());
            System.out.println("🔍 [DevCardController] KNIGHT cards: " + knightCount);
            System.out.println("🔍 [DevCardController] VICTORY_POINT cards: " + vpCount);
            System.out.println("🔍 [DevCardController] ROAD_BUILDING cards: " + roadCount);
            System.out.println("🔍 [DevCardController] YEAR_OF_PLENTY cards: " + yearCount);
            System.out.println("🔍 [DevCardController] Owned cards: " + ownedCards);
            System.out.println("🔍 [DevCardController] Remaining in deck: " + remainingCards);

            String response = String.format(
                    "Total: %d, Knight: %d, VP: %d, Road: %d, Year: %d, Owned: %d, Remaining: %d",
                    allCards.size(), knightCount, vpCount, roadCount, yearCount, ownedCards, remainingCards
            );

            return ResponseEntity.ok(response);

        } catch (Exception e) {
            System.out.println("❌ [DevCardController] Error debugging deck: " + e.getMessage());
            e.printStackTrace();
            return ResponseEntity.ok("Error: " + e.getMessage());
        }
    }
}
