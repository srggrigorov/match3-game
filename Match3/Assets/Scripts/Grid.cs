using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Grid : MonoBehaviour
{
    private Camera cam;
    private AudioSource audioSource;
    public AudioClip scoreSound;
    public AudioClip chooseSound;
    private int touchNumber = 1;
    private int score = 0;
    private TextMeshProUGUI scoreText;
    private Animator scoreAnimator;
    private int ScoreAddedHash;
    public enum PieceType
    {
        EMPTY,
        NORMAL,
        COUNT,
    };
    [System.Serializable]
    public struct PiecePrefab
    {
        public PieceType type;
        public GameObject prefab;
    };
    public int xDim;
    public int yDim;
    public float fillTime;
    public PiecePrefab[] piecePrefabs;
    public GameObject backgroundPrefab;

    private Dictionary<PieceType, GameObject> piecePrefabDict;
    private GamePiece[,] pieces;
    private GamePiece firstPressedPiece;
    private GamePiece secondPressedPiece;

    void Start()
    {
        cam = FindObjectOfType<Camera>();
        audioSource = FindObjectOfType<AudioSource>();
        scoreText = FindObjectOfType<TextMeshProUGUI>();
        scoreAnimator = scoreText.GetComponent<Animator>();
        ScoreAddedHash = Animator.StringToHash("ScoreAdded");
        piecePrefabDict = new Dictionary<PieceType, GameObject>();
        
        for (int i = 0; i < piecePrefabs.Length; i++)
        {
            if (!piecePrefabDict.ContainsKey(piecePrefabs[i].type))
            {
                piecePrefabDict.Add(piecePrefabs[i].type, piecePrefabs[i].prefab);
            }
        }

        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                GameObject background = (GameObject)Instantiate(backgroundPrefab, GetWorldPosition(x, y), Quaternion.identity);
                background.transform.parent = transform;
            }
        }
        pieces = new GamePiece[xDim, yDim];
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                SpawnNewPiece(x, y, PieceType.EMPTY);
            }
        }

        StartCoroutine(Fill());

    }
    void Update()
    {
        if (Input.touchCount == 1) //Обработка касаний
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                Ray ray = cam.ScreenPointToRay(touch.position);
                RaycastHit hit;
                if (Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity))
                {
                    if (touchNumber == 1)
                    {
                        PressFirstPiece(hit.collider.GetComponent<GamePiece>());
                        touchNumber = 2;
                        audioSource.PlayOneShot(chooseSound, 1.0f);
                    }
                    else if (touchNumber == 2)
                    {
                        PressSecondPiece(hit.collider.GetComponent<GamePiece>());
                        touchNumber = 1;
                    }
                }
            }
        }
        else if (Input.GetMouseButtonDown(0)) //Обработка нажатий мыши
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity))
            {
                if (touchNumber == 1)
                {
                    PressFirstPiece(hit.collider.GetComponent<GamePiece>());
                    touchNumber = 2;
                    audioSource.PlayOneShot(chooseSound, 1.0f);
                }
                else if (touchNumber == 2)
                {
                    PressSecondPiece(hit.collider.GetComponent<GamePiece>());
                    touchNumber = 1;
                }
            }
        }
    }

    public IEnumerator Fill()
    {
        bool needsRefill = true;

        while (needsRefill)
        {
            yield return new WaitForSeconds(fillTime);
            while (FillStep())
            {
                yield return new WaitForSeconds(fillTime);
            }
            needsRefill = ClearAllValidMatches();
        }
    }

    public bool FillStep()
    {
        bool movedPiece = false;
        for (int y = yDim - 2; y >= 0; y--)
        {
            for (int x = 0; x < xDim; x++)
            {
                GamePiece piece = pieces[x, y];

                if (piece.IsMovable())
                {
                    GamePiece pieceBelow = pieces[x, y + 1];

                    if (pieceBelow.Type == PieceType.EMPTY)
                    {
                        Destroy(pieceBelow.gameObject);
                        piece.MovableComponent.Move(x, y + 1, fillTime);
                        pieces[x, y + 1] = piece;
                        SpawnNewPiece(x, y, PieceType.EMPTY);
                        movedPiece = true;
                    }
                }
            }
            for (int x = 0; x < xDim; x++)
            {
                GamePiece pieceBelow = pieces[x, 0];

                if (pieceBelow.Type == PieceType.EMPTY)
                {
                    Destroy(pieceBelow.gameObject);
                    GameObject newPiece = (GameObject)Instantiate(piecePrefabDict[PieceType.NORMAL], GetWorldPosition(x, -1), Quaternion.identity);
                    newPiece.transform.parent = transform;

                    pieces[x, 0] = newPiece.GetComponent<GamePiece>();
                    pieces[x, 0].Init(x, -1, this, PieceType.NORMAL);
                    pieces[x, 0].MovableComponent.Move(x, 0, fillTime);
                    pieces[x, 0].ColorComponent.SetColor((ColorPiece.ColorType)Random.Range(0, pieces[x, 0].ColorComponent.NumColors));
                    movedPiece = true;
                }
            }
        }
        return movedPiece;
    }

    public Vector2 GetWorldPosition(int x, int y)
    {
        return new Vector2(transform.position.x - xDim / 2.5f + x,
        transform.position.y + yDim / 2.5f - y);
    }

    public GamePiece SpawnNewPiece(int x, int y, PieceType type)
    {
        GameObject newPiece = (GameObject)Instantiate(piecePrefabDict[type], GetWorldPosition(x, y), Quaternion.identity);
        newPiece.transform.parent = transform;

        pieces[x, y] = newPiece.GetComponent<GamePiece>();
        pieces[x, y].Init(x, y, this, type);
        return pieces[x, y];
    }

    public bool IsAdjacent(GamePiece piece1, GamePiece piece2) //Функция проверяет, являются ли фишки соседними
    {
        return (piece1.X == piece2.X && (int)Mathf.Abs(piece1.Y - piece2.Y) == 1) ||
        (piece1.Y == piece2.Y && (int)Mathf.Abs(piece1.X - piece2.X) == 1);
    }

    public void SwapPieces(GamePiece piece1, GamePiece piece2) // Функция меняет фишки местами
    {
        if (piece1.IsMovable() && piece2.IsMovable())
        {
            pieces[piece1.X, piece1.Y] = piece2;
            pieces[piece2.X, piece2.Y] = piece1;

            if (GetMatch(piece1, piece2.X, piece2.Y, true) != null || GetMatch(piece2, piece1.X, piece1.Y, true) != null)
            {
                int piece1X = piece1.X;
                int piece1Y = piece1.Y;

                piece1.MovableComponent.Move(piece2.X, piece2.Y, fillTime);
                piece2.MovableComponent.Move(piece1X, piece1Y, fillTime);

                ClearAllValidMatches();

                StartCoroutine(Fill());
            }
            else
            {
                pieces[piece1.X, piece1.Y] = piece1;
                pieces[piece2.X, piece2.Y] = piece2;
                StartCoroutine(piece1.MovableComponent.NotSimilarAnim(piece2, fillTime / 2));
                StartCoroutine(piece2.MovableComponent.NotSimilarAnim(piece1, fillTime / 2));

            }
        }
    }

    public void PressFirstPiece(GamePiece piece)
    {
        firstPressedPiece = piece;
        firstPressedPiece.MovableComponent.AnimatorRef.SetBool(firstPressedPiece.MovableComponent.IsChosenHash, true);
    }

    public void PressSecondPiece(GamePiece piece)
    {
        secondPressedPiece = piece;
        firstPressedPiece.MovableComponent.AnimatorRef.SetBool(firstPressedPiece.MovableComponent.IsChosenHash, false);
        if (IsAdjacent(firstPressedPiece, secondPressedPiece))
        {
            SwapPieces(firstPressedPiece, secondPressedPiece);
        }
    }

    //Проверка наличия совпадений по цвету
    public List<GamePiece> GetMatch(GamePiece piece, int newX, int newY, bool swapping) //Swapping убирает лишние проверки при перемещении фишки
    {
        if (piece.IsColored())
        {
            ColorPiece.ColorType color = piece.ColorComponent.Color;
            List<GamePiece> horizontalPieces = new List<GamePiece>();
            List<GamePiece> verticalPieces = new List<GamePiece>();
            List<GamePiece> matchingPieces = new List<GamePiece>();

            //Проверка фишек по цветам по горизонтали
            horizontalPieces.Add(piece);

            for (int dir = 0; dir <= 1; dir++)
            {
                for (int xOffset = 1; xOffset < xDim; xOffset++)
                {
                    int x;

                    if (dir == 0) //Проверка налево
                    {
                        x = newX - xOffset;
                    }
                    else //Проверка направо
                    {
                        x = newX + xOffset;
                    }

                    if (x < 0 || x >= xDim) break;

                    if (pieces[x, newY].IsColored() && pieces[x, newY].ColorComponent.Color == color)
                    {
                        horizontalPieces.Add(pieces[x, newY]);
                    }
                    else break;

                }
            }
            if (horizontalPieces.Count >= 3)
            {
                for (int i = 0; i < horizontalPieces.Count; i++)
                {
                    matchingPieces.Add(horizontalPieces[i]);

                    //Если нашли совпадение по горизонтали, ищем совпадения по вертикали для каждой фишки
                    if (!swapping)
                    {
                        for (int dir = 0; dir <= 1; dir++)
                        {
                            for (int yOffset = 1; yOffset < yDim; yOffset++)
                            {
                                int y;

                                if (dir == 0) //Идем вверх
                                {
                                    y = newY - yOffset;
                                }
                                else //Идем вниз
                                {
                                    y = newY + yOffset;
                                }

                                if (y < 0 || y >= yDim) break;

                                if (pieces[horizontalPieces[i].X, y].IsColored() && pieces[horizontalPieces[i].X, y].ColorComponent.Color == color)
                                {
                                    verticalPieces.Add(pieces[horizontalPieces[i].X, y]);
                                }
                                else break;
                            }
                        }
                        if (verticalPieces.Count >= 2)
                        {
                            for (int j = 0; j < verticalPieces.Count; j++)
                            {
                                matchingPieces.Add(verticalPieces[j]);
                            }
                        }
                        else
                        {
                            verticalPieces.Clear();
                        }
                    }
                }
            }


            if (matchingPieces.Count >= 3)
            {
                return matchingPieces;
            }

            //Если не нашлось совпадений по горизонтали, идет проверка по вертикали
            horizontalPieces.Clear();
            verticalPieces.Clear();
            verticalPieces.Add(piece);

            for (int dir = 0; dir <= 1; dir++)
            {
                for (int yOffset = 1; yOffset < yDim; yOffset++)
                {
                    int y;

                    if (dir == 0) //Проверка наверх
                    {
                        y = newY - yOffset;
                    }
                    else //Проверка вниз
                    {
                        y = newY + yOffset;
                    }

                    if (y < 0 || y >= yDim) break;

                    if (pieces[newX, y].IsColored() && pieces[newX, y].ColorComponent.Color == color)
                    {
                        verticalPieces.Add(pieces[newX, y]);
                    }
                    else break;
                }
            }

            if (verticalPieces.Count >= 3)
            {
                for (int i = 0; i < verticalPieces.Count; i++)
                {
                    matchingPieces.Add(verticalPieces[i]);

                    //Если нашли совпадение по вертикали, ищем совпадения по горизонтали для каждой фишки
                    if (!swapping)
                    {
                        for (int dir = 0; dir <= 1; dir++)
                        {
                            for (int xOffset = 1; xOffset < xDim; xOffset++)
                            {
                                int x;

                                if (dir == 0) //Идем влево
                                {
                                    x = newX - xOffset;
                                }
                                else //Идем вправо
                                {
                                    x = newX + xOffset;
                                }

                                if (x < 0 || x >= xDim) break;

                                if (pieces[x, verticalPieces[i].Y].IsColored() && pieces[x, verticalPieces[i].Y].ColorComponent.Color == color)
                                {
                                    horizontalPieces.Add(pieces[x, verticalPieces[i].Y]);
                                }
                                else break;
                            }
                        }
                        if (horizontalPieces.Count >= 2)
                        {
                            for (int j = 0; j < horizontalPieces.Count; j++)
                            {
                                matchingPieces.Add(horizontalPieces[j]);
                            }
                        }
                        else
                        {
                            horizontalPieces.Clear();
                        }
                    }
                }
            }


            if (matchingPieces.Count >= 3)
            {
                return matchingPieces;
            }
        }
        return null;
    }

    public bool ClearAllValidMatches()
    {
        bool needsRefill = false;

        for (int y = 0; y < yDim; y++)
        {
            for (int x = 0; x < xDim; x++)
            {
                if (pieces[x, y].IsClearable())
                {
                    List<GamePiece> match = GetMatch(pieces[x, y], x, y, false);

                    if (match != null)
                    {
                        for (int i = 0; i < match.Count; i++)
                        {
                            if (ClearPiece(match[i].X, match[i].Y))
                                needsRefill = true;
                        }
                    }
                }
            }
        }
        return needsRefill;
    }
    public bool ClearPiece(int x, int y)
    {
        if (pieces[x, y].IsClearable() && !pieces[x, y].ClearableComponent.IsBeingCleared)
        {
            pieces[x, y].ClearableComponent.Clear();
            score++;
            scoreText.text = "Score: " + score;
            scoreAnimator.SetTrigger(ScoreAddedHash);
            audioSource.PlayOneShot(scoreSound, 1.0f);
            SpawnNewPiece(x, y, PieceType.EMPTY);

            return true;
        }
        return false;
    }

}