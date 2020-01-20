using System;
using System.Collections.Generic;
using System.Configuration;
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
        readonly int gridSide;
        readonly int cellNumber;
        /// <summary>
        /// Number of shots player can fire before loosing the game
        /// </summary>
        /// <remarks>
        /// If last remaining shot is a winning shot player wins
        /// </remarks>
        readonly int shotLimit;
        /// <summary>
        /// How long to wait (in seconds) before revealing result of player's shot
        /// </summary>
        readonly int shotAnimationTime;
        /// <summary>
        /// >How many ships of each size should be spawned. First element of array is number of ships with size 1, second 2, etc
        /// </summary>
        readonly int[] shipNumbers;
        #endregion

        /// <summary>
        /// Instance of <see cref="GameController"/> used in current game
        /// </summary>
        GameController gameController;

        /// <summary>
        /// <see cref="Image"/> controls representing cells
        /// </summary>
        readonly Image[] cellImages;

        /// <summary>
        /// Number of ticks of <see cref="TimeSpan"/> with <see cref="TimeSpan.TotalSeconds"/> equal to <see cref="shotAnimationTime"/>
        /// </summary>
        readonly long animationDuration;

        volatile bool isEndAnimationPlaying = false;
        volatile bool isNewGameStarting = false;

        public MainWindow()
        {
            InitializeComponent();

            #region load settings
            try
            {
                gridSide = int.Parse(ConfigurationManager.AppSettings["gridSide"]);
                cellNumber = gridSide * gridSide;
                shotLimit = int.Parse(ConfigurationManager.AppSettings["shotLimit"]);
                shotAnimationTime = int.Parse(ConfigurationManager.AppSettings["shotAnimationTime"]);
                animationDuration = new TimeSpan(0, 0, shotAnimationTime).Ticks;

                shipNumbers = ConfigurationManager.AppSettings["shipNumbers"].Split(',').Select(s => int.Parse(s)).ToArray();

                cellImages = new Image[cellNumber];
            }
            catch (Exception)
            {
                MessageBox.Show("Invalid configuration file", "Error");
                Application.Current.Shutdown();
            }
            #endregion

            gameController = new GameController(shotLimit, gridSide, shipNumbers);
            ImageResourcesManager.Initialize();
            GameAudioManager.Initialize();
            InitializeCells();
            InitializeButtons();
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

        private void InitializeButtons()
        {
            exit.Click += Exit_Click;
            music.Click += Music_Click;
            newGame.Click += NewGame_Click;
        }

        private async void NewGame_Click(object sender, RoutedEventArgs e)
        {
            if (isNewGameStarting) return;

            isNewGameStarting = true;
            while (isEndAnimationPlaying) await Task.Delay(100);

            gameController = new GameController(shotLimit, gridSide, shipNumbers);
            UpdateAllCellsUI();
            UpdateClicksUI();
            gameEndPopup.Visibility = Visibility.Hidden;
            await Task.Delay(shotAnimationTime * 1000);
            UpdateAllCellsUI();
            isNewGameStarting = false;
        }

        private void Music_Click(object sender, RoutedEventArgs e)
        {
            GameAudioManager.SwitchMute();
            ((ImageBrush)music.Background).ImageSource = GameAudioManager.isAudioMuted ? ImageResourcesManager.soundOffIcon : ImageResourcesManager.soundIcon;
        }

        private void Exit_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

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
            GameAudioManager.PlayShot();

            bool destroyedShip = await Task.Run(() => gameController.UpdateShipGridForShot(cellId));
            UpdateClicksUI();

            long timePassed = DateTime.Now.Ticks - startTime;
            if (timePassed < animationDuration) await Task.Delay(new TimeSpan(animationDuration - timePassed));

            if (destroyedShip)
            {
                GameAudioManager.PlayBubbles();
                UpdateAllCellsUI();
            }
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
            isEndAnimationPlaying = true;
            gameEndPopup.Source = ImageResourcesManager.victory;
            gameEndPopup.Visibility = Visibility.Visible;
            foreach (Image image in cellImages)
            {
                image.Source = ImageResourcesManager.hit;
                await Task.Delay(50);
            }
            isEndAnimationPlaying = false;
        }

        /// <summary>
        /// Asynchronously plays defeat animation
        /// </summary>
        async Task PlayDefeatAnimationAsync()
        {
            isEndAnimationPlaying = true;
            gameEndPopup.Source = ImageResourcesManager.defeat;
            gameEndPopup.Visibility = Visibility.Visible;
            foreach (Image image in cellImages)
            {
                image.Source = ImageResourcesManager.destroyedShip;
                await Task.Delay(50);
            }
            isEndAnimationPlaying = false;
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
        public void UpdateClicksUI() => calculator.Content = gameController.clicksLeft.ToString() + " ";

    }
}
