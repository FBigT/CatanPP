package com.catan.catanbackend.service;

import com.catan.catanbackend.model.dto.ChatMessage;
import org.springframework.messaging.simp.SimpMessagingTemplate;
import org.springframework.stereotype.Service;

@Service
public class NotificationService {
    private final SimpMessagingTemplate messagingTemplate;

    public NotificationService(SimpMessagingTemplate messagingTemplate) {
        this.messagingTemplate = messagingTemplate;
    }

    public void sendChatMessage(String sessionCode, ChatMessage message) {
        messagingTemplate.convertAndSend("/game/chat/" + sessionCode, message);
    }
}
