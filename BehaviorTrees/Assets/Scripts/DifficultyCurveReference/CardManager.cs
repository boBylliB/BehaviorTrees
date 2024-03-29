using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    public float xpHealthScale;
    public float xpPowerScale;
    public float xpSkillScale;
    public float xpDownScale;

    public Color defaultColor;
    public Color favoriteColor;
    public Sprite rangedIcon;
    public Sprite meleeIcon;
    public List<Card> cards = new List<Card>();
    public int maxCards = 40;

    public void sortCards()
    {
        recalculateXPs();
        List<Card> favoriteCards = new List<Card>();
        List<Card> nonFavoriteCards = new List<Card>();
        for (int idx = 0; idx < cards.Count; idx++)
        {
            if (cards[idx].favorite)
            {
                favoriteCards.Add(cards[idx]);
            }
            else
            {
                nonFavoriteCards.Add(cards[idx]);
            }
        }
        quickSort(favoriteCards, 0, favoriteCards.Count - 1);
        quickSort(nonFavoriteCards, 0, nonFavoriteCards.Count - 1);
        cards.Clear();
        for (int idx = 0; idx < nonFavoriteCards.Count; idx++)
        {
            cards.Add(nonFavoriteCards[idx]);
        }
        for (int idx = 0; idx < favoriteCards.Count; idx++)
        {
            cards.Add(favoriteCards[idx]);
        }
    }

    public int calculateXP(Card card)
    {
        int xp = (int)((xpHealthScale * card.health + xpPowerScale * card.powerLevel + xpSkillScale * card.skillLevel) / xpDownScale);
        if (card.attackType.Equals(AttackType.Ranged)) xp *= 2;
        return xp;
    }

    public void recalculateXPs()
    {
        for (int idx = 0; idx < cards.Count; idx++)
        {
            cards[idx].xpDropped = calculateXP(cards[idx]);
        }
    }

    public void toggleFavorite(string name)
    {
        for (int idx = 0; idx < cards.Count; idx++)
        {
            if (cards[idx].cardName == name) cards[idx].favorite = !cards[idx].favorite;
        }
    }

    public Card getCard(string name)
    {
        for (int idx = 0; idx < cards.Count; idx++)
        {
            if (cards[idx].cardName == name) return cards[idx];
        }
        return null;
    }

    private void swap(List<Card> cardList, int idxA, int idxB)
    {
        Card temp = cardList[idxA];
        cardList[idxA] = cardList[idxB];
        cardList[idxB] = temp;
    }
    private int partition(List<Card> cardList, int lowIdx, int highIdx)
    {
        Card pivot = cardList[highIdx];
        int idx = lowIdx - 1;
        for (int jdx = lowIdx; jdx <= highIdx-1; jdx++)
        {
            if (cardList[jdx].xpDropped < pivot.xpDropped)
            {
                idx++;
                swap(cardList, idx, jdx);
            }
        }
        swap(cardList, idx + 1, highIdx);
        return idx + 1;
    }
    private void quickSort(List<Card> cardList, int startIdx, int endIdx)
    {
        if (startIdx < endIdx)
        {
            int partIdx = partition(cardList, startIdx, endIdx);
            quickSort(cardList, startIdx, partIdx - 1);
            quickSort(cardList, partIdx + 1, endIdx);
        }
    }
}
