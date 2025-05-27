package com.catan.catanbackend.service;

import com.catan.catanbackend.model.*;
import com.catan.catanbackend.model.helper.DevCardType;
import com.catan.catanbackend.repository.DevCardRepository;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.util.*;

@Service
@Transactional
public class DevCardService {
    private final DevCardRepository devCardRepo;
    private final SessionPlayerService playerService;
    private final ResourceService resourceService;

    public DevCardService(DevCardRepository devCardRepo,
                          SessionPlayerService playerService,
                          ResourceService resourceService) {
        this.devCardRepo = devCardRepo;
        this.playerService = playerService;
        this.resourceService = resourceService;
    }

    public void deleteAll() {
        devCardRepo.deleteAll();
    }

    public Optional<DevCard> findDevCardById(Long id) {
        return devCardRepo.findById(id);
    }

    public List<DevCard> getAllDevCardsBySessionId(Long sessionId) {
        return devCardRepo.findBySessionId(sessionId);
    }

    /** Initialize a fresh shuffled deck for each new session */
    public void initDeckForSession(Session session) {
        List<DevCard> deck = new ArrayList<>();
        // according to standard counts:
        for (int i = 0; i < 14; i++) deck.add(new DevCard(DevCardType.KNIGHT, session));
        for (int i = 0; i < 5;  i++) deck.add(new DevCard(DevCardType.VICTORY_POINT, session));
        for (int i = 0; i < 2;  i++) deck.add(new DevCard(DevCardType.ROAD_BUILDING, session));
        for (int i = 0; i < 2;  i++) deck.add(new DevCard(DevCardType.YEAR_OF_PLENTY, session));
        Collections.shuffle(deck);

        devCardRepo.saveAll(deck);
    }

    /** Player buys one: checks resources, subtracts, draws top card */
    public DevCard buyDevCard(Long sessionPlayerId) {
        SessionPlayer me = playerService.findById(sessionPlayerId)
                .orElseThrow(() -> new IllegalArgumentException("Not in a session"));
        // cost: 1 ore, 1 grain, 1 sheep
        ResourceGroup cost = new ResourceGroup();
        cost.setOre(1); cost.setRice(1); cost.setSheep(1);

        boolean paid = resourceService.subtractResources(me, cost);
        if (!paid) throw new IllegalArgumentException("Insufficient resources for Dev Card");

        // draw top card
        List<DevCard> deck = devCardRepo.findByOwnerIsNullOrderById();
        if (deck.isEmpty()) throw new IllegalStateException("No Dev Cards left");
        DevCard card = deck.get(0);
        card.setOwner(me);
        // only playable starting next turn:
        card.setPlayable(false);
        return devCardRepo.save(card);
    }

    /** After a turn ends, mark all newly bought as playable */
    public void enablePlayable(Long sessionId) {
        // find all cards in session just bought: owner!=null && playable==false
        List<DevCard> justBought = devCardRepo.findBySessionId(sessionId).stream()
                .filter(c -> c.getOwner() != null &&  !c.isPlayable())
                .toList();
        justBought.forEach(c -> c.setPlayable(true));
        devCardRepo.saveAll(justBought);
    }

    /** List a player’s current dev cards */
    public List<DevCard> getPlayerCards(Long playerId) {
        SessionPlayer sp = playerService.findById(playerId)
                .orElseThrow(() -> new IllegalArgumentException("Player not found"));
        return devCardRepo.findByOwner(sp);
    }

    /** Play/activate one card (e.g. knight moves robber) */
    public DevCard useCard(Long cardId, Long userId) {
        DevCard card = devCardRepo.findById(cardId)
                .orElseThrow(() -> new IllegalArgumentException("Card not found"));
        if (!card.getOwner().getId().equals(userId))
            throw new IllegalArgumentException("You don’t own that card");
        if (!card.isPlayable())
            throw new IllegalArgumentException("Card not yet playable");
        if (card.isUsed())
            throw new IllegalArgumentException("Card already used");

        card.setUsed(true);
        return devCardRepo.save(card);
    }

    public DevCard saveCard(DevCard card) {
        return devCardRepo.save(card);
    }
}
