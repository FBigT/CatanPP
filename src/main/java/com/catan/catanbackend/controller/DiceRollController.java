package com.catan.catanbackend.controller;


import com.catan.catanbackend.service.DiceRollService;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.web.bind.annotation.*;

@RestController
@RequestMapping("/api/dice")
public class DiceRollController {

    @Autowired
    private DiceRollService diceRollService;

    @GetMapping("/roll")
    public int rollDice() {
        int result = diceRollService.rollDice();
        System.out.println("🎲 Dice Roll: " + result);
        return result;
    }
}
