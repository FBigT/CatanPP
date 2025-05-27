package com.catan.catanbackend.controller;

import com.catan.catanbackend.model.Session;
import com.catan.catanbackend.model.SessionPlayer;
import com.catan.catanbackend.model.User;
import com.catan.catanbackend.model.dto.MapGenerationDto;
import com.catan.catanbackend.model.helper.StructureTypeEnum;
import com.catan.catanbackend.model.tile.Road;
import com.catan.catanbackend.model.tile.Structure;
import com.catan.catanbackend.model.tile.Tile;
import com.catan.catanbackend.service.*;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.util.List;
import java.util.Optional;

@RestController
@RequestMapping("/api/place")
@CrossOrigin
public class PlacementController {

    private final PlacementService placementService;
    private final TokenService tokenService;
    private final UserService userService;
    private final SessionPlayerService sessionPlayerService;
    private final Mapper mapper;
    private final GameMoveHandler gameMoveHandler;

    public PlacementController(PlacementService placementService, TokenService tokenService, UserService userService, SessionPlayerService sessionPlayerService, Mapper mapper, GameMoveHandler gameMoveHandler) {
        this.placementService = placementService;
        this.tokenService = tokenService;
        this.userService = userService;
        this.sessionPlayerService = sessionPlayerService;
        this.mapper = mapper;
        this.gameMoveHandler = gameMoveHandler;
    }

    // ----------------------------------------------------------------
    // 1) Place Settlement (Structure)
    // ----------------------------------------------------------------
    @PostMapping("/structure")
    public ResponseEntity<?> placeStructure(@RequestParam Long sessionPlayerId,
                                            @RequestParam Long tileId,
                                            @RequestParam int cornerIndex,
                                            @RequestParam StructureTypeEnum structureType) {
        try {
            Structure s = placementService.placeStructure(sessionPlayerId, tileId, cornerIndex, structureType, false);
            return ResponseEntity.ok(s);
        } catch (IllegalArgumentException e) {
            // e.g. "Not enough resources" or "Cannot place structure here"
            return ResponseEntity.status(HttpStatus.BAD_REQUEST).body(e.getMessage());
        }
    }

    // ----------------------------------------------------------------
    // 2) Place Road
    // ----------------------------------------------------------------
    @PostMapping("/road")
    public ResponseEntity<?> placeRoad(@RequestParam Long sessionPlayerId,
                                       @RequestParam Long tileId,
                                       @RequestParam int edgeIndex) {
        try {
            Road r = placementService.placeRoad(sessionPlayerId, tileId, edgeIndex, false);
            return ResponseEntity.ok(r);
        } catch (IllegalArgumentException e) {
            // e.g. "Not enough resources" or "Cannot place road here"
            return ResponseEntity.status(HttpStatus.BAD_REQUEST).body(e.getMessage());
        }
    }

    // ----------------------------------------------------------------
    // 3) Upgrade Settlement to City
    // ----------------------------------------------------------------
    @PutMapping("/structure/upgrade")
    public ResponseEntity<?> upgradeStructure(@RequestParam Long tileId,
                                              @RequestParam int cornerIndex,
                                              @RequestParam Long sessionPlayerId) {
        try {
            Structure upgraded = placementService.upgradeSettlementToCity(tileId, cornerIndex, sessionPlayerId);
            return ResponseEntity.ok(upgraded);
        } catch (IllegalArgumentException e) {
            // e.g. "Not enough resources" or "Only settlements can be upgraded"
            return ResponseEntity.status(HttpStatus.BAD_REQUEST).body(e.getMessage());
        }
    }

    @PostMapping("/map/generate")
    public ResponseEntity<Void> generateMap(@RequestBody MapGenerationDto mapGenerationDto, @RequestHeader (name="Authorization") String token){
        try {
            if (!token.startsWith("Bearer")) {
                return new ResponseEntity<>(HttpStatus.UNAUTHORIZED);
            }

            Optional<User> user = userService.findById(tokenService.getUserIdFromJwtToken(token.split(" ")[1]));
            if (user.isEmpty()) {
                return new ResponseEntity<>(HttpStatus.UNAUTHORIZED);
            }

            Optional<SessionPlayer> sessionPlayer = sessionPlayerService.findCurrentSessionPlayerByUserId(user.get().getId());
            if (sessionPlayer.isEmpty() || !sessionPlayer.get().getUser().getId().equals(sessionPlayer.get().getSession().getHost().getId())) {
                return new ResponseEntity<>(HttpStatus.UNAUTHORIZED);
            }
            Session session = sessionPlayer.get().getSession();
            List<Tile> tileList = mapGenerationDto.getTileDtos().stream().map(x
                    -> mapper.mapTileDtoToTile(x, session)).toList();

            gameMoveHandler.generateCornersAndEdges(tileList);
            return new ResponseEntity<>(HttpStatus.OK);
        }
        catch (IllegalArgumentException e) {
            return new ResponseEntity<>(HttpStatus.BAD_REQUEST);
        }
    }
}
