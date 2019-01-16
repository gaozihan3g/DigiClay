using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HTC.UnityPlugin.Vive;

public class HapticManager : MonoBehaviour {

    public enum HapticModel
    {
        None,
        Basic,
        Advanced
    }

    [SerializeField]
    HapticModel hapticModel = HapticModel.None;
    public bool friction;
    public bool reaction;
    public bool[] Flag = new bool[2];
    public ushort[] hapticValue = new ushort[2];
    public ushort min;
    public ushort max;
    public ushort mu;
    public float stableThreshold;
    public float c;
    public VivePoseTracker[] vivePoseTrackers = new VivePoseTracker[2];
    public float[] triggerValue = new float[2];

    //current
    public Vector3[] p0 = new Vector3[2];
    // 1 frame before
    public Vector3[] p1 = new Vector3[2];

    public float[] delta = new float[2];

	public static HapticManager Instance;

	void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Destroy(this);
		}
	}

    void OnValidate()
    {
        switch (hapticModel)
        {
            case HapticModel.None:
                friction = false;
                reaction = false;
                break;
            case HapticModel.Basic:
                friction = true;
                reaction = false;
                break;
            case HapticModel.Advanced:
                friction = true;
                reaction = true;
                break;
        }
    }

    void Update()
    {
        if (hapticModel == HapticModel.None)
            return;

        UpdateControllerPosition();
        UpdateTriggerValue();
        UpdateHaptic();
    }

    void UpdateControllerPosition()
    {
        for (int i = 0; i < 2; ++i)
        {
            if (!Flag[i])
                continue;

            p1[i] = p0[i];
            p0[i] = vivePoseTrackers[i].transform.position;

            // speed
            float dist = (p0[i] - p1[i]).magnitude * c;

            if (dist < stableThreshold)
                dist = 0f;

            delta[i] = dist;
        }
    }

    void UpdateTriggerValue()
    {
        for (int i = 0; i < 2; ++i)
            triggerValue[i] = ViveInput.GetTriggerValue((HandRole)i);
    }



    void UpdateHaptic()
    {
        for (int i = 0; i < 2; ++i)
        {
            // init
            hapticValue[i] = 0;

            if (!Flag[i])
                continue;

            if (reaction)
            {
                float t = DigiClayHelpers.Map(triggerValue[i], 0.5f, 1f, 0f, 1f, true);
                hapticValue[i] += (ushort)(min + t * (max - min));
            }

            if (friction)
            {
                hapticValue[i] += (ushort)(delta[i] * mu);
            }

            ViveInput.TriggerHapticPulse((HandRole)i, hapticValue[i]);
        }
    }

    public void TriggerHaptic(HandRole role)
    {
        if (hapticModel == HapticModel.None)
            return;

        ViveInput.TriggerHapticPulse(role);
    }
}
