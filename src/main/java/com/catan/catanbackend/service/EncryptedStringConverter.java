package com.catan.catanbackend.service;


import jakarta.persistence.AttributeConverter;
import jakarta.persistence.Converter;

@Converter
public class EncryptedStringConverter implements AttributeConverter<String, String> {

    @Override
    public String convertToDatabaseColumn(String attribute) {
        try {
            return attribute == null ? null : EncryptionUtils.encrypt(attribute);
        } catch (Exception e) {
            throw new RuntimeException("Encryption failed", e);
        }
    }

    @Override
    public String convertToEntityAttribute(String dbData) {
        try {
            return dbData == null ? null : EncryptionUtils.decrypt(dbData);
        } catch (Exception e) {
            return dbData; // fallback in case of decryption failure
        }
    }
}
