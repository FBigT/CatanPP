# Stage 1: Build the application with Gradle
FROM gradle:8.5-jdk17 AS builder

# Copy everything into the container
COPY --chown=gradle:gradle . /home/gradle/project
WORKDIR /home/gradle/project

# Build the app (skip tests for faster deploy)
RUN gradle build -x test

# Stage 2: Run the application
FROM eclipse-temurin:17-jdk-alpine

# Set working directory
WORKDIR /app

# Copy the jar from the build stage
COPY --from=builder /home/gradle/project/build/libs/*.jar app.jar

# Expose the port Spring Boot runs on
EXPOSE 8080

# Run the jar
ENTRYPOINT ["java", "-jar", "app.jar"]
