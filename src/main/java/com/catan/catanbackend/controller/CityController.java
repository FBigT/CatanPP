package com.catan.catanbackend.controller;

import com.catan.catanbackend.model.City;
import com.catan.catanbackend.service.CityService;
import org.springframework.web.bind.annotation.*;

import java.util.List;

@RestController
@RequestMapping("/api/cities")
public class CityController {

    private final CityService cityService;

    public CityController(CityService cityService) {
        this.cityService = cityService;
    }

    @GetMapping
    public List<City> getAllCities() {
        return cityService.getAllCities();
    }

    @PostMapping("/place")
    public City placeSettlement(@RequestParam String owner, @RequestParam int x, @RequestParam int y) {
        return cityService.placeSettlement(owner, x, y);
    }

    @PutMapping("/{id}/upgrade")
    public City upgradeToCity(@PathVariable Long id) {
        return cityService.upgradeToCity(id);
    }
}
