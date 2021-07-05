using System;
using UnityEngine;
using UnityEngine.UIElements;

public enum TypeOfFigure
{
    King,
    Queen,
    Rock,
    Bishop,
    Knight,
    Pawn
}
public enum SideOfFigure 
{ 
    White,
    Black
}


public class FigureBehavior : MonoBehaviour
{
    [SerializeField]
    private TypeOfFigure type;
    public TypeOfFigure Type
    {
        get {
            return type;
        }
    }
    [SerializeField]
    private SideOfFigure side;
    public SideOfFigure Side
    {
        get
        {
            return side;
        }
    }
    private bool isMoving;
    [SerializeField]
    private float speed;
    private AudioSource source;

    void Start()
    {
        isMoving = false;
        speed = 10.0f;
        source = GetComponent<AudioSource>();
    }
    private void Update()
    {
        if (isMoving)
        {
            Transform tileCollider = transform.parent.GetChild(0);
            Vector3 translation = tileCollider.localPosition - transform.localPosition;
            transform.Translate(translation * speed * Time.deltaTime, Space.World);
            if (Math.Abs(translation.x) <= 0.1f && Math.Abs(translation.z) <= 0.1f)
            {
                transform.localPosition = new Vector3 (0, 0.5f, 0);
                isMoving = false;
                source.Play();
                Debug.Log("Sound PLayed");
            }
        }
    }
    public void MoveToTheOrigin()
    {
        isMoving = true;
    }
    static public int[,] GetKingPossibleMoves()
    {
        int[,] possibleMoves = new int[,] { { 1, 0 }, { 1, 1 }, { 0, 1 }, { -1, 1 }, { -1, 0 }, { -1, -1 }, { 0, -1 }, { 1, -1 } };
        return possibleMoves;
    }

}