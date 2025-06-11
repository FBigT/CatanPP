package com.catan.catanbackend.service;

import com.catan.catanbackend.controller.web_socket.WebSocketController;
import com.catan.catanbackend.model.*;
import com.catan.catanbackend.model.dto.ChatMessage;
import com.catan.catanbackend.model.dto.RawChatMessage;
import com.catan.catanbackend.model.dto.move_dtos.RobberMoveDto;
import com.catan.catanbackend.model.dto.move_dtos.responses.RobberMoveResponseDto;
import com.catan.catanbackend.model.helper.ResourceType;
import com.catan.catanbackend.model.helper.StructureTypeEnum;
import com.catan.catanbackend.model.tile.*;
import com.catan.catanbackend.repository.SessionCodeRepository;
import com.catan.catanbackend.repository.tiles.*;
import org.springframework.stereotype.Service;

import java.util.*;

@Service
public class PlacementService {

    private final TileService tileService;
    private final StructureRepository structureRepository;
    private final StructureTypeRepository structureTypeRepository;
    private final RoadRepository roadRepository;
    private final TileEdgeRepository tileEdgeRepository;
    private final TileCornerRepository tileCornerRepository;
    private final PlayerProfileService playerProfileService;
    private final NotificationService notificationService;
    private final SessionService sessionService;
    private final SessionCodeRepository sessionCodeRepository;


    // We add references to these services so we can access the player's resources.
    private final SessionPlayerService sessionPlayerService;

    public PlacementService(TileService tileService,
                            StructureRepository structureRepository,
                            StructureTypeRepository structureTypeRepository,
                            RoadRepository roadRepository,
                            TileEdgeRepository tileEdgeRepository,
                            TileCornerRepository tileCornerRepository, PlayerProfileService playerProfileService, NotificationService notificationService, SessionService sessionService, SessionCodeRepository sessionCodeRepository,
                            SessionPlayerService sessionPlayerService) {
        this.tileService = tileService;
        this.structureRepository = structureRepository;
        this.structureTypeRepository = structureTypeRepository;
        this.roadRepository = roadRepository;
        this.tileEdgeRepository = tileEdgeRepository;
        this.tileCornerRepository = tileCornerRepository;
        this.playerProfileService = playerProfileService;
        this.notificationService = notificationService;
        this.sessionService = sessionService;
        this.sessionCodeRepository = sessionCodeRepository;
        this.sessionPlayerService = sessionPlayerService;
    }

    // ----------------------------------------------------
    // 1) Place a Settlement
    // ----------------------------------------------------
    public Structure placeStructure(Long owner, Long tileId, Integer cornerIndex, StructureTypeEnum structureType, Boolean ignoreCost) {
        Optional<SessionPlayer> spOpt = sessionPlayerService.findById(owner);
        if (spOpt.isEmpty()) {
            throw new IllegalArgumentException("User has no active session player");
        }
        SessionPlayer sp = spOpt.get();

        Tile tile = tileService.findById(tileId)
                .orElseThrow(() -> new IllegalArgumentException("Tile not found: " + tileId));

        if (!canPlaceStructureWithDistanceRule(tileId, cornerIndex)) {
            throw new IllegalArgumentException("Cannot place structure at tile=" + tileId + ", corner=" + cornerIndex);
        }

        if (!ignoreCost) {
            // 4. Check Resource Cost for Settlement: (1 Brick, 1 Lumber, 1 Wool, 1 Grain)
            if (sp.getBrick() < 1 || sp.getWood() < 1 || sp.getSheep() < 1 || sp.getWheat() < 1) {
                throw new IllegalArgumentException("Not enough resources to buy a settlement");
            }

            // 5. Deduct the cost
            sp.setBrick(sp.getBrick() - 1);
            sp.setWood(sp.getWood() - 1);
            sp.setSheep(sp.getSheep() - 1);
            sp.setWheat(sp.getWheat() - 1);
            sp.setPlayerScore(sp.getPlayerScore() + 1);
        }
        sp.setSettlementsPlaced(sp.getSettlementsPlaced() + 1);
        sessionPlayerService.updateSessionPlayer(sp);

        if (sp.getUser() != null) {
            Optional<PlayerProfile> playerProfileByUserId = playerProfileService.getPlayerProfileByUserId(sp.getUser().getId());
            if (playerProfileByUserId.isPresent()) {
                PlayerProfile playerProfile = playerProfileByUserId.get();
                playerProfile.setStructuresPlaced(playerProfile.getStructuresPlaced() + 1);
                playerProfileService.savePlayerProfile(playerProfile);
            }
        }

        Structure structure = new Structure(sp, tile, cornerIndex, structureTypeRepository.findByEnumOrCreate(structureType));
        structure = structureRepository.save(structure);

        Optional<TileCornerMap> tileCornerMap = tile.getTileCornerMaps().stream().filter(x -> x.getCornerIndex() == cornerIndex).findFirst();
        if (tileCornerMap.isPresent()) {
            TileCorner tileCorner = tileCornerMap.get().getCorner();
            structure.setCorner(tileCorner);
            tileCorner.setStructure(structure);
            tileCornerRepository.save(tileCorner);
            structureRepository.save(structure);
        }

        return structure;
    }

