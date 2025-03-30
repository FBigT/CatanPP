package com.catan.catanbackend.repository;

import com.catan.catanbackend.model.GuestKey;
import org.springframework.data.jpa.repository.JpaRepository;

import java.util.Optional;

public interface GuestKeyRepository extends JpaRepository<GuestKey, Long> {
    Optional<GuestKey> findGuestKeyByGuestId(Long id);
    Optional<GuestKey> findGuestKeyByKey(String key);
}
