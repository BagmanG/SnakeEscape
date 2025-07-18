using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public LevelManager LevelManager;
    private PlayerController playerController;
    private Snake snake;
    private bool isPlayerTurn = true;
    public GameUI UI;
    public int StarsCount = 0;
    public GameObject[] Stars;
    private Action moveCompleteAction;
    bool completed = false;
    public void Start()
    {
        StarsCount = 0;
        LevelManager.LoadLevel();
        playerController = FindObjectOfType<PlayerController>();
        snake = FindObjectOfType<Snake>();

        if (playerController != null)
        {
            moveCompleteAction = () => StartCoroutine(OnPlayerMoveComplete());
            playerController.OnMoveComplete += moveCompleteAction;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseGame();
        }
    }

    private IEnumerator OnPlayerMoveComplete()
    {
        isPlayerTurn = false;

        Vector2Int playerPosition = new Vector2Int(
            Mathf.RoundToInt(playerController.transform.position.x),
            Mathf.RoundToInt(playerController.transform.position.z)
        );

        // �������� ���� ���� �� �����
        Snake[] snakes = FindObjectsOfType<Snake>();

        // ������ ��� �������� ������� ������
        HashSet<Vector2Int> occupiedCells = new HashSet<Vector2Int>();

        // 1. �������� ������� ������� ���� ��������� ���� ����
        foreach (Snake snake in snakes)
        {
            for (int i = 0; i < snake.transform.childCount; i++)
            {
                Transform segment = snake.transform.GetChild(i);
                Vector2Int pos = new Vector2Int(
                    Mathf.RoundToInt(segment.position.x),
                    Mathf.RoundToInt(segment.position.z)
                );
                occupiedCells.Add(pos);
            }
        }

        // 2. ������ ���� ��������� ���� ���
        Dictionary<Snake, Vector2Int> plannedMoves = new Dictionary<Snake, Vector2Int>();
        Dictionary<Snake, List<Vector2Int>> plannedBodies = new Dictionary<Snake, List<Vector2Int>>();

        foreach (Snake snake in snakes)
        {
            // �������� ����������� ������� ���� ������ ����
            List<Vector2Int> dynamicObstacles = new List<Vector2Int>(occupiedCells);
            foreach (var otherSnake in snakes)
            {
                if (otherSnake != snake)
                {
                    dynamicObstacles.AddRange(otherSnake.GetPlannedBodyPositions());
                }
            }

            Vector2Int nextCell = snake.PeekNextMove(playerPosition, dynamicObstacles);
            plannedMoves[snake] = nextCell;
            plannedBodies[snake] = snake.GetPlannedBodyPositions();
        }

        // 3. ��������� ���������
        HashSet<Vector2Int> conflictCells = new HashSet<Vector2Int>();
        HashSet<Vector2Int> allPlannedMoves = new HashSet<Vector2Int>();

        foreach (var move in plannedMoves.Values)
        {
            if (allPlannedMoves.Contains(move) || occupiedCells.Contains(move))
            {
                conflictCells.Add(move);
            }
            allPlannedMoves.Add(move);
        }

        // 4. ��������� ���� ������ ��� ���� ��� ����������
        foreach (Snake snake in snakes)
        {
            Vector2Int plannedMove = plannedMoves[snake];

            if (!conflictCells.Contains(plannedMove) &&
                !occupiedCells.Contains(plannedMove))
            {
                yield return StartCoroutine(snake.Step());

                // ��������� ������� ������ ����� ����
                for (int i = 0; i < snake.transform.childCount; i++)
                {
                    Transform segment = snake.transform.GetChild(i);
                    Vector2Int pos = new Vector2Int(
                        Mathf.RoundToInt(segment.position.x),
                        Mathf.RoundToInt(segment.position.z)
                    );
                    occupiedCells.Add(pos);
                }
            }
        }

        isPlayerTurn = true;
    }


    private void OnDestroy()
    {
        if (playerController != null)
        {
            playerController.OnMoveComplete -= moveCompleteAction;
        }
    }

    public void GameOver()
    {
        if (completed == false)
            UI.ShowGameOver();
    }

    public void TryAgain()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GivePlayerStar()
    {
        StarsCount++;
        for(int i = 0; i < 3;i++)
        {
            Stars[i].SetActive(StarsCount-1 >= i);
        }
    }

    public void LevelCompleted()
    {
        Debug.Log("Level Completed!");
        completed = true;
    }

    public void PauseGame()
    {
        UI.SetPauseVisible(!UI.PauseVisible);
    }
}