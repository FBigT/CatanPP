package com.catan.catanbackend.model;

import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.util.Objects;

@Data
@AllArgsConstructor
@NoArgsConstructor
public class ResourceGroup implements Comparable<ResourceGroup>{
    Integer brick = 0;
    Integer crystal = 0;
    Integer ore = 0;
    Integer rice = 0;
    Integer sheep = 0;
    Integer silver = 0;
    Integer gold = 0;
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
}
