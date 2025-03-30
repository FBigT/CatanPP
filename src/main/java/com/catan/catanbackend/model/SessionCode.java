package com.catan.catanbackend.model;

import jakarta.persistence.*;
import jakarta.validation.constraints.NotNull;
import jakarta.validation.constraints.Size;
import lombok.Data;
import lombok.NoArgsConstructor;

@Data
@Entity
@Table(name = "session_codes")
@NoArgsConstructor
public class SessionCode {
    public SessionCode(Session session, String code) {
        this.session = session;
        this.code = code;
    }

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    @Column(name = "id", nullable = false)
    private Long id;

    @NotNull
    @ManyToOne(fetch = FetchType.LAZY, optional = false)
    @JoinColumn(name = "session_id", nullable = false)
    private Session session;

    @Size(max = 6)
    @NotNull
    @Column(name = "code", nullable = false, length = 6)
    private String code;
}