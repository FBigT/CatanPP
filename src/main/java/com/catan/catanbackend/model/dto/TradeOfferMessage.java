package com.catan.catanbackend.model.dto;

import com.catan.catanbackend.model.ResourceGroup;

public class TradeOfferMessage {
    private String fromUser;
    private String toUser;
    private ResourceGroup offered;
    private ResourceGroup requested;
}