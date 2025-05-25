package com.catan.catanbackend.service;

import com.catan.catanbackend.model.ResourceGroup;
import com.catan.catanbackend.model.SessionPlayer;
import com.catan.catanbackend.model.helper.ResourceType;
import org.springframework.stereotype.Service;

@Service
public class ResourceService {
    private final Mapper mapper;
    private final SessionPlayerService sessionPlayerService;

    public ResourceService(Mapper mapper, SessionPlayerService sessionPlayerService) {
        this.mapper = mapper;
        this.sessionPlayerService = sessionPlayerService;
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
