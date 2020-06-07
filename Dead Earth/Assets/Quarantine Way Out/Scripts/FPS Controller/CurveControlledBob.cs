using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CurveControlledBob
{
    [SerializeField]
    AnimationCurve bobCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, 1f), new Keyframe(1f, 0f),
                                                 new Keyframe(1.5f, -1f), new Keyframe(2f, 0f));

    [SerializeField] float horizontalMultiplier = 0.01f;
    [SerializeField] float verticalMultiplier = 0.02f;
    [SerializeField] float verticalToHorizontalSpeedRatio = 2.0f;
    [SerializeField] float baseInterval = 1.0f;

    private float prevXPlayHead;
    private float prevYPlayHead;

    private float xPlayHead;
    private float yPlayHead;
    private float curveEndTime;
    private List<CurveControlledBobEvent> events = new List<CurveControlledBobEvent>();

    public void Initialize()
    {
        curveEndTime = bobCurve[bobCurve.length - 1].time;
        xPlayHead = 0.0f;
        yPlayHead = 0.0f;
    }

    public void RegisterEventCallback(float time, CurveControlledBobCallback function, CurveControlledBobCallbackType type)
    {
        CurveControlledBobEvent curveControlledBobEvent = new CurveControlledBobEvent();
        curveControlledBobEvent.time = time;
        curveControlledBobEvent.function = function;
        curveControlledBobEvent.type = type;
        events.Add(curveControlledBobEvent);
        events.Sort((CurveControlledBobEvent t1, CurveControlledBobEvent t2) => t1.time.CompareTo(t2.time));

    }

    public Vector3 GetVectorOffset(float speed)
    {
        xPlayHead += (speed * Time.deltaTime) / baseInterval;
        yPlayHead += ((speed * Time.deltaTime) / baseInterval) * verticalToHorizontalSpeedRatio;

        if (xPlayHead > curveEndTime)
            xPlayHead -= curveEndTime;

        if (yPlayHead > curveEndTime)
            yPlayHead -= curveEndTime;

        for (int i = 0; i < events.Count; i++)
        {
            CurveControlledBobEvent ev = events[i];
            if (ev != null)
            {
                if (ev.type == CurveControlledBobCallbackType.Vertical)
                {
                    if ((prevYPlayHead < ev.time && yPlayHead >= ev.time) || (prevYPlayHead > yPlayHead && (ev.time > prevYPlayHead || ev.time <= yPlayHead)))
                    {
                        ev.function();
                    }
                }
                else if ((prevXPlayHead < ev.time && xPlayHead >= ev.time) || (prevXPlayHead > xPlayHead && (ev.time > prevXPlayHead || ev.time <= xPlayHead)))
                {
                    ev.function();
                }
            }

        }

        float xPos = bobCurve.Evaluate(xPlayHead) * horizontalMultiplier;
        float yPos = bobCurve.Evaluate(yPlayHead) * verticalMultiplier;

        prevXPlayHead = xPlayHead;
        prevYPlayHead = yPlayHead;

        return new Vector3(xPos, yPos, 0f);
    }
}
