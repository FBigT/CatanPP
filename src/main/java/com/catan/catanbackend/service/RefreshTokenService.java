package com.catan.catanbackend.service;

import com.catan.catanbackend.model.RefreshToken;
import com.catan.catanbackend.model.User;
import com.catan.catanbackend.repository.RefreshTokenRepository;
import org.springframework.stereotype.Service;

import java.sql.Ref;
import java.time.Instant;
import java.time.LocalDateTime;
import java.time.ZoneOffset;
import java.util.Optional;
import java.util.UUID;

@Service
public class RefreshTokenService {
    final RefreshTokenRepository refreshTokenRepository;

    public RefreshTokenService(RefreshTokenRepository refreshTokenRepository) {
        this.refreshTokenRepository = refreshTokenRepository;
    }

    public RefreshToken createIfNotExists(User user) {
        Optional<RefreshToken> existingToken = refreshTokenRepository.findByUserId(user.getId());
        if (existingToken.isPresent() && tokenIsValid(existingToken.get())) {
            return existingToken.get();
        }

        RefreshToken refreshToken = RefreshToken.builder()
                .user(user)
                .expireDate(Instant.now().plusSeconds(600))
                .build();

        do {
            refreshToken.setToken(UUID.randomUUID().toString());
        } while (refreshTokenRepository.findByToken(refreshToken.getToken()).isPresent());

        return refreshTokenRepository.saveAndFlush(refreshToken);
    }

    public Optional<RefreshToken> getRefreshTokenByToken(String token) {
        return refreshTokenRepository.findByToken(token);
    }

    public Optional<RefreshToken> getRefreshTokenByUserId(Long userId) {
        return refreshTokenRepository.findByUserId(userId);
    }

    public Boolean tokenIsValid(RefreshToken refreshToken) {
        if (refreshToken.getExpireDate().isAfter(Instant.now()) && refreshTokenRepository.findByToken(refreshToken.getToken()).isPresent()){
            return true;
        }
        refreshTokenRepository.delete(refreshToken);
        refreshTokenRepository.flush();
        return false;
    }

    public void purgeInvalidTokens(){
        for (RefreshToken refreshToken : refreshTokenRepository.findAll()) {
            if (refreshToken.getExpireDate().isBefore(LocalDateTime.now().toInstant(ZoneOffset.UTC))){
                refreshTokenRepository.delete(refreshToken);
                refreshTokenRepository.flush();
            }
        }
    }
}
