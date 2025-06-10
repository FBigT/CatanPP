using Assets.Scripts.Dtos;
using Assets.Scripts.Dtos.GameMoveResponses;
using Assets.Scripts.Enums;
using Assets.Scripts.GameMode.Trading.Models;
using Assets.Scripts.Utils;
using Catan.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using Assets.Scripts.DevCards.Core;
using Assets.Scripts.User;
using System.Threading.Tasks;
using Assets;

public class BoardGen : MonoBehaviour
{
    public static BoardGen Instance { get; private set; }
    private int _lastDiceTotal;
    public GameObject hexTilePrefab;
    public GameObject edgePointPrefab;
    private HexTile selectedRobberTile;
    private bool isRobberMoveInProgress;
    public bool IsRobberMoveActive() => isRobberMoveInProgress;
    public GameObject thiefPrefab;
    private GameObject thiefInstance;
    private HexTile currentThiefTile;
    public bool isGenerated = false;
    public float hexSize = 1f;

    private const int radius = 2;

    public GameObject vertexPrefab;
    private Dictionary<Vector3, VertexPoint> vertexMap = new();

    public GameObject portPrefab;

    private List<HexTile> tileList = new List<HexTile>();

    private long cachedSessionId = -1;
    private long cachedSessionPlayerId = -1;
    private bool isHost = false;
    private bool playersLoaded = false;
    public List<PortType> portTypes = new()
    {
        PortType.Generic3To1, PortType.Wood2To1, PortType.Brick2To1,
        PortType.Generic3To1, PortType.Wheat2To1, PortType.Sheep2To1,
        PortType.Generic3To1, PortType.Ore2To1, PortType.Generic3To1
    };

    Vector3 GetVertexPosition(Vector3 center, int cornerIndex, float radius)
    {
        float angle = Mathf.Deg2Rad * (60 * cornerIndex + 30);
        float x = center.x + radius * Mathf.Cos(angle);
        float z = center.z + radius * Mathf.Sin(angle);
        return new Vector3(x, 0.1f, z);
    }

    void SpawnVertexIfNotExists(Vector3 position)
    {
        position = new Vector3(Mathf.Round(position.x * 10) / 10f, 0.1f, Mathf.Round(position.z * 10) / 10f);

        if (vertexMap.ContainsKey(position)) return;

        GameObject v = Instantiate(vertexPrefab, position, Quaternion.identity);

        var vertexComp = v.GetComponent<VertexPoint>();
        vertexMap[position] = vertexComp;
    }

    private static readonly string[] resources = {
        "wood", "wood", "wood", "wood",
        "claypit", "claypit", "claypit",
        "wheat", "wheat", "wheat", "wheat",
        "pasture", "pasture", "pasture", "pasture",
        "mountain", "mountain", "mountain",
        "desert"
    };

    private static readonly int[] numberTokens = {
        5, 2, 6, 3, 8, 10, 9, 12, 11, 4,
        8, 10, 9, 4, 5, 6, 3, 11
    };

    private void Awake()
    {
        Debug.Log("[BoardGen] Awake() started");
        if (Instance == null)
        {
            Debug.Log("[BoardGen] Instance set as singleton");
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.Log("[BoardGen] Duplicate BoardGen detected, destroying...");
            Destroy(gameObject);
            return;
        }

        // Subscribe to map events early
        SubscribeToMapEvents();
        Debug.Log("[BoardGen] Awake() completed - waiting for TradingManager.OnPlayersLoaded");

        WebSocketService.OnDiceResponse += GetDiceData;
        WebSocketService.OnEndTurn += GetEndTurn;
        PuchaseUIManager.OnStructureBuilt += HandleStructurePlaced;
        WebSocketService.OnPlaceStructure += GetStructurePlacedConfirmation;
        PuchaseUIManager.OnRoadBuilt += HandleRoadPlaced;
        WebSocketService.OnPlaceRoad += GetRoadPlacedConfirmation;
    }

