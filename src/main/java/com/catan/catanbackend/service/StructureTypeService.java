package com.catan.catanbackend.service;

import com.catan.catanbackend.model.helper.StructureTypeEnum;
import com.catan.catanbackend.model.tile.StructureType;
import com.catan.catanbackend.repository.tiles.StructureTypeRepository;
import org.springframework.stereotype.Service;

import java.util.Optional;

@Service
public class StructureTypeService {
    private final StructureTypeRepository structureTypeRepository;

    public StructureTypeService(StructureTypeRepository structureTypeRepository) {
        this.structureTypeRepository = structureTypeRepository;
    }

    public StructureType findByEnumOrCreate(StructureTypeEnum tileType){
        Optional<StructureType> byName = structureTypeRepository.findByName(tileType.name());
        return byName.orElseGet(() -> structureTypeRepository.save(new StructureType(tileType.name())));
    }
}
