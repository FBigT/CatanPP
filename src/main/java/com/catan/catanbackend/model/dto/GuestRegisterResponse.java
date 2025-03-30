package com.catan.catanbackend.model.dto;

import lombok.AllArgsConstructor;
import lombok.Data;

@Data
@AllArgsConstructor
public class GuestRegisterResponse {
    Long guestId;
    String username;
    String guestKey;
}
