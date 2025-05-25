package com.catan.catanbackend.repository;

import com.catan.catanbackend.model.SessionPlayer;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Query;
import org.springframework.data.repository.query.Param;

import java.util.List;
import java.util.Optional;

public interface SessionPlayerRepository extends JpaRepository<SessionPlayer, Long> {
    @Query("""
    SELECT sp 
    FROM SessionPlayer sp
    JOIN FETCH sp.user
    WHERE sp.session.id = (
        SELECT sc.session.id FROM SessionCode sc WHERE sc.code = :sessionCode
    )
""")
    List<SessionPlayer> findAllBySessionCodeWithUser(@Param("sessionCode") String sessionCode);
    List<SessionPlayer> findSessionPlayerBySessionId(Long sessionId);
    List<SessionPlayer> findSessionPlayerByUserId(Long userId);
    default Optional<SessionPlayer> findPlayerBySessionCodeAndUserId(String sessionCode, Long userId) {
        return findAllBySessionCodeWithUser(sessionCode).stream().filter(x
                -> x.getUser().getId().equals(userId)).findFirst();
    }
}
