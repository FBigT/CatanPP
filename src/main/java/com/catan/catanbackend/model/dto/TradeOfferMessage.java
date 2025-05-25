package com.catan.catanbackend.model.dto;

import com.catan.catanbackend.model.ResourceGroup;
import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

@Data
@NoArgsConstructor
@AllArgsConstructor
public class TradeOfferMessage {
    private String fromUser;
    private String toUser;
    private ResourceGroup offered;
    private ResourceGroup requested;
}