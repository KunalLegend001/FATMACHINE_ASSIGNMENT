using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Rigidbody))]
public class TrayGridBasedMover : MonoBehaviour
{
    public float moveForce = 300f;
    public float yFixed = 1f;
    public float dragThreshold = 20f;

    public List<Transform> trayPlacements;

    private Rigidbody rb;
    private Camera cam;
    private Vector3 lastInputPosition;
    private bool isDragging = false;
    private bool isMoving = false;
    private Vector3 targetPosition;
    private Vector3 startGridPosition;
    private GridVisualizer gridVisualizer;
    private string trayTag;
    private bool snappedToPlacement = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
        cam = Camera.main;

        trayTag = gameObject.tag;
        targetPosition = transform.position;
    }

    void Update()
    {
        // Handle touch input in Update
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            Ray ray = cam.ScreenPointToRay(touch.position);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    if (touch.phase == TouchPhase.Began)
                    {
                        OnTouchDown(touch.position);
                    }
                    else if (touch.phase == TouchPhase.Moved)
                    {
                        OnTouchDrag(touch.position);
                    }
                    else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                    {
                        OnTouchUp();
                    }
                }
            }
        }
    }

    void OnMouseDown()
    {
        if (snappedToPlacement) return;
        if (IsPointerOverUI()) return;
        lastInputPosition = Input.mousePosition;
        isDragging = true;
    }

    void OnMouseDrag()
    {
        if (!isDragging || isMoving) return;
        if (IsPointerOverUI()) return;

        HandleDragging(Input.mousePosition);
    }

    void OnMouseUp()
    {
        isDragging = false;
    }

    void OnTouchDown(Vector2 touchPosition)
    {
        if (snappedToPlacement) return;

        lastInputPosition = touchPosition;
        isDragging = true;
    }

    void OnTouchDrag(Vector2 touchPosition)
    {
        if (!isDragging || isMoving) return;

        HandleDragging(touchPosition);
    }

    void OnTouchUp()
    {
        isDragging = false;
    }

    void HandleDragging(Vector2 inputPosition)
    {
        if (gridVisualizer == null)
        {
            gridVisualizer = FindObjectOfType<GridVisualizer>();
            if (gridVisualizer == null)
            {
                Debug.LogError("GridVisualizer not found during drag on " + gameObject.name);
                return;
            }
            else
            {
                Debug.Log("GridVisualizer found dynamically during drag on " + gameObject.name);
            }
        }

        Vector3 inputDelta = (Vector3)inputPosition - lastInputPosition;

        if (inputDelta.magnitude > dragThreshold)
        {
            Vector3 moveDir = Vector3.zero;

            if (Mathf.Abs(inputDelta.x) > Mathf.Abs(inputDelta.y))
            {
                moveDir = inputDelta.x > 0 ? Vector3.right : Vector3.left;
            }
            else
            {
                moveDir = inputDelta.y > 0 ? Vector3.forward : Vector3.back;
            }

            Vector3 nextGridPoint = FindNextGridPoint(transform.position, moveDir);

            startGridPosition = transform.position;

            if (nextGridPoint != Vector3.zero)
            {
                targetPosition = nextGridPoint;
            }
            else
            {
                targetPosition = transform.position + moveDir * 2f;
            }

            isMoving = true;
            rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
            lastInputPosition = inputPosition;
        }
    }

    void FixedUpdate()
    {
        if (snappedToPlacement) return;

        Vector3 pos = rb.position;
        pos.y = yFixed;
        rb.position = pos;

        if (isMoving)
        {
            Vector3 moveDirection = (targetPosition - transform.position).normalized;
            rb.AddForce(moveDirection * moveForce * Time.fixedDeltaTime, ForceMode.Force);

            float distance = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z),
                                              new Vector3(targetPosition.x, 0, targetPosition.z));

            if (distance < 0.05f)
            {
                rb.velocity = Vector3.zero;
                rb.MovePosition(new Vector3(targetPosition.x, yFixed, targetPosition.z));
                isMoving = false;

                rb.constraints = RigidbodyConstraints.FreezeAll;
            }
        }
    }

    Vector3 FindNextGridPoint(Vector3 currentPos, Vector3 moveDir)
    {
        if (gridVisualizer == null || gridVisualizer.gridPoints.Count == 0)
            return Vector3.zero;

        Vector3 bestPoint = Vector3.zero;
        float bestScore = float.MaxValue;

        foreach (Vector3 point in gridVisualizer.gridPoints)
        {
            Vector3 toPoint = (point - currentPos).normalized;

            if (Vector3.Dot(moveDir, toPoint) > 0.7f)
            {
                float distance = Vector3.Distance(currentPos, point);
                if (distance < bestScore && distance > 0.1f)
                {
                    bestScore = distance;
                    bestPoint = point;
                }
            }
        }

        return bestPoint;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (snappedToPlacement) return;

        if (collision.gameObject.CompareTag(trayTag))
        {
            Debug.Log("Collided with same color wall: " + trayTag);

            Transform nearestPlacement = FindNearestPlacement();
            if (nearestPlacement != null)
            {
                SnapToPlacement(nearestPlacement.position);
            }
        }
        else
        {
            Debug.Log("Collided with wall but NOT matching color!");

            MoveBackToStartGrid();
        }
    }

    Transform FindNearestPlacement()
    {
        if (trayPlacements == null || trayPlacements.Count == 0)
            return null;

        Transform nearest = null;
        float bestDistance = float.MaxValue;

        foreach (var placement in trayPlacements)
        {
            if (placement == null) continue;

            float distance = Vector3.Distance(transform.position, placement.position);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                nearest = placement;
            }
        }

        return nearest;
    }

    void SnapToPlacement(Vector3 placementPosition)
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.constraints = RigidbodyConstraints.FreezeAll;

        transform.position = new Vector3(placementPosition.x, 1.0f, placementPosition.z);
        snappedToPlacement = true;
        TrayManager.Instance.CheckAllTraysPlaced(); 
    }

    void MoveBackToStartGrid()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.constraints = RigidbodyConstraints.FreezeAll;

        transform.position = new Vector3(startGridPosition.x, 0.1f, startGridPosition.z);
        isMoving = false;
        isDragging = false;
    }
    public bool IsSnapped()
    {
        return snappedToPlacement;
    }

    bool IsPointerOverUI()
    {
#if UNITY_ANDROID || UNITY_IOS
        if (Input.touchCount > 0)
        {
            return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
        }
        else
        {
            return false;
        }
#else
    return EventSystem.current.IsPointerOverGameObject();
#endif
    }


}
