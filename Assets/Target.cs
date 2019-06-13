using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Target : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
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

        MoveArm(transform.position);
    }

    void MoveArm(Vector3 target)
    {
        float eps = 1e-6f;
        // swap y-axis and z-axis
        target = new Vector3(target.x, target.z, target.y);

        // orientation
        Vector4 n = new Vector4(1, 0, 0, 0);
        Vector4 o = new Vector4(0, -1, 0, 0);
        Vector4 a = new Vector4(0, 0, -1, 0);
        // float len = target.sqrMagnitude;
        // Vector4 a = new Vector4(target.x / len, target.y / len, target.z / len, 0);
        // Vector4 n = Vector3.Cross(Vector3.up, target).normalized;
        // Vector4 o = Vector3.Cross(a, n);

        // postition
        Vector4 p = new Vector4(target.x, target.y, target.z, 1);

        Matrix4x4 T06 = new Matrix4x4(n, o, a, p);

        // D-H parameters
        float d1 = 8.9159f;
        float a2 = -42.5f;
        float a3 = -39.225f;
        float d4 = 10.915f;
        float d5 = 9.465f;
        float d6 = 8.23f;

        // https://blog.csdn.net/fengyu19930920/article/details/81144042

        // float th1, th2, th3, th4, th5, th6;
        // float u, v;

        // u = d6 * a.y - p.y;
        // v = d6 * a.x - p.x;
        // th1 = Mathf.Atan2(u, v) - Mathf.Atan2(d4, Mathf.Sqrt(u * u + v * v - d4 * d4));
        // float s1 = Mathf.Sin(th1), c1 = Mathf.Cos(th1);

        // th5 = Mathf.Acos(a.x * s1 - a.y * c1);
        // float s5 = Mathf.Sin(th5), c5 = Mathf.Cos(th5);

        // if (Mathf.Abs(s5) < eps)
        //     throw new UnityException("boom!");

        // u = n.x * s1 - n.y * c1;
        // v = o.x * s1 - o.y * c1;
        // th6 = Mathf.Atan2(u / s5, v / s5);
        // float s6 = Mathf.Sin(th6), c6 = Mathf.Cos(th6);

        // u = d5 * (s6 * (n.x * c1 + n.y * s1) + c6 * (o.x * c1 + o.y * s1)) - d6 * (a.x * c1 + a.y * s1) + p.x * c1 + p.y * s1;
        // v = p.z - d1 - a.z * d6 + d5 * (o.z * c6 + n.z * s6);
        // th3 = Mathf.Acos((u * u + v * v - a2 * a2 - a3 * a3) / (2 * a2 * a3));
        // float s3 = Mathf.Sin(th3), c3 = Mathf.Cos(th3);

        // float s2 = ((a3 * c3 + a2) * v - a3 * s3 * u) / (a2 * a2 + a3 * a3 + 2 * a2 * a3 * c3);
        // float c2 = (u + a3 * s3 * s2) / (a3 * c3 + a2);
        // th2 = Mathf.Atan2(s2, c2);

        // th4 = Mathf.Atan2(-s6 * (n.x * c1 + n.y * s1) - c6 * (o.x * c1 + o.y * s1), o.z * c6 + n.z * s6) - th2 - th3;

        float th1, th2, th3, th4, th5, th6;
        float u, v;

        Vector4 P0_5 = p - d6 * a;
        th1 = Mathf.Atan2(P0_5.y, P0_5.x) + Mathf.Acos(d4 / Mathf.Sqrt(P0_5.x * P0_5.x + P0_5.y * P0_5.y)) + Mathf.PI / 2;
        float s1 = Mathf.Sin(th1), c1 = Mathf.Cos(th1);

        th5 = Mathf.Acos((p.x * s1 - p.y * c1 - d4) / d6);
        float s5 = Mathf.Sin(th5), c5 = Mathf.Cos(th5);

        if (Mathf.Abs(s5) < eps)
            throw new UnityException("boom!");

        th6 = Mathf.Atan2((-o.x * s1 + o.y * c1) / s5, -(-n.x * s1 + n.y * c1) / s5);
        float s6 = Mathf.Sin(th6), c6 = Mathf.Cos(th6);

        u = d5 * (s6 * (n.x * c1 + n.y * s1) + c6 * (o.x * c1 + o.y * s1)) - d6 * (a.x * c1 + a.y * s1) + p.x * c1 + p.y * s1;
        v = p.z - d1 - a.z * d6 + d5 * (o.z * c6 + n.z * s6);
        th3 = Mathf.Acos((u * u + v * v - a2 * a2 - a3 * a3) / (2 * a2 * a3));
        float s3 = Mathf.Sin(th3), c3 = Mathf.Cos(th3);

        float s2 = ((a3 * c3 + a2) * v - a3 * s3 * u) / (a2 * a2 + a3 * a3 + 2 * a2 * a3 * c3);
        float c2 = (u + a3 * s3 * s2) / (a3 * c3 + a2);
        th2 = Mathf.Atan2(s2, c2);

        th4 = Mathf.Atan2(-s6 * (n.x * c1 + n.y * s1) - c6 * (o.x * c1 + o.y * s1), o.z * c6 + n.z * s6) - th2 - th3;

        float[] th = new float[] { th1, th2, th3, th4, th5, th6 };
        if (th.Any(x => float.IsNaN(x)))
        {
            Debug.Log("Out of boundary");
        }
        else
        {
            GameObject joint;
            joint = GameObject.Find("joint0");
            joint.transform.localEulerAngles = new Vector3(0, -th1 / Mathf.PI * 180 + 90, 0);
            joint = GameObject.Find("joint1");
            joint.transform.localEulerAngles = new Vector3(-th2 / Mathf.PI * 180 - 90, 0, 0);
            joint = GameObject.Find("joint2");
            joint.transform.localEulerAngles = new Vector3(-th3 / Mathf.PI * 180, 0, 0);
            joint = GameObject.Find("joint3");
            joint.transform.localEulerAngles = new Vector3(-th4 / Mathf.PI * 180 + 270, 0, 0);
            joint = GameObject.Find("joint4");
            joint.transform.localEulerAngles = new Vector3(0, -th5 / Mathf.PI * 180, 0);
            joint = GameObject.Find("joint5");
            joint.transform.localEulerAngles = new Vector3(-th6 / Mathf.PI * 180, 0, 0);
        }
    }
}
