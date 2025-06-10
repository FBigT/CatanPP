package com.catan.catanbackend.model;


import com.catan.catanbackend.service.EncryptedStringConverter;
import com.catan.catanbackend.service.EncryptionUtils;
import com.fasterxml.jackson.annotation.JsonIgnore;
import jakarta.persistence.*;
import lombok.*;

import java.time.LocalDateTime;
import java.util.UUID;

@Entity
@Data
@Builder
@NoArgsConstructor()
@AllArgsConstructor
@Table(name = "users")
public class User {
    @Id
    @Column(name = "id", unique = true, nullable = false)
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    @Setter(AccessLevel.NONE)
    private Long id;

    @Column(name = "username", nullable = false, unique = true)
    @Convert(converter = EncryptedStringConverter.class)
    private String username;
    @JsonIgnore
    @Column(name = "password_hash")
    private String passwordHash;
    @Column(name = "is_guest")
    private Boolean isGuest;
    @Column(name = "active", nullable = false)
    private Boolean active;
    @Column(name = "created_at", nullable = false)
    private LocalDateTime createdAt;
    @Column(name = "email", nullable = true, unique = false)
    @Convert(converter = EncryptedStringConverter.class)
    private String email;

    @ToString.Exclude
    @OneToOne(mappedBy = "user", cascade = CascadeType.ALL)
    @PrimaryKeyJoinColumn
    private PlayerProfile playerProfile;

    public void anonymize() {
        this.username = "anon_" + UUID.randomUUID();
        this.passwordHash = null;
        this.email = null;
        this.isGuest = true;
        this.active = false;
        this.playerProfile = null;
    }
}
