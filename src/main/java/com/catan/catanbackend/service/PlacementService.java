package com.catan.catanbackend.service;

import com.catan.catanbackend.model.Road;
import com.catan.catanbackend.model.Structure;
import com.catan.catanbackend.model.Tile;
import com.catan.catanbackend.repository.RoadRepository;
import com.catan.catanbackend.repository.StructureRepository;
import com.catan.catanbackend.repository.TileRepository;
import org.springframework.stereotype.Service;

@Service
public class PlacementService {

    private final TileRepository tileRepo;
    private final StructureRepository structureRepo;
    private final RoadRepository roadRepo;

    public PlacementService(TileRepository tileRepo,
                            StructureRepository structureRepo,
                            RoadRepository roadRepo) {
        this.tileRepo = tileRepo;
        this.structureRepo = structureRepo;
        this.roadRepo = roadRepo;
    }

    public boolean canPlaceStructure(Long tileId, int cornerIndex) {
        Tile tile = tileRepo.findById(tileId)
                .orElseThrow(() -> new RuntimeException("Tile not found"));
        return !tile.getCorners().get(cornerIndex);
    }

    public Structure placeStructure(String owner, Long tileId, int cornerIndex) {
        if (!canPlaceStructure(tileId, cornerIndex)) {
            throw new RuntimeException("Corner already occupied!");
        }

        Tile tile = tileRepo.findById(tileId)
                .orElseThrow(() -> new RuntimeException("Tile not found"));

        tile.getCorners().set(cornerIndex, true);
        tileRepo.save(tile);

        Structure structure = new Structure();
        structure.setOwner(owner);
        structure.setTile(tile);
        structure.setCornerIndex(cornerIndex);
        structure.setType("settlement");

        return structureRepo.save(structure);
    }

    public boolean canPlaceRoad(Long tileId, int edgeIndex) {
        Tile tile = tileRepo.findById(tileId)
                .orElseThrow(() -> new RuntimeException("Tile not found"));
        return !tile.getEdges().get(edgeIndex);
    }

    public Road placeRoad(String owner, Long tileId, int edgeIndex) {
        if (!canPlaceRoad(tileId, edgeIndex)) {
            throw new RuntimeException("Edge already occupied!");
        }

        Tile tile = tileRepo.findById(tileId)
                .orElseThrow(() -> new RuntimeException("Tile not found"));

        tile.getEdges().set(edgeIndex, true);
        tileRepo.save(tile);

        Road road = new Road();
        road.setOwner(owner);
        road.setTile(tile);
        road.setEdgeIndex(edgeIndex);

        return roadRepo.save(road);
    }
}
