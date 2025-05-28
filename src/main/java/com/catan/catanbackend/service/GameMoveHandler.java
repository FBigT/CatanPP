package com.catan.catanbackend.service;

import com.catan.catanbackend.model.DevCard;
import com.catan.catanbackend.model.dto.move_dtos.responses.TradeResponseDto;
import com.catan.catanbackend.model.helper.*;
import com.catan.catanbackend.model.Session;
import com.catan.catanbackend.model.SessionPlayer;
import com.catan.catanbackend.model.dto.*;
import com.catan.catanbackend.model.dto.move_dtos.*;
import com.catan.catanbackend.model.dto.move_dtos.responses.*;
import com.catan.catanbackend.model.tile.*;
import com.catan.catanbackend.repository.tiles.TileCornerRepository;
import com.catan.catanbackend.repository.tiles.TileEdgeRepository;
import com.fasterxml.jackson.databind.ObjectMapper;
import jakarta.validation.constraints.NotNull;
import org.springframework.messaging.simp.SimpMessagingTemplate;
import org.springframework.stereotype.Service;

import java.util.*;

import static com.catan.catanbackend.model.helper.GameMoveTypeEnum.TRADE_OFFER;
import static com.catan.catanbackend.model.helper.GameMoveTypeEnum.TRADE_RESPONSE;

@Service
public class GameMoveHandler {
    private final PlacementService placementService;
    private final ObjectMapper objectMapper;
    private final TileService tileService;
    private final TileCornerRepository tileCornerRepository;
    private final TileEdgeRepository tileEdgeRepository;
    private final MoveBlockerService moveBlockerService;
    private final DiceRollService diceRollService;
    private final DevCardService devCardService;
    private final GameService gameService;
    private final ResourceService resourceService;
    private final SessionService sessionService;
    private final Mapper mapper;
    private final TradeService tradeService;
    private final SimpMessagingTemplate messaging;

    public GameMoveHandler(PlacementService placementService, ObjectMapper objectMapper, TileService tileService, TileCornerRepository tileCornerRepository, TileEdgeRepository tileEdgeRepository, MoveBlockerService moveBlockerService, DiceRollService diceRollService, DevCardService devCardService, GameService gameService, ResourceService resourceService, SessionService sessionService, Mapper mapper, TradeService tradeService, SimpMessagingTemplate messaging ) {
        this.placementService = placementService;
        this.objectMapper = objectMapper;
        this.tileService = tileService;
        this.tileCornerRepository = tileCornerRepository;
        this.tileEdgeRepository = tileEdgeRepository;
        this.moveBlockerService = moveBlockerService;
        this.diceRollService = diceRollService;
        this.devCardService = devCardService;
        this.gameService = gameService;
        this.resourceService = resourceService;
        this.sessionService = sessionService;
        this.mapper = mapper;
        this.tradeService = tradeService;
        this.messaging    = messaging;
    }

    private record CornerPair(TileCorner a, TileCorner b) {
            private CornerPair(@NotNull TileCorner a, @NotNull TileCorner b) {
                this.a = a;
                this.b = b;
            }

            @Override
            public boolean equals(Object o) {
                if (this == o) return true;
                if (!(o instanceof CornerPair that)) return false;
                return Objects.equals(a, that.a) && Objects.equals(b, that.b);
            }

    }

