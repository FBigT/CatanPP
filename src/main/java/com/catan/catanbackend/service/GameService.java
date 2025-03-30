package com.catan.catanbackend.service;

import org.springframework.stereotype.Service;

import java.util.Random;

@Service
public class GameService {
    static Random rand = new Random();
    static String[] names = { "Mirko", "Marko", "Mio", "Febo", "Gjuro", "Pero", "Nano", "Fico" };

    public static String generateRandomName(){
        int index = rand.nextInt(names.length);
        return names[index];
    }
}
