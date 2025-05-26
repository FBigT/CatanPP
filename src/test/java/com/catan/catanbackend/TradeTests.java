package com.catan.catanbackend;

import com.catan.catanbackend.model.ResourceGroup;
import com.catan.catanbackend.model.dto.BankTradeDto;
import com.catan.catanbackend.model.dto.LogInForm;
import com.catan.catanbackend.model.dto.LogInResponse;
import com.catan.catanbackend.model.dto.PlayerTradeDto;
import com.catan.catanbackend.model.dto.RegisterForm;
import com.catan.catanbackend.service.UserService;
import com.fasterxml.jackson.databind.ObjectMapper;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.springframework.boot.test.autoconfigure.web.servlet.AutoConfigureMockMvc;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.test.context.ActiveProfiles;
import org.springframework.test.context.TestConstructor;
import org.springframework.test.context.bean.override.mockito.MockitoBean;
import org.springframework.http.HttpHeaders;
import org.springframework.http.MediaType;
import org.springframework.test.web.servlet.MockMvc;
import org.springframework.test.web.servlet.MvcResult;

import static org.mockito.ArgumentMatchers.*;
import static org.mockito.Mockito.doNothing;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.post;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.status;

@ActiveProfiles("test")
@SpringBootTest
@AutoConfigureMockMvc
@TestConstructor(autowireMode = TestConstructor.AutowireMode.ALL)
class TradeTests {

    private final MockMvc mockMvc;
    private final ObjectMapper objectMapper;
    private final UserService userService;

    @MockitoBean
    private com.catan.catanbackend.service.TradeService tradeService;

    public TradeTests(MockMvc mockMvc, ObjectMapper objectMapper, UserService userService) {
        this.mockMvc = mockMvc;
        this.objectMapper = objectMapper;
        this.userService = userService;
    }

    @BeforeEach
    void setup() {
        userService.deleteAllUsers();
        doNothing().when(tradeService).tradeBetweenPlayers(anyLong(), anyString(), anyString(), any(ResourceGroup.class), any(ResourceGroup.class));
        doNothing().when(tradeService).tradeWithBank(anyLong(), anyString(), any(ResourceGroup.class), any(ResourceGroup.class), anyString(), anyInt());
    }

    private LogInResponse registerAndLogin(RegisterForm form) throws Exception {
        mockMvc.perform(post("/api/users/register")
                .contentType(MediaType.APPLICATION_JSON)
                .content(objectMapper.writeValueAsString(form)));

        MvcResult mvcResult = mockMvc.perform(post("/api/users/login")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(objectMapper.writeValueAsString(new LogInForm(form.getUsername(), form.getPassword()))))
                .andReturn();

        return objectMapper.readValue(mvcResult.getResponse().getContentAsString(), LogInResponse.class);
    }

    @Test
    void testPlayerToPlayerTrade() throws Exception {
        LogInResponse player1 = registerAndLogin(new RegisterForm("player1", "pass", "p1@test.com"));
        LogInResponse player2 = registerAndLogin(new RegisterForm("player2", "pass", "p2@test.com"));

        PlayerTradeDto dto = new PlayerTradeDto();
        dto.setSessionId(1L);
        dto.setFromUser(player1.getUserId().toString());
        dto.setToUser(player2.getUserId().toString());
        dto.setOffered(new ResourceGroup(1, 0, 0, 0, 2, 0, 0, 3));
        dto.setRequested(new ResourceGroup(0, 0, 2, 0, 0, 1, 1, 0));

        mockMvc.perform(post("/api/trade/player")
                        .header(HttpHeaders.AUTHORIZATION, player1.getFullToken())
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(objectMapper.writeValueAsString(dto)))
                .andExpect(status().isOk());
    }

    @Test
    void testTradeWithBank() throws Exception {
        LogInResponse player = registerAndLogin(new RegisterForm("banker", "pass", "bank@test.com"));

        BankTradeDto dto = new BankTradeDto();
        dto.setSessionId(1L);
        dto.setFromUser(player.getUserId().toString());
        dto.setOffered(new ResourceGroup(0, 4, 0, 0, 0, 0, 0, 0));
        dto.setRequested(new ResourceGroup(0, 0, 0, 0, 0, 0, 1, 0));
        dto.setPortType("GENERIC");
        dto.setPortRatio(4);

        mockMvc.perform(post("/api/trade/bank")
                        .header(HttpHeaders.AUTHORIZATION, player.getFullToken())
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(objectMapper.writeValueAsString(dto)))
                .andExpect(status().isOk());
    }
}
