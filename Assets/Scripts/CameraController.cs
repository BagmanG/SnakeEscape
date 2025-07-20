using UnityEngine;
using YG;

public class CameraController : MonoBehaviour
{
    public Transform target; // ������, ������ �������� ������� ������
    public float distance = 5.0f; // ��������� ���������� �� �������
    public float minDistance = 2.0f; // ����������� ����������
    public float maxDistance = 15.0f; // ������������ ����������

    [Header("Rotation Settings")]
    public float xSpeed = 120.0f; // �������� �������� �� X (��� ����������)
    public float ySpeed = 120.0f; // �������� �������� �� Y (��� ����������)

    [Space]
    public float mobileXSpeed = 60.0f; // �������� �������� �� X (��� ���������)
    public float mobileYSpeed = 60.0f; // �������� �������� �� Y (��� ���������)

    [Header("Zoom Settings")]
    public float zoomSpeed = 5f; // �������� ���� ��������� ����

    [Header("Angle Limits")]
    public float yMinLimit = -20f; // ����������� ���� �� Y (����)
    public float yMaxLimit = 80f;  // ����������� ���� �� Y (�����)

    [Header("Smoothing")]
    public float zoomSmoothing = 5f; // ����������� ����
    public float rotationSmoothing = 8f; // ����������� ��������

    private float x = 0.0f; // ������� ���� X
    private float y = 0.0f; // ������� ���� Y
    private float currentDistance; // ������� ����������
    private float desiredDistance; // �������� ����������

    private Vector2? lastMousePosition; // ��� ������������ ���������� ������� ����/����
    private int? touchId; // ��� ������������ ����������� ����

    void Start()
    {
        // ������������� �����
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;

        currentDistance = distance;
        desiredDistance = distance;

        // ���� target �� ��������, ��������� ����� ������ � ����� "Player"
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }
    }

    void LateUpdate()
    {
        if (target)
        {
            HandleRotationInput();
            HandleZoomInput();

            // ������� ��������� ����������
            currentDistance = Mathf.Lerp(currentDistance, desiredDistance, Time.deltaTime * zoomSmoothing);

            // ��������� ����� ������� ������
            Quaternion rotation = Quaternion.Euler(y, x, 0);
            Vector3 position = rotation * new Vector3(0.0f, 0.0f, -currentDistance) + target.position;

            // ������� �������� � �����������
            transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * rotationSmoothing);
            transform.position = Vector3.Lerp(transform.position, position, Time.deltaTime * rotationSmoothing);
        }
    }

    private void HandleRotationInput()
    {
        // ��������� ����� ��� ���������� (����)
        if (YG2.envir.isDesktop)
        {
            if (Input.GetMouseButtonDown(0))
            {
                lastMousePosition = Input.mousePosition;
            }
            else if (Input.GetMouseButton(0))
            {
                if (lastMousePosition.HasValue)
                {
                    Vector2 delta = (Vector2)Input.mousePosition - lastMousePosition.Value;
                    x += delta.x * xSpeed * 0.02f;
                    y -= delta.y * ySpeed * 0.02f;
                    y = ClampAngle(y, yMinLimit, yMaxLimit);
                }
                lastMousePosition = Input.mousePosition;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                lastMousePosition = null;
            }
        }
        // ��������� ����� ��� ��������� ��������� (���)
        else
        {
            if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Began)
                {
                    touchId = touch.fingerId;
                    lastMousePosition = touch.position;
                }
                else if (touch.phase == TouchPhase.Moved && touchId == touch.fingerId)
                {
                    if (lastMousePosition.HasValue)
                    {
                        Vector2 delta = touch.position - lastMousePosition.Value;
                        // ���������� ������� �������� ��� ��������� ���������
                        x += delta.x * mobileXSpeed * 0.02f;
                        y -= delta.y * mobileYSpeed * 0.02f;
                        y = ClampAngle(y, yMinLimit, yMaxLimit);
                    }
                    lastMousePosition = touch.position;
                }
                else if (touch.phase == TouchPhase.Ended && touchId == touch.fingerId)
                {
                    touchId = null;
                    lastMousePosition = null;
                }
            }
        }
    }

    private void HandleZoomInput()
    {
        if (YG2.envir.isDesktop)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                desiredDistance = Mathf.Clamp(desiredDistance - scroll * zoomSpeed, minDistance, maxDistance);
            }
        }
    }

    // ��������� ������ ��� ��������� ���������
    public void ZoomIn()
    {
        desiredDistance = Mathf.Clamp(desiredDistance - 1f, minDistance, maxDistance);
    }

    public void ZoomOut()
    {
        desiredDistance = Mathf.Clamp(desiredDistance + 1f, minDistance, maxDistance);
    }

    // ����� ��� ����������� ����
    private float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360f) angle += 360f;
        if (angle > 360f) angle -= 360f;
        return Mathf.Clamp(angle, min, max);
    }
}