package com.catan.catanbackend.model.tile;

import com.catan.catanbackend.model.SessionPlayer;
import com.fasterxml.jackson.annotation.JsonIgnore;
import com.fasterxml.jackson.annotation.JsonIgnoreProperties;
import com.fasterxml.jackson.annotation.JsonProperty;
import jakarta.persistence.*;
import jakarta.validation.constraints.NotNull;
import lombok.Getter;
import lombok.NoArgsConstructor;
import lombok.Setter;

@Entity
@Table(name = "roads")
@Getter
@Setter
@NoArgsConstructor
@JsonIgnoreProperties({ "hibernateLazyInitializer", "handler" })
public class Road {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;

    @JsonIgnore
    @NotNull
    @ManyToOne(fetch = FetchType.EAGER, optional = false)
    @JoinColumn(name = "session_player_id", nullable = false)
    private SessionPlayer owner;

    @JsonIgnore
    @OneToOne(mappedBy = "road", fetch = FetchType.LAZY)
    private TileEdge tileEdge;

    @JsonProperty("edgeId")
    public Long extractEdgeId() {
        return tileEdge.getId();
    }

    @JsonProperty("ownerId")
    public Long extractOwnerId() {
        return owner.getId();
    }
}
