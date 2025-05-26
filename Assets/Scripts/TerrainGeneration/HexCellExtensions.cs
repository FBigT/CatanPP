using System;
using System.Collections.Generic;
using System.Reflection;     // <— add this
using UnityEngine;
using Catan.Placement;      // for Connector
using Catan.GameMode;       // for ResourceType

namespace Catan.TerrainGeneration
{
    public static class HexCellExtensions
    {
        static readonly FieldInfo _tokenField = typeof(HexCell)
            .GetField("numberToken", BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly FieldInfo _resourceField = typeof(HexCell)
            .GetField("resourceType", BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly FieldInfo _robberField = typeof(HexCell)
            .GetField("hasRobber", BindingFlags.NonPublic | BindingFlags.Instance);

        public static bool NumberTokenIs(this HexCell cell, int roll)
        {
            return (int)_tokenField.GetValue(cell) == roll;
        }

        public static int GetNumberToken(this HexCell cell) { 
            return (int)_tokenField.GetValue(cell);
        }

        public static bool HasRobber(this HexCell cell) { 
            return (bool)_robberField.GetValue(cell);
        }

        public static void SetRobber(this HexCell cell, bool robberPresent) {
            _robberField.SetValue(cell, robberPresent);
        }

        public static ResourceType GetResource(this HexCell cell)
        {
            return (ResourceType)_resourceField.GetValue(cell);
        }

        public static IEnumerable<Connector> GetCornerConnectors(this HexCell cell)
        {
            var metrics = cell.CellHexMetrics;
            var center = cell.transform.position;

            foreach (var cornerOffset in metrics.Corners)
            {
                var worldPos = center + cornerOffset;
                var hits = Physics.OverlapSphere(worldPos, 0.1f);
                foreach (var hit in hits)
                {
                    if (hit.TryGetComponent<Connector>(out var conn) &&
                        conn.Connection == Connector.ConnectionType.Corner)
                    {
                        yield return conn;
                        break;
                    }
                }
            }
        }
    }
}
