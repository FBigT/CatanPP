package com.catan.catanbackend.model.dto.move_dtos;


import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

@Data
@NoArgsConstructor
@AllArgsConstructor
public class TradeResponseDto {
    private String fromUser;   // the one replying
    private String toUser;     // original offerer
    private boolean accepted;
}