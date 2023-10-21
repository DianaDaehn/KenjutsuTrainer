using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Logger : MonoBehaviour
{
    public Transform leftSword;
    public Transform rightSword;
    public Transform leftHand;
    public Transform rightHand;
    public Transform headSet;
    public LineRenderer leftTrail;
    public LineRenderer rightTrail;
    public GameObject recordingText;
    public int trailLength;
    private List<Frame> frames = new List<Frame>();
    private bool recording;
    private float playbackFrame;

    // Start is called before the first frame update
    void Start()
    {
        recordingText.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

        if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.Touch))
        {
            Debug.Log("Started recording");
            recording = true;
            recordingText.SetActive(true);
            frames.Clear();
            leftTrail.positionCount = 0;
            rightTrail.positionCount = 0;
        }

        if (OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.Touch))
        {
            Debug.Log("Stopped recording");
            recording = false;
            recordingText.SetActive(false);
            playbackFrame = frames.Count - 1;

            using (var writer = new StreamWriter(File.OpenWrite(Application.persistentDataPath + "/log-" + DateTime.Now.ToString("s") + ".csv")))
            {
                writer.Write("leftHandX,leftHandY,leftHandZ,leftHandQ0,leftHandQ1,leftHandQ2,leftHandQ3," +
                    "rightHandX,rightHandY,rightHandZ,rightHandQ0,rightHandQ1,rightHandQ2,rightHandQ3," +
                    "leftSwordX,leftSwordY,leftSwordZ,leftSwordQ0,leftSwordQ1,leftSwordQ2,leftSwordQ3," +
                    "rightSwordX,rightSwordY,rightSwordZ,rightSwordQ0,rightSwordQ1,rightSwordQ2,rightSwordQ3," +
                    "headX,headY,headZ,headQ0,headQ1,headQ2,headQ3");
                foreach (var frame in frames)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        writer.Write(frame.leftHandPosition[i]);
                        writer.Write(',');
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        writer.Write(frame.leftHandRotation[i]);
                        writer.Write(',');
                    }
                    for (int i = 0; i < 3; i++)
                    {
                        writer.Write(frame.rightHandPosition[i]);
                        writer.Write(',');
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        writer.Write(frame.rightHandRotation[i]);
                        writer.Write(',');
                    }
                    for (int i = 0; i < 3; i++)
                    {
                        writer.Write(frame.leftSwordPosition[i]);
                        writer.Write(',');
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        writer.Write(frame.leftSwordRotation[i]);
                        writer.Write(',');
                    }
                    for (int i = 0; i < 3; i++)
                    {
                        writer.Write(frame.rightSwordPosition[i]);
                        writer.Write(',');
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        writer.Write(frame.rightSwordRotation[i]);
                        writer.Write(',');
                    }
                    for (int i = 0; i < 3; i++)
                    {
                        writer.Write(frame.headPosition[i]);
                        writer.Write(',');
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        writer.Write(frame.headRotation[i]);
                        if (i < 3)
                        {
                            writer.Write(',');
                        }
                    }
                    writer.WriteLine();
                }
            }
        }

        if (!recording)
        {
            Vector2 input = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
            playbackFrame = Mathf.Clamp(playbackFrame + input.x * Time.deltaTime / Time.fixedDeltaTime, 0, frames.Count + trailLength);
            int index = Mathf.RoundToInt(playbackFrame);
            int min = Mathf.Max(index - trailLength, 0);
            int max = Mathf.Min(index, frames.Count - 1);
            int count = Mathf.Max(max - min, 0);
            leftTrail.positionCount = count;
            rightTrail.positionCount = count;
            for (int i = min; i < max; i++)
            {
                leftTrail.SetPosition(i - min, frames[i].leftSwordPosition);
                rightTrail.SetPosition(i - min, frames[i].rightSwordPosition);
            }
        }

        OVRInput.Update();
    }

    void FixedUpdate()
    {
        OVRInput.FixedUpdate();

        if (!recording)
        {
            return;
        }

        frames.Add(new Frame
        {
            leftSwordPosition = leftSword.position,
            leftSwordRotation = leftSword.rotation,
            rightSwordPosition = rightSword.position,
            rightSwordRotation = rightSword.rotation,
            headPosition = headSet.position,
            headRotation = headSet.rotation,
            leftHandPosition = leftHand.position,
            leftHandRotation = leftHand.rotation,
            rightHandPosition = rightHand.position,
            rightHandRotation = rightHand.rotation
        });

        leftTrail.positionCount++;
        rightTrail.positionCount++;
        leftTrail.SetPosition(leftTrail.positionCount-1, leftSword.position);
        rightTrail.SetPosition(rightTrail.positionCount-1, rightSword.position);
    }

    struct Frame
    {
        public Vector3 leftSwordPosition;
        public Vector3 rightSwordPosition;
        public Quaternion leftSwordRotation;
        public Quaternion rightSwordRotation;
        public Vector3 headPosition;
        public Quaternion headRotation;
        public Vector3 leftHandPosition;
        public Vector3 rightHandPosition;
        public Quaternion leftHandRotation;
        public Quaternion rightHandRotation;
    }
}
