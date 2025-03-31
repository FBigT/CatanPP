package com.catan.catanbackend.service;

import com.catan.catanbackend.model.ResourceGroup;
import com.catan.catanbackend.model.RobberBlocker;
import com.catan.catanbackend.model.RobberMoveBlocker;
import com.catan.catanbackend.model.SessionPlayer;
import com.catan.catanbackend.repository.RobberBlockerRepository;
import com.catan.catanbackend.repository.RobberMoveBlockerRepository;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.util.List;
import java.util.Optional;
import java.util.Random;

@Service
@Transactional
public class GameService {
    static Random rand = new Random();
    static String[] names = { "Mirko", "Marko", "Mio", "Febo", "Gjuro", "Pero", "Nano", "Fico" };

    private final Mapper mapper;
    private final SessionService sessionService;
    private final SessionPlayerService sessionPlayerService;
    private final RobberBlockerRepository robberBlockerRepository;
    private final RobberMoveBlockerRepository robberMoveBlockerRepository;

    public GameService(SessionService sessionService, RobberBlockerRepository robberBlockerRepository, Mapper mapper, SessionPlayerService sessionPlayerService, RobberMoveBlockerRepository robberMoveBlockerRepository) {
        this.sessionService = sessionService;
        this.robberBlockerRepository = robberBlockerRepository;
        this.mapper = mapper;
        this.sessionPlayerService = sessionPlayerService;
        this.robberMoveBlockerRepository = robberMoveBlockerRepository;
    }

    public static String generateRandomName(){
        int index = rand.nextInt(names.length);
        return names[index];
    }

    public Boolean activateRobber(Long sessionId, SessionPlayer sessionPlayer) {
        List<SessionPlayer> players = sessionService.getPlayers(sessionId);
        if (!players.contains(sessionPlayer)) {
            return false;
        }
        for (SessionPlayer player : players) {
            if (player.getNumberOfResources() > 7){
                int amount = (int) (player.getNumberOfResources() / 2.0);
                robberBlockerRepository.saveAndFlush(new RobberBlocker(player, amount));
            }
        }
        //Find robber position
        robberMoveBlockerRepository.saveAndFlush(new RobberMoveBlocker(sessionPlayer, 0, 0));
        return true;
    }

    public Optional<RobberBlocker> findDebtByUserId(Long userId) {
        Optional<SessionPlayer> player = sessionPlayerService.findCurrentSessionPlayerByUserId(userId);
        if (player.isPresent()) {
            Optional<RobberBlocker> debt = robberBlockerRepository.findBySessionPlayerId(player.get().getId());
            if (debt.isPresent()) {
                return debt;
            }
        }
        return Optional.empty();
    }

    public Boolean settleDebtByUserId(RobberBlocker debt, Long userId, ResourceGroup resourceGroup) {
        Optional<SessionPlayer> player = sessionPlayerService.findCurrentSessionPlayerByUserId(userId);
        if (player.isPresent() && subtractResources(player.get(), resourceGroup)) {
            robberBlockerRepository.delete(debt);
            return true;
        }
        return false;
    }

    public Boolean subtractResources(SessionPlayer player, ResourceGroup resourceGroup) {
        ResourceGroup playerResources = mapper.mapSessionPlayerToResource(player);
        if (playerResources.compareTo(resourceGroup) < 0) {
            return false;
        }
        playerResources.subtractResources(resourceGroup);
        player.setResources(playerResources);
        sessionPlayerService.updateSessionPlayer(player);
        return true;
    }
}
