using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SeaWars
{
    public partial class MainForm : Form
    {
        private int[,] playerMap;
        private int[,] enemyMap;
        private const int mapSize = 10;
        private const int cellSize = 30;
        private bool isPlaying = false;
        private bool isPlacingShips = true; 
        private int currentShipIndex = 0;

        private Button[,] playerButtons;
        private Button[,] enemyButtons;

        private Random random = new Random();
        private int[] shipSizes = new int[] { 4, 3, 3, 2, 2, 2, 1, 1, 1, 1 };
        private Database db;

        public MainForm()
        {
            InitializeComponent();
            db = new Database(); 
            InitializeGame();

            
            int windowWidth = (mapSize * cellSize * 2) + 150; 
            int windowHeight = (mapSize * cellSize) + 100;

            this.ClientSize = new Size(windowWidth, windowHeight); 
        }

        private void InitializeGame()
        {
            playerMap = new int[mapSize, mapSize];
            enemyMap = new int[mapSize, mapSize];

            playerButtons = new Button[mapSize, mapSize];
            enemyButtons = new Button[mapSize, mapSize];

            CreateGameBoard();
        }

        private void CreateGameBoard()
        {
            Controls.Clear();
            currentShipIndex = 0;

        
            for (int i = 0; i < mapSize; i++)
            {
                for (int j = 0; j < mapSize; j++)
                {
                    var btn = new Button
                    {
                        Width = cellSize,
                        Height = cellSize,
                        Location = new Point(j * cellSize, i * cellSize),
                        Tag = new Point(i, j),
                        BackColor = Color.LightBlue
                    };

                    btn.Click += PlayerPlaceShip;
                    playerButtons[i, j] = btn; 
                    Controls.Add(btn);
                }
            }

            
            for (int i = 0; i < mapSize; i++)
            {
                for (int j = 0; j < mapSize; j++)
                {
                    var btn = new Button
                    {
                        Width = cellSize,
                        Height = cellSize,
                        Location = new Point((j + mapSize) * cellSize, i * cellSize),
                        Tag = new Point(i, j),
                        BackColor = Color.LightGray
                    };

                    btn.Enabled = true;
                    btn.Click += PlayerShoot; 
                    enemyButtons[i, j] = btn;
                    Controls.Add(btn);
                }
            }

           
            var startButton = new Button
            {
                Text = "Начать игру",
                Location = new Point(10, mapSize * cellSize + 10),
                Width = 100
            };

            startButton.Click += (sender, e) => StartGame();
            Controls.Add(startButton);

           
            var statsButton = new Button
            {
                Text = "Показать статистику",
                Location = new Point(120, mapSize * cellSize + 10),
                Width = 150
            };

            statsButton.Click += (sender, e) => ShowStatistics();
            Controls.Add(statsButton);
        }

        private void PlayerPlaceShip(object sender, EventArgs e)
        {
            if (!isPlacingShips) return; 

            Button pressedButton = sender as Button;
            var position = (Point)pressedButton.Tag;

           
            if (playerMap[position.X, position.Y] == 0)
            {
                playerMap[position.X, position.Y] = 1;
                pressedButton.BackColor = Color.
                    Red; 

                
                if (++currentShipIndex >= shipSizes.Length)
                {
                    isPlacingShips = false; 
                    MessageBox.Show("Вы расставили все корабли. Нажмите 'Начать игру'.");
                }
            }
        }

        private void StartGame()
        {
            isPlaying = true;
            MessageBox.Show("Игра началась!");
            PlaceEnemyShips(); 
        }

        private void PlaceEnemyShips()
        {
            
            int[] shipSizesForBot = new int[] { 4, 3, 2, 1 };

            foreach (var shipSize in shipSizesForBot)
            {
                bool placed = false;
                while (!placed)
                {
                    
                    int startX = random.Next(mapSize);
                    int startY = random.Next(mapSize);
                    bool horizontal = random.Next(2) == 0; 

                    
                    if (CanPlaceShip(startX, startY, shipSize, horizontal, enemyMap))
                    {
                        
                        PlaceShip(startX, startY, shipSize, horizontal, enemyMap);
                        placed = true;
                    }
                }
            }
        }

        private bool CanPlaceShip(int startX, int startY, int shipSize, bool horizontal, int[,] map)
        {
            if (horizontal)
            {
                if (startY + shipSize > mapSize) return false;

                for (int i = 0; i < shipSize; i++)
                {
                    if (map[startX, startY + i] != 0) return false;
                }
            }
            else
            {
                if (startX + shipSize > mapSize) return false;

                for (int i = 0; i < shipSize; i++)
                {
                    if (map[startX + i, startY] != 0) return false;
                }
            }

            return true;
        }

        private void PlaceShip(int startX, int startY, int shipSize, bool horizontal, int[,] map)
        {
            if (horizontal)
            {
                for (int i = 0; i < shipSize; i++)
                {
                    map[startX, startY + i] = 1;
                }
            }
            else
            {
                for (int i = 0; i < shipSize; i++)
                {
                    map[startX + i, startY] = 1;
                }
            }
        }

        private void PlayerShoot(object sender, EventArgs e)
        {
            if (!isPlaying) return;

            Button pressedButton = sender as Button;
            var position = (Point)pressedButton.Tag;

            
            if (pressedButton.BackColor == Color.Black || pressedButton.BackColor == Color.Blue)
            {
                MessageBox.Show("Вы уже атаковали эту клетку!");
                return; 
            }

            
            if (enemyMap[position.X, position.Y] == 0)
            {
                pressedButton.BackColor = Color.Black; 
            }
            else
            {
                pressedButton.BackColor = Color.Blue; 
                enemyMap[position.X, position.Y] = 0;
            }

           
            BotAttack();

            
            CheckGameOver();
        }

        private void BotAttack()
        {
            Random r = new Random();
            int posX = r.Next(0, mapSize);
            int posY = r.Next(0, mapSize);

            
            while (playerMap[posX, posY] == 0 &&
                   (playerButtons[posX, posY].BackColor == Color.Blue ||
                    playerButtons[posX, posY].BackColor == Color.Black))
            {
                posX = r.Next(0, mapSize);
                posY = r.Next(0, mapSize);
            }

            
            if (playerMap[posX, posY] != 0)
            {
                playerButtons[posX, posY].BackColor = Color.Blue;
                playerMap[posX, posY] = 0; 
            }
            else
            {
                playerButtons[posX, posY].BackColor = Color.Black; 
            }
        }

        private void CheckGameOver()
        {
            
            if (playerMap.Cast<int>().All(cell => cell == 0))
            {
                MessageBox.Show("Вы проиграли!");
                SaveGameResult("Проигрыш");
                ResetGame();
                return;
            }

           
            if (enemyMap.Cast<int>().All(cell => cell == 0))
            {
                MessageBox.Show("Вы выиграли!");
                SaveGameResult("Победа");
                ResetGame();
                return;
            }
        }

        private void ResetGame()
        {
            playerMap = new int[mapSize, mapSize];
            enemyMap = new int[mapSize, mapSize];
            currentShipIndex = 0;
            isPlacingShips = true;
            isPlaying = false;

            CreateGameBoard();
        }

        private void SaveGameResult(string result)
        {
            db.SaveResult(result); 
        }

        private void ShowStatistics()
        {
            string stats = db.GetStatistics();
            MessageBox.Show(stats);
        }
    }
}