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
            if( controller.inMenu )
                return;
            sprite.color = treeHighlightColour;
            // Show level up cost/info popup
            controller.ShowLevelUpPopup( true );
        } );
        ed.OnPointerExitEvent.AddListener( x =>
        {
            sprite.color = baseColour;
            controller.ShowLevelUpPopup( false );
        } );
        ed.OnPointerDownEvent.AddListener( x =>
        {
            controller.LevelUp();
        } );
    }
}
