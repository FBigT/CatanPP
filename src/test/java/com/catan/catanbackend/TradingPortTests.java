package com.catan.catanbackend;

import com.catan.catanbackend.model.TradingPort;
import com.catan.catanbackend.service.TradingPortService;
import com.fasterxml.jackson.databind.ObjectMapper;
import org.junit.jupiter.api.Test;
import org.springframework.boot.test.autoconfigure.web.servlet.AutoConfigureMockMvc;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.test.context.ActiveProfiles;
import org.springframework.test.context.TestConstructor;
import org.springframework.test.context.bean.override.mockito.MockitoBean;
import org.springframework.http.MediaType;
import org.springframework.test.web.servlet.MockMvc;

import java.util.List;

import static org.mockito.Mockito.doReturn;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.*;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.*;

@ActiveProfiles("test")
@SpringBootTest
@AutoConfigureMockMvc(addFilters = false)
@TestConstructor(autowireMode = TestConstructor.AutowireMode.ALL)
class TradingPortTests {

    private final MockMvc mockMvc;
    private final ObjectMapper objectMapper;

    @MockitoBean
    private TradingPortService tradingPortService;

    public TradingPortTests(MockMvc mockMvc, ObjectMapper objectMapper) {
        this.mockMvc = mockMvc;
        this.objectMapper = objectMapper;
    }

    private TradingPort p(Long id, String type, int ratio, boolean placed) {
        TradingPort tp = new TradingPort();
        tp.setId(id);
        tp.setType(type);
        tp.setTradeRatio(ratio);
        tp.setPlaced(placed);
        return tp;
    }

    @Test
    void testGetAllTradingPorts() throws Exception {
        var ports = List.of(p(1L, "WOOD", 3, false), p(2L, "ORE", 2, true));
        doReturn(ports).when(tradingPortService).getAllTradingPorts();

        mockMvc.perform(get("/api/trading-ports"))
                .andExpect(status().isOk())
                .andExpect(content().contentType(MediaType.APPLICATION_JSON))
                .andExpect(jsonPath("$.length()").value(2))
                .andExpect(jsonPath("$[0].type").value("WOOD"));
    }

    @Test
    void testGetPortsByUsername() throws Exception {
        String user = "alice";
        doReturn(List.of(p(3L, "GENERIC", 4, false)))
                .when(tradingPortService).getPortsByUsername(user);

        mockMvc.perform(get("/api/trading-ports/{username}", user))
                .andExpect(status().isOk())
                .andExpect(jsonPath("$[0].tradeRatio").value(4));
    }

    @Test
    void testCreateTradingPort() throws Exception {
        String type = "SHEEP_PORT";
        int ratio = 2;
        String user = "bob";
        var created = p(10L, type, ratio, false);
        doReturn(created).when(tradingPortService).createTradingPort(type, ratio, user);

        mockMvc.perform(post("/api/trading-ports")
                        .param("type", type)
                        .param("tradeRatio", String.valueOf(ratio))
                        .param("username", user))
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.id").value(10))
                .andExpect(jsonPath("$.type").value(type));
    }

    @Test
    void testUpdatePortPlacement() throws Exception {
        Long id = 5L;
        boolean newPlaced = true;
        var updated = p(id, "WHEAT_PORT", 3, newPlaced);
        doReturn(updated).when(tradingPortService).updatePortPlacement(id, newPlaced);

        mockMvc.perform(put("/api/trading-ports/{id}/place", id)
                        .param("isPlaced", String.valueOf(newPlaced)))
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.placed").value(true));
    }
}
