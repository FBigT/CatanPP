package com.catan.catanbackend.service;

import com.catan.catanbackend.model.ResourceGroup;
import com.catan.catanbackend.model.SessionCode;
import com.catan.catanbackend.model.SessionPlayer;
import com.catan.catanbackend.model.User;
import com.catan.catanbackend.model.dto.RegisterForm;
import com.catan.catanbackend.model.dto.SessionDto;
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

    public SessionDto mapSessionToDto(SessionCode sessionCode) {
        SessionDto sessionDto = new SessionDto();
        sessionDto.setId(sessionCode.getSession().getId());
        sessionDto.setCode(sessionCode.getCode());
        return sessionDto;
    }

    public ResourceGroup mapSessionPlayerToResource(SessionPlayer player) {
        return new ResourceGroup(player.getLumber(), player.getWool(), player.getOre(), player.getGrain(), player.getBricks(), player.getSilver(), player.getGold(), player.getObsidian());
    }
}
