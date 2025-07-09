using UnityEngine;

public class GameManager : MonoBehaviour
{
    public LevelManager LevelManager;
    private PlayerController playerController;
    private Snake snake;
    private bool isPlayerTurn = true;

    public void Start()
    {
        LevelManager.LoadLevel();
        playerController = FindObjectOfType<PlayerController>();
        snake = FindObjectOfType<Snake>();

        if (playerController != null)
        {
            playerController.OnMoveComplete += OnPlayerMoveComplete;
        }
    }

    private void OnPlayerMoveComplete()
    {
        Debug.Log("��� ������");
        // ��� ������ ��������, ������ ��� ������
        isPlayerTurn = false;

        // �������� ������� ������� ������
        Vector2Int playerPosition = new Vector2Int(
            Mathf.RoundToInt(playerController.transform.position.x),
            Mathf.RoundToInt(playerController.transform.position.z)
        );

        // ������� ������
        snake.MakeMove(playerPosition);

        // ���������� ��� ������
        isPlayerTurn = true;
    }

    private void OnDestroy()
    {
        if (playerController != null)
        {
            playerController.OnMoveComplete -= OnPlayerMoveComplete;
        }
    }
}