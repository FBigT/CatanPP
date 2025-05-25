package com.catan.catanbackend.model.dto;

import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Data;
import lombok.NoArgsConstructor;

@Data
@Builder
@AllArgsConstructor
@NoArgsConstructor
public class TileDto {
    private Integer x;
    private Integer y;
    private Integer z;
    private String tileType;
    private Integer number;
    private Boolean hasRobber;
}
