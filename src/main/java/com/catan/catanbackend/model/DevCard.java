package com.catan.catanbackend.model;

import com.catan.catanbackend.model.helper.DevCardType;
import com.fasterxml.jackson.annotation.JsonIgnore;
import com.fasterxml.jackson.annotation.JsonIgnoreProperties;
import com.fasterxml.jackson.annotation.JsonProperty;
import jakarta.persistence.*;
import lombok.Getter;
import lombok.NoArgsConstructor;
import lombok.Setter;

@Entity
@Table(name = "dev_cards")
@Getter @Setter @NoArgsConstructor
@JsonIgnoreProperties({ "hibernateLazyInitializer", "handler" })
public class DevCard {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;

    @Enumerated(EnumType.STRING)
    @Column(nullable = false)
    private DevCardType type;

    @JsonIgnore
    @ManyToOne(fetch = FetchType.LAZY)
    @JoinColumn(name = "session_id")
    private Session session;

    /** null = still in deck; once bought, set to the SessionPlayer who owns it */
    @JsonIgnore
    @ManyToOne(fetch = FetchType.EAGER)
    @JoinColumn(name = "owner_id")
    private SessionPlayer owner;

    @JsonProperty("ownerId")
    public Long extractOwnerId(){
        if(owner == null){
            return null;
        }
        return owner.getId();
    }

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
