package com.catan.catanbackend.model.dto.move_dtos.responses;

import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.util.Map;

@Data
@AllArgsConstructor
@NoArgsConstructor
public class PlayCardResponseDto {
    private String devCardType;
    private Map<String, Object> moveData;
}
