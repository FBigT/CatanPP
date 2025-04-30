package com.catan.catanbackend.config;

import com.catan.catanbackend.controller.webSocket.ChatController;
import com.catan.catanbackend.model.SessionPlayer;
import com.catan.catanbackend.service.SessionPlayerService;
import com.catan.catanbackend.service.SessionService;
import com.catan.catanbackend.service.TokenService;
import org.springframework.context.ApplicationContext;
import org.springframework.messaging.Message;
import org.springframework.messaging.MessageChannel;
import org.springframework.messaging.MessageDeliveryException;
import org.springframework.messaging.simp.SimpMessagingTemplate;
import org.springframework.messaging.simp.stomp.StompCommand;
import org.springframework.messaging.simp.stomp.StompHeaderAccessor;
import org.springframework.messaging.support.ChannelInterceptor;
import org.springframework.messaging.support.MessageHeaderAccessor;
import org.springframework.security.authentication.UsernamePasswordAuthenticationToken;
import org.springframework.stereotype.Component;

import java.util.List;
import java.util.Map;
import java.util.concurrent.ConcurrentHashMap;

@Component
public class JwtChannelInterceptor implements ChannelInterceptor {
    final TokenService tokenService;
    final SessionPlayerService sessionPlayerService;
    final SessionService sessionService;
    private final ApplicationContext applicationContext;
    private SimpMessagingTemplate messagingTemplate;
    private final Map<String, Long> subscribers = new ConcurrentHashMap<>();

    public JwtChannelInterceptor(TokenService tokenService, SessionPlayerService sessionPlayerService, SessionService sessionService, ApplicationContext applicationContext) {
        this.tokenService = tokenService;
        this.sessionPlayerService = sessionPlayerService;
        this.sessionService = sessionService;
        this.applicationContext = applicationContext;
    }

    private SimpMessagingTemplate getMessagingTemplate() {
        if (messagingTemplate == null) {
            messagingTemplate = applicationContext.getBean(SimpMessagingTemplate.class);
        }
        return messagingTemplate;
    }

    @Override
    public Message<?> preSend(Message<?> message, MessageChannel channel) {
        StompHeaderAccessor accessor =
                MessageHeaderAccessor.getAccessor(message, StompHeaderAccessor.class);
        if (accessor == null) {
            throw new MessageDeliveryException("Malformed header received");
        }
        if (accessor.getFirstNativeHeader("Authorization") == null || accessor.getFirstNativeHeader("Authorization").isBlank()) {
            throw new MessageDeliveryException("Authorization header not found");
        }
        String authorizationHeader = accessor.getFirstNativeHeader("Authorization");
        StompCommand command = accessor.getCommand();

        if (authorizationHeader != null && authorizationHeader.startsWith("Bearer ")) {
            String token = authorizationHeader.substring(7);

            if (tokenService.validateJwtToken(token)) {
                String username = tokenService.getUsernameFromJwtToken(token);
                Long userId = tokenService.getUserIdFromJwtToken(token);

                UsernamePasswordAuthenticationToken user =
                        new UsernamePasswordAuthenticationToken(username, null, List.of());
                accessor.setUser(user);


                if (command == StompCommand.SUBSCRIBE && accessor.getDestination() != null && accessor.getDestination().contains("/game/players/")) {
                    String gameSessionCode = accessor.getDestination().substring(accessor.getDestination().lastIndexOf("/")+1);
                    sessionService.joinSession(userId, gameSessionCode);
                    subscribers.put(accessor.getSessionId(), userId);

                    sendJoinSessionUpdate(gameSessionCode);
                }

                if (command == StompCommand.DISCONNECT && accessor.getDestination() != null && accessor.getDestination().contains("/game/players/")) {
                    String gameSessionCode = accessor.getDestination().substring(accessor.getDestination().lastIndexOf("/")+1);
                    sessionService.leaveSession(userId, gameSessionCode);
                    subscribers.remove(accessor.getSessionId());

                    sendJoinSessionUpdate(gameSessionCode);
                }

                return message;
            }
        }
        throw new MessageDeliveryException("Unauthorized: Invalid or missing JWT token");
    }

    private void sendJoinSessionUpdate(String sessionCode) {
        List<SessionPlayer> players = sessionService.getPlayersBySessionCode(sessionCode);
        ChatController.sendJoinSessionUpdate(getMessagingTemplate(), sessionCode, players.stream().map(SessionPlayer::getUser).toList());
    }
}
