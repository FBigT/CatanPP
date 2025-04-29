package com.catan.catanbackend.config;

import com.catan.catanbackend.service.TokenService;
import org.springframework.messaging.Message;
import org.springframework.messaging.MessageChannel;
import org.springframework.messaging.MessageDeliveryException;
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
    private final Map<String, String> sessionDestinationMap = new ConcurrentHashMap<>();

    public JwtChannelInterceptor(TokenService tokenService) {
        this.tokenService = tokenService;
    }

    @Override
    public Message<?> preSend(Message<?> message, MessageChannel channel) {
        StompHeaderAccessor accessor =
                MessageHeaderAccessor.getAccessor(message, StompHeaderAccessor.class);
        String authorizationHeader = accessor.getFirstNativeHeader("Authorization").trim();
        StompCommand command = accessor.getCommand();
        if (authorizationHeader != null && authorizationHeader.startsWith("Bearer ")) {
            String token = authorizationHeader.substring(7);

            if (tokenService.validateJwtToken(token)) {
                String username = tokenService.getUsernameFromJwtToken(token);

                UsernamePasswordAuthenticationToken user =
                        new UsernamePasswordAuthenticationToken(username, null, List.of());
                accessor.setUser(user);
                if (command == StompCommand.SUBSCRIBE) {
                    String sessionId = accessor.getSessionId();
                    String gameSessionCode = accessor.getDestination().substring(accessor.getDestination().lastIndexOf("/")); // ex: "/game/chat/123"
                    sessionDestinationMap.put(sessionId, gameSessionCode);
                    System.out.println("User subscribed: " + sessionId + " -> " + gameSessionCode);
                }

                if (command == StompCommand.DISCONNECT) {
                    String sessionId = accessor.getSessionId();
                    String destination = sessionDestinationMap.get(sessionId);
                    if (destination != null) {
                        sessionDestinationMap.remove(sessionId);
                    }
                }

                return message;
            }
        }
        throw new MessageDeliveryException("Unauthorized: Invalid or missing JWT token");
    }
}
