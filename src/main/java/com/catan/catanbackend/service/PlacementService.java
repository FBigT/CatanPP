package com.catan.catanbackend.service;

import com.catan.catanbackend.model.Road;
import com.catan.catanbackend.model.Structure;
import com.catan.catanbackend.model.Tile;
import com.catan.catanbackend.repository.RoadRepository;
import com.catan.catanbackend.repository.StructureRepository;
import com.catan.catanbackend.repository.TileRepository;
import org.springframework.stereotype.Service;

import java.util.List;

@Service
public class PlacementService {

    private final TileRepository tileRepository;
    private final StructureRepository structureRepository;
    private final RoadRepository roadRepository;

    public PlacementService(TileRepository tileRepository, StructureRepository structureRepository, RoadRepository roadRepository) {
        this.tileRepository = tileRepository;
        this.structureRepository = structureRepository;
        this.roadRepository = roadRepository;
    }

    public Structure placeStructure(String owner, Long tileId, int cornerIndex) {
        if (!canPlaceStructureWithDistanceRule(tileId, cornerIndex)) {
            throw new IllegalArgumentException("Cannot place structure here.");
        }

        Tile tile = tileRepository.findById(tileId)
                .orElseThrow(() -> new IllegalArgumentException("Tile not found"));

        Structure structure = new Structure(owner, tile, cornerIndex);
        tile.getCorners().set(cornerIndex, true);
        tileRepository.save(tile);
        return structureRepository.save(structure);
    }

    // Method to place a road
    public Road placeRoad(String owner, Long tileId, int edgeIndex) {
        if (!canPlaceRoad(tileId, edgeIndex)) {
            throw new IllegalArgumentException("Cannot place road here.");
        }

        Tile tile = tileRepository.findById(tileId)
                .orElseThrow(() -> new IllegalArgumentException("Tile not found"));

        Road road = new Road();
        road.setOwner(owner);
        road.setTile(tile);
        road.setEdgeIndex(edgeIndex);

        tile.getEdges().set(edgeIndex, true);
        tileRepository.save(tile);
        return roadRepository.save(road);
    }

    public boolean canPlaceStructure(Long tileId, int cornerIndex) {
        Tile tile = tileRepository.findById(tileId)
                .orElseThrow(() -> new IllegalArgumentException("Tile not found"));
        return !tile.getCorners().get(cornerIndex);
    }

    public boolean canPlaceRoad(Long tileId, int edgeIndex) {
        Tile tile = tileRepository.findById(tileId)
                .orElseThrow(() -> new IllegalArgumentException("Tile not found"));

        if (tile.getEdges().get(edgeIndex)) {
            return false;
        }

        List<Road> roads = roadRepository.findAll();
        List<Structure> structures = structureRepository.findAll();

        for (Road r : roads) {
            if (r.getOwner().equals(tile.getEdges().get(edgeIndex)) &&
                    r.getTile().getId().equals(tileId) &&
                    Math.abs(r.getEdgeIndex() - edgeIndex) == 1) {
                return true;
            }
        }

        for (Structure s : structures) {
            if (s.getTile().getId().equals(tileId) &&
                    isCornerAdjacentToEdge(s.getCornerIndex(), edgeIndex)) {
                return true;
            }
        }

        return false;
    }

    public boolean canPlaceStructureWithDistanceRule(Long tileId, int cornerIndex) {
        Tile tile = tileRepository.findById(tileId)
                .orElseThrow(() -> new IllegalArgumentException("Tile not found"));

        if (tile.getCorners().get(cornerIndex)) return false;

        List<Structure> allStructures = structureRepository.findAll();

        for (Structure s : allStructures) {
            if (areCornersTooClose(tile, cornerIndex, s.getTile(), s.getCornerIndex())) {
                return false;
            }
        }

        return true;
    }

    private boolean areCornersTooClose(Tile t1, int c1, Tile t2, int c2) {
        if (t1.getId().equals(t2.getId())) {
            return Math.abs(c1 - c2) == 1 || Math.abs(c1 - c2) == 5;
        }

        return false;
    }

    private boolean isCornerAdjacentToEdge(int cornerIndex, int edgeIndex) {
        return cornerIndex == edgeIndex || cornerIndex == (edgeIndex + 1) % 6;
    }

    public Structure upgradeSettlementToCity(Long tileId, int cornerIndex, String owner) {
        Structure structure = structureRepository.findByTileIdAndCornerIndex(tileId, cornerIndex);

        if (structure == null) {
            throw new IllegalArgumentException("No structure found at given position.");
        }

        if (!structure.getOwner().equals(owner)) {
            throw new IllegalArgumentException("You can only upgrade your own settlement.");
        }

        if (!structure.getType().equals("SETTLEMENT")) {
            throw new IllegalArgumentException("Only settlements can be upgraded.");
        }

        structure.setType("CITY");
        return structureRepository.save(structure);
    }
}
