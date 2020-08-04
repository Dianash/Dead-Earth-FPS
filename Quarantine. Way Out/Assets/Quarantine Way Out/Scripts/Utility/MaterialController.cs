using UnityEngine;

[System.Serializable]
public class MaterialController
{
    [SerializeField] protected Material material = null;

    [SerializeField] protected Texture diffuseTexture = null;
    [SerializeField] protected Color diffuseColor = Color.white;
    [SerializeField] protected Texture normalMap = null;
    [SerializeField] protected float normalStrength = 1.0f;

    [SerializeField] protected Texture emissiveTexture = null;
    [SerializeField] protected Color emissionColor = Color.black;
    [SerializeField] protected float emissionScale = 1.0f;

    protected MaterialController backup = null;
    protected bool started = false;

    public Material Material { get => material; }

    public void OnStart()
    {
        if (material == null || started)
            return;

        started = true;
        backup = new MaterialController();

        // Backup settings in a temp controller
        backup.diffuseColor = material.GetColor("_Color");
        backup.diffuseTexture = material.GetTexture("_MainTex");
        backup.emissionColor = material.GetColor("_EmissionColor");
        backup.emissionScale = 1;
        backup.emissiveTexture = material.GetTexture("_EmissionMap");
        backup.normalMap = material.GetTexture("_BumpMap");
        backup.normalStrength = material.GetFloat("_BumpScale");

        // Register this controller with the game scene manager using material instance ID. The GameScene manager will reset
        // all registered materials when the scene closes
        if (GameSceneManager.Instance)
            GameSceneManager.Instance.RegisterMaterialController(material.GetInstanceID(), this);
    }

    public void Activate(bool activate)
    {
        // Can't call this function until it's start has been called
        if (!started || material == null)
            return;

        // Set the material to the assigned properties
        if (activate)
        {
            material.SetColor("_Color", diffuseColor);
            material.SetTexture("_MainTex", diffuseTexture);
            material.SetColor("_EmissionColor", emissionColor * emissionScale);
            material.SetTexture("_EmissionMap", emissiveTexture);
            material.SetTexture("_BumpMap", normalMap);
            material.SetFloat("_BumpScale", normalStrength);
        }
        else
        {
            material.SetColor("_Color", backup.diffuseColor);
            material.SetTexture("_MainTex", backup.diffuseTexture);
            material.SetColor("_EmissionColor", backup.emissionColor * backup.emissionScale);
            material.SetTexture("_EmissionMap", backup.emissiveTexture);
            material.SetTexture("_BumpMap", backup.normalMap);
            material.SetFloat("_BumpScale", backup.normalStrength);
        }
    }

    /// <summary>
    /// Resets the material.
    /// </summary>
    public void OnReset()
    {
        if (backup == null || material == null) 
            return;

        material.SetColor("_Color", backup.diffuseColor);
        material.SetTexture("_MainTex", backup.diffuseTexture);
        material.SetColor("_EmissionColor", backup.emissionColor * backup.emissionScale);
        material.SetTexture("_EmissionMap", backup.emissiveTexture);
        material.SetTexture("_BumpMap", backup.normalMap);
        material.SetFloat("_BumpScale", backup.normalStrength);
    }

    /// <summary>
    /// Returns the instance ID of the managed material
    /// </summary>
    public int GetInstanceID()
    {
        if (material == null)
            return -1;

        return material.GetInstanceID();
    }
}