    // ----------------------------------------------------
    // 2) Place a Road
    // ----------------------------------------------------
    public Road placeRoad(Long sessionPlayerId, Long tileId, int edgeIndex, Boolean ignoreCost) {
        Optional<SessionPlayer> spOpt = sessionPlayerService.findById(sessionPlayerId);
        if (spOpt.isEmpty()) {
            throw new IllegalArgumentException("User has no active session player");
        }
        SessionPlayer sp = spOpt.get();

        Tile tile = tileService.findById(tileId)
                .orElseThrow(() -> new IllegalArgumentException("Tile not found: " + tileId));

        if (!canPlaceRoad(sessionPlayerId, tileId, edgeIndex)) {
            throw new IllegalArgumentException("Cannot place road at tile=" + tileId + ", edge=" + edgeIndex);
        }

        if (!ignoreCost) {
            // 4. Check Resource Cost for Road: (1 Brick, 1 Lumber)
            if (sp.getBrick() < 1 || sp.getWood() < 1) {
                throw new IllegalArgumentException("Not enough resources to buy a road");
            }

            // 5. Deduct resources
            sp.setBrick(sp.getBrick() - 1);
            sp.setWood(sp.getWood() - 1);
        }

        sp.setRoadsPlaced(sp.getRoadsPlaced() + 1);
        sessionPlayerService.updateSessionPlayer(sp);

        if (sp.getUser() != null) {
            Optional<PlayerProfile> playerProfileByUserId = playerProfileService.getPlayerProfileByUserId(sp.getUser().getId());
            if (playerProfileByUserId.isPresent()) {
                PlayerProfile playerProfile = playerProfileByUserId.get();
                playerProfile.setRoadsPlaced(playerProfile.getRoadsPlaced() + 1);
                playerProfileService.savePlayerProfile(playerProfile);
            }
        }

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

        checkForLongestRoad(road);

        return road;
    }

    // ----------------------------------------------------
    // 3) Upgrade a Settlement to City
    // ----------------------------------------------------
    public Structure upgradeSettlementToCity(Long tileId, int cornerIndex, Long ownerId) {
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
        if (sp.getWheat() < 2 || sp.getOre() < 3) {
            throw new IllegalArgumentException("Not enough resources to upgrade to city");
        }

        // 6. Deduct city cost
        sp.setWheat(sp.getWheat() - 2);
        sp.setOre(sp.getOre() - 3);
        sp.setPlayerScore(sp.getPlayerScore() + 1);
        sessionPlayerService.updateSessionPlayer(sp);

        structure.setStructureType(structureTypeRepository.findByEnumOrCreate(StructureTypeEnum.CITY));
        return structureRepository.save(structure);
    }

    public RobberMoveResponseDto moveRobber(RobberMoveDto moveDto, SessionPlayer sessionPlayer) {
        Optional<Tile> robberTile = tileService.getRobberTile(sessionPlayer.getSession().getId());
        if (robberTile.isEmpty()) {
            throw new IllegalArgumentException("No robber found");
        }
        if (!robberTile.get().isHasRobber()
                || robberTile.get().getX() != moveDto.getOriginatingTileX()
                || robberTile.get().getY() != moveDto.getOriginatingTileY()) {
            throw new IllegalArgumentException("Invalid move (originating coordinates for robber are wrong)");
        }

        Optional<Tile> destinationTile = tileService.findByXAndYAndSession(moveDto.getDestinationTileX(), moveDto.getDestinationTileY(), sessionPlayer.getSession().getId());
        if (destinationTile.isEmpty()) {
            throw new IllegalArgumentException("No destination found");
        }
        if (destinationTile.get().isHasRobber()
                || destinationTile.get().getX() != moveDto.getDestinationTileX()
                || destinationTile.get().getY() != moveDto.getDestinationTileY()){
            throw new IllegalArgumentException("Invalid move (destination coordinates for robber are invalid)");
        }

        List<SessionPlayer> list = destinationTile.get().getTileCornerMaps().stream().map(x -> x.getCorner().getStructure())
                .filter(x -> x != null && !x.getOwner().getId().equals(sessionPlayer.getId())).map(Structure::getOwner).distinct().toList();

        RobberMoveResponseDto robberMoveResponseDto;
        if (!list.isEmpty()) {
            Random rand = new Random();
            SessionPlayer victim = list.get(rand.nextInt(list.size()));
            List<ResourceType> resourceList = victim.resourcesToGroup().resourcesToList();
            if (!resourceList.isEmpty()) {
                ResourceType resourceType = resourceList.get(rand.nextInt(resourceList.size()));
                victim.setResource(resourceType, victim.resourcesToGroup().getResourceAmount(resourceType) - 1);
                sessionPlayer.setResource(resourceType, sessionPlayer.resourcesToGroup().getResourceAmount(resourceType) - 1);

                sessionPlayerService.updateSessionPlayer(victim);
                sessionPlayerService.updateSessionPlayer(sessionPlayer);
                robberMoveResponseDto = new RobberMoveResponseDto(moveDto, victim.getName(), resourceType, sessionPlayer.getName());
            } else {
                robberMoveResponseDto = new RobberMoveResponseDto(moveDto, sessionPlayer.getName());
            }
        } else {
            robberMoveResponseDto = new RobberMoveResponseDto(moveDto, sessionPlayer.getName());
        }


        robberTile.get().setHasRobber(false);
        destinationTile.get().setHasRobber(true);
        tileService.save(robberTile.get());
        tileService.save(destinationTile.get());
        return robberMoveResponseDto;
    }

