using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree : MonoBehaviour
{
    [SerializeField] int maxHealth;
    [SerializeField] GameObject treeLevelUpObj;
    [SerializeField] Color treeHighlightColour;
    [SerializeField] PlayerController controller;
    private int health;
    private Color baseColour;

    private void Start()
    {
        health = maxHealth;
        var sprite = treeLevelUpObj.GetComponent<SpriteRenderer>();
        baseColour = sprite.color;

        var ed = treeLevelUpObj.GetComponent<EventDispatcherV2>();
        ed.OnPointerEnterEvent.AddListener( x =>
        {
            sprite.color = treeHighlightColour;
            // Show level up cost/info popup
            controller.ShowLevelUpPopup();
        } );
        ed.OnPointerExitEvent.AddListener( x =>
        {
            sprite.color = baseColour;
        } );
        ed.OnPointerDownEvent.AddListener( x =>
        {
            controller.LevelUp();
        } );
    }
}
