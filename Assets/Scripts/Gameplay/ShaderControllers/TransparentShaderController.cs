using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Renderer))]
public class TransparentShaderController : MonoBehaviour
{
    [Header("Base")]
    [SerializeField] private Color baseColor = new Color(1f, 1f, 1f, 0.2f);

    [Header("Fresnel")]
    [SerializeField] private Color fresnelColor = new Color(0.5f, 0.8f, 1f, 1f);
    [SerializeField, Range(0.1f, 8f)] private float fresnelPower = 4f;

    private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
    private static readonly int FresnelColorID = Shader.PropertyToID("_FresnelColor");
    private static readonly int FresnelPowerID = Shader.PropertyToID("_FresnelPower");

    private Material _material;

    private void OnEnable()
    {
        ApplyAll();
    }

    private void OnValidate()
    {
        ApplyAll();
    }

    private void ApplyAll()
    {
        if (_material == null)
        {
            var renderer = GetComponent<Renderer>();
            if (renderer == null) return;

#if UNITY_EDITOR
            _material = Application.isPlaying ? renderer.material : renderer.sharedMaterial;
#else
            _material = renderer.material;
#endif
        }

        if (_material == null) return;

        _material.SetColor(BaseColorID, baseColor);
        _material.SetColor(FresnelColorID, fresnelColor);
        _material.SetFloat(FresnelPowerID, fresnelPower);
    }

    #region API

    public void SetBaseColor(Color color)
    {
        baseColor = color;
        _material?.SetColor(BaseColorID, color);
    }

    public void SetFresnel(Color color, float power)
    {
        fresnelColor = color;
        fresnelPower = power;
        _material?.SetColor(FresnelColorID, color);
        _material?.SetFloat(FresnelPowerID, power);
    }

    public Color GetBaseColor() => baseColor;
    public Color GetFresnelColor() => fresnelColor;
    public float GetFresnelPower() => fresnelPower;

    #endregion
}