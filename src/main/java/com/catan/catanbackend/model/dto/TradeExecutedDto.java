package com.catan.catanbackend.model.dto;

import com.catan.catanbackend.model.ResourceGroup;

/**
 * When a trade is accepted, the server will broadcast a GameMoveDto of type "TRADE_EXECUTED"
 * whose moveData is this POJO.  Clients will then remove/add resources locally.
 */
public class TradeExecutedDto {
    private String        fromUser;
    private String        toUser;
    private ResourceGroup offered;
    private ResourceGroup requested;

    // no-arg constructor for Jackson
    public TradeExecutedDto() { }

    public TradeExecutedDto(String fromUser, String toUser, ResourceGroup offered, ResourceGroup requested) {
        this.fromUser = fromUser;
        this.toUser   = toUser;
        this.offered  = offered;
        this.requested= requested;
    }

    // ── Getters & setters ──
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
