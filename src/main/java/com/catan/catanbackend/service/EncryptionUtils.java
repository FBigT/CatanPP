package com.catan.catanbackend.service;

import org.springframework.stereotype.Service;

import javax.crypto.Cipher;
import javax.crypto.spec.IvParameterSpec;
import javax.crypto.spec.SecretKeySpec;
import java.nio.charset.StandardCharsets;
import java.util.Base64;

@Service
public class EncryptionUtils {
    private static final byte[] KEY = "12345678901234567890123456789012".getBytes(StandardCharsets.UTF_8);

    public String encrypt(String plainText) throws Exception {
        Cipher cipher = Cipher.getInstance("AES/CBC/PKCS5Padding");

        // Generate random 16-byte IV
        byte[] iv = new byte[16];
        java.security.SecureRandom random = new java.security.SecureRandom();
        random.nextBytes(iv);

        IvParameterSpec ivSpec = new IvParameterSpec(iv);
        SecretKeySpec keySpec = new SecretKeySpec(KEY, "AES");

        cipher.init(Cipher.ENCRYPT_MODE, keySpec, ivSpec);
        byte[] encrypted = cipher.doFinal(plainText.getBytes(StandardCharsets.UTF_8));

        // Prepend IV to ciphertext
        byte[] ivAndEncrypted = new byte[iv.length + encrypted.length];
        System.arraycopy(iv, 0, ivAndEncrypted, 0, iv.length);
        System.arraycopy(encrypted, 0, ivAndEncrypted, iv.length, encrypted.length);

        return Base64.getEncoder().encodeToString(ivAndEncrypted);
    }

    public String decrypt(String base64CipherText) throws Exception {
        byte[] ivAndEncrypted = Base64.getDecoder().decode(base64CipherText);

        // Extract IV (first 16 bytes)
        byte[] iv = new byte[16];
        System.arraycopy(ivAndEncrypted, 0, iv, 0, 16);

        // Extract ciphertext
        byte[] encrypted = new byte[ivAndEncrypted.length - 16];
        System.arraycopy(ivAndEncrypted, 16, encrypted, 0, encrypted.length);

        Cipher cipher = Cipher.getInstance("AES/CBC/PKCS5Padding");
        IvParameterSpec ivSpec = new IvParameterSpec(iv);
        SecretKeySpec keySpec = new SecretKeySpec(KEY, "AES");

        cipher.init(Cipher.DECRYPT_MODE, keySpec, ivSpec);
        byte[] decrypted = cipher.doFinal(encrypted);

        return new String(decrypted, StandardCharsets.UTF_8);
    }
}
