﻿using System;
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

namespace Sea_Bottle
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region configs
        const int gridSide = 10;
        const int cellNumber = gridSide * gridSide;
        const int shotLimit = 40;
        const int shotAnimationTime = 1;
        #endregion

        GameController gameController;

        Image[] cellImages = new Image[cellNumber];



        long animationDuration = new TimeSpan(0, 0, shotAnimationTime).Ticks;

        public MainWindow()
        {
            InitializeComponent();
            gameController = new GameController(shotLimit, cellNumber);
            ImageResourcesManager.Initialize();
            InitializeCells();
            UpdateClicksUI();
        }


        private void InitializeCells()
        {
            for (int i = 0; i < cellNumber; i++)
            {
                int j = i;

                Image image = new Image() { Source = ImageResourcesManager.emptyCell };
                image.MouseDown += async (object sender, MouseButtonEventArgs e) => await HandleCellClickAsync(j);
                CheckerBoard.Children.Add(image);

                cellImages[i] = image;
            }
        }

        async Task HandleCellClickAsync(int cellId)
        {
            if (!gameController.CanCellBeShot(cellId)) return;

            long startTime = DateTime.Now.Ticks;

            cellImages[cellId].Source = ImageResourcesManager.shot;
            await Task.Run(() => gameController.UpdateShipGridForClick(cellId));
            UpdateClicksUI();
            long timePassed = DateTime.Now.Ticks - startTime;
            if (timePassed < animationDuration)
            {
                await Task.Delay(new TimeSpan(animationDuration - timePassed));
            }
            UpdateCellUI(cellId);
        }



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

        public void UpdateClicksUI() => calculator.Content = gameController.clicksLeft;


    }
}
