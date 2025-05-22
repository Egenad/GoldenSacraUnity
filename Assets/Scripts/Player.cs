using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{

    public AnimationCurve movementCurve;
    public float movementDuration = 0.25f;
    private Coroutine moveCoroutine;

    public TileData currentTile;
    public Directions focusedTile = Directions.Up;

    public Vector3 actualRotation;
    public Vector3 targetRotation;
    public Vector3 actualPosition;
    public Vector3 targetPosition;
    public bool moving;

    [Header("Gameplay")]
    int level;
    float health;
    float maxHealth;
    float baseDamage;
    bool myTurn;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        myTurn = true;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void TouchDelta(InputAction.CallbackContext context)
    {
        Vector2 delta = context.ReadValue<Vector2>();

        if (Mathf.Abs(delta.y) > Mathf.Abs(delta.x) && delta.y < -50f)
        {
            MoveForward();
        }
    }

    public void MoveForward()
    {
        TileData nextTile = null;

        foreach (NeighbourTile neighbour in currentTile.neighbours)
        {
            if (neighbour.direction == focusedTile)
            {
                nextTile = neighbour.neighbour;
                break;
            }
        }

        if (nextTile != null && !moving && myTurn)
        {
            FindFirstObjectByType<Player>();

            Tile nextTileGO = nextTile.tileGO.GetComponent<Tile>();

            if (nextTileGO != null && nextTileGO.canStepUp && !nextTile.reserved)
            {
                nextTile.reserved = true;
                currentTile.reserved = false;

                targetPosition = nextTileGO.transform.position;
                moving = true;
                currentTile = nextTile;

                MovePlayer();
            }
        }
    }

    public void TurnBack() { /* ... */ }

    public void Interact() { /* ... */ }

    public void TurnLeft()
    {

    }

    public void TurnRight()
    {
        /*Tile lastTile = currentTile.neighbours[focusedTile];

        if (!moving)
        {
            moving = true;

            actualRotation = targetRotation = playerScene.rotation;
            int focused = (int)focusedTile;

            if (right)
            {
                targetRotation *= Quaternion.Euler(0, 90, 0);
                focused = (focused + 1) % D_END;
            }
            else
            {
                targetRotation *= Quaternion.Euler(0, -90, 0);
                focused = (focused - 1 + D_END) % D_END;
            }

            focusedTile = (Directions)focused;

            // Start some kind of rotation tween (equiv. a turn_timeline)
            // E.g., you could use DOTween or Lerp in Update

            Tile nextTile = currentTile.neighbours[focusedTile];

            if (nextTile != null)
            {
                nextTile.SeeTile();
                SetEnemyHudVisibilityInTile(nextTile, true);

                if (lastTile != null)
                {
                    SetEnemyHudVisibilityInTile(lastTile, false);
                }
            }
        }*/
    }

    public void MovePlayer()
    {
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(MovePlayerCoroutine(targetPosition));
    }

    private IEnumerator MovePlayerCoroutine(Vector3 targetPosition)
    {
        Vector3 startPos = transform.position;

        float time = 0f;

        while (time < movementDuration)
        {
            float t = time / movementDuration;
            float curveT = movementCurve.Evaluate(t);

            transform.position = Vector3.Lerp(startPos, targetPosition, curveT);

            time += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
        moveCoroutine = null;
        moving = false;
        //myTurn = false;
        actualPosition = targetPosition;
    }
}
