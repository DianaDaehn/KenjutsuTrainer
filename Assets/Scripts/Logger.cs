using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class Logger : MonoBehaviour
{
    public Transform leftSword;
    public Transform rightSword;
    public Transform leftHand;
    public Transform rightHand;
    public Transform headSet;
    public LineRenderer leftTrail;
    public LineRenderer rightTrail;
    public Button recordingText;
    public int trailLength;
    private List<Frame> frames = new List<Frame>();
    private bool recording;
    private float playbackFrame;
    private float trailLengthCurrent;

    // Start is called before the first frame update
    void Start()
    {
        recording = false;
    }

    public void OnRecordButtonClick()
    {
        if (recording)
        {
            StopRecording();
        }
        else
        {
            StartRecording();
        }
    }

    void StartRecording()
    {
        Debug.Log("Started recording");
        recording = true;
        var colors = recordingText.colors;
        colors.normalColor = Color.red;
        recordingText.colors = colors;
        frames.Clear();
        leftTrail.positionCount = 0;
        rightTrail.positionCount = 0;
        trailLengthCurrent = trailLength; // reset to the default traillength
    }

    void StopRecording()
    {
        Debug.Log("Stopped recording");
        recording = false;
        var colors = recordingText.colors;
        colors.normalColor = Color.white;
        recordingText.colors = colors;
        playbackFrame = 10.0f;

        using (var writer = new StreamWriter(File.OpenWrite(Application.persistentDataPath + "/log-" + DateTime.Now.ToString("s") + ".csv")))
        {
            writer.WriteLine("timestamp," +
                "left_hand_x,left_hand_y,left_hand_z,left_hand_euler_x,left_hand_euler_y,left_hand_euler_z," +
                "right_hand_x,right_hand_y,right_hand_z,right_hand_euler_x,right_hand_euler_y,right_hand_euler_z," +
                "left_sword_x,left_sword_y,left_sword_z," +
                "right_sword_x,right_sword_y,right_sword_z," +
                "head_x,head_y,head_z,head_euler_x,head_euler_y,head_euler_z");

            /*
             * In Unity the x,y,z is orientated like the monitor.
             * x..left-right
             * y..up-down
             * z..depth into the monitor
             * We want x,y to be the earth and z the height.
             */

            foreach (var frame in frames)
            {
                writer.Write(frame.time);
                writer.Write(',');

                writer.Write(frame.leftHandPosition[2].ToString("F3", CultureInfo.InvariantCulture));
                writer.Write(',');
                writer.Write(frame.leftHandPosition[0].ToString("F3", CultureInfo.InvariantCulture));
                writer.Write(',');
                writer.Write(frame.leftHandPosition[1].ToString("F3", CultureInfo.InvariantCulture));
                writer.Write(',');

                writer.Write(frame.leftHandEuler[2].ToString("F1", CultureInfo.InvariantCulture));
                writer.Write(',');
                writer.Write(frame.leftHandEuler[0].ToString("F1", CultureInfo.InvariantCulture));
                writer.Write(',');
                writer.Write(frame.leftHandEuler[1].ToString("F1", CultureInfo.InvariantCulture));
                writer.Write(',');

                writer.Write(frame.rightHandPosition[2].ToString("F3", CultureInfo.InvariantCulture));
                writer.Write(',');
                writer.Write(frame.rightHandPosition[0].ToString("F3", CultureInfo.InvariantCulture));
                writer.Write(',');
                writer.Write(frame.rightHandPosition[1].ToString("F3", CultureInfo.InvariantCulture));
                writer.Write(',');

                writer.Write(frame.rightHandEuler[2].ToString("F1", CultureInfo.InvariantCulture));
                writer.Write(',');
                writer.Write(frame.rightHandEuler[0].ToString("F1", CultureInfo.InvariantCulture));
                writer.Write(',');
                writer.Write(frame.rightHandEuler[1].ToString("F1", CultureInfo.InvariantCulture));
                writer.Write(',');

                writer.Write(frame.leftSwordPosition[2].ToString("F3", CultureInfo.InvariantCulture));
                writer.Write(',');
                writer.Write(frame.leftSwordPosition[0].ToString("F3", CultureInfo.InvariantCulture));
                writer.Write(',');
                writer.Write(frame.leftSwordPosition[1].ToString("F3", CultureInfo.InvariantCulture));
                writer.Write(',');

                writer.Write(frame.rightSwordPosition[2].ToString("F3", CultureInfo.InvariantCulture));
                writer.Write(',');
                writer.Write(frame.rightSwordPosition[0].ToString("F3", CultureInfo.InvariantCulture));
                writer.Write(',');
                writer.Write(frame.rightSwordPosition[1].ToString("F3", CultureInfo.InvariantCulture));
                writer.Write(',');

                writer.Write(frame.headPosition[2].ToString("F3", CultureInfo.InvariantCulture));
                writer.Write(',');
                writer.Write(frame.headPosition[0].ToString("F3", CultureInfo.InvariantCulture));
                writer.Write(',');
                writer.Write(frame.headPosition[1].ToString("F3", CultureInfo.InvariantCulture));
                writer.Write(',');

                writer.Write(frame.headEuler[2].ToString("F1", CultureInfo.InvariantCulture));
                writer.Write(',');
                writer.Write(frame.headEuler[0].ToString("F1", CultureInfo.InvariantCulture));
                writer.Write(',');
                writer.Write(frame.headEuler[1].ToString("F1", CultureInfo.InvariantCulture));
                writer.WriteLine();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (OVRInput.GetDown(OVRInput.Button.Four, OVRInput.Controller.Touch))
        {
            StartRecording();
        }

        if (recording && OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.Touch))
        {
            StopRecording();
        }

        if (!recording)
        {
            Vector2 input = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
            Vector2 input2 = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
            trailLengthCurrent = Mathf.Clamp(trailLengthCurrent + input2.x * Time.deltaTime / Time.fixedDeltaTime, trailLength, frames.Count);
            playbackFrame = Mathf.Clamp(playbackFrame + input.x * Time.deltaTime / Time.fixedDeltaTime, 0, frames.Count + trailLengthCurrent);
            int startpoint = Mathf.Min(Mathf.RoundToInt(playbackFrame), frames.Count - 1);
            int trail = Mathf.Min(Mathf.Max(Mathf.RoundToInt(playbackFrame) - Mathf.RoundToInt(trailLengthCurrent), 0), frames.Count - 1);
            int count = Mathf.Max(startpoint - trail, 0);
            leftTrail.positionCount = count;
            rightTrail.positionCount = count;
            for (int i = trail; i < startpoint; i++)
            {
                leftTrail.SetPosition(i - trail, frames[i].leftSwordPosition);
                rightTrail.SetPosition(i - trail, frames[i].rightSwordPosition);
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
            headEuler = headSet.eulerAngles,
            headEulerLocal = headSet.localEulerAngles,
            leftHandPosition = leftHand.position,
            leftHandRotation = leftHand.rotation,
            leftHandEuler = leftHand.eulerAngles,
            leftHandEulerLocal = leftHand.localEulerAngles,
            rightHandPosition = rightHand.position,
            rightHandRotation = rightHand.rotation,
            rightHandEuler = rightHand.eulerAngles,
            rightHandEulerLocal = rightHand.localEulerAngles,
            time = DateTimeOffset.Now.ToUnixTimeMilliseconds()
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
        public Vector3 headEuler;
        public Vector3 headEulerLocal;
        public Vector3 leftHandPosition;
        public Vector3 rightHandPosition;
        public Vector3 leftHandEuler;
        public Vector3 leftHandEulerLocal;
        public Vector3 rightHandEuler;
        public Vector3 rightHandEulerLocal;
        public Quaternion leftHandRotation;
        public Quaternion rightHandRotation;
        public long time;
    }
}
