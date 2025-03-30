package com.catan.catanbackend.model.dto;

import lombok.Data;

@Data
public class LogInResponse {
    private String token;
    private String tokenType = "Bearer";

    private String username;
    private Long userId;

    public LogInResponse(Long userId, String username, String accessToken) {
        this.token = accessToken;
        this.username = username;
        this.userId = userId;
    }

    public void setUserId(String userId) {
        this.userId = Long.parseLong(userId);
    }
}
