using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Utils;
using Assets.Scripts.Models;
using Assets.Scripts.Dtos.GameMoves;
using Assets.Scripts.Dtos;
using Assets.Scripts.Enums;
using Assets.Scripts.Dtos.GameMoveResponses;
using System;
using System.Linq;
using System.Collections;
using System.Text;
using UnityEngine.Networking;
using Assets.Scripts.GameMode.Trading;

namespace Assets.Scripts.DevCards.Core
{
    public class DevCardManager : MonoBehaviour
    {
        public static DevCardManager Instance { get; private set; }
        // ADD THESE MISSING FIELDS:
        private DateTime lastBuyRequestTime;
        private int buyRequestsSent = 0;
        private int buyResponsesReceived = 0;
        [Header("Dev Cards")]
        public List<DevCardDto> playerCards = new List<DevCardDto>();

        [Header("Debug Settings")]
        public bool enableVerboseLogging = true;
        public bool enableEventDebugging = true;

        // Events for UI
        public event Action<List<DevCardDto>> OnCardsUpdated;
        public event Action<string> OnCardBought;
        public event Action<string> OnError;

        // Dependencies
        private DevCardService devCardService;
        private long cachedSessionPlayerId = -1;
        private long cachedSessionId = -1;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                DebugLog("DevCardManager instance created");
            }
            else
            {
                DebugLog("Destroying duplicate DevCardManager instance");
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            DebugLog("DevCardManager Start() called");
            ExtractUserIdFromToken();

            devCardService = GetComponent<DevCardService>();
            if (devCardService == null)
            {
                devCardService = gameObject.AddComponent<DevCardService>();
                DebugLog("Created new DevCardService component");
            }
            else
            {
                DebugLog("Found existing DevCardService component");
            }

            SubscribeToWebSocketEvents();
            AddWebSocketDebugging();
            DebugSessionInfo();

            // Subscribe to TradingManager's OnPlayersLoaded event (same as StructurePlacer)
            Assets.Scripts.GameMode.Trading.TradingManager.OnPlayersLoaded += HandlePlayersLoaded;
            DebugLog("✅ Subscribed to TradingManager.OnPlayersLoaded event");

            // Initialize session and load cards
            StartCoroutine(InitializeSessionAndLoadCards());
        }

        private IEnumerator InitializeSessionAndLoadCards()
        {
            DebugLog("🔄 Initializing session and loading dev cards...");

            // Get session ID first
            yield return StartCoroutine(GetSessionIdFromCode());

            if (cachedSessionId > 0)
            {
                DebugLog($"✅ Session ID initialized: {cachedSessionId}");

                // Get session players (same as TradingManager does)
                yield return StartCoroutine(GetSessionPlayersFromAPI());

                if (cachedSessionPlayerId > 0)
                {
                    DebugLog($"✅ SessionPlayer ID initialized: {cachedSessionPlayerId}");
                    LoadPlayerCards();
                }
                else
                {
                    Debug.LogError("❌ Failed to get SessionPlayer ID");
                }
            }
            else
            {
                Debug.LogError("❌ Failed to get Session ID");
            }
        }

        private void HandlePlayersLoaded(List<Assets.Scripts.GameMode.Trading.Models.SessionPlayerDto> players)
        {
            DebugLog($"[DevCardManager] TradingManager loaded {players.Count} players");

            string currentUsername = LocalStorageService.GetString("username");
            var myPlayer = players.FirstOrDefault(p => p.username == currentUsername);

            if (myPlayer != null)
            {
                cachedSessionPlayerId = myPlayer.id;
                DebugLog($"✅ [DevCardManager] Got SessionPlayer ID from TradingManager: {cachedSessionPlayerId}");

                // Store in PlayerPrefs
                PlayerPrefs.SetString("sessionPlayerId", cachedSessionPlayerId.ToString());
                PlayerPrefs.Save();

                // Load dev cards with correct sessionPlayerId
                LoadPlayerCards();
            }
            else
            {
                Debug.LogError($"❌ Could not find user '{currentUsername}' in TradingManager players");
            }
        }

