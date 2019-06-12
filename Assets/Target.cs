using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveTarget : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        transform.position = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 up = transform.up;
        Vector3 right = transform.right;
        Vector3 forward = transform.forward;
        float dt = 25;
        var keyMap = new Dictionary<KeyCode, Vector3>(){
            {KeyCode.L,right},
            {KeyCode.J,-right},
            {KeyCode.I,forward},
            {KeyCode.K,-forward},
            {KeyCode.U,Vector3.up},
            {KeyCode.O,Vector3.down},
        };
        foreach (var map in keyMap)
            if (Input.GetKey(map.Key))
                transform.position += Time.deltaTime * dt * map.Value;
    }
}
