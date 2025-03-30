package com.catan.catanbackend.model;

import jakarta.persistence.*;
import jakarta.validation.constraints.NotNull;
import lombok.Data;
import lombok.NoArgsConstructor;
import org.hibernate.annotations.ColumnDefault;

import java.time.OffsetDateTime;

@Data
@Entity
@Table(name = "sessions")
@NoArgsConstructor
public class Session {
    public Session(User host, int maxPlayers){
        this.host = host;
        this.maxPlayers = maxPlayers;
        startedAt = OffsetDateTime.now();
        active = true;
    }

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    @Column(name = "id", nullable = false)
    private Long id;

    @NotNull
    @ManyToOne(fetch = FetchType.LAZY, optional = false)
    @JoinColumn(name = "host_id", nullable = false)
    private User host;

    @NotNull
    @ColumnDefault("true")
    @Column(name = "active", nullable = false)
    private Boolean active;

    @NotNull
    @Column(name = "started_at", nullable = false)
    private OffsetDateTime startedAt = OffsetDateTime.now();

    @NotNull
    @ColumnDefault("4")
    @Column(name = "max_players", nullable = false)
    private Integer maxPlayers;

}