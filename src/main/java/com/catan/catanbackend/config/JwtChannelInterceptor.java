package com.catan.catanbackend.config;

import com.catan.catanbackend.controller.web_socket.WebSocketController;
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
import org.springframework.security.core.userdetails.UserDetails;
import org.springframework.security.core.userdetails.UserDetailsService;
import org.springframework.stereotype.Component;

import java.util.List;
import java.util.Map;
import java.util.concurrent.ConcurrentHashMap;

@Component
public class JwtChannelInterceptor implements ChannelInterceptor {
    public static final String AUTH_HEADER = "Authorization";
    private final UserDetailsService userDetailsService;
    final TokenService tokenService;
    final SessionPlayerService sessionPlayerService;
    final SessionService sessionService;
    private final ApplicationContext applicationContext;
    private SimpMessagingTemplate messagingTemplate;
    private final Map<String, Long> subscribers = new ConcurrentHashMap<>();

    public JwtChannelInterceptor(UserDetailsService userDetailsService, TokenService tokenService, SessionPlayerService sessionPlayerService, SessionService sessionService, ApplicationContext applicationContext) {
        this.userDetailsService = userDetailsService;
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
        StompHeaderAccessor accessor = MessageHeaderAccessor.getAccessor(message, StompHeaderAccessor.class);
        if (accessor == null) {
            throw new MessageDeliveryException("Malformed header received");
        }

        StompCommand cmd = accessor.getCommand();

        if (StompCommand.CONNECT.equals(cmd)) {
            String auth = accessor.getFirstNativeHeader(AUTH_HEADER);
            if (auth == null || !auth.startsWith("Bearer ")) {
                throw new MessageDeliveryException("Authorization header not found or invalid on CONNECT");
            }
            String token = auth.substring(7);
            if (!tokenService.validateJwtToken(token)) {
                throw new MessageDeliveryException("Unauthorized: Invalid JWT token");
            }
            // bind the user once, for the rest of the session
            UserDetails userDetails = userDetailsService.loadUserByUsername(
                    tokenService.getUsernameFromJwtToken(token)
            );
            UsernamePasswordAuthenticationToken userAuth =
                    new UsernamePasswordAuthenticationToken(
                            userDetails,
                            null,
                            userDetails.getAuthorities()
                    );
            accessor.setUser(userAuth);
            return message;
        }

        if (StompCommand.SEND.equals(cmd)) {
            if (accessor.getUser() != null) {
                return message;
            }
            String auth = accessor.getFirstNativeHeader(AUTH_HEADER);
            if (auth == null || !auth.startsWith("Bearer ")) {
                throw new MessageDeliveryException("Authorization header missing on SEND");
            }
            String token = auth.substring(7);
            if (!tokenService.validateJwtToken(token)) {
                throw new MessageDeliveryException("Unauthorized: Invalid JWT token on SEND");
            }
            return message;
        }

        if (StompCommand.SUBSCRIBE.equals(cmd) || StompCommand.DISCONNECT.equals(cmd)) {
            String dest = accessor.getDestination();
            if (dest != null && dest.contains("/game/players/")) {
                String code = dest.substring(dest.lastIndexOf('/') + 1);
                Long userId = tokenService.getUserIdFromJwtToken(
                        accessor.getFirstNativeHeader(AUTH_HEADER).substring(7)
                );

                if (StompCommand.SUBSCRIBE.equals(cmd)) {
                    sessionService.joinSession(userId, code);
                    subscribers.put(accessor.getSessionId(), userId);
                } else {
                    sessionService.leaveSession(userId, code);
                    subscribers.remove(accessor.getSessionId());
                }
                sendJoinSessionUpdate(code);
            }
            return message;
        }

        if (accessor.getUser() == null) {
            throw new MessageDeliveryException("Unauthorized: no authenticated user in session");
        }
        return message;
    }

    private void sendJoinSessionUpdate(String sessionCode) {
        List<SessionPlayer> players = sessionService.getPlayersBySessionCode(sessionCode);
        WebSocketController.sendJoinSessionUpdate(getMessagingTemplate(), sessionCode, players.stream().map(SessionPlayer::getUser).toList());
    }
}
