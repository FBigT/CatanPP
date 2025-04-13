package com.catan.catanbackend.model.dto;

import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.time.OffsetDateTime;

@Data
@NoArgsConstructor
@AllArgsConstructor
public class SessionSaveSimpleDto {
    Long id;
    String name;
    Integer turnNumber;
    OffsetDateTime saveTime;
}
