package com.catan.catanbackend.controller;


import com.catan.catanbackend.model.*;
import com.catan.catanbackend.model.dto.*;
import com.catan.catanbackend.service.*;
import com.fasterxml.jackson.databind.ObjectMapper;
import org.springframework.dao.DataIntegrityViolationException;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.security.access.prepost.PreAuthorize;
import org.springframework.security.authentication.AuthenticationManager;
import org.springframework.security.authentication.UsernamePasswordAuthenticationToken;
import org.springframework.security.core.Authentication;
import org.springframework.security.core.context.SecurityContextHolder;
import org.springframework.web.bind.annotation.*;

import java.util.List;
import java.util.Objects;
import java.util.Optional;

@CrossOrigin
@RestController
@RequestMapping("/api/users")
public class UserController {
    private final AuthenticationManager authenticationManager;
    private final UserService userService;
    private final Mapper mapper;
    private final RefreshTokenService refreshTokenService;
    private final TokenService tokenService;
    private final GuestKeyService guestKeyService;
    private final PlayerProfileService playerProfileService;
    private final EncryptionUtils encryptionUtils;
    private final ObjectMapper objectMapper;
    private final SessionPlayerService sessionPlayerService;

    private static final String BEARER = "Bearer";

    public UserController(AuthenticationManager authenticationManager, UserService userService, Mapper mapper, TokenService tokenService, GuestKeyService guestKeyService, PlayerProfileService playerProfileService, RefreshTokenService refreshTokenService, EncryptionUtils encryptionUtils, ObjectMapper objectMapper, SessionPlayerService sessionPlayerService) {
        this.authenticationManager = authenticationManager;
        this.userService = userService;
        this.mapper = mapper;
        this.tokenService = tokenService;
        this.guestKeyService = guestKeyService;
        this.playerProfileService = playerProfileService;
        this.refreshTokenService = refreshTokenService;
        this.encryptionUtils = encryptionUtils;
        this.objectMapper = objectMapper;
        this.sessionPlayerService = sessionPlayerService;
    }

    @PostMapping("/login")
    public ResponseEntity<EncryptedResponse> login(@RequestBody EncryptedMessage encryptedMessage) {
        DecryptedMessage decryptedMessage = mapper.mapToObject(encryptedMessage, LogInForm.class);
        LogInForm logInForm = (LogInForm) decryptedMessage.getPayload();

        Authentication authentication = authenticationManager.authenticate(new UsernamePasswordAuthenticationToken(logInForm.getUsername(), logInForm.getPassword()));

        SecurityContextHolder.getContext()
                .setAuthentication(authentication);
        UserDetailsImpl userDetails = (UserDetailsImpl) authentication.getPrincipal();

        Optional<User> user = userService.findById(userDetails.getId());
        if (user.isEmpty())
            return new ResponseEntity<>(HttpStatus.NOT_FOUND);
        if (Boolean.TRUE.equals(!user.get().getActive()) || Boolean.TRUE.equals(user.get().getIsGuest())) {
            return new ResponseEntity<>(HttpStatus.UNAUTHORIZED);
        }

        String jwt = tokenService.generateJwtToken(authentication);
        LogInResponse logInResponse = new LogInResponse(userDetails.getId(), userDetails.getUsername(), jwt, refreshTokenService.createIfNotExists(user.get()).getToken());

        EncryptedResponse encryptedResponse = mapper.mapToEncryptedResponse(logInResponse, decryptedMessage.getAesKey());

        return new ResponseEntity<>(encryptedResponse, HttpStatus.OK);
    }

    @PostMapping("/login/guest")
    public ResponseEntity<EncryptedResponse> guestLogin(@RequestBody EncryptedMessage encryptedMessage) {
        DecryptedMessage decryptedMessage = mapper.mapToObject(encryptedMessage, RefreshRequest.class);
        RefreshRequest refreshToken = (RefreshRequest) decryptedMessage.getPayload();
        Optional<GuestKey> guestKey = guestKeyService.findByKey(refreshToken.getRefreshToken());

        if (guestKey.isEmpty() || Boolean.FALSE.equals(guestKey.get().getGuest().getActive())) {
            return new ResponseEntity<>(HttpStatus.UNAUTHORIZED);
        }
        UserDetailsImpl userDetails = UserDetailsImpl.build(guestKey.get().getGuest());

        UsernamePasswordAuthenticationToken authentication =
                new UsernamePasswordAuthenticationToken(userDetails, null, userDetails.getAuthorities());

        SecurityContextHolder.getContext().setAuthentication(authentication);

        String jwt = tokenService.generateJwtToken(authentication);
        LogInResponse logInResponse = new LogInResponse(userDetails.getId(), userDetails.getUsername(), jwt, refreshTokenService.createIfNotExists(guestKey.get().getGuest()).getToken());
        return new ResponseEntity<>(mapper.mapToEncryptedResponse(logInResponse, decryptedMessage.getAesKey()), HttpStatus.OK);
    }

