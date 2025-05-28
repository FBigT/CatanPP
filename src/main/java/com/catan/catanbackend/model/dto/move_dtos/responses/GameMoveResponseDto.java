package com.catan.catanbackend.model.dto.move_dtos.responses;


import com.catan.catanbackend.model.helper.GameMoveTypeEnum;
import lombok.AllArgsConstructor;
import lombok.Data;


@Data
@AllArgsConstructor
public class GameMoveResponseDto {
    private GameMoveTypeEnum gameMoveType;
    private Object moveData;
}
