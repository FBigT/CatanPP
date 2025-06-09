package com.catan.catanbackend.model;

import com.catan.catanbackend.model.helper.ResourceType;
import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.util.ArrayList;
import java.util.List;
import java.util.Objects;

@Builder
@Data
@AllArgsConstructor
@NoArgsConstructor
public class ResourceGroup implements Comparable<ResourceGroup>{
    @Builder.Default
    Integer brick = 0;
    @Builder.Default
    Integer crystal = 0;
    @Builder.Default
    Integer ore = 0;
    @Builder.Default
    Integer rice = 0;
    @Builder.Default
    Integer sheep = 0;
    @Builder.Default
    Integer silver = 0;
    @Builder.Default
    Integer gold = 0;
    @Builder.Default
    Integer wood = 0;

    public Boolean validate(){
        return brick >= 0 && crystal >= 0 && ore >= 0 && rice >= 0 && sheep >= 0 && silver >= 0 && gold >= 0 && wood >= 0;
    }

    public Integer getSum() {
        return brick + crystal + ore + rice + sheep + silver + gold + wood;
    }

    @Override
    public int compareTo(ResourceGroup o) {
        if(Objects.equals(brick, o.brick) && Objects.equals(crystal, o.crystal) && Objects.equals(ore, o.ore)
                && Objects.equals(wood, o.wood) && Objects.equals(rice, o.rice) && Objects.equals(sheep, o.sheep)
                && Objects.equals(silver, o.silver) && Objects.equals(gold, o.gold)){
            return 0;
        }
        if(brick > o.brick && crystal > o.crystal && ore > o.ore && wood > o.wood && rice > o.rice && sheep > o.sheep && silver > o.silver && gold > o.gold ){
            return 1;
        }
        return -1;
    }

    public void subtractResources(ResourceGroup resourceGroup) {
        brick -= resourceGroup.brick;
        crystal -= resourceGroup.crystal;
        ore -= resourceGroup.ore;
        rice -= resourceGroup.rice;
        sheep -= resourceGroup.sheep;
        silver -= resourceGroup.silver;
        gold -= resourceGroup.gold;
        wood -= resourceGroup.wood;
    }

    public Integer getResourceAmount(ResourceType resourceType) {
        return switch (resourceType) {
            case BRICK -> brick;
            case CRYSTAL -> crystal;
            case ORE -> ore;
            case RICE -> rice;
            case SHEEP -> sheep;
            case SILVER -> silver;
            case GOLD -> gold;
            case WOOD -> wood;
        };
    }

    public void addResource(ResourceType resourceType, int amount) {
        switch (resourceType) {
            case BRICK -> brick += amount;
            case CRYSTAL -> crystal += amount;
            case ORE -> ore += amount;
            case RICE -> rice += amount;
            case SHEEP -> sheep += amount;
            case SILVER -> silver += amount;
            case GOLD -> gold += amount;
            case WOOD -> wood += amount;
        }
    }

    public List<ResourceType> resourcesToList(){
        List<ResourceType> resources = new ArrayList<>();
        for(ResourceType resourceType : ResourceType.values()){
            Integer resourceAmount = getResourceAmount(resourceType);
            for (int i = 0; i <resourceAmount; i++){
                resources.add(resourceType);
            }
        }
        return resources;
    }
}
