package com.catan.catanbackend.service;

import com.catan.catanbackend.model.helper.TileTypeEnum;
import com.catan.catanbackend.model.tile.TileType;
import com.catan.catanbackend.repository.tiles.ResourceRepository;
import com.catan.catanbackend.repository.tiles.TileTypeRepository;
import org.springframework.stereotype.Service;

import java.util.Optional;

@Service
public class TileTypeService {
    private final TileTypeRepository tileTypeRepository;
    private final ResourceRepository resourceRepository;

    public TileTypeService(TileTypeRepository tileTypeRepository, ResourceRepository resourceRepository) {
        this.tileTypeRepository = tileTypeRepository;
        this.resourceRepository = resourceRepository;
    }

    public TileType findByEnumOrCreate(TileTypeEnum tileType){
        Optional<TileType> byName = tileTypeRepository.findByName(tileType.name());
        if (byName.isPresent()) {
            return byName.get();
        }
        if (tileType.getResourceType() != null) {
            return tileTypeRepository.save(
                    new TileType(tileType.name(), resourceRepository.findByEnumOrCreate(tileType.getResourceType())));

        }
        return tileTypeRepository.save(new TileType(tileType.name(), null));
    }
}
