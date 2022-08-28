using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehavior : MonoBehaviour
{
    private Vector3 camOffset = new Vector3(0f, 2f, -5f);
    private Vector3 playerMidOffset = new Vector3(0f, 1f, 0f);
    private Transform target;
    // private float dist = 5.385;
    
    // Start is called before the first frame update
    void Start()
    {
        target = GameObject.Find("Player").transform;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = target.TransformPoint(camOffset);
        transform.LookAt(target.position + playerMidOffset);
        if (Input.GetKey(KeyCode.UpArrow)) {
            camOffset.y += 0.05f;
        } else if (Input.GetKey(KeyCode.DownArrow)) {
            camOffset.y -= 0.05f;
        }
    }
}
