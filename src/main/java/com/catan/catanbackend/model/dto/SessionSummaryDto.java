package com.catan.catanbackend.model.dto;

import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;
import java.time.OffsetDateTime;

@Data
@NoArgsConstructor
@AllArgsConstructor
public class SessionSummaryDto {
    Long sessionId;
    String hostUsername;
    OffsetDateTime createdAt;
}
