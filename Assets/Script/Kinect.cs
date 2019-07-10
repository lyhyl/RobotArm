using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;
using Windows.Kinect;

public class Kinect : MonoBehaviour
{
    private bool kinectAvailable = false;
    private bool armCalibrated = false;
    private float armCalibrationRate;
    private float armLength;
    private const float ur5length = 83.9508189f;
    private KinectSensor kinect;
    private BodyFrameReader bodyFrameReader;
    private Body[] bodies;
    private Vector3 shoulderPosition;
    private Vector3 elbowPositon;
    private Vector3 handPosition;

    private GameObject target;

    void Start()
    {
        target = GameObject.Find("Target");

        kinect = KinectSensor.GetDefault();
        kinect.IsAvailableChanged += Kinect_IsAvailableChanged;

        FrameDescription frameDescription = kinect.DepthFrameSource.FrameDescription;
        bodyFrameReader = kinect.BodyFrameSource.OpenReader();
        bodyFrameReader.FrameArrived += BodyFrameReader_FrameArrived;

        kinect.Open();
    }

    void Update()
    {
    }

    void OnGUI()
    {
        string debugInfo =
            $"{nameof(kinectAvailable)} : {kinectAvailable}\n" +
            $"{nameof(armCalibrated)} : {armCalibrated}\n" +
            $"{nameof(armCalibrationRate)} : {armCalibrationRate}\n" +
            $"{nameof(shoulderPosition)} : {shoulderPosition}\n" +
            $"{nameof(handPosition)} : {handPosition}\n";
        GUI.Label(new Rect(0, 0, Screen.width, Screen.height), debugInfo);
    }

    private void Kinect_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
    {
        kinectAvailable = e.IsAvailable;
    }

    private void BodyFrameReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
    {
        // effector start at (-81.7, -0.6, -19.3)
        // Shoulder->Elbow->Wrist->Hand->HandTip
        // Wrist->Thumb

        bool dataReceived = false;

        using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
        {
            if (bodyFrame != null)
            {
                if (bodies == null)
                    bodies = new Body[bodyFrame.BodyCount];
                bodyFrame.GetAndRefreshBodyData(bodies);
                dataReceived = true;
            }
        }

        if (dataReceived)
        {
            var body = bodies.Where(b => b.IsTracked).FirstOrDefault();
            if (body != null)
            {
                var joints = body.Joints;
                var shoulder = joints[JointType.ShoulderRight];
                var elbow = joints[JointType.ElbowRight];
                var wrist = joints[JointType.WristRight];
                var hand = joints[JointType.HandRight];
                var handTip = joints[JointType.HandTipRight];
                var thumb = joints[JointType.ThumbRight];

                shoulderPosition = new Vector3(shoulder.Position.X, shoulder.Position.Y, shoulder.Position.Z);
                elbowPositon = new Vector3(elbow.Position.X, elbow.Position.Y, elbow.Position.Z);
                handPosition = new Vector3(hand.Position.X, hand.Position.Y, hand.Position.Z);

                var pe = ProjectPointOnLine(shoulderPosition, handPosition, elbowPositon);
                float dist = Vector3.Distance(pe, elbowPositon);
                float length = Vector3.Distance(shoulderPosition, handPosition);
                armCalibrationRate = dist / length;
                
                if (!armCalibrated && dist / length < 0.01)
                {
                    armCalibrated = true;
                    armLength = length;
                }

                if (armCalibrated)
                {
                    var diff = handPosition - shoulderPosition;
                    var tar = diff / armLength * ur5length;
                    var t = tar.z;
                    tar.z = tar.x;
                    tar.x = t;
                    target.transform.position = tar;
                }
            }
        }
    }

    public static Vector3 ProjectPointOnLine(Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 ab = b - a;
        float lensq = ab.sqrMagnitude;
        return a + Vector3.Dot(c - a, ab) / lensq * ab;
    }

    void OnDestroy()
    {
        bodyFrameReader?.Dispose();
        bodyFrameReader = null;

        kinect?.Close();
        kinect = null;
    }
}
