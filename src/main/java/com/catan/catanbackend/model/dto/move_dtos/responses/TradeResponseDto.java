package com.catan.catanbackend.model.dto.move_dtos.responses;

import com.catan.catanbackend.model.ResourceGroup;
import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

@Data
@NoArgsConstructor
@AllArgsConstructor
public class TradeResponseDto {
    private String fromUser;      // who responded
    private String toUser;        // original offerer
    private ResourceGroup offered;    // what the responder gives back
    private ResourceGroup requested;  // what the responder takes
    private boolean accepted;     // true=accepted, false=denied
}