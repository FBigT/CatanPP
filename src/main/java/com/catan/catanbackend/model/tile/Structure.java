package com.catan.catanbackend.model.tile;

import com.catan.catanbackend.model.SessionPlayer;
import com.fasterxml.jackson.annotation.JsonIgnore;
import com.fasterxml.jackson.annotation.JsonIgnoreProperties;
import com.fasterxml.jackson.annotation.JsonProperty;
import jakarta.persistence.*;
import jakarta.validation.constraints.NotNull;
import lombok.Data;
import lombok.EqualsAndHashCode;
import lombok.NoArgsConstructor;
import lombok.ToString;

import java.util.Optional;

@Entity
@Data
@Table(name = "structures")
@NoArgsConstructor
@JsonIgnoreProperties({ "hibernateLazyInitializer", "handler" })
public class Structure {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;

    @JsonIgnore
    @NotNull
    @ManyToOne(fetch = FetchType.EAGER, optional = false)
    @JoinColumn(name = "session_player_id", nullable = false)
    private SessionPlayer owner;

    @ToString.Exclude @EqualsAndHashCode.Exclude
    @JsonIgnore
    @OneToOne(mappedBy = "structure", fetch = FetchType.EAGER)
    private TileCorner corner;

    @JsonProperty("ownerId")
    public Long extractOwnerId(){
        return owner.getId();
    }

    @JsonProperty("cornerId")
    public Long extractCornerId(){
        return corner.getId();
    }

    @ManyToOne(fetch = FetchType.EAGER)
    @JoinColumn(name = "structure_type_id", nullable = false)
    private StructureType structureType;

    public Structure(SessionPlayer owner, Tile tile, Integer cornerIndex, StructureType structureType) {
        this.owner = owner;
        this.structureType = structureType;
        Optional<TileCorner> optionalTileCorner = tile.getTileCorner(cornerIndex);
        if (optionalTileCorner.isPresent()) {
            corner = optionalTileCorner.get();
        }
        else
            throw new IllegalArgumentException("The corner of the tile is not a valid corner");
    }
}
