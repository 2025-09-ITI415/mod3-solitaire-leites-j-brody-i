using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Defines the specific states a card can be in for Pyramid Solitaire
public enum ePyramidCardState { drawpile, tableau, discard, removed }

public class CardPyramid : Card 
{
    [Header("Dynamic: CardPyramid")]
    public ePyramidCardState state = ePyramidCardState.drawpile;
    public List<CardPyramid> hiddenBy = new List<CardPyramid>();
    public int layoutID;
    public JsonLayoutSlot layoutSlot;

    override public void OnMouseUpAsButton() {
        // Instead of handling logic here, we tell the main GameManager
        Pyramid.CARD_CLICKED(this);
        // We do NOT call base.OnMouseUpAsButton() to avoid the print statement
    }

    // Helper to visually highlight the card
    public void SetSelected(bool select) {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) {
            // Tint Yellow if selected, White if not
            sr.color = select ? Color.yellow : Color.white;
        }
    }
}
