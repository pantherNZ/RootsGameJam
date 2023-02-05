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


    void Start(){
        height = Camera.main.ViewportToWorldPoint( new Vector3( 0.0f, 1.0f, 0.0f ) ).y;
    }


    void Update(){
        float ScreenSize = Camera.main.orthographicSize + Input.mouseScrollDelta.y * ScrollSpeed;
        if (ScreenSize >= 15 || ScreenSize <= 3) return;
        else{
            Camera.main.orthographicSize += Input.mouseScrollDelta.y * ScrollSpeed;
        }

        InputPriority.Instance.Request( () => Input.GetMouseButton( 0 ), "rootSelectionUI", -1, () => {
            if (controller.newRoot != null) return;
            Difference = (Camera.main.ScreenToWorldPoint(Input.mousePosition)) - Camera.main.transform.position;
            if (drag == false){
                drag = true;
                Origin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }
            if (drag == true) {
                Vector3 newCameraPos = Origin - Difference;
                if (newCameraPos.y >= height){
                    return;
                }
                Camera.main.transform.position = Origin - Difference;
            }
        });
        if (!Input.GetMouseButton( 0 )){
            drag = false;
        }
        if(Input.GetKey("space")) {
            Camera.main.transform.position = ResetCamera;
        }
    }
    
}
