using UnityEngine;

[System.Serializable]
public class CurveControlledBob
{
    [SerializeField]
    AnimationCurve bobCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, 1f), new Keyframe(1f, 0f),
                                                 new Keyframe(1.5f, -1f), new Keyframe(2f, 0f));

    [SerializeField] float horizontalMultiplier = 0.01f;
    [SerializeField] float verticalMultiplier = 0.02f;
    [SerializeField] float verticalToHorizontalSpeedRation = 2.0f;

    private float xPlayHead;
    private float yPlayHead;
    private float baseInterval;
    private float curveEndTime;

    public void Initialize(float bobBaseInterval)
    {
        baseInterval = bobBaseInterval;
        curveEndTime = bobCurve[bobCurve.length - 1].time;
    }

    public Vector3 GetVectorOffset(float speed)
    {
        xPlayHead += (speed * Time.deltaTime) / baseInterval;
        yPlayHead += ((speed * Time.deltaTime) / baseInterval) * verticalToHorizontalSpeedRation;

        float xPos = bobCurve.Evaluate(xPlayHead) * horizontalMultiplier;
        float yPos = bobCurve.Evaluate(xPlayHead) * verticalMultiplier;

        return new Vector3(xPos, yPos, 0f);
    }
}
