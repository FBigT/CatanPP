package com.catan.catanbackend.model;

import com.catan.catanbackend.service.GameService;
import jakarta.persistence.*;
import jakarta.validation.constraints.NotNull;
import jakarta.validation.constraints.Size;
import lombok.Data;
import lombok.NoArgsConstructor;
import org.hibernate.annotations.ColumnDefault;

@Data
@Entity
@NoArgsConstructor
@Table(name = "session_players")
public class SessionPlayer {
    public SessionPlayer(Session session, User user) {
        this.session = session;
        this.user = user;
        active = true;
        isAi = false;
        name = user.getUsername();
        playerScore = 0;
    }

    public SessionPlayer(Session session) {
        this.session = session;
        active = true;
        isAi = true;
        name = GameService.generateRandomName();
        playerScore = 0;
    }

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    @Column(name = "id", nullable = false)
    private Long id;

    @NotNull
    @ManyToOne(fetch = FetchType.LAZY, optional = false)
    @JoinColumn(name = "session_id", nullable = false)
    private Session session;

    @ManyToOne(fetch = FetchType.LAZY)
    @JoinColumn(name = "user_id")
    private User user;

    @NotNull
    @ColumnDefault("0")
    @Column(name = "player_score", nullable = false)
    private Integer playerScore;

    @NotNull
    @Column(name = "active", nullable = false)
    private Boolean active;

    @NotNull
    @ColumnDefault("false")
    @Column(name = "is_ai", nullable = false)
    private Boolean isAi;

    @Size(max = 255)
    @NotNull
    @Column(name = "name", nullable = false)
    private String name;

}