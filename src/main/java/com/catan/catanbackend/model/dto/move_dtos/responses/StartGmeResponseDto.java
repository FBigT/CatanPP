package com.catan.catanbackend.model.dto.move_dtos.responses;

import com.catan.catanbackend.model.dto.TileDto;
import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.util.List;

@Data
@NoArgsConstructor
@AllArgsConstructor
public class StartGmeResponseDto {
    List<TileDto> tiles;
    List<String> turnOrder;
}
