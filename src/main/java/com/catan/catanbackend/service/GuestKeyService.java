package com.catan.catanbackend.service;

import com.catan.catanbackend.model.GuestKey;
import com.catan.catanbackend.model.User;
import com.catan.catanbackend.repository.GuestKeyRepository;
import org.springframework.security.crypto.bcrypt.BCryptPasswordEncoder;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.util.Optional;
import java.util.UUID;

@Service
@Transactional
public class GuestKeyService {
    private final GuestKeyRepository guestKeyRepository;
    final BCryptPasswordEncoder encoder = new BCryptPasswordEncoder();

    public GuestKeyService(GuestKeyRepository guestKeyRepository) {
        this.guestKeyRepository = guestKeyRepository;
    }

    public String createGuestKey(User user) {
        if (Boolean.FALSE.equals(user.getIsGuest())){
            throw new RuntimeException("This user is not a guest");
        }
        if (guestKeyRepository.findGuestKeyByGuestId(user.getId()).isPresent()) {
            throw new RuntimeException("Guest already has a key");
        }

        String newKey;
        do {
            newKey = UUID.randomUUID().toString().substring(0, 31);
        } while (guestKeyRepository.findGuestKeyByKey(newKey).isPresent());

        GuestKey guestKey = new GuestKey();
        guestKey.setKey(encoder.encode(newKey));
        guestKey.setGuest(user);
        guestKeyRepository.saveAndFlush(guestKey);
        return newKey;
    }

    public Optional<GuestKey> findByKey(String key){
        for (GuestKey guestKey : guestKeyRepository.findAll()) {
            if (encoder.matches(key, guestKey.getKey())) {
                return Optional.of(guestKey);
            }
        }

        return Optional.empty();
    }
}
