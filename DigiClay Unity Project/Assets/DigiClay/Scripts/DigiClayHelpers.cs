using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DigiClayHelpers
{
    public static float Map(float v, float fmin, float fmax, float tmin, float tmax, bool clamp = false)
    {
        float fd = fmax - fmin;
        float t = (v - fmin) / fd;
        float td = tmax - tmin;
        float r = tmin + t * td;
        if (clamp)
            return Mathf.Clamp(r, tmin, tmax);
        return r;
    }
}
