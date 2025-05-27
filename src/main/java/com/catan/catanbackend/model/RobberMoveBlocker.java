package com.catan.catanbackend.model;

import com.fasterxml.jackson.annotation.JsonIgnore;
import com.fasterxml.jackson.annotation.JsonIgnoreProperties;
import com.fasterxml.jackson.annotation.JsonProperty;
import jakarta.persistence.*;
import jakarta.validation.constraints.NotNull;
import lombok.*;

@Data
@NoArgsConstructor
@AllArgsConstructor
@Entity
@Table(name = "robber_move_blockers")
@JsonIgnoreProperties({ "hibernateLazyInitializer", "handler" })
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

    @JsonIgnore
    @NotNull
    @OneToOne(fetch = FetchType.LAZY, optional = false)
    @JoinColumn(name = "session_player_id", nullable = false)
    private SessionPlayer sessionPlayer;

    @JsonProperty("sessionPlayerId")
    private Long extractSessionPlayerId() {
        if(sessionPlayer == null){
            return null;
        }
        return sessionPlayer.getId();
    }

    @NotNull
    @Column(name = "starting_robber_x", nullable = false)
    private Integer startingRobberX;

    @NotNull
    @Column(name = "starting_robber_y", nullable = false)
    private Integer startingRobberY;

}