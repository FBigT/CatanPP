package com.catan.catanbackend.repository;


import com.catan.catanbackend.model.TradingPort;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

@Repository
public interface TradingPortRepository extends JpaRepository<TradingPort, Long> {
}
