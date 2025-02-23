using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{

    public float resetSpeed = 0.5f;
    public float cameraSpeed = 0.3f;

    public Bounds cameraBounds;

    private Transform target;

    private float offsetZ;
    private Vector3 lastTargetPosition;
    private Vector3 currentVelocity;

    private bool followsPlayer;

    void Awake()
    {
        BoxCollider2D myCollider = GetComponent<BoxCollider2D>();
        myCollider.size = new Vector2(Camera.main.aspect * 2f * Camera.main.orthographicSize, 15f);

        cameraBounds = myCollider.bounds;
    }
    void Start()
    {
        target = GameObject.FindGameObjectWithTag(MyTags.PLAYER_TAG).transform;
        lastTargetPosition = target.position;
        offsetZ = (transform.position - target.position).z;
        followsPlayer = true;
    }

    void FixedUpdate()
    {
        if(followsPlayer)
        {
            Vector3 aheadTarget = target.position + Vector3.forward * offsetZ;

            //daca playerul se misca in dreapta
            //if (target.position.x >= transform.position.x)
           // {
                Vector3 newCameraPosition = Vector3.SmoothDamp(transform.position, aheadTarget, ref currentVelocity, cameraSpeed);
                transform.position = new Vector3(newCameraPosition.x, transform.position.y, newCameraPosition.z);

                lastTargetPosition = target.position;
           // }
        }
    }
} //class






















