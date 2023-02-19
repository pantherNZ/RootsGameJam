using UnityEngine;

public class DragCameraMovement : MonoBehaviour
{
    [SerializeField]
    PlayerController controller;

    private Vector3? dragOrigin;
    private Vector3 startingPos;
    private Camera mainCam;

    private void Start()
    {
        mainCam = Camera.main;
    }

    void Update()
    {
        if( controller.inMenu )
        {
            startingPos = controller.transform.position;
            return;
        }

        InputPriority.Instance.Request( () => Input.GetMouseButton( 0 ), "rootSelectionUI", -1, () =>
        {
            if( controller.newRoot != null )
                return;

            if( dragOrigin == null )
                dragOrigin = mainCam.ScreenToWorldPoint( Input.mousePosition );

            var difference = mainCam.ScreenToWorldPoint( Input.mousePosition ) - controller.transform.position;
            controller.transform.position = dragOrigin.Value - difference;
        } );

        if( !Input.GetMouseButton( 0 ) )
        {
            dragOrigin = null;
        }

        if( Input.GetKeyDown( KeyCode.Space ) )
        {
            controller.transform.position = startingPos;
            dragOrigin = null;
        }
    }
}
