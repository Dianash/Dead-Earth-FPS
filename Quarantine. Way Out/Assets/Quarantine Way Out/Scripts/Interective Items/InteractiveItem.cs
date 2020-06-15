using UnityEngine;

public class InteractiveItem : MonoBehaviour
{
    [SerializeField] private int priority = 0;

    protected GameSceneManager gameSceneManager = null;
    protected Collider coll = null;

    public int Priority { get => priority; }

    public virtual string GetText()
    {
        return null;
    }

    public virtual void Activate(CharacterManager characterManager) { }

    protected virtual void Start()
    {
        gameSceneManager = GameSceneManager.Instance;
        coll = GetComponent<Collider>();

        if (gameSceneManager != null && coll != null)
        {
            gameSceneManager.RegisterInteractiveItem(coll.GetInstanceID(), this);
        }
    }
}