    public Object handleGameMove(GameMoveTypeEnum gameMoveTypeEnum, GameMoveDto gameMoveDto, SessionPlayer sessionPlayer) {
        final Long sessionId = sessionPlayer.getSession().getId();
        Optional<Session> sessionById = sessionService.getSessionById(sessionId);
        if (sessionById.isEmpty()) {
            throw new IllegalArgumentException("Session with id " + sessionId + " not found");
        }
        Session session = sessionById.get();

        switch (gameMoveTypeEnum) {
            case MAP_GEN -> {
                if (session.getMapGenerated()) {
                    return new MapGenerationDto(tileService.findBySessionId(sessionId).stream().map(mapper::mapTileToTileDto).toList());
                } else if(!Objects.equals(session.getHost().getId(), sessionPlayer.getId())) {
                    throw new IllegalArgumentException("You cannot generate a Map");
                }

                MapGenerationDto mapGenerationDto = objectMapper.convertValue(gameMoveDto.getMoveData(), MapGenerationDto.class);


                List<Tile> list = mapGenerationDto.getTileDtos().stream().map(x -> mapper.mapTileDtoToTile(x, session)).toList();
                generateCornersAndEdges(list);

                session.setMapGenerated(true);
                sessionService.save(session);
                if(!sessionService.startSession(sessionId)){
                    throw new IllegalArgumentException("Session with id " + sessionId + " could not be started");
                }

                placeRobber(sessionId);
                return mapGenerationDto;
            }
            case BUY_CARD -> {
                checkIfSessionValid(session);
                checkIfSessionBlocked(sessionId);

                DevCard devCard = devCardService.buyDevCard(sessionPlayer.getId());
                return List.of(new PrivateBuyCardResponse(devCard.getType(), devCard.getId()), new BuyCardResponseDto(sessionPlayer.getName(), devCardService.getPlayerCards(sessionPlayer.getId()).size()));
            }
            case PLACE_ROAD -> {
                checkIfSessionValid(session);
                checkIfSessionBlocked(sessionId);
                PlaceRoadDto placeRoadDto = objectMapper.convertValue(gameMoveDto.getMoveData(), PlaceRoadDto.class);
                Optional<Tile> tile = tileService.findByXAndYAndSession(placeRoadDto.getTileX(), placeRoadDto.getTileY(), sessionPlayer.getId());

                if (tile.isEmpty()) {
                    throw new IllegalArgumentException("Tile not found");
                }

                placementService.placeRoad(sessionPlayer.getId(), tile.get().getId(), placeRoadDto.getEdgeIndex(), false);
                return new PlaceRoadResponseDto(placeRoadDto.getTileX(), placeRoadDto.getTileY(), placeRoadDto.getEdgeIndex(), sessionPlayer.getName());
            }
            case PLACE_STRUCTURE -> {
                checkIfSessionValid(session);
                checkIfSessionBlocked(sessionId);
                PlaceStructureDto placeStructureDto = objectMapper.convertValue(gameMoveDto.getMoveData(), PlaceStructureDto.class);
                Optional<Tile> tile = tileService.findByXAndYAndSession(placeStructureDto.getTileX(), placeStructureDto.getTileY(), sessionPlayer.getId());

                if (tile.isEmpty()) {
                    throw new IllegalArgumentException("Tile not found");
                }

                StructureTypeEnum structureTypeEnum = StructureTypeEnum.valueOf(placeStructureDto.getStructureType());
                placementService.placeStructure(sessionPlayer.getId(), tile.get().getId(), placeStructureDto.getCornerIndex(), structureTypeEnum);
                return new PlaceStructureResponseDto(
                        placeStructureDto.getTileX(),
                        placeStructureDto.getTileY(),
                        placeStructureDto.getCornerIndex(),
                        structureTypeEnum.toString(),
                        sessionPlayer.getName());
            }
            case UPGRADE_STRUCTURE -> {
                checkIfSessionValid(session);
                checkIfSessionBlocked(sessionId);
                UpgradeStructureDto upgradeStructureDto = objectMapper.convertValue(gameMoveDto.getMoveData(), UpgradeStructureDto.class);
                Optional<Tile> tile = tileService.findByXAndYAndSession(upgradeStructureDto.getTileX(), upgradeStructureDto.getTileY(), sessionPlayer.getId());

                if (tile.isEmpty()) {
                    throw new IllegalArgumentException("Tile not found");
                }

                placementService.upgradeSettlementToCity(tile.get().getId(), upgradeStructureDto.getCornerIndex(), sessionPlayer.getId());
                return new UpgradeStructureResponseDto(upgradeStructureDto.getTileX(), upgradeStructureDto.getTileY(), upgradeStructureDto.getCornerIndex(), sessionPlayer.getName());
            }
            case END_TURN -> {
                checkIfSessionValid(session);
                checkIfSessionBlocked(sessionId);

                //Gets the next player
                Optional<SessionPlayer> nextPlayer = sessionService.getNextPlayer(sessionId);
                String previousPlayerName = session.getCurrentPlayer().getName();

                session.setCurrentPlayer(nextPlayer.get());
                sessionService.save(session);

                //Makes cards playable
                for (SessionPlayer player : sessionService.getPlayers(sessionId)) {
                    for (DevCard playerCard : devCardService.getPlayerCards(player.getId())) {
                        if (!playerCard.isUsed() && !playerCard.isPlayable()){
                            playerCard.setPlayable(true);
                            devCardService.saveCard(playerCard);
                        }
                    }
                }

                Optional<SessionPlayer> playerAfter = sessionService.getNextPlayer(sessionId);

                return new EndTurnResponseDto(previousPlayerName, session.getCurrentPlayer().getName(), playerAfter.get().getName(), session.getTurnNumber());
            }
            case ROBBER_MOVE -> {
                checkIfSessionValid(session);
                checkIfSessionBlocked(sessionId);
                if (moveBlockerService.isSessionBlocked(sessionId) && moveBlockerService.isPlayerBlocked(sessionPlayer.getId())) {
                    RobberMoveDto robberMoveDto = objectMapper.convertValue(gameMoveDto.getMoveData(), RobberMoveDto.class);

                    placementService.moveRobber(robberMoveDto, sessionPlayer);
                    return robberMoveDto;
                } else {
                    throw new IllegalArgumentException("You cannot move the robber now");
                }
            }
            case DICE_ROLL -> {
                checkIfSessionValid(session);
                int result = diceRollService.rollDice();
                //Find tiles with matching numbers
                List<Tile> affectedTiles = tileService.findBySessionId(sessionId).stream().filter(x -> x.getNumber() == result).toList();

                for (Tile affectedTile : affectedTiles) {
                    //Find structures on the corners
                    affectedTile.getTileCornerMaps().stream().map(tileCornerMap -> tileCornerMap.getCorner().getStructure())
                            .filter(Objects::nonNull).forEach(structure -> {
                        //If city gain 2
                        if (StructureTypeEnum.CITY.equals(StructureTypeEnum.valueOf(structure.getStructureType().getName()))) {
                            resourceService.addResource(ResourceType.valueOf(affectedTile.getTileType().getResource().getName()), 2, sessionPlayer);
                        } //If settlement gain 1
                        else if (StructureTypeEnum.SETTLEMENT.equals(StructureTypeEnum.valueOf(structure.getStructureType().getName()))) {
                            resourceService.addResource(ResourceType.valueOf(affectedTile.getTileType().getResource().getName()), 1, sessionPlayer);
                        }
                    });
                }

                return new DiceResultDto(sessionPlayer.getName(), result);
            }
            case PLAY_CARD -> {
                checkIfSessionValid(session);
                checkIfSessionBlocked(sessionId);
                DevCardPlayDto devCardPlayDto = objectMapper.convertValue(gameMoveDto.getMoveData(), DevCardPlayDto.class);

                Optional<DevCard> devCard = devCardService.getDevCardById(devCardPlayDto.getId());

                if (devCard.isEmpty()) {
                    throw new IllegalArgumentException("DevCard not found");
                }

                return gameService.activateDevCard(devCard.get(), devCardPlayDto.getCardPlayData());
            }
            case VICTORY -> throw new IllegalArgumentException("You cannot choose victory");

            case TRADE_OFFER -> {
                // 1) parse the incoming offer
                TradeOfferDto offer = objectMapper.convertValue(
                        gameMoveDto.getMoveData(),
                        TradeOfferDto.class
                );

                // 2) send it privately to the target player’s queue
                messaging.convertAndSend(
                        "/user/" + offer.getToUser() + "/queue/moves",
                        new GameMoveDto(
                                GameMoveTypeEnum.TRADE_OFFER.name(),
                                objectMapper.convertValue(offer, Map.class)
                        )
                );

                // no further broadcast
                return null;
            }

            case TRADE_RESPONSE -> {
                // 1) parse the response (accept/deny)
                TradeResponseDto resp = objectMapper.convertValue(
                        gameMoveDto.getMoveData(),
                        TradeResponseDto.class
                );

                // 2) if accepted, apply the swap on the backend
                if (resp.isAccepted()) {
                    tradeService.tradeBetweenPlayers(
                            sessionId,
                            resp.getFromUser(),  // the one who clicked “accept”
                            resp.getToUser(),    // the original offerer
                            resp.getOffered(),
                            resp.getRequested()
                    );
                }

                // 3) notify the original offerer privately
                messaging.convertAndSend(
                        "/user/" + resp.getToUser() + "/queue/moves",
                        new GameMoveDto(
                                GameMoveTypeEnum.TRADE_RESPONSE.name(),
                                objectMapper.convertValue(resp, Map.class)
                        )
                );

                return null;
            }

        }


        return null;
    }

