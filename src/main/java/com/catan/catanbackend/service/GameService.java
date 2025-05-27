package com.catan.catanbackend.service;

import com.catan.catanbackend.model.*;
import com.catan.catanbackend.model.dto.PlayerScoreDto;
import com.catan.catanbackend.model.dto.TradeOfferMessage;
import com.catan.catanbackend.model.dto.move_dtos.Place2RoadsDto;
import com.catan.catanbackend.model.dto.move_dtos.PlaceRoadDto;
import com.catan.catanbackend.model.dto.move_dtos.responses.Place2RoadsResponseDto;
import com.catan.catanbackend.model.dto.move_dtos.responses.PlaceRoadResponseDto;
import com.catan.catanbackend.model.dto.move_dtos.RobberMoveDto;
import com.catan.catanbackend.model.dto.move_dtos.responses.PlayCardResponseDto;
import com.catan.catanbackend.model.helper.DevCardType;
import com.catan.catanbackend.model.tile.Tile;
import com.catan.catanbackend.repository.RobberBlockerRepository;
import com.catan.catanbackend.repository.RobberMoveBlockerRepository;
import com.fasterxml.jackson.databind.ObjectMapper;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.util.*;

@Service
@Transactional
public class GameService {
    static Random rand = new Random();
    static String[] names = { "Mirko", "Marko", "Mio", "Febo", "Gjuro", "Pero", "Nano", "Fico" };

    private final SessionService sessionService;
    private final SessionPlayerService sessionPlayerService;
    private final RobberBlockerRepository robberBlockerRepository;
    private final RobberMoveBlockerRepository robberMoveBlockerRepository;
    private final TileService tileService;
    private final DevCardService devCardService;
    private final PlacementService placementService;
    private final ObjectMapper objectMapper;
    private final TradeService tradeService;
    private final ResourceService resourceService;
    private final PlayerProfileService playerProfileService;

    public GameService(SessionService sessionService, RobberBlockerRepository robberBlockerRepository, SessionPlayerService sessionPlayerService, RobberMoveBlockerRepository robberMoveBlockerRepository, TileService tileService, DevCardService devCardService, PlacementService placementService, ObjectMapper objectMapper, TradeService tradeService, ResourceService resourceService, PlayerProfileService playerProfileService) {
        this.sessionService = sessionService;
        this.robberBlockerRepository = robberBlockerRepository;
        this.sessionPlayerService = sessionPlayerService;
        this.robberMoveBlockerRepository = robberMoveBlockerRepository;
        this.tileService = tileService;
        this.devCardService = devCardService;
        this.placementService = placementService;
        this.objectMapper = objectMapper;
        this.tradeService = tradeService;
        this.resourceService = resourceService;
        this.playerProfileService = playerProfileService;
    }

    public static String generateRandomName(){
        int index = rand.nextInt(names.length);
        return names[index];
    }

    public Boolean activateRobber(SessionPlayer sessionPlayer, Boolean extort) {
        List<SessionPlayer> players = sessionService.getPlayers(sessionPlayer.getSession().getId());
        if (!players.contains(sessionPlayer)) {
            return false;
        }
        if (extort) {
            for (SessionPlayer player : players) {
                if (player.getNumberOfResources() > 7){
                    int amount = (int) (player.getNumberOfResources() / 2.0);
                    robberBlockerRepository.saveAndFlush(new RobberDebtBlocker(player, amount));
                }
            }
        }

        Optional<Tile> robberTile = tileService.getRobberTile(sessionPlayer.getSession().getId());
        if (robberTile.isEmpty())
            return false;
        robberMoveBlockerRepository.saveAndFlush(
                new RobberMoveBlocker(sessionPlayer, robberTile.get().getX(), robberTile.get().getY()));
        return true;
    }

    public Optional<RobberDebtBlocker> findDebtByUserId(Long userId) {
        Optional<SessionPlayer> player = sessionPlayerService.findCurrentSessionPlayerByUserId(userId);
        if (player.isPresent()) {
            Optional<RobberDebtBlocker> debt = robberBlockerRepository.findBySessionPlayerId(player.get().getId());
            if (debt.isPresent()) {
                return debt;
            }
        }
        return Optional.empty();
    }

    public Boolean settleDebtByUserId(RobberDebtBlocker debt, Long userId, ResourceGroup resourceGroup) {
        Optional<SessionPlayer> player = sessionPlayerService.findCurrentSessionPlayerByUserId(userId);
        if (player.isPresent() && resourceService.subtractResources(player.get(), resourceGroup)) {
            robberBlockerRepository.delete(debt);
            return true;
        }
        return false;
    }

