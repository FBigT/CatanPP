package com.catan.catanbackend.model;

import jakarta.persistence.*;
import jakarta.validation.constraints.NotNull;
import jakarta.validation.constraints.Size;
import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;
import org.hibernate.annotations.ColumnDefault;

import java.time.OffsetDateTime;

@Data
@NoArgsConstructor
@AllArgsConstructor
@Entity
@Table(name = "session_saves")
public class SessionSave {
    public SessionSave(String name, Session session, Integer turnNumber) {
        savedAt = OffsetDateTime.now();
        this.name = name;
        this.session = session;
        this.turnNumber = turnNumber;
    }

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    @Column(name = "id", nullable = false)
    private Long id;

    @Size(max = 255)
    @NotNull
    @Column(name = "name", nullable = false)
    private String name;

    @NotNull
    @ManyToOne(fetch = FetchType.LAZY, optional = false)
    @JoinColumn(name = "session_id", nullable = false)
    private Session session;

    @NotNull
    @ColumnDefault("0")
    @Column(name = "turn_number", nullable = false)
    private Integer turnNumber;

    @NotNull
    @Column(name = "saved_at", nullable = false)
    private OffsetDateTime savedAt;
}