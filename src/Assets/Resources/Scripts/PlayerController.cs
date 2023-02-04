using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Resources
    [SerializeField] int co2;
    [SerializeField] int water;
    [SerializeField] int sun;
    [SerializeField] int level;
    [SerializeField] bool canPlaceRoots;
    [SerializeField] List<RootBase> rootTypes;
    [SerializeField] GameObject rootSelectionUI;
    [SerializeField] GameObject rootSelectionUIContent;
    [SerializeField] GameObject rootEntryUIPrefab;

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;

        // Find root connections and list to their click event
        var rootConnections = FindObjectsOfType<RootConnection>();

        foreach( var connection in rootConnections )
        {
            var connectionLocal = connection;
            connection.GetComponent<EventDispatcherV2>().OnPointerDownEvent.AddListener( x =>
            {
                ConnectionClicked( connectionLocal );
            } );
        }

        foreach( var type in rootTypes )
        {
            var entry = Instantiate( rootEntryUIPrefab, rootSelectionUIContent.transform );
        }
    }

    void ConnectionClicked( RootConnection connection )
    {
        // Show UI
        ( rootSelectionUI.transform as RectTransform ).anchoredPosition = mainCamera.WorldToScreenPoint( connection.transform.position );
    }

    void Update()
    {
        
    }
}
