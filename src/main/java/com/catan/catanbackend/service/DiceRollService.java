package com.catan.catanbackend.service;


import org.springframework.stereotype.Service;
import java.util.Random;

@Service
public class DiceRollService {
    private final Random random = new Random();

    public int rollDice() {
        int dice1 = random.nextInt(6) + 1;  // Rolls 1-6
        int dice2 = random.nextInt(6) + 1;  // Rolls 1-6
        return dice1 + dice2;  // Sum of both dice
    }
}
