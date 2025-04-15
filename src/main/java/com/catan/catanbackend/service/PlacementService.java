package com.catan.catanbackend.service;

import com.catan.catanbackend.model.*;
import com.catan.catanbackend.repository.RoadRepository;
import com.catan.catanbackend.repository.StructureRepository;
import com.catan.catanbackend.repository.TileRepository;
import org.springframework.stereotype.Service;

import java.util.List;
import java.util.Optional;

@Service
public class PlacementService {

    private final TileRepository tileRepository;
    private final StructureRepository structureRepository;
    private final RoadRepository roadRepository;

    // We add references to these services so we can access the player's resources.
    private final SessionPlayerService sessionPlayerService;
    private final UserService userService;

    public PlacementService(TileRepository tileRepository,
                            StructureRepository structureRepository,
                            RoadRepository roadRepository,
                            SessionPlayerService sessionPlayerService,
                            UserService userService) {
        this.tileRepository = tileRepository;
        this.structureRepository = structureRepository;
        this.roadRepository = roadRepository;
        this.sessionPlayerService = sessionPlayerService;
        this.userService = userService;
    }

    // ----------------------------------------------------
    // 1) Place a Settlement
    // ----------------------------------------------------
    public Structure placeStructure(String owner, Long tileId, int cornerIndex) {
        // 1. Validate that the user + session player exist
        Optional<User> userOpt = userService.getUserByUsername(owner);
        if (userOpt.isEmpty()) {
            throw new IllegalArgumentException("No such user: " + owner);
        }
        User user = userOpt.get();

        // They must be in an active session
        Optional<SessionPlayer> spOpt = sessionPlayerService.findCurrentSessionPlayerByUserId(user.getId());
        if (spOpt.isEmpty()) {
            throw new IllegalArgumentException("User has no active session player");
        }
        SessionPlayer sp = spOpt.get();

        // 2. Check if the tile can be found
        Tile tile = tileRepository.findById(tileId)
                .orElseThrow(() -> new IllegalArgumentException("Tile not found: " + tileId));

        // 3. Check if we can place a structure physically using your existing rules
        if (!canPlaceStructureWithDistanceRule(tileId, cornerIndex)) {
            throw new IllegalArgumentException("Cannot place structure at tile=" + tileId + ", corner=" + cornerIndex);
        }

        // 4. **Check Resource Cost** for Settlement: (1 Brick, 1 Lumber, 1 Wool, 1 Grain)
        if (sp.getBricks() < 1 || sp.getLumber() < 1 || sp.getWool() < 1 || sp.getGrain() < 1) {
            throw new IllegalArgumentException("Not enough resources to buy a settlement");
        }

        // 5. Deduct the cost
        sp.setBricks(sp.getBricks() - 1);
        sp.setLumber(sp.getLumber() - 1);
        sp.setWool(sp.getWool() - 1);
        sp.setGrain(sp.getGrain() - 1);
        sessionPlayerService.updateSessionPlayer(sp);

        // 6. Actually place the structure
        tile.getCorners().set(cornerIndex, true);
        tileRepository.save(tile);

        // Create the Structure entity
        Structure structure = new Structure(owner, tile, cornerIndex);
        // By default it has type "SETTLEMENT"
        structure = structureRepository.save(structure);

        return structure;
    }

    // ----------------------------------------------------
    // 2) Place a Road
    // ----------------------------------------------------
    public Road placeRoad(String owner, Long tileId, int edgeIndex) {
        // 1. Validate the user + session player
        Optional<User> userOpt = userService.getUserByUsername(owner);
        if (userOpt.isEmpty()) {
            throw new IllegalArgumentException("No such user: " + owner);
        }
        User user = userOpt.get();

        Optional<SessionPlayer> spOpt = sessionPlayerService.findCurrentSessionPlayerByUserId(user.getId());
        if (spOpt.isEmpty()) {
            throw new IllegalArgumentException("User has no active session player");
        }
        SessionPlayer sp = spOpt.get();

        // 2. Find the tile
        Tile tile = tileRepository.findById(tileId)
                .orElseThrow(() -> new IllegalArgumentException("Tile not found: " + tileId));

        // 3. Check if we can place a road physically
        if (!canPlaceRoad(tileId, edgeIndex)) {
            throw new IllegalArgumentException("Cannot place road at tile=" + tileId + ", edge=" + edgeIndex);
        }

        // 4. **Check Resource Cost** for Road: (1 Brick, 1 Lumber)
        if (sp.getBricks() < 1 || sp.getLumber() < 1) {
            throw new IllegalArgumentException("Not enough resources to buy a road");
        }

        // 5. Deduct resources
        sp.setBricks(sp.getBricks() - 1);
        sp.setLumber(sp.getLumber() - 1);
        sessionPlayerService.updateSessionPlayer(sp);

        // 6. Place the road
        tile.getEdges().set(edgeIndex, true);
        tileRepository.save(tile);

        Road road = new Road();
        road.setOwner(owner);
        road.setTile(tile);
        road.setEdgeIndex(edgeIndex);
        return roadRepository.save(road);
    }

