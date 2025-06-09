package com.catan.catanbackend.config;

import com.catan.catanbackend.service.EncryptionUtils;
import org.springframework.boot.test.context.TestConfiguration;
import org.springframework.context.annotation.Bean;

import java.nio.file.Files;
import java.nio.file.Paths;

@TestConfiguration
public class EncryptionTestConfig {

    @Bean
    public EncryptionUtils encryptionUtils() throws Exception {
        // Load or mock private key for tests
        byte[] testPrivateKeyBytes = Files.readAllBytes(Paths.get("src/test/resources/private.der"));
        return new EncryptionUtils(testPrivateKeyBytes);
    }
}