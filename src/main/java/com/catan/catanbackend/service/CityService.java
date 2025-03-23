package com.catan.catanbackend.service;


import com.catan.catanbackend.model.City;
import com.catan.catanbackend.repository.CityRepository;
import org.springframework.stereotype.Service;

import java.util.List;

@Service
public class CityService {

    private final CityRepository cityRepository;

    public CityService(CityRepository cityRepository) {
        this.cityRepository = cityRepository;
    }

    public List<City> getAllCities() {
        return cityRepository.findAll();
    }

    public City placeSettlement(String owner, int x, int y) {
        City settlement = new City(owner, x, y);
        return cityRepository.save(settlement);
    }

    public City upgradeToCity(Long id) {
        City city = cityRepository.findById(id)
                .orElseThrow(() -> new RuntimeException("Settlement not found"));
        city.upgradeToCity();
        return cityRepository.save(city);
    }
}
