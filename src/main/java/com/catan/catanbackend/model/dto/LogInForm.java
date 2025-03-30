package com.catan.catanbackend.model.dto;

import lombok.Data;
import lombok.NoArgsConstructor;

@Data
@NoArgsConstructor
public class LogInForm {
    private String username;
    private String password;
}
