package com.catan.catanbackend.service;

import com.catan.catanbackend.model.ResourceGroup;
import com.catan.catanbackend.model.SessionPlayer;
import com.catan.catanbackend.repository.SessionPlayerRepository;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.util.List;

@Service
@Transactional
public class TradeService {

    private final SessionPlayerRepository playerRepo;

    public TradeService(SessionPlayerRepository playerRepo) {
        this.playerRepo = playerRepo;
    }

    public void tradeBetweenPlayers(Long sessionId,
                                    String fromUser,
                                    String toUser,
                                    ResourceGroup offered,
                                    ResourceGroup requested) {
        SessionPlayer pFrom = findActivePlayer(sessionId, fromUser);
        SessionPlayer pTo   = findActivePlayer(sessionId, toUser);

        if (!hasEnough(pFrom, offered)) {
            throw new IllegalArgumentException(fromUser + " lacks offered resources.");
        }
        if (!hasEnough(pTo, requested)) {
            throw new IllegalArgumentException(toUser + " lacks requested resources.");
        }

        applyChange(pFrom, offered,   false);
        applyChange(pFrom, requested,  true);
        applyChange(pTo,   requested,  false);
        applyChange(pTo,   offered,    true);


        playerRepo.saveAll(List.of(pFrom, pTo));
    }

    public void tradeWithBank(Long sessionId,
                              String fromUser,
                              ResourceGroup offered,
                              ResourceGroup requested,
                              String portType,
                              int portRatio) {
        SessionPlayer pFrom = findActivePlayer(sessionId, fromUser);

        if (!hasEnough(pFrom, offered)) {
            throw new IllegalArgumentException(fromUser + " lacks offered resources.");
        }

        int ratio = determineRatio(offered, portType, portRatio);
        if (offered.getSum() / ratio != requested.getSum()) {
            throw new IllegalArgumentException("Invalid trade ratio.");
        }

        applyChange(pFrom, offered,   false);
        applyChange(pFrom, requested, true);
        playerRepo.save(pFrom);
    }


    private SessionPlayer findActivePlayer(Long sessionId, String username) {
        return playerRepo.findAll().stream()
                .filter(p -> p.getSession().getId().equals(sessionId)
                        && p.getActive()
                        && p.getUser() != null
                        && username.equals(p.getUser().getUsername()))
                .findFirst()
                .orElseThrow(() -> new IllegalArgumentException(
                        "No active player in session " + sessionId + " for user: " + username));
    }

    private boolean hasEnough(SessionPlayer p, ResourceGroup need) {
        return p.getBrick()   >= need.getBrick()
                && p.getCrystal() >= need.getCrystal()
                && p.getOre()     >= need.getOre()
                && p.getRice()    >= need.getRice()
                && p.getSheep()   >= need.getSheep()
                && p.getSilver()  >= need.getSilver()
                && p.getGold()    >= need.getGold()
                && p.getWood()    >= need.getWood();
    }

    private void applyChange(SessionPlayer p, ResourceGroup delta, boolean add) {
        int sign = add ? +1 : -1;
        p.setBrick(   p.getBrick()   + sign * delta.getBrick());
        p.setCrystal( p.getCrystal() + sign * delta.getCrystal());
        p.setOre(     p.getOre()     + sign * delta.getOre());
        p.setRice(    p.getRice()    + sign * delta.getRice());
        p.setSheep(   p.getSheep()   + sign * delta.getSheep());
        p.setSilver(  p.getSilver()  + sign * delta.getSilver());
        p.setGold(    p.getGold()    + sign * delta.getGold());
        p.setWood(    p.getWood()    + sign * delta.getWood());
    }

    private int determineRatio(ResourceGroup offered, String portType, int defaultRatio) {
        if (portType == null || portType.isEmpty()) {
            return defaultRatio;
        }
        int total = offered.getSum();
        switch (portType.toLowerCase()) {
            case "generic":
                return defaultRatio;
            case "brick":
                if (offered.getBrick().equals(total)) return defaultRatio;
                break;
            case "crystal":
                if (offered.getCrystal().equals(total)) return defaultRatio;
                break;
            case "ore":
                if (offered.getOre().equals(total)) return defaultRatio;
                break;
            case "rice":
                if (offered.getRice().equals(total)) return defaultRatio;
                break;
            case "sheep":
                if (offered.getSheep().equals(total)) return defaultRatio;
                break;
            case "silver":
                if (offered.getSilver().equals(total)) return defaultRatio;
                break;
            case "gold":
                if (offered.getGold().equals(total)) return defaultRatio;
                break;
            case "wood":
                if (offered.getWood().equals(total)) return defaultRatio;
                break;
        }
        return 4;
    }
}
