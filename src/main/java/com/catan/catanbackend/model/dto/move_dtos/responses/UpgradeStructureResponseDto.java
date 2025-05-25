package com.catan.catanbackend.model.dto.move_dtos.responses;

import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

@Data
@AllArgsConstructor
@NoArgsConstructor
public class UpgradeStructureResponseDto {
    private Integer tileX;
    private Integer tileY;
    private Integer cornerIndex;
    private String username;
}
