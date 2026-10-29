using System;
using UnityEngine;

public enum NoiseKind
{
    Footstep,
    Gunshot,
    Impact,
    Distraction
}

public readonly struct NoiseEventData
{
    public NoiseEventData(
        Vector3 position,
        float radius,
        GameObject source,
        NoiseKind kind
    )
    {
        Position = position;
        Radius = radius;
        Source = source;
        Kind = kind;
    }

    public Vector3 Position { get; }
    public float Radius { get; }
    public GameObject Source { get; }
    public NoiseKind Kind { get; }
}

public static class NoiseSystem
{
    public static event Action<NoiseEventData> NoiseEmitted;

    public static void Emit(
        Vector3 position,
        float radius,
        GameObject source,
        NoiseKind kind
    )
    {
        if (radius <= 0f || !float.IsFinite(radius))
        {
            return;
        }

        NoiseEmitted?.Invoke(
            new NoiseEventData(position, radius, source, kind)
        );
    }
}
