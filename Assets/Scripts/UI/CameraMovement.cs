using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    
    private bool canMove = true;
    private Vector3 startMousePos, startCameraPos, lastMousePos;
    private Vector2 movementConstantRatioWithCamera;
    private IEnumerator driftCoroutine;

    private const float speedConstant = 0.05f;
    private const float maximumSpeed = 4f;
    private const float minimumDriftStartSpeed = .5f;

    private void Awake()
    {
        float pixelByDistance = (Camera.main.orthographicSize * 2) / Screen.height;
        movementConstantRatioWithCamera = new Vector2(pixelByDistance, pixelByDistance / Mathf.Sin(Mathf.Deg2Rad * transform.eulerAngles.x));
    }

    private void Update()
    {
        if (!canMove) return;

        if(Input.GetMouseButtonDown(0))
        {
            if(driftCoroutine != null) StopCoroutine(driftCoroutine);

            startMousePos = Input.mousePosition;
            startCameraPos = transform.position;
        }
        else if(Input.GetMouseButton(0)) 
        {
            Vector3 currentFrameInput = Input.mousePosition;
            float xDiff = startMousePos.x - currentFrameInput.x;
            float yDiff = startMousePos.y - currentFrameInput.y;

            //transform.position = startCameraPos + new Vector3(xDiff * movementConstantRatioWithCamera.x, 0, yDiff * movementConstantRatioWithCamera.y);
            transform.position = Vector3.Lerp(transform.position, startCameraPos + new Vector3(xDiff * movementConstantRatioWithCamera.x, 0, yDiff * movementConstantRatioWithCamera.y), .25f);

            lastMousePos = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            StartCoroutine(driftCoroutine = Drift());
        }

    }

    private IEnumerator Drift()
    {
        Vector3 lastFrameVector = (lastMousePos - Input.mousePosition);
        float speed = lastFrameVector.magnitude * speedConstant;
        speed = Mathf.Clamp(speed, 0f, maximumSpeed);

        if (speed < minimumDriftStartSpeed)
            yield break;

        Vector3 direction = lastFrameVector.normalized;
        direction.z = direction.y;
        direction.y = 0;

        while(speed > .1f)
        {
            speed *= .9f;
            transform.position += speed * direction;
            yield return null;
        }
    }
}
