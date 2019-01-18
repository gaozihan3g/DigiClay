using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HTC.UnityPlugin.Vive;

public class HapticManager : MonoBehaviour {

    public enum HapticModel
    {
        None,
        Basic,
        Advanced,
        Debug
    }

    public enum HapticPattern
    {
        None,
        Sine,
        Sqaure,
        Sawtooth
    }

    [SerializeField]
    HapticModel hapticModel = HapticModel.None;
    [SerializeField]
    HapticPattern hapticPattern = HapticPattern.None;

    public bool friction;
    public bool reaction;

    public bool[] Flag = new bool[2];
    public ushort[] hapticValue = new ushort[2];
    [Range(1, 2000)]
    public ushort min;
    [Range(1, 2000)]
    public ushort max;
    [Range(1, 2000)]
    public ushort mu;
    public float stableThreshold;
    public float c;
    public VivePoseTracker[] vivePoseTrackers = new VivePoseTracker[2];
    public float[] triggerValue = new float[2];

    [Range(1f, 100f)]
    public float frequency;
    float period;
    public float[] timeCounter = new float[2];
    public float patternFrequency = 1f;


    Vector3[] p0 = new Vector3[2];
    Vector3[] p1 = new Vector3[2];
    Vector3[] p2 = new Vector3[2];

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
        period = 1f / frequency;

        switch (hapticModel)
        {
            case HapticModel.None:
                friction = false;
                reaction = false;
                Flag[0] = Flag[1] = false;
                break;
            case HapticModel.Basic:
                friction = true;
                reaction = false;
                Flag[0] = Flag[1] = false;
                break;
            case HapticModel.Advanced:
                friction = true;
                reaction = true;
                Flag[0] = Flag[1] = false;
                break;
            case HapticModel.Debug:
                friction = true;
                reaction = true;
                Flag[0] = Flag[1] = true;
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
            p2[i] = p1[i];
            p1[i] = p0[i];
            p0[i] = vivePoseTrackers[i].transform.position;

            if (!Flag[i])
                continue;

            // speed
            //float dist = (p0[i] - p1[i]).magnitude * c;
            float dist = (p0[i] - 2 * p1[i] + p2[i]).magnitude * c;

            if (dist < stableThreshold)
                dist = 0f;

            delta[i] = Mathf.Clamp01(dist);
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
                float v = min + t * (max - min);

                switch (hapticPattern)
                {
                    case HapticPattern.Sine:
                        v = v * Sine(patternFrequency);
                        break;
                    case HapticPattern.Sqaure:
                        v = v * Square(patternFrequency);
                        break;
                    case HapticPattern.Sawtooth:
                        v = v * Sawtooth(patternFrequency);
                        break;
                    default:
                        break;
                }

                hapticValue[i] += (ushort)v;
                //T[i] = minT + (int)(t * (maxT - minT));
            }

            if (friction)
            {
                hapticValue[i] += (ushort)(delta[i] * mu);
            }

            //if (counter[i] == 0)
            if (timeCounter[i] > period)
            {
                ViveInput.TriggerHapticPulse((HandRole)i, hapticValue[i]);
                timeCounter[i] = 0f;
            }

            timeCounter[i] += Time.deltaTime;

            //counter[i] = (counter[i] + 1) % T[i];
            //Debug.Log("Test: " + Time.deltaTime + "Counter " + i + ": " + counter[i] );
        }
    }

    public void TriggerHaptic(HandRole role, ushort durationMicroSec = 500)
    {
        if (hapticModel == HapticModel.None)
            return;

        ViveInput.TriggerHapticPulse(role, durationMicroSec);
    }

    float Sine(float f = 2 * Mathf.PI)
    {
        return 0.5f * Mathf.Sin(2f * Mathf.PI * f * Time.time) + 0.5f;
    }

    float Square(float f = 1)
    {
        float lambda = 1f / f;
        return (Time.time % lambda) > lambda * 0.5f ? 0f: 1f;
    }

    float Sawtooth(float f = 1)
    {
        return 1f / Mathf.PI * Mathf.Atan(Mathf.Tan(Mathf.PI * f * Time.time)) + 0.5f;
    }


    // haptic (role, intensity, frequency)
}
