using UnityEngine;

public static class Extentions
{

    // %10 noise = .1f noisePercentage
    public static float Noise(float number, float noisePercentage)
    {
        return number + (number * Random.Range(-noisePercentage, noisePercentage));
    }

    public static float RandomWithNegativeChance(float minInclusivePositive, float maxInclusivePositive)
    {
        float number = Random.Range(minInclusivePositive, maxInclusivePositive);
        return (Random.Range(-1f, 1f) > 0) ? number : -number;
    }

    public static Vector2 Vector3ToVector2XZ(Vector3 vector3) => new Vector2(vector3.x, vector3.z);

    public static Vector3 GetRandomPatrolPoint() => new Vector3(Random.Range(-7f, 7f), 0f, Random.Range(-7f, 7f));

    public static bool IsIndexWithinBounds(int index, bool[] arr) => (index < 0 || index >= arr.Length) ? false : true;

}