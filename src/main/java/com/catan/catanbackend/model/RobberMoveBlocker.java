package com.catan.catanbackend.model;

import jakarta.persistence.*;
import jakarta.validation.constraints.NotNull;
import lombok.*;

@Data
@NoArgsConstructor
@AllArgsConstructor
@Entity
@Table(name = "robber_move_blockers")
public class RobberMoveBlocker {
    public RobberMoveBlocker(SessionPlayer sessionPlayer, Integer x, Integer y) {
        this.sessionPlayer = sessionPlayer;
        startingRobberX = x;
        startingRobberY = y;
    }

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    @Column(name = "id", nullable = false)
    private Long id;

    @NotNull
    @OneToOne(fetch = FetchType.LAZY, optional = false)
    @JoinColumn(name = "session_player_id", nullable = false)
    private SessionPlayer sessionPlayer;

    @NotNull
    @Column(name = "starting_robber_x", nullable = false)
    private Integer startingRobberX;

    @NotNull
    @Column(name = "starting_robber_y", nullable = false)
    private Integer startingRobberY;

}