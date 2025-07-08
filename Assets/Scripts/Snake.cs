using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Snake : MonoBehaviour
{
    [Header("Snake Settings")]
    public float segmentSize = 1f;
    public float headRadius = 0.3f;
    public float tailRadius = 0.1f;
    [Range(0, 1)] public float cornerSmoothing = 0.5f;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationLerpSpeed = 10f;
    public float followSmoothness = 10f;

    [Header("References")]
    public Material snakeMaterial;

    private List<Transform> bodySegments = new List<Transform>();
    private Transform head;
    private List<Vector3> pathPoints = new List<Vector3>();
    private Mesh mesh;
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private bool isMoving = false;
    private Vector3 moveDirection = Vector3.forward;
    private Vector3 targetPosition;
    private const int radialSegments = 12;
    private Vector3 lastGoodDirection = Vector3.forward;
    private float[] segmentRadii;
    private List<Vector3> segmentVelocities;

    private void Start()
    {
        // Find head (first child object)
        if (transform.childCount == 0)
        {
            Debug.LogError("No child objects found for snake!");
            return;
        }

        head = transform.GetChild(0);
        head.name = "Head";

        // Find all segments (children with "Segment" in name)
        bodySegments = new List<Transform>();
        for (int i = 1; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.name.Contains("Segment"))
            {
                bodySegments.Add(child);
            }
        }

        // Order segments by name
        bodySegments = bodySegments.OrderBy(t => t.name).ToList();

        // Initialize velocities for smooth movement
        segmentVelocities = new List<Vector3>();
        for (int i = 0; i < bodySegments.Count; i++)
        {
            segmentVelocities.Add(Vector3.zero);
        }

        // Calculate radii for each segment
        CalculateSegmentRadii();

        InitializePath();
        GenerateMesh();
    }

    private void CalculateSegmentRadii()
    {
        segmentRadii = new float[bodySegments.Count + 1];
        segmentRadii[0] = headRadius; // Head radius

        if (bodySegments.Count > 0)
        {
            float radiusStep = (headRadius - tailRadius) / bodySegments.Count;

            for (int i = 1; i < segmentRadii.Length; i++)
            {
                segmentRadii[i] = headRadius - radiusStep * i;
            }
        }
    }

    private void InitializePath()
    {
        pathPoints.Clear();
        pathPoints.Add(head.localPosition); // Head position (0,0,0)

        // Position body segments with proper spacing based on their actual positions
        for (int i = 0; i < bodySegments.Count; i++)
        {
            pathPoints.Add(bodySegments[i].localPosition);
        }
    }

    private void Update()
    {
        HandleInput();
        MoveSnake();
        UpdatePath();
        UpdateMesh();
    }

    private void GenerateMesh()
    {
        mesh = new Mesh();
        mesh.name = "SnakeMesh";
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().material = snakeMaterial;
        mesh.MarkDynamic();
    }

    private void HandleInput()
    {
        if (isMoving) return;

        if (Input.GetKeyDown(KeyCode.W) && moveDirection != Vector3.back)
        {
            ChangeDirection(Vector3.forward);
        }
        else if (Input.GetKeyDown(KeyCode.S) && moveDirection != Vector3.forward)
        {
            ChangeDirection(Vector3.back);
        }
        else if (Input.GetKeyDown(KeyCode.A) && moveDirection != Vector3.right)
        {
            ChangeDirection(Vector3.left);
        }
        else if (Input.GetKeyDown(KeyCode.D) && moveDirection != Vector3.left)
        {
            ChangeDirection(Vector3.right);
        }
    }

    private void ChangeDirection(Vector3 newDirection)
    {
        lastGoodDirection = moveDirection;
        moveDirection = newDirection;
        targetPosition = head.localPosition + moveDirection * segmentSize;
        isMoving = true;
    }

    private void MoveSnake()
    {
        // Update head rotation based on movement direction
        if (moveDirection != Vector3.zero)
        {
            head.rotation = Quaternion.Lerp(
                head.rotation,
                Quaternion.LookRotation(moveDirection),
                rotationLerpSpeed * Time.deltaTime
            );
        }

        if (!isMoving) return;

        // Move head
        head.localPosition = Vector3.MoveTowards(head.localPosition, targetPosition, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(head.localPosition, targetPosition) < 0.01f)
        {
            isMoving = false;
        }
    }

    private void UpdatePath()
    {
        // Only add new point when head has moved enough
        if (pathPoints.Count == 0 || Vector3.Distance(head.localPosition, pathPoints[0]) > segmentSize * 0.9f)
        {
            pathPoints.Insert(0, head.localPosition);
        }

        // Maintain path length
        int maxPathPoints = bodySegments.Count + 1;
        while (pathPoints.Count > maxPathPoints)
        {
            pathPoints.RemoveAt(pathPoints.Count - 1);
        }

        // Update body segments with smooth movement
        for (int i = 0; i < bodySegments.Count; i++)
        {
            int targetIndex = Mathf.Min(i + 1, pathPoints.Count - 1);
            Vector3 targetPos = pathPoints[targetIndex];

            Vector3 currentVelocity = segmentVelocities[i];
            bodySegments[i].localPosition = Vector3.SmoothDamp(
                bodySegments[i].localPosition,
                targetPos,
                ref currentVelocity,
                1f / followSmoothness
            );
            segmentVelocities[i] = currentVelocity;

            // Calculate direction with smoothing
            Vector3 dir;
            if (targetIndex > 0)
            {
                dir = (pathPoints[targetIndex - 1] - pathPoints[targetIndex]).normalized;

                // Apply corner smoothing for non-tail segments
                if (targetIndex < pathPoints.Count - 1)
                {
                    Vector3 nextDir = (pathPoints[targetIndex] - pathPoints[targetIndex + 1]).normalized;
                    dir = Vector3.Lerp(dir, nextDir, cornerSmoothing).normalized;
                }
            }
            else
            {
                dir = (head.localPosition - bodySegments[i].localPosition).normalized;
            }

            if (dir != Vector3.zero)
            {
                bodySegments[i].rotation = Quaternion.LookRotation(dir);
            }
        }
    }

    private void UpdateMesh()
    {
        // Use actual positions for smooth mesh
        List<Vector3> meshPoints = new List<Vector3>();
        meshPoints.Add(head.localPosition);
        for (int i = 0; i < bodySegments.Count; i++)
        {
            meshPoints.Add(bodySegments[i].localPosition);
        }

        if (meshPoints.Count < 2) return;

        vertices.Clear();
        triangles.Clear();

        // Create vertex rings along the path
        for (int i = 0; i < meshPoints.Count; i++)
        {
            Quaternion rotation;
            Vector3 position = meshPoints[i];
            float currentRadius = segmentRadii[Mathf.Min(i, segmentRadii.Length - 1)];

            if (i == 0) // Head
            {
                rotation = head.rotation;
            }
            else
            {
                Vector3 dir = (meshPoints[i - 1] - meshPoints[i]).normalized;

                if (i < meshPoints.Count - 1)
                {
                    Vector3 nextDir = (meshPoints[i] - meshPoints[i + 1]).normalized;
                    dir = Vector3.Lerp(dir, nextDir, cornerSmoothing).normalized;
                }

                rotation = Quaternion.LookRotation(dir != Vector3.zero ? dir : lastGoodDirection);
            }

            for (int j = 0; j < radialSegments; j++)
            {
                float angle = j * Mathf.PI * 2f / radialSegments;
                Vector3 localPos = new Vector3(Mathf.Cos(angle) * currentRadius, Mathf.Sin(angle) * currentRadius, 0);
                vertices.Add(position + rotation * localPos);
            }
        }

        // Create triangles
        for (int i = 0; i < meshPoints.Count - 1; i++)
        {
            for (int j = 0; j < radialSegments; j++)
            {
                int nextJ = (j + 1) % radialSegments;
                int currentBase = i * radialSegments;
                int nextBase = (i + 1) * radialSegments;

                triangles.Add(currentBase + j);
                triangles.Add(nextBase + j);
                triangles.Add(nextBase + nextJ);

                triangles.Add(currentBase + j);
                triangles.Add(nextBase + nextJ);
                triangles.Add(currentBase + nextJ);
            }
        }

        // Create tail cap
        int centerIndex = vertices.Count;
        vertices.Add(meshPoints[meshPoints.Count - 1]);

        int lastRingStart = (meshPoints.Count - 1) * radialSegments;
        for (int j = 0; j < radialSegments; j++)
        {
            int nextJ = (j + 1) % radialSegments;
            triangles.Add(centerIndex);
            triangles.Add(lastRingStart + nextJ);
            triangles.Add(lastRingStart + j);
        }

        // Update mesh
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }
}