    public Object activateDevCard(DevCard devCard, Map<String, Object> playData) {
        devCardService.useCard(devCard.getId(), devCard.getOwner().getId());

        switch (devCard.getType()) {
            case KNIGHT -> {
                Optional<Tile> robberTile = tileService.getRobberTile(devCard.getOwner().getSession().getId());
                if (robberTile.isEmpty()) {
                    throw new IllegalArgumentException("No robber found");
                }
                RobberMoveDto robberMoveDto = objectMapper.convertValue(playData, RobberMoveDto.class);

                placementService.moveRobber(robberMoveDto, devCard.getOwner());

                return new PlayCardResponseDto(DevCardType.KNIGHT.name(), objectMapper.convertValue(robberMoveDto, Map.class));
            }
            case VICTORY_POINT -> {
                devCard.getOwner().setPlayerScore(devCard.getOwner().getPlayerScore() + 1);

                sessionPlayerService.updateSessionPlayer(devCard.getOwner());
                return new PlayCardResponseDto(DevCardType.VICTORY_POINT.name(), objectMapper.convertValue(
                        new PlayerScoreDto(devCard.getOwner().getName(), devCard.getOwner().getPlayerScore()), Map.class));
            }
            case ROAD_BUILDING -> {
                Place2RoadsDto place2RoadsDto = objectMapper.convertValue(playData, Place2RoadsDto.class);
                PlaceRoadDto[] placeRoadDtos = new PlaceRoadDto[]{place2RoadsDto.getPlaceRoadDto1(), place2RoadsDto.getPlaceRoadDto2()};
                if (placeRoadDtos.length != 2) {
                    throw new IllegalArgumentException("Incorrect number of roads");
                }

                Long playerId = devCard.getOwner().getId();

                List<Tile> tiles = new ArrayList<>(2);
                for (PlaceRoadDto dto : placeRoadDtos) {
                    Tile tile = tileService.findByXAndYAndSession(dto.getTileX(), dto.getTileY(), playerId)
                            .orElseThrow(() -> new IllegalArgumentException("Tile not found"));
                    tiles.add(tile);
                }
                for (int i = 0; i < 2; i++) {
                    placementService.placeRoad(playerId, tiles.get(i).getId(), placeRoadDtos[i].getEdgeIndex(), false);
                }

                return new PlayCardResponseDto(DevCardType.ROAD_BUILDING.name(), objectMapper.convertValue(
                        new Place2RoadsResponseDto(
                        new PlaceRoadResponseDto(placeRoadDtos[0].getTileX(), placeRoadDtos[0].getTileY(), placeRoadDtos[0].getEdgeIndex(), devCard.getOwner().getName()),
                        new PlaceRoadResponseDto(placeRoadDtos[1].getTileX(), placeRoadDtos[1].getTileY(), placeRoadDtos[1].getEdgeIndex(), devCard.getOwner().getName())
                        ), Map.class));
            }
            case YEAR_OF_PLENTY -> {
                ResourceGroup resourceGroup = objectMapper.convertValue(playData, ResourceGroup.class);
                if (resourceGroup.getSum() != 2)
                    throw new IllegalArgumentException("Incorrect number of resources");
                tradeService.tradeWithBankDirect(devCard.getOwner().getSession().getId(), devCard.getOwner().getUser().getUsername(), new ResourceGroup(), resourceGroup);

                return new PlayCardResponseDto(DevCardType.YEAR_OF_PLENTY.name(), objectMapper.convertValue(
                        new TradeOfferMessage(devCard.getOwner().getName(), "Bank", new ResourceGroup(), resourceGroup), Map.class));
            }
        }
        throw new IllegalArgumentException("Incorrect card type");
    }

    public Optional<SessionPlayer> checkForWinner(Long sessionId) {
        Optional<Session> sessionById = sessionService.getSessionById(sessionId);
        List<SessionPlayer> players = sessionService.getPlayers(sessionId);

        Optional<SessionPlayer> player1 = sessionById.flatMap(session -> players.stream().filter(player
                -> player.getPlayerScore() >= session.getVictoryPointsCondition()).findFirst());
        if (player1.isPresent()) {
            for (SessionPlayer player : players.stream().filter(x -> x.getUser() != null).toList()) {
                Optional<PlayerProfile> playerProfileByUserId = playerProfileService.getPlayerProfileByUserId(player.getUser().getId());
                if (playerProfileByUserId.isPresent()) {
                    PlayerProfile playerProfile = playerProfileByUserId.get();

                    playerProfile.setGamesPlayed(playerProfile.getGamesPlayed() + 1);
                    if (player.equals(player1.get()))
                        playerProfile.setGamesWon(playerProfile.getGamesWon() + 1);
                    else
                        playerProfile.setGamesLost(playerProfile.getGamesLost() + 1);
                    playerProfileService.savePlayerProfile(playerProfile);
                }
            }

        }
        return player1;
    }
}
