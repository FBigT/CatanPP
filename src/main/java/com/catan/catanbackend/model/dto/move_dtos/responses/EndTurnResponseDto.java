package com.catan.catanbackend.model.dto.move_dtos.responses;

import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

@Data
@AllArgsConstructor
@NoArgsConstructor
public class EndTurnResponseDto {
    private String previousPlayerName;
    private String currentPlayerName;
    private String nextPlayerName;
    private Integer turnNumber;
}
