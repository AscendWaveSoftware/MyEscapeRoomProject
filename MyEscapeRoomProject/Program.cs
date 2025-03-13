namespace MyEscapeRoomProject
{
    internal class Program
    {
        // Definition der Variablen
        const char wall = '#';
        const char floor = ' ';
        const char player = '@';
        const char key = 'S';

        // Tür hat zwei Zustände
        const char doorClosed = 'T';
        const char doorOpened = 'O';


        static void Main(string[] args)
        {
            //=== Spielmechanik erklären ===//
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


            //=== Eingabe der Raumdimension ===//
            int width = 0, height = 0;
            bool validInput = false; // Überprüft, ob die Eingabe gültig ist
            while (!validInput)
            {
                Console.Write("Bitte gib die Breite des Raums (mindestens 5 und maximal 100) ein: ");
                if(int.TryParse(Console.ReadLine(), out width) && width >= 5 && width <= 100)
                {
                    Console.Write("Bitte gib die Höhe des Raums (mindestens 5 und maximal 25) ein: ");
                    if (int.TryParse(Console.ReadLine(), out height) && height >= 5 && height <= 25)
                        validInput = true;
                    else
                        Console.WriteLine("Die eingegebene Höhe ist ungültig!");
                }
                else
                {
                    Console.WriteLine("Die eingegebene Höhe ist ungültig!");
                }
            }


            //=== Raum als 2D-Array initialisieren und erstellen ===//
            Console.WriteLine("Raum wird generiert...");
            char[,] room = new char[height, width];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    // Wände am Rand und Boden im Innenbereich
                    if (i == 0 || i == height - 1 || j == 0 || j == width - 1)
                        room[i, j] = wall;
                    else
                        room[i, j] = floor;
                }
            }

            Random rnd = new Random();

            //=== Zufällige Positionierung von Spieler und Schlüssel im Innenbereich ===//
            int playerX = rnd.Next(1, width - 1);
            int playerY = rnd.Next(1, height - 1);

            int keyX, keyY;

            // Sicherstellen, dass Schlüssel und Spieler nicht kollidieren
            do
            {
                keyX = rnd.Next(1, width - 1);
                keyY = rnd.Next(1, height - 1);
            } while (keyX == playerX && keyY == playerY);


            //=== Tür an einer zufälligen Wand positionieren ===//
            int doorX = 0, doorY = 0;
            int side = rnd.Next(4);

            switch (side)
            {
                case 0: // Oben
                    doorY = 0;
                    doorX = rnd.Next(1, width - 1);
                    break;
                case 1: // Unten
                    doorY = height - 1;
                    doorX = rnd.Next(1, width - 1);
                    break;
                case 2: // Links
                    doorY = rnd.Next(1, height - 1);
                    doorX = 0;
                    break;
                case 3: // Rechts
                    doorY = rnd.Next(1, height - 1);
                    doorX = width - 1;
                    break;
            }

            //=== Platzierung der Objekte im Raum ===//
            room[playerY, playerX] = player;
            room[keyY, keyX] = key;
            room[doorY, doorX] = doorClosed;

            bool keyCollected = false;
            bool isGameOver = false;

            // Methode zum Zeichnen des Raums
            void DrawRoom()
            {
                Console.Clear();
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        Console.Write(room[i, j]);
                    }
                    Console.WriteLine();
                }
                Console.WriteLine("Schlüssel eingesammelt: " + (keyCollected ? "Ja" : "Nein"));
            }

            DrawRoom();

            //=== Spielschleife ===//
            while (!isGameOver)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                int newX = playerX;
                int newY = playerY;

                // Verschiedene States je nach Tasteneingabe
                switch (keyInfo.Key)
                {
                    case ConsoleKey.UpArrow:
                        newY--;
                        break;
                    case ConsoleKey.DownArrow:
                        newY++;
                        break;
                    case ConsoleKey.LeftArrow:
                        newX--;
                        break;
                    case ConsoleKey.RightArrow:
                        newX++;
                        break;
                    default:
                        continue; // Andere Tasten werden hier ignoriert
                }

                // Prüfen, ob neue Position innerhalb des Raums liegt
                if (newX < 0 || newX >= width || newY < 0 || newY >= height)
                    continue;


                // Zielposition
                char targetCell = room[newY, newX];

                // Spieler darf nicht Position einer Wand einnehmen
                if (targetCell == wall)
                    continue;

                // Prüfen, ob es sich um die verschlossene Tür handelt
                if (targetCell == doorClosed)
                {
                    // Wenn Schlüssel noch nicht eingesammelt ist, nicht bewegen
                    if (!keyCollected)
                        continue;
                    else
                    {
                        // Tür öffnen
                        targetCell = doorOpened;
                        room[doorY, doorX] = doorOpened;
                        Console.Beep();
                    }
                }

                //=== Alte Spielerposition als Boden markieren ===//
                room[playerY, playerX] = floor;
                playerX = newX;
                playerY = newY;

                // Schlüssel einsammeln, falls vorhanden
                if(newX == keyX && newY == keyY)
                {
                    if (!keyCollected)
                        Console.Beep();
                    keyCollected = true;
                }

                // Falls der Spieler sich mit dem Schlüssel an der Tür befindet ist das Spiel vorbei
                if(playerX == doorX && playerY == doorY && room[doorY, doorX] == doorOpened)
                {
                    DrawRoom();
                    Console.WriteLine("Glückwunsch! Du hast dich erfolgreich durch den Escape Room navigiert und bist entkommen.");
                    isGameOver = true;
                    break;
                }

                // Spieler an der neuen Position platzieren
                room[playerY, playerX] = player;

                DrawRoom();
            }
            Console.WriteLine("Drücke eine beliebige Taste zum Beenden...");
        }
    }
}