package com.catan.catanbackend.model.dto;

import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

@Data
@NoArgsConstructor
@AllArgsConstructor
public class EncryptedMessageWithKey {
    private EncryptedMessage encryptedMessage;
    private byte[] key;
}