    private void Start()
    {
        Debug.Log("[BoardGen] Start() called");
        WebSocketService.OnRobberMoved += HandleRobberMoveResponse;
        // Subscribe to TradingManager's OnPlayersLoaded event (same as DevCardManager)
        Assets.Scripts.GameMode.Trading.TradingManager.OnPlayersLoaded += HandlePlayersLoaded;
        Debug.Log("[BoardGen] ✅ Subscribed to TradingManager.OnPlayersLoaded event");

        // Initialize session data
        StartCoroutine(InitializeSessionData());
    }
    public async void InitiateRobberMove(bool fromDiceRoll)
    {
        if (!isRobberMoveInProgress)
        {
            await RobberMoveSequence(fromDiceRoll);
        }
    }
    private async Task RobberMoveSequence(bool fromDiceRoll)
    {
        isRobberMoveInProgress = true;
        HighlightValidRobberTiles();

        selectedRobberTile = null;
        while (selectedRobberTile == null)
        {
            await Task.Yield();
        }

        var moveDto = new RobberMoveDto
        {
            originatingTileX = currentThiefTile.xCoord,
            originatingTileY = currentThiefTile.yCoord,
            destinationTileX = selectedRobberTile.xCoord,
            destinationTileY = selectedRobberTile.yCoord
        };

        try
        {
            await WebSocketService.SendRobberMove(moveDto);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Robber move failed: {ex.Message}");

        }

        isRobberMoveInProgress = false;
    }

    // Add missing GetTileByCoords method
    public HexTile GetTileByCoords(int x, int y)
    {
        return tileList.FirstOrDefault(t => t.xCoord == x && t.yCoord == y);
    }

    public void OnRobberTileSelected(HexTile tile)
    {
        if (IsValidRobberTile(tile))
        {
            selectedRobberTile = tile;
            ClearHighlights();
        }
    }

    private bool IsValidRobberTile(HexTile tile)
    {
        return tile != currentThiefTile &&
               tile.resourceType != "desert" && // Match string comparison
               !tile.isWater;
    }

    private void HighlightValidRobberTiles()
    {
        foreach (var tile in tileList)
        {
            if (IsValidRobberTile(tile))
            {
                tile.Highlight(Color.red);
            }
        }
    }

    private void ClearHighlights()
    {
        foreach (var tile in tileList)
        {
            tile.ClearHighlight();
        }
    }
    // In BoardGen.cs
    private void HandleRobberMoveResponse(RobberMoveResponse response)
    {
        var newTile = GetTileByCoords(response.destinationTileX, response.destinationTileY);
        MoveThiefTo(newTile);


    }


    private IEnumerator InitializeSessionData()
    {
        Debug.Log("[BoardGen] 🔄 Initializing session data...");

        // Get session ID first (same pattern as DevCardManager)
        yield return StartCoroutine(GetSessionIdFromCode());

        if (cachedSessionId > 0)
        {
            Debug.Log($"[BoardGen] ✅ Session ID initialized: {cachedSessionId}");
        }
        else
        {
            Debug.LogError("[BoardGen] ❌ Failed to get Session ID");
        }
    }
    public void GenerateAll()
    {
        GenerateBoard();
        List<VertexPoint> allPoints = FindObjectsByType<VertexPoint>(FindObjectsSortMode.None).ToList();
        GenerateEdgePoints(allPoints);
        PlacePorts();
        isGenerated = true;
    }
    private void HandlePlayersLoaded(List<Assets.Scripts.GameMode.Trading.Models.SessionPlayerDto> players)
    {

        if (isGenerated)
        {
            return;
        }
        Debug.Log($"[BoardGen] TradingManager loaded {players.Count} players");

        string currentUsername = LocalStorageService.GetString("username");
        var myPlayer = players.FirstOrDefault(p => p.username == currentUsername);

        if (myPlayer != null)
        {
            cachedSessionPlayerId = myPlayer.id;
            Debug.Log($"[BoardGen] ✅ Got SessionPlayer ID: {cachedSessionPlayerId}");

            // Determine if this player is the host
            // Method 1: Check if this is the first player in the list (most common pattern)
            isHost = players[0].username == currentUsername;

            // Alternative Method 2: Compare with session host (if available)
            // You can also get the session host from cached session data if needed

            Debug.Log($"[BoardGen] 🏷️ Player '{currentUsername}' is {(isHost ? "HOST" : "NON-HOST")}");

            playersLoaded = true;

            // Now trigger the appropriate behavior based on host status
            if (isHost)
            {
                TriggerHostBehavior();
            }
            else
            {
                TriggerNonHostBehavior();
            }
        }
        else
        {
            Debug.LogError($"[BoardGen] ❌ Could not find user '{currentUsername}' in players list");
        }
    }
    #region Session Management (same pattern as DevCardManager)

