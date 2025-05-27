package com.catan.catanbackend.model.dto.move_dtos;

import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

@Data
@AllArgsConstructor
@NoArgsConstructor
public class Place2RoadsDto {
    private PlaceRoadDto placeRoadDto1;
    private PlaceRoadDto placeRoadDto2;
}
