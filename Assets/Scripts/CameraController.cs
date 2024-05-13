using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Camera camera;
    Vector3 defaultPosition;
    [SerializeField] private float minZoom = 0.5f, maxZoom = 6f;
    private Vector3 dragOrigin;
    [SerializeField] private bool cameraDragging = true;

    [SerializeField] private float outerLeft = 0f, outerRight, outerDown = 0f, outerUp;


    public void SetBorders(int width, int height)
    {
        outerLeft = height * -0.5f;
        outerRight = width;
        outerUp = height;
    }

    public void SetDefaultPosition(float x, float y)
    {
        defaultPosition = new Vector3(x, y, -10f);
        transform.position = defaultPosition;
        camera.orthographicSize = 5;
    }

    public void MoveCamera(float x, float y, float range)
    {
        CameraMontion(new Vector3(x, y, -10f), range);
    }

    public void ResetCamera()
    {
        CameraMontion(defaultPosition, 5);
    }

    public async void CameraMontion(Vector3 _moveTarget, float _zoomTarget)
    {
        Vector3 _MovePath = new Vector3(_moveTarget.x - gameObject.transform.position.x, _moveTarget.y - gameObject.transform.position.y, 0f);
        float _zoomDiference = _zoomTarget - camera.orthographicSize;
        var endTime = Time.time + 0.5f;
        while (Time.time < endTime)
        {
            transform.position += _MovePath * Time.deltaTime * 2;
            camera.orthographicSize += _zoomDiference * Time.deltaTime * 2;
            await Task.Yield();
        }
        transform.position = _moveTarget;
        camera.orthographicSize = _zoomTarget;
        await Task.Yield();
    }

    void Update()
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0 && camera.orthographicSize > minZoom)
        {
            camera.orthographicSize -= 0.5f;
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0 && camera.orthographicSize < maxZoom)
        {
            camera.orthographicSize += 0.5f;
        }
    }


    void LateUpdate()
    {
        float dragSpeed = 5 * camera.orthographicSize;

        if (Input.GetMouseButtonDown(1))
        {
            dragOrigin = Input.mousePosition;
            return;
        }

        if (!Input.GetMouseButton(1)) return;

        Vector3 pos = Camera.main.ScreenToViewportPoint(dragOrigin - Input.mousePosition);
        Vector3 move = new Vector3(pos.x * dragSpeed, pos.y * dragSpeed, 0f);

        if (move.x > 0f)
        {
            if (!(this.transform.position.x < outerRight))
            {
                move.x = 0f;
            }
        }
        else
        {
            if (!(this.transform.position.x > outerLeft))
            {
                move.x = 0f;
            }
        }
        if (move.y > 0f)
        {
            if (!(this.transform.position.y < outerUp))
            {
                move.y = 0f;
            }
        }
        else
        {
            if (!(this.transform.position.y > outerDown))
            {
                move.y = 0f;
            }
        }
        transform.Translate(move, Space.World);

        if (Input.GetMouseButton(1))
        {
            dragOrigin = Input.mousePosition;
        }

    }
}