package com.catan.catanbackend.model;

import com.fasterxml.jackson.annotation.JsonIgnore;
import com.fasterxml.jackson.annotation.JsonProperty;
import jakarta.persistence.*;
import jakarta.validation.constraints.NotNull;
import lombok.Data;
import lombok.NoArgsConstructor;
import org.hibernate.annotations.ColumnDefault;

@NoArgsConstructor
@Entity
@Table(name = "player_profiles")
@Data
public class PlayerProfile {
    public PlayerProfile(User user) {
        this.user = user;
    }

    @Id
    @Column(name = "id", nullable = false)
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;

    @JsonIgnore
    @NotNull
    @OneToOne(fetch = FetchType.EAGER, optional = false)
    @JoinColumn(name = "user_id", nullable = false)
    private User user;

    @JsonProperty("username")
    public String getUsername() {
        return user.getUsername();
    }

    @NotNull
    @ColumnDefault("0")
    @Column(name = "games_won", nullable = false)
    private Integer gamesWon = 0;

    @NotNull
    @ColumnDefault("0")
    @Column(name = "games_played", nullable = false)
    private Integer gamesPlayed = 0;

    @NotNull
    @ColumnDefault("0")
    @Column(name = "games_lost", nullable = false)
    private Integer gamesLost = 0;

    @NotNull
    @ColumnDefault("0")
    @Column(name = "turns_taken", nullable = false)
    private Integer turnsTaken = 0;

    @NotNull
    @ColumnDefault("0")
    @Column(name = "resources_gathered", nullable = false)
    private Integer resourcesGathered = 0;

    @NotNull
    @ColumnDefault("0")
    @Column(name = "structures_placed", nullable = false)
    private Integer structuresPlaced = 0;

    @NotNull
    @ColumnDefault("0")
    @Column(name = "roads_placed", nullable = false)
    private Integer roadsPlaced = 0;

    @NotNull
    @ColumnDefault("0")
    @Column(name = "skins_unlocked", nullable = false)
    private Integer skinsUnlocked = 0;

    @Embedded
    private ResourceGroup resources = new ResourceGroup();
}
