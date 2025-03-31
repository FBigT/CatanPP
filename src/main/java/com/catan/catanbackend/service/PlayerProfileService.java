package com.catan.catanbackend.service;

import com.catan.catanbackend.model.PlayerProfile;
import com.catan.catanbackend.repository.PlayerProfileRepository;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.util.Optional;

@Service
@Transactional
public class PlayerProfileService {
    private final PlayerProfileRepository playerProfileRepository;

    public PlayerProfileService(PlayerProfileRepository playerProfileRepository) {
        this.playerProfileRepository = playerProfileRepository;
    }

    public PlayerProfile createPlayerProfile(PlayerProfile playerProfile) {
        return playerProfileRepository.save(playerProfile);
    }

    public PlayerProfile updatePlayerProfile(PlayerProfile playerProfile) {
        return playerProfileRepository.save(playerProfile);
    }

    public Boolean deletePlayerProfile(PlayerProfile playerProfile) {
        playerProfileRepository.delete(playerProfile);
        return true;
    }

    public Optional<PlayerProfile> getPlayerProfileById(Long id) {
        return playerProfileRepository.findById(id);
    }

    public Optional<PlayerProfile> getPlayerProfileByUsername(String username) {
        return playerProfileRepository.findByUserUsername(username);
    }

    public Optional<PlayerProfile> getPlayerProfileByUserId(Long id) {
        return playerProfileRepository.findByUserId(id);
    }
}
