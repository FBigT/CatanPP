package com.catan.catanbackend.model.dto.move_dtos;

import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

@Data
@AllArgsConstructor
@NoArgsConstructor
public class RobberMoveDto {
    private Integer originatingTileX;
    private Integer originatingTileY;
    private Integer destinationTileX;
    private Integer destinationTileY;
}
