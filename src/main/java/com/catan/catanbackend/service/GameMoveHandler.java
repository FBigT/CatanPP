package com.catan.catanbackend.service;

import com.catan.catanbackend.model.*;
import com.catan.catanbackend.model.dto.move_dtos.responses.TradeResponseDto;
import com.catan.catanbackend.model.helper.*;
import com.catan.catanbackend.model.dto.*;
import com.catan.catanbackend.model.dto.move_dtos.*;
import com.catan.catanbackend.model.dto.move_dtos.responses.*;
import com.catan.catanbackend.model.tile.*;
import com.catan.catanbackend.repository.RobberBlockerRepository;
import com.catan.catanbackend.repository.RobberMoveBlockerRepository;
import com.catan.catanbackend.repository.SessionCodeRepository;
import com.catan.catanbackend.repository.TradeOfferRepository;
import com.catan.catanbackend.repository.tiles.RoadRepository;
import com.catan.catanbackend.repository.tiles.StructureRepository;
import com.catan.catanbackend.repository.tiles.TileCornerRepository;
import com.catan.catanbackend.repository.tiles.TileEdgeRepository;
import com.fasterxml.jackson.databind.ObjectMapper;
import jakarta.validation.constraints.NotNull;
import org.springframework.messaging.simp.SimpMessagingTemplate;
import org.springframework.stereotype.Service;

