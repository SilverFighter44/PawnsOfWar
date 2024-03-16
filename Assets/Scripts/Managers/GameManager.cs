using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private GameState State;

    [SerializeField] private TextWriter gameOverText;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        ChangeGameState(GameState.GenerateGrid);
    }

    public GameState getGameState()
    {
        return State;
    }

    public void winGame(bool team)
    {
        if(!team)
        {
                // red win
                gameOverText.showMessage("Red win");
        }
        else
        {
                // blue win
                gameOverText.showMessage("Blue win");
        }
    }

    public void ChangeGameState(GameState newState)
    {
        State = newState;
        switch (newState)
        {
            case GameState.GenerateGrid:
                GridManager.Instance.GenerateGrid();
                break;
            case GameState.SetLevel:
                GridManager.Instance.SetBoard();
                break;
            case GameState.PlayerTurn:
                break;
            case GameState.EnemyTurn:
                break;
            default:
                Debug.Log("Invalid game state");
                break;
        }
    }
}

public enum GameState
{
    GenerateGrid,
    SetLevel,
    PlayerTurn,
    EnemyTurn,
    Victory,
    Lose
}