    @PostMapping("/refresh")
    public ResponseEntity<EncryptedResponse> refreshToken(@RequestBody EncryptedMessage encryptedMessage) {
        DecryptedMessage decryptedMessage = mapper.mapToObject(encryptedMessage, RefreshRequest.class);
        RefreshRequest refreshToken = (RefreshRequest) decryptedMessage.getPayload();

        Optional<RefreshToken> existingToken = refreshTokenService.getRefreshTokenByToken(refreshToken.getRefreshToken());
        if (existingToken.isEmpty()) {
            return new ResponseEntity<>(HttpStatus.NOT_FOUND);
        }

        if (Boolean.FALSE.equals(refreshTokenService.tokenIsValid(existingToken.get()))) {
            return new ResponseEntity<>(HttpStatus.UNAUTHORIZED);
        }

        UserDetailsImpl userDetails = UserDetailsImpl.build(existingToken.get().getUser());

        UsernamePasswordAuthenticationToken authentication =
                new UsernamePasswordAuthenticationToken(userDetails, null, userDetails.getAuthorities());

        SecurityContextHolder.getContext().setAuthentication(authentication);

        String jwt = tokenService.generateJwtToken(authentication);
        EncryptedResponse encryptedResponse = mapper.mapToEncryptedResponse(new LogInResponse(userDetails.getId(), userDetails.getUsername(), jwt, refreshToken.getRefreshToken()), decryptedMessage.getAesKey());
        return new ResponseEntity<>(encryptedResponse, HttpStatus.OK);
    }

    @GetMapping("/username/{username}")
    public ResponseEntity<UserDto> getUserByUsername(@PathVariable String username) {
        Optional<User> user = userService.getUserByUsername(username);
        return user.map(value ->
                    new ResponseEntity<>(mapper.mapUserToDto(user.get()), HttpStatus.OK))
                .orElseGet(() -> new ResponseEntity<>(HttpStatus.NOT_FOUND));
    }

    @GetMapping("/{id}")
    public ResponseEntity<UserDto> getUserById(@PathVariable Long id) {
        return userService.findById(id).filter(User::getActive)
                .map(value -> new ResponseEntity<>(mapper.mapUserToDto(value), HttpStatus.OK))
                .orElseGet(() -> new ResponseEntity<>(HttpStatus.NOT_FOUND));
    }

    @PostMapping("/register")
    public ResponseEntity<EncryptedResponse> create(@RequestBody EncryptedMessage encryptedMessage) {
        DecryptedMessage decryptedMessage = mapper.mapToObject(encryptedMessage, RegisterForm.class);
        RegisterForm signInForm = (RegisterForm) decryptedMessage.getPayload();

        User newUser = userService.createUser(mapper.mapRegisterFormToUser(signInForm));
        playerProfileService.savePlayerProfile(new PlayerProfile(newUser));
        UserDto userDto = mapper.mapUserToDto(newUser);

        EncryptedResponse encryptedResponse = mapper.mapToEncryptedResponse(userDto, decryptedMessage.getAesKey());

        return new ResponseEntity<>(encryptedResponse, HttpStatus.CREATED);
    }

    @PostMapping("/register/guest")
    public ResponseEntity<EncryptedResponse> createGuest(@RequestBody EncryptedMessage encryptedMessage) {
        byte[] aesKey;
        try {
            aesKey = encryptionUtils.decryptAESKey(encryptedMessage.getEncryptedKey());
        } catch (Exception e) {
            return new ResponseEntity<>(HttpStatus.BAD_REQUEST);
        }
        User guest = userService.createGuest();
        String key = guestKeyService.createGuestKey(guest);
        GuestRegisterResponse registerResponse = new GuestRegisterResponse(guest.getId(), guest.getUsername(), key);
        return new ResponseEntity<>(mapper.mapToEncryptedResponse(registerResponse, aesKey), HttpStatus.CREATED);
    }

    @DeleteMapping("/deactivate/{id}")
    public ResponseEntity<Void> deactivateUser(@PathVariable Long id, @RequestHeader (name="Authorization") String token){
        if (!token.startsWith(BEARER)
            || !Objects.equals(tokenService.getUserIdFromJwtToken(token.split(" ")[1]), id)) {
            return new ResponseEntity<>(HttpStatus.UNAUTHORIZED);
        }

        Optional<User> user = userService.findById(id);
        if (user.isPresent() && Boolean.TRUE.equals(userService.deactivateUser(id)))
            return new ResponseEntity<>(HttpStatus.OK);
        return new ResponseEntity<>(HttpStatus.NOT_FOUND);
    }

