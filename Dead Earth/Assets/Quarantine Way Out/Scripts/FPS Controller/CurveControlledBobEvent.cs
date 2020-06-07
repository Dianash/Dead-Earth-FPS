using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void CurveControlledBobCallback();
[System.Serializable]
public class CurveControlledBobEvent
{
    public float time = 0.0f;
    public CurveControlledBobCallback function = null;
    public CurveControlledBobCallbackType type = CurveControlledBobCallbackType.Vertical;
}
