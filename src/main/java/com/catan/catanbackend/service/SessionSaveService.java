package com.catan.catanbackend.service;

import com.catan.catanbackend.model.*;
import com.catan.catanbackend.model.dto.SessionSummaryDto;
import com.catan.catanbackend.model.helper.StructureTypeEnum;
import com.catan.catanbackend.model.helper.TileTypeEnum;
import com.catan.catanbackend.model.tile.*;
import com.catan.catanbackend.repository.RobberBlockerRepository;
import com.catan.catanbackend.repository.RobberMoveBlockerRepository;
import com.catan.catanbackend.repository.SessionSaveRepository;
import com.catan.catanbackend.repository.tiles.*;
import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.core.type.TypeReference;
import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.ObjectMapper;
import jakarta.persistence.EntityManager;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.util.ArrayList;
import java.util.List;
import java.util.Objects;
import java.util.Optional;
import java.util.stream.Collectors;
import java.util.stream.StreamSupport;

@Service
@Transactional
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
    private final TileTypeService tileTypeService;
    private final RobberMoveBlockerRepository robberMoveBlockerRepository;
    private final RobberBlockerRepository robberBlockerRepository;
    private final EntityManager entityManager;
    private final StructureTypeService structureTypeService;


    public SessionSaveService(ObjectMapper objectMapper, SessionSaveRepository repository, SessionService sessionService, SessionPlayerService sessionPlayerService, DevCardService devCardService, TileService tileService, RoadRepository roadRepository, StructureRepository structureRepository, TileCornerRepository tileCornerRepository, TileEdgeRepository tileEdgeRepository, TileTypeService tileTypeService, RobberMoveBlockerRepository robberMoveBlockerRepository, RobberBlockerRepository robberBlockerRepository, EntityManager entityManager, StructureTypeService structureTypeService) {
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
        this.tileTypeService = tileTypeService;
        this.robberMoveBlockerRepository = robberMoveBlockerRepository;
        this.robberBlockerRepository = robberBlockerRepository;
        this.entityManager = entityManager;
        this.structureTypeService = structureTypeService;
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


    public SessionSummaryDto mapSessionToSummaryDto(Session session) {
        return new SessionSummaryDto(session.getId(), session.getHost().getUsername(), session.getCreatedAt());
    }



    public Session loadSave(String json) {
        try {
            boolean ignoreId = false;

            //Session
            JsonNode jsonNode = objectMapper.readTree(json);
            Session session = objectMapper.treeToValue(jsonNode.get("session"), Session.class);

            Optional<Session> sessionById = sessionService.getSessionById(session.getId());
            if (sessionById.isEmpty()) {
                ignoreId = true;
                session.setId(null);
            }

            JsonNode currPlayerNode = jsonNode.get("session").get("currentPlayerId");
            Long currPlayerId = (currPlayerNode != null && !currPlayerNode.isNull()) ? currPlayerNode.asLong() : null;
            SessionPlayer currPlayer = null;
            Long hostId = jsonNode.get("session").get("hostId").asLong();
            User hostRef = entityManager.getReference(User.class, hostId);
            session.setHost(hostRef);

            //Session player
            List<SessionPlayer> sessionPlayers = objectMapper.convertValue(jsonNode.get("sessionPlayers"), new TypeReference<>() {});
            for (int i = 0; i < sessionPlayers.size(); i++) {
                SessionPlayer sessionPlayer = sessionPlayers.get(i);

                if (Objects.equals(currPlayerId, sessionPlayer.getId()))
                    currPlayer = sessionPlayer;

                sessionPlayer.setSession(session);

                Long userId = jsonNode.get("sessionPlayers").get(i).get("userId").asLong();
                User userRef = entityManager.getReference(User.class, userId);
                sessionPlayer.setUser(userRef);
            }

            //Dev cards
            List<DevCard> devCards = objectMapper.convertValue(jsonNode.get("devCards"), new TypeReference<>() {});
            for (int i = 0; i < devCards.size(); i++) {
                DevCard devCard = devCards.get(i);
                if (ignoreId){
                    devCard.setId(null);
                }
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
                String text = jsonNode.get("tiles").get(i).get("tileType").asText();
                tile.setTileType(tileTypeService.findByEnumOrCreate(TileTypeEnum.valueOf(text)));
                tile.setSession(session);
                tile.setTileEdgeMaps(new ArrayList<>());
                tile.setTileCornerMaps(new ArrayList<>());
            }

            //Tile corners
            List<TileCorner> tileCorners = objectMapper.convertValue(jsonNode.get("tileCorners"), new TypeReference<>() {});
            for (int i = 0; i < tileCorners.size(); i++) {
                TileCorner tileCorner = tileCorners.get(i);

                tileCorner.setSession(session);
                tileCorner.setTileCornerMaps(new ArrayList<>());
            }

            //Tile edges
            List<TileEdge> tileEdges = objectMapper.convertValue(jsonNode.get("tileEdges"), new TypeReference<>() {});
            for (int i = 0; i < tileEdges.size(); i++) {
                TileEdge edge = tileEdges.get(i);

                edge.setSession(session);
                edge.setTileEdgeMaps(new ArrayList<>());

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
                if (ignoreId){
                    structure.setId(null);
                }
                Long ownerId = jsonNode.get("structures").get(i).get("ownerId").asLong();
                Optional<SessionPlayer> owner = sessionPlayers.stream().filter(x -> Objects.equals(x.getId(), ownerId)).findFirst();

                String text = jsonNode.get("structures").get(i).get("structureType").get("name").asText();
                structure.setStructureType(structureTypeService.findByEnumOrCreate(StructureTypeEnum.valueOf(text)));

                Long cornerId = jsonNode.get("structures").get(i).get("cornerId").asLong();
                Optional<TileCorner> first = tileCorners.stream().filter(x -> Objects.equals(x.getId(), cornerId)).findFirst();
                if (first.isPresent() && owner.isPresent()) {
                    TileCorner corner = first.get();
                    structure.setCorner(corner);
                    corner.setStructure(structure);
                    structure.setOwner(owner.get());
                }
            }

            //Roads
            List<Road> roads = objectMapper.convertValue(jsonNode.get("roads"), new TypeReference<>() {});
            for (int i = 0; i < roads.size(); i++) {
                Road road = roads.get(i);
                if (ignoreId){
                    road.setId(null);
                }
                Long ownerId = jsonNode.get("roads").get(i).get("ownerId").asLong();
                Optional<SessionPlayer> owner = sessionPlayers.stream().filter(x -> Objects.equals(x.getId(), ownerId)).findFirst();

                Long edgeId = jsonNode.get("roads").get(i).get("edgeId").asLong();
                Optional<TileEdge> tileEdge = tileEdges.stream().filter(x -> Objects.equals(x.getId(), edgeId)).findFirst();
                if (tileEdge.isPresent() && owner.isPresent()) {
                    TileEdge edge = tileEdge.get();
                    road.setTileEdge(edge);
                    edge.setRoad(road);
                    road.setOwner(owner.get());
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
                    tile.getTileEdgeMaps().add(edgeMap);
                    tileEdge.getTileEdgeMaps().add(edgeMap);
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
                    tile.getTileCornerMaps().add(cornerMap);
                    tileCorner.getTileCornerMaps().add(cornerMap);
                }
            }

            //Robber move blocker
            List<RobberMoveBlocker> robberMoveBlockers = objectMapper.convertValue(jsonNode.get("robberMoveBlockers"), new TypeReference<>() {});
            for (int i = 0; i < robberMoveBlockers.size(); i++) {
                RobberMoveBlocker robberMoveBlocker = robberMoveBlockers.get(i);
                if (ignoreId){
                    robberMoveBlocker.setId(null);
                }
                Long sessionPlayerId = jsonNode.get("robberMoveBlockers").get(i).get("sessionPlayerId").asLong();
                sessionPlayers.stream().filter(x -> Objects.equals(x.getId(), sessionPlayerId)).findFirst()
                        .ifPresent(robberMoveBlocker::setSessionPlayer);
            }

            //Robber debt blocker
            List<RobberDebtBlocker> robberDebtBlockers = objectMapper.convertValue(jsonNode.get("robberDebtBlockers"), new TypeReference<>() {});
            for (int i = 0; i < robberDebtBlockers.size(); i++) {
                RobberDebtBlocker robberDebtBlocker = robberDebtBlockers.get(i);
                if (ignoreId){
                    robberDebtBlocker.setId(null);
                }
                Long sessionPlayerId = jsonNode.get("robberDebtBlockers").get(i).get("sessionPlayerId").asLong();
                sessionPlayers.stream().filter(x -> Objects.equals(x.getId(), sessionPlayerId)).findFirst()
                        .ifPresent(robberDebtBlocker::setSessionPlayer);
            }

            sessionService.save(session);

            if (ignoreId){
                tiles.forEach(x -> x.setId(null));
                tileCornerMaps.forEach(x -> x.setId(null));
                tileEdgeMaps.forEach(x -> x.setId(null));
                tileCorners.forEach(x -> x.setId(null));
                tileEdges.forEach(x -> x.setId(null));
                roads.forEach(x -> x.setId(null));
                structures.forEach(x -> x.setId(null));
                sessionPlayers.forEach(x -> x.setId(null));
            }

            sessionPlayers.forEach(sessionPlayerService::saveSessionPlayer);
            session.setCurrentPlayer(currPlayer);
            sessionService.save(session);

            robberMoveBlockerRepository.saveAll(robberMoveBlockers);
            robberBlockerRepository.saveAll(robberDebtBlockers);
            devCards.forEach(devCardService::saveCard);

            tileService.saveAll(tiles);
            structureRepository.saveAll(structures);
            roadRepository.saveAll(roads);
            return session;
        } catch (JsonProcessingException e) {
            throw new RuntimeException(e);
        }


    }
}
