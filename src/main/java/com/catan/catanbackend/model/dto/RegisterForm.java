package com.catan.catanbackend.model.dto;

import lombok.AllArgsConstructor;
import lombok.Data;

@Data
@AllArgsConstructor
public class RegisterForm {
    String username;
    String password;
    String email;
}
