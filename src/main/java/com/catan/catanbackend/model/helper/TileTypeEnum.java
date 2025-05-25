package com.catan.catanbackend.model.helper;

public enum TileTypeEnum {
    DESERT(null),
    WOOD(ResourceType.WOOD),
    SAND(ResourceType.CRYSTAL),
    PASTURE(ResourceType.SHEEP),
    CLAYPIT(ResourceType.BRICK),
    MOUNTAIN(ResourceType.ORE),;

    private ResourceType resourceType;

    public ResourceType getResourceType() {
        return resourceType;
    }

    TileTypeEnum(ResourceType resourceType) {
        this.resourceType = resourceType;
    }
}
