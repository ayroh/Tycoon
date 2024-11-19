using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Navigator
{
    private int wallLayerMask;

    public Navigator()
    {
        wallLayerMask = LayerMask.GetMask("Wall");
    }

    public void SetPatrolPath(List<Vector3> patrolPath, Vector3 startPoint, Vector3 targetPoint)
    {
        patrolPath.Clear();

        Vector3 direction = targetPoint - startPoint;
        Ray ray = new Ray(startPoint, direction.normalized);

        int count = 0;
        while (Physics.Raycast(ray, out RaycastHit hit, direction.magnitude, wallLayerMask) && ++count != 10)
        {
            if (hit.collider == null)
                break;

            WallInteraction wall = hit.collider.GetComponent<WallInteraction>();
            //Vector3 text = startPoint;
            startPoint = Vector3.Distance(wall.LeftPoint, targetPoint) < Vector3.Distance(wall.RightPoint, targetPoint) ? wall.LeftPoint : wall.RightPoint;
            patrolPath.Add(Extentions.Vector3ZeroY(startPoint));

            //Debug.DrawLine(text, startPoint, Color.red, 5f);
            direction = targetPoint - startPoint;
            ray.origin = startPoint;
            ray.direction = direction.normalized;

        }
        if(count == 10)
            Debug.Log("Count: " + count);

        patrolPath.Add(targetPoint);
        patrolPath.Reverse();
        //Debug.DrawLine(targetPoint, startPoint, Color.red, 5f);
    }

}
