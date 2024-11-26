using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities.Enums;
using Utilities.Signals;

public class CameraMovement : MonoBehaviour
{
    
    private Vector3 startMousePos, startCameraPos, lastCameraPos;
    private Vector2 movementConstantRatioWithCamera;
    private IEnumerator driftCoroutine;

    private const float speedConstant = .5f;
    private const float maximumSpeed = 4f;
    private const float minimumDriftStartSpeed = .35f;
    private const float driftEndSpeed = .01f;
    
    private bool clicked = false;


    private void Start()
    {
        float pixelByDistance = (Camera.main.orthographicSize * 2) / Screen.height;
        movementConstantRatioWithCamera = new Vector2(pixelByDistance, pixelByDistance / Mathf.Sin(Mathf.Deg2Rad * transform.eulerAngles.x));

        Signals.OnFaceCanvasToCamera?.Invoke(transform.rotation);
    }

    private void Update()
    {
        if (GameManager.GameState != GameState.Play)
        {
            clicked = false;
            return;
        }

        if(Input.GetMouseButtonDown(0))
        {
            if(driftCoroutine != null) StopCoroutine(driftCoroutine);

            startMousePos = Input.mousePosition;
            startCameraPos = transform.position;

            clicked = true;
        }
        else if(clicked && Input.GetMouseButton(0)) 
        {
            lastCameraPos = transform.position;

            Vector3 currentFrameInput = Input.mousePosition;
            float xDiff = startMousePos.x - currentFrameInput.x;
            float yDiff = startMousePos.y - currentFrameInput.y;

            transform.position = Vector3.Lerp(transform.position, startCameraPos + new Vector3(xDiff * movementConstantRatioWithCamera.x, 0, yDiff * movementConstantRatioWithCamera.y), .25f);
        }
        else if (clicked && Input.GetMouseButtonUp(0))
        {
            StartCoroutine(driftCoroutine = Drift());
            clicked = false;
        }


    }

    private IEnumerator Drift()
    {
        Vector3 lastFrameVector = (transform.position - lastCameraPos);
        float speed = lastFrameVector.magnitude * speedConstant;

        speed = Mathf.Clamp(speed, 0f, maximumSpeed);
        if (speed < minimumDriftStartSpeed)
            yield break;

        Vector3 direction = lastFrameVector.normalized;
        direction.y = 0;

        while(speed > driftEndSpeed) 
        {
            speed *= .9f;
            transform.position += speed * direction;
            yield return null;
        }
    }
}
