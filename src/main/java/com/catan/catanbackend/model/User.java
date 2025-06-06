package com.catan.catanbackend.model;


import com.fasterxml.jackson.annotation.JsonIgnore;
import jakarta.persistence.*;
import lombok.*;

import java.time.LocalDateTime;

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
    @Column(name = "email")
    private String email;

    @ToString.Exclude
    @OneToOne(mappedBy = "user", cascade = CascadeType.ALL)
    @PrimaryKeyJoinColumn
    private PlayerProfile playerProfile;

    @OneToOne(mappedBy = "user", cascade = CascadeType.ALL)
    private RefreshToken refreshToken;
}
