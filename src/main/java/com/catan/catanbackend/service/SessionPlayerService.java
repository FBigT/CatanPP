package com.catan.catanbackend.service;

import com.catan.catanbackend.model.Session;
import com.catan.catanbackend.model.SessionPlayer;
import com.catan.catanbackend.repository.SessionPlayerRepository;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.util.List;
import java.util.Optional;

@Service
@Transactional
public class SessionPlayerService {
    private final SessionPlayerRepository sessionPlayerRepository;

    public SessionPlayerService(SessionPlayerRepository sessionPlayerRepository) {
        this.sessionPlayerRepository = sessionPlayerRepository;
    }

    public SessionPlayer saveSessionPlayer(SessionPlayer sessionPlayer) {
        return sessionPlayerRepository.saveAndFlush(sessionPlayer);
    }

    public SessionPlayer updateSessionPlayer(SessionPlayer sessionPlayer) {
        return sessionPlayerRepository.saveAndFlush(sessionPlayer);
    }

    public void deleteSessionPlayer(SessionPlayer sessionPlayer) {
        sessionPlayerRepository.delete(sessionPlayer);
    }

    public List<SessionPlayer> findPlayersByUserId(Long userId) {
        return sessionPlayerRepository.findSessionPlayerByUserId(userId);
    }

    public List<SessionPlayer> findActivePlayersByUserId(Long userId) {
        return sessionPlayerRepository.findSessionPlayerByUserId(userId).stream().filter(SessionPlayer::getActive).toList();
    }

    public List<SessionPlayer> findPlayerBySessionId(Long sessionId) {
        return sessionPlayerRepository.findSessionPlayerBySessionId(sessionId);
    }

    public void deactivateSessionPlayers(Long sessionId) {
        sessionPlayerRepository.findSessionPlayerBySessionId(sessionId).forEach(x -> {
            x.setActive(false);
            sessionPlayerRepository.saveAndFlush(x);
        });
    }

    public Optional<SessionPlayer> findCurrentSessionPlayerByUserId(Long userId) {
        List<SessionPlayer> sessionPlayers = findPlayersByUserId(userId);
        long count = sessionPlayers.stream().filter(SessionPlayer::getActive).count();
        if (count == 0){
            return Optional.empty();
        }
        if (count > 1){
            throw new RuntimeException("Too many session for this player");
        }
        return sessionPlayers.stream().filter(SessionPlayer::getActive).findFirst();
    }

    public Optional<Session> findSessionByPlayerId(Long userId) {
        Optional<SessionPlayer> first = findCurrentSessionPlayerByUserId(userId);
        if (first.isPresent()) {
            return first.map(SessionPlayer::getSession);
        }
        return Optional.empty();
    }

    public List<SessionPlayer> findPlayersBySessionCode(String sessionCode) {
        return sessionPlayerRepository.findAllBySessionCodeWithUser(sessionCode);
    }


//    public Optional<SessionPlayer> findById(Long playerId) {
//        return sessionPlayerRepository.findById(playerId);
//    }

}
