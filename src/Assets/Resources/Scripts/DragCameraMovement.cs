using UnityEngine;

public class DragCameraMovement : MonoBehaviour
{
    [SerializeField] PlayerController controller;
    [SerializeField] RectTransform aboveGroundBounds;

    private Vector3? dragOrigin;
    private Vector3 startingPos;
    private Camera mainCam;
    private Rect cameraLimRect;

    private void Start()
    {
        mainCam = Camera.main;

        cameraLimRect = aboveGroundBounds.GetWorldRect();
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

            var camTopLeft = mainCam.ViewportToWorldPoint( new Vector3( 0.0f, 1.0f, 0.0f ) );
            var camTopRight = mainCam.ViewportToWorldPoint( new Vector3( 1.0f, 1.0f, 0.0f ) );
            const float margin = 0.1f;
            var wasOutsideBounds = camTopLeft.x <= ( cameraLimRect.xMin - margin ) ||
                            camTopLeft.x > ( cameraLimRect.xMax + margin );

            if( dragOrigin == null )
                dragOrigin = mainCam.ScreenToWorldPoint( Input.mousePosition );

            var difference = mainCam.ScreenToWorldPoint( Input.mousePosition ) - controller.transform.position;
            controller.transform.position = dragOrigin.Value - difference;

            // Sideways limit
            var camHeight = mainCam.ViewportToWorldPoint( new Vector3( 0.0f, 1.0f, 0.0f ) ).y;
            if( camHeight > 0.0f && !wasOutsideBounds )
            {
                var newCamTopLeft = mainCam.ViewportToWorldPoint( new Vector3( 0.0f, 1.0f, 0.0f ) );
                if( newCamTopLeft.x <= cameraLimRect.xMin )
                {
                    var diff = newCamTopLeft.x - cameraLimRect.xMin;
                    var currentX = controller.transform.position.x;
                    controller.transform.position = controller.transform.position.SetX( currentX - diff );
                    startingPos = startingPos.SetX( controller.transform.position.x );
                }

                var newCamTopRight = mainCam.ViewportToWorldPoint( new Vector3( 1.0f, 1.0f, 0.0f ) );
                if( newCamTopRight.x >= cameraLimRect.xMax )
                {
                    var diff = newCamTopRight.x - cameraLimRect.xMax;
                    var currentX = controller.transform.position.x;
                    controller.transform.position = controller.transform.position.SetX( currentX - diff );
                    startingPos = startingPos.SetX( controller.transform.position.x );
                }
            }

            // Vertical limit
            camTopLeft = mainCam.ViewportToWorldPoint( new Vector3( 0.0f, 1.0f, 0.0f ) );
            camTopRight = mainCam.ViewportToWorldPoint( new Vector3( 1.0f, 1.0f, 0.0f ) );
            var outsideBounds = camTopLeft.x <= ( cameraLimRect.xMin - margin ) ||
                            camTopLeft.x > ( cameraLimRect.xMax + margin );
            var heightMax = outsideBounds ? 0.0f : cameraLimRect.yMax - margin;

            if( camTopLeft.y >= heightMax )
            {
                var diff = camTopLeft.y - heightMax;
                var currentHeight = controller.transform.position.y;
                controller.transform.position = controller.transform.position.SetY( currentHeight - diff );
                startingPos = startingPos.SetY( controller.transform.position.y );
            }
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
