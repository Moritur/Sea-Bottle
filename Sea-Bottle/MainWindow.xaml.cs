using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GameControl;

namespace Sea_Bottle
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region configs
        /// <summary>
        /// Length of grid's side in cells
        /// </summary>
        const int gridSide = 10;
        const int cellNumber = gridSide * gridSide;
        /// <summary>
        /// Number of shots player can fire before loosing the game
        /// </summary>
        /// <remarks>
        /// If last remaining shot is a winning shot player wins
        /// </remarks>
        const int shotLimit = 80;
        /// <summary>
        /// How long to wait (in seconds) before revealing result of player's shot
        /// </summary>
        const int shotAnimationTime = 1;
        /// <summary>
        /// >How many ships of each size should be spawned. First element of array is number of ships with size 1, second 2, etc
        /// </summary>
        readonly int[] shipNumbers = new int[] { 5, 3, 2, 1, 1, 1 };
        #endregion

        /// <summary>
        /// Instance of <see cref="GameController"/> used in current game
        /// </summary>
        GameController gameController;

        /// <summary>
        /// <see cref="Image"/> controls representing cells
        /// </summary>
         readonly Image[] cellImages = new Image[cellNumber];


        /// <summary>
        /// Instance of <see cref="TimeSpan"/> with <see cref="TimeSpan.TotalSeconds"/> equal to <see cref="shotAnimationTime"/>
        /// </summary>
        readonly long animationDuration = new TimeSpan(0, 0, shotAnimationTime).Ticks;

        public MainWindow()
        {
            InitializeComponent();
            gameController = new GameController(shotLimit, gridSide, shipNumbers);
            ImageResourcesManager.Initialize();
            InitializeCells();
            UpdateClicksUI();
        }

        /// <summary>
        /// Fills <see cref="CheckerBoard"/> with instances of <see cref="Image"/> representing cells
        /// and sets their <see cref="Image.Source"/> and <see cref="UIElement.MouseDown"/> to proper values
        /// </summary>
        private void InitializeCells()
        {
            for (int i = 0; i < cellNumber; i++)
            {
                // this is necessary, because value is passed to lambda expression
                int j = i;

                Image image = new Image() { Source = ImageResourcesManager.emptyCell };
                image.MouseDown += async (object sender, MouseButtonEventArgs e) => await HandleCellClickAsync(j);
                CheckerBoard.Children.Add(image);

                cellImages[i] = image;
            }
        }

        /// <summary>
        /// Handler for <see cref="UIElement.MouseDown"/> event of each cell in the game
        /// </summary>
        /// <param name="cellId">Index of the cell which was clicked in the <see cref="cellImages"/> array</param>
        async Task HandleCellClickAsync(int cellId)
        {
            if (cellId < 0 || cellId >= cellImages.Length) throw new ArgumentOutOfRangeException(nameof(cellId), $"Argument must be index of {nameof(Image)} in the {nameof(cellImages)} array");

            if (!gameController.CanCellBeShot(cellId)) return;

            long startTime = DateTime.Now.Ticks;
            cellImages[cellId].Source = ImageResourcesManager.shot;

            bool destroyedShip = await Task.Run(() => gameController.UpdateShipGridForShot(cellId));
            UpdateClicksUI();

            long timePassed = DateTime.Now.Ticks - startTime;
            if (timePassed < animationDuration) await Task.Delay(new TimeSpan(animationDuration - timePassed));

            if (destroyedShip) UpdateAllCellsUI();
            else UpdateCellUI(cellId);

            if (gameController.gameState == GameController.GameState.won)
            {
                await Task.Delay(new TimeSpan(animationDuration));
                await PlayVictoryAnimationAsync();
            }
            else if (gameController.gameState == GameController.GameState.lost)
            {
                await Task.Delay(new TimeSpan(animationDuration));
                await PlayDefeatAnimationAsync();
            }
        }

        /// <summary>
        /// Asynchronously plays voctory animation
        /// </summary>
        async Task PlayVictoryAnimationAsync()
        {
            foreach (Image image in cellImages)
            {
                image.Source = ImageResourcesManager.hit;
                await Task.Delay(50);
            }
        }

        /// <summary>
        /// Asynchronously plays defeat animation
        /// </summary>
        async Task PlayDefeatAnimationAsync()
        {
            foreach (Image image in cellImages)
            {
                image.Source = ImageResourcesManager.destroyedShip;
                await Task.Delay(50);
            }
        }

        /// <summary>
        /// Updates all cells' UI depending on their current <see cref="GameController.CellState"/> 
        /// </summary>
        /// <remarks>
        /// This method calls <see cref="UpdateCellUI(int)"/> for each cell.
        /// </remarks>
        void UpdateAllCellsUI()
        {
            for (int cellId = 0; cellId < cellNumber; cellId++)
            {
                UpdateCellUI(cellId);
            }
        }

        /// <summary>
        /// Updates UI of a cell based on its current <see cref="GameController.CellState"/> 
        /// </summary>
        /// <remarks>
        /// Cell's state is determined by <see cref="GameController.CellStates"/> of <see cref="gameController"/> at index from <paramref name="celllId"/>.
        /// </remarks>
        /// <param name="celllId"></param>
        void UpdateCellUI(int celllId)
        {
            switch (gameController.CellStates[celllId])
            {
                case GameController.CellState.empty:
                case GameController.CellState.ship:
                    cellImages[celllId].Source = ImageResourcesManager.emptyCell;
                    break;
                case GameController.CellState.miss:
                    cellImages[celllId].Source = ImageResourcesManager.miss;
                    break;
                case GameController.CellState.hit:
                    cellImages[celllId].Source = ImageResourcesManager.hit;
                    break;
                case GameController.CellState.destroyed:
                    cellImages[celllId].Source = ImageResourcesManager.destroyedShip;
                    break;
                default:
                    throw new ArgumentException("Cell in unexpected state", nameof(celllId));
            }
        }

        /// <summary>
        /// Updates number of clicks player has left in UI
        /// </summary>
        public void UpdateClicksUI() => calculator.Content = gameController.clicksLeft;

    }
}
