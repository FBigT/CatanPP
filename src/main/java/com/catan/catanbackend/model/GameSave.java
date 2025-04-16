package com.catan.catanbackend.model;

import jakarta.persistence.*;
import lombok.Getter;
import lombok.NoArgsConstructor;
import lombok.Setter;

import java.time.LocalDateTime;

@Entity
@Table(name = "game_saves")
@NoArgsConstructor
@Getter
@Setter
public class GameSave {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;

    @ManyToOne(fetch = FetchType.LAZY)
    @JoinColumn(name = "user_id")
    private User user;

    private String saveName;

    @Column(columnDefinition = "TEXT")
    private String gameStateJson;

    private LocalDateTime savedAt = LocalDateTime.now();
}
