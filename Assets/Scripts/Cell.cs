using UnityEngine;

public struct Cell
{
    public enum Type
    {
        Empty,
        Number,
        Bomb,
    }

    public Type type;
    public Vector3Int position;
    public int value;
    public bool clicked;
    public bool flagged;
    public bool exploded;
}
