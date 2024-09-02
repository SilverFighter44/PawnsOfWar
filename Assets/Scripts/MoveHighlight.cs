using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class MoveHighlight : MonoBehaviour
    {
        [SerializeField] private int range;
        Queue<GridTools.TileCoordinates> movesQueue = new Queue<GridTools.TileCoordinates>();

    public void setMovesQueue(Queue<GridTools.TileCoordinates> movesQueue)
    {
        this.movesQueue = movesQueue;
    }

    public Queue<GridTools.TileCoordinates> getMovesQeue()
    {
        return movesQueue;
    }

    public void setRange(int range)
        {
            this.range = range;
        }

        public int getRange()
        {
            return range;
        }
    }