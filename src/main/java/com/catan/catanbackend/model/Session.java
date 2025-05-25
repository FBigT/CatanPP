package com.catan.catanbackend.model;

import jakarta.persistence.*;
import jakarta.validation.constraints.NotNull;
import lombok.Data;
import lombok.EqualsAndHashCode;
import lombok.NoArgsConstructor;
import lombok.ToString;
import org.hibernate.annotations.ColumnDefault;

import java.time.OffsetDateTime;

@Data
@Entity
@Table(name = "sessions")
@NoArgsConstructor
public class Session {
    public Session(User host, int maxPlayers){
        this(host, maxPlayers, 10);
    }

    public Session(User host, int maxPlayers, int victoryCondition){
        this.host = host;
        this.maxPlayers = maxPlayers;
        startedAt = OffsetDateTime.now();
        active = true;
        turnNumber = 0;
        victoryPointsCondition = victoryCondition;
        mapGenerated = false;
    }

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    @Column(name = "id", nullable = false)
    private Long id;

    @ToString.Exclude @EqualsAndHashCode.Exclude
    @NotNull
    @ManyToOne(fetch = FetchType.EAGER, optional = false)
    @JoinColumn(name = "host_id", nullable = false)
    private User host;

    @NotNull
    @ColumnDefault("true")
    @Column(name = "active", nullable = false)
    private Boolean active;

    @NotNull
    @Column(name = "started_at", nullable = false)
    private OffsetDateTime startedAt;

    @NotNull
    @Column(name = "turn_number", nullable = false)
    private Integer turnNumber;

    @NotNull
    @ColumnDefault("4")
    @Column(name = "max_players", nullable = false)
    private Integer maxPlayers;

    @ToString.Exclude @EqualsAndHashCode.Exclude
    @ManyToOne
    @JoinColumn(name = "current_player_id")
    private SessionPlayer currentPlayer;

    @Column(name = "victory_points_condition")
    private Integer victoryPointsCondition = 10;

    @Column(name = "map_generated")
    private Boolean mapGenerated;
}