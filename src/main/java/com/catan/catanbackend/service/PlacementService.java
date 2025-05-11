package com.catan.catanbackend.service;

import com.catan.catanbackend.model.*;
import com.catan.catanbackend.model.tile.*;
import com.catan.catanbackend.repository.tiles.*;
import org.springframework.stereotype.Service;

import java.util.List;
import java.util.Optional;

@Service
public class PlacementService {

    private final TileRepository tileRepository;
    private final StructureRepository structureRepository;
    private final StructureTypeRepository structureTypeRepository;
    private final RoadRepository roadRepository;
    private final TileEdgeRepository tileEdgeRepository;
    private final TileCornerRepository tileCornerRepository;

    // We add references to these services so we can access the player's resources.
    private final SessionPlayerService sessionPlayerService;
    private final UserService userService;

    public PlacementService(TileRepository tileRepository,
                            StructureRepository structureRepository,
                            StructureTypeRepository structureTypeRepository,
                            RoadRepository roadRepository,
                            TileEdgeRepository tileEdgeRepository,
                            TileCornerRepository tileCornerRepository,
                            SessionPlayerService sessionPlayerService,
                            UserService userService) {
        this.tileRepository = tileRepository;
        this.structureRepository = structureRepository;
        this.structureTypeRepository = structureTypeRepository;
        this.roadRepository = roadRepository;
        this.tileEdgeRepository = tileEdgeRepository;
        this.tileCornerRepository = tileCornerRepository;
        this.sessionPlayerService = sessionPlayerService;
        this.userService = userService;
    }

    // ----------------------------------------------------
    // 1) Place a Settlement
    // ----------------------------------------------------
    public Structure placeStructure(Long owner, Long tileId, int cornerIndex) {
        /*Optional<User> userOpt = userService.getUserByUsername(owner);
        if (userOpt.isEmpty()) {
            throw new IllegalArgumentException("No such user: " + owner);
        }
        User user = userOpt.get();*/

        Optional<SessionPlayer> spOpt = sessionPlayerService.findCurrentSessionPlayerByUserId(owner);
        if (spOpt.isEmpty()) {
            throw new IllegalArgumentException("User has no active session player");
        }
        SessionPlayer sp = spOpt.get();

        Tile tile = tileRepository.findById(tileId)
                .orElseThrow(() -> new IllegalArgumentException("Tile not found: " + tileId));

        if (!canPlaceStructureWithDistanceRule(tileId, cornerIndex)) {
            throw new IllegalArgumentException("Cannot place structure at tile=" + tileId + ", corner=" + cornerIndex);
        }

        // 4. Check Resource Cost for Settlement: (1 Brick, 1 Lumber, 1 Wool, 1 Grain)
        if (sp.getBrick() < 1 || sp.getWood() < 1 || sp.getSheep() < 1 || sp.getRice() < 1) {
            throw new IllegalArgumentException("Not enough resources to buy a settlement");
        }

        // 5. Deduct the cost
        sp.setBrick(sp.getBrick() - 1);
        sp.setWood(sp.getWood() - 1);
        sp.setSheep(sp.getSheep() - 1);
        sp.setRice(sp.getRice() - 1);
        sessionPlayerService.updateSessionPlayer(sp);

        Structure structure = new Structure(sp, tile, cornerIndex);
        structure = structureRepository.save(structure);

        Optional<TileCornerMap> tileCornerMap = tile.getTileCornerMaps().stream().filter(x -> x.getCornerIndex() == cornerIndex).findFirst();
        if (tileCornerMap.isPresent()) {
            TileCorner tileCorner = tileCornerMap.get().getCorner();
            tileCorner.setStructure(structure);
            tileCornerRepository.save(tileCorner);
        }

        return structure;
    }

    // ----------------------------------------------------
    // 2) Place a Road
    // ----------------------------------------------------
    public Road placeRoad(Long sessionPlayerId, Long tileId, int edgeIndex) {
        /*Optional<User> userOpt = userService.getUserByUsername(sessionPlayerId);
        if (userOpt.isEmpty()) {
            throw new IllegalArgumentException("No such user: " + sessionPlayerId);
        }
        User user = userOpt.get();*/

        Optional<SessionPlayer> spOpt = sessionPlayerService.findById(sessionPlayerId);
        if (spOpt.isEmpty()) {
            throw new IllegalArgumentException("User has no active session player");
        }
        SessionPlayer sp = spOpt.get();

        Tile tile = tileRepository.findById(tileId)
                .orElseThrow(() -> new IllegalArgumentException("Tile not found: " + tileId));

        if (!canPlaceRoad(sessionPlayerId, tileId, edgeIndex)) {
            throw new IllegalArgumentException("Cannot place road at tile=" + tileId + ", edge=" + edgeIndex);
        }

        // 4. Check Resource Cost for Road: (1 Brick, 1 Lumber)
        if (sp.getBrick() < 1 || sp.getWood() < 1) {
            throw new IllegalArgumentException("Not enough resources to buy a road");
        }

        // 5. Deduct resources
        sp.setBrick(sp.getBrick() - 1);
        sp.setWood(sp.getWood() - 1);
        sessionPlayerService.updateSessionPlayer(sp);

        Optional<TileEdge> tileEdgeMap = tile.getTileEdge(edgeIndex);
        if (tileEdgeMap.isEmpty()) {
            throw new IllegalArgumentException("No tile edge found for edge=" + edgeIndex);
        }
        TileEdge tileEdge = tileEdgeMap.get();

        Road road = new Road();
        road.setOwner(sp);
        road.setTileEdge(tileEdge);
        road = roadRepository.save(road);

        tileEdge.setRoad(road);
        tileEdgeRepository.save(tileEdge);

        return road;
    }

