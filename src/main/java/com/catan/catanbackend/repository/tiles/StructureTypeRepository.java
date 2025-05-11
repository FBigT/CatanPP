package com.catan.catanbackend.repository.tiles;

import com.catan.catanbackend.model.StructureTypeEnum;
import com.catan.catanbackend.model.tile.StructureType;
import org.springframework.data.jpa.repository.JpaRepository;

import java.util.Optional;

public interface StructureTypeRepository extends JpaRepository<StructureType, Long> {
    Optional<StructureType> findByName(String name);

    default StructureType findByEnumOrCreate(StructureTypeEnum structureTypeEnum){
        Optional<StructureType> type = findByName(structureTypeEnum.name());
        return type.orElseGet(() -> saveAndFlush(new StructureType(structureTypeEnum.name())));
    }
}
