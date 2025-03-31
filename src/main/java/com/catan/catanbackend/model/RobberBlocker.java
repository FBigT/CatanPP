package com.catan.catanbackend.model;

import com.catan.catanbackend.repository.RobberBlockerRepository;
import jakarta.persistence.*;
import jakarta.validation.constraints.NotNull;
import lombok.Data;
import lombok.Getter;
import lombok.NoArgsConstructor;
import lombok.Setter;
import org.hibernate.annotations.ColumnDefault;

@Data
@Entity
@Table(name = "robber_blockers")
@NoArgsConstructor
public class RobberBlocker {
    public RobberBlocker(SessionPlayer sessionPlayer, Integer amount) {
        this.sessionPlayer = sessionPlayer;
        this.amount = amount;
    }

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    @Column(name = "id", nullable = false)
    private Long id;

    @NotNull
    @ManyToOne(fetch = FetchType.LAZY, optional = false)
    @JoinColumn(name = "session_player_id", nullable = false)
    private SessionPlayer sessionPlayer;

    @NotNull
    @ColumnDefault("0")
    @Column(name = "amount", nullable = false)
    private Integer amount;

}