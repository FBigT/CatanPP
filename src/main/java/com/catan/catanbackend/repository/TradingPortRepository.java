package com.catan.catanbackend.repository;

import com.catan.catanbackend.model.PlayerProfile;
import com.catan.catanbackend.model.TradingPort;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import java.util.List;

@Repository
public interface TradingPortRepository extends JpaRepository<TradingPort, Long> {
    List<TradingPort> findAllByOwner(PlayerProfile owner);
}
