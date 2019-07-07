using System;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using Windows.Kinect;

public class Main : MonoBehaviour
{
    private bool rotCamera = false;
    private Vector3 prevPos;

    /// <summary>
    /// Constant for clamping Z values of camera space points from being negative
    /// </summary>
    private const float InferredZPositionClamp = 0.1f;

    /// <summary>
    /// Active Kinect sensor
    /// </summary>
    private KinectSensor kinectSensor = null;

    /// <summary>
    /// Coordinate mapper to map one type of point to another
    /// </summary>
    private CoordinateMapper coordinateMapper = null;

    /// <summary>
    /// Reader for body frames
    /// </summary>
    private BodyFrameReader bodyFrameReader = null;

    /// <summary>
    /// Array for the bodies
    /// </summary>
    private Body[] bodies = null;

    /// <summary>
    /// definition of bones
    /// </summary>
    private List<Tuple<JointType, JointType>> bones;

    // Start is called before the first frame update
    void Start()
    {
        // one sensor is currently supported
        kinectSensor = KinectSensor.GetDefault();

        // get the coordinate mapper
        coordinateMapper = kinectSensor.CoordinateMapper;

        // get the depth (display) extents
        FrameDescription frameDescription = kinectSensor.DepthFrameSource.FrameDescription;

        // open the reader for the body frames
        bodyFrameReader = kinectSensor.BodyFrameSource.OpenReader();
        bodyFrameReader.FrameArrived += BodyFrameReader_FrameArrived;

        // a bone defined as a line between two joints
        bones = new List<Tuple<JointType, JointType>>();

        // Torso
        bones.Add(new Tuple<JointType, JointType>(JointType.Head, JointType.Neck));
        bones.Add(new Tuple<JointType, JointType>(JointType.Neck, JointType.SpineShoulder));
        bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.SpineMid));
        bones.Add(new Tuple<JointType, JointType>(JointType.SpineMid, JointType.SpineBase));
        bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.ShoulderRight));
        bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.ShoulderLeft));
        bones.Add(new Tuple<JointType, JointType>(JointType.SpineBase, JointType.HipRight));
        bones.Add(new Tuple<JointType, JointType>(JointType.SpineBase, JointType.HipLeft));

        // Right Arm
        bones.Add(new Tuple<JointType, JointType>(JointType.ShoulderRight, JointType.ElbowRight));
        bones.Add(new Tuple<JointType, JointType>(JointType.ElbowRight, JointType.WristRight));
        bones.Add(new Tuple<JointType, JointType>(JointType.WristRight, JointType.HandRight));
        bones.Add(new Tuple<JointType, JointType>(JointType.HandRight, JointType.HandTipRight));
        bones.Add(new Tuple<JointType, JointType>(JointType.WristRight, JointType.ThumbRight));

        // Left Arm
        bones.Add(new Tuple<JointType, JointType>(JointType.ShoulderLeft, JointType.ElbowLeft));
        bones.Add(new Tuple<JointType, JointType>(JointType.ElbowLeft, JointType.WristLeft));
        bones.Add(new Tuple<JointType, JointType>(JointType.WristLeft, JointType.HandLeft));
        bones.Add(new Tuple<JointType, JointType>(JointType.HandLeft, JointType.HandTipLeft));
        bones.Add(new Tuple<JointType, JointType>(JointType.WristLeft, JointType.ThumbLeft));

        // Right Leg
        bones.Add(new Tuple<JointType, JointType>(JointType.HipRight, JointType.KneeRight));
        bones.Add(new Tuple<JointType, JointType>(JointType.KneeRight, JointType.AnkleRight));
        bones.Add(new Tuple<JointType, JointType>(JointType.AnkleRight, JointType.FootRight));

        // Left Leg
        bones.Add(new Tuple<JointType, JointType>(JointType.HipLeft, JointType.KneeLeft));
        bones.Add(new Tuple<JointType, JointType>(JointType.KneeLeft, JointType.AnkleLeft));
        bones.Add(new Tuple<JointType, JointType>(JointType.AnkleLeft, JointType.FootLeft));

        // set IsAvailableChanged event notifier
        kinectSensor.IsAvailableChanged += KinectSensor_IsAvailableChanged;

        // open the sensor
        kinectSensor.Open();
    }

    // Update is called once per frame
    void Update()
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

    private void KinectSensor_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
    {
        Debug.Log($"Available Changed : {e.IsAvailable}");
    }

    private void BodyFrameReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
    {
        bool dataReceived = false;

        using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
        {
            if (bodyFrame != null)
            {
                if (bodies == null)
                {
                    bodies = new Body[bodyFrame.BodyCount];
                }

                bodyFrame.GetAndRefreshBodyData(bodies);
                dataReceived = true;
            }
        }

        if (dataReceived)
        {
            foreach (Body body in bodies)
            {
                if (body.IsTracked)
                {
                    IReadOnlyDictionary<JointType, Windows.Kinect.Joint> joints = body.Joints;
                }
            }
        }
    }

    void OnDestroy()
    {
        bodyFrameReader?.Dispose();
        bodyFrameReader = null;

        kinectSensor?.Close();
        kinectSensor = null;
    }
}
