package com.catan.catanbackend.model.dto.move_dtos.responses;

import com.catan.catanbackend.model.dto.move_dtos.RobberMoveDto;
import com.catan.catanbackend.model.helper.ResourceType;
import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

@Data
@NoArgsConstructor
@AllArgsConstructor
public class RobberMoveResponseDto {
    public RobberMoveResponseDto(RobberMoveDto moveDto, String victimName, ResourceType resourceType, String moverName) {
        resourceName = resourceType.name();
        this.moverName = moverName;
        this.victimName = victimName;
        originatingTileX = moveDto.getOriginatingTileX();
        originatingTileY = moveDto.getOriginatingTileY();
        destinationTileX = moveDto.getDestinationTileX();
        destinationTileY = moveDto.getDestinationTileY();
    }

    public RobberMoveResponseDto(RobberMoveDto moveDto, String moverName) {
        resourceName = null;
        this.moverName = moverName;
        this.victimName = null;
        originatingTileX = moveDto.getOriginatingTileX();
        originatingTileY = moveDto.getOriginatingTileY();
        destinationTileX = moveDto.getDestinationTileX();
        destinationTileY = moveDto.getDestinationTileY();
    }

    private Integer originatingTileX;
    private Integer originatingTileY;
    private Integer destinationTileX;
    private Integer destinationTileY;
    private String victimName;
    private String resourceName;
    private String moverName;
}
