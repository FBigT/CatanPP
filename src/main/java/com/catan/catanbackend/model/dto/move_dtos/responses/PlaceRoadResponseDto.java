package com.catan.catanbackend.model.dto.move_dtos.responses;

import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

@Data
@NoArgsConstructor
@AllArgsConstructor
public class PlaceRoadResponseDto {
    private Integer tileX;
    private Integer tileY;
    private Integer edgeIndex;
    private String username;
}
