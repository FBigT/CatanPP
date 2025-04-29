package com.catan.catanbackend.model.dto;

import com.catan.catanbackend.model.ResourceGroup;
import lombok.Data;
import lombok.NoArgsConstructor;

@Data
@NoArgsConstructor
public class BankTradeDto {
    private Long sessionId;
    private String fromUser;
    private ResourceGroup offered;
    private ResourceGroup requested;
    private String portType;
    private int portRatio;
}
