package com.catan.catanbackend.controller;

import com.catan.catanbackend.model.SessionPlayer;
import com.catan.catanbackend.model.User;
import com.catan.catanbackend.model.dto.TileDto;
import com.catan.catanbackend.model.tile.Tile;
import com.catan.catanbackend.service.*;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.util.List;
import java.util.Optional;

@RestController
@RequestMapping("/api/map")
@CrossOrigin
public class MapController {

    private final TokenService tokenService;
    private final UserService userService;
    private final SessionPlayerService sessionPlayerService;
    private final Mapper mapper;
    private final TileService tileService;

    public MapController(TokenService tokenService, UserService userService, SessionPlayerService sessionPlayerService, Mapper mapper, TileService tileService) {
        this.tokenService = tokenService;
        this.userService = userService;
        this.sessionPlayerService = sessionPlayerService;
        this.mapper = mapper;
        this.tileService = tileService;
    }

    @GetMapping("/state")
    public ResponseEntity<List<TileDto>> getMapState(@RequestHeader(name = "Authorization") String token) {
        if (!token.startsWith("Bearer")) {
            return new ResponseEntity<>(HttpStatus.UNAUTHORIZED);
        }

        Optional<User> user = userService.findById(tokenService.getUserIdFromJwtToken(token.split(" ")[1]));
        if (user.isEmpty()) {
            return new ResponseEntity<>(HttpStatus.UNAUTHORIZED);
        }

        Optional<SessionPlayer> sessionPlayer = sessionPlayerService.findCurrentSessionPlayerByUserId(user.get().getId());
        if (sessionPlayer.isEmpty()) {
            return new ResponseEntity<>(HttpStatus.UNAUTHORIZED);
        }

        Long sessionId = sessionPlayer.get().getSession().getId();
        List<Tile> tiles = tileService.findBySessionId(sessionId);

        List<TileDto> tileDtos = tiles.stream()
                .map(mapper::mapTileToDto)
                .toList();

        return new ResponseEntity<>(tileDtos, HttpStatus.OK);
    }

}