        #region Auth refresh (copied from TradingManager)
        private IEnumerator EnsureValidToken()
        {
            string jwt = LocalStorageService.GetString("token");
            string refresh = LocalStorageService.GetString("refresh-token");

            DebugLog($"[TokenCheck] Existing JWT: {jwt}");
            DebugLog($"[TokenCheck] Refresh token: {refresh}");

            if (SecurityUtils.IsTokenValid(jwt))
            {
                DebugLog("[TokenCheck] JWT is still valid.");
                yield break;
            }

            if (string.IsNullOrEmpty(refresh))
            {
                Debug.LogError("[TokenCheck] No refresh token available.");
                yield break;
            }

            var body = System.Text.Encoding.UTF8.GetBytes($"\"{refresh}\"");
            using UnityWebRequest req = new UnityWebRequest(EndpointUtils.Refresh, "POST")
            {
                uploadHandler = new UploadHandlerRaw(body),
                downloadHandler = new DownloadHandlerBuffer()
            };
            req.SetRequestHeader("Content-Type", "application/json");

            DebugLog("[TokenCheck] Attempting to refresh token...");
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                AuthResponse resp = JsonUtility.FromJson<AuthResponse>(req.downloadHandler.text);
                string newToken = resp.tokenType + " " + resp.token;

                LocalStorageService.SetVariable("token", newToken);
                LocalStorageService.SetVariable("refresh-token", resp.refreshToken);

                DebugLog("[TokenCheck] Token refresh successful.");
            }
            else
            {
                Debug.LogError("[TokenCheck] Token refresh failed: " + req.error);
            }
        }
        #endregion

        #region Get Session ID (same pattern as TradingManager)
        private IEnumerator GetSessionIdFromCode()
        {
            string sessionCode = LocalStorageService.GetString("session-code");
            if (string.IsNullOrEmpty(sessionCode))
            {
                Debug.LogError("❌ No session code found");
                yield break;
            }

            yield return StartCoroutine(EnsureValidToken());

            string jwt = LocalStorageService.GetString("token");
            if (!SecurityUtils.IsTokenValid(jwt))
            {
                Debug.LogError("❌ User not authenticated (token invalid)");
                yield break;
            }

            // Use the same endpoint pattern as TradingManager
            string url = $"http://localhost:8080/api/session/code/{sessionCode}";
            using UnityWebRequest req = UnityWebRequest.Get(url);

            // Use the EXACT same authorization header format as TradingManager
            req.SetRequestHeader("Authorization", jwt); // TradingManager uses jwt directly, not "Bearer " + jwt

            DebugLog($"[GetSessionId] GET {url}");
            DebugLog($"[GetSessionId] Authorization header: {jwt.Substring(0, 20)}...");

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[GetSessionId] {req.error} ({req.responseCode})");
                if (!string.IsNullOrEmpty(req.downloadHandler.text))
                {
                    Debug.LogError($"[GetSessionId] Response body: {req.downloadHandler.text}");
                }
                yield break;
            }

            DebugLog("[GetSessionId] Response: " + req.downloadHandler.text);

            try
            {
                var sessionData = JsonUtility.FromJson<SessionDto>(req.downloadHandler.text);
                if (sessionData != null && sessionData.id > 0)
                {
                    cachedSessionId = sessionData.id;
                    DebugLog($"✅ Got session ID: {cachedSessionId}");
                }
                else
                {
                    Debug.LogError("❌ Invalid session data received");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[GetSessionId] JSON parse error: " + ex.Message);
            }
        }

        #endregion

        #region Get Session Players (exactly like TradingManager)
        private IEnumerator GetSessionPlayersFromAPI()
        {
            if (cachedSessionId <= 0)
            {
                Debug.LogError("❌ Invalid session ID");
                yield break;
            }

            yield return StartCoroutine(EnsureValidToken());

            string jwt = LocalStorageService.GetString("token");
            if (!SecurityUtils.IsTokenValid(jwt))
            {
                Debug.LogError("❌ User not authenticated (token invalid)");
                yield break;
            }

            // Use the same endpoint as TradingManager
            string url = EndpointUtils.GetSessionPlayers(cachedSessionId);
            using UnityWebRequest req = UnityWebRequest.Get(url);
            req.SetRequestHeader("Authorization", jwt);

            DebugLog($"[GetSessionPlayers] GET {url}");
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[GetSessionPlayers] {req.error} ({req.responseCode})");
                yield break;
            }

            DebugLog("[GetSessionPlayers] Response: " + req.downloadHandler.text);

            try
            {

                

                // Alternative without JsonHelper:
                string jsonResponse = req.downloadHandler.text;
                string wrappedJson = $"{{\"Items\":{jsonResponse}}}";
                var wrapper = JsonUtility.FromJson<SessionPlayerArrayWrapper>(wrappedJson);
                Assets.Scripts.GameMode.Trading.Models.SessionPlayerDto[] arr = wrapper.Items;

                var list = new List<Assets.Scripts.GameMode.Trading.Models.SessionPlayerDto>(arr);

                string currentUsername = LocalStorageService.GetString("username");
                var myPlayer = list.FirstOrDefault(p => p.username == currentUsername);

                if (myPlayer != null)
                {
                    cachedSessionPlayerId = myPlayer.id;
                    DebugLog($"✅ Found my SessionPlayer ID: {cachedSessionPlayerId}");

                    // Store in PlayerPrefs
                    PlayerPrefs.SetString("sessionPlayerId", cachedSessionPlayerId.ToString());
                    PlayerPrefs.Save();
                }
                else
                {
                    Debug.LogError($"❌ Could not find user '{currentUsername}' in session players");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[GetSessionPlayers] JSON parse error: " + ex.Message);
            }
        }
        #endregion

        private void ExtractUserIdFromToken()
        {
            DebugLog("=== EXTRACTING USER ID FROM JWT TOKEN ===");

            string token = LocalStorageService.GetString("token");
            if (string.IsNullOrEmpty(token))
            {
                Debug.LogError("❌ No token found in LocalStorage!");
                return;
            }

            try
            {
                // Remove 'Bearer ' prefix if present
                string cleanToken = token;
                if (token.StartsWith("Bearer "))
                {
                    cleanToken = token.Substring(7);
                    DebugLog("✅ Removed 'Bearer ' prefix from token");
                }

                string[] tokenParts = cleanToken.Split('.');
                if (tokenParts.Length < 2)
                {
                    Debug.LogError("❌ Invalid JWT token format - not enough parts");
                    return;
                }

                DebugLog($"✅ Token split into {tokenParts.Length} parts");

                string payload = tokenParts[1];
                DebugLog($"✅ Extracted payload: {payload.Substring(0, Math.Min(20, payload.Length))}...");

                while (payload.Length % 4 != 0)
                {
                    payload += "=";
                }
                DebugLog($"✅ Added padding, final length: {payload.Length}");

                byte[] jsonBytes = Convert.FromBase64String(payload);
                string jsonString = Encoding.UTF8.GetString(jsonBytes);

                DebugLog($"✅ Decoded JWT payload: {jsonString}");

                var tokenData = JsonUtility.FromJson<TokenPayload>(jsonString);

                if (tokenData == null || tokenData.id <= 0)
                {
                    Debug.LogError("❌ Failed to parse token data or invalid user ID");
                    return;
                }

                DebugLog($"✅ Successfully extracted User ID: {tokenData.id}");

                // Store as string in PlayerPrefs
                PlayerPrefs.SetString("userId", tokenData.id.ToString());
                PlayerPrefs.Save();
                DebugLog($"✅ User ID {tokenData.id} stored in PlayerPrefs!");
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ Failed to extract User ID from JWT token: {ex.Message}");
            }
        }

        private void SubscribeToWebSocketEvents()
        {
            DebugLog("Subscribing to WebSocket events...");

            try
            {
                WebSocketService.OnBuyCardResponse += HandleBuyCardResponse;
                DebugLog("✅ Subscribed to OnBuyCardResponse");

                WebSocketService.OnPrivateBuyCard += HandlePrivateBuyCard;
                DebugLog("✅ Subscribed to OnPrivateBuyCard");

                WebSocketService.OnPlayCardResponse += HandlePlayCardResponse;
                DebugLog("✅ Subscribed to OnPlayCardResponse");
                // ✅ ADD THIS MISSING SUBSCRIPTION:
                WebSocketService.OnDevCardsListReceived += HandleDevCardsListReceived;
                DebugLog("✅ Subscribed to OnDevCardsListReceived");
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ Failed to subscribe to WebSocket events: {ex.Message}");
            }
        }

        private void AddWebSocketDebugging()
        {
            if (!enableEventDebugging) return;

            DebugLog("Adding WebSocket debugging...");

            WebSocketService.OnBuyCardResponse += (response) => {
                DebugLog($"[WEBSOCKET] OnBuyCardResponse fired: {response.username} (cards: {response.numberOfCards})");
            };

            WebSocketService.OnPrivateBuyCard += (card) => {
                DebugLog($"[WEBSOCKET] OnPrivateBuyCard fired: {card.devCardType} (ID: {card.cardId})");
            };

            WebSocketService.OnPlayCardResponse += (response) => {
                DebugLog($"[WEBSOCKET] OnPlayCardResponse fired: {response.devCardType}");
            };
        }

        private void DebugSessionInfo()
        {
            string sessionCode = LocalStorageService.GetString("session-code");
            string userIdStr = PlayerPrefs.GetString("userId", "");
            string sessionPlayerIdStr = PlayerPrefs.GetString("sessionPlayerId", "");

            long userId = -1;
            if (!string.IsNullOrEmpty(userIdStr) && long.TryParse(userIdStr, out long parsedUserId))
            {
                userId = parsedUserId;
            }

            long sessionPlayerId = -1;
            if (!string.IsNullOrEmpty(sessionPlayerIdStr) && long.TryParse(sessionPlayerIdStr, out long parsedSessionPlayerId))
            {
                sessionPlayerId = parsedSessionPlayerId;
            }

            string token = LocalStorageService.GetString("token");
            bool wsConnected = WebSocketService.Connected;

            DebugLog($"[SESSION INFO]");
            DebugLog($"  Session Code: {sessionCode ?? "NULL"}");
            DebugLog($"  Session ID: {cachedSessionId}");
            DebugLog($"  User ID: {userId}");
            DebugLog($"  SessionPlayer ID: {sessionPlayerId}");
            DebugLog($"  Cached SessionPlayer ID: {cachedSessionPlayerId}");
            DebugLog($"  Token: {(string.IsNullOrEmpty(token) ? "MISSING" : "Present")}");
            DebugLog($"  WebSocket Connected: {wsConnected}");

            if (cachedSessionPlayerId <= 0)
            {
                Debug.LogError("❌ SESSIONPLAYER ID IS INVALID! This is why dev cards aren't working!");
            }
            else
            {
                DebugLog($"✅ SessionPlayer ID looks valid: {cachedSessionPlayerId}");
            }
        }

        public async void BuyDevCard()
        {
            DebugLog("=== BUY DEV CARD ATTEMPT ===");
            DebugLog("BuyDevCard() method called");

            if (!WebSocketService.Connected)
            {
                Debug.LogError("❌ WebSocket not connected!");
                OnError?.Invoke("Not connected to game session");
                return;
            }
            DebugLog("✅ WebSocket connection verified");

            try
            {
                var gameMove = new GameMoveDto(GameMoveType.BUY_CARD);
                DebugLog($"✅ Created GameMoveDto: {Newtonsoft.Json.JsonConvert.SerializeObject(gameMove)}");

                DebugLog("📤 Sending BUY_CARD via SendGameMove");
                await WebSocketService.SendGameMove(gameMove);
                DebugLog("✅ Buy dev card request sent successfully");

                Invoke(nameof(CheckForBuyCardResponse), 2f);
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ Failed to send buy dev card: {ex.Message}");
                OnError?.Invoke("Failed to send buy request: " + ex.Message);
            }
        }

        private void CheckForBuyCardResponse()
        {
            DebugLog("⏱️ === DETAILED BUY CARD RESPONSE CHECK ===");
            DebugLog($"⏱️ Time since last buy request: {(DateTime.Now - lastBuyRequestTime).TotalSeconds:F1} seconds");
            DebugLog($"📊 Buy requests sent: {buyRequestsSent}");
            DebugLog($"📊 Buy responses received: {buyResponsesReceived}");

            // Check WebSocket connection status
            bool wsConnected = WebSocketService.Connected;
            DebugLog($"📡 WebSocket Connected: {wsConnected}");

            if (!wsConnected)
            {
                Debug.LogError("❌ WebSocket disconnected! This explains why no response was received.");
                OnError?.Invoke("WebSocket connection lost");
                return;
            }

            // Check if we missed a response
            if (buyResponsesReceived < buyRequestsSent)
            {
                Debug.LogWarning($"⚠️ Missing response! Expected {buyRequestsSent}, got {buyResponsesReceived}");
                DebugLog("🔍 Possible causes:");
                DebugLog("  1. Backend validation failed (insufficient resources)");
                DebugLog("  2. Not your turn / game state restrictions");
                DebugLog("  3. Backend exception occurred");
                DebugLog("  4. WebSocket message routing failed");
                DebugLog("  5. JSON deserialization failed");
                DebugLog("  6. Event subscription not working");

                // Check current resources
                DebugCurrentPlayerResources();

                // Check session state
                DebugSessionState();

                // Force reload cards to see if anything changed
                DebugLog("🔄 Force reloading cards to check for changes...");
                LoadPlayerCards();
            }
            else
            {
                DebugLog("✅ Response received successfully!");
            }
        }
        private void DebugCurrentPlayerResources()
        {
            DebugLog("💰 === CURRENT PLAYER RESOURCES ===");
            // You'll need to add a way to get current resources
            // This might require calling your resource service or checking PlayerPrefs
            DebugLog("💰 (Add resource checking logic here)");
        }
        private void DebugSessionState()
        {
            DebugLog("🎮 === CURRENT SESSION STATE ===");
            string sessionCode = LocalStorageService.GetString("session-code");
            string username = LocalStorageService.GetString("username");

            DebugLog($"🎮 Session Code: {sessionCode}");
            DebugLog($"🎮 Username: {username}");
            DebugLog($"🎮 SessionPlayer ID: {cachedSessionPlayerId}");
            DebugLog($"🎮 Current Cards: {playerCards.Count}");

            foreach (var card in playerCards)
            {
                DebugLog($"  🃏 {card.type} (ID: {card.id}, playable: {card.playable}, used: {card.used})");
            }
        }
        public async void PlayDevCard(DevCardDto card, DevCardType type)
        {
            DebugLog($"=== PLAY DEV CARD ATTEMPT ===");
            DebugLog($"Playing dev card: {type} (ID: {card.id})");

            if (card == null)
            {
                Debug.LogError("❌ Cannot play null card");
                return;
            }

            if (!card.playable)
            {
                Debug.LogWarning($"⚠️ Card {type} is not playable (playable: {card.playable}, used: {card.used})");
            }

            try
            {
                var playCardDto = new PlayCardDto(type);
                var gameMove = new GameMoveDto(playCardDto);

                DebugLog($"📤 Sending PLAY_CARD via SendGameMove: {type}");
                await WebSocketService.SendGameMove(gameMove);
                DebugLog($"✅ Play card request sent successfully: {type}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ Failed to play card: {ex.Message}");
                OnError?.Invoke($"Failed to play {type} card");
            }
        }

        private void HandleBuyCardResponse(BuyCardResponseDto response)
        {
            DebugLog($"🎉 BUY_CARD response received!");
            DebugLog($"  Player: {response.username}");
            DebugLog($"  Total cards: {response.numberOfCards}");
            LoadPlayerCards();
        }

        private void HandlePrivateBuyCard(PrivateBuyCard privateBuyCard)
        {
            DebugLog($"🎉 PRIVATE_BUY_CARD response received!");
            DebugLog($"  Card Type: {privateBuyCard.devCardType}");
            DebugLog($"  Card ID: {privateBuyCard.cardId}");

            var newCard = new DevCardDto
            {
                id = privateBuyCard.cardId,
                type = privateBuyCard.devCardType,
                playable = false,
                used = false
            };

            playerCards.Add(newCard);
            DebugLog($"✅ Added card to local collection. Total cards: {playerCards.Count}");

            OnCardsUpdated?.Invoke(playerCards);
            OnCardBought?.Invoke($"You received a {privateBuyCard.devCardType} card!");

            DebugLog($"🎯 UI updated with new dev card: {privateBuyCard.devCardType} (ID: {privateBuyCard.cardId})");
        }

        private void HandlePlayCardResponse(PlayCardResponseDto response)
        {
            DebugLog($"🎉 PLAY_CARD response received!");
            DebugLog($"  Card Type: {response.devCardType}");

            var playedCard = playerCards.FirstOrDefault(c => !c.used && c.type == response.devCardType);
            if (playedCard != null)
            {
                playedCard.used = true;
                OnCardsUpdated?.Invoke(playerCards);
                DebugLog($"✅ Marked card as used: {response.devCardType}");
            }
            else
            {
                DebugLog($"⚠️ Could not find matching card to mark as used: {response.devCardType}");
            }
        }

        public void LoadPlayerCards()
        {
            DebugLog("=== LOAD PLAYER CARDS VIA WEBSOCKET START ===");

            if (!WebSocketService.Connected)
            {
                Debug.LogError("❌ WebSocket not connected - cannot load dev cards");
                OnError?.Invoke("Not connected to game session");
                return;
            }

            if (cachedSessionPlayerId <= 0)
            {
                Debug.LogError("❌ Invalid SessionPlayer ID - cannot load dev cards");
                OnError?.Invoke("Invalid session player ID");
                return;
            }

            try
            {
                // Create request message
                var gameMove = new GameMoveDto(GameMoveType.REQUEST_DEV_CARDS);
                string serialized = Newtonsoft.Json.JsonConvert.SerializeObject(gameMove);
                DebugLog($"✅ Created REQUEST_DEV_CARDS message: {serialized}");

                // Send via WebSocket (no StartCoroutine needed)
                SendRequestDevCards(gameMove);
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ Failed to request dev cards: {ex.Message}");
                OnError?.Invoke("Failed to request dev cards: " + ex.Message);
            }

            DebugLog("=== LOAD PLAYER CARDS VIA WEBSOCKET END ===");
        }

        private void HandleDevCardsListReceived(DevCardsListResponseDto response)
        {
            DebugLog("=== DEV CARDS LIST RECEIVED VIA WEBSOCKET ===");
            DebugLog($"📥 Received {response.devCards.Count} dev cards for {response.username}");

            // Update local cards list
            playerCards.Clear();
            playerCards.AddRange(response.devCards);

            // Debug the received cards - this should now show correct types!
            DebugLog("📋 Received dev cards from WebSocket:");
            for (int i = 0; i < playerCards.Count; i++)
            {
                var card = playerCards[i];
                DebugLog($"  {i + 1}. {card.type} (ID: {card.id}, playable: {card.playable}, used: {card.used})");
            }

            // Notify UI
            OnCardsUpdated?.Invoke(playerCards);

            if (enableEventDebugging)
            {
                DebugLog("🔔 OnCardsUpdated event fired with correct card types!");
            }

            DebugLog("=== DEV CARDS LIST PROCESSING COMPLETE ===");
        }


        private async void SendRequestDevCards(GameMoveDto gameMove)
        {
            DebugLog("📤 Sending REQUEST_DEV_CARDS via WebSocket...");

            try
            {
                await WebSocketService.SendGameMove(gameMove);
                DebugLog("✅ REQUEST_DEV_CARDS sent successfully");
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ Failed to send REQUEST_DEV_CARDS: {ex.Message}");
                OnError?.Invoke("Failed to send dev cards request: " + ex.Message);
            }
        }



        public List<DevCardDto> GetPlayerCards()
        {
            return new List<DevCardDto>(playerCards);
        }

        private long GetSessionPlayerId()
        {
            // First check cached value
            if (cachedSessionPlayerId > 0)
            {
                DebugLog($"✅ Using cached SessionPlayer ID: {cachedSessionPlayerId}");
                return cachedSessionPlayerId;
            }

            // Then try PlayerPrefs
            string sessionPlayerIdStr = PlayerPrefs.GetString("sessionPlayerId", "");
            if (!string.IsNullOrEmpty(sessionPlayerIdStr) && long.TryParse(sessionPlayerIdStr, out long sessionPlayerId))
            {
                DebugLog($"✅ Found SessionPlayer ID in PlayerPrefs: {sessionPlayerId}");
                cachedSessionPlayerId = sessionPlayerId; // Cache it
                return sessionPlayerId;
            }

            Debug.LogError("❌ SessionPlayer ID not found!");
            return -1;
        }

        [ContextMenu("Debug Current State")]
        public void DebugCurrentState()
        {
            DebugLog("=== CURRENT DEV CARD MANAGER STATE ===");
            DebugLog($"Player Cards Count: {playerCards.Count}");
            DebugLog($"WebSocket Connected: {WebSocketService.Connected}");
            DebugSessionInfo();
        }

        [ContextMenu("Refresh SessionPlayer ID")]
        public void RefreshSessionPlayerId()
        {
            cachedSessionPlayerId = -1; // Clear cache
            cachedSessionId = -1;
            StartCoroutine(InitializeSessionAndLoadCards());
        }

        private void DebugLog(string message)
        {
            if (enableVerboseLogging)
            {
                Debug.Log($"[DevCardManager] {message}");
            }
        }

        private void OnDestroy()
        {
            DebugLog("DevCardManager destroyed - unsubscribing from events");
            WebSocketService.OnBuyCardResponse -= HandleBuyCardResponse;
            WebSocketService.OnPrivateBuyCard -= HandlePrivateBuyCard;
            WebSocketService.OnPlayCardResponse -= HandlePlayCardResponse;

            // Unsubscribe from TradingManager event
            Assets.Scripts.GameMode.Trading.TradingManager.OnPlayersLoaded -= HandlePlayersLoaded;
        }

        // Helper DTOs (same as TradingManager)
        [System.Serializable]
        public class TokenPayload
        {
            public string sub;
            public long id;
            public string jti;
            public long iat;
            public long exp;
        }

        [System.Serializable]
        public class SessionDto
        {
            public long id;
            public string code;
            public string status;
        }

        [System.Serializable]
        class AuthResponse
        {
            public string tokenType;
            public string token;
            public string refreshToken;
        }
        [System.Serializable]
        private class SessionPlayerArrayWrapper
        {
            public Assets.Scripts.GameMode.Trading.Models.SessionPlayerDto[] Items;
        }

    }
}
