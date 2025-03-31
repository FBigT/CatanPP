package com.catan.catanbackend.controller;

import com.catan.catanbackend.model.Road;
import com.catan.catanbackend.model.Structure;
import com.catan.catanbackend.service.PlacementService;
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

    @PostMapping("/structure")
    public ResponseEntity<Structure> placeStructure(@RequestParam String owner,
                                                    @RequestParam Long tileId,
                                                    @RequestParam int cornerIndex) {
        Structure s = placementService.placeStructure(owner, tileId, cornerIndex);
        return ResponseEntity.ok(s);
    }

    @PostMapping("/road")
    public ResponseEntity<Road> placeRoad(@RequestParam String owner,
                                          @RequestParam Long tileId,
                                          @RequestParam int edgeIndex) {
        Road r = placementService.placeRoad(owner, tileId, edgeIndex);
        return ResponseEntity.ok(r);
    }

    @GetMapping("/canPlace/structure")
    public ResponseEntity<Boolean> canPlaceStructure(@RequestParam Long tileId,
                                                     @RequestParam int cornerIndex) {
        boolean allowed = placementService.canPlaceStructure(tileId, cornerIndex);
        return ResponseEntity.ok(allowed);
    }

    @GetMapping("/canPlace/road")
    public ResponseEntity<Boolean> canPlaceRoad(@RequestParam Long tileId,
                                                @RequestParam int edgeIndex) {
        boolean allowed = placementService.canPlaceRoad(tileId, edgeIndex);
        return ResponseEntity.ok(allowed);
    }

    @GetMapping("/canPlace/structure/distance")
    public ResponseEntity<Boolean> canPlaceStructureWithDistance(@RequestParam Long tileId,
                                                                 @RequestParam int cornerIndex) {
        boolean allowed = placementService.canPlaceStructureWithDistanceRule(tileId, cornerIndex);
        return ResponseEntity.ok(allowed);
    }

    @PutMapping("/structure/upgrade")
    public ResponseEntity<Structure> upgradeStructure(@RequestParam Long tileId,
                                                      @RequestParam int cornerIndex,
                                                      @RequestParam String owner) {
        Structure upgraded = placementService.upgradeSettlementToCity(tileId, cornerIndex, owner);
        return ResponseEntity.ok(upgraded);
    }
}
