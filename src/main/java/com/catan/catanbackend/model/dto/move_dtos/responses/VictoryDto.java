package com.catan.catanbackend.model.dto.move_dtos.responses;

import com.catan.catanbackend.model.dto.PlayerScoreDto;
import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.util.List;

@Data
@AllArgsConstructor
@NoArgsConstructor
public class VictoryDto {
    private List<PlayerScoreDto> players;
}
