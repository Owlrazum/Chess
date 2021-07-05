using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum StateOfTheGame
{
    Menu,
    Game,
    End
}
public enum StateOfTheTile
{ 
    Empty,
    UnderWhite,
    UnderBlack,
}
public enum TypeOfThreat
{
    NoThreat,
    ByWhite,
    ByBlack
}

public class Game_manager : MonoBehaviour
{
    [SerializeField]
    private GameObject Menu;
    
    [SerializeField]
    private GameObject WinPanel;
    [SerializeField]
    private TextMeshProUGUI WinMessage;
    [SerializeField]
    private AudioSource winSound;
    
    [SerializeField]
    private GameObject arenaObject;
    [SerializeField]
    private TileBehavior turnIndicator;

    [SerializeField]
    private AudioSource gameSound;

    private int rowWK; // white king
    private int colWK;

    private int rowBK; // black king
    private int colBK;

    private StateOfTheGame state;

    private const int rowCount = 8;
    private const int columnCount = 8;

    private TileBehavior clickedTile;
    private int rowCL;// rowOfClickedTile
    private int colCL;// columnOfClickedTile

    private TileBehavior selectedTile;
    private int rowOfSelectedTile;
    private int columnOfSelectedTile;

    private FigureBehavior currentFigure;
    private bool isFigureSelected;

    struct EnPassantData
    {
        EnPassantData(int index, bool firstBool, bool secondBool)
        {
            threatColumn = index;
            active = firstBool;
            shouldClean = secondBool;
        }
        public int threatColumn;
        public bool active;
        public bool shouldClean;
    };

    EnPassantData enPassantDataWhite;
    EnPassantData enPassantDataBlack;

    private List<List<TileBehavior>> arena;
    private List<List<StateOfTheTile>> statesOfTheTiles;
 //   private List<List<TypeOfThreat>> threatsOnTheTiles;
    private List<List<int>> highlightedTiles;
    private int[] directions;

    private bool isWhiteTurn;
    private bool isWhiteWins;

