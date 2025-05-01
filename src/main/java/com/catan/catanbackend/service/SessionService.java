package com.catan.catanbackend.service;

import com.catan.catanbackend.model.*;
import com.catan.catanbackend.repository.SessionCodeRepository;
import com.catan.catanbackend.repository.SessionRecordRepository;
import com.catan.catanbackend.repository.SessionRepository;
import org.hibernate.Hibernate;
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

        if ((long)sessionPlayerService.findActivePlayersByUserId(hostId).size() > 0) {
            return Optional.empty();
        }

        Session savedSession = sessionRepository.save(new Session(host.get(), maxPlayers));

        String newSessionCode;
        do {
            newSessionCode = generateSessionCode();
        } while (sessionCodeRepository.findByCode(newSessionCode).isPresent());
        sessionPlayerService.saveSessionPlayer(new SessionPlayer(savedSession, host.get()));
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
            List<SessionPlayer> players = getPlayers(sessionCode.get().getSession().getId());

            if (players.stream().anyMatch(player -> player.getUser().getId().equals(userId) && !player.getActive())){
                SessionPlayer sessionPlayer = players.stream().filter(player -> player.getUser().getId().equals(userId)).findFirst().get();
                sessionPlayer.setActive(true);
                sessionPlayerService.saveSessionPlayer(sessionPlayer);
            } else if (players.size() >= sessionCode.get().getSession().getMaxPlayers() ||
                    players.stream().anyMatch(player -> player.getUser().getId().equals(userId) && player.getActive()))
                return Optional.empty();
            else
                sessionPlayerService.saveSessionPlayer(new SessionPlayer(sessionCode.get().getSession(), user.get()));
        }
        return sessionCode;
    }

    public Boolean addBotToSession(String code) {
        Optional<Session> session = getSessionBySessionCode(code);

        if (session.isEmpty()
            || getPlayers(session.get().getId()).size() >= session.get().getMaxPlayers()) {
            return false;
        }
        sessionPlayerService.saveSessionPlayer(new SessionPlayer(session.get()));
        return true;
    }

    public Boolean leaveSession(Long userId, String code) {
        Optional<Session> session = getSessionBySessionCode(code);
        Optional<List<SessionPlayer>> sessionPlayers = session.map(value ->
                getPlayers(value.getId()).stream()
                        .filter(player -> player.getUser().getId().equals(userId)).toList());
        if (sessionPlayers.isPresent() && sessionPlayers.get().size() == 1 ) {
            SessionPlayer sessionPlayer = sessionPlayers.get().stream().findFirst().get();
            if (sessionPlayer.getActive()){
                sessionPlayerService.saveSessionPlayer(sessionPlayer);
                return true;
            }
        }
        return false;
    }

    public List<Session> getSessionsByHostId(Long hostId) {
        return sessionRepository.findByHostId(hostId);
    }

    public Optional<Session> getActiveSessionsByHostId(Long hostId) {
        if (sessionRepository.findByHostId(hostId).stream().noneMatch(Session::getActive)) {
            return Optional.empty();
        }
        if (sessionRepository.findByHostId(hostId).stream().filter(Session::getActive).count() >= 2) {
            throw new RuntimeException("Multiple active sessions found for host " + hostId);
        }
        return sessionRepository.findByHostId(hostId).stream().filter(Session::getActive).findFirst();
    }

    public void closeSession(Session session) {
        session.setActive(false);
        sessionRepository.saveAndFlush(session);
        sessionPlayerService.deactivateSessionPlayers(session.getId());
    }

    public Optional<Session> getSessionById(Long id){
        return sessionRepository.findById(id);
    }

    public Optional<Session> getSessionBySessionCode(String code){
        return sessionCodeRepository.findByCode(code).map(sessionCode -> {
            Hibernate.initialize(sessionCode.getSession());
            return sessionCode.getSession();
        });
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

    public String createSaveJson(Session session) {
        //create save logic
        return "";
    }

    public List<SessionPlayer> getPlayers(Long sessionId){
        return sessionPlayerService.findPlayerBySessionId(sessionId);
    }

    public List<SessionPlayer> getPlayersBySessionCode(String sessionCode){
        return sessionPlayerService.findPlayersBySessionCode(sessionCode);
    }
}
