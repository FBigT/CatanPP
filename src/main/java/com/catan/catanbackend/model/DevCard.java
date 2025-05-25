package com.catan.catanbackend.model;

import com.catan.catanbackend.model.helper.DevCardType;
import jakarta.persistence.*;
import lombok.Getter;
import lombok.NoArgsConstructor;
import lombok.Setter;

@Entity
@Table(name = "dev_cards")
@Getter @Setter @NoArgsConstructor
public class DevCard {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;

    @Enumerated(EnumType.STRING)
    @Column(nullable = false)
    private DevCardType type;

    @ManyToOne(fetch = FetchType.LAZY)
    @JoinColumn(name = "session_id")
    private Session session;

    /** null = still in deck; once bought, set to the SessionPlayer who owns it */
    @ManyToOne(fetch = FetchType.LAZY)
    @JoinColumn(name = "owner_id")
    private SessionPlayer owner;

    /** only usable starting next turn */
    @Column(nullable = false)
    private boolean playable = false;

    /** once played (e.g. knight used), mark true */
    @Column(nullable = false)
    private boolean used = false;

    public DevCard(DevCardType type, Session session) {
        this.type = type;
        this.session = session;
    }
}
