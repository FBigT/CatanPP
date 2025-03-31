package com.catan.catanbackend.controller;

import com.catan.catanbackend.model.Tile;
import com.catan.catanbackend.repository.TileRepository;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

@RestController
@RequestMapping("/api/tiles")
@CrossOrigin
public class TileController {

    private final TileRepository tileRepository;

    public TileController(TileRepository tileRepository) {
        this.tileRepository = tileRepository;
    }

    @PostMapping("/create")
    public ResponseEntity<Tile> createTile(@RequestBody Tile tile) {
        return ResponseEntity.ok(tileRepository.save(tile));
    }
}

