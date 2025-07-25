using UnityEngine;

public class BackgroundSound : MonoBehaviour
{
    private static BackgroundSound _instance;

    void Awake()
    {
        // ���������, ���������� �� ��� ���������
        if (_instance != null && _instance != this)
        {
            // ���� ��, ���������� ����� ������
            Destroy(gameObject);
        }
        else
        {
            // ���� ���, ��������� ������� ���������
            _instance = this;
            DontDestroyOnLoad(gameObject); // ������ ������ ��������������
        }
    }
}