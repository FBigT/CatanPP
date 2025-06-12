package com.catan.catanbackend.service;

import com.catan.catanbackend.model.SessionCode;
import com.catan.catanbackend.model.SessionPlayer;
import com.catan.catanbackend.model.dto.ChatMessage;
import com.catan.catanbackend.model.dto.RawChatMessage;
import com.catan.catanbackend.model.tile.Road;
import com.catan.catanbackend.model.tile.TileCorner;
import com.catan.catanbackend.model.tile.TileEdge;
import com.catan.catanbackend.repository.SessionCodeRepository;
import com.catan.catanbackend.repository.tiles.TileEdgeRepository;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.util.HashSet;
import java.util.List;
import java.util.Optional;
import java.util.Set;

@Service
public class LongestRoadService {

    private final TileEdgeRepository tileEdgeRepository;
    private final SessionPlayerService sessionPlayerService;
    private final SessionService sessionService;
    private final SessionCodeRepository sessionCodeRepository;
    private final NotificationService notificationService;

    public LongestRoadService(TileEdgeRepository tileEdgeRepository, SessionPlayerService sessionPlayerService, SessionService sessionService, SessionCodeRepository sessionCodeRepository, NotificationService notificationService) {
        this.tileEdgeRepository = tileEdgeRepository;
        this.sessionPlayerService = sessionPlayerService;
        this.sessionService = sessionService;
        this.sessionCodeRepository = sessionCodeRepository;
        this.notificationService = notificationService;
    }

    @Transactional
    public void checkForLongestRoad(Road road) {
        TileEdge startEdge = road.getTileEdge();
        Long ownerId = road.getOwner().getId();

        Set<Long> visited = new HashSet<>();
        visited.add(road.getId());

        int left = walkRoad(startEdge.getCornerA(), visited, startEdge, ownerId);
        int right = walkRoad(startEdge.getCornerB(), visited, startEdge, ownerId);

        Integer totalRoadLength = left + 1 + right;
        if (totalRoadLength > road.getOwner().getSession().getLongestRoadValue() && !road.getOwner().getLongestRoad()) {
            List<SessionPlayer> players = sessionPlayerService.findPlayerBySessionId(road.getOwner().getSession().getId());
            players.stream().filter(SessionPlayer::getLongestRoad).forEach(sessionPlayer -> {
                sessionPlayer.setPlayerScore(sessionPlayer.getPlayerScore() - 2);
                sessionPlayer.setLongestRoad(false);
                sessionPlayerService.updateSessionPlayer(sessionPlayer);
            });

            road.getOwner().getSession().setLongestRoadValue(totalRoadLength);
            road.getOwner().setLongestRoad(true);
            road.getOwner().setPlayerScore(road.getOwner().getPlayerScore() + 2);

            sessionPlayerService.updateSessionPlayer(road.getOwner());
            sessionService.save(road.getOwner().getSession());

            Optional<SessionCode> bySessionId = sessionCodeRepository.findBySessionId(road.getOwner().getSession().getId());
            if (bySessionId.isPresent()) {
                SessionCode sessionCode = bySessionId.get();
                notificationService.sendChatMessage(sessionCode.getCode(),
                        new ChatMessage("System", new RawChatMessage("A new longest road has been achieved by " + road.getOwner().getName() + " it is " + totalRoadLength)));
            }
        }
    }

    @Transactional
    public int walkRoad(TileCorner currentCorner, Set<Long> visitedRoadIds, TileEdge fromEdge, Long ownerId) {
        int maxLength = 0;

        List<TileEdge> connectedEdges = tileEdgeRepository.findAllConnectedToCorner(currentCorner);

        for (TileEdge edge : connectedEdges) {
            if (edge.equals(fromEdge)) continue;

            Road road = edge.getRoad();
            if (road != null
                    && !visitedRoadIds.contains(road.getId())
                    && road.getOwner().getId().equals(ownerId)) {

                visitedRoadIds.add(road.getId());
                TileCorner nextCorner = edge.getCornerA().equals(currentCorner) ? edge.getCornerB() : edge.getCornerA();
                int pathLength = 1 + walkRoad(nextCorner, visitedRoadIds, edge, ownerId);
                maxLength = Math.max(maxLength, pathLength);
                visitedRoadIds.remove(road.getId()); // backtrack for other paths
            }
        }

        return maxLength;
    }
}
