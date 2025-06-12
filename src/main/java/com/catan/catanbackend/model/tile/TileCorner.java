package com.catan.catanbackend.model.tile;

import com.catan.catanbackend.model.Session;
import com.fasterxml.jackson.annotation.JsonIgnore;
import com.fasterxml.jackson.annotation.JsonIgnoreProperties;
import com.fasterxml.jackson.annotation.JsonProperty;
import jakarta.persistence.*;
import lombok.*;

import java.awt.geom.Point2D;
import java.util.ArrayList;
import java.util.List;
import java.util.Objects;

@Entity
@Data
@NoArgsConstructor
@AllArgsConstructor
@JsonIgnoreProperties({ "hibernateLazyInitializer", "handler" })
@Table(name = "tile_corners", uniqueConstraints = @UniqueConstraint(columnNames = {"x", "y", "z", "session_id"}))
public class TileCorner {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;

    private Double x;
    private Double y;
    private Double z;

    @JsonIgnore
    @ManyToOne(fetch = FetchType.LAZY)
    @JoinColumn(name = "session_id", nullable = false)
    private Session session;

    @JsonIgnore
    @OneToOne(fetch = FetchType.EAGER, cascade = CascadeType.PERSIST)
    @JoinColumn(name = "structure_id")
    private Structure structure;

    @JsonProperty("structureId")
    public Long extractStructureId() {
        if (structure == null) {
            return null;
        }
        return structure.getId();
    }

    @ToString.Exclude @EqualsAndHashCode.Exclude
    @JsonIgnore
    @OneToMany(mappedBy = "corner", cascade = CascadeType.ALL, orphanRemoval = true, fetch = FetchType.EAGER)
    private List<TileCornerMap> tileCornerMaps = new ArrayList<>();

    @Override
    public boolean equals(Object o) {
        if (this == o) return true;
        if (!(o instanceof TileCorner that)) return false;
        return Objects.equals(x, that.x) && Objects.equals(y, that.y);
    }

    @Override
    public int hashCode() {
        return Objects.hash(x, y);
    }

    public Point2D.Double getCoordinates() {
        return new Point2D.Double(this.x, this.y);
    }
}
