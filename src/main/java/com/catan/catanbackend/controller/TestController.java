package com.catan.catanbackend.controller;

import jakarta.annotation.PostConstruct;
import org.junit.platform.launcher.EngineFilter;
import org.junit.platform.launcher.Launcher;
import org.junit.platform.launcher.LauncherDiscoveryRequest;
import org.junit.platform.launcher.core.LauncherDiscoveryRequestBuilder;
import org.junit.platform.launcher.core.LauncherFactory;
import org.junit.platform.launcher.listeners.SummaryGeneratingListener;
import org.junit.platform.launcher.listeners.TestExecutionSummary;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.CrossOrigin;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import java.net.URL;

import static org.junit.platform.engine.discovery.DiscoverySelectors.selectClass;
import static org.junit.platform.engine.discovery.DiscoverySelectors.selectPackage;

@CrossOrigin
@RestController
@RequestMapping("/api/tests")
public class TestController {
    @PostConstruct
    public void disableAdminMBean() {
        // Must happen before any SpringApplication context is actually launched
        System.setProperty("spring.application.admin.enabled", "false");
    }

    @PostMapping("/run")
    public ResponseEntity<String> runTests() {
        LauncherDiscoveryRequest request = LauncherDiscoveryRequestBuilder.request()
                .selectors(selectPackage("com.catan.catanbackend"))
                .build();

        Launcher launcher = LauncherFactory.create();
        SummaryGeneratingListener listener = new SummaryGeneratingListener();
        launcher.registerTestExecutionListeners(listener);
        launcher.execute(request);

        TestExecutionSummary summary = listener.getSummary();
        long runCount     = summary.getTestsFoundCount();
        long failedCount  = summary.getTestsFailedCount();
        long skippedCount = summary.getTestsSkippedCount();

        // If there were failures/exceptions, you can include details in the response:
        StringBuilder sb = new StringBuilder();
        sb.append(String.format("Tests run: %d, Failures: %d, Skipped: %d%n", runCount, failedCount, skippedCount));

        if (failedCount > 0) {
            sb.append("Failed tests details:\n");
            for (TestExecutionSummary.Failure failure : summary.getFailures()) {
                sb.append(String.format(" - %s: %s%n",
                        failure.getTestIdentifier().getDisplayName(),
                        failure.getException().getMessage()));
            }
        }

        return ResponseEntity.ok(sb.toString());
    }
}