    public void checkIfSessionBlocked(Long sessionPlayerId) {
        if (moveBlockerService.isSessionBlocked(sessionPlayerId)) {
            throw new IllegalArgumentException("Cannot move robber now");
        }
    }

    public void checkIfSessionValid(Session session) {
        if (!session.getActive()) {
            throw new IllegalArgumentException("Session is not active");
        }
    }

    public void generateCornersAndEdges(List<Tile> tiles) {
        Map<CubeCoordinates, TileCorner> cornerMap = new HashMap<>();
        Map<CornerPair, TileEdge> edgeMap = new HashMap<>();

        for (Tile tile : tiles) {
            TileCorner[] tileCorners = new TileCorner[6];

            for (int i = 0; i < 6; i++) {
                CubeCoordinates coordinates = getCornerCoordinates(tile.getX(), tile.getY(), tile.getZ(), i);
                TileCorner corner = cornerMap.computeIfAbsent(coordinates, c -> {
                    TileCorner tc = new TileCorner();
                    tc.setX(c.getX());
                    tc.setY(c.getY());
                    tc.setZ(c.getZ());
                    tc.setSession(tile.getSession());
                    return tc;
                });

                TileCornerMap tcm = new TileCornerMap();
                tcm.setTile(tile);
                tcm.setCorner(corner);
                tcm.setCornerIndex(i);

                tile.getTileCornerMaps().add(tcm);
                corner.getTileCornerMaps().add(tcm);

                tileCorners[i] = corner;
            }

            for (int i = 0; i < 6; i++) {
                TileCorner cornerA = tileCorners[i];
                TileCorner cornerB = tileCorners[(i + 1) % 6];
                CornerPair key = new CornerPair(cornerA, cornerB);

                TileEdge edge = edgeMap.computeIfAbsent(key, k -> {
                    TileEdge te = new TileEdge();
                    te.setCornerA(k.a);
                    te.setCornerB(k.b);
                    te.setSession(tile.getSession());
                    return te;
                });

                TileEdgeMap tem = new TileEdgeMap();
                tem.setTile(tile);
                tem.setEdge(edge);
                tem.setEdgeIndex(i);

                tile.getTileEdgeMaps().add(tem);
                edge.getTileEdgeMaps().add(tem);
            }

        }
        tileService.saveAll(tiles);
    }

