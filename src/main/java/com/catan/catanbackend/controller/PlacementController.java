package com.catan.catanbackend.controller;

import com.catan.catanbackend.model.tile.Road;
import com.catan.catanbackend.model.tile.Structure;
import com.catan.catanbackend.service.PlacementService;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

@RestController
@RequestMapping("/api/place")
@CrossOrigin
public class PlacementController {

    private final PlacementService placementService;

    public PlacementController(PlacementService placementService) {
        this.placementService = placementService;
    }

    // ----------------------------------------------------------------
    // 1) Place Settlement (Structure)
    // ----------------------------------------------------------------
    @PostMapping("/structure")
    public ResponseEntity<?> placeStructure(@RequestParam Long sessionPlayerId,
                                            @RequestParam Long tileId,
                                            @RequestParam int cornerIndex) {
        try {
            Structure s = placementService.placeStructure(sessionPlayerId, tileId, cornerIndex);
            return ResponseEntity.ok(s);
        } catch (IllegalArgumentException e) {
            // e.g. "Not enough resources" or "Cannot place structure here"
            return ResponseEntity.status(HttpStatus.BAD_REQUEST).body(e.getMessage());
        }
    }

    // ----------------------------------------------------------------
    // 2) Place Road
    // ----------------------------------------------------------------
    @PostMapping("/road")
    public ResponseEntity<?> placeRoad(@RequestParam Long sessionPlayerId,
                                       @RequestParam Long tileId,
                                       @RequestParam int edgeIndex) {
        try {
            Road r = placementService.placeRoad(sessionPlayerId, tileId, edgeIndex);
            return ResponseEntity.ok(r);
        } catch (IllegalArgumentException e) {
            // e.g. "Not enough resources" or "Cannot place road here"
            return ResponseEntity.status(HttpStatus.BAD_REQUEST).body(e.getMessage());
        }
    }

    // ----------------------------------------------------------------
    // 3) Upgrade Settlement to City
    // ----------------------------------------------------------------
    @PutMapping("/structure/upgrade")
    public ResponseEntity<?> upgradeStructure(@RequestParam Long tileId,
                                              @RequestParam int cornerIndex,
                                              @RequestParam Long sessionPlayerId) {
        try {
            Structure upgraded = placementService.upgradeSettlementToCity(tileId, cornerIndex, sessionPlayerId);
            return ResponseEntity.ok(upgraded);
        } catch (IllegalArgumentException e) {
            // e.g. "Not enough resources" or "Only settlements can be upgraded"
            return ResponseEntity.status(HttpStatus.BAD_REQUEST).body(e.getMessage());
        }
    }
}
