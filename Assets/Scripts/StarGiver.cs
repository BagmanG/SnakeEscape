using UnityEngine;

public class StarGiver : MonoBehaviour
{
    public void GivePlayer()
    {
        Debug.Log("����� ���� ������");
    }

    public void GiveSnake()
    {
        Destroy(gameObject);
    }
}
