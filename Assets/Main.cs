using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    private bool rotCamera = false;
    private Vector3 prevPos;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Camera cam = Camera.main;
        Vector3 up = cam.transform.up;
        Vector3 right = cam.transform.right;
        Vector3 forward = cam.transform.forward;

        if (Input.GetMouseButtonDown(0))
        {
            rotCamera = true;
            prevPos = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(0))
        {
            rotCamera = false;
        }
        if (rotCamera)
        {
            Vector3 d = Input.mousePosition - prevPos;
            cam.transform.Rotate(Vector3.up, d.x, Space.World);
            cam.transform.Rotate(Vector3.right, -d.y, Space.Self);
        }
        prevPos = Input.mousePosition;

        float dt = 25;
        var keyMap = new Dictionary<KeyCode, Vector3>(){
            {KeyCode.D,right},
            {KeyCode.A,-right},
            {KeyCode.W,forward},
            {KeyCode.S,-forward},
            {KeyCode.Q,Vector3.up},
            {KeyCode.E,Vector3.down},
        };
        foreach (var map in keyMap)
            if (Input.GetKey(map.Key))
                cam.transform.position += Time.deltaTime * dt * map.Value;
    }

    void LateUpdate()
    {
        //Camera.main.transform.LookAt(Vector3.zero);
    }
}
