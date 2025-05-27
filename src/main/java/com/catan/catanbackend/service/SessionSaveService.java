package com.catan.catanbackend.service;

import com.catan.catanbackend.model.*;
import com.catan.catanbackend.model.tile.*;
import com.catan.catanbackend.repository.RobberBlockerRepository;
import com.catan.catanbackend.repository.RobberMoveBlockerRepository;
import com.catan.catanbackend.repository.SessionSaveRepository;
import com.catan.catanbackend.repository.tiles.RoadRepository;
import com.catan.catanbackend.repository.tiles.StructureRepository;
import com.catan.catanbackend.repository.tiles.TileCornerRepository;
import com.catan.catanbackend.repository.tiles.TileEdgeRepository;
import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.core.type.TypeReference;
import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.ObjectMapper;
import jakarta.persistence.EntityManager;
import org.springframework.stereotype.Service;

import java.util.List;
import java.util.Objects;
import java.util.Optional;
import java.util.stream.Collectors;
import java.util.stream.StreamSupport;

@Service
public class SessionSaveService {
    private final ObjectMapper objectMapper;
    private final SessionSaveRepository repository;
    private final SessionService sessionService;
    private final SessionPlayerService sessionPlayerService;
    private final DevCardService devCardService;
    private final TileService tileService;
    private final RoadRepository roadRepository;
    private final StructureRepository structureRepository;
    private final TileCornerRepository tileCornerRepository;
    private final TileEdgeRepository tileEdgeRepository;
    private final RobberMoveBlockerRepository robberMoveBlockerRepository;
    private final RobberBlockerRepository robberBlockerRepository;
    private final EntityManager entityManager;


    public SessionSaveService(ObjectMapper objectMapper, SessionSaveRepository repository, SessionService sessionService, SessionPlayerService sessionPlayerService, DevCardService devCardService, TileService tileService, RoadRepository roadRepository, StructureRepository structureRepository, TileCornerRepository tileCornerRepository, TileEdgeRepository tileEdgeRepository, RobberMoveBlockerRepository robberMoveBlockerRepository, RobberBlockerRepository robberBlockerRepository, EntityManager entityManager) {
        this.objectMapper = objectMapper;
        this.repository = repository;
        this.sessionService = sessionService;
        this.sessionPlayerService = sessionPlayerService;
        this.devCardService = devCardService;
        this.tileService = tileService;
        this.roadRepository = roadRepository;
        this.structureRepository = structureRepository;
        this.tileCornerRepository = tileCornerRepository;
        this.tileEdgeRepository = tileEdgeRepository;
        this.robberMoveBlockerRepository = robberMoveBlockerRepository;
        this.robberBlockerRepository = robberBlockerRepository;
        this.entityManager = entityManager;
    }

    public SessionSave save(String saveName, Session session) {
        return repository.saveAndFlush(new SessionSave(saveName, session, session.getTurnNumber(), createSaveJson(session.getId())));
    }

    public Optional<SessionSave> findById(Long id){
        return repository.findById(id);
    }

    public void deleteSave(Long saveId) {
        repository.deleteById(saveId);
        repository.flush();
    }

    public List<SessionSave> getSavesByHostId(Long hostId) {
        return repository.findBySessionHostId(hostId);
    }

    public String createSaveJson(Long sessionId) {
        Optional<Session> sessionById = sessionService.getSessionById(sessionId);
        if (sessionById.isEmpty())
            throw new IllegalArgumentException("No such session");
        Session session = sessionById.get();
        List<Tile> tiles = tileService.findBySessionId(session.getId());

        SessionSaveJsonHolder build = SessionSaveJsonHolder.builder()
                .session(session)
                .sessionPlayers(sessionPlayerService.findPlayerBySessionId(session.getId()))
                .devCards(devCardService.getAllDevCardsBySessionId(session.getId()))
                .tiles(tiles)
                .tileEdges(tileEdgeRepository.findBySessionId(session.getId()))
                .tileCorners(tileCornerRepository.findBySessionId(session.getId()))
                .tileCornerMaps(tiles.stream()
                        .flatMap(tile -> tile.getTileCornerMaps().stream())
                        .toList())
                .tileEdgeMaps(tiles.stream()
                        .flatMap(tile -> tile.getTileEdgeMaps().stream())
                        .toList())
                .robberMoveBlockers(robberMoveBlockerRepository.findBySessionPlayerSessionId(session.getId()))
                .robberDebtBlockers(robberBlockerRepository.findBySessionPlayerSessionId(session.getId()))
                .roads(roadRepository.findByOwnerSessionId(session.getId()))
                .structures(structureRepository.findByOwnerSessionId(session.getId()))
                .build();
        try {
            return objectMapper.writeValueAsString(build);
        } catch (JsonProcessingException e) {
            throw new RuntimeException(e);
        }
    }

