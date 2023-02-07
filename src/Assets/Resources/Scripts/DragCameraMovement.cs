using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DragCameraMovement : MonoBehaviour
{

    [SerializeField]
    PlayerController controller;
    private Vector3 Origin;
    private Vector3 Difference;
    [SerializeField]
    private Vector3 ResetCamera;
    private bool drag = false;
    private float height;
    public float ScrollSpeed = 2f;
    private Camera mainCam;

    void Start(){
        mainCam = Camera.main;
        height = mainCam.ViewportToWorldPoint( new Vector3( 0.0f, 1.0f, 0.0f ) ).y;
    }

    void Update() {
        if( controller.inMenu )
            return;

        float currentScreenTop = mainCam.ViewportToWorldPoint( new Vector3( 0.0f, 1.0f, 0.0f ) ).y;
        float maxSize = currentScreenTop > height ? mainCam.orthographicSize : 10.0f;
        mainCam.orthographicSize = Mathf.Clamp( mainCam.orthographicSize - Input.mouseScrollDelta.y * ScrollSpeed, 3.0f, maxSize );

        InputPriority.Instance.Request( () => Input.GetMouseButton( 0 ), "rootSelectionUI", -1, () => {
            if (controller.newRoot != null) return;
            Difference = (mainCam.ScreenToWorldPoint(Input.mousePosition)) - controller.transform.position;
            if (drag == false) {
                drag = true;
                Origin = mainCam.ScreenToWorldPoint(Input.mousePosition);
            }
            if (drag == true) {
                Vector3 newCameraPos = Origin - Difference;
                if( mainCam.ViewportToWorldPoint( new Vector3( 0.0f, 1.0f, 0.0f ) ).y > height )
                {
                    newCameraPos.y = Mathf.Min( newCameraPos.y, controller.transform.position.y );
                    Origin = mainCam.ScreenToWorldPoint( Input.mousePosition );
                }
                controller.transform.position = newCameraPos;
            }
        });
        if (!Input.GetMouseButton( 0 )){
            drag = false;
        }
        if( Input.GetKeyDown( KeyCode.Space ) )
        {
            controller.transform.position = ResetCamera;
            mainCam.orthographicSize = 5.0f;
        }
    }
    
}
