package com.catan.catanbackend.model.dto.move_dtos.responses;

import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

@Data
@NoArgsConstructor
@AllArgsConstructor
public class Place2RoadsResponseDto {
    private PlaceRoadResponseDto placeRoadResponseDto1;
    private PlaceRoadResponseDto placeRoadResponseDto2;
}
