using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MovesCount : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI movesCount;

    public void updateMovesCount(int moves)
    {
        if (moves >= 0)
        {
            movesCount.text = moves.ToString();
        }
        else
        {
            movesCount.text = "X";
        }
    }
}
