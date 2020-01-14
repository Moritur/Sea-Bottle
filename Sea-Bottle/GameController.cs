using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sea_Bottle
{
    public class GameController
    {
        public int clicksLeft { get; private set; }

        public IReadOnlyList<CellState> CellStates => Array.AsReadOnly(cellStates);

        int gridSide;
        int gridSize => gridSide * gridSide;
        CellState[] cellStates;
        Ship[] ships;

        readonly int shotLimit;

        public enum CellState
        {
            empty,
            ship,
            miss,
            hit,
            destroyed
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="shotLimit"></param>
        /// <param name="numberOfCells"></param>
        /// <param name="shipNumbers">How many ships of each size should be spawned. First element of array is number of ships with size 1, second 2, etc</param>
        public GameController(int shotLimit, int gridSide, params int[] shipNumbers)
        {
            if (gridSide < shipNumbers.Length) throw new ArgumentException("Ships can't be larger than grid", nameof(shipNumbers));
            if (gridSide * gridSide < shipNumbers.Sum()) throw new ArgumentException("Too many ships for given grid size", nameof(shipNumbers));

            this.shotLimit = shotLimit;
            clicksLeft = shotLimit;
            this.gridSide = gridSide;
            cellStates = new CellState[gridSide * gridSide];
            //GenerateShips(shipNumbers);
        }

        public bool CanCellBeShot(int cellId) => cellStates[cellId] != CellState.miss && cellStates[cellId] != CellState.hit && cellStates[cellId] != CellState.destroyed;

        public void UpdateShipGridForClick(int cellId)
        {
            clicksLeft--;
            cellStates[cellId] = (cellStates[cellId] == CellState.ship) ? CellState.hit : CellState.miss;

            if(clicksLeft <= 0)
            {
                clicksLeft = 0;
                Loose();
            }
        }

        void Loose()
        {

        }

        void Win()
        {

        }

        void GenerateShips(int[] shipNumbers)
        {
            throw new NotImplementedException();
        }

        bool TrySpawnShipRand(int size)
        {
            throw new NotImplementedException();
        }

    }
}
