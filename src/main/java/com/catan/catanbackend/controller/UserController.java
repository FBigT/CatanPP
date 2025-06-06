package com.catan.catanbackend.controller;


import com.catan.catanbackend.model.*;
import com.catan.catanbackend.model.dto.*;
import com.catan.catanbackend.service.*;
import com.fasterxml.jackson.core.JsonProcessingException;
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

    private static final String BEARER = "Bearer";

    public UserController(AuthenticationManager authenticationManager, UserService userService, Mapper mapper, TokenService tokenService, GuestKeyService guestKeyService, PlayerProfileService playerProfileService, RefreshTokenService refreshTokenService, EncryptionUtils encryptionUtils, ObjectMapper objectMapper) {
        this.authenticationManager = authenticationManager;
        this.userService = userService;
        this.mapper = mapper;
        this.tokenService = tokenService;
        this.guestKeyService = guestKeyService;
        this.playerProfileService = playerProfileService;
        this.refreshTokenService = refreshTokenService;
        this.encryptionUtils = encryptionUtils;
        this.objectMapper = objectMapper;
    }

    @PostMapping("/login")
    public ResponseEntity<EncryptedMessage> login(@RequestBody EncryptedMessage encryptedMessage) {
        LogInForm logInForm;
        try {
            String decrypt = encryptionUtils.decrypt(encryptedMessage.getCrypto());
            logInForm = objectMapper.readValue(decrypt, LogInForm.class);
        } catch (Exception e) {
            return ResponseEntity.status(HttpStatus.BAD_REQUEST).build();
        }
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
        String encrypt;
        try {
            String json = objectMapper.writeValueAsString(logInResponse);
            encrypt = encryptionUtils.encrypt(json);
        } catch (Exception e) {
            return ResponseEntity.status(HttpStatus.INTERNAL_SERVER_ERROR).build();
        }
        return new ResponseEntity<>(new EncryptedMessage(encrypt), HttpStatus.OK);
    }

    @PostMapping("/login/bad")
    public ResponseEntity<LogInResponse> login(@RequestBody LogInForm logInForm) {
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
        return new ResponseEntity<>(logInResponse, HttpStatus.OK);
    }

    @PostMapping("/login/guest")
    public ResponseEntity<LogInResponse> guestLogin(@RequestBody String key) {
        Optional<GuestKey> guestKey = guestKeyService.findByKey(key);

        if (guestKey.isEmpty() || Boolean.FALSE.equals(guestKey.get().getGuest().getActive())) {
            return new ResponseEntity<>(HttpStatus.UNAUTHORIZED);
        }
        UserDetailsImpl userDetails = UserDetailsImpl.build(guestKey.get().getGuest());

        UsernamePasswordAuthenticationToken authentication =
                new UsernamePasswordAuthenticationToken(userDetails, null, userDetails.getAuthorities());

        SecurityContextHolder.getContext().setAuthentication(authentication);

        String jwt = tokenService.generateJwtToken(authentication);

        return new ResponseEntity<>(new LogInResponse(userDetails.getId(), userDetails.getUsername(), jwt,refreshTokenService.createIfNotExists(guestKey.get().getGuest()).getToken()), HttpStatus.OK);
    }

    @PostMapping("/refresh")
    public ResponseEntity<EncryptedMessage> refreshToken(@RequestBody String refreshToken){
        refreshToken = refreshToken.replace("\"", "");
        Optional<RefreshToken> existingToken = refreshTokenService.getRefreshTokenByToken(refreshToken);
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
        String encrypt;
        try {
            String json = objectMapper.writeValueAsString(new LogInResponse(userDetails.getId(), userDetails.getUsername(), jwt, refreshToken));
            encrypt = encryptionUtils.encrypt(json);
        } catch (Exception e) {
            return ResponseEntity.status(HttpStatus.INTERNAL_SERVER_ERROR).build();
        }
        return new ResponseEntity<>(new EncryptedMessage(encrypt), HttpStatus.OK);
    }

    //@PreAuthorize("isAuthenticated()")
    @GetMapping
    public List<User> getAllUsers() {
        return userService.getAllUsers();
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
        return userService.findById(id)
                .map(value -> new ResponseEntity<>(mapper.mapUserToDto(value), HttpStatus.OK))
                .orElseGet(() -> new ResponseEntity<>(HttpStatus.NOT_FOUND));
    }

    @PostMapping("/register")
    public ResponseEntity<EncryptedMessage> create(@RequestBody EncryptedMessage encryptedMessage) {
        RegisterForm signInForm;
        try {
            String decrypt = encryptionUtils.decrypt(encryptedMessage.getCrypto());
            signInForm = objectMapper.readValue(decrypt, RegisterForm.class);
        } catch (Exception e) {
            return ResponseEntity.status(HttpStatus.BAD_REQUEST).build();
        }

        User newUser = userService.createUser(mapper.mapRegisterFormToUser(signInForm));
        playerProfileService.savePlayerProfile(new PlayerProfile(newUser));
        UserDto userDto = mapper.mapUserToDto(newUser);
        String encrypt;
        try {
            encrypt = encryptionUtils.encrypt(objectMapper.writeValueAsString(userDto));
        } catch (Exception e) {
            return ResponseEntity.status(HttpStatus.INTERNAL_SERVER_ERROR).build();
        }
        return new ResponseEntity<>(new EncryptedMessage(encrypt), HttpStatus.CREATED);
    }

    @PostMapping("/register/guest")
    public ResponseEntity<GuestRegisterResponse> createGuest() {
        User guest = userService.createGuest();
        String key = guestKeyService.createGuestKey(guest);
        return new ResponseEntity<>(new GuestRegisterResponse(guest.getId(), guest.getUsername(), key), HttpStatus.CREATED);
    }

    @DeleteMapping("/deactivate/{id}")
    public ResponseEntity<UserDto> deactivateUser(@PathVariable Long id, @RequestHeader (name="Authorization") String token){
        if (!token.startsWith(BEARER)
            || !Objects.equals(tokenService.getUserIdFromJwtToken(token.split(" ")[1]), id)) {
            return new ResponseEntity<>(HttpStatus.UNAUTHORIZED);
        }

        Optional<User> user = userService.findById(id);
        if (user.isPresent() && Boolean.TRUE.equals(userService.deactivateUser(id)))
            return new ResponseEntity<>(mapper.mapUserToDto(user.get()), HttpStatus.OK);
        return new ResponseEntity<>(HttpStatus.NOT_FOUND);
    }

    @DeleteMapping("/forget/{id}")
    public ResponseEntity<UserDto> deleteUser(@PathVariable Long id, @RequestHeader (name="Authorization") String token){
        if (!token.startsWith(BEARER)
                || !Objects.equals(tokenService.getUserIdFromJwtToken(token.split(" ")[1]), id)) {
            return new ResponseEntity<>(HttpStatus.UNAUTHORIZED);
        }
        refreshTokenService.deleteByUserId(id);
        userService.deleteUser(id);

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
