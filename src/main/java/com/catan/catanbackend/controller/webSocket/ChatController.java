package com.catan.catanbackend.controller.webSocket;

import com.catan.catanbackend.controller.UserController;
import com.catan.catanbackend.model.User;
import com.catan.catanbackend.model.dto.ChatMessage;
import com.catan.catanbackend.model.dto.JoinSessionNotification;
import com.catan.catanbackend.service.SessionPlayerService;
import org.springframework.messaging.handler.annotation.DestinationVariable;
import org.springframework.messaging.handler.annotation.MessageMapping;
import com.catan.catanbackend.model.dto.RawChatMessage;
import org.springframework.messaging.simp.SimpMessagingTemplate;
import org.springframework.stereotype.Controller;
import org.springframework.web.bind.annotation.CrossOrigin;

import java.security.Principal;
import java.util.List;

@Controller
@CrossOrigin
public class ChatController {
    private final SimpMessagingTemplate messagingTemplate;
    private final UserController userController;

    public ChatController(SimpMessagingTemplate messagingTemplate, UserController userController) {
        this.messagingTemplate = messagingTemplate;
        this.userController = userController;
    }

    @MessageMapping("/chat/{sessionCode}")
    public void send(@DestinationVariable String sessionCode, RawChatMessage message, Principal principal) {
        String username = principal.getName();
        messagingTemplate.convertAndSend("/game/chat/" + sessionCode, new ChatMessage(username, message));
    }

    public void sendJoinSessionUpdate(String sessionCode, List<User> users) {
        messagingTemplate.convertAndSend("/game/players/" + sessionCode, new JoinSessionNotification(users));
    }

    public void userDisconnected(Long id) {

    }
}
