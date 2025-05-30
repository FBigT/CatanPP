package com.catan.catanbackend.service;

import com.catan.catanbackend.model.*;
import com.catan.catanbackend.model.dto.*;
import com.catan.catanbackend.model.ResourceGroup;
import com.catan.catanbackend.model.dto.move_dtos.TradeOfferDto;
import com.catan.catanbackend.model.helper.TileTypeEnum;
import com.catan.catanbackend.model.tile.Tile;
import org.springframework.security.crypto.bcrypt.BCryptPasswordEncoder;
import org.springframework.security.crypto.password.PasswordEncoder;
import org.springframework.stereotype.Component;

import java.time.LocalDateTime;
import java.util.ArrayList;
import java.util.Optional;

@Component
public class Mapper {
    private final PasswordEncoder passwordEncoder;
    private final TileTypeService tileTypeService;
    private final SessionPlayerService sessionPlayerService;

    public Mapper(TileTypeService tileTypeService, SessionPlayerService sessionPlayerService) {
        this.tileTypeService = tileTypeService;
        this.sessionPlayerService = sessionPlayerService;
        this.passwordEncoder = new BCryptPasswordEncoder();
    }

    public User mapRegisterFormToUser(RegisterForm registerForm) {
        User user = new User();
        user.setActive(true);
        user.setEmail(registerForm.getEmail());
        user.setUsername(registerForm.getUsername());
        user.setCreatedAt(LocalDateTime.now());

        if (registerForm.getPassword() != null && !registerForm.getPassword().isEmpty()) {
            user.setPasswordHash(passwordEncoder.encode(registerForm.getPassword()));
            user.setIsGuest(false);
        } else {
            user.setPasswordHash(null);
            user.setIsGuest(true);
        }

        return user;
    }

    public UserDto mapUserToDto(User user) {
        return new UserDto(user.getId(), user.getUsername(), user.getEmail(), user.getActive(), user.getIsGuest(), user.getCreatedAt());
    }

    public SessionCodeDto mapSessionToDto(SessionCode sessionCode) {
        SessionCodeDto sessionCodeDto = new SessionCodeDto();
        sessionCodeDto.setId(sessionCode.getSession().getId());
        sessionCodeDto.setCode(sessionCode.getCode());
        return sessionCodeDto;
    }

    public SessionSaveSimpleDto mapSessionSaveToSaveDto(SessionSave session) {
        return new SessionSaveSimpleDto(session.getId(), session.getName(), session.getTurnNumber(), session.getSavedAt());
    }

    public ResourceGroup mapSessionPlayerToResource(SessionPlayer player) {
        return new ResourceGroup(
                player.getWood(),
                player.getSheep(),
                player.getOre(),
                player.getRice(),
                player.getBrick(),
                player.getSilver(),
                player.getGold(),
                player.getCrystal()
        );
    }

    public Tile mapTileDtoToTile(TileDto tileDto, Session session) {
        return Tile.builder()
                .x(tileDto.getX())
                .y(tileDto.getY())
                .z(tileDto.getZ())
                .hasRobber(false)
                .number(tileDto.getNumber())
                .tileType(tileTypeService.findByEnumOrCreate(TileTypeEnum.valueOf(tileDto.getTileType())))
                .session(session)
                .tileCornerMaps(new ArrayList<>())
                .tileEdgeMaps(new ArrayList<>())
                .build();
    }

    public TileDto mapTileToTileDto(Tile tile) {
        return TileDto.builder()
                .tileType(tile.getTileType().getName())
                .x(tile.getX())
                .y(tile.getY())
                .z(tile.getZ())
                .number(tile.getNumber())
                .hasRobber(tile.isHasRobber())
                .build();
    }

    public SessionSummaryDto mapSessionToSummaryDto(Session session) {
        return new SessionSummaryDto(
                session.getId(),
                session.getHost().getUsername(),
                session.getCreatedAt()
        );
    }


    public Optional<TradeOffer> mapTradeOfferDtoToTradeOffer(TradeOfferDto tradeOfferDto, Long sessionId) {
        TradeOffer tradeOffer = new TradeOffer();
        Optional<SessionPlayer> fromPlayer = sessionPlayerService.findSessionPlayerBySessionIdAndUsername(sessionId, tradeOfferDto.getFromUser());
        Optional<SessionPlayer> toPlayer = sessionPlayerService.findSessionPlayerBySessionIdAndUsername(sessionId, tradeOfferDto.getToUser());

        if (fromPlayer.isEmpty() || toPlayer.isEmpty())
            return Optional.empty();

        tradeOffer.setFromPlayer(fromPlayer.get());
        tradeOffer.setToPlayer(toPlayer.get());
        tradeOffer.setOfferResources(tradeOfferDto.getOffered());
        tradeOffer.setRequestResources(tradeOfferDto.getRequested());
        return Optional.of(tradeOffer);
    }
}
