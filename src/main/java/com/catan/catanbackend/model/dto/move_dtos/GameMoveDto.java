package com.catan.catanbackend.model.dto.move_dtos;

import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.util.Map;

@Data
@NoArgsConstructor
@AllArgsConstructor
public class GameMoveDto {
    private String gameMoveType;
    private Map<String, Object> moveData;
}
