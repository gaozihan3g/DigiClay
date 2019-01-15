using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HTC.UnityPlugin.Vive;

public class HapticManager : MonoBehaviour {

    public enum HapticModel
    {
        No,
        Basic,
        Advanced
    }

    [SerializeField]
    HapticModel model = HapticModel.No;

	[SerializeField]
	ushort[] m_duration = new ushort[2];
	[SerializeField]
	float[] m_strength = new float[2];

    [SerializeField]
    [Range(0f, 1f)]
    float interval;

	public static HapticManager Instance;
	Dictionary<HandRole, IEnumerator> coroutineDic = new Dictionary<HandRole, IEnumerator>();

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

	public void SetRoleStrength(HandRole role, float str = 0f)
	{
        if (model == HapticModel.No)
            return;

        //get trigger value
        float t = ViveInput.GetTriggerValue(role);
        ushort r = (ushort)(t * DigiClayConstant.HAPTIC_MU);

		int i = (int)role;
		str = Mathf.Clamp01 (str);
		m_strength[i] = str;

        ushort f = (ushort)(DigiClayConstant.MIN_HAPTIC + (DigiClayConstant.MAX_HAPTIC - DigiClayConstant.MIN_HAPTIC) * str);

        m_duration[i] = (ushort)(r + f);
	}
		
	public void StartHaptic(HandRole role, float str = 0f)
	{
        if (model == HapticModel.No)
            return;

        SetRoleStrength (role, str);

		if (!coroutineDic.ContainsKey (role)) {
			IEnumerator c = HapticSequence (role);
			coroutineDic.Add (role, c);
			StartCoroutine (c);
		}
	}

	public void EndHaptic(HandRole role)
	{
        if (model == HapticModel.No)
            return;

        if (!coroutineDic.ContainsKey (role))
			return;
		
		IEnumerator c = coroutineDic [role];
		StopCoroutine (c);
		coroutineDic.Remove (role);
	}

	IEnumerator HapticSequence(HandRole role)
	{
		int i = (int)role;
		while (true)
		{
			ViveInput.TriggerHapticPulse(role, m_duration[i]);
			yield return new WaitForSecondsRealtime(interval);
		}
	}

    public void TriggerHaptic(HandRole role)
    {
        if (model == HapticModel.No)
            return;

        ViveInput.TriggerHapticPulse(role);
    }
}
