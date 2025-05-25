package com.catan.catanbackend.model.dto.move_dtos;

import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

@Data
@NoArgsConstructor
@AllArgsConstructor
public class UpgradeStructureDto {
    private Integer tileX;
    private Integer tileY;
    private Integer cornerIndex;
}
