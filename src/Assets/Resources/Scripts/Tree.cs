using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree : MonoBehaviour
{
    [SerializeField] float spinSpeed;
    [SerializeField] int maxHealth;
    [SerializeField] GameObject treeLevelUpObj;
    [SerializeField] Color treeHighlightColour;
    [SerializeField] PlayerController controller;
    private int health;
    private Color baseColour;
    private bool hovered = false;

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
            hovered = true;
            sprite.color = treeHighlightColour;
            // Show level up cost/info popup
            controller.ShowLevelUpPopup( true );
        } );
        ed.OnPointerExitEvent.AddListener( x =>
        {
            hovered = false;
            sprite.color = baseColour;
            controller.ShowLevelUpPopup( false );
        } );
        ed.OnPointerDownEvent.AddListener( x =>
        {
            hovered = false;
            controller.LevelUp();
        } );
    }

    private void Update()
    {
        if( hovered )
            treeLevelUpObj.transform.Rotate( 0.0f, 0.0f, spinSpeed * Time.deltaTime, Space.Self );
    }
}
