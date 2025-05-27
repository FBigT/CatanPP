using UnityEngine;
using Assets.Scripts.Models;

namespace Assets.Scripts.DevCards.Effects
{
    public class VictoryPointEffect
    {
        public void Execute(DevCardDto cardData)
        {
            Debug.Log("Victory Point card played - adding 1 victory point");

            // TODO: Integrate with your scoring system
            // Example: ScoreManager.Instance.AddVictoryPoint();
        }
    }
}
