package com.catan.catanbackend.model.dto.move_dtos.responses;

import com.catan.catanbackend.model.ResourceGroup;
import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

@Data
@NoArgsConstructor
@AllArgsConstructor
public class PayDebtResponse {
    String username;
    ResourceGroup resourceGroup;
}
