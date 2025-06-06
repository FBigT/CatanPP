package com.catan.catanbackend.model.dto.move_dtos.responses;

import com.catan.catanbackend.model.DevCard;
import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.util.List;

@Data
@AllArgsConstructor
@NoArgsConstructor
public class DevCardsListResponseDto {
    private List<DevCard> devCards;
    private String username;
}
