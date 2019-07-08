using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Manipulation : MonoBehaviour
{
    private Vector3 prevPos;
    private bool rotCamera = false;
    private float height = 0;
    private float angleSpeed = 45f; // deg/s

    private GameObject target;

    void Start()
    {
        target = GameObject.Find("Target");
    }

    void Update()
    {
        MoveCamera();
        MoveTarget();
        EaseArm(target.transform.position);
    }

    private void MoveCamera()
    {
        Camera cam = Camera.main;
        Vector3 up = cam.transform.up;
        Vector3 right = cam.transform.right;
        Vector3 forward = cam.transform.forward;

        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
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
    }

    private void MoveTarget()
    {
        float dt = 25;
        Vector3 position = target.transform.position;
        if (Input.GetKey(KeyCode.Z))
        {
            height -= Time.deltaTime * dt;
            position.y = height;
        }
        if (Input.GetKey(KeyCode.C))
        {
            height += Time.deltaTime * dt;
            position.y = height;
        }
        if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane ground = new Plane(Vector3.up, new Vector3(0, height, 0));
            ground.Raycast(ray, out float enter);
            position = ray.GetPoint(enter);
        }
        target.transform.position = position;
    }

    private float[] GetTransformAngles(Vector3 target)
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

        // if (Mathf.Abs(s5) < eps)
        //     throw new UnityException("boom!");

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

        return new float[] { th1, th2, th3, th4, th5, th6 };
    }

    private void EaseArm(Vector3 target)
    {
        float[] targetAngles = AngleFromRobotToUnity(GetTransformAngles(target));
        if (targetAngles.Any(x => float.IsNaN(x)))
        {
            Debug.Log("Out of boundary");
            return;
        }
        float[] moveAngles = new float[6];
        float amount = angleSpeed * Time.deltaTime;
        for (int i = 0; i < 6; i++)
            moveAngles[i] = AngleInterpolation(ArmAngles[i], targetAngles[i], amount);
        ArmAngles = moveAngles;
    }

    private float[] AngleFromRobotToUnity(float[] angleInRobot)
    {
        float[] angleInUnity = new float[6];
        float[] offset = { 90, -90, 0, 270, 0, 0 };
        for (int i = 0; i < 6; i++)
            angleInUnity[i] = AngleClamp(-angleInRobot[i] / Mathf.PI * 180 + offset[i]);
        return angleInUnity;
    }

    private float AngleInterpolation(float currentAngles, float targetAngles, float amount)
    {
        float next;
        float dt = targetAngles - currentAngles;
        if (Mathf.Abs(dt) < 180)
        {
            next = Mathf.Abs(dt) < amount ? targetAngles : currentAngles + Mathf.Sign(dt) * amount;
        }
        else
        {
            next = Mathf.Abs(dt) < amount ? targetAngles : currentAngles - Mathf.Sign(dt) * amount;
        }
        return AngleClamp(next);
    }

    private float AngleClamp(float angle)
    {
        while (angle < 0)
            angle += 360;
        while (angle >= 360)
            angle -= 360;
        return angle;
    }

    private float[] armAngles = new float[6];
    public float[] ArmAngles
    {
        set
        {
            if (value == null || value.Length != 6)
                throw new System.ArgumentException("ArmAngles should be a 6-element array");
            if (value.Any(x => float.IsNaN(x)))
            {
                Debug.Log("Out of boundary");
            }
            else
            {
                armAngles = value;

                GameObject joint;
                joint = GameObject.Find("joint0");
                joint.transform.localEulerAngles = new Vector3(0, value[0], 0);
                joint = GameObject.Find("joint1");
                joint.transform.localEulerAngles = new Vector3(value[1], 0, 0);
                joint = GameObject.Find("joint2");
                joint.transform.localEulerAngles = new Vector3(value[2], 0, 0);
                joint = GameObject.Find("joint3");
                joint.transform.localEulerAngles = new Vector3(value[3], 0, 0);
                joint = GameObject.Find("joint4");
                joint.transform.localEulerAngles = new Vector3(0, value[4], 0);
                joint = GameObject.Find("joint5");
                joint.transform.localEulerAngles = new Vector3(value[5], 0, 0);
            }
        }
        get
        {
            return armAngles;
        }
    }
}
