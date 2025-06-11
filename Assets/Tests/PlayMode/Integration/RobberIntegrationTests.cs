using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Assets.Scripts.Utils;  // ThifeManager & BoardGen live here

[TestFixture]
public class RobberIntegrationTests
{
    private ThifeManager thiefManager;
    private GameObject thiefManagerObj;
    private GameObject boardGenObj;
    private BoardGen boardGen;

    [SetUp]
    public void SetUp()
    {
        // Create BoardGen singleton
        boardGenObj = new GameObject("BoardGen");
        boardGen = boardGenObj.AddComponent<BoardGen>();

        // Manually initialize two tiles
        // 1) Current tile at (0,0)
        var currentTileObj = new GameObject("CurrentTile");
        var currentTile = currentTileObj.AddComponent<HexTile>();
        currentTile.Initialize(resource: "wood", number: 1, q: 0, r: 0);

        // 2) Destination tile at (1,0)
        var targetTileObj = new GameObject("TargetTile");
        var targetTile = targetTileObj.AddComponent<HexTile>();
        targetTile.Initialize(resource: "brick", number: 2, q: 1, r: 0);

        // Populate the board's TileList
        boardGen.TileList.Clear();
        boardGen.TileList.Add(currentTile);
        boardGen.TileList.Add(targetTile);

        // Place the thief on the "current" tile
        boardGen.MoveThiefTo(currentTile);

        // Create the ThifeManager singleton
        thiefManagerObj = new GameObject("ThiefManager");
        thiefManager = thiefManagerObj.AddComponent<ThifeManager>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(thiefManagerObj);
        Object.DestroyImmediate(boardGenObj);
    }

    /// <summary>
    /// Moving the thief to a different valid tile should update the BoardGen's current tile.
    /// </summary>
    [UnityTest]
    public IEnumerator MoveThief_ToValidTile_ShouldRelocateThief()
    {
        // Arrange
        var tiles = boardGen.TileList;
        var initial = boardGen.GetCurrentThiefTile();
        var destination = tiles[1];
        Assert.AreNotEqual(initial, destination, "Sanity check: initial and destination must differ.");

        // Act
        thiefManager.MoveThief(destination);
        yield return null;

        // Assert
        Assert.AreEqual(destination, boardGen.GetCurrentThiefTile(),
            "ThiefManager.MoveThief should cause BoardGen to move the thief.");
    }

    /// <summary>
    /// Moving the thief to the same tile should be a no-op.
    /// </summary>
    [UnityTest]
    public IEnumerator MoveThief_ToSameTile_ShouldNotRelocate()
    {
        // Arrange
        var current = boardGen.GetCurrentThiefTile();

        // Act
        thiefManager.MoveThief(current);
        yield return null;

        // Assert
        Assert.AreEqual(current, boardGen.GetCurrentThiefTile(),
            "Moving to the same tile must not change the current thief position.");
    }
}