    // ----------------------------------------------------
    // 3) Upgrade a Settlement to City
    // ----------------------------------------------------
    public Structure upgradeSettlementToCity(Long tileId, int cornerIndex, Long ownerId) {
        /*Optional<User> userOpt = userService.getUserByUsername(owner);
        if (userOpt.isEmpty()) {
            throw new IllegalArgumentException("No such user: " + owner);
        }
        User user = userOpt.get();*/

        Optional<SessionPlayer> spOpt = sessionPlayerService.findById(ownerId);
        if (spOpt.isEmpty()) {
            throw new IllegalArgumentException("User has no active session player");
        }
        SessionPlayer sp = spOpt.get();

        Structure structure = structureRepository.findByTileIdAndCornerIndex(tileId, cornerIndex);
        if (structure == null) {
            throw new IllegalArgumentException("No structure found at tile=" + tileId + ", corner=" + cornerIndex);
        }

        if (!structure.getOwner().getId().equals(sp.getId())) {
            throw new IllegalArgumentException("You do not own this settlement");
        }

        if (!structure.getStructureType().getName().equals(StructureTypeEnum.SETTLEMENT.name())) {
            throw new IllegalArgumentException("Only settlements can be upgraded to a city");
        }

        // 5. Check City cost: (2 Grain, 3 Ore)
        if (sp.getRice() < 2 || sp.getOre() < 3) {
            throw new IllegalArgumentException("Not enough resources to upgrade to city");
        }

        // 6. Deduct city cost
        sp.setRice(sp.getRice() - 2);
        sp.setOre(sp.getOre() - 3);
        sessionPlayerService.updateSessionPlayer(sp);

        structure.setStructureType(structureTypeRepository.findByEnumOrCreate(StructureTypeEnum.CITY));
        return structureRepository.save(structure);
    }

    // ----------------------------------------------------
    // Helper: canPlaceStructureWithDistanceRule
    // ----------------------------------------------------
    public boolean canPlaceStructureWithDistanceRule(Long tileId, int cornerIndex) {
        Tile tile = tileRepository.findById(tileId)
                .orElseThrow(() -> new IllegalArgumentException("Tile not found"));
        Optional<TileCorner> optionalTileCorner = tile.getTileCorner(cornerIndex);
        if (optionalTileCorner.isEmpty() || optionalTileCorner.get().getStructure() != null) {
            return false;
        }
        TileCorner tileCorner = optionalTileCorner.get();
        List<TileEdge> tileEdges = tileEdgeRepository.findByCorner(tileCorner);
        for (TileEdge tileEdge : tileEdges) {
            if (tileEdge.getCornerB() != tileCorner && tileEdge.getCornerB().getStructure() != null) {
                return false;
            }
            if (tileEdge.getCornerA() != tileCorner && tileEdge.getCornerA().getStructure() != null) {
                return false;
            }
        }
        return true;
    }

    /*private boolean areCornersTooClose(Tile t1, int c1, Tile t2, int c2) {
        if (t1.getId().equals(t2.getId())) {
            return Math.abs(c1 - c2) == 1 || Math.abs(c1 - c2) == 5;
        }
        return false;
    }*/

    // ----------------------------------------------------
    // Helper: canPlaceRoad
    // ----------------------------------------------------
    public boolean canPlaceRoad(Long sessionPlayerId, Long tileId, int edgeIndex) {
        Tile tile = tileRepository.findById(tileId)
                .orElseThrow(() -> new IllegalArgumentException("Tile not found"));
        Optional<TileEdge> optionalTileEdge = tile.getTileEdge(edgeIndex);
        if (optionalTileEdge.isEmpty() || optionalTileEdge.get().getRoad() != null) {
            return false;
        }

        TileEdge tileEdge = optionalTileEdge.get();

        if (tileEdge.getCornerB().getStructure() != null
                && tileEdge.getCornerB().getStructure().getOwner().getId().equals(sessionPlayerId)
        || (tileEdge.getCornerA().getStructure() != null
                && tileEdge.getCornerA().getStructure().getOwner().getId().equals(sessionPlayerId))) {
            return true;
        }

        List<Road> playerRoads = roadRepository.findByOwnerSessionId(tile.getSession().getId()).stream()
                .filter(road -> road.getOwner().getId().equals(sessionPlayerId))
                .toList();

        for (Road road : playerRoads) {
            TileEdge currentEdge = road.getTileEdge();

            TileCorner otherA = currentEdge.getCornerA();
            TileCorner otherB = currentEdge.getCornerB();

            if (otherA.equals(tileEdge.getCornerA()) || otherA.equals(tileEdge.getCornerB())
                    || otherB.equals(tileEdge.getCornerA()) || otherB.equals(tileEdge.getCornerB())) {
                return true;
            }
        }
        return false; // or true, depending on your adjacency logic
    }

    /*private boolean isCornerAdjacentToEdge(int cornerIndex, int edgeIndex) {
        return cornerIndex == edgeIndex || cornerIndex == (edgeIndex + 1) % 6;
    }*/
}
