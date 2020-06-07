using UnityEngine;

[CreateAssetMenu(fileName = "Custom Animation Curve")]
public class CustomCurve : ScriptableObject
{
    [SerializeField] AnimationCurve _curve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0));

    public float Evaluate(float time)
    {
        return _curve.Evaluate(time);
    }
}
