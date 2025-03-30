package com.catan.catanbackend.model;

import jakarta.persistence.*;
import lombok.Data;

@Entity
@Data
@Table(name = "guest_keys")
public class GuestKey {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    long id;
    @OneToOne
    @MapsId
    @JoinColumn(name = "guest_id")
    User guest;
    @Column(name = "key", nullable = false)
    String key;
}
