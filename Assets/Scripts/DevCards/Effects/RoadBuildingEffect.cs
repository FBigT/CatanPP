using UnityEngine;
using Assets.Scripts.Models;

namespace Assets.Scripts.DevCards.Effects
{
    public class RoadBuildingEffect
    {
        private DevCardDto currentCard;
        private object roadBuildingPanel; // You can type this properly based on your panel class

        public RoadBuildingEffect(object panel)
        {
            roadBuildingPanel = panel;
        }

        public void Execute(DevCardDto cardData)
        {
            currentCard = cardData;
            Debug.Log("Road Building effect activated - place 2 free roads");

            // TODO: Integrate with your road placement system
            // Example: StructurePlacer.Instance.ActivateRoadBuildingMode(2);
        }

        public DevCardDto GetCurrentCard()
        {
            return currentCard;
        }
    }
}
