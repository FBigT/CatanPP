package com.catan.catanbackend.model.tile;

import com.catan.catanbackend.model.SessionPlayer;
import jakarta.persistence.*;
import jakarta.validation.constraints.NotNull;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.util.Optional;

@Entity
@Data
@Table(name = "structures")
@NoArgsConstructor
public class Structure {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;

    @NotNull
    @ManyToOne(fetch = FetchType.LAZY, optional = false)
    @JoinColumn(name = "session_player_id", nullable = false)
    private SessionPlayer owner;

    @OneToOne(mappedBy = "structure", fetch = FetchType.EAGER)
    private TileCorner corner;

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

    //Moved upgradeToCity() to PlacementService
}
