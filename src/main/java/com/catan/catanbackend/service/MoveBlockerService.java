package com.catan.catanbackend.service;

import com.catan.catanbackend.model.RobberMoveBlocker;
import com.catan.catanbackend.model.SessionPlayer;
import com.catan.catanbackend.model.tile.Tile;
import com.catan.catanbackend.repository.RobberMoveBlockerRepository;
import org.springframework.stereotype.Service;

import java.util.Optional;

@Service
public class MoveBlockerService {
    private final RobberMoveBlockerRepository moveBlockerRepository;
    private final TileService tileService;
    private final SessionPlayerService sessionPlayerService;

    public MoveBlockerService(RobberMoveBlockerRepository moveBlockerRepository, TileService tileService, SessionPlayerService sessionPlayerService) {
        this.moveBlockerRepository = moveBlockerRepository;
        this.tileService = tileService;
        this.sessionPlayerService = sessionPlayerService;
    }

    public Boolean isPlayerBlocked(Long sessionPlayerId) {
        Optional<SessionPlayer> sessionPlayer = sessionPlayerService.findById(sessionPlayerId);
        if (sessionPlayer.isEmpty()) {
            throw new IllegalArgumentException("User has no active session player");
        }
        Long sessionId = sessionPlayer.get().getSession().getId();
        Optional<SessionPlayer> blockerPlayer = getBlockerPlayer(sessionId);
        Optional<Tile> robberTile = tileService.getRobberTile(sessionId);

        if (robberTile.isEmpty()) {
            throw new IllegalArgumentException("No robber found");
        }

        return blockerPlayer.map(player -> !player.getId().equals(sessionPlayerId)).orElse(false);
    }

    public Boolean isSessionBlocked(Long sessionId) {
        Optional<Tile> robberTile = tileService.getRobberTile(sessionId);
        if (robberTile.isEmpty()) {
            throw new IllegalArgumentException("No robber found");
        }
        return moveBlockerRepository.findAll().stream().anyMatch(x -> x.getSessionPlayer().getSession().getId().equals(sessionId));
    }

    public Optional<SessionPlayer> getBlockerPlayer(Long sessionId) {
        return moveBlockerRepository.findAll().stream()
                .map(RobberMoveBlocker::getSessionPlayer)
                .filter(sessionPlayer -> sessionPlayer.getSession().getId().equals(sessionId)).findFirst();
    }
}
