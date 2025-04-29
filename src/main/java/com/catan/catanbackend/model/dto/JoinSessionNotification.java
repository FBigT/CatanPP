package com.catan.catanbackend.model.dto;

import com.catan.catanbackend.model.User;
import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.util.ArrayList;
import java.util.List;
import java.util.stream.Collectors;

@Data
@NoArgsConstructor
public class JoinSessionNotification {
    public JoinSessionNotification(List<User> users) {
        usernames = users.stream().map(User::getUsername).collect(Collectors.toList());
    }

    List<String> usernames = new ArrayList<>();
}
