package com.catan.catanbackend.model;

import jakarta.persistence.*;
import lombok.Getter;
import lombok.NoArgsConstructor;
import lombok.Setter;

import java.util.Arrays;
import java.util.List;

@Entity
@Table(name = "tiles")
@Getter
@Setter
@NoArgsConstructor
public class Tile {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;

    private int x;
    private int y;

    private int number;
    private boolean hasRobber;

    @ElementCollection
    private List<Boolean> corners = Arrays.asList(false, false, false, false, false, false);

    @ElementCollection
    private List<Boolean> edges = Arrays.asList(false, false, false, false, false, false);
}
