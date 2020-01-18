using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sea_Bottle
{
    /// <summary>
    /// Class responsible for game logic
    /// </summary>
    public class GameController
    {
        /// <summary>
        /// Number of clicks player has left
        /// </summary>
        public int clicksLeft { get; private set; }

        /// <summary>
        /// Property allowing read acces to <see cref="cellStates"/> 
        /// </summary>
        public IReadOnlyList<CellState> CellStates => Array.AsReadOnly(cellStates);

        /// <summary>
        /// Length of grid's side in cells
        /// </summary>
        int gridSide;
        /// <summary>
        /// Total number of cells in the grid
        /// </summary>
        int gridSize => gridSide * gridSide;
        /// <summary>
        /// Stores <see cref="CellState"/> of each cell in the game
        /// </summary>
        CellState[] cellStates;
        /// <summary>
        /// All ships present in the game
        /// </summary>
        List<Ship> ships;
        /// <summary>
        /// Used for pseudo randomness when spawning ships
        /// </summary>
        Random random = new Random();
        /// <summary>
        /// Number of shots player can fire before loosing the game
        /// </summary>
        /// <remarks>
        /// If last remaining shot is a winning shot player wins
        /// </remarks>
        readonly int shotLimit;

        /// <summary>
        /// Current <see cref="GameState"/> of the game
        /// </summary>
        public GameState gameState { get; private set; } = GameState.loading;

        /// <summary>
        /// Represents state of a game
        /// </summary>
        public enum GameState
        {
            loading,
            inProgress,
            won,
            lost
        }

        /// <summary>
        /// Represents state of a cell
        /// </summary>
        public enum CellState
        {
            empty,
            ship,
            miss,
            hit,
            destroyed
        }

        /// <summary>
        /// Constructor for <see cref="GameController"/>
        /// </summary>
        /// <param name="shotLimit">Number of shots player can fire before loosing</param>
        /// <param name="numberOfCells"></param>
        /// <param name="shipNumbers">How many ships of each size should be spawned. First element of array is number of ships with size 1, second 2, etc</param>
        public GameController(int shotLimit, int gridSide, params int[] shipNumbers)
        {
            if (shipNumbers == null) throw new ArgumentNullException(nameof(shipNumbers), "Can't create a game without ships");
            if (shipNumbers.Length <= 0) throw new ArgumentException("Can't create a game without ships", nameof(shipNumbers));
            if (gridSide < shipNumbers.Length) throw new ArgumentException("Ships can't be larger than the grid", nameof(shipNumbers));
            if (gridSide * gridSide < shipNumbers.Sum()) throw new ArgumentException("Too many ships for the given grid size", nameof(shipNumbers));

            this.shotLimit = shotLimit;
            this.clicksLeft = shotLimit;
            this.gridSide = gridSide;
            this.cellStates = new CellState[gridSide * gridSide];
            this.ships = new List<Ship>(shipNumbers.Sum());
            GenerateShips(shipNumbers);
            gameState = GameState.inProgress;
        }

        /// <summary>
        /// Test if cell can be shot
        /// </summary>
        /// <param name="cellId">Id of a cell</param>
        /// <remarks>
        /// Takes into account <see cref="gameState"/> and <see cref="CellState"/> in <see cref="cellStates"/>
        /// </remarks>
        /// <returns>
        /// <see langword="true"/> if cell can be shot, otherwise false
        /// </returns>
        public bool CanCellBeShot(int cellId) => gameState == GameState.inProgress && cellStates[cellId] != CellState.miss && cellStates[cellId] != CellState.hit && cellStates[cellId] != CellState.destroyed;

        /// <summary>
        /// Processes player's shot and updates grid based on the result
        /// </summary>
        /// <param name="cellId">Id of the cell which was shot</param>
        /// <returns>
        /// <see langword="true"/> if any ship is destroyed by the shot
        /// </returns>
        public bool UpdateShipGridForShot(int cellId)
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

        /// <summary>
        /// Checks if ship should be destroyed
        /// </summary>
        /// <param name="cellId">Id of any of the cells on which the ship is placed</param>
        /// <remarks>
        /// If <see cref="Ship"/> should be destroyed, but is not,
        /// this method will call <see cref="Ship.Destroy"/> on it
        /// </remarks>
        /// <returns>
        /// <see langword="true"/> if ship was destroyed
        /// </returns>
        bool CheckForShipDestruction(int cellId)
        {
            bool destroyed = true;

            foreach (Ship ship in ships)
            {
                if (ship.cells.Contains(cellId))
                {
                    if (ship.isDestroyed) break;

                    foreach (int currentCellId in ship.cells)
                    {
                        if (cellStates[currentCellId] != CellState.hit && cellStates[currentCellId] != CellState.destroyed)
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

        /// <summary>
        /// Called when player looses the game
        /// </summary>
        void Loose()
        {

        }

        /// <summary>
        /// Called when player wins the game
        /// </summary>
        void Win()
        {

        }

        #region spawning ships

        /// <summary>
        /// Spawns ships on the grid
        /// </summary>
        /// <param name="shipNumbers">How many ships of each size should be spawned. First element of array is number of ships with size 1, second 2, etc</param>
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

        /// <summary>
        /// Tries to spawn a ship on random position
        /// </summary>
        /// <param name="size">Size of the ship to spawn</param>
        /// <returns>
        /// <see langword="true"/> when ship was spawned successfully, otherwise <see langword="false"/>
        /// </returns>
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

        /// <summary>
        /// Tries to spawn a ship starting from a given cell
        /// </summary>
        /// <param name="cellId">Id of the cell on which ship should start</param>
        /// <param name="size">Size of the ship to spawn</param>
        /// <returns>
        /// <see langword="true"/> when ship was spawned successfully, otherwise <see langword="false"/>
        /// </returns>
        bool TrySpawnShip(int cellId, int size)
        {
            int row = cellId / gridSide;
            int column = cellId % gridSize;
            bool spawned;

            bool startVertical = random.Next(0, 2) != 0;

            spawned = startVertical ? TrySpawnShipVertical(cellId, size) : TrySpawnShipHorizontal(cellId, size);

            if (!spawned)
            {
                spawned = !startVertical ? TrySpawnShipVertical(cellId, size) : TrySpawnShipHorizontal(cellId, size);
            }

            return spawned;
        }

        /// <summary>
        /// Tries to spawn ship vertically, starting from a given cell
        /// </summary>
        /// <param name="cellId">Id of the cell on which ship should start</param>
        /// <param name="size">Size of the ship to spawn</param>
        /// <returns>
        /// <see langword="true"/> when ship was spawned successfully, otherwise <see langword="false"/>
        /// </returns>
        bool TrySpawnShipVertical(int cellId, int size)
        {
            bool spawned = true;
            int[] shipCells = new int[size];
            int column = cellId % gridSize;

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

        /// <summary>
        /// Tries to spawn ship horizontally, starting from a given cell
        /// </summary>
        /// <param name="cellId">Id of the cell on which ship should start</param>
        /// <param name="size">Size of the ship to spawn</param>
        /// <returns>
        /// <see langword="true"/> when ship was spawned successfully, otherwise <see langword="false"/>
        /// </returns>
        bool TrySpawnShipHorizontal(int cellId, int size)
        {
            bool spawned = true;
            int[] shipCells = new int[size];
            int row = cellId / gridSide;

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

        /// <summary>
        /// Tests if ship can be spawned in a given cell
        /// </summary>
        /// <param name="cellId">Id of the cell</param>
        /// <returns>
        /// <see langword="true"/> if a ship can be spawned in the cell with id <paramref name="cellId"/> <see langword="false"/>
        /// </returns>
        //bool CanSpawnShipInCell(int cellId) => cellStates[cellId] == CellState.empty || cellStates[cellId] == CellState.miss;
        bool CanSpawnShipInCell(int cellId)
        {
            return
                (cellStates[cellId] == CellState.empty || cellStates[cellId] == CellState.miss)
                &&
                (
                    (cellId + 1) >= cellStates.Length
                    ||
                    (cellStates[cellId + 1] == CellState.empty || cellStates[cellId + 1] == CellState.miss)
                )
                &&
                (
                    (cellId - 1) < 0
                    ||
                    (cellStates[cellId - 1] == CellState.empty || cellStates[cellId - 1] == CellState.miss)
                )
                &&
                (
                    (cellId + gridSide) >= cellStates.Length
                    ||
                    (cellStates[cellId + gridSide] == CellState.empty || cellStates[cellId + gridSide] == CellState.miss)
                )
                &&
                (
                    (cellId - gridSide) < 0
                    ||
                    (cellStates[cellId - gridSide] == CellState.empty || cellStates[cellId - gridSide] == CellState.miss)
                );

        }

        /// <summary>
        /// Spawns a ship
        /// </summary>
        /// <param name="cells">Cells on which ship will be spawned</param>
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
