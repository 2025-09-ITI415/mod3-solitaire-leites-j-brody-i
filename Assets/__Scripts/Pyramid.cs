using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Deck))]
[RequireComponent(typeof(JsonParseLayout))]
public class Pyramid : MonoBehaviour 
{
    static public Pyramid S; // Singleton

    [Header("Dynamic")]
    public List<CardPyramid> drawPile;
    public List<CardPyramid> discardPile;
    public List<CardPyramid> tableau; 
    public CardPyramid selection; 

    private Transform layoutAnchor;
    private Deck deck;
    private JsonLayout jsonLayout;
    private Dictionary<int, CardPyramid> tableauIdToCardDict;

    void Start() 
    {
        if (S != null) Debug.LogError("Attempted to set S more than once!");
        S = this;

        jsonLayout = GetComponent<JsonParseLayout>().layout;
        deck = GetComponent<Deck>();
        deck.InitDeck();
        Deck.Shuffle(ref deck.cards);

        drawPile = ConvertCardsToCardPyramids(deck.cards);
        LayoutGame();
    }

    List<CardPyramid> ConvertCardsToCardPyramids(List<Card> listCard) 
    {
        List<CardPyramid> listCP = new List<CardPyramid>();
        CardPyramid cp;
        foreach (Card card in listCard) {
            cp = card as CardPyramid;
            listCP.Add(cp);
        }
        return listCP;
    }

    void LayoutGame() 
    {
        if (layoutAnchor == null) {
            GameObject tGO = new GameObject("_LayoutAnchor");
            layoutAnchor = tGO.transform;
        }

        CardPyramid cp;
        tableauIdToCardDict = new Dictionary<int, CardPyramid>();

        foreach (JsonLayoutSlot slot in jsonLayout.slots) 
        {
            cp = Draw();
            cp.faceUp = slot.faceUp;
            cp.transform.SetParent(layoutAnchor);

            int z = int.Parse(slot.layer[slot.layer.Length - 1].ToString());
            cp.SetLocalPos(new Vector3(
                jsonLayout.multiplier.x * slot.x,
                jsonLayout.multiplier.y * slot.y,
                -z));

            cp.layoutID = slot.id;
            cp.layoutSlot = slot;
            cp.state = ePyramidCardState.tableau;
            cp.SetSpriteSortingLayer(slot.layer);

            tableau.Add(cp);
            tableauIdToCardDict.Add(slot.id, cp);
        }

        UpdateDrawPile();

        SetTableauFaceUps();
    }

    CardPyramid Draw() 
    {
        if (drawPile.Count == 0) return null;
        CardPyramid cp = drawPile[0];
        drawPile.RemoveAt(0);
        return cp;
    }

    void UpdateDrawPile() 
    {
        CardPyramid cp;
        for (int i = 0; i < drawPile.Count; i++) {
            cp = drawPile[i];
            cp.transform.SetParent(layoutAnchor);
            Vector3 cpPos = new Vector3();
            cpPos.x = jsonLayout.multiplier.x * jsonLayout.drawPile.x;
            cpPos.x += jsonLayout.drawPile.xStagger * i;
            cpPos.y = jsonLayout.multiplier.y * jsonLayout.drawPile.y;
            cpPos.z = 0.1f * i;
            cp.SetLocalPos(cpPos);
            cp.faceUp = false;
            cp.state = ePyramidCardState.drawpile;
            cp.SetSpriteSortingLayer(jsonLayout.drawPile.layer);
            cp.SetSortingOrder(-10 * i);
        }
    }

    void MoveToDiscard(CardPyramid cp) 
    {
        cp.state = ePyramidCardState.discard;
        discardPile.Add(cp);
        cp.transform.SetParent(layoutAnchor);
        cp.SetLocalPos(new Vector3(
            jsonLayout.multiplier.x * jsonLayout.discardPile.x,
            jsonLayout.multiplier.y * jsonLayout.discardPile.y,
            0));
        cp.faceUp = true;
        cp.SetSpriteSortingLayer(jsonLayout.discardPile.layer);
        cp.SetSortingOrder(-200 + (discardPile.Count * 3));
    }

    void SetTableauFaceUps() 
    {
        foreach (CardPyramid cp in tableau) {
            bool faceUp = true; 
            foreach (int coverID in cp.layoutSlot.hiddenBy) {
                if (tableauIdToCardDict.ContainsKey(coverID)) {
                    if (tableauIdToCardDict[coverID].state == ePyramidCardState.tableau) {
                        faceUp = false;
                    }
                }
            }
            cp.faceUp = faceUp;
        }
    }

    static public void CARD_CLICKED(CardPyramid cp) 
    {
        switch (cp.state) 
        {
            case ePyramidCardState.drawpile:
                CardPyramid topCard = S.Draw();
                if (topCard != null) {
                    S.MoveToDiscard(topCard);
                    S.UpdateDrawPile();
                }
                break;

            case ePyramidCardState.tableau:
            case ePyramidCardState.discard:
                if (!cp.faceUp) return;

                if (cp.rank == 13) {
                    S.RemoveCardFromPlay(cp);
                    if (S.selection != null) {
                        S.selection.SetSelected(false);
                        S.selection = null;
                    }
                    return;
                }

                if (S.selection == null) {
                    S.selection = cp;
                    S.selection.SetSelected(true);
                } else {
                    if (S.selection == cp) {
                        S.selection.SetSelected(false);
                        S.selection = null;
                    } else {
                        if (S.selection.rank + cp.rank == 13) {
                            S.RemoveCardFromPlay(S.selection);
                            S.RemoveCardFromPlay(cp);
                            S.selection = null;
                        } else {
                            S.selection.SetSelected(false);
                            S.selection = cp;
                            S.selection.SetSelected(true);
                        }
                    }
                }
                break;
        }
    }

    void RemoveCardFromPlay(CardPyramid cp) 
    {
        if (tableau.Contains(cp)) tableau.Remove(cp);
        if (discardPile.Contains(cp)) discardPile.Remove(cp);
        cp.transform.position = new Vector3(100, 100, 0);
        cp.state = ePyramidCardState.removed;
        SetTableauFaceUps();
    }
}
