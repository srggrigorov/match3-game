using System.Collections;
using UnityEngine;

public class MovablePiece : MonoBehaviour
{
    private GamePiece piece;
    private IEnumerator moveCoroutine;
    private Animator animator;
    public Animator AnimatorRef
    {
        get { return animator; }
    }
    private int isChosenHash;
    public int IsChosenHash
    {
        get { return isChosenHash; }
    }

    private void Awake()
    {
        piece = GetComponent<GamePiece>();
        animator = GetComponent<Animator>();
        isChosenHash = Animator.StringToHash("IsChosen");
    }

    void Start()
    {

    }


    void Update()
    {

    }

    public void Move(int newX, int newY, float time)
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }

        moveCoroutine = MoveCoroutine(newX, newY, time);
        StartCoroutine(moveCoroutine);
    }

    private IEnumerator MoveCoroutine(int newX, int newY, float time)
    {
        {
            piece.X = newX;
            piece.Y = newY;
            Vector3 startPos = transform.position;
            Vector3 endPos = piece.GridRef.GetWorldPosition(newX, newY);

            for (float t = 0; t <= 1 * time; t += Time.deltaTime)
            {
                piece.transform.position = Vector3.Lerp(startPos, endPos, t / time);
                yield return 0;
            }

            piece.transform.position = endPos;
        }
    }

    public IEnumerator NotSimilarAnim(GamePiece secondPiece, float time)
    {
        {


            Vector3 startPos = transform.position;
            Vector3 endPos = piece.GridRef.GetWorldPosition(secondPiece.X, secondPiece.Y);

            for (float t = 0; t <= 1 * time; t += Time.deltaTime)
            {
                piece.transform.position = Vector3.Lerp(startPos, endPos, t / time);
            }
            for (float t = 0; t <= 1 * time; t += Time.deltaTime)
            {
                piece.transform.position = Vector3.Lerp(endPos, startPos, t / time);
                yield return 0;
            }

            piece.transform.position = startPos;
        }
    }
}
