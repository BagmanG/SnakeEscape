using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public LevelManager LevelManager;
    private PlayerController playerController;
    private Snake snake;
    private bool isPlayerTurn = true;
    public GameUI UI;
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

    public void GameOver()
    {
        UI.ShowGameOver();
    }

    public void TryAgain()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}