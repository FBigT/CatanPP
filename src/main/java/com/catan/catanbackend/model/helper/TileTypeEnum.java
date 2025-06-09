package com.catan.catanbackend.model.helper;

public enum TileTypeEnum {
    DESERT(null),
    WOOD(ResourceType.WOOD),
    SAND(ResourceType.CRYSTAL),
    PASTURE(ResourceType.SHEEP),
    CLAYPIT(ResourceType.BRICK),
    MOUNTAIN(ResourceType.ORE),
    WHEAT(ResourceType.WHEAT);

    private ResourceType resourceType;

    TileTypeEnum(ResourceType resourceType) {
        this.resourceType = resourceType;
    }

    public ResourceType getResourceType() {
        return resourceType;
    }

    public static TileTypeEnum valueOfIgnoreCase(String name) {
        if (name == null) {
            return null;
        }
        for (TileTypeEnum tileType : TileTypeEnum.values()) {
            if (tileType.name().equalsIgnoreCase(name)) {
                return tileType;
            }
        }
        throw new IllegalArgumentException("No enum constant " +
                TileTypeEnum.class.getCanonicalName() + "." + name);
    }
}
