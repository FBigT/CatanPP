package com.catan.catanbackend.service;

import com.catan.catanbackend.model.PlayerProfile;
import com.catan.catanbackend.model.ResourceGroup;
import com.catan.catanbackend.repository.PlayerProfileRepository;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

@Service
@Transactional
public class TradeService {

    private final PlayerProfileRepository profileRepo;

    public TradeService(PlayerProfileRepository profileRepo) {
        this.profileRepo = profileRepo;
    }

    public void tradeBetweenPlayers(String fromUser, String toUser, ResourceGroup offered, ResourceGroup requested) {
        PlayerProfile pFrom = profileRepo.findByUserUsername(fromUser)
                .orElseThrow(() -> new IllegalArgumentException("No such user: " + fromUser));
        PlayerProfile pTo = profileRepo.findByUserUsername(toUser)
                .orElseThrow(() -> new IllegalArgumentException("No such user: " + toUser));

        if (!hasEnough(pFrom.getResources(), offered)) {
            throw new IllegalArgumentException(fromUser + " lacks offered resources.");
        }
        if (!hasEnough(pTo.getResources(), requested)) {
            throw new IllegalArgumentException(toUser + " lacks requested resources.");
        }

        subtractResources(pFrom.getResources(), offered);
        addResources(pFrom.getResources(), requested);
        subtractResources(pTo.getResources(), requested);
        addResources(pTo.getResources(), offered);

        profileRepo.save(pFrom);
        profileRepo.save(pTo);
    }

    public void tradeWithBank(String fromUser, ResourceGroup offered, ResourceGroup requested, String portType, int portRatio) {
        PlayerProfile pFrom = profileRepo.findByUserUsername(fromUser)
                .orElseThrow(() -> new IllegalArgumentException("No such user: " + fromUser));

        if (!hasEnough(pFrom.getResources(), offered)) {
            throw new IllegalArgumentException("Not enough resources.");
        }

        int ratio = determineRatio(offered, portType, portRatio);
        int offeredSum = offered.getSum();
        int requestedSum = requested.getSum();

        if (offeredSum / ratio != requestedSum) {
            throw new IllegalArgumentException("Invalid ratio.");
        }

        subtractResources(pFrom.getResources(), offered);
        addResources(pFrom.getResources(), requested);
        profileRepo.save(pFrom);
    }

    private boolean hasEnough(ResourceGroup have, ResourceGroup need) {
        return have.getLumber()   >= need.getLumber()
                && have.getWool()     >= need.getWool()
                && have.getOre()      >= need.getOre()
                && have.getGrain()    >= need.getGrain()
                && have.getBricks()   >= need.getBricks()
                && have.getSilver()   >= need.getSilver()
                && have.getGold()     >= need.getGold()
                && have.getObsidian() >= need.getObsidian();
    }

    private void subtractResources(ResourceGroup from, ResourceGroup what) {
        from.setLumber(from.getLumber()       - what.getLumber());
        from.setWool(from.getWool()           - what.getWool());
        from.setOre(from.getOre()             - what.getOre());
        from.setGrain(from.getGrain()         - what.getGrain());
        from.setBricks(from.getBricks()       - what.getBricks());
        from.setSilver(from.getSilver()       - what.getSilver());
        from.setGold(from.getGold()           - what.getGold());
        from.setObsidian(from.getObsidian()   - what.getObsidian());
    }

    private void addResources(ResourceGroup to, ResourceGroup what) {
        to.setLumber(to.getLumber()       + what.getLumber());
        to.setWool(to.getWool()           + what.getWool());
        to.setOre(to.getOre()             + what.getOre());
        to.setGrain(to.getGrain()         + what.getGrain());
        to.setBricks(to.getBricks()       + what.getBricks());
        to.setSilver(to.getSilver()       + what.getSilver());
        to.setGold(to.getGold()           + what.getGold());
        to.setObsidian(to.getObsidian()   + what.getObsidian());
    }

    private int determineRatio(ResourceGroup offered, String portType, int portRatio) {
        int ratio = 4;
        if (portType == null || portType.isEmpty()) return ratio;
        if ("generic".equalsIgnoreCase(portType)) {
            ratio = portRatio;
        } else {
            int total = offered.getSum();
            switch (portType.toLowerCase()) {
                case "lumber":
                    if (offered.getLumber().equals(total)) ratio = portRatio;
                    break;
                case "wool":
                    if (offered.getWool().equals(total)) ratio = portRatio;
                    break;
                case "ore":
                    if (offered.getOre().equals(total)) ratio = portRatio;
                    break;
                case "grain":
                    if (offered.getGrain().equals(total)) ratio = portRatio;
                    break;
                case "bricks":
                    if (offered.getBricks().equals(total)) ratio = portRatio;
                    break;
                case "silver":
                    if (offered.getSilver().equals(total)) ratio = portRatio;
                    break;
                case "gold":
                    if (offered.getGold().equals(total)) ratio = portRatio;
                    break;
                case "obsidian":
                    if (offered.getObsidian().equals(total)) ratio = portRatio;
                    break;
            }
        }
        return ratio;
    }
}
