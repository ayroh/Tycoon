using UnityEngine;

public class OrientedBounds
{
    public Vector3 center;
    public Vector3 size;
    public Quaternion rotation;
    private Vector3 halfSize;

    public OrientedBounds(Vector3 center, Vector3 size, Quaternion rotation)
    {
        this.center = center;
        this.size = size;
        this.rotation = rotation;
        halfSize = size / 2;
    }

    public bool Contains(Vector3 point)
    {
        Vector3 localPoint = Quaternion.Inverse(rotation) * (point - center);
        return Mathf.Abs(localPoint.x) <= halfSize.x && Mathf.Abs(localPoint.y) <= halfSize.y && Mathf.Abs(localPoint.z) <= halfSize.z;
    }
}