    public void placeRobber(Long sessionId) {
        if (tileService.getRobberTile(sessionId).isPresent()) {
            return;
        }

        List<Tile> tiles = tileService.findBySessionId(sessionId);
        boolean hasDeserts = tiles.stream().anyMatch(tile
                -> Objects.equals(tile.getTileType().getName(), TileTypeEnum.DESERT.name()));

        //Place on desert if exists
        if (hasDeserts) {
            tiles.stream().filter(tile
                    -> Objects.equals(tile.getTileType().getName(), TileTypeEnum.DESERT.name())).findFirst().ifPresent(desertTile -> {
                desertTile.setHasRobber(true);
                tileService.save(desertTile);
            });
        } //Place randomly if not
        else {
            Tile tile = tiles.get(new Random().nextInt(tiles.size()));
            tile.setHasRobber(true);
            tileService.save(tile);
        }
    }

    private CubeCoordinates getCornerCoordinates(int x, int y, int z, int cornerIndex) {
        int[][] offsets = {
                {1, -1, 0}, {1, 0, -1}, {0, 1, -1},
                {-1, 1, 0}, {-1, 0, 1}, {0, -1, 1}
        };
        int[] offset = offsets[cornerIndex % 6];
        return new CubeCoordinates(x + offset[0], y + offset[1], z + offset[2]);
    }
}
