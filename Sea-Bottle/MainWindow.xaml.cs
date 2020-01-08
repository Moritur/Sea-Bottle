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

namespace Sea_Bottle
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region configs
        const int cellNumber = 100;
        const int shotLimit = 40;
        #endregion

        (Image image, CellState state)[] cells = new (Image uiElement, CellState state)[cellNumber];

        int clicksLeft = shotLimit;

        enum CellState
        {
            empty,
            ship,
            miss,
            hit,
            destroyed
        }

        public MainWindow()
        {
            InitializeComponent();
            InitializeCells();
        }


        private void InitializeCells()
        {
            for (int i = 0; i < cellNumber; i++)
            {
                int j = i;

                Image image = new Image();
                image.MouseDown += async (object sender, MouseButtonEventArgs e) => await HandleCellClickAsync(j);
                CheckerBoard.Children.Add(image);

                cells[i] = (image, CellState.empty);
            }
        }

        async Task HandleCellClickAsync(int cellId)
        {
            if (cells[cellId].state == CellState.miss) return;

            clicksLeft--;
            await Task.Run(() => UpdateShipGridForClick(cellId));
            UpdateCellUI(cellId);
        }

        void UpdateShipGridForClick(int cellId)
        {
            cells[cellId].state = (cells[cellId].state == CellState.ship) ? CellState.hit : CellState.miss;
        }

        void UpdateCellUI(int celllId)
        {

        }

    }
}