    // ----------------------------------------------------
    // Helper: canPlaceStructureWithDistanceRule
    // ----------------------------------------------------
    public boolean canPlaceStructureWithDistanceRule(Long tileId, int cornerIndex) {
        Tile tile = tileService.findById(tileId)
                .orElseThrow(() -> new IllegalArgumentException("Tile not found"));
        Optional<TileCorner> optionalTileCorner = tile.getTileCorner(cornerIndex);
        if (optionalTileCorner.isEmpty() || optionalTileCorner.get().getStructure() != null) {
            return false;
        }
        TileCorner tileCorner = optionalTileCorner.get();
        List<TileEdge> tileEdges = tileEdgeRepository.findAllConnectedToCorner(tileCorner);
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

    // ----------------------------------------------------
    // Helper: canPlaceRoad
    // ----------------------------------------------------
    public boolean canPlaceRoad(Long sessionPlayerId, Long tileId, int edgeIndex) {
        Tile tile = tileService.findById(tileId)
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

    public void checkForLongestRoad(Road road) {
        TileEdge startEdge = road.getTileEdge();
        Long ownerId = road.getOwner().getId();

        Set<Long> visited = new HashSet<>();
        visited.add(road.getId());

        int left = walkRoad(startEdge.getCornerA(), visited, startEdge, ownerId);
        int right = walkRoad(startEdge.getCornerB(), visited, startEdge, ownerId);

        Integer totalRoadLength = left + 1 + right;
        if (totalRoadLength > road.getOwner().getSession().getLongestRoadValue() && !road.getOwner().getLongestRoad()) {
            List<SessionPlayer> players = sessionPlayerService.findPlayerBySessionId(road.getOwner().getSession().getId());
            players.stream().filter(SessionPlayer::getLongestRoad).forEach(sessionPlayer -> {
                sessionPlayer.setPlayerScore(sessionPlayer.getPlayerScore() - 2);
                sessionPlayer.setLongestRoad(false);
                sessionPlayerService.updateSessionPlayer(sessionPlayer);
            });

            road.getOwner().getSession().setLongestRoadValue(totalRoadLength);
            road.getOwner().setLongestRoad(true);
            road.getOwner().setPlayerScore(road.getOwner().getPlayerScore() + 2);

            sessionPlayerService.updateSessionPlayer(road.getOwner());
            sessionService.save(road.getOwner().getSession());

            Optional<SessionCode> bySessionId = sessionCodeRepository.findBySessionId(road.getOwner().getSession().getId());
            if (bySessionId.isPresent()) {
                SessionCode sessionCode = bySessionId.get();
                notificationService.sendChatMessage(sessionCode.getCode(),
                        new ChatMessage("System", new RawChatMessage("A new longest road has been achieved by " + road.getOwner().getName() + " it is " + totalRoadLength)));
            }
        }
    }

    private int walkRoad(TileCorner currentCorner, Set<Long> visitedRoadIds, TileEdge fromEdge, Long ownerId) {
        int maxLength = 0;

        List<TileEdge> connectedEdges = tileEdgeRepository.findAllConnectedToCorner(currentCorner);

        for (TileEdge edge : connectedEdges) {
            if (edge.equals(fromEdge)) continue;

            Road road = edge.getRoad();
            if (road != null
                    && !visitedRoadIds.contains(road.getId())
                    && road.getOwner().getId().equals(ownerId)) {

                visitedRoadIds.add(road.getId());
                TileCorner nextCorner = edge.getCornerA().equals(currentCorner) ? edge.getCornerB() : edge.getCornerA();
                int pathLength = 1 + walkRoad(nextCorner, visitedRoadIds, edge, ownerId);
                maxLength = Math.max(maxLength, pathLength);
                visitedRoadIds.remove(road.getId()); // backtrack for other paths
            }
        }

        return maxLength;
    }
}
