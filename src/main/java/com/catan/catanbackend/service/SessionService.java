package com.catan.catanbackend.service;

import com.catan.catanbackend.model.*;
import com.catan.catanbackend.repository.SessionCodeRepository;
import com.catan.catanbackend.repository.SessionRecordRepository;
import com.catan.catanbackend.repository.SessionRepository;
import org.hibernate.Hibernate;

import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;
import org.springframework.context.annotation.Lazy;
import java.time.OffsetDateTime;
import java.util.*;

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
    private final DevCardService devCardService;

    public SessionService(SessionRepository sessionRepository,
                          SessionCodeRepository sessionCodeRepository,
                          PlayerProfileService playerProfileService,
                          UserService userService,
                          SessionRecordRepository sessionRecordRepository,
                          SessionPlayerService sessionPlayerService, @Lazy DevCardService devCardService) {
        this.sessionRepository = sessionRepository;
        this.sessionCodeRepository = sessionCodeRepository;
        this.playerProfileService = playerProfileService;
        this.userService = userService;
        this.sessionRecordRepository = sessionRecordRepository;
        this.sessionPlayerService = sessionPlayerService;
        this.devCardService = devCardService;
    }

    public Session save(Session session) {
        return sessionRepository.saveAndFlush(session);
    }

    public Optional<SessionCode> createSession(Long hostId, Integer maxPlayers) {
        Optional<User> host = userService.findById(hostId);
        if (host.isEmpty() || maxPlayers < MIN_PLAYERS) {
            return Optional.empty();
        }

        // No longer preventing multiple active sessions per host:
        Session session = new Session(host.get(), maxPlayers);
        session.setActive(false);
        Session savedSession = sessionRepository.save(session);
        devCardService.initDeckForSession(savedSession);

        String newSessionCode;
        do {
            newSessionCode = generateSessionCode();
        } while (sessionCodeRepository.findByCode(newSessionCode).isPresent());

        sessionPlayerService.saveSessionPlayer(new SessionPlayer(savedSession, host.get()));
        return Optional.of(sessionCodeRepository.save(new SessionCode(savedSession, newSessionCode)));
    }

    public Boolean startSession(Long sessionId) {
        Optional<Session> session = sessionRepository.findById(sessionId);
        if (session.isEmpty() || !session.get().getMapGenerated()) {
            return false;
        }

        List<SessionPlayer> players = getPlayers(sessionId);
        Collections.shuffle(players);
        session.get().setCurrentPlayer(players.get(0));

        for (int i = 0; i < players.size(); i++) {
            players.get(i).setTurnOrder(i+1);
            sessionPlayerService.saveSessionPlayer(players.get(i));
        }
        session.get().setActive(true);
        save(session.get());

        return true;
    }

    public Optional<SessionPlayer> getNextPlayer(Long sessionId) {
        Optional<Session> sessionOptional = getSessionById(sessionId);
        if (sessionOptional.isEmpty()) {
            return Optional.empty();
        }

        Session session = sessionOptional.get();
        List<SessionPlayer> players = getPlayersInTurnOrder(sessionId);
        int totalPlayers = players.size();
        int turnNumber = session.getTurnNumber();

        // Exit setup phase if done
        if (turnNumber >= totalPlayers * 2 && session.getInSetup()) {
            session.setInSetup(false);
            sessionRepository.save(session);
        }

        if (!session.getInSetup()) {
            // Normal play order
            int currentIndex = players.indexOf(session.getCurrentPlayer());
            int nextIndex = (currentIndex + 1) % totalPlayers;
            return Optional.of(players.get(nextIndex));
        }

        // Setup phase logic
        if (turnNumber < totalPlayers) {
            // Forward setup: 0 → N-1
            return Optional.of(players.get(turnNumber));
        } else {
            // Reverse setup: N-1 → 0
            int reverseIndex = 2 * totalPlayers - turnNumber - 1;
            return Optional.of(players.get(reverseIndex));
        }
    }

    public Boolean endSession(SessionCode sessionCode, User winner) {
        Optional<PlayerProfile> playerProfileByUsername =
                playerProfileService.getPlayerProfileByUsername(winner.getUsername());
        playerProfileByUsername.ifPresent(pp -> pp.setGamesWon(pp.getGamesWon() + 1));

        List<SessionPlayer> players =
                getPlayers(sessionCode.getSession().getId());
        for (SessionPlayer sp : players) {
            playerProfileService.getPlayerProfileByUserId(sp.getUser().getId())
                    .ifPresent(profile -> profile.setGamesPlayed(profile.getGamesPlayed() + 1));
            sessionPlayerService.deleteSessionPlayer(sp);
        }

        sessionRecordRepository.save(
                new SessionRecord(winner,
                        sessionCode.getSession().getStartedAt(),
                        OffsetDateTime.now())
        );
        sessionCodeRepository.delete(sessionCode);
        sessionRepository.delete(sessionCode.getSession());
        return true;
    }

    public Optional<SessionCode> joinSession(Long userId, String code) {
        System.out.println(userId);
        Optional<User> user = userService.findById(userId);
        Optional<SessionCode> sessionCode = sessionCodeRepository.findByCode(code);

        if (user.isPresent() && sessionCode.isPresent()) {
            List<SessionPlayer> players =
                    getPlayers(sessionCode.get().getSession().getId());

            if (players.stream().anyMatch(p ->
                    p.getUser().getId().equals(userId) && !p.getActive())) {
                SessionPlayer sp = players.stream()
                        .filter(p -> p.getUser().getId().equals(userId))
                        .findFirst().get();
                sp.setActive(true);
                sessionPlayerService.saveSessionPlayer(sp);
            }
            else if (players.size() >= sessionCode.get().getSession().getMaxPlayers()
                    || players.stream().anyMatch(p ->
                    p.getUser().getId().equals(userId) && p.getActive())) {
                return Optional.empty();
            }
            else {
                sessionPlayerService.saveSessionPlayer(
                        new SessionPlayer(sessionCode.get().getSession(), user.get())
                );
            }
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
        Optional<List<SessionPlayer>> sessionPlayers = session.map(s ->
                getPlayers(s.getId()).stream()
                        .filter(p -> p.getUser().getId().equals(userId))
                        .toList()
        );
        if (sessionPlayers.isPresent() && sessionPlayers.get().size() == 1) {
            SessionPlayer sp = sessionPlayers.get().get(0);
            if (sp.getActive()) {
                sessionPlayerService.saveSessionPlayer(sp);
                return true;
            }
        }
        return false;
    }

    public List<Session> getSessionsByHostId(Long hostId) {
        return sessionRepository.findByHostId(hostId);
    }

    public Optional<Session> getActiveSessionsByHostId(Long hostId) {
        List<Session> all = sessionRepository.findByHostId(hostId).stream()
                .filter(Session::getActive)
                .toList();
        if (all.isEmpty()) return Optional.empty();
        if (all.size() > 1) {
            throw new RuntimeException("Multiple active sessions for host " + hostId);
        }
        return Optional.of(all.get(0));
    }

    public void closeSession(Session session) {
        session.setActive(false);
        sessionRepository.saveAndFlush(session);
        sessionPlayerService.deactivateSessionPlayers(session.getId());
    }

    public Optional<Session> getSessionById(Long id) {
        return sessionRepository.findById(id);
    }

    public Optional<Session> getSessionBySessionCode(String code) {
        return sessionCodeRepository.findByCode(code)
                .map(sc -> {
                    Hibernate.initialize(sc.getSession());
                    return sc.getSession();
                });
    }

    private String generateSessionCode() {
        int leftLimit = 48; // '0'
        int rightLimit = 122; // 'z'
        int length = 6;
        return random.ints(leftLimit, rightLimit + 1)
                .filter(i -> (i <= 57 || i >= 65) && (i <= 90 || i >= 97))
                .limit(length)
                .collect(StringBuilder::new,
                        StringBuilder::appendCodePoint,
                        StringBuilder::append)
                .toString()
                .toUpperCase();
    }
    public List<Session> getAllSessionsByUser(Long userId) {
        return sessionPlayerService.findPlayersByUserId(userId)
                .stream()
                .map(SessionPlayer::getSession)
                .distinct()
                .toList();
    }



    public void deleteAllSessions() {
        sessionRepository.deleteAll();
    }

    public List<SessionPlayer> getPlayers(Long sessionId){
        return sessionPlayerService.findPlayerBySessionId(sessionId);
    }

    public List<SessionPlayer> getPlayersInTurnOrder(Long sessionId){
        return sessionPlayerService.findPlayerBySessionId(sessionId).stream().sorted(Comparator.comparingInt(SessionPlayer::getTurnOrder)).toList();
    }

    public List<SessionPlayer> getPlayersBySessionCode(String sessionCode){
        return sessionPlayerService.findPlayersBySessionCode(sessionCode);
    }
}
