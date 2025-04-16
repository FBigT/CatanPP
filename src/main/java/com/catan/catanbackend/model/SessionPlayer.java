package com.catan.catanbackend.model;

import com.catan.catanbackend.service.GameService;
import jakarta.persistence.*;
import jakarta.validation.constraints.NotNull;
import jakarta.validation.constraints.Size;
import lombok.Data;
import lombok.NoArgsConstructor;
import org.hibernate.annotations.ColumnDefault;
import org.hibernate.annotations.OnDelete;
import org.hibernate.annotations.OnDeleteAction;

@Data
@Entity
@Table(name = "session_players")
@NoArgsConstructor
public class SessionPlayer {
    public SessionPlayer(Session session, User user) {
        this.session = session;
        this.user = user;
        active = true;
        isAi = false;
        name = user.getUsername();
    }

    public SessionPlayer(Session session) {
        this.session = session;
        active = true;
        isAi = true;
        name = GameService.generateRandomName();
    }

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    @Column(name = "id", nullable = false)
    private Long id;

    @NotNull
    @ManyToOne(fetch = FetchType.LAZY, optional = false)
    @OnDelete(action = OnDeleteAction.CASCADE)
    @JoinColumn(name = "session_id", nullable = false)
    private Session session;

    @ManyToOne(fetch = FetchType.LAZY)
    @JoinColumn(name = "user_id")
    private User user;

    @NotNull
    @ColumnDefault("0")
    @Column(name = "player_score", nullable = false)
    private Integer playerScore = 0;

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

    @NotNull
    @Column(name = "lumber", nullable = false)
    @ColumnDefault("0")
    private Integer lumber = 0;

    @NotNull
    @Column(name = "wool", nullable = false)
    @ColumnDefault("0")
    private Integer wool = 0;

    @NotNull
    @Column(name = "grain", nullable = false)
    @ColumnDefault("0")
    private Integer grain = 0;

    @NotNull
    @Column(name = "bricks", nullable = false)
    @ColumnDefault("0")
    private Integer bricks = 0;

    @NotNull
    @Column(name = "ore", nullable = false)
    @ColumnDefault("0")
    private Integer ore = 0;

    @NotNull
    @Column(name = "gold", nullable = false)
    @ColumnDefault("0")
    private Integer gold = 0;

    @NotNull
    @Column(name = "silver", nullable = false)
    @ColumnDefault("0")
    private Integer silver = 0;

    @NotNull
    @Column(name = "obsidian", nullable = false)
    @ColumnDefault("0")
    private Integer obsidian = 0;

    public Integer getNumberOfResources() {
        return obsidian + silver + gold + bricks + wool + grain + ore + lumber;
    }

    public void setResources(ResourceGroup resourceGroup) {
        wool = resourceGroup.getWool();
        grain = resourceGroup.getGrain();
        ore = resourceGroup.getOre();
        lumber = resourceGroup.getLumber();
        silver = resourceGroup.getSilver();
        gold = resourceGroup.getGold();
        bricks = resourceGroup.getBricks();
        obsidian = resourceGroup.getObsidian();
    }
}