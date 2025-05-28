package com.catan.catanbackend.model.dto.move_dtos;

import com.catan.catanbackend.model.ResourceGroup;
import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

@Data
@NoArgsConstructor
@AllArgsConstructor
public class TradeOfferDto {
    private String fromUser;
    private String toUser;
    private ResourceGroup offered;
    private ResourceGroup requested;
}