import java.time.OffsetDateTime;
import java.util.*;
import java.util.stream.Collectors;

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
    private final RobberMoveBlockerRepository robberMoveBlockerRepository;
    private final RobberBlockerRepository roberBlockerRepository;
    private final ResourceService resourceService;
    private final SessionService sessionService;
    private final Mapper mapper;
    private final RoadRepository roadRepository;
    private final StructureRepository structureRepository;
    private final NotificationService notificationService;
    private final PlayerProfileService playerProfileService;
    private final TradeService tradeService;
    private final SimpMessagingTemplate messagingTemplate;
    private final TradeOfferRepository tradeOfferRepository;
    private final SessionCodeRepository sessionCodeRepository;


    public GameMoveHandler(PlacementService placementService, ObjectMapper objectMapper, TileService tileService, TileCornerRepository tileCornerRepository, TileEdgeRepository tileEdgeRepository, MoveBlockerService moveBlockerService, DiceRollService diceRollService, DevCardService devCardService, GameService gameService, RobberMoveBlockerRepository robberMoveBlockerRepository, RobberBlockerRepository roberBlockerRepository, ResourceService resourceService, SessionService sessionService, Mapper mapper, RoadRepository roadRepository, StructureRepository structureRepository, NotificationService notificationService, PlayerProfileService playerProfileService, TradeService tradeService, SimpMessagingTemplate messagingTemplate, TradeOfferRepository tradeOfferRepository, SessionCodeRepository sessionCodeRepository) {
        this.placementService = placementService;
        this.objectMapper = objectMapper;
        this.tileService = tileService;
        this.tileCornerRepository = tileCornerRepository;
        this.tileEdgeRepository = tileEdgeRepository;
        this.moveBlockerService = moveBlockerService;
        this.diceRollService = diceRollService;
        this.devCardService = devCardService;
        this.gameService = gameService;
        this.robberMoveBlockerRepository = robberMoveBlockerRepository;
        this.roberBlockerRepository = roberBlockerRepository;
        this.resourceService = resourceService;
        this.sessionService = sessionService;
        this.mapper = mapper;
        this.roadRepository = roadRepository;
        this.structureRepository = structureRepository;
        this.notificationService = notificationService;
        this.playerProfileService = playerProfileService;
        this.tradeService      = tradeService;
        this.messagingTemplate = messagingTemplate;
        this.tradeOfferRepository = tradeOfferRepository;
        this.sessionCodeRepository = sessionCodeRepository;

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

        //checkForSetupOrdering(gameMoveTypeEnum, session, sessionPlayer);
        //Disabled for testing
        /*if (gameMoveTypeEnum != GameMoveTypeEnum.MAP_GEN && gameMoveTypeEnum != GameMoveTypeEnum.TRADE_RESPONSE && gameMoveTypeEnum != GameMoveTypeEnum.TURN_ORDER) {
            checkIfItsTheCurrentPlayer(sessionPlayer, session);
        }*/

        final Optional<SessionCode> bySessionId = sessionCodeRepository.findBySessionId(sessionId);
        if (bySessionId.isEmpty()) {
            throw new IllegalArgumentException("Session with id " + sessionId + " not found");
        }
        switch (gameMoveTypeEnum) {
            case PRIVATE_BUY_CARD -> throw new IllegalArgumentException("Not like this");
            case MAP_GEN -> {
                System.out.println("🗺️ [GameMoveHandler] === MAP GENERATION REQUEST ===");
                System.out.println("🗺️ [GameMoveHandler] SessionPlayer: " + sessionPlayer.getName() + " (ID: " + sessionPlayer.getId() + ")");
                System.out.println("🗺️ [GameMoveHandler] Is Host: " + Objects.equals(session.getHost().getId(), sessionPlayer.getUser().getId()));
                System.out.println("🗺️ [GameMoveHandler] Map already generated: " + session.getMapGenerated());

                if (session.getMapGenerated()) {
                    System.out.println("🗺️ [GameMoveHandler] Map already generated, returning existing data");

                    try {
                        List<TileDto> existingTiles = tileService.findBySessionId(sessionId)
                                .stream()
                                .map(mapper::mapTileToTileDto)
                                .toList();
                        return new MapGenerationDto(existingTiles);
                    } catch (Exception e) {
                        System.out.println("🗺️ [GameMoveHandler] Error retrieving existing map: " + e.getMessage());
                        throw new IllegalArgumentException("Failed to retrieve existing map data");
                    }
                }

                // Check if user is host (only hosts can generate new maps)
                if (!Objects.equals(session.getHost().getId(), sessionPlayer.getUser().getId())) {
                    System.out.println("🗺️ [GameMoveHandler] ❌ Non-host attempted to generate new map");
// For debugging, let's be more informative
                    throw new IllegalArgumentException("Only the host can generate a new map. Use REQUEST_MAP to get existing map data.");
                }

                System.out.println("🗺️ [GameMoveHandler] ✅ Host generating new map...");

                try {
                    // Parse incoming map data
                    MapGenerationDto mapGenerationDto = objectMapper.convertValue(gameMoveDto.getMoveData(), MapGenerationDto.class);
                    System.out.println("🗺️ [GameMoveHandler] Received " + mapGenerationDto.getTileDtos().size() + " tiles from host");

                    // Convert to tiles and save
                    List<Tile> list = mapGenerationDto.getTileDtos().stream()
                            .map(x -> mapper.mapTileDtoToTile(x, session))
                            .toList();

                    System.out.println("🗺️ [GameMoveHandler] Converting tiles and generating corners/edges...");
                    generateCornersAndEdges(list);

                    // Update session status
                    session.setMapGenerated(true);
                    session.setInSetup(true);
                    sessionService.save(session);

                    System.out.println("🗺️ [GameMoveHandler] Session updated - map generated: " + session.getMapGenerated());

                    // Start session
                    if (!sessionService.startSession(sessionId)) {
                        throw new IllegalArgumentException("Session with id " + sessionId + " could not be started");
                    }

                    // Place robber
                    placeRobber(sessionId);

                    System.out.println("🗺️ [GameMoveHandler] ✅ Map generation complete!");
                    return mapGenerationDto;

                } catch (Exception e) {
                    System.out.println("🗺️ [GameMoveHandler] ❌ Error during map generation: " + e.getMessage());
                    e.printStackTrace();
                    throw new IllegalArgumentException("Failed to generate map: " + e.getMessage());
                }
            }

            case START_GAME -> {
                if (!sessionService.startSession(sessionId)) {
                    throw new IllegalArgumentException("Session with id " + sessionId + " could not be started");
                }

                session.setInSetup(true);
                sessionService.save(session);

                return new StartGameResponseDto(tileService.findBySessionId(sessionId).stream().map(mapper::mapTileToTileDto).toList(),
                        sessionService.getPlayersInTurnOrder(sessionId).stream().map(SessionPlayer::getName).toList());
            }

            case BUY_CARD -> {
                // checkIfSessionValid(session);  // Already commented out
                // checkIfSessionBlocked(sessionPlayer.getId());  // Comment this out too - dev cards don't need robber check

                DevCard devCard = devCardService.buyDevCard(sessionPlayer.getId());
                return List.of(new PrivateBuyCardResponse(devCard.getType(), devCard.getId()),
                        new BuyCardResponseDto(sessionPlayer.getName(), devCardService.getPlayerCards(sessionPlayer.getId()).size()));
            }
            case REQUEST_DEV_CARDS -> {
                System.out.println("🃏 [GameMoveHandler] === REQUEST DEV CARDS ===");
                System.out.println("🃏 [GameMoveHandler] SessionPlayer: " + sessionPlayer.getName() + " (ID: " + sessionPlayer.getId() + ")");

                try {
                    // Get player's dev cards
                    List<DevCard> playerCards = devCardService.getPlayerCards(sessionPlayer.getId());

                    System.out.println("🃏 [GameMoveHandler] Found " + playerCards.size() + " cards for player");
                    for (DevCard card : playerCards) {
                        System.out.println("🃏 [GameMoveHandler]   - " + card.getType() +
                                " (ID: " + card.getId() + ", playable: " + card.isPlayable() +
                                ", used: " + card.isUsed() + ")");
                    }

                    // Create response
                    DevCardsListResponseDto response = new DevCardsListResponseDto(playerCards, sessionPlayer.getName());

                    System.out.println("🃏 [GameMoveHandler] ✅ Returning dev cards list for " + sessionPlayer.getName());
                    return response;

                } catch (Exception e) {
                    System.out.println("🃏 [GameMoveHandler] ❌ Error getting dev cards: " + e.getMessage());
                    e.printStackTrace();
                    throw new IllegalArgumentException("Failed to load dev cards: " + e.getMessage());
                }
            }

            case REQUEST_MAP -> {
                System.out.println("🗺️ [GameMoveHandler] === REQUEST MAP FROM NON-HOST ===");
                System.out.println("🗺️ [GameMoveHandler] SessionPlayer: " + sessionPlayer.getName() + " (ID: " + sessionPlayer.getId() + ")");
                System.out.println("🗺️ [GameMoveHandler] Session ID: " + sessionId);
                System.out.println("🗺️ [GameMoveHandler] Map generated status: " + session.getMapGenerated());

                // Check if map is already generated
                if (session.getMapGenerated()) {
                    System.out.println("🗺️ [GameMoveHandler] ✅ Map already exists, returning existing map data");

                    try {
                        // Get existing map data
                        List<TileDto> existingTiles = tileService.findBySessionId(sessionId)
                                .stream()
                                .map(mapper::mapTileToTileDto)
                                .toList();

                        System.out.println("🗺️ [GameMoveHandler] Found " + existingTiles.size() + " tiles in session");

                        if (existingTiles.isEmpty()) {
                            System.out.println("🗺️ [GameMoveHandler] ⚠️ No tiles found despite map being marked as generated");
                            throw new IllegalArgumentException("Map data not found despite being marked as generated");
                        }

                        MapGenerationDto existingMapData = new MapGenerationDto(existingTiles);
                        System.out.println("🗺️ [GameMoveHandler] ✅ Returning " + existingTiles.size() + " tiles to requesting player: " + sessionPlayer.getName());

                        return existingMapData;

                    } catch (Exception e) {
                        System.out.println("🗺️ [GameMoveHandler] ❌ Error retrieving existing map data: " + e.getMessage());
                        e.printStackTrace();
                        throw new IllegalArgumentException("Failed to retrieve existing map data: " + e.getMessage());
                    }
                } else {
                    System.out.println("🗺️ [GameMoveHandler] ❌ Map not yet generated by host");
                    throw new IllegalArgumentException("Map has not been generated yet. Please wait for the host to generate the map.");
                }
            }

            case PLACE_ROAD -> {
                checkIfSessionValid(session);
                checkIfSessionBlocked(sessionId);
                PlaceRoadDto placeRoadDto = objectMapper.convertValue(gameMoveDto.getMoveData(), PlaceRoadDto.class);
                Optional<Tile> tile = tileService.findByXAndYAndSession(placeRoadDto.getTileX(), placeRoadDto.getTileY(), sessionId);

                if (tile.isEmpty()) {
                    throw new IllegalArgumentException("Tile not found");
                }

                placementService.placeRoad(sessionPlayer.getId(), tile.get().getId(), placeRoadDto.getEdgeIndex(), session.getInSetup());
                PlaceRoadResponseDto placeRoadResponseDto = new PlaceRoadResponseDto(placeRoadDto.getTileX(), placeRoadDto.getTileY(), placeRoadDto.getEdgeIndex(), sessionPlayer.getName());

                if (session.getInSetup()){
                    return List.of(placeRoadResponseDto, getEndTurnResponseDto(session, true, sessionPlayer));
                }

                return placeRoadResponseDto;
            }
            case PLACE_STRUCTURE -> {
                checkIfSessionValid(session);
                checkIfSessionBlocked(sessionId);
                PlaceStructureDto placeStructureDto = objectMapper.convertValue(gameMoveDto.getMoveData(), PlaceStructureDto.class);
                Optional<Tile> tile = tileService.findByXAndYAndSession(placeStructureDto.getTileX(), placeStructureDto.getTileY(), sessionId);

                if (tile.isEmpty()) {
                    throw new IllegalArgumentException("Tile not found");
                }

                StructureTypeEnum structureTypeEnum = StructureTypeEnum.valueOf(placeStructureDto.getStructureType());
                placementService.placeStructure(sessionPlayer.getId(), tile.get().getId(), placeStructureDto.getCornerIndex(), structureTypeEnum, session.getInSetup());

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
                Optional<Tile> tile = tileService.findByXAndYAndSession(upgradeStructureDto.getTileX(), upgradeStructureDto.getTileY(), session.getId());

                if (tile.isEmpty()) {
                    throw new IllegalArgumentException("Tile not found");
                }

                placementService.upgradeSettlementToCity(tile.get().getId(), upgradeStructureDto.getCornerIndex(), sessionPlayer.getId());
                return new UpgradeStructureResponseDto(upgradeStructureDto.getTileX(), upgradeStructureDto.getTileY(), upgradeStructureDto.getCornerIndex(), sessionPlayer.getName());
            }
            case END_TURN -> {
                EndTurnResponseDto endTurnResponseDto = getEndTurnResponseDto(session, false, sessionPlayer);
                notificationService.sendChatMessage( bySessionId.get().getCode(), new ChatMessage("System",  new RawChatMessage( sessionPlayer.getName()+ "ended their turn. Current turn: " + endTurnResponseDto.getTurnNumber())));
                return endTurnResponseDto;
            }
            case ROBBER_MOVE -> {
                //checkIfSessionValid(session);
                //checkIfSessionBlocked(sessionId);
                if (moveBlockerService.isSessionBlocked(sessionId) && !moveBlockerService.isPlayerBlocked(sessionPlayer.getId())) {
                    RobberMoveDto robberMoveDto = objectMapper.convertValue(gameMoveDto.getMoveData(), RobberMoveDto.class);

                    RobberMoveResponseDto robberMoveResponseDto = placementService.moveRobber(robberMoveDto, sessionPlayer);
                    if (robberMoveResponseDto.getResourceName() != null) {
                        notificationService.sendChatMessage(bySessionId.get().getCode(),
                                new ChatMessage("System",
                                        new RawChatMessage(robberMoveResponseDto.getMoverName() +
                                                " moved the robber to " + robberMoveResponseDto.getDestinationTileX() + ", " + robberMoveResponseDto.getDestinationTileY() + " and stole " +
                                                robberMoveResponseDto.getResourceName() + " from " +
                                                robberMoveResponseDto.getVictimName())));
                    } else {
                        notificationService.sendChatMessage(bySessionId.get().getCode(),
                                new ChatMessage("System",
                                        new RawChatMessage(robberMoveResponseDto.getMoverName() +
                                                " moved the robber to " + robberMoveResponseDto.getDestinationTileX() + ", " + robberMoveResponseDto.getDestinationTileY())));
                    }


                    List<RobberMoveBlocker> bySessionPlayerSessionId = robberMoveBlockerRepository.findBySessionPlayerSessionId(sessionId);
                    bySessionPlayerSessionId.stream().filter(x -> x.getSessionPlayer().getId().equals(sessionPlayer.getId())).findFirst().ifPresent(x -> {
                       robberMoveBlockerRepository.delete(x);
                       robberMoveBlockerRepository.flush();
                    });

                    return robberMoveResponseDto;
                } else {
                    throw new IllegalArgumentException("You cannot move the robber now");
                }
            }
            case PAY_DEBT -> {
                ResourceGroup resourceGroup = objectMapper.convertValue(gameMoveDto.getMoveData(), ResourceGroup.class);
                Optional<RobberDebtBlocker> debtByUserId = gameService.findDebtByUserId(sessionPlayer.getUser().getId());
                if (debtByUserId.isPresent() && gameService.settleDebtByUserId(debtByUserId.get(), sessionPlayer.getUser().getId(), resourceGroup)) {
                    return new PayDebtResponse(sessionPlayer.getName(), resourceGroup);
                }
            }
            case DICE_ROLL -> {
                //checkIfSessionValid(session);
                int result = diceRollService.rollDice();
                if (result == 7){
                    gameService.activateRobber(sessionPlayer, true);
                }

                //Find tiles with matching numbers
                List<Tile> affectedTiles = tileService.findBySessionId(sessionId).stream().filter(x -> x.getNumber() == result).toList();
                Map<SessionPlayer, ResourceGroup> gainedResourceGroups = new HashMap<>();
                for (Tile affectedTile : affectedTiles) {
                    //Find structures on the corners
                    affectedTile.getTileCornerMaps().stream().map(tileCornerMap -> tileCornerMap.getCorner().getStructure())
                            .filter(Objects::nonNull).forEach(structure -> {
                        gainedResourceGroups.computeIfAbsent(structure.getOwner(), x -> new ResourceGroup());

                        //If city gain 2
                        if (StructureTypeEnum.CITY.equals(StructureTypeEnum.valueOf(structure.getStructureType().getName()))) {
                            resourceService.addResource(ResourceType.valueOf(affectedTile.getTileType().getResource().getName()),
                                    2, structure.getOwner(), gainedResourceGroups.get(structure.getOwner()));
                        } //If settlement gain 1
                        else if (StructureTypeEnum.SETTLEMENT.equals(StructureTypeEnum.valueOf(structure.getStructureType().getName()))) {
                            resourceService.addResource(ResourceType.valueOf(affectedTile.getTileType().getResource().getName()),
                                    1, structure.getOwner(), gainedResourceGroups.get(structure.getOwner()));
                        }
                    });
                }
                Map<String, ResourceGroup> resultMap = gainedResourceGroups.entrySet().stream()
                        .collect(Collectors.toMap(
                                e -> (e.getKey()).getName(), // Extract field
                                Map.Entry::getValue
                        ));
                DiceResultDto diceResultDto = new DiceResultDto(sessionPlayer.getName(), result, resultMap);
                notificationService.sendChatMessage(bySessionId.get().getCode(),
                        new ChatMessage("System",
                                 new RawChatMessage(diceResultDto.getUsername() + " rolled a " + diceResultDto.getRollResult())));
                return diceResultDto;
            }
            case PLAY_CARD -> {
                //checkIfSessionValid(session);
                //checkIfSessionBlocked(sessionId);
                DevCardPlayDto devCardPlayDto = objectMapper.convertValue(gameMoveDto.getMoveData(), DevCardPlayDto.class);

                Optional<DevCard> devCard = devCardService.findDevCardById(devCardPlayDto.getId());

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

                Optional<TradeOffer> tradeOffer = mapper.mapTradeOfferDtoToTradeOffer(offer, sessionId);
                if (tradeOffer.isEmpty()) {
                    throw new IllegalArgumentException("TradeOffer not found");
                }

                tradeOfferRepository.save(tradeOffer.get());
                return offer;
            }

            case TRADE_RESPONSE -> {
                // 1) parse the response (accept/deny)
                TradeResponseDto resp = objectMapper.convertValue(
                        gameMoveDto.getMoveData(),
                        TradeResponseDto.class
                );

                Optional<TradeOffer> tradeOffer = tradeOfferRepository
                        .findByToPlayerNameAndFromPlayerName(resp.getFromUser(), resp.getToUser());
                if (tradeOffer.isEmpty()) {
                    throw new IllegalArgumentException("Trade offer not found");
                }
                if (tradeOffer.get().getRequestResources().compareTo(resp.getRequested()) != 0 ||
                        tradeOffer.get().getOfferResources().compareTo(resp.getOffered()) != 0) {
                    throw new IllegalArgumentException("Trade offer has different resources");
                }

                if (resp.isAccepted()) {
                    tradeService.tradeBetweenPlayers(
                            sessionId,
                            resp.getFromUser(),
                            resp.getToUser(),
                            tradeOffer.get().getRequestResources(),
                            tradeOffer.get().getOfferResources()
                    );
                } else {
                    ChatMessage deniedMsg = new ChatMessage();
                    deniedMsg.setSenderUsername(resp.getToUser());
                    deniedMsg.setText("Trade offer from " + resp.getFromUser() + " was denied.");
                    deniedMsg.setTimestamp(OffsetDateTime.now());

                    // Look up the six-character session code instead of using sessionId.toString():
                    String sessionCode = bySessionId
                            .orElseThrow(() -> new IllegalArgumentException("Session code not found"))
                            .getCode();

                    messagingTemplate.convertAndSend(
                            "/game/chat/" + sessionCode,
                            deniedMsg
                    );
                }

                tradeOfferRepository.delete(tradeOffer.get());
                tradeOfferRepository.flush();

                return resp;
            }

            case TURN_ORDER -> {
                return new TurnOrderResponseDto(sessionService.getPlayersInTurnOrder(sessionId).stream().map(SessionPlayer::getName).toList());
            }
        }
        return null;
    }

    private EndTurnResponseDto getEndTurnResponseDto(Session session, Boolean manual, SessionPlayer sessionPlayer) {
        /*?if (!manual) {
            checkForSetupOrdering(GameMoveTypeEnum.END_TURN, session, sessionPlayer);
        }*/

        Long sessionId = session.getId();
        //checkIfSessionValid(session);
        //checkIfSessionBlocked(sessionId);

        if (!session.getCurrentPlayer().getId().equals(sessionPlayer.getId())) {
            throw new IllegalArgumentException(sessionPlayer.getName() + " is not the current player");
        }

        //Gets the next player
        Optional<SessionPlayer> nextPlayer = sessionService.getNextPlayer(sessionId);
        String previousPlayerName = session.getCurrentPlayer().getName();

        session.setCurrentPlayer(nextPlayer.get());
        session.setTurnNumber(session.getTurnNumber() + 1);
        sessionService.save(session);

        //Makes cards playable
        devCardService.enablePlayable(sessionId);

        Optional<SessionPlayer> playerAfter = sessionService.getNextPlayer(sessionId);

        if (sessionPlayer.getUser() != null) {
            Optional<PlayerProfile> playerProfileByUserId = playerProfileService.getPlayerProfileByUserId(sessionPlayer.getUser().getId());
            if (playerProfileByUserId.isPresent()) {
                PlayerProfile playerProfile = playerProfileByUserId.get();
                playerProfile.setTurnsTaken(playerProfile.getTurnsTaken() + 1);
                playerProfileService.savePlayerProfile(playerProfile);
            }
        }

        return new EndTurnResponseDto(previousPlayerName, session.getCurrentPlayer().getName(), playerAfter.get().getName(), session.getTurnNumber());
    }

    private void checkForSetupOrdering(GameMoveTypeEnum gameMoveType, Session session, SessionPlayer sessionPlayer) {
        if (session.getInSetup()) {
            switch (gameMoveType) {
                case PLACE_ROAD -> {
                    if (Objects.equals(sessionPlayer.getRoadsPlaced(), sessionPlayer.getSettlementsPlaced()))
                        throw new IllegalArgumentException("You cannot place road before the settlement is placed");
                    return;
                }
                case PLACE_STRUCTURE -> {
                    if (sessionPlayer.getRoadsPlaced() < sessionPlayer.getSettlementsPlaced())
                        throw new IllegalArgumentException("Place some roads ya dingus");
                    return;
                }
            }
            throw new IllegalArgumentException("Game move type not supported during setup");
        }
    }

    private void checkIfSessionBlocked(Long sessionPlayerId) {
        System.out.println("🔧 [GameMoveHandler] checkIfSessionBlocked called with: " + sessionPlayerId);

        try {
            boolean isBlocked = moveBlockerService.isSessionBlocked(sessionPlayerId);
            System.out.println("🔧 [GameMoveHandler] Session blocked status: " + isBlocked);

            if (isBlocked) {
                System.out.println("❌ [GameMoveHandler] Session is blocked - throwing exception");
                throw new IllegalArgumentException("Cannot move robber now");
            }

            System.out.println("✅ [GameMoveHandler] Session is not blocked - continuing");
        } catch (Exception e) {
            System.out.println("❌ [GameMoveHandler] Exception in checkIfSessionBlocked: " + e.getMessage());
            e.printStackTrace();
            throw e;
        }
    }


    private void checkIfSessionValid(Session session) {
        if (!session.getActive()) {
            throw new IllegalArgumentException("Session is not active");
        }
    }

    private void checkIfItsTheCurrentPlayer(SessionPlayer sessionPlayer, Session session) {
        if (!sessionPlayer.getId().equals(session.getCurrentPlayer().getId())) {
            throw new IllegalArgumentException("You are not the current player");
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
