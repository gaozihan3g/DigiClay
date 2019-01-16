using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HTC.UnityPlugin.Vive;

public class HapticTest : MonoBehaviour
{
    public bool friction;
    public bool reaction;
    public ushort[] hapticValue = new ushort[2];
    public ushort min;
    public ushort max;
    public ushort mu;
    public float interval;
    public float stableThreshold;
    public float c;
    public float[] triggerValue;
    public VivePoseTracker[] vivePoseTrackers;

    //current
    public Vector3[] p0 = new Vector3[2];
    // 1 frame before
    public Vector3[] p1 = new Vector3[2];
    // 2 frames before
    public Vector3[] p2 = new Vector3[2];

    public float[] delta = new float[2];



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateControllerPosition();
        UpdateTriggerValue();
        UpdateHaptic();
    }

    void UpdateTriggerValue()
    {
        triggerValue[0] = ViveInput.GetTriggerValue(HandRole.LeftHand);
        triggerValue[1] = ViveInput.GetTriggerValue(HandRole.RightHand);
    }

    void UpdateControllerPosition()
    {

        for (int i = 0; i < 2; ++i)
        {
            p2[i] = p1[i];
            p1[i] = p0[i];
            p0[i] = vivePoseTrackers[i].transform.position;

            // speed
            float dist = (p0[i] - p1[i]).magnitude * c;

            if (dist < stableThreshold)
                dist = 0f;

            delta[i] = dist;
        }
    }

    void UpdateHaptic()
    {
        // init
        for (int i = 0; i < 2; ++i)
            hapticValue[i] = 0;

        if (reaction)
        {
            for (int i = 0; i < 2; ++i)
                hapticValue[i] += (ushort)(min + triggerValue[i] * (max - min));
        }

        if (friction)
        {
            for (int i = 0; i < 2; ++i)
                hapticValue[i] += (ushort)(delta[i] * mu);
        }

        ViveInput.TriggerHapticPulse(HandRole.LeftHand, hapticValue[0]);
        ViveInput.TriggerHapticPulse(HandRole.RightHand, hapticValue[1]);
    }

}
