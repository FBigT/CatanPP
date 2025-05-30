package com.catan.catanbackend.model;

import com.fasterxml.jackson.annotation.JsonIgnore;
import com.fasterxml.jackson.annotation.JsonIgnoreProperties;
import com.fasterxml.jackson.annotation.JsonProperty;
import jakarta.persistence.*;
import jakarta.validation.constraints.NotNull;
import lombok.Data;
import lombok.EqualsAndHashCode;
import lombok.NoArgsConstructor;
import lombok.ToString;
import org.hibernate.annotations.ColumnDefault;
import org.hibernate.annotations.CreationTimestamp;

import java.time.OffsetDateTime;

@Data
@Entity
@Table(name = "sessions")
@NoArgsConstructor
@JsonIgnoreProperties({ "hibernateLazyInitializer", "handler" })
public class Session {
    public Session(User host, int maxPlayers){
        this(host, maxPlayers, 10);
    }

    public Session(User host, int maxPlayers, int victoryCondition){
        this.host = host;
        this.maxPlayers = maxPlayers;
        startedAt = OffsetDateTime.now();
        active = true;
        turnNumber = 1;
        victoryPointsCondition = victoryCondition;
        mapGenerated = false;
        inSetup = false;
    }

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    @Column(name = "id", nullable = false)
    private Long id;

    @JsonIgnore
    @ToString.Exclude @EqualsAndHashCode.Exclude
    @NotNull
    @ManyToOne(fetch = FetchType.EAGER, optional = false)
    @JoinColumn(name = "host_id", nullable = false)
    private User host;

    @JsonProperty("hostId")
    public Long extractHostId() {
        return host.getId();
    }

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

    @JsonIgnore
    @ToString.Exclude @EqualsAndHashCode.Exclude
    @ManyToOne(fetch = FetchType.EAGER)
    @JoinColumn(name = "current_player_id")
    private SessionPlayer currentPlayer;

    @JsonProperty("currentPlayerId")
    public Long extractCurrentPlayerId() {
        if (currentPlayer == null) {
            return null;
        }
        return currentPlayer.getId();
    }

    @Column(name = "victory_points_condition")
    private Integer victoryPointsCondition = 10;

    @Column(name = "map_generated")
    private Boolean mapGenerated;

    @Column(name = "in_setup")
    private Boolean inSetup;

    @CreationTimestamp
    @Column(name = "created_at", updatable = false)
    private OffsetDateTime createdAt;
}