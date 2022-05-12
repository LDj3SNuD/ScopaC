using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace ScopaC
{
    public class Program
    {
        public const int WindowWidth = 71; // Max 9 cards on the table.
        public const int WindowHeight = 41;

        public const int CardsPerPlayer = 3;
        public const int CardsOnTable = 4;

        public static void Main(string[] args)
        {
            Console.Title = "ScopaC";

            Console.SetWindowPosition(0, 0);

            Console.WindowWidth = WindowWidth;
            Console.WindowHeight = WindowHeight;

            Console.BufferWidth = Console.WindowWidth;
            Console.BufferHeight = Console.WindowHeight;

            Console.OutputEncoding = System.Text.Encoding.Unicode;
            Console.CursorVisible = false;

            Console.TreatControlCAsInput = true;

            Console.Clear();
            Console.ResetColor();

            Player player0 = new(id: 0, x: 3, y: WindowHeight / 2 + 6);
            Player player1 = new(id: 1, x: 3, y: WindowHeight / 2 - 6);

            Dealer dealer = new(x: 3, y: WindowHeight / 2 - 2, player0, player1);

            PrintWall();
            Dealer.PrintWall();
            PrintControls();

            player0.Print();
            player1.Print();

            Sleep(1000);

            int round = 0;

            while (GetGameOverState(player0, player1) == GameOverState.None)
            {
                round++;

                int hand = 0;

                Player lastPlayerToTake = null;

                dealer.Deck.InitAndShuffleCards();
                dealer.Deck.Print();
                Sleep(1000);

                dealer.GetNextPlayer();

                while (dealer.Deck.Count != 0)
                {
                    hand++;

                    lastPlayerToTake = null;

                    Player player = dealer.GetNextPlayer();
                    player.Active = true;
                    dealer.Deck.GiveCards(player.Cards, CardsPerPlayer);
                    player.Cards.ActivateCards(player.Active);
                    player.Cards.HideCards(player.Id != 0);
                    player.Cards.Print(delayed: true);
                    dealer.Deck.Print();
                    Sleep(1000);

                    player = dealer.GetNextPlayer();
                    player.Active = false;
                    dealer.Deck.GiveCards(player.Cards, CardsPerPlayer);
                    player.Cards.ActivateCards(player.Active);
                    player.Cards.HideCards(player.Id != 0);
                    player.Cards.Print(delayed: true);
                    dealer.Deck.Print();
                    Sleep(1000);

                    if (hand == 1)
                    {
                        dealer.Deck.GiveCards(dealer.Cards, CardsOnTable);
                        dealer.Cards.Print(delayed: true);
                        dealer.Deck.Print();
                        Sleep(1000);
                    }

                    if (round == 1 && hand == 1)
                    {
                        PrintControls(clear: true);
                    }

                    while (dealer.GetPlayersCardsCount() != 0)
                    {
                        player = dealer.GetNextPlayer();
                        player.Active = true;
                        player.Cards.ActivateCards(player.Active);
                        player.Cards.Print();

                        if (player.Id == 0)
                        {
                            if (player.Cards.TryEnableHighlight())
                            {
                                player.Cards.Print();
                            }

                            bool ready = false;

                            while (!ready)
                            {
                                ConsoleKeyInfo cKI = Console.ReadKey(true);
                                ThreadPool.QueueUserWorkItem((_) => { while (Console.KeyAvailable && Console.ReadKey(true) == cKI); });

                                switch (cKI.Key)
                                {
                                    case ConsoleKey.UpArrow:
                                    {
                                        if (dealer.Cards.TryEnableHighlight())
                                        {
                                            dealer.Cards.Print();

                                            BeepAsync(frequencyA: 800, frequencyB: 800, duration: 2);
                                        }

                                        if (player.Cards.TryDisableHighlight())
                                        {
                                            player.Cards.Print();
                                        }

                                        break;
                                    }

                                    case ConsoleKey.LeftArrow:
                                    {
                                        if (dealer.Cards.TryHighlightPrevCard())
                                        {
                                            dealer.Cards.Print();

                                            BeepAsync(frequencyA: 800, duration: 1);
                                        }

                                        if (player.Cards.TryHighlightPrevCard())
                                        {
                                            player.Cards.Print();

                                            BeepAsync(frequencyA: 800, duration: 1);
                                        }

                                        break;
                                    }

                                    case ConsoleKey.DownArrow:
                                    {
                                        if (dealer.Cards.TryDisableHighlight())
                                        {
                                            dealer.Cards.Print();
                                        }

                                        if (player.Cards.TryEnableHighlight())
                                        {
                                            player.Cards.Print();

                                            BeepAsync(frequencyA: 800, frequencyB: 800, duration: 2);
                                        }

                                        break;
                                    }

                                    case ConsoleKey.RightArrow:
                                    {
                                        if (dealer.Cards.TryHighlightNextCard())
                                        {
                                            dealer.Cards.Print();

                                            BeepAsync(frequencyA: 800, duration: 1);
                                        }

                                        if (player.Cards.TryHighlightNextCard())
                                        {
                                            player.Cards.Print();

                                            BeepAsync(frequencyA: 800, duration: 1);
                                        }

                                        break;
                                    }

                                    case ConsoleKey.S:
                                    {
                                        if (dealer.Cards.TrySwitchSelectionHighlightedCard(Selection.Down, out bool selected, multiSelect: true))
                                        {
                                            dealer.Cards.Print();

                                            BeepAsync(frequencyA: selected ? 900 : 700, duration: 125);
                                        }

                                        if (player.Cards.TrySwitchSelectionHighlightedCard(Selection.Down, out selected))
                                        {
                                            player.Cards.Print();

                                            BeepAsync(frequencyA: selected ? 900 : 700, duration: 125);
                                        }

                                        break;
                                    }

                                    case ConsoleKey.Enter:
                                    {
                                        dealer.Cards.TryDisableHighlight();
                                        player.Cards.TryDisableHighlight();

                                        if (player.TryTakeOrPlaceSelectedCards(dealer, out Take take))
                                        {
                                            dealer.Cards.Print();
                                            player.Cards.Print();

                                            if (take != Take.None)
                                            {
                                                lastPlayerToTake = player;

                                                player.Deck.Print();

                                                if (take == Take.IsSettebello)
                                                {
                                                    BeepAsync(frequencyA: 1000, duration: 250);
                                                }
                                                else if (take == Take.IsScopa)
                                                {
                                                    BeepAsync(frequencyA: 1500, duration: 250);
                                                }
                                            }

                                            ready = true;
                                        }
                                        else
                                        {
                                            player.Cards.TryEnableHighlight();

                                            dealer.Cards.Print();
                                            player.Cards.Print();

                                            BeepAsync(frequencyA: 500, duration: 500);
                                        }

                                        break;
                                    }

                                    case ConsoleKey.Escape:
                                    {
                                        Exit();

                                        return;
                                    }
                                }
                            }
                        }
                        else if (player.Id == 1)
                        {
                            player.HandleAI(dealer, out Card playerCardOut, out var dealerCardsOut);

                            Sleep(1000);

                            playerCardOut.Hidden = false;
                            playerCardOut.Selected = Selection.Up;

                            dealerCardsOut.ForEach((card) => card.Selected = Selection.Up);

                            player.Cards.Print();
                            dealer.Cards.Print();

                            BeepAsync(frequencyA: 900, duration: 125);

                            Sleep(2000);

                            if (player.TryTakeOrPlaceSelectedCards(dealer, out Take take))
                            {
                                if (take != Take.None)
                                {
                                    lastPlayerToTake = player;

                                    player.Deck.Print();

                                    if (take == Take.IsSettebello)
                                    {
                                        BeepAsync(frequencyA: 1000, duration: 250);
                                    }
                                    else if (take == Take.IsScopa)
                                    {
                                        BeepAsync(frequencyA: 1500, duration: 250);
                                    }
                                }
                            }

                            player.Cards.Print();
                            dealer.Cards.Print();
                        }

                        Sleep(1000);

                        player.Active = false;
                        player.Cards.ActivateCards(player.Active);
                        player.Cards.Print();
                    }

                    if (dealer.Cards.TryMoveEmptyCards())
                    {
                        dealer.Cards.Print();

                        Sleep(1000);
                    }
                }

                if (dealer.Cards.Count != 0)
                {
                    //? The remaining cards on the table, not assignable to a last player who took (previous hand), should return to the main deck.
                    Trace.Assert(lastPlayerToTake != null);

                    var dealerCards = dealer.Cards.NotEmptyCards;

                    bool isSettebello = dealerCards.Exists((card) => card.IsSettebello());

                    lastPlayerToTake.Deck.AddCards(dealerCards);
                    dealer.Cards.ReplaceCardsWithEmptyCard(dealerCards);

                    dealer.Cards.Print();
                    lastPlayerToTake.Deck.Print();

                    if (isSettebello)
                    {
                        BeepAsync(frequencyA: 1000, duration: 250);
                    }

                    Sleep(1000);
                }

                dealer.AwardPointsAndPrintReport();

                while (true)
                {
                    ConsoleKeyInfo cKI = Console.ReadKey(true);
                    ThreadPool.QueueUserWorkItem((_) => { while (Console.KeyAvailable && Console.ReadKey(true) == cKI); });

                    if (cKI.Key == ConsoleKey.Enter)
                    {
                        break;
                    }
                    else if (cKI.Key == ConsoleKey.Escape)
                    {
                        Exit();

                        return;
                    }
                }

                Dealer.PrintWall();

                player0.Print();
                player1.Print();

                player0.Deck.ClearCards();
                player1.Deck.ClearCards();

                player0.Deck.Print();
                player1.Deck.Print();

                Sleep(1000);
            }

            if (GetGameOverState(player0, player1) == GameOverState.YouWin)
            {
                player0.Print(GameOverState.YouWin);
                player1.Print(GameOverState.YouWin);

                BeepAsync(frequencyA: 1000, frequencyB: 1500, duration: 1000, ramp: true);
            }
            else
            {
                player0.Print(GameOverState.YouLose);
                player1.Print(GameOverState.YouLose);

                BeepAsync(frequencyA: 1000, frequencyB: 500, duration: 1000, ramp: true);
            }

            while (Console.ReadKey(true).Key != ConsoleKey.Escape);

            Exit();
        }

        private static void PrintWall(int left = 1, int top = 1)
        {
            const ConsoleColor WallColor = ConsoleColor.Gray;

            const int width = WindowWidth - 2;
            const int height = WindowHeight - 2;

            Write(    "╔" + new String('═', width - 2) + "╗", left, top, WallColor);
            for (int i = 1; i <= height - 2; i++)
            {
                Write("║" + new String(' ', width - 2) + "║", left, top + i, WallColor);
            }
            Write(    "╚" + new String('═', width - 2) + "╝", left, top + height - 1, WallColor);
        }

        private static void PrintControls(int left = WindowWidth - 32, bool clear = false)
        {
            const ConsoleColor WallColor = ConsoleColor.Gray;

            const int heightHalf = Program.WindowHeight / 2;

            if (!clear)
            {
                Write("┌ Controls ─────────────────┐", left, heightHalf - 3, WallColor);
                Write("│                           │", left, heightHalf - 2, WallColor);
                Write("│                           │", left, heightHalf - 1, WallColor);
                Write("│                           │", left, heightHalf + 0, WallColor);
                Write("│                           │", left, heightHalf + 1, WallColor);
                Write("│                           │", left, heightHalf + 2, WallColor);
                Write("└───────────────────────────┘", left, heightHalf + 3, WallColor);

                Write("↑ ← ↓ → │ Highlight      ", left + 2, heightHalf - 2, ConsoleColor.DarkGray);
                Write("S       │ Select/Unselect", left + 2, heightHalf - 1, ConsoleColor.DarkGray);
                Write("Enter   │ Confirm        ", left + 2, heightHalf + 0, ConsoleColor.DarkGray);
                Write("SB      │ Speed up       ", left + 2, heightHalf + 1, ConsoleColor.DarkGray);
                Write("Escape  │ Exit           ", left + 2, heightHalf + 2, ConsoleColor.DarkGray);
            }
            else
            {
                for (int i = -3; i <= 3; i++)
                {
                    Write(new String(' ', 29), left, heightHalf + i, ConsoleColor.Black);
                }
            }
        }

        private static GameOverState GetGameOverState(Player player0, Player player1)
        {
            return (player0.Points, player1.Points) switch
            {
                (>= 11, <  11) => GameOverState.YouWin,
                (<  11, >= 11) => GameOverState.YouLose,
                (>  11, >= 11) when player0.Points > player1.Points => GameOverState.YouWin,
                (>= 11, >  11) when player0.Points < player1.Points => GameOverState.YouLose,
                (_, _) => GameOverState.None
            };
        }

        private static void Exit()
        {
            Console.OutputEncoding = System.Text.Encoding.Default;
            Console.CursorVisible = true;

            Console.TreatControlCAsInput = false;

            Console.Clear();
            Console.ResetColor();
        }

        private static readonly object _lock = new();

        public static void Write(string str, int left, int top, ConsoleColor fColor = ConsoleColor.Gray, ConsoleColor bColor = ConsoleColor.Black)
        {
            lock (_lock)
            {
                Console.ForegroundColor = fColor;
                Console.BackgroundColor = bColor;

                Console.SetCursorPosition(left, top);
                Console.Write(str);

                Console.ResetColor();
            }
        }

        public static void Sleep(int millisecondsTimeout)
        {
            Stopwatch sW = Stopwatch.StartNew();

            int count = 0;

            while (millisecondsTimeout > (int)sW.ElapsedMilliseconds)
            {
                Thread.Sleep(10);

                if (count < 10 && Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Spacebar)
                {
                    millisecondsTimeout -= 100;

                    count++;
                }
            }

            sW.Stop();
        }

        private static void BeepAsync(int frequencyA = 800, int frequencyB = 0, int duration = 200, bool ramp = false, int steps = 4)
        {
            ThreadPool.QueueUserWorkItem((_) =>
            {
                if (!ramp)
                {
                    if (frequencyA != 0)
                    {
                        Console.Beep(frequencyA, frequencyB == 0 ? duration : duration / 2);
                    }

                    if (frequencyB != 0)
                    {
                        Console.Beep(frequencyB, frequencyA == 0 ? duration : duration / 2);
                    }
                }
                else
                {
                    int frequencyStep = (frequencyA - frequencyB) / (steps - 1);
                    duration /= steps;

                    for (int i = 1; i <= steps; i++)
                    {
                        if (frequencyA != 0)
                        {
                            Console.Beep(frequencyA, duration);
                        }

                        frequencyA -= frequencyStep;
                    }
                }
            });
        }
    }

    public enum GameOverState { None, YouWin, YouLose }

    public enum Seed { None, Heart, Diamond, Club, Spade }

    public enum Selection { None, Up, Down }

    public enum Take { None, IsTake, IsSettebello, IsScopa }

    public record Position
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Position(int x = 0, int y = 0)
        {
            X = x;
            Y = y;
        }
    }

    public record Card
    {
        public Seed Seed { get; private set; }
        public int Value { get; private set; }

        public bool Half { get; set; } // Scopa.

        public bool Active { get; set; }

        public bool Hidden { get; set; } // Face-up (default).

        public bool Highlighted { get; set; }
        public Selection Selected { get; set; }

        public Card(Seed seed = Seed.None, int value = 0)
        {
            Seed = seed;
            Value = value;

            Active = true;
        }

        public bool IsEmpty()
        {
            return Seed == Seed.None && Value == 0;
        }

        public bool IsSettebello()
        {
            return Seed == Seed.Diamond && Value == 7;
        }

        public void Print(Position position)
        {
            if (IsEmpty())
            {
                Program.Write("     ", position.X, position.Y - 1, ConsoleColor.Black);

                Program.Write("     ", position.X, position.Y + 0, ConsoleColor.Black);
                Program.Write("     ", position.X, position.Y + 1, ConsoleColor.Black);
                Program.Write("     ", position.X, position.Y + 2, ConsoleColor.Black);
                Program.Write("     ", position.X, position.Y + 3, ConsoleColor.Black);
                Program.Write("     ", position.X, position.Y + 4, ConsoleColor.Black);

                Program.Write("     ", position.X, position.Y + 5, ConsoleColor.Black);

                return;
            }

            int offsetY;

            switch (Selected)
            {
                case Selection.Up:
                {
                    Program.Write("     ", position.X, position.Y + 4, ConsoleColor.Black);
                    Program.Write("     ", position.X, position.Y + 5, ConsoleColor.Black);

                    offsetY = -1;

                    break;
                }

                case Selection.Down:
                {
                    Program.Write("     ", position.X, position.Y - 1, ConsoleColor.Black);
                    Program.Write("     ", position.X, position.Y + 0, ConsoleColor.Black);

                    offsetY = 1;

                    break;
                }

                default:
                {
                    Program.Write("     ", position.X, position.Y - 1, ConsoleColor.Black);
                    Program.Write("     ", position.X, position.Y + 5, ConsoleColor.Black);

                    offsetY = 0;

                    break;
                };
            }

            ConsoleColor bColor = !Active ? ConsoleColor.DarkGray : ConsoleColor.White;

            if (!Highlighted)
            {
                Program.Write(!Half ? "┌───┐" : "┌──", position.X, position.Y + offsetY + 0, ConsoleColor.Black, !Half ? bColor : ConsoleColor.Gray);
                Program.Write(!Half ? "│   │" : "│  ", position.X, position.Y + offsetY + 1, ConsoleColor.Black, !Half ? bColor : ConsoleColor.Gray);
                Program.Write(!Half ? "│   │" : "│  ", position.X, position.Y + offsetY + 2, ConsoleColor.Black, !Half ? bColor : ConsoleColor.Gray);
                Program.Write(!Half ? "│   │" : "│  ", position.X, position.Y + offsetY + 3, ConsoleColor.Black, !Half ? bColor : ConsoleColor.Gray);
                Program.Write(!Half ? "└───┘" : "└──", position.X, position.Y + offsetY + 4, ConsoleColor.Black, !Half ? bColor : ConsoleColor.Gray);
            }
            else
            {
                Program.Write("╔═══╗", position.X, position.Y + offsetY + 0, ConsoleColor.Black, bColor);
                Program.Write("║   ║", position.X, position.Y + offsetY + 1, ConsoleColor.Black, bColor);
                Program.Write("║   ║", position.X, position.Y + offsetY + 2, ConsoleColor.Black, bColor);
                Program.Write("║   ║", position.X, position.Y + offsetY + 3, ConsoleColor.Black, bColor);
                Program.Write("╚═══╝", position.X, position.Y + offsetY + 4, ConsoleColor.Black, bColor);
            }

            if (!Hidden)
            {
                string strSeed = Seed switch
                {
                    Seed.Heart   => "♥",
                    Seed.Diamond => "♦",
                    Seed.Club    => "♣",
                    Seed.Spade   => "♠",
                    _ => throw new Exception(nameof(Seed))
                };

                ConsoleColor fColor = Seed switch
                {
                    Seed.Heart   => ConsoleColor.Red,
                    Seed.Diamond => ConsoleColor.Red,
                    Seed.Club    => ConsoleColor.Black,
                    Seed.Spade   => ConsoleColor.Black,
                    _ => throw new Exception(nameof(Seed))
                };

                Program.Write(!Half ? $"{Value,2} "  : $"{Value,2}",  position.X + 1, position.Y + offsetY + 1, fColor, !Half ? bColor : ConsoleColor.Gray);
                Program.Write(!Half ? "   "          : "  ",          position.X + 1, position.Y + offsetY + 2, fColor, !Half ? bColor : ConsoleColor.Gray);
                Program.Write(!Half ? $" {strSeed} " : $" {strSeed}", position.X + 1, position.Y + offsetY + 3, fColor, !Half ? bColor : ConsoleColor.Gray);
            }
            else
            {
                Program.Write("░░░", position.X + 1, position.Y + offsetY + 1, ConsoleColor.Black, bColor);
                Program.Write("░░░", position.X + 1, position.Y + offsetY + 2, ConsoleColor.Black, bColor);
                Program.Write("░░░", position.X + 1, position.Y + offsetY + 3, ConsoleColor.Black, bColor);
            }
        }
    }

    public class DealerDeck
    {
        public const int CardsInDeck = 4 * 10;

        public Position Position { get; }

        public int Count => _cards.Count;

        private readonly List<Card> _cards;

        public DealerDeck(int x, int y)
        {
            Position = new(x, y);

            _cards = new();
        }

        public void InitAndShuffleCards()
        {
            bool IsReady()
            {
                if (_cards.Count == CardsInDeck)
                {
                    int count = 0;

                    for (int i = Program.CardsOnTable - 1; i >= 0; i--)
                    {
                        if (_cards[i + CardsInDeck - Program.CardsPerPlayer * 2 - Program.CardsOnTable].Value == 10)
                        {
                            count++;
                        }
                    }

                    if (count < 3)
                    {
                        return true;
                    }

                    _cards.Clear();
                }

                return false;
            }

            _cards.Clear();

            Random rndSeed = new();
            Random rndValue = new();

            do
            {
                Card card = new((Seed)rndSeed.Next((int)Seed.Heart, (int)Seed.Spade + 1), rndValue.Next(1, 11));

                if (!_cards.Contains(card))
                {
                    _cards.Add(card);
                }
            }
            while (!IsReady());
        }

        public void GiveCards(Cards cards, int count)
        {
            for (int i = 1; i <= count; i++)
            {
                Card card = _cards[^1];

                cards.AddCard(card);

                _cards.Remove(card);
            }
        }

        private int _lastCount;

        public void Print()
        {
            if (_cards.Count != 0)
            {
                Program.Write("┌───┐", Position.X, Position.Y + 0, ConsoleColor.Black, ConsoleColor.White);
                Program.Write("│   │", Position.X, Position.Y + 1, ConsoleColor.Black, ConsoleColor.White);
                Program.Write("│   │", Position.X, Position.Y + 2, ConsoleColor.Black, ConsoleColor.White);
                Program.Write("│   │", Position.X, Position.Y + 3, ConsoleColor.Black, ConsoleColor.White);
                Program.Write("└───┘", Position.X, Position.Y + 4, ConsoleColor.Black, ConsoleColor.White);

                Program.Write("░░░", Position.X + 1, Position.Y + 1, ConsoleColor.Black, ConsoleColor.White);
                Program.Write("░░░", Position.X + 1, Position.Y + 2, ConsoleColor.Black, ConsoleColor.White);
                Program.Write("░░░", Position.X + 1, Position.Y + 3, ConsoleColor.Black, ConsoleColor.White);

                Program.Write($"{_cards.Count:D2}", Position.X + 6, Position.Y + 2, ConsoleColor.Black, ConsoleColor.White);
            }
            else
            {
                Program.Write("     ", Position.X, Position.Y + 0, ConsoleColor.Black);
                Program.Write("     ", Position.X, Position.Y + 1, ConsoleColor.Black);
                Program.Write("     ", Position.X, Position.Y + 2, ConsoleColor.Black);
                Program.Write("     ", Position.X, Position.Y + 3, ConsoleColor.Black);
                Program.Write("     ", Position.X, Position.Y + 4, ConsoleColor.Black);

                Program.Write("  ", Position.X + 6, Position.Y + 2, ConsoleColor.Black);
            }

            if (_cards.Count != _lastCount)
            {
                if (_cards.Count != 0 && _lastCount != 0)
                {
                    int diff = Math.Abs(_cards.Count - _lastCount);
                    string sign = _cards.Count - _lastCount >= 0 ? "+" : "-";

                    ThreadPool.QueueUserWorkItem((_) =>
                    {
                        Program.Write($"{sign}{diff}", Position.X + 6, Position.Y + 1, ConsoleColor.White);
                        Thread.Sleep(333);
                        Program.Write($"{sign}{diff}", Position.X + 6, Position.Y + 1, ConsoleColor.Gray);
                        Thread.Sleep(333);
                        Program.Write($"{sign}{diff}", Position.X + 6, Position.Y + 1, ConsoleColor.DarkGray);
                        Thread.Sleep(333);
                        Program.Write("  ", Position.X + 6, Position.Y + 1, ConsoleColor.Black);
                    });
                }

                _lastCount = _cards.Count;
            }
        }
    }

    public class PlayerDeck
    {
        public Position Position { get; }

        public List<Card> Cards { get; }

        public PlayerDeck(int x, int y)
        {
            Position = new(x, y);

            Cards = new();
        }

        public void AddCards(List<Card> cards)
        {
            cards.ForEach((card) => Cards.Add(card));
        }

        public void ClearCards()
        {
            Cards.Clear();
        }

        private int _lastCount;

        public void Print()
        {
            const int MaxScope = 4;

            if (Cards.Count != 0)
            {
                Program.Write("┌───┐", Position.X, Position.Y + 0, ConsoleColor.Black, ConsoleColor.White);
                Program.Write("│   │", Position.X, Position.Y + 1, ConsoleColor.Black, ConsoleColor.White);
                Program.Write("│   │", Position.X, Position.Y + 2, ConsoleColor.Black, ConsoleColor.White);
                Program.Write("│   │", Position.X, Position.Y + 3, ConsoleColor.Black, ConsoleColor.White);
                Program.Write("└───┘", Position.X, Position.Y + 4, ConsoleColor.Black, ConsoleColor.White);

                Program.Write("░░░", Position.X + 1, Position.Y + 1, ConsoleColor.Black, ConsoleColor.White);
                Program.Write("░░░", Position.X + 1, Position.Y + 2, ConsoleColor.Black, ConsoleColor.White);
                Program.Write("░░░", Position.X + 1, Position.Y + 3, ConsoleColor.Black, ConsoleColor.White);

                Program.Write($"{Cards.Count:D2}", Position.X + 6, Position.Y + 2, ConsoleColor.Black, ConsoleColor.White);

                /**/

                List<Card> scope = new();

                for (int i = Cards.Count - 1; i >= 0; i--)
                {
                    Card card = Cards[i];

                    if (card.Half)
                    {
                        if (scope.Count < MaxScope)
                        {
                            scope.Insert(0, card);
                        }
                    }
                }

                for (int i = 1; i <= scope.Count; i++)
                {
                    Card card = scope[i - 1];

                    card.Print(new(x: Position.X - i * 3, y: Position.Y));
                }
            }
            else
            {
                Program.Write(new String(' ', MaxScope * 3 + 5), Position.X - MaxScope * 3, Position.Y + 0, ConsoleColor.Black);
                Program.Write(new String(' ', MaxScope * 3 + 5), Position.X - MaxScope * 3, Position.Y + 1, ConsoleColor.Black);
                Program.Write(new String(' ', MaxScope * 3 + 5), Position.X - MaxScope * 3, Position.Y + 2, ConsoleColor.Black);
                Program.Write(new String(' ', MaxScope * 3 + 5), Position.X - MaxScope * 3, Position.Y + 3, ConsoleColor.Black);
                Program.Write(new String(' ', MaxScope * 3 + 5), Position.X - MaxScope * 3, Position.Y + 4, ConsoleColor.Black);

                Program.Write("  ", Position.X + 6, Position.Y + 2, ConsoleColor.Black);
            }

            if (Cards.Count != _lastCount)
            {
                if (Cards.Count != 0 && _lastCount != 0)
                {
                    int diff = Math.Abs(Cards.Count - _lastCount);
                    string sign = Cards.Count - _lastCount >= 0 ? "+" : "-";

                    ThreadPool.QueueUserWorkItem((_) =>
                    {
                        Program.Write($"{sign}{diff}", Position.X + 6, Position.Y + 1, ConsoleColor.White);
                        Thread.Sleep(333);
                        Program.Write($"{sign}{diff}", Position.X + 6, Position.Y + 1, ConsoleColor.Gray);
                        Thread.Sleep(333);
                        Program.Write($"{sign}{diff}", Position.X + 6, Position.Y + 1, ConsoleColor.DarkGray);
                        Thread.Sleep(333);
                        Program.Write("  ", Position.X + 6, Position.Y + 1, ConsoleColor.Black);
                    });
                }

                _lastCount = Cards.Count;
            }
        }
    }

    public class Cards
    {
        public Position Position { get; }

        public int Count => _cards.FindAll((card) => !card.IsEmpty()).Count;

        public List<Card> NotEmptyCards => _cards.FindAll((card) => !card.IsEmpty());
        public List<Card> SelectedCards => _cards.FindAll((card) => card.Selected != Selection.None);

        private readonly List<Card> _cards;

        public Cards(int x, int y)
        {
            Position = new(x, y);

            _cards = new();
        }

        public void AddCard(Card card)
        {
            int index = _cards.FindIndex((card) => card.IsEmpty());

            if (index != -1)
            {
                _cards[index] = card;
            }
            else
            {
                _cards.Add(card);
            }
        }

        public void ReplaceCardsWithEmptyCard(List<Card> cards)
        {
            foreach (Card card in cards)
            {
                int index = _cards.IndexOf(card);

                if (index != -1)
                {
                    _cards[index] = new();
                }
            }
        }

        public bool TryMoveEmptyCards()
        {
            var cardsOld = new List<Card>(_cards);

            int count = _cards.RemoveAll((card) => card.IsEmpty());

            for (int i = 1; i <= count; i++)
            {
                _cards.Add(new());
            }

            return !_cards.SequenceEqual(cardsOld);
        }

        public void ActivateCards(bool active)
        {
            _cards.ForEach((card) => card.Active = active);
        }

        public void HideCards(bool hidden)
        {
            _cards.ForEach((card) => card.Hidden = hidden);
        }

        public void UnselectCards()
        {
            _cards.ForEach((card) => card.Selected = Selection.None);
        }

        public bool TryEnableHighlight()
        {
            Card card = _cards.Find((card) => card.Highlighted);

            if (card == null)
            {
                card = _cards.Find((card) => !card.IsEmpty());

                if (card != null)
                {
                    card.Highlighted = true;

                    return true;
                }
            }

            return false;
        }

        public bool TryDisableHighlight()
        {
            Card card = _cards.Find((card) => card.Highlighted);

            if (card != null)
            {
                card.Highlighted = false;

                return true;
            }

            return false;
        }

        public bool TryHighlightPrevCard()
        {
            int cardIndex = _cards.FindIndex((card) => card.Highlighted);

            if (cardIndex != -1)
            {
                for (int i = cardIndex - 1; i >= 0; i--)
                {
                    if (!_cards[i].IsEmpty())
                    {
                        _cards[cardIndex].Highlighted = false;
                        _cards[i].Highlighted = true;

                        return true;
                    }
                }
            }

            return false;
        }

        public bool TryHighlightNextCard()
        {
            int cardIndex = _cards.FindIndex((card) => card.Highlighted);

            if (cardIndex != -1)
            {
                for (int i = cardIndex + 1; i < _cards.Count; i++)
                {
                    if (!_cards[i].IsEmpty())
                    {
                        _cards[cardIndex].Highlighted = false;
                        _cards[i].Highlighted = true;

                        return true;
                    }
                }
            }

            return false;
        }

        public bool TrySwitchSelectionHighlightedCard(Selection selection, out bool selected, bool multiSelect = false)
        {
            selected = false;

            Card card = _cards.Find((card) => card.Highlighted);

            if (card != null)
            {
                if (card.Selected == Selection.None)
                {
                    if (!multiSelect)
                    {
                        _cards.ForEach((card) => card.Selected = Selection.None);
                    }

                    card.Selected = selection;

                    selected = true;
                }
                else
                {
                    card.Selected = Selection.None;
                }

                return true;
            }

            return false;
        }

        public void Print(bool delayed = false)
        {
            for (int i = 0; i < _cards.Count; i++)
            {
                Card card = _cards[i];

                card.Print(new(x: Position.X + i * 6, y: Position.Y));

                if (delayed)
                {
                    Program.Sleep(500);
                }
            }
        }
    }

    public class Dealer
    {
        public Position Position { get; }

        public DealerDeck Deck { get; }
        public Cards Cards { get; }

        private List<Player> _players;
        private Random _rnd;
        private int _cnt;

        public Dealer(int x, int y, params Player[] players)
        {
            Position = new(x, y);

            Deck = new(x, y);
            Cards = new(x + 12, y);

            _players = new();

            foreach (Player player in players)
            {
                _players.Add(player);
            }

            _rnd = new();

            _cnt = _rnd.Next(0, _players.Count);
        }

        public Player GetNextPlayer()
        {
            return _players[++_cnt % _players.Count];
        }

        public int GetPlayersCardsCount()
        {
            int count = 0;

            _players.ForEach((player) => count += player.Cards.Count);

            return count;
        }

        public void AwardPointsAndPrintReport(int left = 1)
        {
            const ConsoleColor WallColor = ConsoleColor.Gray;

            const int heightHalf = Program.WindowHeight / 2;

            Program.Write("╟──────────┬──────────┬──────────┬──────────┬──────────╥────────────╢", left, heightHalf - 4, WallColor);
            Program.Write("║          │          │          │          │          ║            ║", left, heightHalf - 3, WallColor);
            Program.Write("╟──────────┼──────────┼──────────┼──────────┼──────────╫────────────╢", left, heightHalf - 2, WallColor);
            Program.Write("║          │ Number   │ Number   │          │          ║            ║", left, heightHalf - 1, WallColor);
            Program.Write("║ Scope    │ of       │ of       │ Sette    │ Primiera ║ Points     ║", left, heightHalf + 0, WallColor);
            Program.Write("║          │ cards    │ coins    │ bello    │          ║            ║", left, heightHalf + 1, WallColor);
            Program.Write("╟──────────┼──────────┼──────────┼──────────┼──────────╫────────────╢", left, heightHalf + 2, WallColor);
            Program.Write("║          │          │          │          │          ║            ║", left, heightHalf + 3, WallColor);
            Program.Write("╟──────────┴──────────┴──────────┴──────────┴──────────╨────────────╢", left, heightHalf + 4, WallColor);

            void PrintInfo(int offsetX, bool onTop, int val0, int? val1 = null, bool revColors = false)
            {
                string str = $"{(val0 != 0 ? "+" : String.Empty)}{val0}{(val1 != null ? $" ({val1})" : String.Empty)}";

                ConsoleColor fColor = !revColors ? (val0 != 0 ? ConsoleColor.White : ConsoleColor.DarkGray) : ConsoleColor.Black;
                ConsoleColor bColor = !revColors ? ConsoleColor.Black : (val0 != 0 ? ConsoleColor.White : ConsoleColor.DarkGray);

                Program.Write(str, left + offsetX, heightHalf + (onTop ? -3 : 3), fColor, bColor);
            }

            Player player0 = _players[0];
            Player player1 = _players[1];

            Trace.Assert(player0.Id == 0);
            Trace.Assert(player1.Id == 1);

            var deckCards0 = player0.Deck.Cards;
            var deckCards1 = player1.Deck.Cards;

            int points0 = 0;
            int points1 = 0;

            /**/

            int scope0 = 0;
            int scope1 = 0;

            deckCards0.ForEach((card) => { if (card.Half) scope0++; });
            deckCards1.ForEach((card) => { if (card.Half) scope1++; });

            PrintInfo(2, false, scope0);
            PrintInfo(2, true,  scope1);

            points0 += scope0;
            points1 += scope1;

            /**/

            if (deckCards0.Count > deckCards1.Count)
            {
                PrintInfo(13, false, 1, deckCards0.Count);
                PrintInfo(13, true,  0, deckCards1.Count);

                points0++;
            }
            else if (deckCards0.Count < deckCards1.Count)
            {
                PrintInfo(13, false, 0, deckCards0.Count);
                PrintInfo(13, true,  1, deckCards1.Count);

                points1++;
            }
            else
            {
                PrintInfo(13, false, 0, deckCards0.Count);
                PrintInfo(13, true,  0, deckCards1.Count);
            }

            /**/

            int numberOfCoins0 = 0;
            int numberOfCoins1 = 0;

            deckCards0.ForEach((card) => { if (card.Seed == Seed.Diamond) numberOfCoins0++; });
            deckCards1.ForEach((card) => { if (card.Seed == Seed.Diamond) numberOfCoins1++; });

            if (numberOfCoins0 > numberOfCoins1)
            {
                PrintInfo(24, false, 1, numberOfCoins0);
                PrintInfo(24, true,  0, numberOfCoins1);

                points0++;
            }
            else if (numberOfCoins0 < numberOfCoins1)
            {
                PrintInfo(24, false, 0, numberOfCoins0);
                PrintInfo(24, true,  1, numberOfCoins1);

                points1++;
            }
            else
            {
                PrintInfo(24, false, 0, numberOfCoins0);
                PrintInfo(24, true,  0, numberOfCoins1);
            }

            /**/

            if (deckCards0.Exists((card) => card.IsSettebello()))
            {
                PrintInfo(35, false, 1);
                PrintInfo(35, true,  0);

                points0++;
            }
            else if (deckCards1.Exists((card) => card.IsSettebello()))
            {
                PrintInfo(35, false, 0);
                PrintInfo(35, true,  1);

                points1++;
            }
            else
            {
                //? Impossible if there are no cards left on the table after one last hand.
                Trace.Assert(false);

                PrintInfo(35, false, 0);
                PrintInfo(35, true,  0);
            }

            /**/

            Dictionary<Seed, List<int>> prime0 = new();
            Dictionary<Seed, List<int>> prime1 = new();

            foreach (Card card in deckCards0)
            {
                if (!prime0.ContainsKey(card.Seed))
                {
                    prime0.Add(card.Seed, new());
                }

                prime0[card.Seed].Add(Player.PrimieraByCards[card.Value]);
            }

            foreach (Card card in deckCards1)
            {
                if (!prime1.ContainsKey(card.Seed))
                {
                    prime1.Add(card.Seed, new());
                }

                prime1[card.Seed].Add(Player.PrimieraByCards[card.Value]);
            }

            int prime0Sum = 0;
            int prime1Sum = 0;

            if (prime0.Count == 4)
            {
                foreach (var prime in prime0.Values)
                {
                    prime0Sum += prime.Max();
                }
            }

            if (prime1.Count == 4)
            {
                foreach (var prime in prime1.Values)
                {
                    prime1Sum += prime.Max();
                }
            }

            if (prime0Sum > prime1Sum)
            {
                PrintInfo(46, false, 1, prime0Sum);
                PrintInfo(46, true,  0, prime1Sum);

                points0++;
            }
            else if (prime0Sum < prime1Sum)
            {
                PrintInfo(46, false, 0, prime0Sum);
                PrintInfo(46, true,  1, prime1Sum);

                points1++;
            }
            else
            {
                PrintInfo(46, false, 0, prime0Sum);
                PrintInfo(46, true,  0, prime1Sum);
            }

            /**/

            PrintInfo(57, false, points0, revColors: true);
            PrintInfo(57, true,  points1, revColors: true);

            /**/

            player0.Points += points0;
            player1.Points += points1;
        }

        public static void PrintWall(int left = 1)
        {
            const ConsoleColor WallColor = ConsoleColor.Gray;

            const int width = Program.WindowWidth - 2;
            const int heightHalf = Program.WindowHeight / 2;

            Program.Write(    "╟" + new String('─', width - 2) + "╢", left, heightHalf - 4, WallColor);
            for (int i = -3; i <= 3; i++)
            {
                Program.Write("║" + new String(' ', width - 2) + "║", left, heightHalf + i, WallColor);
            }
            Program.Write(    "╟" + new String('─', width - 2) + "╢", left, heightHalf + 4, WallColor);
        }
    }

    public class Player
    {
        public static readonly List<int> PrimieraByCards = new() { 0, 16, 12, 13, 14, 15, 18, 21, 10, 10, 10 };

        public int Id { get; }

        public Position Position { get; }

        public bool Active { get; set; }

        public int Points { get; set; }

        public Cards Cards { get; }
        public PlayerDeck Deck { get; }

        public Player(int id, int x, int y)
        {
            Id = id;

            Position = new(x, y);

            if (id == 0)
            {
                Cards = new(x + 12, y);
                Deck = new(x + 24, y + 7);
            }
            else
            {
                Cards = new(x + 12, y - 4);
                Deck = new(x + 24, y - 11);
            }
        }

        public bool TryTakeOrPlaceSelectedCards(Dealer dealer, out Take take)
        {
            take = Take.None;

            if (!Active)
            {
                return false;
            }

            var dealerSelectedCards = dealer.Cards.SelectedCards;
            dealer.Cards.UnselectCards();

            var playerSelectedCards = Cards.SelectedCards;
            Cards.UnselectCards();

            if (playerSelectedCards.Count != 1)
            {
                return false;
            }

            if (dealerSelectedCards.Count == 0)
            {
                if (GetPowerSet(dealer.Cards.NotEmptyCards).TrueForAll((combo) => combo.Sum((card) => card.Value) != playerSelectedCards[0].Value))
                {
                    dealer.Cards.AddCard(playerSelectedCards[0]);

                    Cards.ReplaceCardsWithEmptyCard(playerSelectedCards);

                    return true;
                }
            }
            else if (dealerSelectedCards.Count == 1)
            {
                if (playerSelectedCards[0].Value == dealerSelectedCards[0].Value)
                {
                    Deck.AddCards(playerSelectedCards);
                    Deck.AddCards(dealerSelectedCards);

                    Cards.ReplaceCardsWithEmptyCard(playerSelectedCards);
                    dealer.Cards.ReplaceCardsWithEmptyCard(dealerSelectedCards);

                    take = Take.IsTake;

                    if (playerSelectedCards[0].IsSettebello() || dealerSelectedCards[0].IsSettebello())
                    {
                        take = Take.IsSettebello;
                    }

                    if (dealer.Cards.Count == 0)
                    {
                        if (dealer.Deck.Count != 0 || dealer.GetPlayersCardsCount() != 0) // If it is not already the last play of a round.
                        {
                            playerSelectedCards[0].Half = true;

                            take = Take.IsScopa;
                        }
                    }

                    return true;
                }
            }
            else
            {
                if (playerSelectedCards[0].Value == dealerSelectedCards.Sum((card) => card.Value) &&
                    !dealer.Cards.NotEmptyCards.Exists((card) => card.Value == playerSelectedCards[0].Value))
                {
                    Deck.AddCards(playerSelectedCards);
                    Deck.AddCards(dealerSelectedCards);

                    Cards.ReplaceCardsWithEmptyCard(playerSelectedCards);
                    dealer.Cards.ReplaceCardsWithEmptyCard(dealerSelectedCards);

                    take = Take.IsTake;

                    if (playerSelectedCards[0].IsSettebello() || dealerSelectedCards.Exists((card) => card.IsSettebello()))
                    {
                        take = Take.IsSettebello;
                    }

                    if (dealer.Cards.Count == 0)
                    {
                        if (dealer.Deck.Count != 0 || dealer.GetPlayersCardsCount() != 0) // If it is not already the last play of a round.
                        {
                            playerSelectedCards[0].Half = true;

                            take = Take.IsScopa;
                        }
                    }

                    return true;
                }
            }

            return false;
        }

        public void HandleAI(Dealer dealer, out Card playerCardOut, out List<Card> dealerCardsOut)
        {
            playerCardOut = null;
            dealerCardsOut = new();

            var dealerCards = dealer.Cards.NotEmptyCards;
            var dealerCardsCombos = GetPowerSet(dealerCards);
            var playerCards = Cards.NotEmptyCards;

            /**/

            List<Card> placeCards = new();
            List<Card> placeCardsUnord = new();
            List<int> CardsByReversePrimiera = new() { 8, 9, 10, 2, 3, 4, 5, 1, 6, 7 }; // const.

            foreach (Card playerCard in playerCards)
            {
                if (dealerCardsCombos.TrueForAll((dealerCardsCombo) => dealerCardsCombo.Sum((card) => card.Value) != playerCard.Value))
                {
                    placeCardsUnord.Add(playerCard);
                }
            }

            foreach (int value in CardsByReversePrimiera)
            {
                foreach (Card placeCard in placeCardsUnord)
                {
                    if (placeCard.Seed != Seed.Diamond && placeCard.Value == value)
                    {
                        placeCards.Add(placeCard);
                    }
                }
            }

            foreach (int value in CardsByReversePrimiera)
            {
                foreach (Card placeCard in placeCardsUnord)
                {
                    if (placeCard.Seed == Seed.Diamond && placeCard.Value == value)
                    {
                        placeCards.Add(placeCard);
                    }
                }
            }

            /**/

            List<(Card playerCard, List<Card> dealerCards, float score)> takeCards = new();

            foreach (Card playerCard in playerCards)
            {
                foreach (var dealerCardsCombo in dealerCardsCombos)
                {
                    if (dealerCardsCombo.Count == 1)
                    {
                        if (playerCard.Value == dealerCardsCombo[0].Value)
                        {
                            float score = 0f;

                            if (dealerCards.Count == 1) score += 2000f;

                            if (playerCard.IsSettebello() || dealerCardsCombo[0].IsSettebello()) score += 1000f;

                            int cnt = 0;
                            if (playerCard.Seed == Seed.Diamond) cnt++;
                            if (dealerCardsCombo[0].Seed == Seed.Diamond) cnt++;
                            score += cnt * 100f;

                            score += 2 * 10f;

                            score += (float)PrimieraByCards[playerCard.Value] / (float)PrimieraByCards.Max();
                            score += (float)PrimieraByCards[dealerCardsCombo[0].Value] / (float)PrimieraByCards.Max();

                            takeCards.Add((playerCard, dealerCardsCombo, score));
                        }
                    }
                    else
                    {
                        if (playerCard.Value == dealerCardsCombo.Sum((card) => card.Value) &&
                            !dealerCards.Exists((card) => card.Value == playerCard.Value))
                        {
                            float score = 0f;

                            if (dealerCardsCombo.Count == dealerCards.Count) score += 2000f;

                            if (playerCard.IsSettebello() || dealerCardsCombo.Exists((card) => card.IsSettebello())) score += 1000f;

                            int cnt = 0;
                            if (playerCard.Seed == Seed.Diamond) cnt++;
                            cnt += dealerCardsCombo.FindAll((card) => card.Seed == Seed.Diamond).Count;
                            score += cnt * 100f;

                            score += (1 + dealerCardsCombo.Count) * 10f;

                            score += (float)PrimieraByCards[playerCard.Value] / (float)PrimieraByCards.Max();
                            dealerCardsCombo.ForEach((card) => score += (float)PrimieraByCards[card.Value] / (float)PrimieraByCards.Max());

                            takeCards.Add((playerCard, dealerCardsCombo, score));
                        }
                    }
                }
            }

            takeCards.Sort((x, y) => y.score.CompareTo(x.score));

            /**/

            if (takeCards.Count == 0)
            {
                Trace.Assert(placeCards.Count != 0);

                if (dealer.Deck.Count != 0 || dealer.GetPlayersCardsCount() != 1) // If it is not yet the last play of a round.
                {
                    int dealerCardsSum = dealerCards.Sum((card) => card.Value);

                    if (dealerCardsSum <= 10)
                    {
                        Card placeCardFind = placeCards.Find((card) => card.Value + dealerCardsSum > 10);

                        if (placeCardFind != null)
                        {
                            playerCardOut = placeCardFind;
                        }
                        else
                        {
                            playerCardOut = placeCards[0];
                        }
                    }
                    else
                    {
                        playerCardOut = placeCards[0];
                    }
                }
                else
                {
                    playerCardOut = placeCards[0];
                }
            }
            else
            {
                if (dealer.Deck.Count != 0 || dealer.GetPlayersCardsCount() != 1) // If it is not yet the last play of a round.
                {
                    if (takeCards[0].score >= 1000f)
                    {
                        playerCardOut = takeCards[0].playerCard;
                        dealerCardsOut = takeCards[0].dealerCards;
                    }
                    else
                    {
                        if (placeCards.Count != 0)
                        {
                            int dealerCardsSum = dealerCards.Sum((card) => card.Value);

                            if (dealerCardsSum <= 10)
                            {
                                Card placeCardFind = placeCards.Find((card) => card.Value + dealerCardsSum > 10);

                                if (placeCardFind != null)
                                {
                                    playerCardOut = placeCardFind;
                                }
                                else
                                {
                                    playerCardOut = takeCards[0].playerCard;
                                    dealerCardsOut = takeCards[0].dealerCards;
                                }
                            }
                            else
                            {
                                if (takeCards[0].score >= 100f)
                                {
                                    playerCardOut = takeCards[0].playerCard;
                                    dealerCardsOut = takeCards[0].dealerCards;
                                }
                                else
                                {
                                    if (dealerCardsSum - takeCards[0].dealerCards.Sum((card) => card.Value) <= 10)
                                    {
                                        playerCardOut = placeCards[0];
                                    }
                                    else
                                    {
                                        playerCardOut = takeCards[0].playerCard;
                                        dealerCardsOut = takeCards[0].dealerCards;
                                    }
                                }
                            }
                        }
                        else
                        {
                            playerCardOut = takeCards[0].playerCard;
                            dealerCardsOut = takeCards[0].dealerCards;
                        }
                    }
                }
                else
                {
                    playerCardOut = takeCards[0].playerCard;
                    dealerCardsOut = takeCards[0].dealerCards;
                }
            }

            Trace.Assert(playerCardOut != null);
        }

        private static List<List<Card>> GetPowerSet(List<Card> cards)
        {
            List<List<Card>> combos = new();

            for (int mask = 1; mask < 1 << cards.Count; mask++)
            {
                List<Card> combo = new();

                for (int idx = 0; idx < cards.Count; idx++)
                {
                    if (((mask >> idx) & 1) == 1)
                    {
                        combo.Add(cards[idx]);
                    }
                }

                combos.Add(combo);
            }

            return combos;
        }

        private int _lastPoints;

        public void Print(GameOverState gameOverState = GameOverState.None)
        {
            if (Id == 0)
            {
                if (gameOverState == GameOverState.YouWin)
                {
                    Program.Write("YOU WIN!", Position.X, Position.Y, ConsoleColor.Black, ConsoleColor.Gray);
                }
                else if (gameOverState == GameOverState.YouLose)
                {
                    Program.Write("YOU LOSE!", Position.X, Position.Y, ConsoleColor.Black, ConsoleColor.Gray);
                }
                else
                {
                    Program.Write("YOU", Position.X, Position.Y, ConsoleColor.Black, ConsoleColor.Gray);
                }

                Program.Write($"Points: {Points:D2}", Position.X, Position.Y + 2);

                if (Points != _lastPoints)
                {
                    int diff = Points - _lastPoints;

                    ThreadPool.QueueUserWorkItem((_) =>
                    {
                        Program.Write($"+{diff}", Position.X + 8, Position.Y + 1, ConsoleColor.Black, ConsoleColor.Gray);
                        Thread.Sleep(500);
                        Program.Write($"+{diff}", Position.X + 8, Position.Y + 1, ConsoleColor.Black, ConsoleColor.DarkGray);
                        Thread.Sleep(500);
                        Program.Write("   ", Position.X + 8, Position.Y + 1, ConsoleColor.Black);
                    });

                    _lastPoints = Points;
                }
            }
            else if (Id == 1)
            {
                if (gameOverState == GameOverState.YouWin)
                {
                    Program.Write("CPU LOSE!", Position.X, Position.Y, ConsoleColor.Black, ConsoleColor.Gray);
                }
                else if (gameOverState == GameOverState.YouLose)
                {
                    Program.Write("CPU WIN!", Position.X, Position.Y, ConsoleColor.Black, ConsoleColor.Gray);
                }
                else
                {
                    Program.Write("CPU", Position.X, Position.Y, ConsoleColor.Black, ConsoleColor.Gray);
                }

                Program.Write($"Points: {Points:D2}", Position.X, Position.Y - 2);

                if (Points != _lastPoints)
                {
                    int diff = Points - _lastPoints;

                    ThreadPool.QueueUserWorkItem((_) =>
                    {
                        Program.Write($"+{diff}", Position.X + 8, Position.Y - 3, ConsoleColor.Black, ConsoleColor.Gray);
                        Thread.Sleep(500);
                        Program.Write($"+{diff}", Position.X + 8, Position.Y - 3, ConsoleColor.Black, ConsoleColor.DarkGray);
                        Thread.Sleep(500);
                        Program.Write("   ", Position.X + 8, Position.Y - 3, ConsoleColor.Black);
                    });

                    _lastPoints = Points;
                }
            }
        }
    }
}
