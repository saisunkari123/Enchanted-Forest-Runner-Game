using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sylvan Rush - Universal Material & Shader Helper
/// Guarantees 100% compatibility with Unity 6 (6000.3.1f) across both:
/// 1. Universal Render Pipeline (URP - Unity 6 default)
/// 2. Built-in Render Pipeline (Standard/Diffuse)
/// Eliminates the dreaded "pink/magenta" missing shader bug!
/// </summary>
public static class SylvanMaterialHelper
{
    private static Shader cachedLitShader;
    private static Shader cachedUnlitShader;
    private static bool shaderResolved = false;

    private static void ResolveShaders()
    {
        if (shaderResolved && cachedLitShader != null) return;

        // 1. Try Universal Render Pipeline Shaders (Unity 6 default)
        cachedLitShader = Shader.Find("Universal Render Pipeline/Lit");
        if (cachedLitShader == null) cachedLitShader = Shader.Find("Universal Render Pipeline/Simple Lit");
        if (cachedLitShader == null) cachedLitShader = Shader.Find("Universal Render Pipeline/Unlit");

        // 2. Try Built-in Render Pipeline Shaders
        if (cachedLitShader == null) cachedLitShader = Shader.Find("Standard");
        if (cachedLitShader == null) cachedLitShader = Shader.Find("Legacy Shaders/Diffuse");
        if (cachedLitShader == null) cachedLitShader = Shader.Find("Diffuse");

        // 3. Ultimate Fallback
        if (cachedLitShader == null) cachedLitShader = Shader.Find("Sprites/Default");

        // Unlit Shader for glowing particles and runes
        cachedUnlitShader = Shader.Find("Universal Render Pipeline/Unlit");
        if (cachedUnlitShader == null) cachedUnlitShader = Shader.Find("Unlit/Color");
        if (cachedUnlitShader == null) cachedUnlitShader = Shader.Find("Sprites/Default");
        if (cachedUnlitShader == null) cachedUnlitShader = cachedLitShader;

        shaderResolved = true;
    }

    /// <summary>
    /// Creates a vibrant lit or emissive material compatible with both URP and Built-in pipelines.
    /// </summary>
    public static Material CreateMaterial(Color baseColor, Color emissionColor = default, float smoothness = 0.4f, float metallic = 0.0f)
    {
        ResolveShaders();

        Material mat = new Material(cachedLitShader);
        mat.color = baseColor;

        // Support both Built-in (_Color) and URP (_BaseColor) property names
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", baseColor);
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", baseColor);

        // Smoothness & Metallic
        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
        if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", smoothness);
        if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);

        // Emission
        if (emissionColor != default && emissionColor != Color.black)
        {
            mat.EnableKeyword("_EMISSION");
            if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", emissionColor);
        }

        return mat;
    }

    /// <summary>
    /// Creates an unlit/additive material ideal for particles, shields, and glowing runes.
    /// </summary>
    public static Material CreateUnlitMaterial(Color color, bool additive = false)
    {
        ResolveShaders();

        Material mat = new Material(cachedUnlitShader);
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);

        if (additive)
        {
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }

        return mat;
    }
}
