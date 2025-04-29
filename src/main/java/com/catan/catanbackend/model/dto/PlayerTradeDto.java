package com.catan.catanbackend.model.dto;

import com.catan.catanbackend.model.ResourceGroup;
import lombok.Data;
import lombok.NoArgsConstructor;

@Data
@NoArgsConstructor
public class PlayerTradeDto {
    private Long sessionId;
    private String fromUser;
    private String toUser;
    private ResourceGroup offered;
    private ResourceGroup requested;
}
