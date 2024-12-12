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

    static YPRAngles GetYPRAngles(Quaternion q)
    {
        YPRAngles angles;
        angles.yaw =   Mathf.Rad2Deg * Mathf.Atan2(2 * q.y * q.w - 2 * q.x * q.z, 1 - 2 * q.y * q.y - 2 * q.z * q.z);
        angles.pitch = Mathf.Rad2Deg * Mathf.Atan2(2 * q.x * q.w - 2 * q.y * q.z, 1 - 2 * q.x * q.x - 2 * q.z * q.z);
        angles.roll =  Mathf.Rad2Deg * Mathf.Asin(2  * q.x * q.y + 2 * q.z * q.w);
        return angles;
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
            writer.WriteLine("timestamp," +                                                 // 0: time
                "left_hand_x,left_hand_y,left_hand_z," +                                    // 1: left_hand
                "left_q_x,left_q_y,left_q_z,left_q_w," +                                    // 2: left_q
                "left_euler_x,left_euler_y,left_euler_z," +                                 // 3: left_euler
                "left_yaw,left_pitch,left_roll," +                                          // 4: left_ypr
                "right_hand_x,right_hand_y,right_hand_z," +                                 // 5: right_hand
                "right_q_x,right_q_y,right_q_z,right_q_w," +                                // 6: right_sword_q
                "right_euler_x,right_euler_y,right_euler_z," +                              // 7: right_euler
                "right_yaw,right_pitch,right_roll," +                                       // 8: right_ypr
                "left_sword_x,left_sword_y,left_sword_z," +                                 // 9: left_sword
                "right_sword_x,right_sword_y,right_sword_z," +                              // 10: right_sword
                "head_x,head_y,head_z," +                                                   // 11: head
                "head_q_x,head_q_y,head_q_z,head_q_w," +                                    // 12: head_q
                "head_euler_x,head_euler_y,head_euler_z," +                                 // 13: head_euler
                "head_yaw,head_pitch,head_roll," +                                          // 14: head_ypr
                "left_heading_vector_x,left_heading_vector_y,left_heading_vector_z," +      // 15: left_heading_vector
                "right_heading_vector_x,right_heading_vector_y,right_heading_vector_z," +   // 16: right_heading_vector
                "head_heading_vector_x,head_heading_vector_y,head_heading_vector_z," +      // 17: head_heading_vector
                "left_blade_vector_x,left_blade_vector_y,left_blade_vector_z," +            // 18: left_blade_vector
                "right_blade_vector_x,right_blade_vector_y,right_blade_vector_z");          // 19: right_blade_vector

            /*
             * In Unity the x,y,z is orientated like the monitor.
             * x..left-right
             * y..up-down
             * z..depth into the monitor
             * We want x,y to be the earth and z the height.
             */

            /* Within Unity: [uX, uY, uZ, uW] with W being extra for quaternions
             * Within my Script: {sX: uZ, sY: uX, sZ: uY, sW: uW}
             */

            foreach (var frame in frames)
            {
                writer.Write(frame.time);
                writer.Write(',');

                // 1: left_hand
                writer.Write(frame.leftHandPosition[2].ToString("F3", CultureInfo.InvariantCulture));
                writer.Write(',');
                writer.Write(frame.leftHandPosition[0].ToString("F3", CultureInfo.InvariantCulture));
                writer.Write(',');
                writer.Write(frame.leftHandPosition[1].ToString("F3", CultureInfo.InvariantCulture));
                writer.Write(',');

                // 2: left_q
                writer.Write(frame.leftSwordRotation[2].ToString("F3", CultureInfo.InvariantCulture));
                writer.Write(',');
                writer.Write(frame.leftSwordRotation[0].ToString("F3", CultureInfo.InvariantCulture));
                writer.Write(',');
                writer.Write(frame.leftSwordRotation[1].ToString("F3", CultureInfo.InvariantCulture));
                writer.Write(',');
                writer.Write(frame.leftSwordRotation[3].ToString("F3", CultureInfo.InvariantCulture));
                writer.Write(',');

                // 3: left_euler
                writer.Write(frame.leftSwordEuler[2].ToString("F1", CultureInfo.InvariantCulture));
                writer.Write(',');
                writer.Write(frame.leftSwordEuler[0].ToString("F1", CultureInfo.InvariantCulture));
                writer.Write(',');
                writer.Write(frame.leftSwordEuler[1].ToString("F1", CultureInfo.InvariantCulture));
                writer.Write(',');

                // 4: left_ypr
                writer.Write(frame.leftSwordYPR.roll.ToString("F3", CultureInfo.InvariantCulture));
                writer.Write(',');
                writer.Write(frame.leftSwordYPR.pitch.ToString("F3", CultureInfo.InvariantCulture));
                writer.Write(',');
                writer.Write(frame.leftSwordYPR.yaw.ToString("F3", CultureInfo.InvariantCulture));
                writer.Write(',');

                // 5: right_hand
                writer.Write(frame.rightHandPosition[2].ToString("F3", CultureInfo.InvariantCulture));
                writer.Write(',');
                writer.Write(frame.rightHandPosition[0].ToString("F3", CultureInfo.InvariantCulture));
                writer.Write(',');
                writer.Write(frame.rightHandPosition[1].ToString("F3", CultureInfo.InvariantCulture));
                writer.Write(',');

                // 6: right_q
                writer.Write(frame.rightSwordRotation[2].ToString("F3", CultureInfo.InvariantCulture));
                writer.Write(',');
                writer.Write(frame.rightSwordRotation[0].ToString("F3", CultureInfo.InvariantCulture));
                writer.Write(',');
                writer.Write(frame.rightSwordRotation[1].ToString("F3", CultureInfo.InvariantCulture));
                writer.Write(',');
                writer.Write(frame.rightSwordRotation[3].ToString("F3", CultureInfo.InvariantCulture));
                writer.Write(',');

                // 7: right_euler
                writer.Write(frame.rightSwordEuler[2].ToString("F1", CultureInfo.InvariantCulture));
                writer.Write(',');
                writer.Write(frame.rightSwordEuler[0].ToString("F1", CultureInfo.InvariantCulture));
                writer.Write(',');
                writer.Write(frame.rightSwordEuler[1].ToString("F1", CultureInfo.InvariantCulture));
                writer.Write(',');

                // 8: right_ypr
                writer.Write(frame.rightSwordYPR.roll.ToString("F3", CultureInfo.InvariantCulture));
                writer.Write(',');
                writer.Write(frame.rightSwordYPR.pitch.ToString("F3", CultureInfo.InvariantCulture));
                writer.Write(',');
                writer.Write(frame.rightSwordYPR.yaw.ToString("F3", CultureInfo.InvariantCulture));
                writer.Write(',');

                // 9: left_sword
                writer.Write(frame.leftSwordPosition[2].ToString("F3", CultureInfo.InvariantCulture));
                writer.Write(',');
                writer.Write(frame.leftSwordPosition[0].ToString("F3", CultureInfo.InvariantCulture));
                writer.Write(',');
                writer.Write(frame.leftSwordPosition[1].ToString("F3", CultureInfo.InvariantCulture));
                writer.Write(',');

                // 10: right_sword
                writer.Write(frame.rightSwordPosition[2].ToString("F3", CultureInfo.InvariantCulture));
                writer.Write(',');
                writer.Write(frame.rightSwordPosition[0].ToString("F3", CultureInfo.InvariantCulture));
                writer.Write(',');
                writer.Write(frame.rightSwordPosition[1].ToString("F3", CultureInfo.InvariantCulture));
                writer.Write(',');

                // 11: head
                writer.Write(frame.headPosition[2].ToString("F3", CultureInfo.InvariantCulture));
                writer.Write(',');
                writer.Write(frame.headPosition[0].ToString("F3", CultureInfo.InvariantCulture));
                writer.Write(',');
                writer.Write(frame.headPosition[1].ToString("F3", CultureInfo.InvariantCulture));
                writer.Write(',');

                // 12: head_q
                writer.Write(frame.headRotation[2].ToString("F3", CultureInfo.InvariantCulture));
                writer.Write(',');
                writer.Write(frame.headRotation[0].ToString("F3", CultureInfo.InvariantCulture));
                writer.Write(',');
                writer.Write(frame.headRotation[1].ToString("F3", CultureInfo.InvariantCulture));
                writer.Write(',');
                writer.Write(frame.headRotation[3].ToString("F3", CultureInfo.InvariantCulture));
                writer.Write(',');

                // 13: head_euler
                writer.Write(frame.headEuler[2].ToString("F1", CultureInfo.InvariantCulture));
                writer.Write(',');
                writer.Write(frame.headEuler[0].ToString("F1", CultureInfo.InvariantCulture));
                writer.Write(',');
                writer.Write(frame.headEuler[1].ToString("F1", CultureInfo.InvariantCulture));
                writer.Write(',');

                // 14: head_ypr
                writer.Write(frame.headYPR.roll.ToString("F3", CultureInfo.InvariantCulture));
                writer.Write(',');
                writer.Write(frame.headYPR.pitch.ToString("F3", CultureInfo.InvariantCulture));
                writer.Write(',');
                writer.Write(frame.headYPR.yaw.ToString("F3", CultureInfo.InvariantCulture));
                writer.Write(',');

                // 15: left_heading_vector
                writer.Write(frame.leftSwordHeading[2].ToString("F3", CultureInfo.InvariantCulture));
                writer.Write(',');
                writer.Write(frame.leftSwordHeading[0].ToString("F3", CultureInfo.InvariantCulture));
                writer.Write(',');
                writer.Write(frame.leftSwordHeading[1].ToString("F3", CultureInfo.InvariantCulture));
                writer.Write(',');

                // 16: right_heading_vector
                writer.Write(frame.rightSwordHeading[2].ToString("F3", CultureInfo.InvariantCulture));
                writer.Write(',');
                writer.Write(frame.rightSwordHeading[0].ToString("F3", CultureInfo.InvariantCulture));
                writer.Write(',');
                writer.Write(frame.rightSwordHeading[1].ToString("F3", CultureInfo.InvariantCulture));
                writer.Write(',');

                // 17: head_heading_vector
                writer.Write(frame.headHeading[2].ToString("F3", CultureInfo.InvariantCulture));
                writer.Write(',');
                writer.Write(frame.headHeading[0].ToString("F3", CultureInfo.InvariantCulture));
                writer.Write(',');
                writer.Write(frame.headHeading[1].ToString("F3", CultureInfo.InvariantCulture));
                writer.Write(',');

                // 18: left_blade_vector
                writer.Write(frame.leftBlade[2].ToString("F3", CultureInfo.InvariantCulture));
                writer.Write(',');
                writer.Write(frame.leftBlade[0].ToString("F3", CultureInfo.InvariantCulture));
                writer.Write(',');
                writer.Write(frame.leftBlade[1].ToString("F3", CultureInfo.InvariantCulture));
                writer.Write(',');

                // 19: right_blade_vector
                writer.Write(frame.rightBlade[2].ToString("F3", CultureInfo.InvariantCulture));
                writer.Write(',');
                writer.Write(frame.rightBlade[0].ToString("F3", CultureInfo.InvariantCulture));
                writer.Write(',');
                writer.Write(frame.rightBlade[1].ToString("F3", CultureInfo.InvariantCulture));
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
            // time = DateTime.UtcNow()
            // time = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
            time = (long)(Time.fixedTime * 1000),
            leftSwordPosition = leftSword.position,
            leftSwordRotation = leftSword.rotation,
            rightSwordPosition = rightSword.position,
            rightSwordRotation = rightSword.rotation,
            headPosition = headSet.position,
            headRotation = headSet.rotation,
            headYPR = GetYPRAngles(headSet.rotation.normalized),
            headEuler = headSet.eulerAngles,
            leftHandPosition = leftHand.position,
            leftSwordYPR = GetYPRAngles(leftSword.rotation.normalized),
            leftSwordEuler = leftSword.eulerAngles,
            rightHandPosition = rightHand.position,
            rightSwordYPR = GetYPRAngles(rightSword.rotation.normalized),
            rightSwordEuler = rightSword.eulerAngles,
            leftSwordLocal = leftSword.rotation * (leftSword.position - leftHand.position),
            rightSwordLocal = rightSword.rotation * (rightSword.position - rightHand.position),
            leftSwordHeading = leftSword.rotation * Vector3.up,
            rightSwordHeading = rightSword.rotation * Vector3.up,
            headHeading = headSet.rotation * Vector3.forward,
            leftBlade = leftSword.rotation * Vector3.back,
            rightBlade = rightSword.rotation * Vector3.back,
        });

        leftTrail.positionCount++;
        rightTrail.positionCount++;
        leftTrail.SetPosition(leftTrail.positionCount-1, leftSword.position);
        rightTrail.SetPosition(rightTrail.positionCount-1, rightSword.position);
    }

    struct YPRAngles
    {
        public float roll;
        public float pitch;
        public float yaw;
    }

    struct Frame
    {
        public Vector3 leftSwordPosition;
        public Vector3 rightSwordPosition;
        public Quaternion leftSwordRotation;
        public Quaternion rightSwordRotation;
        public Vector3 headPosition;
        public Quaternion headRotation;
        public YPRAngles headYPR;
        public Vector3 headEuler;
        public Vector3 leftHandPosition;
        public Vector3 rightHandPosition;
        public Vector3 leftSwordEuler;
        public Vector3 rightSwordEuler;
        public YPRAngles leftSwordYPR;
        public YPRAngles rightSwordYPR;
        public long time;
        public Vector3 leftSwordLocal;
        public Vector3 rightSwordLocal;
        public Vector3 leftSwordHeading;
        public Vector3 rightSwordHeading;
        public Vector3 headHeading;
        public Vector3 leftBlade;
        public Vector3 rightBlade;
    }
}
