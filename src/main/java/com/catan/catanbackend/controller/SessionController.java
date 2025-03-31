package com.catan.catanbackend.controller;

import com.catan.catanbackend.model.SessionCode;
import com.catan.catanbackend.model.dto.SessionDto;
import com.catan.catanbackend.service.Mapper;
import com.catan.catanbackend.service.SessionService;
import com.catan.catanbackend.service.TokenService;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.util.Optional;

@CrossOrigin
@RestController
@RequestMapping("/api/sessions")
public class SessionController {
    final SessionService sessionService;
    final Mapper mapper;
    final TokenService tokenService;

    public SessionController(Mapper mapper, SessionService sessionService, TokenService tokenService) {
        this.mapper = mapper;
        this.sessionService = sessionService;
        this.tokenService = tokenService;
    }

    @PostMapping("/{maxPlayers}")
    public ResponseEntity<SessionDto> createSession(@PathVariable int maxPlayers, @RequestHeader (name="Authorization") String token) {
        if (!token.startsWith("Bearer ")) {
            return new ResponseEntity<>(HttpStatus.UNAUTHORIZED);
        }
        Optional<SessionCode> sessionCode = sessionService.startSession(tokenService.getUserIdFromJwtToken(token.split(" ")[1]), maxPlayers);

        return sessionCode
                .map(code -> new ResponseEntity<>(mapper.mapSessionToDto(code), HttpStatus.CREATED))
                .orElseGet(() -> new ResponseEntity<>(HttpStatus.BAD_REQUEST));
    }

    @PostMapping("/join/{code}")
    public ResponseEntity<SessionDto> joinSession(@PathVariable String code, @RequestHeader (name="Authorization") String token) {
        if (!token.startsWith("Bearer ")) {
            return new ResponseEntity<>(HttpStatus.UNAUTHORIZED);
        }

        return sessionService.joinSession(tokenService.getUserIdFromJwtToken(token.split(" ")[1]), code)
                .map(value -> new ResponseEntity<>(mapper.mapSessionToDto(value), HttpStatus.OK))
                .orElseGet(() -> new ResponseEntity<>(HttpStatus.BAD_REQUEST));
    }
}
