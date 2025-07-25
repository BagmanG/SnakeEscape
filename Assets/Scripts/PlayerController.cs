using UnityEngine;
using System;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;

    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private bool isMoving = false;
    private bool canMove = true;
    public GameManager GameManager;
    public LevelManager LevelManager;
    public CameraController cameraController;
    public Animator animator;
    public event Action OnMoveComplete;

    private bool isPerformingAction = false;
    private Vector3Int currentMoveDirection;

    private void Start()
    {
        targetPosition = transform.position;
        targetRotation = transform.rotation;
        canMove = true;
    }

    private void Update()
    {
        if (isMoving)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            CheckTileAfterMovement();
            CheckSnakeCollision();

            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                transform.position = targetPosition;
                transform.rotation = targetRotation;
                isMoving = false;

                CheckTileAfterMovement();
                CheckSnakeCollision();
                CheckStarCollision();

                if (IsOnIce() && ShouldContinueSlide(currentMoveDirection))
                {
                    StartCoroutine(ContinueSlide(currentMoveDirection));
                }
                else
                {
                    CompletePlayerAction();
                }
            }
            return;
        }

        if (!isPerformingAction && canMove)
        {
            // ����� ���������� ����������, ������������ ����� �����
            Vector3Int moveDirection = Vector3Int.zero;

            if (Input.GetKeyDown(KeyCode.W)) moveDirection = GetCameraRelativeDirection(Vector3Int.forward);
            else if (Input.GetKeyDown(KeyCode.S)) moveDirection = GetCameraRelativeDirection(Vector3Int.back);
            else if (Input.GetKeyDown(KeyCode.A)) moveDirection = GetCameraRelativeDirection(Vector3Int.left);
            else if (Input.GetKeyDown(KeyCode.D)) moveDirection = GetCameraRelativeDirection(Vector3Int.right);

            if (moveDirection != Vector3Int.zero)
            {
                StartPlayerAction(moveDirection);
            }
        }
    }

    // ��������� ������ (MoveUp, MoveDown � �.�.) �������� ��� ���������, ��� � ���������� �������

    public void MoveUp()
    {
        if (!isPerformingAction && canMove && !isMoving)
        {
            Vector3Int direction = GetCameraRelativeDirection(Vector3Int.forward);
            StartPlayerAction(direction);
        }
    }

    public void MoveDown()
    {
        if (!isPerformingAction && canMove && !isMoving)
        {
            Vector3Int direction = GetCameraRelativeDirection(Vector3Int.back);
            StartPlayerAction(direction);
        }
    }

    public void MoveLeft()
    {
        if (!isPerformingAction && canMove && !isMoving)
        {
            Vector3Int direction = GetCameraRelativeDirection(Vector3Int.left);
            StartPlayerAction(direction);
        }
    }

    public void MoveRight()
    {
        if (!isPerformingAction && canMove && !isMoving)
        {
            Vector3Int direction = GetCameraRelativeDirection(Vector3Int.right);
            StartPlayerAction(direction);
        }
    }

    private void StartPlayerAction(Vector3Int direction)
    {
        isPerformingAction = true;
        canMove = false;
        currentMoveDirection = direction;

        Vector3 newPosition = transform.position + direction;
        Vector2Int gridPos = new Vector2Int(Mathf.RoundToInt(newPosition.x), Mathf.RoundToInt(newPosition.z));

        if (IsPositionValid(gridPos))
        {
            animator.SetTrigger("Jump");
            targetRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            targetPosition = newPosition;
            isMoving = true;
        }
        else
        {
            // ���� �������� ����������, ����� ��������� ����� �������� ��� ������ CompletePlayerAction
            isPerformingAction = false;
            canMove = true;
            animator.ResetTrigger("Jump"); // ���������� �������� ������, ���� �������� �� ����������
        }
        Debugger.Instance?.Log($"Player attempts move to {gridPos}");

    }

    private void CompletePlayerAction()
    {
        isPerformingAction = false;
        OnMoveComplete?.Invoke();
        StartCoroutine(EnableMovementAfterDelay(0.3f));
    }

    private IEnumerator EnableMovementAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        canMove = true;
    }

    private IEnumerator ContinueSlide(Vector3Int direction)
    {
        yield return new WaitForSeconds(0.1f);
        Vector3 newPosition = transform.position + direction;
        Vector2Int gridPos = new Vector2Int(Mathf.RoundToInt(newPosition.x), Mathf.RoundToInt(newPosition.z));

        if (IsPositionValid(gridPos))
        {
            targetPosition = newPosition;
            isMoving = true;
        }
        else
        {
            CompletePlayerAction();
        }
    }

    private bool IsOnIce()
    {
        Vector2Int currentPos = new Vector2Int(
            Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.z)
        );
        return LevelManager.CurrentLevel.grid[currentPos.x, currentPos.y] == 7;
    }

    private bool ShouldContinueSlide(Vector3Int direction)
    {
        Vector2Int currentPos = new Vector2Int(
            Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.z)
        );

        Vector2Int nextPos = currentPos + new Vector2Int(direction.x, direction.z);

        if (nextPos.x < 0 || nextPos.x >= LevelManager.CurrentLevel.width ||
            nextPos.y < 0 || nextPos.y >= LevelManager.CurrentLevel.height)
        {
            return false;
        }

        int nextTile = LevelManager.CurrentLevel.grid[nextPos.x, nextPos.y];
        return nextTile == 0 || nextTile == 3 || nextTile == 4 || nextTile == 7;
    }

    private bool IsPositionValid(Vector2Int gridPos)
    {
        return gridPos.x >= 0 && gridPos.x < LevelManager.CurrentLevel.width &&
               gridPos.y >= 0 && gridPos.y < LevelManager.CurrentLevel.height &&
               (LevelManager.CurrentLevel.grid[gridPos.x, gridPos.y] == 0 ||
                LevelManager.CurrentLevel.grid[gridPos.x, gridPos.y] == 3 ||
                LevelManager.CurrentLevel.grid[gridPos.x, gridPos.y] == 4 ||
                LevelManager.CurrentLevel.grid[gridPos.x, gridPos.y] == 7);
    }

    private void CheckStarCollision()
    {
        StarGiver[] stars = FindObjectsOfType<StarGiver>();
        if (stars == null || stars.Length == 0) return;

        Vector2Int playerPos = new Vector2Int(
            Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.z)
        );

        foreach (StarGiver star in stars)
        {
            Vector2Int starPos = new Vector2Int(
                Mathf.RoundToInt(star.transform.position.x),
                Mathf.RoundToInt(star.transform.position.z)
            );

            if (playerPos == starPos)
            {
                star.GivePlayer();
                break;
            }
        }
    }

    private void CheckSnakeCollision()
    {
        Snake[] snakes = FindObjectsOfType<Snake>();
        if (snakes == null || snakes.Length == 0) return;

        Vector3 playerPos = transform.position;

        foreach (Snake snake in snakes)
        {
            Transform snakeHead = snake.transform.GetChild(0);
            if (Vector3.Distance(playerPos, snakeHead.position) < 0.5f)
            {
                GameManager.GameOver();
                return;
            }

            for (int i = 1; i < snake.transform.childCount; i++)
            {
                Transform segment = snake.transform.GetChild(i);
                if (Vector3.Distance(playerPos, segment.position) < 0.5f)
                {
                    GameManager.GameOver();
                    return;
                }
            }
        }
    }

    private void CheckTileAfterMovement()
    {
        Vector2Int currentPos = new Vector2Int(
            Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.z)
        );

        if (LevelManager.CurrentLevel.grid[currentPos.x, currentPos.y] == 4 &&
            GameManager.StarsCount > 0)
        {
            GameManager.LevelCompleted();
        }
    }

    private Vector3Int GetCameraRelativeDirection(Vector3Int baseDirection)
    {
        Vector3 forward = cameraController.transform.forward;
        Vector3 right = cameraController.transform.right;
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        if (baseDirection == Vector3Int.forward || baseDirection == Vector3Int.back)
        {
            if (Mathf.Abs(forward.x) > Mathf.Abs(forward.z))
                return new Vector3Int((int)Mathf.Sign(forward.x), 0, 0) * (baseDirection.z > 0 ? 1 : -1);
            else
                return new Vector3Int(0, 0, (int)Mathf.Sign(forward.z)) * (baseDirection.z > 0 ? 1 : -1);
        }
        else if (baseDirection == Vector3Int.left || baseDirection == Vector3Int.right)
        {
            if (Mathf.Abs(right.x) > Mathf.Abs(right.z))
                return new Vector3Int((int)Mathf.Sign(right.x), 0, 0) * (baseDirection.x > 0 ? 1 : -1);
            else
                return new Vector3Int(0, 0, (int)Mathf.Sign(right.z)) * (baseDirection.x > 0 ? 1 : -1);
        }

        return baseDirection;
    }
}