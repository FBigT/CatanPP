using UnityEngine;
using Assets.Scripts.Models;

namespace Assets.Scripts.DevCards.Effects
{
    public class YearOfPlentyEffect
    {
        private DevCardDto currentCard;
        private object yearOfPlentyPanel; // You can type this properly based on your panel class

        public YearOfPlentyEffect(object panel)
        {
            yearOfPlentyPanel = panel;
        }

        public void Execute(DevCardDto cardData)
        {
            currentCard = cardData;
            Debug.Log("Year of Plenty effect activated - choose 2 resources");

            // TODO: Show resource selection panel
            // Example: yearOfPlentyPanel.Show();
        }

        public DevCardDto GetCurrentCard()
        {
            return currentCard;
        }
    }
}
