using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Catan.Placement;  // your Connector class namespace

[TestFixture]
public class PlacementIntegrationTests
{
    private GameObject connectorObj;
    private Connector connector;

    private GameObject settlementPrefab;
    private GameObject roadPrefab;
    private GameObject cityPrefab;

    [SetUp]
    public void SetUp()
    {
        // Create a Connector
        connectorObj = new GameObject("Connector");
        connector = connectorObj.AddComponent<Connector>();

        // Prepare dummy prefabs
        settlementPrefab = new GameObject("SettlementPrefab");
        settlementPrefab.tag = "Corner";

        roadPrefab = new GameObject("RoadPrefab");
        roadPrefab.tag = "Edge";

        cityPrefab = new GameObject("CityPrefab");
        cityPrefab.tag = "Corner";
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(connectorObj);
        Object.DestroyImmediate(settlementPrefab);
        Object.DestroyImmediate(roadPrefab);
        Object.DestroyImmediate(cityPrefab);
    }

    [Test]
    public void CanPlaceStructure_CornerConnector_WithCornerPrefab_ReturnsTrue()
    {
        connector.Connection = Connector.ConnectionType.Corner;
        Assert.IsTrue(connector.CanPlaceStructure(settlementPrefab));
    }

    [Test]
    public void CanPlaceStructure_CornerConnector_WithEdgePrefab_ReturnsFalse()
    {
        connector.Connection = Connector.ConnectionType.Corner;
        Assert.IsFalse(connector.CanPlaceStructure(roadPrefab));
    }

    [Test]
    public void CanPlaceStructure_EdgeConnector_WithEdgePrefab_ReturnsTrue()
    {
        connector.Connection = Connector.ConnectionType.Edge;
        Assert.IsTrue(connector.CanPlaceStructure(roadPrefab));
    }

    [Test]
    public void CanPlaceStructure_EdgeConnector_WithCornerPrefab_ReturnsFalse()
    {
        connector.Connection = Connector.ConnectionType.Edge;
        Assert.IsFalse(connector.CanPlaceStructure(settlementPrefab));
    }

    [UnityTest]
    public IEnumerator PlaceStructure_OnEmptyConnector_InstantiatesChild()
    {
        connector.Connection = Connector.ConnectionType.Corner;
        connector.PlaceStructure(settlementPrefab);
        yield return null; // wait for Instantiate

        Assert.IsTrue(connector.IsOccupied, "Connector should now be occupied.");
        Assert.AreEqual(1, connectorObj.transform.childCount,
            "There should be exactly one child (the placed settlement).");
        Assert.IsTrue(connectorObj.transform.GetChild(0).name.Contains("SettlementPrefab"),
            "Child should be a clone of SettlementPrefab.");
    }

    [UnityTest]
    public IEnumerator RemoveStructure_ClearsChildAndFreesConnector()
    {
        connector.Connection = Connector.ConnectionType.Corner;
        connector.PlaceStructure(settlementPrefab);
        yield return null;

        // now remove
        connector.RemoveStructure();
        yield return null;

        Assert.IsFalse(connector.IsOccupied);
        Assert.AreEqual(0, connectorObj.transform.childCount);
    }

    [UnityTest]
    public IEnumerator UpgradeStructure_SettlementToCity()
    {
        connector.Connection = Connector.ConnectionType.Corner;

        // 1) Place settlement
        connector.PlaceStructure(settlementPrefab);
        yield return null;
        Assert.IsTrue(connector.IsOccupied && connectorObj.transform.childCount == 1);

        var firstChild = connectorObj.transform.GetChild(0).gameObject;
        Assert.IsTrue(firstChild.name.Contains("SettlementPrefab"));

        // 2) Remove settlement
        connector.RemoveStructure();
        yield return null;
        Assert.IsFalse(connector.IsOccupied);

        // 3) Place city
        connector.PlaceStructure(cityPrefab);
        yield return null;

        Assert.IsTrue(connector.IsOccupied, "After placing city, connector should be occupied.");
        Assert.AreEqual(1, connectorObj.transform.childCount);
        Assert.IsTrue(connectorObj.transform.GetChild(0).name.Contains("CityPrefab"),
            "Child should be a clone of CityPrefab.");
    }

    [UnityTest]
    public IEnumerator PlaceRoad_OnEdgeConnector_WorksSameAsCorner()
    {
        connector.Connection = Connector.ConnectionType.Edge;

        connector.PlaceStructure(roadPrefab);
        yield return null;

        Assert.IsTrue(connector.IsOccupied);
        Assert.AreEqual(1, connectorObj.transform.childCount);
        Assert.IsTrue(connectorObj.transform.GetChild(0).name.Contains("RoadPrefab"));
    }
}
