using UnityEngine;
using System.Collections.Generic;

public class LipSyncFromClip : MonoBehaviour
{
    public SkinnedMeshRenderer faceRenderer;
    public float maxWeight = 100f;
    public float changeVisemeInterval = 0.2f; // tempo mínimo para trocar visemes
    public float blendSpeed = 8f; // suavidade da transição

    private List<float> volumeCurve;
    private float timer = 0f;
    private float clipLength = 0f;
    private bool isPlaying = false;

    private readonly string[] visemeNames = new string[]
    {
        "EE", "Er", "Ah", "Oh", "W_OO", "S_Z", "Ch_J",
        "F_V", "TH", "T_L_D_N", "B_M_P", "K_G_H_NG", "AE", "R"
    };

    private int[] visemeIndices;
    private float[] currentWeights;
    private float[] targetWeights;

    private float visemeChangeTimer = 0f;
    private int activeViseme1 = -1;
    private int activeViseme2 = -1;

    public void StartLipSync(AudioClip clip)
    {
        if (clip == null || faceRenderer == null) return;

        volumeCurve = ExtractVolumeCurve(clip);
        clipLength = clip.length;
        timer = 0f;
        isPlaying = true;

        visemeIndices = new int[visemeNames.Length];
        currentWeights = new float[visemeNames.Length];
        targetWeights = new float[visemeNames.Length];

        for (int i = 0; i < visemeNames.Length; i++)
        {
            int index = faceRenderer.sharedMesh.GetBlendShapeIndex(visemeNames[i]);
            if (index == -1)
                Debug.LogWarning($"BlendShape '{visemeNames[i]}' não encontrado.");
            visemeIndices[i] = index;
        }
    }

    void Update()
    {
        if (!isPlaying || volumeCurve == null || volumeCurve.Count == 0)
            return;

        timer += Time.deltaTime;
        visemeChangeTimer += Time.deltaTime;

        int index = Mathf.FloorToInt((timer / clipLength) * volumeCurve.Count);
        if (index >= volumeCurve.Count)
        {
            StopLipSync();
            return;
        }

        float rawVolume = volumeCurve[index];
        float volume = Mathf.Clamp01((rawVolume - 0.05f) * 2f); // abertura mínima maior

        if (visemeChangeTimer >= changeVisemeInterval)
        {
            visemeChangeTimer = 0f;

            activeViseme1 = Random.Range(0, visemeIndices.Length);
            activeViseme2 = Random.Range(0, visemeIndices.Length);

            for (int i = 0; i < visemeIndices.Length; i++)
            {
                if (i == activeViseme1)
                    targetWeights[i] = volume * maxWeight;
                else if (i == activeViseme2)
                    targetWeights[i] = volume * maxWeight * 0.5f;
                else
                    targetWeights[i] = 0f;
            }
        }

        // Interpolação suave dos pesos
        for (int i = 0; i < visemeIndices.Length; i++)
        {
            if (visemeIndices[i] == -1) continue;

            currentWeights[i] = Mathf.Lerp(currentWeights[i], targetWeights[i], Time.deltaTime * blendSpeed);
            faceRenderer.SetBlendShapeWeight(visemeIndices[i], currentWeights[i]);
        }
    }

    void StopLipSync()
    {
        isPlaying = false;

        for (int i = 0; i < visemeIndices.Length; i++)
        {
            if (visemeIndices[i] != -1)
                faceRenderer.SetBlendShapeWeight(visemeIndices[i], 0f);
        }
    }

    private List<float> ExtractVolumeCurve(AudioClip clip)
    {
        int sampleCount = clip.samples;
        int channels = clip.channels;
        float[] samples = new float[sampleCount * channels];
        clip.GetData(samples, 0);

        List<float> curve = new List<float>();
        int step = 1024;

        for (int i = 0; i < samples.Length; i += step)
        {
            float sum = 0f;
            for (int j = 0; j < step && i + j < samples.Length; j++)
                sum += samples[i + j] * samples[i + j];

            float rms = Mathf.Sqrt(sum / step);
            curve.Add(rms);
        }

        return curve;
    }
}
