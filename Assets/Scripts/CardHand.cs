using System.Collections.Generic;
using UnityEngine;

public class CardHand : MonoBehaviour
{
    public List<GameObject> cards = new List<GameObject>();
    public GameObject card;
    public bool isDealer = false;
    public int points;
    private int coordY;

    private void Awake()
    {
        points = 0;

        if (!isDealer)
            coordY = -2;
        else
            coordY = 2;
    }

    public void Clear()
    {
        points = 0;

        if (!isDealer)
            coordY = -2;
        else
            coordY = 2;

        foreach (GameObject g in cards)
        {
            Destroy(g);
        }

        cards.Clear();
    }

    public void InitialToggle()
    {
        if (cards.Count > 0)
            cards[0].GetComponent<CardModel>().ToggleFace(true);
    }

    public void Push(Sprite front, int value)
    {
        GameObject cardCopy = Instantiate(card);
        cards.Add(cardCopy);

        float coordX = 1.4f * (cards.Count - 1);
        Vector3 pos = new Vector3(coordX, coordY, 0);
        cardCopy.transform.position = pos;

        cardCopy.GetComponent<CardModel>().front = front;
        cardCopy.GetComponent<CardModel>().value = value;

        if (isDealer && cards.Count <= 1)
            cardCopy.GetComponent<CardModel>().ToggleFace(false);
        else
            cardCopy.GetComponent<CardModel>().ToggleFace(true);

        int val = 0;
        int aces = 0;

        foreach (GameObject f in cards)
        {
            if (f.GetComponent<CardModel>().value != 1)
                val += f.GetComponent<CardModel>().value;
            else
                aces++;
        }

        for (int i = 0; i < aces; i++)
        {
            if (val + 11 <= 21)
                val += 11;
            else
                val += 1;
        }

        points = val;
    }
}