    public void loadSave(String json) {
        try {
            //Session
            JsonNode jsonNode = objectMapper.readTree(json);
            Session session = objectMapper.treeToValue(jsonNode.get("session"), Session.class);

            JsonNode currPlayerNode = jsonNode.get("session").get("currentPlayerId");
            Long currPlayerId = (currPlayerNode != null && !currPlayerNode.isNull()) ? currPlayerNode.asLong() : null;

            Long hostId = jsonNode.get("session").get("hostId").asLong();
            User hostRef = entityManager.getReference(User.class, hostId);
            session.setHost(hostRef);

            //Session player
            List<SessionPlayer> sessionPlayers = objectMapper.convertValue(jsonNode.get("sessionPlayers"), new TypeReference<>() {});

            for (int i = 0; i < sessionPlayers.size(); i++) {
                SessionPlayer sessionPlayer = sessionPlayers.get(i);
                if (Objects.equals(currPlayerId, sessionPlayer.getId()))
                    session.setCurrentPlayer(sessionPlayer);

                sessionPlayer.setSession(session);

                Long userId = jsonNode.get("sessionPlayers").get(i).get("userId").asLong();
                User userRef = entityManager.getReference(User.class, userId);
                sessionPlayer.setUser(userRef);
            }

            //Dev cards
            List<DevCard> devCards = objectMapper.convertValue(jsonNode.get("devCards"), new TypeReference<>() {});
            for (int i = 0; i < devCards.size(); i++) {
                DevCard devCard = devCards.get(i);
                devCard.setSession(session);

                JsonNode userIdNode = jsonNode.get("devCards").get(i).get("ownerId");
                Long ownerId = (userIdNode != null && !userIdNode.isNull()) ? userIdNode.asLong() : null;
                if (ownerId != null) {
                    Optional<SessionPlayer> first = sessionPlayers.stream().filter(x -> x.getId().equals(ownerId)).findFirst();
                    first.ifPresent(devCard::setOwner);
                }
            }

            //Tiles
            List<Tile> tiles = objectMapper.convertValue(jsonNode.get("tiles"), new TypeReference<>() {});
            for (int i = 0; i < tiles.size(); i++) {
                Tile tile = tiles.get(i);
                tile.setSession(session);
            }

            //Tile corners
            List<TileCorner> tileCorners = objectMapper.convertValue(jsonNode.get("tileCorners"), new TypeReference<>() {});
            for (int i = 0; i < tileCorners.size(); i++) {
                TileCorner tile = tileCorners.get(i);
                tile.setSession(session);
            }

            //Tile edges
            List<TileEdge> tileEdges = objectMapper.convertValue(jsonNode.get("tileEdges"), new TypeReference<>() {});
            for (int i = 0; i < tileEdges.size(); i++) {
                TileEdge edge = tileEdges.get(i);
                edge.setSession(session);

                Long cornerAId = jsonNode.get("tileEdges").get(i).get("cornerAId").asLong();
                Long cornerBId = jsonNode.get("tileEdges").get(i).get("cornerBId").asLong();

                Optional<TileCorner> firstA = tileCorners.stream().filter(x -> Objects.equals(x.getId(), cornerAId)).findFirst();
                Optional<TileCorner> firstB = tileCorners.stream().filter(x -> Objects.equals(x.getId(), cornerBId)).findFirst();
                if (firstA.isPresent() && firstB.isPresent()) {
                    TileCorner cornerA = firstA.get();
                    TileCorner cornerB = firstB.get();
                    edge.setCornerA(cornerA);
                    edge.setCornerB(cornerB);
                }
            }

            //Structures
            List<Structure> structures = objectMapper.convertValue(jsonNode.get("structures"), new TypeReference<>() {});
            for (int i = 0; i < structures.size(); i++) {
                Structure structure = structures.get(i);

                Long ownerId = jsonNode.get("structures").get(i).get("ownerId").asLong();
                structure.setOwner(entityManager.getReference(SessionPlayer.class, ownerId));

                Long cornerId = jsonNode.get("structures").get(i).get("cornerId").asLong();
                Optional<TileCorner> first = tileCorners.stream().filter(x -> Objects.equals(x.getId(), cornerId)).findFirst();
                if (first.isPresent()) {
                    TileCorner corner = first.get();
                    structure.setCorner(corner);
                    corner.setStructure(structure);
                }
            }

            //Roads
            List<Road> roads = objectMapper.convertValue(jsonNode.get("roads"), new TypeReference<>() {});
            for (int i = 0; i < roads.size(); i++) {
                Road road = roads.get(i);

                Long ownerId = jsonNode.get("roads").get(i).get("ownerId").asLong();
                road.setOwner(entityManager.getReference(SessionPlayer.class, ownerId));

                Long edgeId = jsonNode.get("roads").get(i).get("edgeId").asLong();
                Optional<TileEdge> first = tileEdges.stream().filter(x -> Objects.equals(x.getId(), edgeId)).findFirst();
                if (first.isPresent()) {
                    TileEdge edge = first.get();
                    road.setTileEdge(edge);
                    edge.setRoad(road);
                }
            }

            //Tile edge maps
            List<TileEdgeMap> tileEdgeMaps = objectMapper.convertValue(jsonNode.get("tileEdgeMaps"), new TypeReference<>() {});
            for (int i = 0; i < tileEdgeMaps.size(); i++) {
                TileEdgeMap edgeMap = tileEdgeMaps.get(i);

                Long edgeId = jsonNode.get("tileEdgeMaps").get(i).get("edgeId").asLong();
                Long tileId = jsonNode.get("tileEdgeMaps").get(i).get("tileId").asLong();

                Optional<Tile> tileOptional = tiles.stream().filter(x -> Objects.equals(x.getId(), tileId)).findFirst();
                Optional<TileEdge> tileEdgeOptional = tileEdges.stream().filter(x -> Objects.equals(x.getId(), edgeId)).findFirst();
                if (tileEdgeOptional.isPresent() && tileOptional.isPresent()) {
                    Tile tile = tileOptional.get();
                    TileEdge tileEdge = tileEdgeOptional.get();
                    edgeMap.setTile(tile);
                    edgeMap.setEdge(tileEdge);
                }
            }

            //Tile corner maps
            List<TileCornerMap> tileCornerMaps = objectMapper.convertValue(jsonNode.get("tileCornerMaps"), new TypeReference<>() {});
            for (int i = 0; i < tileCornerMaps.size(); i++) {
                TileCornerMap cornerMap = tileCornerMaps.get(i);

                Long cornerId = jsonNode.get("tileCornerMaps").get(i).get("cornerId").asLong();
                Long tileId = jsonNode.get("tileCornerMaps").get(i).get("tileId").asLong();

                Optional<Tile> tileOptional = tiles.stream().filter(x -> Objects.equals(x.getId(), tileId)).findFirst();
                Optional<TileCorner> tileCornerOptional = tileCorners.stream().filter(x -> Objects.equals(x.getId(), cornerId)).findFirst();
                if (tileCornerOptional.isPresent() && tileOptional.isPresent()) {
                    Tile tile = tileOptional.get();
                    TileCorner tileCorner = tileCornerOptional.get();
                    cornerMap.setTile(tile);
                    cornerMap.setCorner(tileCorner);
                }
            }

            //Robber move blocker
            List<RobberMoveBlocker> robberMoveBlockers = objectMapper.convertValue(jsonNode.get("robberMoveBlockers"), new TypeReference<>() {});
            for (int i = 0; i < robberMoveBlockers.size(); i++) {
                RobberMoveBlocker robberMoveBlocker = robberMoveBlockers.get(i);

                Long sessionPlayerId = jsonNode.get("robberMoveBlockers").get(i).get("sessionPlayerId").asLong();
                sessionPlayers.stream().filter(x -> Objects.equals(x.getId(), sessionPlayerId)).findFirst()
                        .ifPresent(robberMoveBlocker::setSessionPlayer);
            }

            //Robber debt blocker
            List<RobberDebtBlocker> robberDebtBlockers = objectMapper.convertValue(jsonNode.get("robberDebtBlockers"), new TypeReference<>() {});
            for (int i = 0; i < robberDebtBlockers.size(); i++) {
                RobberDebtBlocker robberDebtBlocker = robberDebtBlockers.get(i);

                Long sessionPlayerId = jsonNode.get("robberDebtBlockers").get(i).get("sessionPlayerId").asLong();
                sessionPlayers.stream().filter(x -> Objects.equals(x.getId(), sessionPlayerId)).findFirst()
                        .ifPresent(robberDebtBlocker::setSessionPlayer);
            }

            System.out.println("");
        } catch (JsonProcessingException e) {
            throw new RuntimeException(e);
        }
    }
}
