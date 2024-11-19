using UnityEngine;

public class WallInteraction : MonoBehaviour
{
    [SerializeField] private Exhibition exhibition;
    [SerializeField] private BoxCollider boxCollider;
    private Vector3 _leftPoint, _rightPoint;

    public Vector3 LeftPoint => _leftPoint;
    public Vector3 RightPoint => _rightPoint;
    public Exhibition Exhibition => exhibition;


    private void Awake()
    {
        Vector3 halfSize = new Vector3(boxCollider.size.x * transform.localScale.x, boxCollider.size.y * transform.localScale.y, boxCollider.size.z * transform.localScale.z) / 2;
       
        Vector3 sizeVector = halfSize;
        sizeVector.y = 0f;
        sizeVector.x = -sizeVector.x;
        sizeVector = transform.rotation * sizeVector;
        _leftPoint = boxCollider.bounds.center + sizeVector + Quaternion.Euler(0f, -45f, 0f) * transform.forward;

        sizeVector = halfSize;
        sizeVector.y = 0f;
        sizeVector = transform.rotation * sizeVector;
        _rightPoint = boxCollider.bounds.center + sizeVector + Quaternion.Euler(0f, 45f, 0f) * transform.forward;
    }


}
