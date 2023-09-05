using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(TilemapCollider2D))]
[RequireComponent(typeof(CompositeCollider2D))]

public class Ladder : MonoBehaviour
{

    Collider2D _collider;
    Grid _grid;
    static LayerMask _ladderLayer, _groundLayer;

    void OnValidate()
    {
        if (gameObject.GetComponent<CompositeCollider2D>().geometryType != CompositeCollider2D.GeometryType.Polygons)
        {
            gameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
            gameObject.GetComponent<TilemapCollider2D>().usedByComposite = true;
            gameObject.GetComponent<CompositeCollider2D>().isTrigger = true;
            gameObject.GetComponent<CompositeCollider2D>().geometryType = CompositeCollider2D.GeometryType.Polygons;
            gameObject.GetComponent<CompositeCollider2D>().GenerateGeometry();
            gameObject.layer = LayerMask.GetMask("Ladder");
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        _collider = GetComponent<Collider2D>();
        _grid = GetComponentInParent<Grid>();
        _ladderLayer = LayerMask.GetMask("Ladder");
        _groundLayer = LayerMask.GetMask("Ground");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        ILadderInteractable ladderInteractable = (ILadderInteractable)collision.gameObject.GetComponent(typeof(ILadderInteractable));
        if (ladderInteractable == null) return;
        
        Collider2D overlapLeft = null;
        Collider2D overlapRight = null;
        float overlapTest = collision.bounds.extents.x + 0.1f;

        for (int i = -1; i < 2; i++)
        {
            overlapLeft = Physics2D.OverlapPoint((Vector2)collision.transform.position + new Vector2(-overlapTest, collision.bounds.extents.y * i), _ladderLayer);
            overlapRight = Physics2D.OverlapPoint((Vector2)collision.transform.position + new Vector2(overlapTest, collision.bounds.extents.y * i), _ladderLayer);
            if (overlapLeft != null || overlapRight != null) i = 2;
        }
        Vector2 collisionPosition;
        if (overlapLeft && !overlapRight) collisionPosition = (Vector2)collision.transform.position - Vector2.right * overlapTest;
        else if (!overlapLeft && overlapRight) collisionPosition = (Vector2)collision.transform.position + Vector2.right * overlapTest;
        else collisionPosition = collision.transform.position;
        Vector3Int cellPosition = _grid.LocalToCell(Physics2D.ClosestPoint(collisionPosition, _collider));
        float xPosition = _grid.GetCellCenterLocal(cellPosition).x;

        ladderInteractable.OnLadderEnter(xPosition);
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        ILadderInteractable ladderInteractable = (ILadderInteractable)collision.gameObject.GetComponent(typeof(ILadderInteractable));
        if (ladderInteractable == null) return;

        ladderInteractable.OnLadderExit();
    }

    public static Vector2? GetLadderBottom(Collider2D collider, float ladderXCoord)
    {
        Vector2 bottom = Vector2.zero;
        for (int i = -1; i < 2; i++)
        {
            bottom = new Vector2(ladderXCoord, collider.transform.position.y + collider.bounds.extents.y * i);
            if (Physics2D.OverlapPoint(bottom, _ladderLayer) == null)
            {
                if (i == 1)
                {
                    Debug.LogError($"There is no ladder at [{bottom.x}; {bottom.y}]");
                    return null;
                }
            }
            else
            {
                i = 2;
            }
        }
        for (int i = 0; i < 1000; i++)
        {
            if (Physics2D.OverlapPoint(bottom + Vector2.down * 0.2f * i, _ladderLayer) == null)
            {
                bottom += Vector2.down * 0.2f * i;
                i = 1000;
            }
            if (i == 999)
            {
                Debug.LogError($"Could not find the bottom of the ladder at [{bottom.x}; {bottom.y}]");
                return null;
            }
        }
        bottom = Physics2D.Raycast(bottom, Vector2.up, 0.6f, _ladderLayer).point;
        Collider2D ground = null;
        do
        {
            ground = Ladder.Overlap(bottom, collider);
            if (ground != null) bottom += Vector2.up * 0.2f;
        } while (ground != null);
        do
        {
            ground = Ladder.Overlap(bottom + Vector2.down * 0.001f, collider);
            if (ground == null) bottom += Vector2.down * 0.001f;
        } while (ground == null);
        return bottom;
    }

    public static Vector2? GetLadderTop(Collider2D collider, float ladderXCoord)
    {
        Vector2 top = Vector2.zero;
        for (int i = -1; i < 2; i++)
        {
            top = new Vector2(ladderXCoord, collider.transform.position.y + collider.bounds.extents.y * i);
            if (Physics2D.OverlapPoint(top, _ladderLayer) == null)
            {
                if (i == 1)
                {
                    Debug.LogError($"There is no ladder at [{top.x}; {top.y}]");
                    return null;
                }
            }
            else
            {
                i = 2;
            }
        }
        for (int i = 0; i < 1000; i++)
        {
            if (Physics2D.OverlapPoint(top + Vector2.up * 0.2f * i, _ladderLayer) == null)
            {
                top += Vector2.up * 0.2f * i;
                i = 1000;
            }
            if (i == 999)
            {
                Debug.LogError($"Could not find the top of the ladder at [{top.x}; {top.y}]");
                return null;
            }
        }
        top = Physics2D.Raycast(top, Vector2.down, 0.6f, _ladderLayer).point;
        top += Vector2.up * (collider.bounds.extents.y - 0.1f);

        Collider2D ground = null;
        Vector2 groundedTop = top;
        for (int i = 0; i < 1000; i++)
        {
            ground = Ladder.Overlap(groundedTop + Vector2.down * 0.001f, collider);
            if (ground == null) groundedTop += Vector2.down * 0.001f;
            else
            {
                i = 1000;
                top = groundedTop;
            }
        }

        return top;
    }

    #nullable enable
    static Collider2D? Overlap(Vector2 point, Collider2D collider)
    {
        if (collider.GetType() == typeof(CircleCollider2D))
        {
            return Physics2D.OverlapCircle(point, collider.bounds.extents.x, _groundLayer);
        }
        else
        {
            return Physics2D.OverlapBox(point, collider.bounds.extents * 2, 0, _groundLayer);
        }
    }
    #nullable disable
}
