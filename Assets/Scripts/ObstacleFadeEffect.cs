using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Renderer))]
public class ObstacleFadeEffect : MonoBehaviour
{
    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 1.5f;
    [SerializeField] private bool affectChildren = true;
    
    private List<Material> materials = new List<Material>();
    private List<Color> originalColors = new List<Color>();
    private float currentAlpha;

    void Start()
    {
        InitializeMaterials();
        StartFade();
    }

    void InitializeMaterials()
    {
        // Очищаем предыдущие материалы
        CleanupMaterials();

        // Получаем все рендереры
        Renderer[] renderers = affectChildren ? 
            GetComponentsInChildren<Renderer>(true) : 
            new Renderer[] { GetComponent<Renderer>() };

        foreach (Renderer rend in renderers)
        {
            if (rend == null) continue;

            // Создаем новые экземпляры материалов
            Material[] newMaterials = new Material[rend.sharedMaterials.Length];
            for (int i = 0; i < rend.sharedMaterials.Length; i++)
            {
                if (rend.sharedMaterials[i] == null) continue;

                newMaterials[i] = new Material(rend.sharedMaterials[i]);
                materials.Add(newMaterials[i]);
                originalColors.Add(newMaterials[i].color);
            }

            rend.materials = newMaterials;
        }
    }

    void StartFade()
    {
        currentAlpha = 0f;
        UpdateMaterialsAlpha();
    }

    public void ResetFade()
    {
        // Переинициализируем материалы на случай изменения
        InitializeMaterials(); 
        StartFade();
    }

    void Update()
    {
        if (materials.Count == 0 || currentAlpha >= 1f) return;
        
        currentAlpha += Time.deltaTime / fadeDuration;
        currentAlpha = Mathf.Clamp01(currentAlpha);
        UpdateMaterialsAlpha();
    }

    void UpdateMaterialsAlpha()
    {
        for (int i = 0; i < materials.Count; i++)
        {
            if (materials[i] != null)
            {
                materials[i].color = new Color(
                    originalColors[i].r,
                    originalColors[i].g,
                    originalColors[i].b,
                    currentAlpha
                );
            }
        }
    }

    void CleanupMaterials()
    {
        foreach (Material mat in materials)
        {
            if (mat != null)
            {
                Destroy(mat);
            }
        }
        materials.Clear();
        originalColors.Clear();
    }

    void OnDestroy()
    {
        CleanupMaterials();
    }

    void OnEnable()
    {
        // Переинициализация при повторной активации
        InitializeMaterials();
    }
}