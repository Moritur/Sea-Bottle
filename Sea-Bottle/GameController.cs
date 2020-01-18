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
        List<Ship> ships;
        Random random = new Random();

        readonly int shotLimit;

        public GameState gameState { get; private set; } = GameState.loading;

        public enum GameState
        {
            loading,
            inProgress,
            won,
            lost
        }

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
            this.clicksLeft = shotLimit;
            this.gridSide = gridSide;
            this.cellStates = new CellState[gridSide * gridSide];
            this.ships = new List<Ship>(shipNumbers.Sum());
            GenerateShips(shipNumbers);
            gameState = GameState.inProgress;
        }

        public bool CanCellBeShot(int cellId) => gameState == GameState.inProgress && cellStates[cellId] != CellState.miss && cellStates[cellId] != CellState.hit && cellStates[cellId] != CellState.destroyed;

        public bool UpdateShipGridForClick(int cellId)
        {
            clicksLeft--;
            cellStates[cellId] = (cellStates[cellId] == CellState.ship) ? CellState.hit : CellState.miss;
            bool destroyrdShip = CheckForShipDestruction(cellId);

            if (gameState == GameState.inProgress && clicksLeft <= 0)
            {
                clicksLeft = 0;
                gameState = GameState.lost;
                Loose();
            }
            else if (gameState == GameState.won)
            {
                Win();
            }
            return destroyrdShip;
        }

        bool CheckForShipDestruction(int cellId)
        {
            bool destroyed = true;

            foreach (Ship ship in ships)
            {
                if (ship.cells.Contains(cellId))
                {
                    foreach (int currentCellId in ship.cells)
                    {
                        if (cellStates[currentCellId] != CellState.hit)
                        {
                            destroyed = false;
                            break;
                        }
                    }
                    if (destroyed)
                    {
                        ship.Destroy();
                        foreach (int currentCellId in ship.cells)
                        {
                            cellStates[currentCellId] = CellState.destroyed;
                        }
                        bool wasLastShip = true;
                        foreach (Ship anotherShip in ships)
                        {
                            if (!anotherShip.isDestroyed)
                            {
                                wasLastShip = false;
                                break;
                            }
                        }
                        if (wasLastShip) gameState = GameState.won;
                    }
                    break;
                }
            }

            return destroyed;
        }

        void Loose()
        {

        }

        void Win()
        {

        }

        #region spawning ships

        void GenerateShips(int[] shipNumbers)
        {
            for (int i = 0; i < shipNumbers.Length; i++)
            {
                for (int j = 0; j < shipNumbers[i]; j++)
                {
                    if (!TrySpawnShipRand(i + 1)) return;
                }
            }
        }

        bool TrySpawnShipRand(int size)
        {
            int cellId = random.Next(0, gridSize);
            bool spawned = false;

            for (int i = 0; i < gridSize; i++)
            {
                spawned = TrySpawnShip(cellId, size);
                if (spawned) break;
                if (++cellId >= gridSize) cellId = 0;
            }

            return spawned;
        }

        bool TrySpawnShip(int cellId, int size)
        {
            int row = cellId / gridSide;
            int column = cellId % gridSize;
            bool spawned;

            bool startVertical = random.Next(0, 2) != 0;

            spawned = startVertical ? TrySpawnShipVertical(cellId, size, column) : TrySpawnShipHorizontal(cellId, size, row);

            if (!spawned)
            {
                spawned = !startVertical ? TrySpawnShipVertical(cellId, size, column) : TrySpawnShipHorizontal(cellId, size, row);
            }

            return spawned;
        }

        bool TrySpawnShipVertical(int cellId, int size, int column)
        {
            bool spawned = true;
            int[] shipCells = new int[size];

            for (int i = 0; i < size; i++)
            {
                int currentCellId = cellId + (i * gridSide);
                shipCells[i] = currentCellId;
                if ((currentCellId % gridSide) != column || !CanSpawnShipInCell(currentCellId))
                {
                    spawned = false;
                    break;
                }
            }

            if (spawned) SpawnShip(shipCells);
            return spawned;
        }

        bool TrySpawnShipHorizontal(int cellId, int size, int row)
        {
            bool spawned = true;
            int[] shipCells = new int[size];

            for (int i = 0; i < size; i++)
            {
                int currentCellId = cellId + i;
                shipCells[i] = currentCellId;
                if ((currentCellId / gridSide) != row || !CanSpawnShipInCell(currentCellId))
                {
                    spawned = false;
                    break;
                }
            }

            if (spawned) SpawnShip(shipCells);
            return spawned;
        }

        bool CanSpawnShipInCell(int cellId) => cellStates[cellId] == CellState.empty || cellStates[cellId] == CellState.miss;

        void SpawnShip(int[] cells)
        {
            ships.Add(new Ship(cells));

            for (int i = 0; i < cells.Length; i++)
            {
                cellStates[cells[i]] = CellState.ship;
            }
        }

        #endregion
    }
}
