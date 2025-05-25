package com.catan.catanbackend.model.dto.move_dtos.responses;

import com.catan.catanbackend.model.helper.DevCardType;
import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

@Data
@AllArgsConstructor
@NoArgsConstructor
public class PrivateBuyCardResponse {
    private DevCardType cardType;
    private Long cardId;
}
