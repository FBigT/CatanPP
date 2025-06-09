package com.catan.catanbackend.model.dto;

import com.catan.catanbackend.model.ResourceGroup;
import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.util.Map;

@Data
@NoArgsConstructor
@AllArgsConstructor
public class DiceResultDto {
    private String username;
    private Integer rollResult;
    private Map<String, ResourceGroup> userResourcesGained;
}
