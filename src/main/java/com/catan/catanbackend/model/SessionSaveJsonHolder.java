package com.catan.catanbackend.model;

import com.catan.catanbackend.model.tile.*;
import lombok.Builder;
import lombok.Data;

import java.util.List;

@Data
@Builder
public class SessionSaveJsonHolder {
    private Session session;
    private List<SessionPlayer> sessionPlayers;
    private List<DevCard> devCards;
    private List<Tile> tiles;
    private List<TileEdge> tileEdges;
    private List<TileCorner> tileCorners;
    private List<TileCornerMap> tileCornerMaps;
    private List<TileEdgeMap> tileEdgeMaps;
    private List<RobberMoveBlocker> robberMoveBlockers;
    private List<RobberDebtBlocker> robberDebtBlockers;
    private List<Road> roads;
    private List<Structure> structures;
}