    private void Start()
    {
        state = StateOfTheGame.Game;
        isFigureSelected = false;

        isWhiteTurn = true;

        rowWK = 0;
        colWK = 3;

        rowBK = 7;
        colBK = 3;

        InitializeContainers();

        PlayerInput.current.OnCanselButtonDown += ProcessCanselButtonDown;
        PlayerInput.current.OnClick            += ProcessClick;
        PlayerInput.current.OnClickNoWhere     += ProcessClickNoWhere;
        PlayerInput.current.OnClickFigure      += ProcessClickCollider;
        PlayerInput.current.OnClickTile        += ProcessClickCollider;
    }
    private void ProcessCanselButtonDown()
    {
        if (state == StateOfTheGame.Game)
        {
            state = StateOfTheGame.Menu;
            Menu.SetActive(true);
        } else
        {
            state = StateOfTheGame.Game;
            Menu.SetActive(false);            
        }
    }
    private void InitializeContainers()
    {
        arena = new List<List<TileBehavior>>();
        statesOfTheTiles = new List<List<StateOfTheTile>>();
        highlightedTiles = new List<List<int>>();
   //     threatsOnTheTiles = new List<List<TypeOfThreat>>();
        int index;
        //                     012345678901234567
        String pathTemplate = "?th row/?th tile";
        for (int i = 1; i <= 8; i++)
        {
            arena.Add(new List<TileBehavior>());
            statesOfTheTiles.Add(new List<StateOfTheTile>());
            highlightedTiles.Add(new List<int>());
      //      threatsOnTheTiles.Add(new List<TypeOfThreat>());
            index = i - 1;
            for (int j = 1; j <= 8; j++)
            {
                char[] pathChar = pathTemplate.ToCharArray();
                pathChar[0] = (char)(i + 48);
                pathChar[8] = (char)(j + 48);
                String path = new String(pathChar);
                GameObject temp = arenaObject.transform.Find(path).gameObject;
                arena[index].Add(temp.GetComponent<TileBehavior>());
                if (i == 1 || i == 2)
                {
                    statesOfTheTiles[index].Add(StateOfTheTile.UnderWhite);
          //          if (i == 1 && (j == 1 || j == 8))
            //        {
              //          threatsOnTheTiles[index].Add(TypeOfThreat.NoThreat);
                //    } else
                  //  {
                    //    threatsOnTheTiles[index].Add(TypeOfThreat.ByWhite);
                    //}
                }
                else if (i == 7 || i == 8)
                {
                    statesOfTheTiles[index].Add(StateOfTheTile.UnderBlack);
                  //  if (i == 8 && (j == 1 || j == 8))
//                    {
  //                      threatsOnTheTiles[index].Add(TypeOfThreat.NoThreat);
    //                }
      //              else
        //            {
          //              threatsOnTheTiles[index].Add(TypeOfThreat.ByBlack);
            //        }
                } else
                {
                    statesOfTheTiles[index].Add(StateOfTheTile.Empty);
                }
    //            if (i == 3)
      //          {
        //            threatsOnTheTiles[index].Add(TypeOfThreat.ByWhite);
          //      }
            //    else if (i == 6)
              //  {
                //    threatsOnTheTiles[index].Add(TypeOfThreat.ByBlack);
                //}
            }
        }
        directions = new int[8];
        index = 0;
        for (int j = -columnCount; index < 6; j += 2 * columnCount)
        {
            for (int i = -1; i < 2; i++)
            {
                directions[index++] = j + i;
            }
        }
        directions[index++] = -1;
        directions[index] = 1;

        string message = "";
     //   for (int i = 0; i < threatsOnTheTiles.Count; i++)
       // {
         //   for (int j = 0; j < threatsOnTheTiles[i].Count; j++)
           // {
             //   message += threatsOnTheTiles[i][j] + " ";
            //}
            //message += '\n';
       // }
        //Debug.Log(message);
        
        // Debug.Log(highlightedTiles.Count);
    }
    private void ProcessClick()
    {
        if (state == StateOfTheGame.End)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
        }
    }
    private void ProcessClickNoWhere()
    {
        if (state == StateOfTheGame.Game)
        {
            Unselect();
        }
    }
    private void Unselect()
    {
        if (isFigureSelected)
        {
            selectedTile.Default();
            for (int i = 0; i < highlightedTiles.Count; i++)
            {
                for (int j = 0; j < highlightedTiles[i].Count; j++)
                {
                    arena[i][highlightedTiles[i][j]].Default();
                }
                highlightedTiles[i].Clear();
            }
            isFigureSelected = false;
        }
    }
    private void ProcessClickCollider(Collider collider)
    {
        if (state != StateOfTheGame.Game)
        {
            return;
        }
    //    OutputThreatsOfTheTiles();
        clickedTile = collider.GetComponentInParent<TileBehavior>();

        String columnName = clickedTile.transform.name;
        String rowName = clickedTile.transform.parent.name;
        colCL = columnName.ToCharArray()[0] - 48 - 1;
        rowCL = rowName.ToCharArray()[0] - 48 - 1;
        //  Debug.Log("Proccessing row " + rowCL + " column " + colCL);
        if (isFigureSelected)
        {
            //  Debug.Log("Figure Selected");
            bool isMoveNeeded = highlightedTiles[rowCL].Contains(colCL);
            if (isMoveNeeded)
            {
                Unselect();
                MoveFigure();
            }
            else
            {
                Unselect();
                if (statesOfTheTiles[rowCL][colCL] == StateOfTheTile.UnderBlack
                    || statesOfTheTiles[rowCL][colCL] == StateOfTheTile.UnderWhite)
                {
                    Select();
                }
            }
        }
        else
        {
            //   Debug.Log("Figure Unselected");
            //  Debug.Log(statesOfTheTiles[rowCL][colCL]);
            if (statesOfTheTiles[rowCL][colCL] == StateOfTheTile.UnderBlack
                || statesOfTheTiles[rowCL][colCL] == StateOfTheTile.UnderWhite)
            {
                //    Debug.Log("Selecting Figure");
                Select();
            }

        }

    }
    private void Select()
    {
        currentFigure = clickedTile.GetComponentInChildren<FigureBehavior>();
        if ((isWhiteTurn && currentFigure.Side == SideOfFigure.Black) ||
             (!isWhiteTurn && currentFigure.Side == SideOfFigure.White))
        {
            return;
        }
        clickedTile.Select();
        selectedTile = clickedTile;
        rowOfSelectedTile = rowCL;
        columnOfSelectedTile = colCL;
        isFigureSelected = true;
        HighlightPossibleMoves();
    }
    private void HighlightPossibleMoves()
    {
        {
            if (isWhiteTurn)
            {
                if (enPassantDataBlack.active)
                {
                    Debug.Log("enPassantBlack should clean");
                    enPassantDataBlack.shouldClean = true;
                }
                else if (enPassantDataBlack.shouldClean)
                {
                    Debug.Log("enPassantBlack Data cleaned");
                    enPassantDataBlack.threatColumn = -1;
                    enPassantDataBlack.active = false;
                    enPassantDataBlack.shouldClean = false;
                }
            }
            else
            {
                if (enPassantDataWhite.active)
                {
                    enPassantDataWhite.shouldClean = true;
                }
                else if (enPassantDataWhite.shouldClean)
                {
                    Debug.Log("enPassantWhite Data cleaned");
                    enPassantDataWhite.threatColumn = -1;
                    enPassantDataWhite.active = false;
                    enPassantDataWhite.shouldClean = false;
                }
            }
        }
        int rowTmp = rowCL;
        int colTmp = colCL;
        switch (currentFigure.Type)
        {
            case TypeOfFigure.King:
                {
                    int[,] moves = FigureBehavior.GetKingPossibleMoves();
                    StateOfTheTile enemyTile = isWhiteTurn ? StateOfTheTile.UnderBlack : StateOfTheTile.UnderWhite;
                    for (int i = 0; i < moves.Length / 2; i++)
                    {
                        //Debug.Log(moves[i, 0] + " " + moves[i, 1]);
                        if (moves[i, 0] == 1)
                        {
                            if (rowTmp != rowCount - 1)
                            {
                                rowTmp++;
                                Debug.Log("row increased");
                            } else
                            {
                                continue;
                            }
                        }
                        else if (moves[i, 0] == -1)
                        {
                            if (rowTmp > 0)
                            {
                                rowTmp--;
                                Debug.Log("row decreased");
                            }
                            else
                            {
                                continue;
                            }
                        }
                        if (moves[i, 1] == 1)
                        {
                            if (colTmp != columnCount - 1)
                            {
                                colTmp++;
                                Debug.Log("column increased");
                            } else
                            {
                                continue;
                            }
                        }
                        else if (moves[i, 1] == -1)
                        {
                            if (colTmp > 0)
                            {
                                colTmp--;
                                Debug.Log("column decreased");
                            }
                            else
                            {
                                continue;
                            }
                        }
                        if (statesOfTheTiles[rowTmp][colTmp] == StateOfTheTile.Empty)
                        {
                            arena[rowTmp][colTmp].Allow();
                            highlightedTiles[rowTmp].Add(colTmp);
                        }
                        else if (statesOfTheTiles[rowTmp][colTmp] == enemyTile)
                        {
                            arena[rowTmp][colTmp].Threat();
                            highlightedTiles[rowTmp].Add(colTmp);
                        }
                        rowTmp = rowCL;
                        colTmp = colCL;
                    }
                }
                break;
            case TypeOfFigure.Pawn:
                PawnHighlight(rowCL, colCL);
                break;
            case TypeOfFigure.Knight:
                KnightHighlight(rowCL, colCL);
                break;
            case TypeOfFigure.Bishop:
                BishopHighlight(rowCL, colCL);
                break;
            case TypeOfFigure.Rock:
                RockHighlight(rowCL, colCL);
                break;
            case TypeOfFigure.Queen:
                BishopHighlight(rowCL, colCL);
                RockHighlight(rowCL, colCL);
                break;
        }
    }
    private void PawnHighlight(int baseRow, int baseCol, bool isCheckingStates = false)
    {
        int rowModifier = isWhiteTurn ? 1 : -1;
        StateOfTheTile enemyTile = isWhiteTurn ? StateOfTheTile.UnderBlack : StateOfTheTile.UnderWhite;

        int rowIndex = baseRow + rowModifier;
        int colIndex = baseCol - 1;
        bool isValidColumn = colIndex >= 0 && colIndex <= columnCount - 1;

        if (isValidColumn)
        {
            if (statesOfTheTiles[rowIndex][colIndex] == enemyTile)
            {
                if (rowIndex == rowWK && colIndex == colWK
                    || rowIndex == rowBK && colIndex == colBK)
                {
                    arena[rowIndex][colIndex].ThreatKing();
                }
                else
                {
                    arena[rowIndex][colIndex].Threat();
                }
                highlightedTiles[rowIndex].Add(colIndex);
            }
        }

        rowIndex = baseRow + rowModifier;
        colIndex = baseCol;

        if (statesOfTheTiles[rowIndex][colIndex] == StateOfTheTile.Empty)
        {
            arena[rowIndex][colIndex].Allow();
            //  Debug.Log("Attention " + highlightedTiles.Count);
            highlightedTiles[rowIndex].Add(colIndex);

            bool isPawnMoved = isWhiteTurn ? baseRow != 1 : baseRow != 6;
            if (!isPawnMoved)
            {
                rowIndex = baseRow + rowModifier * 2;
                colIndex = baseCol;

                if (statesOfTheTiles[rowIndex][colIndex] == StateOfTheTile.Empty)
                {
                    arena[rowIndex][colIndex].Allow();
                    highlightedTiles[rowIndex].Add(colIndex);
                }
            }
        }

        rowIndex = baseRow + rowModifier;
        colIndex = baseCol + 1;
        isValidColumn = colIndex >= 0 && colIndex <= columnCount - 1;
        if (isValidColumn)
        {
            if (statesOfTheTiles[rowIndex][colIndex] == enemyTile)
            {
                if (rowIndex == rowWK && colIndex == colWK
                    || rowIndex == rowBK && colIndex == colBK)
                {
                    arena[rowIndex][colIndex].ThreatKing();
                }
                else
                {
                    arena[rowIndex][colIndex].Threat();

                }
                highlightedTiles[rowIndex].Add(colIndex);
            }
        }
        checkEnPassantThreatHighlight();
    }
    private void checkEnPassantThreatHighlight()
    {
        // Debug.Log(indexOfCurrentTile);
        bool isOnTheRowNeededWhite = rowCL == 4;
        bool isOnTheRowNeededBlack = rowCL == 3;
        if ((colCL == enPassantDataBlack.threatColumn - 1
            || colCL == enPassantDataBlack.threatColumn + 1)
            && isWhiteTurn
            && isOnTheRowNeededWhite)
        {
            if (enPassantDataBlack.active)
            {
                //    Debug.Log("Entered Black " + enPassantDataBlack.threatColumn);

                int threatRow = 5;
                int threatCol = enPassantDataBlack.threatColumn;
                arena[threatRow][threatCol].Threat();
                highlightedTiles[threatRow].Add(threatCol);
            }
        }
        else if ((colCL == enPassantDataWhite.threatColumn - 1
            || colCL == enPassantDataWhite.threatColumn + 1)
            && !isWhiteTurn
            && isOnTheRowNeededBlack)
        {
            if (enPassantDataWhite.active)
            {
                //    Debug.Log("Entered White " + enPassantDataWhite.threatColumn);
                int threatRow = 2;
                int threatCol = enPassantDataWhite.threatColumn;
                arena[threatRow][threatCol].Threat();
                highlightedTiles[threatRow].Add(threatCol);
            }
        }
        else
        {
            return;
        }
    }
    private enum CheckEnPassantMoveResult
    {
        Empty,
        Activation,
        Capture
    }
    private  CheckEnPassantMoveResult checkEnPassantMove()
    {
        if (currentFigure.Type == TypeOfFigure.Pawn)
        {
            if (Math.Abs(rowCL - rowOfSelectedTile) == 2)
            {
                if (isWhiteTurn)
                {
             //       Debug.Log("Activated White");
                    enPassantDataWhite.threatColumn = colCL;
                    enPassantDataWhite.active = true;
                    return CheckEnPassantMoveResult.Activation;
                }
                else
                {
              //      Debug.Log("Activated Black");
                    enPassantDataBlack.threatColumn = colCL;
                    enPassantDataBlack.active = true;
                    return CheckEnPassantMoveResult.Activation;
                }
            }
            else if (isWhiteTurn)
            {
                int threatRow = 5;
                int threatCol = enPassantDataBlack.threatColumn;
                bool ClickEqualsThreat = rowCL == threatRow && colCL == threatCol;
                if (enPassantDataBlack.active && ClickEqualsThreat)
                {
                    Debug.Log("En Passant capture of Black happened");
                    statesOfTheTiles[rowCL][colCL] = StateOfTheTile.UnderWhite;
                    rowCL--;
                    enPassantDataBlack.active = false;
                    enPassantDataBlack.shouldClean = true;
                    return CheckEnPassantMoveResult.Capture;
                }
                else if (enPassantDataBlack.shouldClean)
                {
                    enPassantDataBlack.active = false;
                }
            }
            else if (!isWhiteTurn)
            {
                int threatRow = 2;
                int threatCol = enPassantDataWhite.threatColumn;
                bool ClickEqualsThreat = rowCL == threatRow && colCL == threatCol;
                if (enPassantDataWhite.active && ClickEqualsThreat)
                {
              //      Debug.Log("En Passant capture of White happened");
                    statesOfTheTiles[rowCL][colCL] = StateOfTheTile.UnderBlack;
                    rowCL++;
                    enPassantDataWhite.active = false;
                    enPassantDataWhite.shouldClean = true;
                    return CheckEnPassantMoveResult.Capture;
                }
                else if (enPassantDataWhite.shouldClean)
                {
                    enPassantDataWhite.active = false;
                }
            }
        }
        return CheckEnPassantMoveResult.Empty;
    }
    private void KnightHighlight(int baseRow, int baseCol)
    {
        StateOfTheTile enemyTile = isWhiteTurn ? StateOfTheTile.UnderBlack : StateOfTheTile.UnderWhite;
        List<int> possibleMoves = new List<int>();
        int baseIndex = baseRow * columnCount + baseCol;

        if (baseCol != 7 && baseRow < 6)  possibleMoves.Add(baseIndex + columnCount * 2 + 1);
        if (baseCol != 7 && baseRow > 1)  possibleMoves.Add(baseIndex - columnCount * 2 + 1);
        if (baseCol < 6  && baseRow != 7) possibleMoves.Add(baseIndex + columnCount     + 2);
        if (baseCol < 6  && baseRow != 0) possibleMoves.Add(baseIndex - columnCount     + 2);
        if (baseCol > 1  && baseRow != 7) possibleMoves.Add(baseIndex + columnCount     - 2);
        if (baseCol > 1  && baseRow != 0) possibleMoves.Add(baseIndex - columnCount     - 2);
        if (baseCol != 0 && baseRow < 6)  possibleMoves.Add(baseIndex + columnCount * 2 - 1);
        if (baseCol != 0 && baseRow > 1)  possibleMoves.Add(baseIndex - columnCount * 2 - 1);

        for (int i = 0; i < possibleMoves.Count; i++)
        {
            int indexTemp = possibleMoves[i];
            if (indexTemp >= 0 && indexTemp < columnCount * rowCount)
            {
                int rowTmp = indexTemp / 8;
                int colTmp = indexTemp % 8;
                if (statesOfTheTiles[rowTmp][colTmp] == enemyTile)
                {
                    if (rowTmp == rowWK && colTmp == colWK
                        || rowTmp == rowBK && colTmp == colBK)
                    {
                        arena[rowTmp][colTmp].ThreatKing();
                    }
                    else
                    {
                        arena[rowTmp][colTmp].Threat();
                    }
                    highlightedTiles[rowTmp].Add(colTmp);
                }
                else if (statesOfTheTiles[rowTmp][colTmp] == StateOfTheTile.Empty)
                {
                        arena[rowTmp][colTmp].Allow();
                        highlightedTiles[rowTmp].Add(colTmp);
                }
            }
        }
    } 
    private void BishopHighlight (int baseRow, int baseCol)
    {
        bool metObstacle = false;

        int rowTmp = baseRow;
        int colTmp = baseCol;

        int upDown = 1;
        int leftRight = -1;
        StateOfTheTile enemyTile = isWhiteTurn ? StateOfTheTile.UnderBlack : StateOfTheTile.UnderWhite;
        while (true) 
        {
            while(!metObstacle)
            {
                if (!((rowTmp + upDown) >= 0 && (rowTmp + upDown) <= rowCount - 1 
                   && (colTmp + leftRight) >= 0 && (colTmp + leftRight) <= columnCount - 1))
                {
                    break;
                }
                rowTmp += upDown;
                colTmp += leftRight;
                if (statesOfTheTiles[rowTmp][colTmp] == StateOfTheTile.Empty)
                {
                    arena[rowTmp][colTmp].Allow();
                    highlightedTiles[rowTmp].Add(colTmp);
                }
                else if (statesOfTheTiles[rowTmp][colTmp] == enemyTile)
                {
                    if (rowTmp == rowWK && colTmp == colWK
                        || rowTmp == rowBK && colTmp == colBK)
                    {
                        arena[rowTmp][colTmp].ThreatKing();
                    }
                    else
                    {
                        arena[rowTmp][colTmp].Threat();
                    }
                    highlightedTiles[rowTmp].Add(colTmp);
                    metObstacle = true;
                }
                else
                {
                    metObstacle = true;
                }
            }
            rowTmp = baseRow;
            colTmp = baseCol;
            metObstacle = false;
            if (leftRight == -1 && upDown == 1)
            {
                leftRight = 1;
            }
            else if (leftRight == 1 && upDown == 1)
            {
                upDown = -1;
            }
            else if (leftRight == 1 && upDown == -1)
            {
                leftRight = -1;
            }
            else
            {
                break;
            }
        }
    }
    private void RockHighlight(int baseRow, int baseCol)
    {
        StateOfTheTile enemyTile = isWhiteTurn ? StateOfTheTile.UnderBlack : StateOfTheTile.UnderWhite;
        bool metObstacle = false;
        int rowTmp = baseRow;
        int colTmp = baseCol;
        Debug.Log(rowTmp + " " + colTmp);
        int upDown = 0;
        int leftRight = -1;
        while (true)
        {
            while (!metObstacle)
            {
                if (!((rowTmp + upDown) >= 0 && (rowTmp + upDown) <= rowCount - 1
                      && (colTmp + leftRight) >= 0 && (colTmp + leftRight) <= columnCount - 1))
                {
                    break;
                }
                if (upDown == 0)
                {
                    colTmp += leftRight;
                }
                else
                {
                    rowTmp += upDown;
                }
                if (statesOfTheTiles[rowTmp][colTmp] == StateOfTheTile.Empty)
                {
                    arena[rowTmp][colTmp].Allow();
                    highlightedTiles[rowTmp].Add(colTmp);
                }
                else if (statesOfTheTiles[rowTmp][colTmp] == enemyTile)
                {
                    if (rowTmp == rowWK && colTmp == colWK
                        || rowTmp == rowBK && colTmp == colBK)
                    {
                        arena[rowTmp][colTmp].ThreatKing();
                    }
                    else
                    {
                        arena[rowTmp][colTmp].Threat();
                    }
                    highlightedTiles[rowTmp].Add(colTmp);
                    metObstacle = true;
                }
                else
                {
                    metObstacle = true;
                }
            }
            rowTmp = baseRow;  
            colTmp = baseCol;
            metObstacle = false;
            if (leftRight == -1)
            {
                leftRight = 0;
                upDown = 1;
            } else if (upDown == 1)
            {
                upDown = 0;
                leftRight = 1;
            } else if (leftRight == 1)
            {
                leftRight = 0;
                upDown = -1;
            } else
            {
                break;
            }
        }
    }
    private void MoveFigure()
    {
        currentFigure.transform.SetParent(clickedTile.transform);
        currentFigure.MoveToTheOrigin();
        statesOfTheTiles[rowOfSelectedTile][columnOfSelectedTile] = StateOfTheTile.Empty;
        CheckEnPassantMoveResult checkEnPassantMoveResult = checkEnPassantMove();
        if (isWhiteTurn)
        {
            if (checkEnPassantMoveResult == CheckEnPassantMoveResult.Capture)
            {
                DestroyClickedFigure();
                statesOfTheTiles[rowCL][colCL] = StateOfTheTile.Empty;
            } else
            {
                if (rowOfSelectedTile == rowWK && columnOfSelectedTile == colWK)
                {
                    rowWK = rowCL;
                    colWK = colCL;
                }
                if (statesOfTheTiles[rowCL][colCL] == StateOfTheTile.UnderBlack)
                {
                    DestroyClickedFigure();
                    if (rowCL == rowBK && colCL == colBK)
                    {
                        EndGame(true);
                    }
                }
                statesOfTheTiles[rowCL][colCL] = StateOfTheTile.UnderWhite;
            }
            isWhiteTurn = false;
            turnIndicator.TestBlack();
        } else
        {
            if (checkEnPassantMoveResult == CheckEnPassantMoveResult.Capture) 
            {
                DestroyClickedFigure();
                statesOfTheTiles[rowCL][colCL] = StateOfTheTile.Empty;
            } else
            {
                if (rowOfSelectedTile == rowBK && columnOfSelectedTile == colBK)
                {
                    rowBK = rowCL;
                    colBK = colCL;
                }
                if (statesOfTheTiles[rowCL][colCL] == StateOfTheTile.UnderWhite)
                {
                    DestroyClickedFigure();
                    if (rowCL == rowWK && colCL == colWK)
                    {
                        EndGame(false);
                    }
                }
                statesOfTheTiles[rowCL][colCL] = StateOfTheTile.UnderBlack;
            }
            
            isWhiteTurn = true;
            turnIndicator.TestWhite();
        }
    }
    private void DestroyClickedFigure()
    {
        var capturedFigure = arena[rowCL][colCL].transform.GetChild(1);
        GameObject.Destroy(capturedFigure.gameObject);
    }
    private void EndGame(bool isWhitesWin)
    {
        state = StateOfTheGame.End;
        string message = isWhitesWin ? "WHITES WIN!" : "BLACKS WIN!";
        WinMessage.SetText(message);
        WinPanel.SetActive(true);
        winSound.Play();
        gameSound.Stop();
    }

}
/*
 * 
 * 
 *     private void OutputThreatsOfTheTiles()
 {
     string message = "ThreatsOnTheTiles:\n";
     for (int i = 0; i < threatsOnTheTiles.Count; i++)
     {
         for (int j = 0; j < threatsOnTheTiles[i].Count; j++)
         {
             message += threatsOnTheTiles[i][j] + " ";
         }
         message += '\n';
     }
     Debug.Log(message);
 }
 * 
 * 
 * 
 * private void IsCheckHappened(int kingRow, int kingCol)
 {
     for (int i = 0; i < 8; i++) // 8 directions
     {
         int prevIndex = kingIndex;
         for (int j = 0; j < 1000000; j++)
         {
             kingIndex += directions[i];
             if (kingIndex < 0 || kingIndex >= rowCount * columnCount - 1)
             {
                 break;
             }
             // Quenn is included in both bishop and rock check
             if (i == 0 || i == 2 || i == 3 || i == 5) // diagonal directions, bishop check
             { 
                 if (Math.Abs(kingIndex / 8 - prevIndex / 8) != 1)
                 {
                     break;
                 }
                 StateOfTheTile stateOfTheTile = statesOfTheTiles[kingIndex];
                 if (stateOfTheTile == StateOfTheTile.UnderWhite && isWhiteTurn
                     || stateOfTheTile == StateOfTheTile.UnderBlack && !isWhiteTurn)
                 {
                     break;
                 }
                 else if (stateOfTheTile == StateOfTheTile.UnderBlack && isWhiteTurn)
                 {
                     FigureBehavior blackfigure = arena[kingIndex].GetComponentInChildren<FigureBehavior>();
                 } 
                 else if (stateOfTheTile == StateOfTheTile.UnderWhite && !isWhiteTurn)
                 {
                 }
             }
             else // horizontal vertical directions, rock check
             {
                 if ((i == 6 || i == 7) && Math.Abs(kingIndex / 8 - prevIndex / 8) != 0) // horizontal not on the same row
                 {
                     break;
                 }
             }
         }
     }
 } */
