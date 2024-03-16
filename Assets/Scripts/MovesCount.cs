using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MovesCount : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI movesCount;

    public void updateMovesCount(int moves)
    {
        movesCount.text = moves.ToString();
    }
}
