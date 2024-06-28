using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FinalProject
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        //Dictionary maps grid vales to the sources
        private readonly Dictionary<GridValue, ImageSource> grid_value_to_image = new()
        {
            //grid position is empty, display empty image
            {GridValue.EmptyPartOfGrid, snakeFeature.empty},

            //grid position contains snake, display body image
             {GridValue.GridOccupiedBySnake, snakeFeature.Snakebody},

            //grid position contains food, display food image
            {GridValue.GridOccupiedByFood, snakeFeature.Snakefood },

            //grid position contains bonus food, display food image
            {GridValue.GridOccupiedByBonusFood, snakeFeature.Bonusfood}

        };

        //Dictionary to move snake head
        private readonly Dictionary<Direction, int> DirectionsForSnakeRotation = new()
        {
            {Direction.Up, 0},
            {Direction.Right, 90 },
            {Direction.Down, 180 },
            {Direction.Left, 270 }
        };
        private readonly int rows = 15, cols = 15;
        private readonly Image[,] ImagesOnGrid;
        private GameBoard gameStatus;
        private  SpeedManager speedManager;
        private HighScoreManager highScoreManager;
        private int trials = 0;

        public MainWindow()
        {
            InitializeComponent();
            //Initialize GUI
            ImagesOnGrid = SetupGrid(); 
            gameStatus = new GameBoard(rows, cols);     
            speedManager = new SpeedManager();
            highScoreManager = new HighScoreManager();
            ShowStartMessage();
            DisplayHighScore();
        }
        #region Events
        private async void WindowLoaded(object sender, RoutedEventArgs e)
        {
            display();
            await WaitForKeyPress();
            await GameLoop();
        }


        // This method is called when user presses the key
        private void WindowKeyDown(object sender, KeyEventArgs e)
        {
            // If the game is over and user presses a key, check for restart
            if (gameStatus.GameOver)
            {
                CheckForRestart();
                return;
            }

            // Check which key is pressed
            switch (e.Key)
            {
                case Key.Left:
                    gameStatus.ChangeSnakeDirection(Direction.Left);
                    break;
                case Key.Right:
                    gameStatus.ChangeSnakeDirection(Direction.Right);
                    break;
                case Key.Down:
                    gameStatus.ChangeSnakeDirection(Direction.Down);
                    break;
                case Key.Up:
                    gameStatus.ChangeSnakeDirection(Direction.Up);
                    break;
            }
        }
        #endregion

        #region SetupGrid (Initialize GUI)
        private Image[,] SetupGrid()
        {
            Image[,] Grid = new Image[rows, cols];
            Gamegrid.Rows = rows;
            Gamegrid.Columns = cols;

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    Image snakefeature = new Image
                    {
                        Source = snakeFeature.empty
                    };

                    Border border = new Border
                    {
                        BorderBrush = (Brush)FindResource("gridLineColor"),
                        BorderThickness = new Thickness(1),
                        Child = snakefeature
                    };

                    Grid[i, j] = snakefeature;
                    Gamegrid.Children.Add(border);
                }
            }

            return Grid;
        }
        #endregion

        #region Methods
        //This methood will look at the grid array in the grid state and update the grid images to rflect grid.
        //It loops through every grid position
        //we get the grid value at the current positon and set the source for the corresponding image using dictionary
        private void display()
        {
            DisplayGrid();
            YourScore.Text = $"SCORE {gameStatus.Score}";
        }

        // Method to update the grid display
        private void DisplayGrid()
        {
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    GridValue gridValue = gameStatus.Values[i, j];
                    ImagesOnGrid[i, j].Source = grid_value_to_image[gridValue];
                }
            }
        }

        private async Task WaitForKeyPress()
        {
            while (true)
            {
                if (Keyboard.IsKeyDown(Key.Left) || Keyboard.IsKeyDown(Key.Right) || Keyboard.IsKeyDown(Key.Up) || Keyboard.IsKeyDown(Key.Down))
                {
                    break;
                }
                await Task.Delay(100);
            }
        }
        private void ShowStartMessage()
        {
            MessageBox.Show("Press any key to start the game.", "Start Game", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        //This method moves the snake at regular intervals

        private async Task GameLoop()
        {
            while (!gameStatus.GameOver)
            {

                // Use the current speed for the delay
                // Update speed based on score
                speedManager.UpdateSpeed(gameStatus.Score);
                // Use the current speed for the delay
                await Task.Delay(speedManager.CurrentSpeed);
                display();
                gameStatus.Move();
                

                if (gameStatus.GameOver)
                {
                    trials++;
                    MessageBox.Show("Game Over! The snake " + (gameStatus.HitSelf ? "hit itself." : "moved outside the grid."), "Game Over", MessageBoxButton.OK, MessageBoxImage.Information);
                    highScoreManager.SaveHighScore(gameStatus.Score);
                    DisplayHighScore();
                    if (trials < 2)
                    {
                        MessageBox.Show("Press any key to restart.", "Restart Game", MessageBoxButton.OK, MessageBoxImage.Information);

                        highScoreManager.LoadHighScore();
                        ResetGame();
                        await WaitForKeyPress();
                        await GameLoop();

                    }
                    else
                    {
                        if (MessageBox.Show("Do you want to play again?", "Restart Game", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            trials = 0;

                            highScoreManager.LoadHighScore();
                            ResetGame();                   
                            await WaitForKeyPress();
                            await GameLoop();
                        }
                        else
                        {
                            Application.Current.Shutdown();
                        }
                    }
                }
            }
        }

        private void ResetGame()
        {
            gameStatus = new GameBoard(rows, cols);
            display();
        }

        private void CheckForRestart()
        {
            if (MessageBox.Show("Press any key to restart the game.", "Restart Game", MessageBoxButton.OK, MessageBoxImage.Information) == MessageBoxResult.OK)
            {
                if (trials < 2)
                {
                    ResetGame();
                }
                else
                {
                    if (MessageBox.Show("Do you want to play again?", "Restart Game", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        trials = 0;
                        ResetGame();
                    }
                    else
                    {
                        Application.Current.Shutdown();
                    }
                }
            }
        }

        private void DisplayHighScore()
        {
            HighScore.Text = $"HIGH SCORE {highScoreManager.HighScore}";
        }
        #endregion
    }
}