    private IEnumerator EnsureValidToken()
    {
        string jwt = LocalStorageService.GetString("token");
        string refresh = LocalStorageService.GetString("refresh-token");

        Debug.Log($"[BoardGen] [TokenCheck] Existing JWT: {jwt?.Substring(0, Math.Min(20, jwt?.Length ?? 0))}...");

        if (SecurityUtils.IsTokenValid(jwt))
        {
            Debug.Log("[BoardGen] [TokenCheck] JWT is still valid.");
            yield break;
        }

        if (string.IsNullOrEmpty(refresh))
        {
            Debug.LogError("[BoardGen] [TokenCheck] No refresh token available.");
            yield break;
        }

        var body = System.Text.Encoding.UTF8.GetBytes($"\"{refresh}\"");
        using UnityWebRequest req = new UnityWebRequest(EndpointUtils.Refresh, "POST")
        {
            uploadHandler = new UploadHandlerRaw(body),
            downloadHandler = new DownloadHandlerBuffer()
        };
        req.SetRequestHeader("Content-Type", "application/json");

        Debug.Log("[BoardGen] [TokenCheck] Attempting to refresh token...");
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            var resp = JsonUtility.FromJson<AuthResponse>(req.downloadHandler.text);
            string newToken = resp.tokenType + " " + resp.token;

            LocalStorageService.SetVariable("token", newToken);
            LocalStorageService.SetVariable("refresh-token", resp.refreshToken);

            Debug.Log("[BoardGen] [TokenCheck] Token refresh successful.");
        }
        else
        {
            Debug.LogError("[BoardGen] [TokenCheck] Token refresh failed: " + req.error);
        }
    }

    private IEnumerator GetSessionIdFromCode()
    {
        string sessionCode = LocalStorageService.GetString("session-code");
        if (string.IsNullOrEmpty(sessionCode))
        {
            Debug.LogError("[BoardGen] ❌ No session code found");
            yield break;
        }

        yield return StartCoroutine(EnsureValidToken());

        string jwt = LocalStorageService.GetString("token");
        if (!SecurityUtils.IsTokenValid(jwt))
        {
            Debug.LogError("[BoardGen] ❌ User not authenticated (token invalid)");
            yield break;
        }

        string url = $"http://localhost:8080/api/session/code/{sessionCode}";
        using UnityWebRequest req = UnityWebRequest.Get(url);
        req.SetRequestHeader("Authorization", jwt);

        Debug.Log($"[BoardGen] [GetSessionId] GET {url}");
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[BoardGen] [GetSessionId] {req.error} ({req.responseCode})");
            yield break;
        }

        Debug.Log("[BoardGen] [GetSessionId] Response: " + req.downloadHandler.text);

        try
        {
            var sessionData = JsonUtility.FromJson<SessionDto>(req.downloadHandler.text);
            if (sessionData != null && sessionData.id > 0)
            {
                cachedSessionId = sessionData.id;
                Debug.Log($"[BoardGen] ✅ Got session ID: {cachedSessionId}");
            }
            else
            {
                Debug.LogError("[BoardGen] ❌ Invalid session data received");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("[BoardGen] [GetSessionId] JSON parse error: " + ex.Message);
        }
    }

    #endregion

    private void TriggerNonHostBehavior()
    {
        Debug.Log("[BoardGen] 🏘️ Non-host: Will request map data from backend via WebSocket...");

        // Check if WebSocket is already connected, if not wait for it
        if (WebSocketService.Connected)
        {
            Debug.Log("[BoardGen] WebSocket already connected, requesting map immediately");
            RequestMapData();
        }
        else
        {
            Debug.Log("[BoardGen] WebSocket not connected yet, starting coroutine to wait and request");
            StartCoroutine(RequestMapFromBackend());
        }
    }

    private void TriggerHostBehavior()
    {
        Debug.Log("[BoardGen] 🏠 Host: Generating board locally...");
        if (isGenerated == false)
        {
            GenerateAll();
        }

        if (BoardGenBackendClient.Instance != null)
        {

            Debug.Log("[BoardGen] 📤 Host: Sending initial board data to backend...");
            BoardGenBackendClient.Instance.SendBoardData(tileList);
        }
        else
        {
            Debug.LogWarning("[BoardGen] ⚠️ BoardGenBackendClient not found, cannot send initial board data.");
        }
    }
    void GenerateBoard()
    {
        List<string> shuffledResources = new(resources);
        List<int> shuffledNumbers = new(numberTokens);

        Shuffle(shuffledResources);
        Shuffle(shuffledNumbers);

        int index = 0;
        int numberIndex = 0;

        for (int q = -radius; q <= radius; q++)
        {
            int r1 = Mathf.Max(-radius, -q - radius);
            int r2 = Mathf.Min(radius, -q + radius);

            for (int r = r1; r <= r2; r++)
            {
                Vector3 pos = HexToWorld(q, r);

                for (int i = 0; i < 6; i++)
                {
                    Vector3 vertexPos = GetVertexPosition(pos, i, hexSize);
                    SpawnVertexIfNotExists(vertexPos);
                }

                GameObject tileObj = Instantiate(hexTilePrefab, pos, Quaternion.identity);

                tileList.Add(tileObj.GetComponent<HexTile>());

                if (tileObj.GetComponent<MeshCollider>() == null)
                {
                    MeshCollider meshCollider = tileObj.AddComponent<MeshCollider>();
                    MeshFilter mf = tileObj.GetComponent<MeshFilter>();
                    if (mf != null)
                        meshCollider.sharedMesh = mf.sharedMesh;
                }

                string resource = shuffledResources[index++];
                int number = resource == "desert" ? 0 : shuffledNumbers[numberIndex++];

                HexTile tile = tileObj.GetComponent<HexTile>();
                tile.Initialize(resource, number, q, r);

                if (resource == "desert")
                {
                    thiefInstance = Instantiate(thiefPrefab, tileObj.transform);
                    thiefInstance.transform.localPosition = Vector3.up * 0.1f;
                    currentThiefTile = tile;
                }
            }
        }

        foreach (var vertex in vertexMap.Values)
        {
            foreach (var tile in FindObjectsOfType<HexTile>())
            {
                if (Vector3.Distance(tile.transform.position, vertex.Position) < hexSize + .1f)
                {
                    vertex.nearbyTiles.Add(tile);
                }
            }
        }

        Debug.Log("Board generated!");
    }

    public void GenerateEdgePoints(List<VertexPoint> allVertexPoints)
    {
        HashSet<(VertexPoint, VertexPoint)> createdEdges = new();

        for (int i = 0; i < allVertexPoints.Count; i++)
        {
            VertexPoint a = allVertexPoints[i];

            for (int j = i + 1; j < allVertexPoints.Count; j++)
            {
                VertexPoint b = allVertexPoints[j];

                float dist = Vector3.Distance(a.Position, b.Position);

                float minDist = hexSize * 0.8f;
                float maxDist = hexSize * 1.3f;

                if (dist >= minDist && dist <= maxDist)
                {
                    if (!createdEdges.Contains((a, b)) && !createdEdges.Contains((b, a)))
                    {
                        GameObject edgeGO = Instantiate(edgePointPrefab, (a.Position + b.Position) / 2, Quaternion.identity, transform);
                        EdgePoint ep = edgeGO.GetComponent<EdgePoint>();

                        ep.pointA = a;
                        ep.pointB = b;

                        a.edgePoints.Add(ep);
                        b.edgePoints.Add(ep);

                        List<HexTile> sharedTiles = new();

                        foreach (HexTile tile in a.nearbyTiles)
                        {
                            if (tile != null && b.nearbyTiles.Contains(tile))
                            {
                                sharedTiles.Add(tile);
                                if (sharedTiles.Count == 2) break;
                            }
                        }

                        for (int k = 0; k < sharedTiles.Count; k++)
                        {
                            ep.adjacentTiles[k] = sharedTiles[k];
                        }

                        createdEdges.Add((a, b));
                    }

                }
            }
        }
    }

    void PlacePorts()
    {
        Shuffle(portTypes);
        var portPairs = FindValidPortPairs();

        // Order port edges based on angle around board center
        Vector3 center = CalculateBoardCenter();
        var sorted = portPairs.OrderBy(pair =>
        {
            Vector3 mid = (pair.Item1.Position + pair.Item2.Position) / 2f;
            Vector3 dir = (mid - center).normalized;
            return Mathf.Atan2(dir.z, dir.x);
        }).ToList();

        // Evenly spaced selection from the sorted list
        int count = Mathf.Min(portTypes.Count, sorted.Count);
        int spacing = sorted.Count / count;

        for (int i = 0; i < count; i++)
        {
            int index = i * spacing;
            var (a, b) = sorted[index];
            PortType type = portTypes[i];

            // Step 1: Find the hex shared between vertex A and B
            List<HexTile> sharedHexes = a.nearbyTiles.Intersect(b.nearbyTiles).ToList();
            if (sharedHexes.Count == 0)
            {
                Debug.LogWarning("No shared hex found for port placement between vertices.");
                continue;
            }

            Vector3 hexCenter = sharedHexes[0].transform.position;

            // Step 2: Direction from hex to edge midpoint
            Vector3 midpoint = (a.Position + b.Position) / 2f;
            Vector3 outwardDirection = (midpoint - hexCenter).normalized;

            // Step 3: Calculate port position off the coast
            Vector3 portPosition = midpoint + outwardDirection * hexSize;

            // Step 4: Spawn and initialize the port
            GameObject portObj = Instantiate(portPrefab, portPosition, Quaternion.identity, transform);
            Port port = portObj.GetComponent<Port>();
            port.Initialize(type, a, b);
        }
    }

    List<(VertexPoint, VertexPoint)> FindValidPortPairs()
    {
        var portPairs = new List<(VertexPoint, VertexPoint)>();

        foreach (var vertex in vertexMap.Values)
        {
            if (vertex.nearbyTiles.Count > 2) continue; // not a border vertex

            foreach (var edge in vertex.edgePoints)
            {
                VertexPoint other = edge.pointA == vertex ? edge.pointB : edge.pointA;

                // Must also be border
                if (other == null || other == vertex || other.nearbyTiles.Count > 2)
                    continue;

                var pair = (vertex, other);

                // Avoid duplicates (A-B == B-A)
                if (!portPairs.Any(p => (p.Item1 == pair.Item2 && p.Item2 == pair.Item1)))
                    portPairs.Add(pair);
            }
        }

        return portPairs;
    }

    Vector3 CalculateBoardCenter()
    {
        Vector3 sum = Vector3.zero;
        int count = 0;

        foreach (var tile in FindObjectsByType<HexTile>(FindObjectsSortMode.None))
        {
            sum += tile.transform.position;
            count++;
        }

        return count > 0 ? sum / count : Vector3.zero;
    }

    Vector3 HexToWorld(int q, int r)
    {
        float x = Mathf.Sqrt(3f) * (q + r / 2f);
        float z = 1.5f * r;
        return new Vector3(x * hexSize, 0, z * hexSize);
    }

    void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = UnityEngine.Random.Range(i, list.Count);

            (list[i], list[rand]) = (list[rand], list[i]);
        }
    }

    public void MoveThiefTo(HexTile newTile)
    {
        if (thiefInstance == null || newTile == currentThiefTile)
            return;

        thiefInstance.transform.SetParent(newTile.transform);
        thiefInstance.transform.localPosition = Vector3.up * 0.1f;
        currentThiefTile = newTile;
        ClearHighlights();
    }

    public HexTile GetCurrentThiefTile()
    {
        return currentThiefTile;
    }

    public void ConstructBoardFromTiles(List<TileDto> tiles)
    {
        if (isGenerated)
            return;

        vertexMap.Clear();

        if (thiefInstance != null)
        {
            Destroy(thiefInstance);
            currentThiefTile = null;
        }

        foreach (var tileDto in tiles)
        {
            Vector3 pos = HexToWorld(tileDto.x, tileDto.y);

            for (int i = 0; i < 6; i++)
            {
                Vector3 vertexPos = GetVertexPosition(pos, i, hexSize);
                SpawnVertexIfNotExists(vertexPos);
            }

            GameObject tileObj = Instantiate(hexTilePrefab, pos, Quaternion.identity);

            if (tileObj.GetComponent<MeshCollider>() == null)
            {
                MeshCollider meshCollider = tileObj.AddComponent<MeshCollider>();
                MeshFilter mf = tileObj.GetComponent<MeshFilter>();
                if (mf != null) meshCollider.sharedMesh = mf.sharedMesh;
            }

            HexTile tile = tileObj.GetComponent<HexTile>();
            tile.Initialize(tileDto.tileType.ToLower(), tileDto.number, tileDto.x, tileDto.y);

            if (tileDto.tileType.ToLower() == "desert")
            {
                thiefInstance = Instantiate(thiefPrefab, tileObj.transform);
                thiefInstance.transform.localPosition = Vector3.up * 0.1f;
                currentThiefTile = tile;
            }
        }

        foreach (var vertex in vertexMap.Values)
        {
            foreach (var tile in FindObjectsByType<HexTile>(FindObjectsSortMode.None))
            {
                if (Vector3.Distance(tile.transform.position, vertex.Position) < hexSize + 0.1f)
                {
                    vertex.nearbyTiles.Add(tile);
                }
            }
        }

        GenerateEdgePoints(vertexMap.Values.ToList());

        Debug.Log("Board constructed from TileDto list.");
    }
    private void SubscribeToMapEvents()
    {
        Debug.Log("[BoardGen] 🔌 Subscribing to map generation events...");
        try
        {
            WebSocketService.OnMapGenerated += HandleMapReceived;
            Debug.Log("[BoardGen] ✅ Subscribed to OnMapGenerated event");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[BoardGen] ❌ Failed to subscribe to map events: {ex.Message}");
        }
    }



    private void HandleMapReceived(GenerateMapDto mapData)
    {
        Debug.Log("[BoardGen] === MAP DATA RECEIVED FROM BACKEND ===");
        Debug.Log($"[BoardGen] 📥 Received map with {mapData.tileDtos.Count} tiles");
        try
        {
            Debug.Log("[BoardGen] Clearing existing board elements...");
            ClearExistingBoard();
            Debug.Log("[BoardGen] Constructing board from received tile data...");
            ConstructBoardFromTiles(mapData.tileDtos);
            Debug.Log("[BoardGen] ✅ Board successfully constructed from backend map data");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[BoardGen] ❌ Failed to construct board from map data: {ex.Message}");
        }
    }


    private void ClearExistingBoard()
    {
        if (isGenerated)
            return;
        Debug.Log("🧹 Clearing any existing board elements...");

        // Clear existing tiles
        var existingTiles = FindObjectsOfType<HexTile>();
        foreach (var tile in existingTiles)
        {
            if (tile != null)
                Destroy(tile.gameObject);
        }

        // Clear existing vertices
        var existingVertices = FindObjectsOfType<VertexPoint>();
        foreach (var vertex in existingVertices)
        {
            if (vertex != null)
                Destroy(vertex.gameObject);
        }

        // Clear existing edges
        var existingEdges = FindObjectsOfType<EdgePoint>();
        foreach (var edge in existingEdges)
        {
            if (edge != null)
                Destroy(edge.gameObject);
        }

        // Clear vertex map
        vertexMap.Clear();

        Debug.Log("✅ Existing board elements cleared");
    }

    private IEnumerator RequestMapFromBackend()
    {
        Debug.Log("[BoardGen] ⏳ RequestMapFromBackend coroutine started");
        Debug.Log("[BoardGen] Waiting for WebSocket connection to be established...");
        Debug.Log($"[BoardGen] Current WebSocket connected state: {WebSocketService.Connected}");

        float timeout = 15f;
        float elapsed = 0f;

        while (!WebSocketService.Connected && elapsed < timeout)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
            if (elapsed % 1f < 0.1f) // Log every second
            {
                Debug.Log($"[BoardGen] Waiting for WebSocket... Elapsed: {elapsed}s, Connected: {WebSocketService.Connected}");
            }
        }

        if (WebSocketService.Connected)
        {
            Debug.Log("[BoardGen] ✅ WebSocket connected! Requesting map data...");
            yield return new WaitForSeconds(0.5f);
            RequestMapData();
        }
        else
        {
            Debug.LogError("[BoardGen] ❌ WebSocket connection timeout - cannot request map data. Retrying...");
            yield return new WaitForSeconds(2f);
            Debug.Log("[BoardGen] Restarting RequestMapFromBackend coroutine...");
            //StartCoroutine(RequestMapFromBackend());
        }
        Debug.Log("[BoardGen] RequestMapFromBackend coroutine completed");
    }


    private void OnDestroy()
    {
        Debug.Log("[BoardGen] BoardGen destroyed - unsubscribing from events");
        WebSocketService.OnMapGenerated -= HandleMapReceived;
        WebSocketService.OnDiceResponse -= GetDiceData;
        WebSocketService.OnEndTurn -= GetEndTurn;
        PuchaseUIManager.OnStructureBuilt -= HandleStructurePlaced;
        WebSocketService.OnPlaceStructure -= GetStructurePlacedConfirmation;
        PuchaseUIManager.OnRoadBuilt -= HandleRoadPlaced;
        WebSocketService.OnPlaceRoad -= GetRoadPlacedConfirmation;

        Assets.Scripts.GameMode.Trading.TradingManager.OnPlayersLoaded -= HandlePlayersLoaded;
    }

    private void RequestMapData()
    {
        Debug.Log("[BoardGen] RequestMapData called");

        if (!playersLoaded)
        {
            Debug.LogWarning("[BoardGen] ⚠️ Players not loaded yet, deferring map request");
            return;
        }

        if (!WebSocketService.Connected)
        {
            Debug.LogError("[BoardGen] ❌ WebSocket not connected - cannot request map data");
            return;
        }

        if (cachedSessionPlayerId <= 0)
        {
            Debug.LogError("[BoardGen] ❌ Invalid SessionPlayer ID - cannot request map data");
            return;
        }

        try
        {
            Debug.Log("[BoardGen] Creating REQUEST_MAP GameMoveDto...");
            var gameMove = new GameMoveDto(GameMoveType.REQUEST_MAP);
            Debug.Log($"[BoardGen] 🎯 GameMove type: {gameMove.gameMoveType}");
            Debug.Log("[BoardGen] 📤 Sending REQUEST_MAP via WebSocket...");
            WebSocketService.SendGameMove(gameMove);
            Debug.Log("[BoardGen] ✅ Map request sent successfully");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[BoardGen] ❌ Failed to request map data: {ex.Message}");
            //StartCoroutine(RetryMapRequest());
        }
    }


    private IEnumerator RetryMapRequest()
    {
        Debug.Log("[BoardGen] 🔄 Retrying map request in 3 seconds...");
        yield return new WaitForSeconds(3f);
        RequestMapData();
    }
    #region Supporting DTOs
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
    #endregion

    public async void RoleDice()
    {
        if (isRobberMoveInProgress)
        {
            Debug.LogWarning("[BoardGen] Cannot roll dice during robber move");
            return;
        }

        Debug.Log("[BoardGen] 🎲 RoleDice called - sending dice roll request to WebSocket");
        await WebSocketService.SendDiceRoll();
    }

    public void GetDiceData(DiceResultDto diceResult)
    {

        Debug.Log($"[Dice Result] 🎲 {diceResult.username} rolled a {diceResult.rollResult}");

        foreach (var entry in diceResult.userResourcesGained)
        {
            string user = entry.Key;
            ResourceGroup resources = entry.Value;
            var resourceDict = resources.GetResourceDictionary();

            Debug.Log($"Resources gained by {user}:");

            foreach (var resource in resourceDict)
            {
                if (resource.Value > 0)
                {
                    Debug.Log($"  - {resource.Key}: {resource.Value}");
                }
            }

            Debug.Log($"[BoardGen] 🎲 Dice rolled: {diceResult.rollResult}");
            _lastDiceTotal = diceResult.rollResult;



            // Trigger robber move on 7
            if (_lastDiceTotal == 7)
            {
                
                Debug.Log("[BoardGen] ⚠️ 7 rolled - initiating robber move");
                InitiateRobberMove(true);

            }
        }
    }

    public async void EndTurn()
    {
        Debug.Log("[BoardGen] 🏁 EndTurn called - sending end turn request to WebSocket");
        await WebSocketService.SendEndTurn();
    }

    public void GetEndTurn(EndTurnResponse t)
    {
        Debug.Log("[BoardGen] EndTurn received from WebSocket - handling end turn logic");

        DevCardManager.Instance.LoadPlayerCards();
        DevCardManager.Instance.SetCardPlayable();
    }

        private async void HandleStructurePlaced(PurchaseType type, VertexPoint vo)
        {
            var tile = vo.nearbyTiles[0];

            var dto = new PlaceStructureDto(
                tile.Q,
                tile.R,
                vo.GetNeighborVertexIndex(tile),
                vo.type
            );

            await WebSocketService.SendPlaceStructure(dto);
        }

    public void GetStructurePlacedConfirmation(PlaceStructureResponse dto)
    {
        Debug.Log($"[StructurePlacementSender] ✅ Server confirmed structure placed at ({dto.tileX}, {dto.tileY}) corner {dto.cornerIndex}");
    }

    private async void HandleRoadPlaced(PurchaseType type, EdgePoint ep)
    {
        var dto = new PlaceRoadDto(-99, -99, 1);

        await WebSocketService.SendPlaceRoad(dto);
    }

    public void GetRoadPlacedConfirmation(PlaceRoadResponse dto)
    {
        Debug.LogError("road built");
    }
}
