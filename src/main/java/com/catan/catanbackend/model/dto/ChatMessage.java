package com.catan.catanbackend.model.dto;

import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.time.OffsetDateTime;

@Data
@AllArgsConstructor
@NoArgsConstructor
public class ChatMessage {
    public ChatMessage(String sender, RawChatMessage message) {
        senderUsername = sender;
        text = message.getText();
        timestamp = OffsetDateTime.now();
    }

    private String senderUsername;
    private String text;
    private OffsetDateTime timestamp;
}
