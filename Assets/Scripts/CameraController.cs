using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target; // ������, ������ �������� ������� ������
    public float distance = 5.0f; // ��������� ���������� �� �������
    public float minDistance = 2.0f; // ����������� ����������
    public float maxDistance = 15.0f; // ������������ ����������

    public float xSpeed = 120.0f; // �������� �������� �� X
    public float ySpeed = 120.0f; // �������� �������� �� Y

    public float yMinLimit = -20f; // ����������� ���� �� Y (����)
    public float yMaxLimit = 80f;  // ����������� ���� �� Y (�����)

    public float zoomSpeed = 10f; // �������� ����
    public float zoomSmoothing = 5f; // ����������� ����

    public float rotationSmoothing = 8f; // ����������� ��������

    private float x = 0.0f; // ������� ���� X
    private float y = 0.0f; // ������� ���� Y
    private float currentDistance; // ������� ����������
    private float desiredDistance; // �������� ����������

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
            // �������� ������ ��� ������� ������ ������ ����
            if (Input.GetMouseButton(0))
            {
                x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
                y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;

                // ������������ ���� �� Y
                y = ClampAngle(y, yMinLimit, yMaxLimit);
            }

            // ��������� ���� ��������� ����
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0.0f)
            {
                desiredDistance = Mathf.Clamp(desiredDistance - scroll * zoomSpeed, minDistance, maxDistance);
            }

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

    // ����� ��� ����������� ����
    private float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360f) angle += 360f;
        if (angle > 360f) angle -= 360f;
        return Mathf.Clamp(angle, min, max);
    }
}