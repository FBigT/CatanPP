package com.catan.catanbackend.service;

import com.catan.catanbackend.model.GuestKey;
import com.catan.catanbackend.model.User;
import com.catan.catanbackend.repository.GuestKeyRepository;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.util.Optional;
import java.util.UUID;

@Service
@Transactional
public class GuestKeyService {
    private final GuestKeyRepository guestKeyRepository;

    public GuestKeyService(GuestKeyRepository guestKeyRepository) {
        this.guestKeyRepository = guestKeyRepository;
    }

    public GuestKey createGuestKey(User user) {
        if (Boolean.FALSE.equals(user.getIsGuest())){
            throw new RuntimeException("This user is not a guest");
        }
        if (guestKeyRepository.findGuestKeyByGuestId(user.getId()).isPresent()) {
            throw new RuntimeException("Guest already has a key");
        }
        GuestKey newGuestKey = new GuestKey();
        newGuestKey.setGuest(user);
        do {
            newGuestKey.setKey(UUID.randomUUID().toString().substring(0, 31));
        } while (guestKeyRepository.findGuestKeyByKey(newGuestKey.getKey()).isPresent());

        return guestKeyRepository.saveAndFlush(newGuestKey);
    }

    public Optional<GuestKey> findByKey(String key){
        return guestKeyRepository.findGuestKeyByKey(key);
    }
}
