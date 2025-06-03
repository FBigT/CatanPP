package com.catan.catanbackend.model.dto.move_dtos.responses;

import com.catan.catanbackend.model.ResourceGroup;


public class TradeResponseMessage {
    private Long           sessionId;
    private String         fromUser;
    private String         toUser;
    private boolean        accepted;
    private ResourceGroup  offered;
    private ResourceGroup  requested;

    // ── no‐arg constructor is required for Jackson deserialization ──
    public TradeResponseMessage() { }

    // ── Getters & setters ──
    public Long getSessionId() {
        return sessionId;
    }
    public void setSessionId(Long sessionId) {
        this.sessionId = sessionId;
    }

    public String getFromUser() {
        return fromUser;
    }
    public void setFromUser(String fromUser) {
        this.fromUser = fromUser;
    }

    public String getToUser() {
        return toUser;
    }
    public void setToUser(String toUser) {
        this.toUser = toUser;
    }

    public boolean isAccepted() {
        return accepted;
    }
    public void setAccepted(boolean accepted) {
        this.accepted = accepted;
    }

    public ResourceGroup getOffered() {
        return offered;
    }
    public void setOffered(ResourceGroup offered) {
        this.offered = offered;
    }

    public ResourceGroup getRequested() {
        return requested;
    }
    public void setRequested(ResourceGroup requested) {
        this.requested = requested;
    }
}
