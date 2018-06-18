using System;
using System.Collections.Generic;
using System.Linq;
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
using Whist.Core.Controls;
using Whist.Core.Model;
using System.Threading;
using System.Threading.Tasks;

namespace Whist
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Fields
        private const int CardsPerPlayer = 13;
        private const int NumberOfPlayers = 4;

        private Deck _dealer;

        List<DeckShape> PlayersHandDecks;
        List<DeckShape> PlayersTricks;

        //ustawienia inteligencji poszczegolnych graczy
        private bool humanPlayer;
        public bool HumanPlayer
        {
            get { return humanPlayer; }
            set { humanPlayer = value; }
        }

        private bool autoPlay = false;
        public bool AutoPlay
        {
            get { return autoPlay; }
            set { autoPlay = value; }
        }

        //do badan
        private int repetitions = 1;
        public int Repetitions
        {
            get { return repetitions; }
            set { repetitions = value; }
        }

        private bool repeat = false;
        public bool Repeat
        {
            get { return repeat; }
            set { repeat = value; }
        }

        //domyslnie - losowa karta dla kazdego
        private List<AlgorithmTypes> playersAlgorithms = new List<AlgorithmTypes> { AlgorithmTypes.Random, AlgorithmTypes.Random, AlgorithmTypes.Random, AlgorithmTypes.Random };
        public List<AlgorithmTypes> PlayersAlgorithms
        {
            get { return playersAlgorithms; }
            set { playersAlgorithms = value; }
        }

        //co partie inkrementowany jest numer dealera
        private int dealerNumber = 3;
        public int DealerNumber
        {
            get { return dealerNumber; }
            set { dealerNumber = value; }
        }

        //do wistu
        private int wistPlayerNumber; //od 1 do 4
        /// <summary>
        /// Zwraca numer wistujacego gracza. Wartosci od 1 do 4.
        /// </summary>
        public int WistPlayerNumber
        {
            get { return wistPlayerNumber; }
            set { wistPlayerNumber = value; this.textWist.DataContext = wistPlayerNumber; }
        }
        /// <summary>
        /// Zwraca indeks gracza w zaleznosci od jego pozycji w lewie.
        /// Mozliwe wartosci to od 0 do 3.</summary>
        /// <param name="trickPosition">Pozycja w lewie.</param>
        /// <returns>Indeks gracza.</returns>
        public int IndexOfPlayer(int trickPosition)
        {
            int index = (WistPlayerNumber - 1 + trickPosition) % 4;
            return index;
        }

        private Card wistCard; //chyba niekoniecznie musza ja miec
        public Card WistCard
        {
            get { return wistCard; }
            set { wistCard = value; }
        }

        //zrzucone karty pozostalych graczy
        Hand Player1UsedCards;
        Hand Player2UsedCards;
        Hand Player3UsedCards;
        Hand Player4UsedCards;
        List<Hand> PlayersUsedCards;

        //obecna lewa
        List<Card> currentTrick;

        //lista wistujacych
        List<int> wistList = new List<int>();

        //do obslugi, niekoniecznie uzywane
        private SemaphoreSlim signal;
        public System.Threading.ManualResetEvent manualEvent;
        private AutoResetEvent _messageRecieved;

        //referencja do okna z wynikami
        private ScoreAndInfo scoreWinRef;
        public ScoreAndInfo ScoreWinRef
        {
            get { return scoreWinRef; }
            set { scoreWinRef = value; }
        }

        //kolor atutu
        private CardSuit trumpColor = CardSuit.Clubs;
        public CardSuit TrumpColor
        {
            get { return trumpColor; }
            set { trumpColor = value; }
        }

        //karta atutu ktora pokazana na poczatku rozgrywki
        private Card trumpCard;
        public Card TrumpCard
        {
            get { return trumpCard; }
            //jak nadajesz karte to od razu nadajesz atut
            set { trumpCard = value; trumpColor = TrumpCard.Suit; }
        }

        #endregion

        #region Initialize
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }
        #endregion

        #region Methods

        /// <summary>
        /// Inicjalizacja gry.
        /// </summary>
        private void NewGame()
        {
            // create new game instance for our main window gameshape control
            GameShape.Game = new Game();
            
            signal = new SemaphoreSlim(0, 1);
            _messageRecieved = new AutoResetEvent(false);
            //scorePerRound = new List<List<int>>();
            currentTrick = new List<Card>();
            humanPlayer = true;
            repetitions = 1;

            //this.Width = SystemParameters.PrimaryScreenWidth;
            //this.Height = Screen.PrimaryScreen.WorkingArea.Height / 2;
            //this.Top = (Screen.PrimaryScreen.WorkingArea.Top + Screen.PrimaryScreen.WorkingArea.Height) / 4;
            //this.Left = (Screen.PrimaryScreen.WorkingArea.Left + Screen.PrimaryScreen.WorkingArea.Width) / 4;
            // call some setup methods to initialize the game components
            SetupDealerDeck();
            SetupPlayerHandDecks();
            SetupTrickDecks();
            SetupPlayerUsedCardDecks();
        }

        /// <summary>
        /// Deals the next hand.
        /// </summary>
        private void DealNextHand()
        {

            // collection cards if any are out
            if (_dealer.Cards.Count < 52)
            {
                CollectCards();
            }
            _dealer.Shuffle(5);


            //dealer chodzi o 1 pozycje zgodnie ze wskazowkami zegara
            DealerNumber += 1;
            WistPlayerNumber = 1;   //wyzeruj zeby policzyl kto jest dealerem
            int index = IndexOfPlayer(DealerNumber);
            WistPlayerNumber = index + 1;   //ustaw wistujacego
            TrumpCard = _dealer.BottomCard; //ostatnia karta jest atutem
            this.trumpCardBlock.DataContext = "(" + ((DealerNumber-1) % 4 + 1).ToString() + ")" + " "
                                                + TrumpCard.ToString();

            //oblicz moce karty (bo zmienil sie atut)
            _dealer.InitPower(TrumpColor);

            //rozdaj CardPerPlayer kart kazdemu graczowi
            for (var cardCount = 0; cardCount < CardsPerPlayer; cardCount++)
            {
                _dealer.Draw(PlayersHandDecks[index].Deck, 1);
                _dealer.Draw(PlayersHandDecks[index].Next.Deck, 1);
                _dealer.Draw(PlayersHandDecks[index].Next.Next.Deck, 1);
                _dealer.Draw(PlayersHandDecks[index].Next.Next.Next.Deck, 1);
            }

            // turn over human player [4] hand
            Player4Hand.Deck.Sort();
            Player4Hand.Deck.MakeAllCardsDragable(true);
            Player4Hand.Deck.AllCardsVisible();

            //odwroc karty wszystkich graczy
            Player1Hand.Deck.AllCardsNotVisible(); Player1Hand.Deck.Sort();
            Player2Hand.Deck.AllCardsNotVisible(); Player2Hand.Deck.Sort();
            Player3Hand.Deck.AllCardsNotVisible(); Player3Hand.Deck.Sort();
        }

        public void BeginGameCalculations()
        {
            //policz punkty na rece kazdego gracza systemem brydzowym (4 as do 1 walet)
            Player1Hand.Deck.CountPoints(); 
            Player2Hand.Deck.CountPoints(); 
            Player3Hand.Deck.CountPoints(); 
            Player4Hand.Deck.CountPoints();

            //przelicz power level kart 
            Player1Hand.Deck.RecalculatePower();
            Player2Hand.Deck.RecalculatePower();
            Player3Hand.Deck.RecalculatePower();
            Player4Hand.Deck.RecalculatePower();

            //policz poczatkowe dlugosci kolorow kazdej reki
            Player1UsedCards.startingColorsLength = Player1Hand.Deck.CountColors();
            Player2UsedCards.startingColorsLength = Player2Hand.Deck.CountColors();
            Player3UsedCards.startingColorsLength = Player3Hand.Deck.CountColors();
            Player4UsedCards.startingColorsLength = Player4Hand.Deck.CountColors();

            //ustaw wyniki wzietych lew na 0
            this.tricksNS.DataContext = 0;
            this.tricksWE.DataContext = 0;
        }

        /// <summary>
        /// Collects the cards.
        /// </summary>
        public void CollectCards()
        {
            Player1Hand.Deck.Draw(_dealer, Player1Hand.Deck.Cards.Count);
            Player2Hand.Deck.Draw(_dealer, Player2Hand.Deck.Cards.Count);
            Player3Hand.Deck.Draw(_dealer, Player3Hand.Deck.Cards.Count);
            Player4Hand.Deck.FlipAllCards();    // flip user cards face down
            Player4Hand.Deck.Draw(_dealer, Player4Hand.Deck.Cards.Count);
            Player1Trick.Deck.FlipAllCards();
            Player1Trick.Deck.Draw(_dealer, Player1Trick.Deck.Cards.Count);
            Player2Trick.Deck.FlipAllCards();
            Player2Trick.Deck.Draw(_dealer, Player2Trick.Deck.Cards.Count);
            Player3Trick.Deck.FlipAllCards();
            Player3Trick.Deck.Draw(_dealer, Player3Trick.Deck.Cards.Count);
            Player4Trick.Deck.FlipAllCards();
            Player4Trick.Deck.Draw(_dealer, Player4Trick.Deck.Cards.Count);

            //ze stosu zuzytych kart przenies do dealera
            PlayedCards.Deck.AllCardsNotVisible();
            PlayedCards.Deck.Draw(_dealer, PlayedCards.Deck.Cards.Count);

            _dealer.AllCardsNotVisible();

            //posprzataj pamiec
            //CleanMemory();
        }

        /*
         *  Setup Methods
         *      initialize game/deck/hand objects etc.
         */
        private void SetupDealerDeck()
        {
            _dealer = new Deck(1, 13, GameShape.Game);

            _dealer.Shuffle(5);
            _dealer.MakeAllCardsDragable(false);
            _dealer.Enabled = true;
            _dealer.FlipAllCards();

            Dealer.Deck = _dealer;
            GameShape.DeckShapes.Add(Dealer);
            Dealer.DeckMouseLeftButtonDown += new MouseButtonEventHandler(Dealer_DeckMouseLeftButtonDown);
        }

        private void SetupTrickDecks()
        {
            Player1Trick.Deck = new Deck(GameShape.Game) { Enabled = true };
            Player2Trick.Deck = new Deck(GameShape.Game) { Enabled = true };
            Player3Trick.Deck = new Deck(GameShape.Game) { Enabled = true };
            Player4Trick.Deck = new Deck(GameShape.Game) { Enabled = true };

            Player1Trick.Deck.MakeAllCardsDragable(false);
            Player2Trick.Deck.MakeAllCardsDragable(false);
            Player3Trick.Deck.MakeAllCardsDragable(false);
            Player4Trick.Deck.MakeAllCardsDragable(false);

            //nie moge tego ustawic na true bo cos nie dziala?
            //Player1Trick.Deck.TopCard.Visible = true;
            //Player1Trick.Deck.TopCard.Visible = true;
            //Player1Trick.Deck.TopCard.Visible = true;
            //Player1Trick.Deck.TopCard.Visible = true;

            //ustaw partnerow
            Player1Trick.Deck.Partner = Player3Hand.Deck;
            Player2Trick.Deck.Partner = Player4Hand.Deck;
            Player3Trick.Deck.Partner = Player1Hand.Deck;
            Player4Trick.Deck.Partner = Player2Hand.Deck;

            GameShape.DeckShapes.Add(Player1Trick);
            GameShape.DeckShapes.Add(Player2Trick);
            GameShape.DeckShapes.Add(Player3Trick);
            GameShape.DeckShapes.Add(Player4Trick);

            PlayersTricks = new List<DeckShape>();
            PlayersTricks.Add(Player1Trick);
            PlayersTricks.Add(Player2Trick);
            PlayersTricks.Add(Player3Trick);
            PlayersTricks.Add(Player4Trick);
        }

        private void SetupPlayerHandDecks()
        {
            Player1Hand.Deck = new Deck(GameShape.Game) { Enabled = true };
            Player2Hand.Deck = new Deck(GameShape.Game) { Enabled = true };
            Player3Hand.Deck = new Deck(GameShape.Game) { Enabled = true };
            Player4Hand.Deck = new Deck(GameShape.Game) { Enabled = true };
            PlayedCards.Deck = new Deck(GameShape.Game) { Enabled = true };

            //ustawia nastepnikow kazdej reki oraz miejsce na ktora ma klasc karty
            Player1Hand.Deck.MakeAllCardsDragable(true); Player1Hand.Next = Player2Hand; Player1Hand.Trick = Player1Trick;
            Player2Hand.Deck.MakeAllCardsDragable(true); Player2Hand.Next = Player3Hand; Player2Hand.Trick = Player2Trick;
            Player3Hand.Deck.MakeAllCardsDragable(true); Player3Hand.Next = Player4Hand; Player3Hand.Trick = Player3Trick;
            Player4Hand.Deck.MakeAllCardsDragable(true); Player4Hand.Next = Player1Hand; Player4Hand.Trick = Player4Trick;
            PlayedCards.Deck.MakeAllCardsDragable(false);

            //ustawia partnerow kazdej reki
            Player1Hand.Partner = Player3Hand; Player1Hand.Deck.Partner = Player3Hand.Deck;
            Player2Hand.Partner = Player4Hand; Player2Hand.Deck.Partner = Player4Hand.Deck;
            Player3Hand.Partner = Player1Hand; Player3Hand.Deck.Partner = Player1Hand.Deck;
            Player4Hand.Partner = Player2Hand; Player4Hand.Deck.Partner = Player2Hand.Deck;
            
            GameShape.DeckShapes.Add(Player1Hand);
            GameShape.DeckShapes.Add(Player2Hand);
            GameShape.DeckShapes.Add(Player3Hand);
            GameShape.DeckShapes.Add(Player4Hand);
            GameShape.DeckShapes.Add(PlayedCards);

            PlayersHandDecks = new List<DeckShape>();
            PlayersHandDecks.Add(Player1Hand);
            PlayersHandDecks.Add(Player2Hand);
            PlayersHandDecks.Add(Player3Hand);
            PlayersHandDecks.Add(Player4Hand);
        }

        private void SetupPlayerUsedCardDecks()
        {
            Player1UsedCards = new Hand();
            Player2UsedCards = new Hand();
            Player3UsedCards = new Hand();
            Player4UsedCards = new Hand();

            PlayersUsedCards = new List<Hand>();
            PlayersUsedCards.Add(Player1UsedCards);
            PlayersUsedCards.Add(Player2UsedCards);
            PlayersUsedCards.Add(Player3UsedCards);
            PlayersUsedCards.Add(Player4UsedCards);
        }

        private void UpdateAllCardShapes()
        {
            GameShape.GetGameShape(Player1Hand.Deck.Game).GetDeckShape(Player1Hand.Deck).UpdateCardShapes();
            GameShape.GetGameShape(Player2Hand.Deck.Game).GetDeckShape(Player2Hand.Deck).UpdateCardShapes();
            GameShape.GetGameShape(Player3Hand.Deck.Game).GetDeckShape(Player3Hand.Deck).UpdateCardShapes();
            GameShape.GetGameShape(Player4Hand.Deck.Game).GetDeckShape(Player4Hand.Deck).UpdateCardShapes();
        }

        /// <summary>
        /// Wyczysc wszystkie informacje dot. graczy ktore nagromadziles
        /// </summary>
        public void CleanMemory()
        {
            //wyczysc
            Player1UsedCards.ClearHand(); Player1UsedCards.DeleteRenons();
            Player2UsedCards.ClearHand(); Player2UsedCards.DeleteRenons();
            Player3UsedCards.ClearHand(); Player3UsedCards.DeleteRenons();
            Player4UsedCards.ClearHand(); Player4UsedCards.DeleteRenons();

            //wyczysz karty i decki ktore potworzyly sie w czasie gry
            GameShape.Game.RemoveCards(52);
            GameShape.Game.RemoveDecks(10);
            wistList.Clear();

            currentTrick.Clear();
        }

        private void CleanUpTrick()
        {
            //dodaj numer wistujacego juz w zakresie 0-3
            wistList.Add(WistPlayerNumber - 1);

            //sprawdz kto wygral lewe i ustaw nowego wistujacego
            WistPlayerNumber = GameRules.TrickWinner(Player1Trick.Deck.TopCard, Player2Trick.Deck.TopCard, Player3Trick.Deck.TopCard, Player4Trick.Deck.TopCard, TrumpColor, WistPlayerNumber);
            WistCard = null;

            //w tym miejscu wygrany jest juz wyliczony
            Hand trickVictoryPlayer = PlayersUsedCards[WistPlayerNumber - 1];
            //Hand trickVictoryPlayer = DetermineWhichPlayerUsedCards(WistPlayerNumber);

            //dodaj wygrane karty do reki gracza
            trickVictoryPlayer.AddInHand(new Card(Player1Trick.Deck.TopCard));
            trickVictoryPlayer.AddInHand(new Card(Player2Trick.Deck.TopCard));
            trickVictoryPlayer.AddInHand(new Card(Player3Trick.Deck.TopCard));
            trickVictoryPlayer.AddInHand(new Card(Player4Trick.Deck.TopCard));

            //przesun karty ze srodka do talii zuzytych kart
            Player1Trick.Deck.Draw(PlayedCards.Deck, 1);
            Player2Trick.Deck.Draw(PlayedCards.Deck, 1);
            Player3Trick.Deck.Draw(PlayedCards.Deck, 1);
            Player4Trick.Deck.Draw(PlayedCards.Deck, 1);
            PlayedCards.Deck.AllCardsNotVisible();
            PlayedCards.Deck.Sort();

            this.tricksNS.DataContext = Player2UsedCards.CardsInHand.Count/4 + Player4UsedCards.CardsInHand.Count/4;
            this.tricksWE.DataContext = Player1UsedCards.CardsInHand.Count/4 + Player3UsedCards.CardsInHand.Count/4; 

            currentTrick.Clear();
        }

        public void UpdatePower(Card compareCard, CardSuit trumpColor)
        {
            Player1Hand.Deck.ChangePower(compareCard, trumpColor);
            Player2Hand.Deck.ChangePower(compareCard, trumpColor);
            Player3Hand.Deck.ChangePower(compareCard, trumpColor);
            Player4Hand.Deck.ChangePower(compareCard, trumpColor);
        }

        //zagrywa karte z wybranej reki
        //funkcja do animacji, nie robi nic poza tym
        private void PlayCard(Deck playerHand, Deck playerTrick, Card cardToPlay)
        {
            var card = cardToPlay;
            var gameShape = GameShape.GetGameShape(card.Deck.Game);
            var cardShape = gameShape.GetCardShape(card);
            var oldDeckShape = gameShape.GetDeckShape(card.Deck);

            card.Deck = playerTrick;
            //DO OBLICZENIA MOCY KART
            UpdatePower(card, TrumpColor);
            // playerTrick.Visibility = System.Windows.Visibility.Visible;
            playerTrick.TopCard.Visible = true;

            gameShape.GetDeckShape(card.Deck).UpdateCardShapes();
        }

        /// <summary>
        /// Zagrywa karte wyliczona przez algorytm.
        /// </summary>
        /// <param name="playerPossibleMoves">Wszystkie mozliwe ruchy gracza.</param>
        /// <param name="playerTrick">Kupka na ktora gracz zagrywa karte.</param>
        /// <param name="usedCards">Reka w ktorej przechowywane sa zagrane karty gracza.</param>
        /// <returns>Zagrana karta.</returns>
        private Card PlayCardChosenAlgorithm(Deck playerPossibleMoves, DeckShape playerTrick, Hand usedCards, AlgorithmTypes chosenAlg)
        {
            //tutaj jest wybor algorytmu - akurat teraz jest losowa karta z gory
            //var card = playerPossibleMoves.RandomCard;
            var card = ChooseAlgorithm(playerPossibleMoves, currentTrick.Count, currentTrick, chosenAlg);

            //zagraj karte zwrocona przez algorytm
            Card playedCard = new Card(card.Rank, card.Suit, playerPossibleMoves);
            PlayCard(playerPossibleMoves, playerTrick.Deck, card);

            //wylaczyc zeby dzialalo jeszcze szybciej
            //ExtraFunctions.WaitTime(10);

            usedCards.AddPlayed(playedCard);
            usedCards.SortLow();
            return playedCard;
        }

        //graj sam
        //NIESKONCZONA - chyba juz skonczona
        private void PlayAutoTrick(int @howManyTricks = 13)
        {
            GameShape.CardMouseLeftButtonDown -= new MouseButtonEventHandler(GameShape_CardMouseLeftButtonDown);
            bool renons;
            int i;

            if (PlayersHandDecks[WistPlayerNumber - 1].Deck.Cards.Count == 0)
            {
                //zakoncz gre
                EndGame();
                return;
            }

            for (i = 0; i < 4; i++)
            {
                int index = (WistPlayerNumber - 1 + i) % 4;

                Deck playerXMoves = new Deck(PlayersHandDecks[index].Next.Deck);
                //pierwszy gracz jest oczywiscie wistujacym
                if (i == 0)
                {
                    playerXMoves = GameRules.PossibleMoves(PlayersHandDecks[index].Deck, null, out renons);
                    WistCard = new Card(PlayCardChosenAlgorithm(playerXMoves, PlayersTricks[index], PlayersUsedCards[index], PlayersAlgorithms[index]));
                }
                //kolejni gracze musze dostosowac ruchy do wistujacego
                else
                {
                    playerXMoves = GameRules.PossibleMoves(PlayersHandDecks[index].Deck, WistCard, out renons);
                    PlayCardChosenAlgorithm(playerXMoves, PlayersTricks[index], PlayersUsedCards[index], PlayersAlgorithms[index]);
                    //sprawdz czy dolozyl do koloru, jezeli nie to ustaw krotkosc
                    if (renons)
                    {
                        PlayersUsedCards[index].CardRenons[WistCard.Suit] = true;
                    }
                }

                Canvas.SetZIndex(PlayersTricks[index], i);
                PlayersHandDecks[index].Deck.Sort();
            }
            GameShape.MouseLeftButtonDown += new MouseButtonEventHandler(GameShape_MouseLeftButtonDown);
        }

        #endregion

        #region Event Handlers

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            NewGame();

            GameShape.CardMouseLeftButtonDown += new MouseButtonEventHandler(GameShape_CardMouseLeftButtonDown);
            //GameShape.CardDrag += new CardDragEventHandler(GameShape_CardDrag);
        }

        private void GameShape_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                GameShape.MouseLeftButtonDown -= new MouseButtonEventHandler(GameShape_MouseLeftButtonDown);
                GameShape.KeyUp -= new KeyEventHandler(GameShape_KeyUp);
                //w tym miejscu wygrany jest juz wyliczony
                CleanUpTrick();
                e.Handled = true;

                if (humanPlayer)
                    BeginTrick();
                else
                    /*BeginTrick(); //*/
                    PlayAutoTrick();
            }
        }

        //z jakiegos powodu nie dziala :<
        private void GameShape_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F)
            {
                GameShape.MouseLeftButtonDown -= new MouseButtonEventHandler(GameShape_MouseLeftButtonDown);
                GameShape.KeyUp -= new KeyEventHandler(GameShape_KeyUp);
                CleanUpTrick();
                BeginTrick();
            }
        }

        private void Dealer_DeckMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CollectCards();
        }

        private void GameShape_CardDrag(CardShape cardShape, DeckShape oldDeckShape, DeckShape newDeckShape)
        {
            // check for lead suit renege

            if (((newDeckShape.Deck.TopCard == null) && (cardShape.Card.Number == 1)) ||
                ((newDeckShape.Deck.TopCard != null) && (cardShape.Card.Suit == newDeckShape.Deck.TopCard.Suit) && (cardShape.Card.Number - 1 == newDeckShape.Deck.TopCard.Number)))
            {
                //Move card to stack
                cardShape.Card.Deck = newDeckShape.Deck;

                //Flip the first remaining card in the old deck
                if (oldDeckShape.Deck.TopCard != null)
                {
                    oldDeckShape.Deck.TopCard.Visible = true;
                    oldDeckShape.Deck.TopCard.Enabled = true;
                    oldDeckShape.Deck.TopCard.IsDragable = true;
                }
            }
        }

        /// <summary>
        /// Poczatek lewy, wykonuje sie przed wykonaniem ruchu przez gracza
        /// </summary>
        private void BeginTrick()
        {
            GameShape.CardMouseLeftButtonDown -= new MouseButtonEventHandler(GameShape_CardMouseLeftButtonDown);
            bool renons;
            int i;

            if (DetermineWhichPlayerHand(WistPlayerNumber).Deck.Cards.Count == 0)
            {
                //zakoncz gre
                EndGame();
                return;
            }

            if (WistPlayerNumber == 4)
            {
                GameShape.CardMouseLeftButtonDown += new MouseButtonEventHandler(GameShape_CardMouseLeftButtonDown);
                return;           //jezeli gracz jest wistujacym to skoncz
            }

            //zaczyna od numeru gracza wistujacego - gdy wistujacym jest gracz nie wejdzie ani razu
            for (i = WistPlayerNumber; i < 4; i++)
            {
                Deck playerXMoves;// = new Deck(PlayersHandDecks[i-1].Deck);
                //pierwszy gracz jest wistujacym
                if (i == WistPlayerNumber)
                {
                    playerXMoves = GameRules.PossibleMoves(PlayersHandDecks[i - 1].Deck, null, out renons);
                    WistCard = new Card(PlayCardChosenAlgorithm(playerXMoves, PlayersTricks[i - 1], PlayersUsedCards[i - 1], PlayersAlgorithms[i - 1]));
                }
                //kolejni gracze musze dostosowac ruchy do wistujacego
                else
                {
                    playerXMoves = GameRules.PossibleMoves(PlayersHandDecks[i - 1].Deck, WistCard, out renons);
                    PlayCardChosenAlgorithm(playerXMoves, PlayersTricks[i - 1], PlayersUsedCards[i - 1], PlayersAlgorithms[i - 1]);
                    //sprawdz czy dolozyl do koloru, jezeli nie to ustaw krotkosc
                    if (renons)
                    {
                        PlayersUsedCards[i - 1].CardRenons[WistCard.Suit] = true;
                    }
                }

                Canvas.SetZIndex(PlayersTricks[i - 1], i);
                PlayersHandDecks[i - 1].Deck.Sort();
            }

            GameShape.CardMouseLeftButtonDown += new MouseButtonEventHandler(GameShape_CardMouseLeftButtonDown);
            return;
        }

        /// <summary>
        /// Koncowka lewy, wykonuje sie po wykonaniu ruchu przez gracza
        /// </summary>
        private void EndTrick()
        {
            GameShape.CardMouseLeftButtonDown -= new MouseButtonEventHandler(GameShape_CardMouseLeftButtonDown);
            Canvas.SetZIndex(Player4Trick, 4);
            bool renons;

            for (int i = 1; i < WistPlayerNumber; i++)
            {
                Deck playerXMoves = new Deck(PlayersHandDecks[i - 1].Deck);
                playerXMoves = GameRules.PossibleMoves(PlayersHandDecks[i - 1].Deck, WistCard, out renons);
                PlayCardChosenAlgorithm(playerXMoves, PlayersTricks[i - 1], PlayersUsedCards[i - 1], PlayersAlgorithms[i - 1]);
                PlayersHandDecks[i - 1].Deck.Sort();
                //sprawdz czy dolozyl do koloru, jezeli nie to ustaw krotkosc
                if (renons)
                {
                    PlayersUsedCards[i - 1].CardRenons[WistCard.Suit] = true;
                }

                Canvas.SetZIndex(PlayersTricks[i - 1], 4 + i);
            }

            GameShape.MouseLeftButtonDown += new MouseButtonEventHandler(GameShape_MouseLeftButtonDown);
            GameShape.KeyUp += new KeyEventHandler(GameShape_KeyUp);
            //BeginTrick();
        }

        private void GameShape_CardMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //zablokuj eventy
            //GameShape.CardDrag -= new CardDragEventHandler(GameShape_CardDrag);
            GameShape.CardMouseLeftButtonDown -= new MouseButtonEventHandler(GameShape_CardMouseLeftButtonDown);

            bool renons;
            //klikniecie na karte sprobuje zagrac karte
            var cardShapeClicked = (CardShape)sender;
            CardShape card = (CardShape)sender;
            var gameShape = GameShape.GetGameShape(card.Card.Deck.Game);
            var oldDeckShape = gameShape.GetDeckShape(card.Card.Deck);

            if (oldDeckShape.Name == "Player4Hand")
            {
                //wygeneruj mozliwe ruchy gracza
                Deck player4Moves = new Deck(Player4Hand.Deck);
                player4Moves = GameRules.PossibleMoves(Player4Hand.Deck, WistCard, out renons);
                Player4Hand.Deck.Sort();

                //karta ktora chce zagrac gracz
                Card userPlayedCard = new Card(card.Card.Rank, card.Card.Suit, player4Moves);

                //sprawdz czy karta ktora chce zagrac gracz jest w jego mozliwych ruchac
                if (player4Moves.Has(userPlayedCard.Rank, userPlayedCard.Suit))
                {
                    Player4UsedCards.AddPlayed(userPlayedCard);
                    Player4UsedCards.SortHigh();
                    //jezeli graz jest wistujacym to jego ruch staje sie karta wistujaca
                    if (WistPlayerNumber == 4)
                        WistCard = userPlayedCard;
                    //jezeli nie jest to sprawdz czy nie ma krotkosci
                    else if (renons)
                    {
                        Player4UsedCards.CardRenons[WistCard.Suit] = true;
                    }

                    PlayCard(Player4Hand.Deck, Player4Trick.Deck, card.Card);

                    //card.Card.Deck = Player4Trick.Deck;
                    currentTrick.Add(userPlayedCard);
                    EndTrick();
                    return;
                }

            }
            //Nie zagral prawidlowej karty - odblokuj klikanie
            GameShape.CardMouseLeftButtonDown += GameShape_CardMouseLeftButtonDown;
            //GameShape.CardMouseLeftButtonDown += new MouseButtonEventHandler(GameShape_CardMouseLeftButtonDown);

            //gameShape.GetDeckShape(card.Card.Deck).UpdateCardShapes();
            UpdateAllCardShapes();
            //Canvas.SetZIndex(oldDeckShape, 0);         
        }

        #endregion

        #region 3 human vs 1 AI
        //nie wiem jak zaimplementowac 3 graczy vs 1 komputer jakos sensownie, zastanowic sie nad tym jeszcze
        private void PlayTrick()
        {
            //if player1 == human -> +event 
        }

        #endregion

        //public poniewaz CHCEMY zeby bylo to dostepne z poziomu okienka obslugi
        public void MainWindow_DealButton_Click(object sender, RoutedEventArgs e)
        {
            //MainWindow_DealButton.Click -= MainWindow_DealButton_Click;
            GameShape.MouseLeftButtonDown -= GameShape_MouseLeftButtonDown;
            //jezeli gra nie zostala zakonczona to trzeba wyczyscic pamiec!
            CleanMemory();

            // prompt user to confirm deal new game
            var result = MessageBox.Show("Deal a new hand?", "Confirm New Deal", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.No)
            {
                MainWindow_DealButton.Click += MainWindow_DealButton_Click;
                return;
            }

            //rozdaj nowa reke
            DealNextHand();
            //sprawdz kto zaczyna
            WhoStarts();
            //policz na poczatku gry
            BeginGameCalculations();

            //graj samemu, zero interkacji z graczem. Przede wszystkim do badan
            if (AutoPlay)
            {
                if (Repeat)
                {
                    PlayContinuouslyRepeatHands(Repetitions);
                }
                else
                {
                    PlayContinuously(Repetitions);
                }
            }
            //Zywy gracz, ustawiony na pozycji Player 4
            else if (humanPlayer == true)
            {
                BeginTrick();
            }
            //Na pozycji Playera 4 ustawiony jest komputer, uzytkownik moze obserwowac gre
            // oraz musi akceptowac koniec kazdej lewy
            else if (humanPlayer == false)
            {
                //nie mozesz zakonczyc partii dopoki komputer nie przestanie grac
                PlayAutoTrick();
            }

            e.Handled = true;
        }
    }
}
