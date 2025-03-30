package com.catan.catanbackend.model;

import jakarta.persistence.*;
import jakarta.validation.constraints.NotNull;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.time.OffsetDateTime;

@Data
@Entity
@Table(name = "session_records")
@NoArgsConstructor
public class SessionRecord {
    public SessionRecord(User winner, OffsetDateTime start, OffsetDateTime end) {
        this.winner = winner;
        endedAt = end;
        startedAt = start;
    }

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    @Column(name = "id", nullable = false)
    private Long id;

    @NotNull
    @ManyToOne(fetch = FetchType.LAZY, optional = false)
    @JoinColumn(name = "winner_id", nullable = false)
    private User winner;

    @NotNull
    @Column(name = "started_at", nullable = false)
    private OffsetDateTime startedAt;

    @NotNull
    @Column(name = "ended_at", nullable = false)
    private OffsetDateTime endedAt;

}