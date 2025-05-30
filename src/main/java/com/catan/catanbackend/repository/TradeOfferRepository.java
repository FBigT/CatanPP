package com.catan.catanbackend.repository;

import com.catan.catanbackend.model.TradeOffer;
import org.springframework.data.jpa.repository.JpaRepository;

import java.util.Optional;

public interface TradeOfferRepository extends JpaRepository<TradeOffer, Long> {
    Optional<TradeOffer> findByToPlayerNameAndFromPlayerName(String toPlayerName, String fromPlayerName);
}
