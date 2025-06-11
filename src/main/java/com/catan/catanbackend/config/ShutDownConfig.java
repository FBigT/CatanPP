package com.catan.catanbackend.config;

import com.zaxxer.hikari.HikariDataSource;
import jakarta.annotation.PreDestroy;
import org.springframework.context.annotation.Configuration;

import javax.sql.DataSource;

@Configuration
public class ShutDownConfig {
    private final DataSource dataSource;

    public ShutDownConfig(DataSource dataSource) {
        this.dataSource = dataSource;
    }

    @PreDestroy
    public void onShutdown() {
        if (dataSource instanceof HikariDataSource hikari) {
            hikari.close();
        }
    }
}
