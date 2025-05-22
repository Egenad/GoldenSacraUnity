using System.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{

    public AnimationCurve movementCurve;
    public float movementDuration = 0.25f;
    private Coroutine moveCoroutine;
    private Coroutine rotateCoroutine;

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

        if (Mathf.Abs(delta.y) > Mathf.Abs(delta.x))
        {
            if (delta.y < -50f)
            {
                MoveForward();
            }
            else if (delta.y > 50f)
            {
                TurnBack();
            }
        }
        else
        {
            if (delta.x < -50f)
            {
                TurnToDirection(Directions.Left);
            }
            else if (delta.x > 50f)
            {
                TurnToDirection(Directions.Right);
            }
        }
    }

    private TileData ObtainTileInDirection(Directions d)
    {
        TileData result = null;

        foreach (NeighbourTile neighbour in currentTile.neighbours)
        {
            if (neighbour.direction == d)
            {
                result = neighbour.neighbour;
                break;
            }
        }
        return result;
    }

    public void MoveForward()
    {
        TileData nextTile = ObtainTileInDirection(focusedTile);

        if (nextTile != null && !moving && myTurn)
        {
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

    public void TurnBack()
    {
        int focused = (int)focusedTile + 2;

        if (focused >= (int)Directions.End)
        {
            focused -= (int)Directions.End;
        }

        if (!moving && myTurn)
        {
            moving = true;
            focusedTile = (Directions)focused;
            targetRotation = (transform.rotation * Quaternion.Euler(0, 180, 0)).eulerAngles;
            TurnPlayer();
        }
    }

    public void Interact() { /* ... */ }

    public void TurnLeft()
    {
        TurnToDirection(Directions.Left);
    }

    public void TurnRight()
    {
        TurnToDirection(Directions.Right);
    }

    private void TurnToDirection(Directions d)
    {
        if (!moving && myTurn)
        {
            moving = true;
            int focused;

            if (d == Directions.Right)
            {
                targetRotation = (transform.rotation * Quaternion.Euler(0, 90, 0)).eulerAngles;
                focused = (int)focusedTile + 1;
                if (focused >= (int)Directions.End)
                {
                    focused -= (int)Directions.End;
                }
            }
            else
            {
                targetRotation = (transform.rotation * Quaternion.Euler(0, -90, 0)).eulerAngles;
                focused = (int)focusedTile - 1;
                if (focused < 0)
                {
                    focused = (int)Directions.End - 1;
                }
            }
            focusedTile = (Directions)focused;
            TurnPlayer();
        }
    }

    private void MovePlayer()
    {
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(MovePlayerCoroutine(targetPosition));
    }

    private void TurnPlayer()
    {
        if (rotateCoroutine != null) StopCoroutine(rotateCoroutine);
        rotateCoroutine = StartCoroutine(RotatePlayerCoroutine(targetRotation));
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

    private IEnumerator RotatePlayerCoroutine(Vector3 targetRotation)
    {
        Quaternion startRot = transform.rotation;

        float time = 0f;

        while (time < movementDuration)
        {
            float t = time / movementDuration;
            float curveT = movementCurve.Evaluate(t);

            transform.rotation = Quaternion.Slerp(startRot, Quaternion.Euler(targetRotation), curveT);

            time += Time.deltaTime;
            yield return null;
        }

        transform.rotation = Quaternion.Euler(targetRotation);
        actualRotation = targetRotation;
        rotateCoroutine = null;
        moving = false;
    }
}
