package com.catan.catanbackend.controller;

import com.catan.catanbackend.controller.webSocket.ChatController;
import com.catan.catanbackend.model.Session;
import com.catan.catanbackend.model.SessionCode;
import com.catan.catanbackend.model.SessionPlayer;
import com.catan.catanbackend.model.SessionSave;
import com.catan.catanbackend.model.dto.SessionCodeDto;
import com.catan.catanbackend.model.dto.SessionSaveSimpleDto;
import com.catan.catanbackend.service.*;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.util.List;
import java.util.Objects;
import java.util.Optional;

@CrossOrigin
@RestController
@RequestMapping("/api/sessions")
public class SessionController {
    final SessionService sessionService;
    final SessionSaveService sessionSaveService;
    final Mapper mapper;
    final TokenService tokenService;
    final String tokenType = "Bearer";
    final SessionPlayerService sessionPlayerService;
    final ChatController chatController;

    public SessionController(Mapper mapper, SessionService sessionService, SessionSaveService sessionSaveService, TokenService tokenService, SessionPlayerService sessionPlayerService, ChatController chatController) {
        this.mapper = mapper;
        this.sessionService = sessionService;
        this.sessionSaveService = sessionSaveService;
        this.tokenService = tokenService;
        this.sessionPlayerService = sessionPlayerService;
        this.chatController = chatController;
    }

    @PostMapping("/{maxPlayers}")
    public ResponseEntity<SessionCodeDto> createSession(@PathVariable int maxPlayers, @RequestHeader (name="Authorization") String token) {
        if (!token.startsWith(tokenType)) {
            return new ResponseEntity<>(HttpStatus.UNAUTHORIZED);
        }
        Optional<SessionCode> sessionCode = sessionService.startSession(tokenService.getUserIdFromJwtToken(token.split(" ")[1]), maxPlayers);

        return sessionCode
                .map(code -> {
                    //chatController.sendJoinSessionUpdate(code.getCode(), sessionService.getPlayers(code.getSession().getId()).stream().map(SessionPlayer::getUser).toList());
                    return new ResponseEntity<>(mapper.mapSessionToDto(code), HttpStatus.CREATED);
                })
                .orElseGet(() -> new ResponseEntity<>(HttpStatus.BAD_REQUEST));
    }

    @PostMapping("/close")
    public ResponseEntity<Void> closeSession(@RequestHeader (name="Authorization") String token) {
        if (!token.startsWith(tokenType)) {
            return new ResponseEntity<>(HttpStatus.UNAUTHORIZED);
        }
        Optional<Session> session = sessionService.getActiveSessionsByHostId(tokenService.getUserIdFromJwtToken(token.split(" ")[1]));

        if (session.isPresent()) {
            sessionService.closeSession(session.get());
            return new ResponseEntity<>(HttpStatus.OK);
        }
        return new ResponseEntity<>(HttpStatus.NOT_FOUND);
    }

    @PostMapping("/join/{code}")
    public ResponseEntity<SessionCodeDto> joinSession(@PathVariable String code, @RequestHeader (name="Authorization") String token) {
        if (!token.startsWith(tokenType)) {
            return new ResponseEntity<>(HttpStatus.UNAUTHORIZED);
        }

        return sessionService.joinSession(tokenService.getUserIdFromJwtToken(token.split(" ")[1]), code)
                .map(value -> {
                    //chatController.sendJoinSessionUpdate(code, sessionService.getPlayers(value.getId()).stream().map(SessionPlayer::getUser).toList());
                    return new ResponseEntity<>(mapper.mapSessionToDto(value), HttpStatus.OK);
                })
                .orElseGet(() -> new ResponseEntity<>(HttpStatus.BAD_REQUEST));
    }

    @PostMapping("/leave/{code}")
    public ResponseEntity<Void> leaveSession(@PathVariable String code, @RequestHeader (name="Authorization") String token) {
        if (!token.startsWith(tokenType)) {
            return new ResponseEntity<>(HttpStatus.UNAUTHORIZED);
        }

        if (sessionService.leaveSession(tokenService.getUserIdFromJwtToken(token.split(" ")[1]), code)) {
            return new ResponseEntity<>(HttpStatus.OK);
        }
        return new ResponseEntity<>(HttpStatus.NOT_FOUND);
    }

    @GetMapping("/saves")
    public ResponseEntity<List<SessionSaveSimpleDto>> getSessionSavesByHostId(@RequestHeader (name="Authorization") String token) {
        if (!token.startsWith(tokenType)) {
            return new ResponseEntity<>(HttpStatus.UNAUTHORIZED);
        }
        Long hostId = tokenService.getUserIdFromJwtToken(token.split(" ")[1]);
        return new ResponseEntity<>(sessionSaveService.getSavesByHostId(hostId).stream().map(mapper::mapSessionSaveToSaveDto).toList(), HttpStatus.OK);
    }

    @PostMapping("/save")
    public ResponseEntity<SessionSaveSimpleDto> createSessionSave(@RequestParam("name") String name, @RequestHeader (name="Authorization") String token) {
        if (!token.startsWith(tokenType)) {
            return new ResponseEntity<>(HttpStatus.UNAUTHORIZED);
        }
        Long userId = tokenService.getUserIdFromJwtToken(token.split(" ")[1]);

        Optional<SessionPlayer> sessionPlayer = sessionPlayerService.findCurrentSessionPlayerByUserId(userId);

        if (sessionPlayer.isEmpty())
            return new ResponseEntity<>(HttpStatus.NOT_FOUND);
        if (name == null)
            name = "New Save";

        SessionSave save = sessionSaveService.save(name, sessionPlayer.get().getSession());
        return new ResponseEntity<>(mapper.mapSessionSaveToSaveDto(save), HttpStatus.CREATED);
    }

    @DeleteMapping("/save/{id}")
    public ResponseEntity<Void> deleteSessionSave(@PathVariable Long id, @RequestHeader (name="Authorization") String token) {
        if (!token.startsWith(tokenType)) {
            return new ResponseEntity<>(HttpStatus.UNAUTHORIZED);
        }
        Long userId = tokenService.getUserIdFromJwtToken(token.split(" ")[1]);

        Optional<SessionSave> save = sessionSaveService.findById(id);
        if (save.isEmpty())
            return new ResponseEntity<>(HttpStatus.NOT_FOUND);
        if (!Objects.equals(save.get().getSession().getHost().getId(), userId))
            return new ResponseEntity<>(HttpStatus.FORBIDDEN);

        sessionSaveService.deleteSave(id);
        return new ResponseEntity<>(HttpStatus.OK);
    }
}
