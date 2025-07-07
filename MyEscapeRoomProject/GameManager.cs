namespace MyEscapeRoomProject
{
    internal class GameManager
    {
        // Zeichen-Konstanten
        private const char wall = '#';
        private const char floor = ' ';
        private const char player = '@';
        private const char key = 'S';
        private const char doorClosed = 'T';
        private const char doorOpened = 'O';

        // Spielfeld und Status
        private char[,] room;
        private int width, height;
        private (int X, int Y) playerPos, keyPos, doorPos;
        private bool keyCollected;
        private bool isGameOver;


        /// <summary>
        /// Startet den Escape Room
        /// </summary>
        public void RunGame()
        {
            ShowIntro();
            (width, height) = AskDimensions();
            InitRoom();
            PlaceEntities();
            GameLoop();
            EndGame();
        }


        private void ShowIntro()
        {
            // Spielmechanik erklären
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Willkommen zum Escape Room! Die Tür hinter dir fällt ins Schloss und ein kühler Luftzug streift deinen Nacken. Wirst du den Weg hinaus finden?");

            // Kurze Pause
            Thread.Sleep(1500);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine();
            Console.WriteLine("Dein Ziel: Finde und sammle den Schlüssel, öffne die Tür und entkomme.");

            Thread.Sleep(1500);
            Console.WriteLine();
            Console.WriteLine("Bewege dich mit den Pfeiltasten. Drücke eine Taste pro Schritt.");


            Thread.Sleep(1000);
            Console.WriteLine();
            Console.WriteLine("Drücke eine beliebige Taste, um fortzufahren...");
            Console.ReadKey();
            Console.Clear();
        }

        private (int width, int height) AskDimensions()
        {
            int w, h;
            while (true)
            {
                Console.Write("Bitte gib die Breite des Raums (mindestens 5 und maximal 100) ein: "); // 4 als Minimum funktioniert faktisch, schränkt die Spielfläche allerdings zu sehr ein
                if (!int.TryParse(Console.ReadLine(), out w) || w < 5 || w > 100) // min max durch const austauschen
                {
                    Console.WriteLine("Die eingegebene Breite ist ungültig!");
                    continue;
                }

                Console.Write("Bitte gib die Höhe des Raums (mindestens 5 und maximal 25) ein: ");
                if (!int.TryParse(Console.ReadLine(), out h) || h < 5 || h > 25)
                {
                    Console.WriteLine("Die eingegebene Höhe ist ungültig!");
                    continue;
                }
                break;
            }
            Console.WriteLine("Raum wird generiert...");
            return (w, h);
        }

        private void InitRoom()
        {
            room = new char[height, width];
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    room[y, x] = (y == 0 || y == height - 1 || x == 0 || x == width - 1)
                                  ? wall : floor;
        }

        private void PlaceEntities()
        {
            var rnd = new Random();
            // Spieler
            playerPos = (rnd.Next(1, width - 1), rnd.Next(1, height - 1));
            room[playerPos.Y, playerPos.X] = player;

            // Schlüssel
            do
                keyPos = (rnd.Next(1, width - 1), rnd.Next(1, height - 1));
            while (keyPos == playerPos);
            room[keyPos.Y, keyPos.X] = key;

            // Tür
            int side = rnd.Next(4);
            doorPos = side switch
            {
                0 => (rnd.Next(1, width - 1), 0),            // oben
                1 => (rnd.Next(1, width - 1), height - 1),     // unten
                2 => (0, rnd.Next(1, height - 1)),           // links
                _ => (width - 1, rnd.Next(1, height - 1)),     // rechts
            };
            room[doorPos.Y, doorPos.X] = doorClosed;
        }

        private void GameLoop()
        {
            keyCollected = false;
            isGameOver = false;
            DrawRoom();

            while (!isGameOver)
            {
                var newPos = ReadInput();
                if (!IsInside(newPos) || IsWall(newPos))
                    continue;

                if (IsClosedDoor(newPos) && !keyCollected)
                    continue;
                if (IsClosedDoor(newPos) && keyCollected)
                    OpenDoor();

                MovePlayer(newPos);
                CheckKeyPickup();
                CheckWin();

                DrawRoom();
            }
        }

        private (int X, int Y) ReadInput()
        {
            var key = Console.ReadKey(true).Key;
            return key switch
            {
                ConsoleKey.UpArrow => (playerPos.X, playerPos.Y - 1),
                ConsoleKey.DownArrow => (playerPos.X, playerPos.Y + 1),
                ConsoleKey.LeftArrow => (playerPos.X - 1, playerPos.Y),
                ConsoleKey.RightArrow => (playerPos.X + 1, playerPos.Y),
                _ => playerPos
            };
        }

        private bool IsInside((int X, int Y) pos)
            => pos.X >= 0 && pos.X < width && pos.Y >= 0 && pos.Y < height;

        private bool IsWall((int X, int Y) pos)
            => room[pos.Y, pos.X] == wall;

        private bool IsClosedDoor((int X, int Y) pos)
            => room[pos.Y, pos.X] == doorClosed;

        private void OpenDoor()
        {
            room[doorPos.Y, doorPos.X] = doorOpened;
            Console.Beep();
        }

        private void MovePlayer((int X, int Y) newPos)
        {
            if (playerPos == doorPos)
                room[playerPos.Y, playerPos.X] = doorOpened;
            else
                room[playerPos.Y, playerPos.X] = floor;


            playerPos = newPos;
            room[playerPos.Y, playerPos.X] = player;
        }

        private void CheckKeyPickup()
        {
            if (!keyCollected && playerPos == keyPos)
            {
                keyCollected = true;
                Console.Beep();
            }
        }

        private void CheckWin()
        {
            if (keyCollected && playerPos == doorPos)
            {
                DrawRoom();
                isGameOver = true;
            }
        }

        private void DrawRoom()
        {
            Console.Clear();
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                    Console.Write(room[y, x]);
                Console.WriteLine();
            }
            Console.WriteLine($"Schlüssel eingesammelt: {(keyCollected ? "Ja" : "Nein")}");
        }

        private void EndGame()
        {
            Console.WriteLine("Glückwunsch, du bist entkommen! Drücke eine beliebige Taste um zu beenden...");
            Console.ReadKey();
            Console.ResetColor();
        }
    }
}
