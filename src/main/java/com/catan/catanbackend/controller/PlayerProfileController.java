package com.catan.catanbackend.controller;

import com.catan.catanbackend.model.PlayerProfile;
import com.catan.catanbackend.model.ResourceGroup;
import com.catan.catanbackend.repository.PlayerProfileRepository;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.util.List;
import java.util.Optional;

@RestController
@RequestMapping("/api/playerProfiles")
@CrossOrigin
public class PlayerProfileController {

    private final PlayerProfileRepository profileRepo;

    public PlayerProfileController(PlayerProfileRepository profileRepo) {
        this.profileRepo = profileRepo;
    }

    @GetMapping
    public List<PlayerProfile> getAllProfiles() {
        return profileRepo.findAll();
    }

    @GetMapping("/{id}")
    public ResponseEntity<PlayerProfile> getProfileById(@PathVariable Long id) {
        Optional<PlayerProfile> profile = profileRepo.findByUserId(id);
        return profile.map(ResponseEntity::ok).orElseGet(() -> ResponseEntity.notFound().build());
    }

    @PutMapping("/{id}/resources")
    public ResponseEntity<PlayerProfile> updateResources(@PathVariable Long id,
                                                         @RequestBody ResourceGroup newResources) {
        PlayerProfile profile = profileRepo.findByUserId(id)
                .orElseThrow(() -> new IllegalArgumentException("No PlayerProfile found for ID: " + id));
        profile.setResources(newResources);
        profileRepo.save(profile);
        return ResponseEntity.ok(profile);
    }
}
