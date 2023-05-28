using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MotionType { Custom, Linear, EaseIn, EaseOut, EaseInOut, EaseInElastic, EaseOutElastic, EaseInOutElastic, EaseInBounce, EaseOutBounce, EaseInOutBounce }
public static class ZUIEquations {

    #region Motion Functions
    public static float Custom(float t, AnimationCurve curve)
    {
        return curve.Evaluate(t);
    }

    public static float Linear(float t)
    {
        return t;
    }

    public static float EaseIn(float t, int power)
    {
        return Mathf.Pow(t, power);
    }
    public static float EaseOut(float t, int power)
    {
        return 1 - Mathf.Abs(Mathf.Pow(t - 1, power));
    }
    public static float EaseInOut(float t, int power)
    {
        return t < 0.5f ? EaseIn(t * 2, power) / 2 : EaseOut(t * 2 - 1, power) / 2 + 0.5f;
    }

    public static float EaseInElastic(float t, float magnitude = 0.7f)
    {
        if (t == 0 || t == 1)
        {
            return t;
        }

        float scaledTime = t / 1;
        float scaledTime1 = scaledTime - 1;

        float p = 1 - magnitude;
        float s = p / (2 * Mathf.PI) * Mathf.Asin(1);

        return -(
            Mathf.Pow(2, 10 * scaledTime1) *
            Mathf.Sin((scaledTime1 - s) * (2 * Mathf.PI) / p)
        );
    }
    public static float EaseOutElastic(float t, float magnitude = 0.7f)
    {
        float p = 1 - magnitude;
        float scaledTime = t * 1f;

        if (t == 0 || t == 1)
        {
            return t;
        }

        float s = p / (2 * Mathf.PI) * Mathf.Asin(1);
        return (
            Mathf.Pow(2, -10 * scaledTime) *
            Mathf.Sin((scaledTime - s) * (2 * Mathf.PI) / p)
        ) + 1;
    }
    public static float EaseInOutElastic(float t, float magnitude = 0.7f)
    {
        float p = 1 - magnitude;

        if (t == 0 || t == 1)
        {
            return t;
        }

        float scaledTime = t * 2;
        float scaledTime1 = scaledTime - 1;

        float s = p / (2 * Mathf.PI) * Mathf.Asin(1);

        if (scaledTime < 1)
        {
            return -0.5f * (
                Mathf.Pow(2, 10 * scaledTime1) *
                Mathf.Sin((scaledTime1 - s) * (2 * Mathf.PI) / p)
            );
        }

        return (
            Mathf.Pow(2, -10 * scaledTime1) *
            Mathf.Sin((scaledTime1 - s) * (2 * Mathf.PI) / p) * 0.5f
        ) + 1;
    }

    public static float EaseInBounce(float t)
    {
        return 1 - EaseOutBounce(1 - t);
    }
    public static float EaseOutBounce(float t)
    {
        float scaledTime = t / 1;

        if (scaledTime < (1 / 2.75f))
        {

            return 7.5625f * scaledTime * scaledTime;

        }
        else if (scaledTime < (2 / 2.75f))
        {

            float scaledTime2 = scaledTime - (1.5f / 2.75f);
            return (7.5625f * scaledTime2 * scaledTime2) + 0.75f;

        }
        else if (scaledTime < (2.5f / 2.75f))
        {

            float scaledTime2 = scaledTime - (2.25f / 2.75f);
            return (7.5625f * scaledTime2 * scaledTime2) + 0.937f;

        }
        else {

            float scaledTime2 = scaledTime - (2.625f / 2.75f);
            return (7.5625f * scaledTime2 * scaledTime2) + 0.984375f;

        }
    }
    public static float EaseInOutBounce(float t)
    {
        if (t < 0.5)
            return EaseInBounce(t * 2) * 0.5f;
        else
            return (EaseOutBounce((t * 2) - 1) * 0.5f) + 0.5f;
    }
    #endregion

    /// <summary>
    /// Get's the ease float based on the motion type selected.
    /// </summary>
    /// <param name="t">Time (0 to 1).</param>
    /// <param name="type">Motion Type.</param>
    /// <param name="parameters">Desired ease function parameters.</param>
    /// <returns></returns>
    public static float GetEaseFloat(float t, MotionType type, EasingEquationsParameters p)
    {
        float ease = 0;
        switch (type)
        {
            case MotionType.Custom:
                ease = Custom(t, p.Custom.Curve);
                break;
            case MotionType.Linear:
                ease = Linear(t);
                break;
            case MotionType.EaseIn:
                ease = EaseIn(t, p.EaseIn.EasingPower);
                break;
            case MotionType.EaseOut:
                ease = EaseOut(t, p.EaseOut.EasingPower);
                break;
            case MotionType.EaseInOut:
                ease = EaseInOut(t, p.EaseInOut.EasingPower);
                break;
            case MotionType.EaseInElastic:
                ease = EaseInElastic(t, p.EaseInElastic.ElasticityPower);
                break;
            case MotionType.EaseOutElastic:
                ease = EaseOutElastic(t, p.EaseOutElastic.ElasticityPower);
                break;
            case MotionType.EaseInOutElastic:
                ease = EaseInOutElastic(t, p.EaseInOutElastic.ElasticityPower);
                break;
            case MotionType.EaseInBounce:
                ease = EaseInBounce(t);
                break;
            case MotionType.EaseOutBounce:
                ease = EaseOutBounce(t);
                break;
            case MotionType.EaseInOutBounce:
                ease = EaseInOutBounce(t);
                break;
        }
        return ease;
    }
}

[Serializable]
public class EasingEquationsParameters
{
    [Serializable]
    public class CustomParameters
    {
        public AnimationCurve Curve = AnimationCurve.Linear(0,0,1,1);
    }
    [Serializable]
    public class EaseInOutParameters
    {
        [Range(2, 25)]
        public int EasingPower = 2;
    }
    [Serializable]
    public class EaseInOutElasticParameters
    {
        [Range(0.1f, 0.9f)]
        public float ElasticityPower = 0.5f;
    }

    public CustomParameters Custom;
    public EaseInOutParameters EaseIn;
    public EaseInOutParameters EaseOut;
    public EaseInOutParameters EaseInOut;
    public EaseInOutElasticParameters EaseInElastic;
    public EaseInOutElasticParameters EaseOutElastic;
    public EaseInOutElasticParameters EaseInOutElastic;
}