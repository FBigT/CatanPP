package com.catan.catanbackend.model.dto;

import lombok.Data;

@Data
public class GameSaveDto {
    private String saveName;
    private String gameStateJson;
}
