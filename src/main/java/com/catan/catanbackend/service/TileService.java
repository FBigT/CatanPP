package com.catan.catanbackend.service;

import com.catan.catanbackend.model.tile.Tile;
import com.catan.catanbackend.repository.tiles.TileRepository;
import org.springframework.stereotype.Service;

import java.util.List;
import java.util.Optional;

@Service
public class TileService {
    private final TileRepository tileRepository;

    public TileService(TileRepository tileRepository) {
        this.tileRepository = tileRepository;
    }

    public Optional<Tile> findById(Long id) {
        return tileRepository.findById(id);
    }

    public List<Tile> findAll() {
        return tileRepository.findAll();
    }

    public Optional<Tile> findByXAndYAndSession(Integer x, Integer y, Long sessionId) {
        return tileRepository.findTileByXAndYAndSessionId(x, y, sessionId);
    }

    public List<Tile> findBySessionId(Long sessionId) {
        return tileRepository.findBySessionId(sessionId);
    }

    public Tile save(Tile tile) {
        return tileRepository.saveAndFlush(tile);
    }

    public List<Tile> saveAll(List<Tile> tiles) {
        return tileRepository.saveAll(tiles);
    }

    public Optional<Tile> getRobberTile(Long sessionId) {
        return findBySessionId(sessionId).stream().filter(Tile::isHasRobber).findFirst();
    }
}
