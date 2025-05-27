using UnityEngine;
using Assets.Scripts.Models;

namespace Assets.Scripts.DevCards.Effects
{
    public class KnightEffect
    {
        private DevCardDto currentCard;

        public void Execute(DevCardDto cardData)
        {
            currentCard = cardData;
            Debug.Log("Knight effect activated - Move robber and steal from player");

            // TODO: Integrate with your existing robber system
            // Example: RobberManager.Instance.ActivateRobberMovement();
        }

        public DevCardDto GetCurrentCard()
        {
            return currentCard;
        }
    }
}
