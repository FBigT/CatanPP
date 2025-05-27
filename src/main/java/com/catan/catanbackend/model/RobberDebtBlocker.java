package com.catan.catanbackend.model;

import com.fasterxml.jackson.annotation.JsonIgnore;
import com.fasterxml.jackson.annotation.JsonIgnoreProperties;
import com.fasterxml.jackson.annotation.JsonProperty;
import jakarta.persistence.*;
import jakarta.validation.constraints.NotNull;
import lombok.Data;
import lombok.NoArgsConstructor;
import org.hibernate.annotations.ColumnDefault;

@Data
@Entity
@Table(name = "robber_blockers")
@NoArgsConstructor
@JsonIgnoreProperties({ "hibernateLazyInitializer", "handler" })
public class RobberDebtBlocker {
    public RobberDebtBlocker(SessionPlayer sessionPlayer, Integer amount) {
        this.sessionPlayer = sessionPlayer;
        this.amount = amount;
    }

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    @Column(name = "id", nullable = false)
    private Long id;

    @JsonIgnore
    @NotNull
    @ManyToOne(fetch = FetchType.LAZY, optional = false)
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
    @ColumnDefault("0")
    @Column(name = "amount", nullable = false)
    private Integer amount;

}