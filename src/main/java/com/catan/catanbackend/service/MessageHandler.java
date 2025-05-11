package com.catan.catanbackend.service;

import com.catan.catanbackend.model.CubeCoordinates;
import com.catan.catanbackend.model.tile.*;
import jakarta.validation.constraints.NotNull;
import org.springframework.stereotype.Service;

import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.Objects;

@Service
public class MessageHandler {

    private record CornerPair(TileCorner a, TileCorner b) {
            private CornerPair(@NotNull TileCorner a, @NotNull TileCorner b) {
                if (a.getId() > b.getId()) {
                    this.a = b;
                    this.b = a;
                } else {
                    this.a = a;
                    this.b = b;
                }
            }

            @Override
            public boolean equals(Object o) {
                if (this == o) return true;
                if (!(o instanceof CornerPair that)) return false;
                return Objects.equals(a, that.a) && Objects.equals(b, that.b);
            }

    }

    public static void generateCornersAndEdges(List<Tile> tiles) {
        Map<CubeCoordinates, TileCorner> cornerMap = new HashMap<>();
        Map<CornerPair, TileEdge> edgeMap = new HashMap<>();

        for (Tile tile : tiles) {
            TileCorner[] tileCorners = new TileCorner[6];

            for (int i = 0; i < 6; i++) {
                CubeCoordinates coordinates = getCornerCoordinates(tile.getX(), tile.getY(), tile.getZ(), i);
                TileCorner corner = cornerMap.computeIfAbsent(coordinates, c -> {
                    TileCorner tc = new TileCorner();
                    tc.setX(c.getX());
                    tc.setY(c.getY());
                    tc.setZ(c.getZ());
                    tc.setSession(tile.getSession());
                    return tc;
                });

                TileCornerMap tcm = new TileCornerMap();
                tcm.setTile(tile);
                tcm.setCorner(corner);
                tcm.setCornerIndex(i);

                tile.getTileCornerMaps().add(tcm);
                corner.getTileCornerMaps().add(tcm);

                tileCorners[i] = corner;
            }

            for (int i = 0; i < 6; i++) {
                TileCorner cornerA = tileCorners[i];
                TileCorner cornerB = tileCorners[(i + 1) % 6];
                CornerPair key = new CornerPair(cornerA, cornerB);

                TileEdge edge = edgeMap.computeIfAbsent(key, k -> {
                    TileEdge te = new TileEdge();
                    te.setCornerA(k.a);
                    te.setCornerB(k.b);
                    te.setSession(tile.getSession());
                    return te;
                });

                TileEdgeMap tem = new TileEdgeMap();
                tem.setTile(tile);
                tem.setEdge(edge);
                tem.setEdgeIndex(i);

                tile.getTileEdgeMaps().add(tem);
                edge.getTileEdgeMaps().add(tem);
            }
        }
    }

    private static CubeCoordinates getCornerCoordinates(int x, int y, int z, int cornerIndex) {
        int[][] offsets = {
                {1, -1, 0}, {1, 0, -1}, {0, 1, -1},
                {-1, 1, 0}, {-1, 0, 1}, {0, -1, 1}
        };
        int[] offset = offsets[cornerIndex % 6];
        return new CubeCoordinates(x + offset[0], y + offset[1], z + offset[2]);
    }
}
