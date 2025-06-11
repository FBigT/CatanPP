package com.catan.catanbackend.service;

import com.catan.catanbackend.model.*;
import com.catan.catanbackend.model.dto.*;
import com.catan.catanbackend.model.ResourceGroup;
import com.catan.catanbackend.model.dto.move_dtos.TradeOfferDto;
import com.catan.catanbackend.model.helper.TileTypeEnum;
import com.catan.catanbackend.model.tile.Tile;
import com.fasterxml.jackson.databind.ObjectMapper;
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
    private final ObjectMapper objectMapper;
    private final EncryptionUtils encryptionUtils;

    public Mapper(TileTypeService tileTypeService, SessionPlayerService sessionPlayerService, ObjectMapper objectMapper, EncryptionUtils encryptionUtils) {
        this.tileTypeService = tileTypeService;
        this.sessionPlayerService = sessionPlayerService;
        this.objectMapper = objectMapper;
        this.encryptionUtils = encryptionUtils;
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
        ResourceGroup resourceGroup = new ResourceGroup();

        resourceGroup.setBrick(player.getBrick());
        resourceGroup.setCrystal(player.getCrystal());
        resourceGroup.setOre(player.getOre());
        resourceGroup.setRice(player.getRice());
        resourceGroup.setSheep(player.getSheep());
        resourceGroup.setSilver(player.getSilver());
        resourceGroup.setGold(player.getGold());
        resourceGroup.setWood(player.getWood());
        resourceGroup.setWheat(player.getWheat());

        return resourceGroup;
    }

    public Tile mapTileDtoToTile(TileDto tileDto, Session session) {
        return Tile.builder()
                .x(tileDto.getX())
                .y(tileDto.getY())
                .z(tileDto.getZ())
                .hasRobber(false)
                .number(tileDto.getNumber())
                .tileType(tileTypeService.findByEnumOrCreate(TileTypeEnum.valueOfIgnoreCase(tileDto.getTileType())))
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

    public EncryptedResponse mapToEncryptedResponse(Object object, byte[] aesKey) {
        try {
            String json = objectMapper.writeValueAsString(object);
            String encryptedPayload = encryptionUtils.encryptResponse(json, aesKey);
            return new EncryptedResponse(encryptedPayload);
        } catch (Exception e) {
            throw new IllegalStateException("Failed to encrypt response", e);
        }
    }

    public EncryptedMessageWithKey mapToEncryptedMessage(Object object) {
        try {
            String json = objectMapper.writeValueAsString(object);
            return encryptionUtils.simulateFrontendEncryption(json);
        } catch (Exception e) {
            throw new IllegalStateException("Failed to encrypt response", e);
        }
    }

    public Object mapFromEncryptedResponse(EncryptedResponse encryptedResponse, byte[] aesKey, Class<?> clazz) {
        try {
            String s = encryptionUtils.decryptPayload(encryptedResponse.getPayload(), aesKey);
            return objectMapper.readValue(s, clazz);
        } catch (Exception e) {
            throw new RuntimeException(e);
        }
    }

    public <T> DecryptedMessage mapToObject(EncryptedMessage encryptedMessage, Class<T> clazz) {
        try {
            byte[] aesKey = encryptionUtils.decryptAESKey(encryptedMessage.getEncryptedKey());

            String decryptedJson = encryptionUtils.decryptPayload(encryptedMessage.getPayload(), aesKey);
            T payload = objectMapper.readValue(decryptedJson, clazz);
            return new DecryptedMessage(payload, aesKey);
        } catch (Exception e) {
            throw new IllegalArgumentException("Failed to decrypt message", e);
        }
    }

    public TileDto mapTileToDto(Tile tile) {
        return TileDto.builder()
                .x(tile.getX())
                .y(tile.getY())
                .z(tile.getZ())
                .tileType(tile.getTileTypeName())
                .number(tile.getNumber())
                .hasRobber(tile.isHasRobber())
                .build();
    }

}
