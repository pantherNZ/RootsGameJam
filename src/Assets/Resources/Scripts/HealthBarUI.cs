using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] ValueBarUI bar;
    [SerializeField] Tree tree;
    [SerializeField] GameConstants constants;

    private void Start()
    {
        bar.SetValue( constants.treeInitialStats.health, constants.treeInitialStats.maxHealth );
        tree.OnHealthChanged += Tree_OnHealthChanged;
    }

    private void Tree_OnHealthChanged( TreeStats oldStats, TreeStats newStats, DamageType type )
    {
        bar.SetValue( newStats.health, newStats.maxHealth );
    }
}
