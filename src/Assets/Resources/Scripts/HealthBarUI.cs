using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] ValueBarUI bar;
    [SerializeField] Tree tree;

    protected void Start()
    {
        var constants = GameController.Instance.Constants;
        bar.SetValue( constants.treeInitialStats.health, constants.treeInitialStats.maxHealth );
    }

    protected void OnDisable()
    {
        tree.OnHealthChanged -= Tree_OnHealthChanged;
    }

    private void OnEnable()
    {
        tree.OnHealthChanged += Tree_OnHealthChanged;

        var constants = GameController.Instance.Constants;
        bar.SetValue( constants.treeInitialStats.health, constants.treeInitialStats.maxHealth );
    }

    private void Tree_OnHealthChanged( TreeStats oldStats, TreeStats newStats, DamageType type )
    {
        bar.SetValue( newStats.health, newStats.maxHealth, type != DamageType.Cheat );
    }
}
