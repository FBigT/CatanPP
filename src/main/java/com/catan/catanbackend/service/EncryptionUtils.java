package com.catan.catanbackend.service;

import com.catan.catanbackend.model.dto.EncryptedMessage;
import com.catan.catanbackend.model.dto.EncryptedMessageWithKey;
import org.springframework.stereotype.Service;

import javax.crypto.Cipher;
import javax.crypto.KeyGenerator;
import javax.crypto.SecretKey;
import javax.crypto.spec.IvParameterSpec;
import javax.crypto.spec.SecretKeySpec;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.security.KeyFactory;
import java.security.PrivateKey;
import java.security.PublicKey;
import java.security.SecureRandom;
import java.security.spec.PKCS8EncodedKeySpec;
import java.security.spec.X509EncodedKeySpec;
import java.util.Base64;

@Service
public class EncryptionUtils {

    private final PrivateKey privateKey;
    private final PublicKey testPublicKey;

    public EncryptionUtils() throws Exception {
        byte[] keyBytes = Files.readAllBytes(Paths.get("src/main/resources/private.der"));
        PKCS8EncodedKeySpec spec = new PKCS8EncodedKeySpec(keyBytes);
        KeyFactory kf = KeyFactory.getInstance("RSA");
        privateKey = kf.generatePrivate(spec);
        testPublicKey = loadPublicKeyFromPEM();
    }

    // Injectable constructor for tests
    public EncryptionUtils(byte[] privateKeyBytes) throws Exception {
        PKCS8EncodedKeySpec spec = new PKCS8EncodedKeySpec(privateKeyBytes);
        KeyFactory kf = KeyFactory.getInstance("RSA");
        this.privateKey = kf.generatePrivate(spec);
        testPublicKey = loadPublicKeyFromPEM();
    }

    public PublicKey loadPublicKeyFromPEM() throws Exception {
        String pem = Files.readString(Path.of("src/test/resources/public.pem"));
        String publicKeyPEM = pem.replace("-----BEGIN PUBLIC KEY-----", "")
                .replace("-----END PUBLIC KEY-----", "")
                .replaceAll("\\s", "");

        byte[] encoded = Base64.getDecoder().decode(publicKeyPEM);
        X509EncodedKeySpec keySpec = new X509EncodedKeySpec(encoded);
        KeyFactory keyFactory = KeyFactory.getInstance("RSA");
        return keyFactory.generatePublic(keySpec);
    }

    public byte[] encryptAESKeyWithRSA(byte[] aesKey, PublicKey publicKey) throws Exception {
        Cipher cipher = Cipher.getInstance("RSA");
        cipher.init(Cipher.ENCRYPT_MODE, publicKey);
        return cipher.doFinal(aesKey);
    }

    /**
     * Decrypt AES key sent from client.
     */
    public byte[] decryptAESKey(String encryptedKeyB64) throws Exception {
        Cipher rsaCipher = Cipher.getInstance("RSA");
        rsaCipher.init(Cipher.DECRYPT_MODE, privateKey);
        return rsaCipher.doFinal(Base64.getDecoder().decode(encryptedKeyB64));
    }

    /**
     * Decrypt AES-encrypted payload.
     */
    public String decryptPayload(String encryptedPayloadB64, byte[] aesKey) throws Exception {
        byte[] ivAndEncryptedPayload = Base64.getDecoder().decode(encryptedPayloadB64);

        // Extract IV (first 16 bytes)
        byte[] iv = new byte[16];
        byte[] encryptedPayload = new byte[ivAndEncryptedPayload.length - 16];
        System.arraycopy(ivAndEncryptedPayload, 0, iv, 0, 16);
        System.arraycopy(ivAndEncryptedPayload, 16, encryptedPayload, 0, encryptedPayload.length);

        // Decrypt using AES/CBC/PKCS5Padding
        SecretKeySpec aesKeySpec = new SecretKeySpec(aesKey, "AES");
        IvParameterSpec ivSpec = new IvParameterSpec(iv);
        Cipher aesCipher = Cipher.getInstance("AES/CBC/PKCS5Padding");
        aesCipher.init(Cipher.DECRYPT_MODE, aesKeySpec, ivSpec);

        byte[] decrypted = aesCipher.doFinal(encryptedPayload);
        return new String(decrypted, StandardCharsets.UTF_8);
    }

    /**
     * Encrypt a server response using the AES key.
     */
    public String encryptResponse(String responseJson, byte[] aesKey) throws Exception {
        SecretKeySpec aesKeySpec = new SecretKeySpec(aesKey, "AES");
        Cipher aesCipher = Cipher.getInstance("AES/CBC/PKCS5Padding");

        // Generate random IV
        byte[] iv = new byte[16];
        SecureRandom random = new SecureRandom();
        random.nextBytes(iv);

        IvParameterSpec ivSpec = new IvParameterSpec(iv);
        aesCipher.init(Cipher.ENCRYPT_MODE, aesKeySpec, ivSpec);

        byte[] encrypted = aesCipher.doFinal(responseJson.getBytes(StandardCharsets.UTF_8));

        // Prepend IV to ciphertext
        byte[] ivAndEncrypted = new byte[iv.length + encrypted.length];
        System.arraycopy(iv, 0, ivAndEncrypted, 0, iv.length);
        System.arraycopy(encrypted, 0, ivAndEncrypted, iv.length, encrypted.length);

        return Base64.getEncoder().encodeToString(ivAndEncrypted);
    }

    /**
     * Older methods for database storage
     */
    private static final byte[] KEY = "12345678901234567890123456789012".getBytes(StandardCharsets.UTF_8);
    private static final byte[] FIXED_IV = new byte[16]; // 16 bytes of zeros
    public static String encrypt(String plainText) throws Exception {
        Cipher cipher = Cipher.getInstance("AES/CBC/PKCS5Padding");

        IvParameterSpec ivSpec = new IvParameterSpec(FIXED_IV); // Fixed IV
        SecretKeySpec keySpec = new SecretKeySpec(KEY, "AES");

        cipher.init(Cipher.ENCRYPT_MODE, keySpec, ivSpec);
        byte[] encrypted = cipher.doFinal(plainText.getBytes(StandardCharsets.UTF_8));

        return Base64.getEncoder().encodeToString(encrypted); // No need to prepend IV
    }

    public static String decrypt(String base64CipherText) throws Exception {
        byte[] encrypted = Base64.getDecoder().decode(base64CipherText);

        Cipher cipher = Cipher.getInstance("AES/CBC/PKCS5Padding");
        IvParameterSpec ivSpec = new IvParameterSpec(FIXED_IV);
        SecretKeySpec keySpec = new SecretKeySpec(KEY, "AES");

        cipher.init(Cipher.DECRYPT_MODE, keySpec, ivSpec);
        byte[] decrypted = cipher.doFinal(encrypted);

        return new String(decrypted, StandardCharsets.UTF_8);
    }

    // Simulate frontend encryption for integration tests
    public EncryptedMessageWithKey simulateFrontendEncryption(String jsonPayload) throws Exception {
        KeyGenerator keyGen = KeyGenerator.getInstance("AES");
        keyGen.init(256);
        SecretKey aesKey = keyGen.generateKey();

        String encryptedPayload = encryptResponse(jsonPayload, aesKey.getEncoded());
        byte[] encryptedKey = encryptAESKeyWithRSA(aesKey.getEncoded(), testPublicKey);

        return new EncryptedMessageWithKey(new EncryptedMessage(Base64.getEncoder().encodeToString(encryptedKey), encryptedPayload), aesKey.getEncoded());
    }
}
