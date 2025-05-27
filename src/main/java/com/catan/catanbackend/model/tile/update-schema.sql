ALTER TABLE sessions
    ADD in_setup BOOLEAN;

ALTER TABLE session_players
    ADD roads_placed INTEGER;

ALTER TABLE session_players
    ADD settlements_placed INTEGER;