    @DeleteMapping("/forget")
    public ResponseEntity<Void> deleteUser(@RequestHeader (name="Authorization") String token){
        if (!token.startsWith(BEARER)) {
            return new ResponseEntity<>(HttpStatus.UNAUTHORIZED);
        }
        Long id = tokenService.getUserIdFromJwtToken(token.split(" ")[1]);
        refreshTokenService.deleteByUserId(id);
        userService.deleteUser(id);

        return new ResponseEntity<>(HttpStatus.OK);
    }

    @DeleteMapping("/forget/anonymize")
    public ResponseEntity<Void> anonymizeUser(@RequestHeader (name="Authorization") String token){
        if (!token.startsWith(BEARER)) {
            return new ResponseEntity<>(HttpStatus.UNAUTHORIZED);
        }
        Optional<User> byId = userService.findById(tokenService.getUserIdFromJwtToken(token.split(" ")[1]));
        if (byId.isEmpty()){
            return new ResponseEntity<>(HttpStatus.NOT_FOUND);
        }
        User user = byId.get();
        playerProfileService.deletePlayerProfile(user.getPlayerProfile());
        user.anonymize();
        userService.updateUser(user);

        sessionPlayerService.findPlayersByUserId(tokenService.getUserIdFromJwtToken(token.split(" ")[1])).forEach(player -> {
           player.setName(user.getUsername());
           sessionPlayerService.updateSessionPlayer(player);
        });

        return new ResponseEntity<>(HttpStatus.OK);
    }

    @PutMapping("/{id}")
    public ResponseEntity<UserDto> updateUser(@PathVariable Long id, @RequestBody UserDto userDto, @RequestHeader (name="Authorization") String token) {
        Optional<User> user = userService.findById(id);
        if (user.isEmpty())
            return new ResponseEntity<>(HttpStatus.NOT_FOUND);

        if (!token.startsWith(BEARER)
                || !Objects.equals(tokenService.getUserIdFromJwtToken(token.split(" ")[1]), id)) {
            return new ResponseEntity<>(HttpStatus.UNAUTHORIZED);
        }

        user.get().setEmail(userDto.getEmail());
        user.get().setUsername(userDto.getUsername());
        User updatedUser = userService.updateUser(user.get());
        return new ResponseEntity<>(mapper.mapUserToDto(updatedUser), HttpStatus.OK);
    }

    @PreAuthorize("isAuthenticated()")
    @GetMapping("/profile")
    public ResponseEntity<PlayerProfile> getCurrentPlayerProfile(@RequestHeader (name="Authorization") String token) {
        Long userId = tokenService.getUserIdFromJwtToken(token.split(" ")[1]);
        Optional<User> user = userService.findById(userId);

        if (user.isEmpty()){
            return new ResponseEntity<>(HttpStatus.NOT_FOUND);
        }

        return playerProfileService.getPlayerProfileByUserId(userId)
                .map(playerProfile -> new ResponseEntity<>(playerProfile, HttpStatus.OK))
                .orElseGet(() -> new ResponseEntity<>(HttpStatus.NOT_FOUND));
    }

    @GetMapping("/profile/{id}")
    public ResponseEntity<PlayerProfile> getPlayerProfileByUserId(@PathVariable Long id) {
        return playerProfileService.getPlayerProfileByUserId(id)
                .map(playerProfile -> new ResponseEntity<>(playerProfile, HttpStatus.OK))
                .orElseGet(() -> new ResponseEntity<>(HttpStatus.NOT_FOUND));
    }

    @GetMapping("/profile/username/{username}")
    public ResponseEntity<PlayerProfile> getPlayerProfileByUsername(@PathVariable String username) {
        return playerProfileService.getPlayerProfileByUsername(username)
                .map(playerProfile -> new ResponseEntity<>(playerProfile, HttpStatus.OK))
                .orElseGet(() -> new ResponseEntity<>(HttpStatus.NOT_FOUND));
    }

    @ExceptionHandler(DataIntegrityViolationException.class)
    public ResponseEntity<String> dataIntegrityViolationException(final DataIntegrityViolationException e) {
        if (e.getMessage().toLowerCase().contains("email")){
            return new ResponseEntity<>("\"email\"", HttpStatus.CONFLICT);
        }
        else if (e.getMessage().toLowerCase().contains("username")){
            return new ResponseEntity<>("\"username\"", HttpStatus.CONFLICT);
        }
        return new ResponseEntity<>("\"general\"", HttpStatus.CONFLICT);
    }
}
