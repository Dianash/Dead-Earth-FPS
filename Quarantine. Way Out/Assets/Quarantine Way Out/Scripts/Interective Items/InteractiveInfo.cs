using UnityEngine;

public class InteractiveInfo : InteractiveItem
{
    [SerializeField] private string infoText;

    public override string GetText()
    {
        return infoText;
    }
}
