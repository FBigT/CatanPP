package com.catan.catanbackend.model.dto;

import lombok.Data;

@Data
public class LogInResponse {
    private String token;
    private String tokenType = "Bearer";

    private String refreshToken;

    private String username;
    private Long userId;

    public LogInResponse(Long userId, String username, String accessToken, String refreshToken) {
        this.token = accessToken;
        this.username = username;
        this.userId = userId;
        this.refreshToken = refreshToken;
    }

    public void setUserId(String userId) {
        this.userId = Long.parseLong(userId);
    }
}
