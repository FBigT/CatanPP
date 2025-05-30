package com.catan.catanbackend.model.dto.move_dtos.responses;

import com.catan.catanbackend.model.ResourceGroup;
import com.catan.catanbackend.model.dto.move_dtos.TradeOfferDto;
import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

@Data
@NoArgsConstructor
@AllArgsConstructor
public class TradeResponseDto {
    public TradeResponseDto(TradeOfferDto tradeOfferDto, Boolean accepted) {
        this.accepted = accepted;
        fromUser = tradeOfferDto.getToUser();
        toUser = tradeOfferDto.getFromUser();
    }

    private String fromUser;      // who responded
    private String toUser;        // original offerer
    private boolean accepted;     // true=accepted, false=denied
}