    // ----------------------------------------------------
    // 3) Upgrade a Settlement to City
    // ----------------------------------------------------
    public Structure upgradeSettlementToCity(Long tileId, int cornerIndex, String owner) {
        // 1. Validate the user + session player
        Optional<User> userOpt = userService.getUserByUsername(owner);
        if (userOpt.isEmpty()) {
            throw new IllegalArgumentException("No such user: " + owner);
        }
        User user = userOpt.get();

        Optional<SessionPlayer> spOpt = sessionPlayerService.findCurrentSessionPlayerByUserId(user.getId());
        if (spOpt.isEmpty()) {
            throw new IllegalArgumentException("User has no active session player");
        }
        SessionPlayer sp = spOpt.get();

        // 2. Find the existing structure
        Structure structure = structureRepository.findByTileIdAndCornerIndex(tileId, cornerIndex);
        if (structure == null) {
            throw new IllegalArgumentException("No structure found at tile=" + tileId + ", corner=" + cornerIndex);
        }

        // 3. Must be owned by the same player
        if (!structure.getOwner().equals(owner)) {
            throw new IllegalArgumentException("You do not own this settlement");
        }

        // 4. Must be an actual settlement to upgrade
        if (!structure.getType().equals("SETTLEMENT")) {
            throw new IllegalArgumentException("Only settlements can be upgraded to a city");
        }

        // 5. Check City cost: (2 Grain, 3 Ore) example
        if (sp.getGrain() < 2 || sp.getOre() < 3) {
            throw new IllegalArgumentException("Not enough resources to upgrade to city");
        }

        // 6. Deduct city cost
        sp.setGrain(sp.getGrain() - 2);
        sp.setOre(sp.getOre() - 3);
        sessionPlayerService.updateSessionPlayer(sp);

        // 7. Upgrade
        structure.setType("CITY");
        return structureRepository.save(structure);
    }

    // ----------------------------------------------------
    // Helper: canPlaceStructureWithDistanceRule
    // ----------------------------------------------------
    public boolean canPlaceStructureWithDistanceRule(Long tileId, int cornerIndex) {
        Tile tile = tileRepository.findById(tileId)
                .orElseThrow(() -> new IllegalArgumentException("Tile not found"));
        if (tile.getCorners().get(cornerIndex)) {
            return false;
        }

        List<Structure> allStructures = structureRepository.findAll();
        for (Structure s : allStructures) {
            if (areCornersTooClose(tile, cornerIndex, s.getTile(), s.getCornerIndex())) {
                return false;
            }
        }
        return true;
    }

    private boolean areCornersTooClose(Tile t1, int c1, Tile t2, int c2) {
        if (t1.getId().equals(t2.getId())) {
            return Math.abs(c1 - c2) == 1 || Math.abs(c1 - c2) == 5;
        }
        return false;
    }

    // ----------------------------------------------------
    // Helper: canPlaceRoad
    // ----------------------------------------------------
    public boolean canPlaceRoad(Long tileId, int edgeIndex) {
        Tile tile = tileRepository.findById(tileId)
                .orElseThrow(() -> new IllegalArgumentException("Tile not found"));
        if (tile.getEdges().get(edgeIndex)) {
            return false;
        }

        List<Road> roads = roadRepository.findAll();
        List<Structure> structures = structureRepository.findAll();

        for (Road r : roads) {
            if (r.getTile().getId().equals(tileId)
                    && Math.abs(r.getEdgeIndex() - edgeIndex) == 1
                    && r.getOwner().equals(tile.getEdges().get(edgeIndex))) {
                return true;
            }
        }
        for (Structure s : structures) {
            if (s.getTile().getId().equals(tileId)
                    && isCornerAdjacentToEdge(s.getCornerIndex(), edgeIndex)) {
                return true;
            }
        }
        return true; // or false, depending on your adjacency logic
    }

    private boolean isCornerAdjacentToEdge(int cornerIndex, int edgeIndex) {
        return cornerIndex == edgeIndex || cornerIndex == (edgeIndex + 1) % 6;
    }
}
