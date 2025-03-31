package com.catan.catanbackend.service;

import com.catan.catanbackend.model.*;
import com.catan.catanbackend.repository.SessionCodeRepository;
import com.catan.catanbackend.repository.SessionRecordRepository;
import com.catan.catanbackend.repository.SessionRepository;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.time.OffsetDateTime;
import java.util.List;
import java.util.Optional;
import java.util.Random;

@Service
@Transactional
public class SessionService {
    public static final Integer MIN_PLAYERS = 2;

    final SessionRepository sessionRepository;
    final SessionCodeRepository sessionCodeRepository;
    final SessionRecordRepository sessionRecordRepository;
    final PlayerProfileService playerProfileService;
    final UserService userService;
    private final SessionPlayerService sessionPlayerService;
    final Random random = new Random();

    public SessionService(SessionRepository sessionRepository, SessionCodeRepository sessionCodeRepository, PlayerProfileService playerProfileService, UserService userService, SessionRecordRepository sessionRecordRepository, SessionPlayerService sessionPlayerService) {
        this.sessionRepository = sessionRepository;
        this.sessionCodeRepository = sessionCodeRepository;
        this.playerProfileService = playerProfileService;
        this.userService = userService;
        this.sessionRecordRepository = sessionRecordRepository;
        this.sessionPlayerService = sessionPlayerService;
    }

    public Optional<SessionCode> startSession(Long hostId, Integer maxPlayers) {
        Optional<User> host = userService.findById(hostId);
        if (host.isEmpty() || maxPlayers <= MIN_PLAYERS) {
            return Optional.empty();
        }

        if ((long)sessionPlayerService.findPlayersByUserId(hostId).size() > 0) {
            return Optional.empty();
        }

        Session savedSession = sessionRepository.save(new Session(host.get(), maxPlayers));

        String newSessionCode;
        do {
            newSessionCode = generateSessionCode();
        } while (sessionCodeRepository.findByCode(newSessionCode).isPresent());
        sessionPlayerService.createSessionPlayer(new SessionPlayer(savedSession, host.get()));
        return Optional.of(sessionCodeRepository.save(new SessionCode(savedSession, newSessionCode)));
    }

    public Boolean endSession(SessionCode sessionCode, User winner){
        Optional<PlayerProfile> playerProfileByUsername = playerProfileService.getPlayerProfileByUsername(winner.getUsername());
        playerProfileByUsername.ifPresent(playerProfile -> playerProfile.setGamesWon(playerProfile.getGamesWon() + 1));

        List<SessionPlayer> players = getPlayers(sessionCode.getSession().getId());
        for (SessionPlayer sessionPlayer : players) {
            playerProfileService.getPlayerProfileByUserId(sessionPlayer.getUser().getId())
                    .ifPresent(profile -> profile.setGamesPlayed(profile.getGamesPlayed() + 1));
            sessionPlayerService.deleteSessionPlayer(sessionPlayer);
        }

        sessionRecordRepository.save(new SessionRecord(winner, sessionCode.getSession().getStartedAt(), OffsetDateTime.now()));
        sessionCodeRepository.delete(sessionCode);
        sessionRepository.delete(sessionCode.getSession());
        return true;
    }

    public Optional<SessionCode> joinSession(Long userId, String code) {
        Optional<User> user = userService.findById(userId);
        Optional<SessionCode> sessionCode = sessionCodeRepository.findByCode(code);

        if (user.isPresent() && sessionCode.isPresent()) {
            List<User> players = getPlayers(sessionCode.get().getSession().getId()).stream()
                    .map(SessionPlayer::getUser).toList();

            if (players.contains(user.get()) || players.size() >= sessionCode.get().getSession().getMaxPlayers()) {
                return Optional.empty();
            }

            sessionPlayerService.createSessionPlayer(new SessionPlayer(sessionCode.get().getSession(), user.get()));
        }
        return sessionCode;
    }

    public Boolean addBotToSession(String code) {
        Optional<SessionCode> sessionCode = sessionCodeRepository.findByCode(code);

        if (sessionCode.isEmpty()
            || getPlayers(sessionCode.get().getSession().getId()).size() >= sessionCode.get().getSession().getMaxPlayers()) {
            return false;
        }
        sessionPlayerService.createSessionPlayer(new SessionPlayer(sessionCode.get().getSession()));
        return true;
    }

    private String generateSessionCode() {
        int leftLimit = 48; // '0'
        int rightLimit = 122; // 'z'
        int targetStringLength = 6;

        String generatedString = random.ints(leftLimit, rightLimit + 1)
                .filter(i -> (i <= 57 || i >= 65) && (i <= 90 || i >= 97))
                .limit(targetStringLength)
                .collect(StringBuilder::new, StringBuilder::appendCodePoint, StringBuilder::append)
                .toString();

        return generatedString.toUpperCase();
    }

    public List<SessionPlayer> getPlayers(Long sessionId){
        return sessionPlayerService.findPlayerBySessionId(sessionId);
    }
}
