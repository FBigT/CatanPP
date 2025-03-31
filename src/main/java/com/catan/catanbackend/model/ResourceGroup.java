package com.catan.catanbackend.model;

import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.util.Objects;

@Data
@AllArgsConstructor
@NoArgsConstructor
public class ResourceGroup implements Comparable<ResourceGroup>{
    Integer lumber = 0;
    Integer wool = 0;
    Integer ore = 0;
    Integer grain = 0;
    Integer bricks = 0;
    Integer silver = 0;
    Integer gold = 0;
    Integer obsidian = 0;

    public Boolean validate(){
        return lumber >= 0 && wool >= 0 && ore >= 0 && grain >= 0 && bricks >= 0 && silver >= 0 && gold >= 0 && obsidian >= 0;
    }

    public Integer getSum() {
        return lumber + wool + ore + grain + bricks + silver + gold + obsidian;
    }

    @Override
    public int compareTo(ResourceGroup o) {
        if(Objects.equals(lumber, o.lumber) && Objects.equals(wool, o.wool) && Objects.equals(ore, o.ore)
                && Objects.equals(obsidian, o.obsidian) && Objects.equals(grain, o.grain) && Objects.equals(bricks, o.bricks)
                && Objects.equals(silver, o.silver) && Objects.equals(gold, o.gold)){
            return 0;
        }
        if(lumber > o.lumber && wool > o.wool && ore > o.ore && obsidian > o.obsidian && grain > o.grain && bricks > o.bricks && silver > o.silver && gold > o.gold ){
            return 1;
        }
        return -1;
    }

    public void subtractResources(ResourceGroup resourceGroup) {
        lumber -= resourceGroup.lumber;
        wool -= resourceGroup.wool;
        ore -= resourceGroup.ore;
        grain -= resourceGroup.grain;
        bricks -= resourceGroup.bricks;
        silver -= resourceGroup.silver;
        gold -= resourceGroup.gold;
        obsidian -= resourceGroup.obsidian;
    }
}
