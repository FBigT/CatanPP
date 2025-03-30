package com.catan.catanbackend.model.dto;

import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.io.Serializable;
import java.time.LocalDateTime;

@Data
@NoArgsConstructor
@AllArgsConstructor
public class UserDto implements Serializable {
    Long id;
    String username;
    String email;
    Boolean active;
    Boolean guest;
    LocalDateTime createdAt;
}
