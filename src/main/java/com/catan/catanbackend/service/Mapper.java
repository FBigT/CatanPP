package com.catan.catanbackend.service;

import com.catan.catanbackend.model.*;
import com.catan.catanbackend.model.dto.RegisterForm;
import com.catan.catanbackend.model.dto.SessionCodeDto;
import com.catan.catanbackend.model.dto.SessionSaveSimpleDto;
import com.catan.catanbackend.model.dto.UserDto;
import org.springframework.security.crypto.bcrypt.BCryptPasswordEncoder;
import org.springframework.security.crypto.password.PasswordEncoder;
import org.springframework.stereotype.Component;

import java.time.LocalDateTime;

@Component
public class Mapper {
    final UserService userService;
    final PasswordEncoder passwordEncoder;

    public Mapper(UserService userService) {
        this.userService = userService;
        this.passwordEncoder = new BCryptPasswordEncoder();
    }

    public User mapRegisterFormToUser(RegisterForm registerForm) {
        User user = new User();
        user.setActive(true);
        user.setEmail(registerForm.getEmail());
        user.setUsername(registerForm.getUsername());
        user.setCreatedAt(LocalDateTime.now());

        if (registerForm.getPassword() != null && !registerForm.getPassword().isEmpty()) {
            user.setPasswordHash(passwordEncoder.encode(registerForm.getPassword()));
            user.setIsGuest(false);
        } else {
            user.setPasswordHash(null);
            user.setIsGuest(true);
        }

        return user;
    }

    public UserDto mapUserToDto(User user) {
        return new UserDto(user.getId(), user.getUsername(), user.getEmail(), user.getActive(), user.getIsGuest(), user.getCreatedAt());
    }

    public SessionCodeDto mapSessionToDto(SessionCode sessionCode) {
        SessionCodeDto sessionCodeDto = new SessionCodeDto();
        sessionCodeDto.setId(sessionCode.getSession().getId());
        sessionCodeDto.setCode(sessionCode.getCode());
        return sessionCodeDto;
    }

    public SessionSaveSimpleDto mapSessionSaveToSaveDto(SessionSave session) {
        return new SessionSaveSimpleDto(session.getId(), session.getName(), session.getTurnNumber(), session.getSavedAt());
    }

    public ResourceGroup mapSessionPlayerToResource(SessionPlayer player) {
        return new ResourceGroup(player.getLumber(), player.getWool(), player.getOre(), player.getGrain(), player.getBricks(), player.getSilver(), player.getGold(), player.getObsidian());
    }
}
