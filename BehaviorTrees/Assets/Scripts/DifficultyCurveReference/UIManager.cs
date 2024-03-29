using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GameManager gameManager;
    public CardManager cardManager;
    public GameObject cardPrefab;
    public GameObject cardParent;
    public GameObject cardLayer;
    public List<GameObject> cardSlots = new List<GameObject>();

    public List<GameObject> selectedCards = new List<GameObject>();

    public BoxCollider2D favoritesBox;

    private int selectedCardID = -1;

    void Start()
    {
        DisplayCards();
    }

    void Update()
    {
        selectCard();
        updateCardLifting();
        if (Input.GetMouseButtonDown(0))
        {
            if (selectedCardID > -1)
            {
                copyCard();
            }
        }
    }

    public void addToSelectedCards(GameObject card)
    {
        selectedCards.Add(card);
    }
    public void removeFromSelectedCards(GameObject card)
    {
        selectedCards.Remove(card);
    }

    public void updateDisplayOrder()
    {
        DisplayCards();
    }

    private void copyCard()
    {
        GameObject selectedCard = null;
        for (int idx = 0; idx < cardParent.transform.childCount && selectedCard == null; idx++)
        {
            GameObject currentCard = cardParent.transform.GetChild(idx).gameObject;
            if (currentCard.GetComponent<CardController>().cardID == selectedCardID)
            {
                selectedCard = currentCard;
            }
        }
        if (selectedCard == null) return;
        GameObject newCard = Instantiate(selectedCard, cardLayer.transform);
        newCard.GetComponent<CardController>().cardID = -2;
        newCard.GetComponent<CardController>().uiManager = this;
        newCard.GetComponent<CardController>().cardManager = cardManager;
        newCard.GetComponent<CardController>().favoritesBox = favoritesBox;
        newCard.GetComponent<CardController>().initializeCopy(selectedCard.transform.position, Input.mousePosition);
    }

    private void selectCard()
    {
        selectedCardID = -1;
        if (gameManager.paused) return; //Disable card selection if the game is paused
        for (int idx = 0; idx < selectedCards.Count; idx++)
        {
            if (selectedCards[idx].GetComponent<CardController>().cardID > selectedCardID)
            {
                selectedCardID = selectedCards[idx].GetComponent<CardController>().cardID;
            }
        }
    }
    private void updateCardLifting()
    {
        for (int idx = 0; idx < cardParent.transform.childCount; idx++)
        {
            GameObject currentCard = cardParent.transform.GetChild(idx).gameObject;
            if (currentCard.GetComponent<CardController>().cardID == selectedCardID)
            {
                currentCard.GetComponent<CardController>().selected = true;
            }
            else
            {
                currentCard.GetComponent<CardController>().selected = false;
            }
        }
    }

    private void DisplayCards()
    {
        //Update our list to represent the current card slots
        syncCardSlots();
        //Check if any card slots need to be added or removed
        while (cardManager.cards.Count > cardSlots.Count)
        {
            //We don't have enough slots, and need to add more
            addCardSlot();
        }
        while (cardManager.cards.Count < cardSlots.Count)
        {
            //We have too many slots, and need to remove some
            removeCardSlot();
        }
        cardManager.sortCards();

        for (int idx = 0; idx < cardManager.cards.Count; idx++)
        {
            //Assign the card preset information to the UI references
            if (cardManager.cards[idx].favorite)
            {
                cardSlots[idx].transform.GetChild(0).GetComponent<Image>().color = cardManager.favoriteColor;
            }
            else
            {
                cardSlots[idx].transform.GetChild(0).GetComponent<Image>().color = cardManager.defaultColor;
            }
            cardSlots[idx].transform.GetChild(1).GetComponent<Image>().color = cardManager.cards[idx].outerColor;
            cardSlots[idx].transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = cardManager.cards[idx].cardName;
            cardSlots[idx].transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = cardManager.cards[idx].health + "";
            cardSlots[idx].transform.GetChild(4).GetComponent<TextMeshProUGUI>().text = cardManager.cards[idx].powerLevel + "";
            cardSlots[idx].transform.GetChild(5).GetComponent<TextMeshProUGUI>().text = cardManager.cards[idx].skillLevel + "";
            switch (cardManager.cards[idx].attackType)
            {
                case AttackType.Ranged:
                    cardSlots[idx].transform.GetChild(6).GetComponent<Image>().sprite = cardManager.rangedIcon;
                    cardSlots[idx].transform.GetChild(7).GetComponent<Image>().sprite = cardManager.rangedIcon;
                    break;
                default:
                    cardSlots[idx].transform.GetChild(6).GetComponent<Image>().sprite = cardManager.meleeIcon;
                    cardSlots[idx].transform.GetChild(7).GetComponent<Image>().sprite = cardManager.meleeIcon;
                    break;
            }
            cardSlots[idx].transform.GetChild(7).GetComponent<Image>().color = cardManager.cards[idx].innerColor;
            cardSlots[idx].transform.GetChild(8).GetComponent<TextMeshProUGUI>().text = cardManager.cards[idx].xpDropped + "";
        }
    }

    private void syncCardSlots()
    {
        cardSlots.Clear();
        for (int idx = 0; idx < cardParent.transform.childCount; idx++)
        {
            cardSlots.Add(cardParent.transform.GetChild(idx).gameObject);
        }
    }

    private void addCardSlot()
    {
        GameObject newCard = Instantiate(cardPrefab, cardParent.transform);
        newCard.GetComponent<CardController>().cardID = cardSlots.Count;
        newCard.GetComponent<CardController>().uiManager = this;
        newCard.GetComponent<CardController>().cardManager = cardManager;
        newCard.GetComponent<CardController>().gameManager = gameManager;
        newCard.GetComponent<CardController>().favoritesBox = favoritesBox;
        cardSlots.Add(newCard);
    }

    private void removeCardSlot()
    {
        GameObject target = cardSlots[cardSlots.Count - 1];
        cardSlots.Remove(target);
        Destroy(target);
    }
}
