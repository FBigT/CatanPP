package com.catan.catanbackend.repository.tiles;

import com.catan.catanbackend.model.helper.ResourceType;
import com.catan.catanbackend.model.tile.Resource;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Service;

import java.util.Optional;

@Service
public interface ResourceRepository extends JpaRepository<Resource, Long> {
    Optional<Resource> findByName(String name);

    default Resource findByEnumOrCreate(ResourceType resourceType){
        Optional<Resource> type = findByName(resourceType.name());
        if(type.isPresent()){
            return type.get();
        }
        return saveAndFlush(new Resource(resourceType.name()));
    }
}
