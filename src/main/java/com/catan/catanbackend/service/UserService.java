package com.catan.catanbackend.service;


import com.catan.catanbackend.model.User;
import com.catan.catanbackend.repository.UserRepository;
import org.springframework.stereotype.Service;

import java.time.LocalDateTime;
import java.util.List;
import java.util.Optional;
import java.util.Random;

@Service

public class UserService {
    final Random random;
    final UserRepository userRepository;
    static String candidateChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
    static int guestNameLength = 8;

    public UserService(UserRepository userRepository) {
        this.userRepository = userRepository;
        this.random = new Random();
    }

    public List<User> getAllUsers() {
        return userRepository.findAll();
    }

    public Optional<User> getUserByUsername(String username) {
        return userRepository.findByUsername(username);
    }

    public User createUser(User user) {
        user.setCreatedAt(LocalDateTime.now());
        user.setActive(true);
        return userRepository.saveAndFlush(user);
    }

    public User updateUser(User user) {
        return userRepository.saveAndFlush(user);
    }

    public Boolean deactivateUser(Long id) {
        Optional<User> user = userRepository.findById(id);
        if (user.isPresent() && Boolean.TRUE.equals(user.get().getActive())) {
            user.get().setActive(false);
            userRepository.saveAndFlush(user.get());
            return true;
        }
        return false;
    }

    public void deleteUser(Long id) {
        userRepository.deleteById(id);
    }

    public Optional<User> findById(Long id) {
        return userRepository.findById(id);
    }

    public User createGuest(){
        User guestUser = new User();
        guestUser.setIsGuest(true);
        guestUser.setActive(true);
        guestUser.setPasswordHash(null);
        guestUser.setCreatedAt(LocalDateTime.now());
        StringBuilder sb = new StringBuilder ();

        do{
            sb.setLength(0);
            for (int i = 0; i < guestNameLength; i ++) {
                sb.append (candidateChars.charAt (random.nextInt (candidateChars
                        .length ())));
            }
            guestUser.setUsername("Guest_" + sb.toString());
        } while (userRepository.findByUsername(guestUser.getUsername()).isPresent());

        return createUser(guestUser);
    }

    public void deleteAllUsers() {
        userRepository.deleteAll();
    }
}
