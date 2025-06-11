using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Assets.Scripts.Dtos;
using Assets.Scripts.Utils;

public class TestBoardGen : BoardGen
{
    // Prevent any of the real Awake/Start logic from running
    new void Awake() { /* no-op */ }
    new void Start() { /* no-op */ }
}

[TestFixture]
public class BoardGenIntegrationTests
{
    private GameObject go;
    private TestBoardGen boardGen;
    private GameObject hexPrefab, vertexPrefab, edgePrefab, portPrefab, thiefPrefab;
    private GameObject panel;

    [SetUp]
    public void SetUp()
    {
        // Create a GameObject and attach our test subclass
        go = new GameObject("BoardGenTest");
        boardGen = go.AddComponent<TestBoardGen>();

        // Stub out the blockPanel
        panel = new GameObject("BlockPanel", typeof(RectTransform));
        panel.SetActive(true);
        boardGen.blockPanel = panel.GetComponent<RectTransform>();

        // Create and assign an active hexTilePrefab
        hexPrefab = new GameObject("HexPrefab");
        hexPrefab.AddComponent<HexTile>();
        hexPrefab.AddComponent<MeshFilter>();
        hexPrefab.AddComponent<MeshRenderer>();
        hexPrefab.AddComponent<MeshCollider>();
        hexPrefab.SetActive(true);
        boardGen.hexTilePrefab = hexPrefab;

        // Vertex prefab
        vertexPrefab = new GameObject("VertexPrefab");
        vertexPrefab.AddComponent<VertexPoint>();
        vertexPrefab.SetActive(true);
        boardGen.vertexPrefab = vertexPrefab;

        // Edge prefab
        edgePrefab = new GameObject("EdgePrefab");
        edgePrefab.AddComponent<EdgePoint>();
        edgePrefab.SetActive(true);
        boardGen.edgePointPrefab = edgePrefab;

        // Port prefab (not used by ConstructBoardFromTiles, but needs to exist)
        portPrefab = new GameObject("PortPrefab");
        portPrefab.AddComponent<Port>();
        portPrefab.SetActive(true);
        boardGen.portPrefab = portPrefab;

        // Thief prefab for desert tile
        thiefPrefab = new GameObject("ThiefPrefab");
        thiefPrefab.SetActive(true);
        boardGen.thiefPrefab = thiefPrefab;
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(go);
        Object.DestroyImmediate(hexPrefab);
        Object.DestroyImmediate(vertexPrefab);
        Object.DestroyImmediate(edgePrefab);
        Object.DestroyImmediate(portPrefab);
        Object.DestroyImmediate(thiefPrefab);
        Object.DestroyImmediate(panel);
    }

    [UnityTest]
    public IEnumerator HandleMapReceived_ConstructsBoardAccordingToDto()
    {
        // Arrange: create two TileDto entries
        var dtos = new List<TileDto>
        {
            // x, y, type, numberToken, unused
            new TileDto(0, 0, "wood",   5, 0),
            new TileDto(1, 1, "desert", 0, 0)
        };
        var mapDto = new GenerateMapDto(dtos);

        // Act: invoke the non-public Handler
        var method = typeof(BoardGen)
            .GetMethod("HandleMapReceived", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNotNull(method, "HandleMapReceived method not found");

        method.Invoke(boardGen, new object[] { mapDto });
        // wait a frame so all Instantiates/StartCoroutine have run
        yield return null;

        // Assert: blockPanel should be hidden
        Assert.IsFalse(panel.activeSelf, "blockPanel should be deactivated after map arrives");

        // Assert: one HexTile per DTO
        Assert.AreEqual(dtos.Count, boardGen.TileList.Count,
            "TileList must contain exactly one tile per DTO");

        // Assert: both resource types present
        Assert.IsTrue(boardGen.TileList.Exists(t => t.resourceType == "wood"),
            "Missing wood tile");
        Assert.IsTrue(boardGen.TileList.Exists(t => t.resourceType == "desert"),
            "Missing desert tile");

        // Assert: desert tile got the thief
        var desertTile = boardGen.TileList.Find(t => t.resourceType == "desert");
        Assert.AreEqual(desertTile, boardGen.GetCurrentThiefTile(),
            "Thief should be placed on the desert tile");

        // Assert: 6 vertices per tile
        var allVertices = Object.FindObjectsOfType<VertexPoint>(includeInactive: true);
        Assert.AreEqual(dtos.Count * 6, allVertices.Length,
            "There should be exactly 6 vertices per tile");

        // Assert: 6 edges per tile
        var allEdges = Object.FindObjectsOfType<EdgePoint>(includeInactive: true);
        Assert.AreEqual(dtos.Count * 6, allEdges.Length,
            "There should be exactly 6 edges per tile");
    }
}
