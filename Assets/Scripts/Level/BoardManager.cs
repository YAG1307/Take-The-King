using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BoardManager : MonoBehaviour
{
    public List<LevelDataSO> availableLevels;
    public static event Action OnBlackKingCaptured;
    public static event Action OnWhiteKingCaptured;
    public static event Action OnOutOfMoves;

    public BoardHighlight boardHighlight;
    public MoveHistoryUI moveHistoryUI;
    public GameObject victoryUI;
    public Dialogue dialogueSystem;
    public GameUI GameUI;
    public GameObject promotionPanel;
    public float panelYOffset = -50f;

    public float enemyMoveDelay = 1.0f;

    private Vector2Int pendingPromotionPos;
    private PieceColor pendingPromotionColor;
    private Piece pendingPawnToDestroy;
    private bool isAwaitingPromotion;

    private Piece[,] board = new Piece[5, 5];
    private ChessBoard chessBoard;
    private EnemyAI enemyAI;
    private Piece selectedPiece;
    private bool isProcessingTurn;
    private int turnCount;
    private LevelDataSO currentLevelData;
    public GameObject fade;
    public TextMeshProUGUI fadeDialogue;
    public GameObject fadeNextButton;
    private bool isLevel20SoloPawnPhase = false;
    private bool isCutsceneActive = false;

    void Start()
    {
        chessBoard = FindAnyObjectByType<ChessBoard>();
        enemyAI = GetComponent<EnemyAI>() ?? gameObject.AddComponent<EnemyAI>();
        enemyAI.Setup(this);

        if (boardHighlight == null) boardHighlight = FindAnyObjectByType<BoardHighlight>();
        if (dialogueSystem == null) dialogueSystem = FindAnyObjectByType<Dialogue>();
        if (GameUI == null) GameUI = FindAnyObjectByType<GameUI>();
        if (promotionPanel != null) promotionPanel.SetActive(false);
        if (fade != null) fade.SetActive(false);

        int selectedLevel = PlayerPrefs.GetInt("SelectedLevel", 1);
        if (selectedLevel == 20)
        {
            SFX.Instance?.PlayEnding();
        }

        IncrementAttempts();
        LoadLevel(selectedLevel);
    }

    private void IncrementAttempts()
    {
        int attempts = PlayerPrefs.GetInt("TotalAttempts", 0);
        PlayerPrefs.SetInt("TotalAttempts", attempts + 1);
        PlayerPrefs.Save();
    }

    public void OnTileClicked(Vector2Int pos) => TileClicked(pos);
    public List<Vector2Int> GetLegalMoves(Piece piece) => LegalMoves(piece);
    public bool IsSqrAttacked(Vector2Int pos, PieceColor color) => SquareAttacked(pos, color);

    private void LoadLevel(int levelIndex)
    {
        if (availableLevels == null || levelIndex < 0 || levelIndex >= availableLevels.Count) return;

        currentLevelData = availableLevels[levelIndex];

        if (GameUI != null) GameUI.SetupLevelHeader(currentLevelData);

        foreach (PieceSpawnData data in currentLevelData.piecesToSpawn)
        {
            SpawnPiece(data.type, data.color, data.gridPosition);
        }

        isProcessingTurn = false;

        if (dialogueSystem != null)
        {
            if (currentLevelData.isTutorial)
            {
                isProcessingTurn = true;

                List<string> tutorialText = new List<string>
                {
                    "Welcome! This is no ordinary chessboard as you can see!",
                    "Your goal is to capture the enemy King in 3 moves or less.",
                    "Give it a try. You are playing white!"
                };

                dialogueSystem.PlayTutorialSequence(tutorialText, EnableInput);
            }
            else
            {
                dialogueSystem.ShowRandomIntro();
            }
        }
    }

    public void SpawnPiece(PieceType type, PieceColor color, Vector2Int gridPos)
    {
        Sprite loadedSprite = Resources.Load<Sprite>($"Pieces/{(color + "_" + type).ToLower()}");
        if (loadedSprite == null) return;

        GameObject pieceObj = new GameObject($"{color}_{type}");
        SpriteRenderer sr = pieceObj.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 10;
        sr.sprite = loadedSprite;

        Piece piece = pieceObj.AddComponent<Piece>();
        piece.type = type;
        piece.color = color;

        piece.SetGridPosition(gridPos, chessBoard.GetWorldPos(gridPos.x, gridPos.y));
        board[gridPos.x, gridPos.y] = piece;
    }

    public Piece GetPieceAt(Vector2Int gridPos)
    {
        if (gridPos.x >= 0 && gridPos.x < 5 && gridPos.y >= 0 && gridPos.y < 5)
            return board[gridPos.x, gridPos.y];
        return null;
    }

    public void TileClicked(Vector2Int gridPos)
    {
        if (isProcessingTurn || isAwaitingPromotion || isCutsceneActive) return;

        if (!HasLegalMoves(PieceColor.white))
        {
            OnOutOfMoves?.Invoke();
            EndGame(false);
            return;
        }

        Piece clickedPiece = GetPieceAt(gridPos);

        if (isLevel20SoloPawnPhase)
        {
            if (clickedPiece != null && (clickedPiece.color != PieceColor.white || clickedPiece.type != PieceType.pawn))
            {
                return;
            }
        }

        if (selectedPiece == null || (clickedPiece != null && clickedPiece.color == selectedPiece.color))
        {
            if (clickedPiece != null && clickedPiece.color == PieceColor.white)
            {
                selectedPiece = clickedPiece;
                boardHighlight?.HighlightedMoves(LegalMoves(selectedPiece), selectedPiece);
            }
        }
        else
        {
            if (LegalMoves(selectedPiece).Contains(gridPos))
            {
                MovePiece(selectedPiece, gridPos);
            }

            boardHighlight?.ClearHighlights();
            selectedPiece = null;
        }
    }

    public void MovePiece(Piece pieceToMove, Vector2Int newGridPos)
    {
        SFX.Instance?.PlayPieceMove();

        Vector2Int oldPos = pieceToMove.boardPosition;
        Piece targetPiece = board[newGridPos.x, newGridPos.y];
        bool isCapture = targetPiece != null;

        BoardMove(pieceToMove, oldPos, newGridPos, targetPiece);

        if (targetPiece != null && targetPiece.type == PieceType.king)
        {
            if (targetPiece.color == PieceColor.black)
            {
                BlackWins();
            }
            else
            {
                OnWhiteKingCaptured?.Invoke();
                int currentLevelIndex = PlayerPrefs.GetInt("SelectedLevel", 1);

                if (currentLevelIndex == 20 && !isLevel20SoloPawnPhase)
                {
                    StartCoroutine(Level20Scene());
                }
                else
                {
                    EndGame(false);
                }
            }
            return;
        }

        int activeLevel = PlayerPrefs.GetInt("SelectedLevel", 1);
        if (activeLevel == 20 && pieceToMove.color == PieceColor.black && !isLevel20SoloPawnPhase && !isCutsceneActive)
        {
            StartCoroutine(Level20Scene());
            return;
        }

        if (pieceToMove.color == PieceColor.white)
        {
            moveHistoryUI?.RecordMove(pieceToMove.type, oldPos, newGridPos, isCapture);
            turnCount++;

            if (pieceToMove.type == PieceType.pawn && (newGridPos.y == 4 || isLevel20SoloPawnPhase))
            {
                StartPromotion(pieceToMove, newGridPos);
                return;
            }

            NextTurn();
        }
    }

    private void StartPromotion(Piece pawn, Vector2Int gridPos)
    {
        isAwaitingPromotion = true;
        isProcessingTurn = true;

        pendingPawnToDestroy = pawn;
        pendingPromotionPos = gridPos;
        pendingPromotionColor = pawn.color;

        if (promotionPanel != null)
        {
            Vector3 tileWorldPos = chessBoard.GetWorldPos(gridPos.x, gridPos.y);
            Vector3 screenPos = Camera.main.WorldToScreenPoint(tileWorldPos);
            screenPos.y += panelYOffset;

            RectTransform panelRect = promotionPanel.GetComponent<RectTransform>();
            if (panelRect != null) panelRect.position = screenPos;

            promotionPanel.SetActive(true);
        }
    }

    public void PromoteToQueen() => Promote(PieceType.queen);

    public void PromoteToRook()
    {
        if (IsLevel20()) return;
        Promote(PieceType.rook);
    }

    public void PromoteToBishop()
    {
        if (IsLevel20()) return;
        Promote(PieceType.bishop);
    }

    public void PromoteToKnight()
    {
        if (IsLevel20()) return;
        Promote(PieceType.knight);
    }

    private bool IsLevel20()
    {
        return PlayerPrefs.GetInt("SelectedLevel", 1) == 20 || isLevel20SoloPawnPhase;
    }

    private void Promote(PieceType chosenType)
    {
        if (pendingPawnToDestroy != null) Destroy(pendingPawnToDestroy.gameObject);

        SpawnPiece(chosenType, pendingPromotionColor, pendingPromotionPos);

        if (promotionPanel != null) promotionPanel.SetActive(false);

        isAwaitingPromotion = false;

        if (isLevel20SoloPawnPhase)
        {
            BlackWins();
        }
        else
        {
            NextTurn();
        }
    }

    private void NextTurn()
    {
        if (!KingAlive(PieceColor.black))
        {
            BlackWins();
            return;
        }

        StartCoroutine(EnemyTurn());
    }

    private void BoardMove(Piece pieceToMove, Vector2Int oldPos, Vector2Int newGridPos, Piece targetPiece)
    {
        if (targetPiece != null) Destroy(targetPiece.gameObject);

        board[oldPos.x, oldPos.y] = null;
        board[newGridPos.x, newGridPos.y] = pieceToMove;
        pieceToMove.SetGridPosition(newGridPos, chessBoard.GetWorldPos(newGridPos.x, newGridPos.y));
    }

    private IEnumerator EnemyTurn()
    {
        isProcessingTurn = true;

        yield return new WaitForSeconds(enemyMoveDelay);

        if (isCutsceneActive) yield break;

        if (!KingAlive(PieceColor.black))
        {
            BlackWins();
            yield break;
        }

        if (!HasLegalMoves(PieceColor.black))
        {
            bool isKingInCheck = SquareAttacked(KingPos(PieceColor.black), PieceColor.white);

            if (isKingInCheck)
            {
                BlackWins();
            }
            else
            {
                OnOutOfMoves?.Invoke();
                EndGame(false);
            }
            yield break;
        }

        var (bestPiece, bestMove) = enemyAI.GetBestMove(board);
        if (bestPiece != null)
        {
            MovePiece(bestPiece, bestMove);
        }

        if (isCutsceneActive) yield break;

        if (turnCount >= 3 && !isLevel20SoloPawnPhase && PlayerPrefs.GetInt("SelectedLevel", 1) != 20)
        {
            OnOutOfMoves?.Invoke();
            EndGame(false);
            yield break;
        }

        isProcessingTurn = false;
    }

    private IEnumerator Level20Scene()
    {
        isProcessingTurn = true;
        isCutsceneActive = true;

        if (dialogueSystem != null)
        {
            dialogueSystem.PlaySideDialogue("Checkmate! I have won this match!");
            yield return new WaitForSeconds(3.5f);

            dialogueSystem.PlaySideDialogue("Wait... what is going on...?");
            yield return new WaitForSeconds(3.5f);
        }

        if (fade != null)
        {
            fade.SetActive(true);
            CanvasGroup canvasGroup = fade.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = fade.AddComponent<CanvasGroup>();

            canvasGroup.alpha = 0f;
            float fadeDuration = 1.5f;
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeDuration);
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }

        if (fadeDialogue != null)
        {
            fadeDialogue.gameObject.SetActive(true);
            StartCoroutine(TypewriterText(fadeDialogue, 0.08f));
        }

        if (fadeNextButton != null)
        {
            fadeNextButton.SetActive(true);
            Button btn = fadeNextButton.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(NextFade);
            }
        }
    }

    private void BlackWins()
    {
        OnBlackKingCaptured?.Invoke();
        EndGame(true);
    }

    private IEnumerator TypewriterText(TextMeshProUGUI textUI, float delay)
    {
        string originalText = textUI.text;
        textUI.text = "";

        foreach (char c in originalText)
        {
            textUI.text += c;
            yield return new WaitForSeconds(delay);
        }
    }

    public void NextFade()
    {
        SFX.Instance?.PlayButtonClick();
        if (fade != null) fade.SetActive(false);
        if (fadeNextButton != null) fadeNextButton.SetActive(false);

        isLevel20SoloPawnPhase = true;
        isCutsceneActive = false;
        isProcessingTurn = false;
    }

    public bool HasLegalMoves(PieceColor color)
    {
        for (int x = 0; x < 5; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                Piece p = board[x, y];
                if (p != null && p.color == color && LegalMoves(p).Count > 0)
                    return true;
            }
        }
        return false;
    }

    public List<Vector2Int> LegalMoves(Piece piece)
    {
        List<Vector2Int> legalMoves = new List<Vector2Int>();
        Vector2Int originalPos = piece.boardPosition;
        PieceColor opponentColor = (piece.color == PieceColor.white) ? PieceColor.black : PieceColor.white;

        foreach (Vector2Int targetPos in piece.GetPossibleMoves(board))
        {
            Piece targetPiece = board[targetPos.x, targetPos.y];
            board[targetPos.x, targetPos.y] = piece;
            board[originalPos.x, originalPos.y] = null;
            piece.boardPosition = targetPos;

            if (isLevel20SoloPawnPhase || !SquareAttacked(KingPos(piece.color), opponentColor))
            {
                legalMoves.Add(targetPos);
            }

            board[originalPos.x, originalPos.y] = piece;
            board[targetPos.x, targetPos.y] = targetPiece;
            piece.boardPosition = originalPos;
        }

        return legalMoves;
    }

    public bool SquareAttacked(Vector2Int pos, PieceColor attackingColor)
    {
        if (pos == new Vector2Int(-1, -1)) return false;

        for (int x = 0; x < 5; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                Piece p = board[x, y];
                if (p != null && p.color == attackingColor && p.GetPossibleMoves(board).Contains(pos))
                    return true;
            }
        }
        return false;
    }

    private Vector2Int KingPos(PieceColor color)
    {
        for (int x = 0; x < 5; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                Piece p = board[x, y];
                if (p != null && p.type == PieceType.king && p.color == color)
                    return new Vector2Int(x, y);
            }
        }
        return new Vector2Int(-1, -1);
    }

    private bool KingAlive(PieceColor color) => KingPos(color) != new Vector2Int(-1, -1);

    public void EnableInput() => isProcessingTurn = false;

    private void EndGame(bool win)
    {
        if (isCutsceneActive) return;

        isProcessingTurn = true;

        if (win)
        {
            SFX.Instance?.PlayVictory();
            int currentLevelIndex = PlayerPrefs.GetInt("SelectedLevel", 1);

            Progress.MarkLevelCompleted(currentLevelIndex);
            PlayerPrefs.SetInt($"Level_{currentLevelIndex}_Completed", 1);
            PlayerPrefs.Save();

            if (GameUI != null) GameUI.ShowWinUI();
            else if (victoryUI != null) victoryUI.SetActive(true);
        }
        else
        {
            SFX.Instance?.PlayLoss();

            if (dialogueSystem != null)
            {
                dialogueSystem.ShowRandomLoss();
            }

            Invoke(nameof(ResetLevel), 2.5f);
        }
    }

    public void NextLevel()
    {
        SFX.Instance?.PlayButtonClick();
        Time.timeScale = 1f;
        int currentLevelIndex = PlayerPrefs.GetInt("SelectedLevel", 1);

        if (currentLevelIndex >= 19)
        {
            LevelSelect();
            return;
        }

        int nextLevelIndex = currentLevelIndex + 1;
        if (nextLevelIndex < availableLevels.Count)
        {
            PlayerPrefs.SetInt("SelectedLevel", nextLevelIndex);
            PlayerPrefs.Save();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        else
        {
            LevelSelect();
        }
    }

    public void ResetLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void MainMenu()
    {
        SFX.Instance?.PlayButtonClick();
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void LevelSelect()
    {
        SFX.Instance?.PlayButtonClick();
        Time.timeScale = 1f;
        SceneManager.LoadScene("LevelSelect");
    }
}