package com.catan.catanbackend.model.helper;

import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.util.Objects;

@AllArgsConstructor
@Data
public class CubeCoordinates {
    private final int x;
    private final int y;
    private final int z;

    @Override
    public boolean equals(Object o) {
        if (this == o) return true;
        if (!(o instanceof CubeCoordinates that)) return false;
        // now works as intended
        return x == that.x
                && y == that.y
                && z == that.z;
    }

    @Override
    public int hashCode() {
        return Objects.hash(x, y, z);
    }
}
