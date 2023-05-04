using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Moves a character according to paths found by a pathfinding algorithm.
/// </summary>
public class Character : MonoBehaviour
{
    // The current tile the character is on.
    public GameObject CurrentTile { get; set; } = null;

    // The tile the character is moving to.
    public GameObject TargetTile { get; set; } = null;

    // The path the character is following.
    public Stack<NodeRecord> Path { get; set; } = new Stack<NodeRecord>();

    // speed of movement
    public float speed = 1.0f;

    public bool isReach = true;

    // Start is called before the first frame update
    void Start()
    {
        TargetTile = CurrentTile;
    }

    // Update is called once per frame
    void Update()
    {

        if (Path.Count > 0)
        {
            if (isReach)
            {
                // set target tile
                CurrentTile = TargetTile;
                TargetTile = Path.Pop().Tile;
            }
            
        }
        else
        {
            // TargetTile= CurrentTile;
        }

        // move kinematic
        MoveToTarget();
    }

    // Movement function
    void MoveToTarget()
    {
        isReach = false;

        // first orient to correct direction
        Vector3 targetVector = TargetTile.transform.position - CurrentTile.transform.position;
        UnityEngine.Debug.Log(targetVector);
        this.GetComponent<Rigidbody2D>().rotation = Vector2.SignedAngle(new Vector2(0, 1), new Vector2(targetVector.x, targetVector.y));

        // then move to target
        this.GetComponent<Rigidbody2D>().velocity = Vector3.Normalize(targetVector) * speed;

        // stop if reached
        if ((this.GetComponent<Rigidbody2D>().position - new Vector2(TargetTile.transform.position.x, TargetTile.transform.position.y)).magnitude < 0.1f)
        {
            this.GetComponent<Rigidbody2D>().velocity = new Vector2(0.0f, 0.0f);
            isReach = true;
            this.GetComponent<Rigidbody2D>().position = new Vector2(TargetTile.transform.position.x, TargetTile.transform.position.y);
        }


    }
}
