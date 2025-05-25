package com.catan.catanbackend.model.dto.move_dtos;

import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.util.Map;

@Data
@NoArgsConstructor
@AllArgsConstructor
public class DevCardPlayDto {
    private Long id;
    private Map<String, Object> cardPlayData;
}
