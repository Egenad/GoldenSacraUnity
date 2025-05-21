using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{

    public TileData currentTile;
    public Directions focusedTile;

    public Vector3 actualRotation;
    public Vector3 targetRotation;
    public Vector3 actualLocation;
    public Vector3 targetLocation;
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
    
    public void MoveForward() { /* ... */ }

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
}
