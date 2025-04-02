using UnityEngine;

public class TradingPort : MonoBehaviour
{
    public string portType; // Generic, Brick, Wood, etc.
    public int tradeRatio; // 3:1, 2:1, etc.

    public void Initialize(string type, int ratio)
    {
        portType = type;
        tradeRatio = ratio;
    }
}
