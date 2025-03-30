package com.catan.catanbackend.controller;


import com.catan.catanbackend.service.DiceRollService;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.web.bind.annotation.*;

@CrossOrigin
@RestController
@RequestMapping("/api/dice")
public class DiceRollController {
    private final DiceRollService diceRollService;

    public DiceRollController(DiceRollService diceRollService) {
        this.diceRollService = diceRollService;
    }

    @GetMapping("/roll")
    public int rollDice() {
        int result = diceRollService.rollDice();
        System.out.println("🎲 Dice Roll: " + result);
        return result;
    }
}
