package com.catan.catanbackend.model.dto;

public class SessionPlayerDto {
    private Long id;
    private Long sessionId;
    private Long userId;
    private String username;
    private Integer playerScore;
    private Boolean active;
    private Boolean isAi;
    private String name;
    private Integer brick;
    private Integer crystal;
    private Integer ore;
    private Integer rice;
    private Integer sheep;
    private Integer silver;
    private Integer gold;
    private Integer wood;

    public SessionPlayerDto() {}

    public SessionPlayerDto(Long id, Long sessionId, Long userId, String username,
                            Integer playerScore, Boolean active, Boolean isAi, String name,
                            Integer brick, Integer crystal, Integer ore, Integer rice,
                            Integer sheep, Integer silver, Integer gold, Integer wood) {
        this.id = id;
        this.sessionId = sessionId;
        this.userId = userId;
        this.username = username;
        this.playerScore = playerScore;
        this.active = active;
        this.isAi = isAi;
        this.name = name;
        this.brick = brick;
        this.crystal = crystal;
        this.ore = ore;
        this.rice = rice;
        this.sheep = sheep;
        this.silver = silver;
        this.gold = gold;
        this.wood = wood;
    }

    public Long getId() { return id; }
    public void setId(Long id) { this.id = id; }

    public Long getSessionId() { return sessionId; }
    public void setSessionId(Long sessionId) { this.sessionId = sessionId; }

    public Long getUserId() { return userId; }
    public void setUserId(Long userId) { this.userId = userId; }

    public String getUsername() { return username; }
    public void setUsername(String username) { this.username = username; }

    public Integer getPlayerScore() { return playerScore; }
    public void setPlayerScore(Integer playerScore) { this.playerScore = playerScore; }

    public Boolean getActive() { return active; }
    public void setActive(Boolean active) { this.active = active; }

    public Boolean getIsAi() { return isAi; }
    public void setIsAi(Boolean isAi) { this.isAi = isAi; }

    public String getName() { return name; }
    public void setName(String name) { this.name = name; }

    public Integer getBrick() { return brick; }
    public void setBrick(Integer brick) { this.brick = brick; }

    public Integer getCrystal() { return crystal; }
    public void setCrystal(Integer crystal) { this.crystal = crystal; }

    public Integer getOre() { return ore; }
    public void setOre(Integer ore) { this.ore = ore; }

    public Integer getRice() { return rice; }
    public void setRice(Integer rice) { this.rice = rice; }

    public Integer getSheep() { return sheep; }
    public void setSheep(Integer sheep) { this.sheep = sheep; }

    public Integer getSilver() { return silver; }
    public void setSilver(Integer silver) { this.silver = silver; }

    public Integer getGold() { return gold; }
    public void setGold(Integer gold) { this.gold = gold; }

    public Integer getWood() { return wood; }
    public void setWood(Integer wood) { this.wood = wood; }
}
