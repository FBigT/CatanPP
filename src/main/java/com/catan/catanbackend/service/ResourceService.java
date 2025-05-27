package com.catan.catanbackend.service;

import com.catan.catanbackend.model.PlayerProfile;
import com.catan.catanbackend.model.ResourceGroup;
import com.catan.catanbackend.model.SessionPlayer;
import com.catan.catanbackend.model.helper.ResourceType;
import org.springframework.stereotype.Service;

import java.util.Optional;

@Service
public class ResourceService {
    private final Mapper mapper;
    private final SessionPlayerService sessionPlayerService;
    private final PlayerProfileService playerProfileService;

    public ResourceService(Mapper mapper, SessionPlayerService sessionPlayerService, PlayerProfileService playerProfileService) {
        this.mapper = mapper;
        this.sessionPlayerService = sessionPlayerService;
        this.playerProfileService = playerProfileService;
    }

    public Boolean subtractResources(SessionPlayer player, ResourceGroup resourceGroup) {
        ResourceGroup playerResources = mapper.mapSessionPlayerToResource(player);
        if (playerResources.compareTo(resourceGroup) < 0) {
            return false;
        }
        playerResources.subtractResources(resourceGroup);
        player.setResources(playerResources);
        sessionPlayerService.updateSessionPlayer(player);
        return true;
    }

    public void addResource(ResourceType resourceType, Integer amount, SessionPlayer sessionPlayer) {
        if (sessionPlayer.getUser() != null) {
            Optional<PlayerProfile> playerProfileByUserId = playerProfileService.getPlayerProfileByUserId(sessionPlayer.getUser().getId());
            if (playerProfileByUserId.isPresent()) {
                PlayerProfile playerProfile = playerProfileByUserId.get();
                playerProfile.setResourcesGathered(playerProfile.getResourcesGathered() + amount);
                playerProfileService.savePlayerProfile(playerProfile);
            }
        }

        switch (resourceType) {
            case WOOD -> sessionPlayer.setWood(sessionPlayer.getWood() + amount);
            case BRICK -> sessionPlayer.setBrick(sessionPlayer.getBrick() + amount);
            case CRYSTAL -> sessionPlayer.setCrystal(sessionPlayer.getCrystal() + amount);
            case ORE -> sessionPlayer.setOre(sessionPlayer.getOre() + amount);
            case RICE -> sessionPlayer.setRice(sessionPlayer.getRice() + amount);
            case SHEEP -> sessionPlayer.setSheep(sessionPlayer.getSheep() + amount);
            case SILVER -> sessionPlayer.setSilver(sessionPlayer.getSilver() + amount);
            case GOLD -> sessionPlayer.setGold(sessionPlayer.getGold() + amount);
        }

        sessionPlayerService.updateSessionPlayer(sessionPlayer);
    }
}
