package com.catan.catanbackend.model;

import com.catan.catanbackend.service.GameService;
import com.fasterxml.jackson.annotation.JsonIgnoreProperties;
import jakarta.persistence.*;
import jakarta.validation.constraints.NotNull;
import jakarta.validation.constraints.Size;
import lombok.Data;
import lombok.NoArgsConstructor;
import org.hibernate.annotations.ColumnDefault;
import org.hibernate.annotations.OnDelete;
import org.hibernate.annotations.OnDeleteAction;

import java.util.Objects;

@Data
@Entity
@Table(name = "session_players")
@NoArgsConstructor
@JsonIgnoreProperties({ "hibernateLazyInitializer", "handler" })
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
    @Column(name = "brick", nullable = false)
    @ColumnDefault("0")
    private Integer brick = 0;

    @NotNull
    @Column(name = "crystal", nullable = false)
    @ColumnDefault("0")
    private Integer crystal = 0;

    @NotNull
    @Column(name = "ore", nullable = false)
    @ColumnDefault("0")
    private Integer ore = 0;

    @NotNull
    @Column(name = "rice", nullable = false)
    @ColumnDefault("0")
    private Integer rice = 0;

    @NotNull
    @Column(name = "sheep", nullable = false)
    @ColumnDefault("0")
    private Integer sheep = 0;

    @NotNull
    @Column(name = "silver", nullable = false)
    @ColumnDefault("0")
    private Integer silver = 0;

    @NotNull
    @Column(name = "gold", nullable = false)
    @ColumnDefault("0")
    private Integer gold = 0;

    @NotNull
    @Column(name = "wood", nullable = false)
    @ColumnDefault("0")
    private Integer wood = 0;

    @Column(name = "turn_order")
    private Integer turnOrder;

    public Integer getNumberOfResources() {
        return brick + crystal + ore + rice + sheep + silver + gold + wood;
    }

    public void setResources(ResourceGroup resourceGroup) {
        brick = resourceGroup.getBrick();
        crystal = resourceGroup.getCrystal();
        ore = resourceGroup.getOre();
        rice = resourceGroup.getRice();
        sheep = resourceGroup.getSheep();
        silver = resourceGroup.getSilver();
        gold = resourceGroup.getGold();
        wood = resourceGroup.getWood();
    }

    @Override
    public boolean equals(Object o) {
        if (this == o) return true;
        if (o == null || getClass() != o.getClass()) return false;
        SessionPlayer that = (SessionPlayer) o;
        return Objects.equals(id, that.id);
    }

    @Override
    public int hashCode() {
        return Objects.hash(id);
    }
}