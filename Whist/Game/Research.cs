using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Media;
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
    public partial class MainWindow : Window
    {
        #region Research related
        //najlepiej wziac ladowanie pliku z innego projektu - SE_MedOddech

        /* Metody dodawania list do siebie:
         * List<Card> gh = new List<Card>(Player1UsedCards.CardsPlayed);
         * gh.AddRange(Player2UsedCards.CardsPlayed);
         * gh.Concat(Player3UsedCards.CardsPlayed);
         */

        /// <summary> Robi liste kart na podstawie zagranych kart graczy. Trzeba uzyc przed EndGame()
        /// zeby wyciagnac rece wszystkich graczy a nastepnie rozdac je przy pomocy 
        /// GivePreppedHands. Dzieki temu mozemy badac to samo rozdanie dla roznych graczy i patrzec
        /// na skutecznosc kazdego z nich.
        /// </summary>
        /// <returns>Lista kart.</returns>
        public List<Card> MakeListCardFromHands()
        {
            List<Card> cardsPlayed = new List<Card>();

            for (int i = 0; i < PlayersUsedCards.Count; i++)
            {
                cardsPlayed.AddRange(PlayersUsedCards[i].CardsPlayed);
            }
            return cardsPlayed;
        }

        /// <summary>
        /// Rozdaje przygotowana reke. Karty rozdaje w takiej kolejnosci jak sa na liscie.
        /// </summary>
        /// <param name="preppedCards">Lista przygotowanych kart. Jej calkowita liczba 
        /// musi byc podzielna przez 4.</param>
        /// <param name="whichPlayer">Od ktorego gracza zaczynamy rozdawac karty. 
        /// Gracze numerowani sa 1-4. Jest obsluga bledu przy przekroczeniu parametru.</param>
        public void GivePreppedHands(List<Card> preppedCards, int whichPlayer, bool @fromFile = false)
        {
            Card readCard;
            int inLength = preppedCards.Count;

            if(fromFile)
                TrumpCard = preppedCards.Last();

            for (int i = 0; i < inLength; i++)
            {
                //czytamy od konca, tak zeby 1sza karta z listy byla na szczycie
                readCard = preppedCards[inLength - 1 - i];
                if (_dealer.Has(readCard.Rank, readCard.Suit))
                {
                    _dealer.GetCard(readCard.Rank, readCard.Suit).MoveToLast();
                }
            }

            int index = 0;
            //musza byc karty dla wszystkich 4 graczy, inaczej bedzie tutaj zwracac bledy
            for (int i = 0; i < 4; i++)
            {
                index = (i + ExtraFunctions.InRange(whichPlayer, 1, 4) - 1) % 4;
                _dealer.Draw(PlayersHandDecks[index].Deck, inLength / 4);
            }

        }

        /// <summary>
        /// Rozdaje przygotowana reke. Karty rozdaje w takiej kolejnosci jak sa na liscie.
        /// </summary>
        /// <param name="preppedCards">Lista list przygotowanych kart. Jej calkowita liczba 
        /// musi byc wynosic 4.</param>
        /// <param name="whichPlayer">Od ktorego gracza zaczynamy rozdawac karty. 
        /// Gracze numerowani sa 1-4. Jest obsluga bledu przy przekroczeniu parametru.</param>
        public void GivePreppedHands(List<List<Card>> preppedCards, int whichPlayer)
        {
            Card readCard;
            int inLength = preppedCards.Count;

            for (int i = 0; i < inLength; i++)
            {
                //chcemy jechac od konca
                for (int j = preppedCards[i].Count - 1; j <= 0; j++)
                {
                    //czytamy od konca, tak zeby 1sza karta z listy byla na szczycie
                    readCard = preppedCards[inLength - 1 - i][j];
                    if (_dealer.Has(readCard.Rank, readCard.Suit))
                    {
                        _dealer.GetCard(readCard.Rank, readCard.Suit).MoveToLast();
                    }
                }
            }

            int index = 0;
            //musza byc karty dla wszystkich 4 graczy, inaczej bedzie tutaj zwracac bledy
            for (int i = 0; i < 4; i++)
            {
                index = (i + ExtraFunctions.InRange(whichPlayer, 1, 4) - 1) % 4;
                _dealer.Draw(PlayersHandDecks[index].Deck, preppedCards[i].Count);
            }
        }

        /// <summary>
        /// Komputer rozgrywa okreslona liczbe rak, bez jakiejkolwiek interakcji z graczem.
        /// Kazde rozdanie jest powtorzone 1krotnie z powodu rotacji rak. 1 raz bo gramy w parach.
        /// </summary>
        /// <param name="repetitions">Liczba unikalnych rozdan - nie powinna byc zbyt duza
        /// poniewaz kazde rozdanie jest rotowanie 1 raz.</param>
        private void PlayContinuouslyRepeatHands(int repetitions)
        {
            ExtraFunctions.BeginTime();
            for (int i = 0; i < Repetitions; i++)
            {
                DealNextHand();

                //gramy w parach wiec 2 powtorzenia
                for (int k = 0; k < 2; k++)
                {
                    //policz na poczatku gry
                    BeginGameCalculations();
                    WhoStarts();
                    for (int j = 0; j < CardsPerPlayer; j++)
                    {
                        PlayAutoTrick();
                        GameShape.MouseLeftButtonDown -= new MouseButtonEventHandler(GameShape_MouseLeftButtonDown);
                        CleanUpTrick();
                    }
                    //zamien rece -> do badan
                    List<Card> changeHands = MakeListCardFromHands();
                    bool rotation = (k == 0) ? false : true;//sprawdz czy zrobic rotacje
                    EndGame(rotation);                      //skoncz rozdanie
                    CollectCards();                         //przenies karty do dealera
                    _dealer.InitPower(TrumpColor);          //policz power kazdej karty znowu
                    GivePreppedHands(changeHands, 2);       //daj 2giemu graczowi karty 1go (itd.)
                    DealerNumber += 1;      

                    //posortuj
                    Player1Hand.Deck.Sort();
                    Player2Hand.Deck.Sort();
                    Player3Hand.Deck.Sort();
                    Player4Hand.Deck.Sort();
                }
            }
            //jak skonczyles liczyc/grac to pozbieraj karty
            CollectCards();
            ExtraFunctions.EndTime();
            SystemSounds.Asterisk.Play();   //dzwiek zakonczenia
            scoreWinRef.resultBox.Items.Add("Czas wykonania: " + ExtraFunctions.GlobalWatch.Elapsed.TotalSeconds.ToString() + " s");
        }

        /// <summary>
        /// Komputer rozgrywa okreslona liczbe rak, bez jakiejkolwiek interakcji z graczem.
        /// </summary>
        /// <param name="repetitions">Liczba rozdan ktore AI ma ze soba zagrac.</param>
        private void PlayContinuously(int repetitions)
        {
            ExtraFunctions.BeginTime();
            for (int i = 0; i < repetitions; i++)
            {
                DealNextHand();
                WhoStarts();
                //policz na poczatku gry
                BeginGameCalculations();
                for (int j = 0; j < CardsPerPlayer; j++)
                {
                    PlayAutoTrick();
                    GameShape.MouseLeftButtonDown -= new MouseButtonEventHandler(GameShape_MouseLeftButtonDown);
                    CleanUpTrick();
                }
                //zamien rece do badan -> zrealizowane w PlayContinuouslyRepeatHands
                EndGame();
            }
            ExtraFunctions.EndTime();
            SystemSounds.Asterisk.Play();
            scoreWinRef.resultBox.Items.Add("Czas wykonania: " + ExtraFunctions.GlobalWatch.Elapsed.TotalSeconds.ToString() + " s");
        }

        //ten przycisk jest tylko do testu
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            List<Card> prepHandTest = new List<Card>();
            Deck prepDeckTest = new Deck(_dealer.Game);
            WistPlayerNumber = 1;

            //tutaj prepHandTest = LoadFromFile(..)
            //karty beda rozdane w takiej kolejnosci w jakiej zostaly dodane
            prepHandTest.Add(new Card(CardRank.Eight, CardSuit.Diamonds, prepDeckTest));
            prepHandTest.Add(new Card(CardRank.King, CardSuit.Hearts, prepDeckTest));
            prepHandTest.Add(new Card(CardRank.Four, CardSuit.Spades, prepDeckTest));
            prepHandTest.Add(new Card(CardRank.Seven, CardSuit.Clubs, prepDeckTest));

            GivePreppedHands(prepHandTest, 1);

            //Wlacz widzenie kart
            Player4Hand.Deck.AllCardsVisible();

            //odwroc karty wszystkich graczy
            Player1Hand.Deck.AllCardsVisible(); Player1Hand.Deck.Sort();
            Player2Hand.Deck.AllCardsVisible(); Player2Hand.Deck.Sort();
            Player3Hand.Deck.AllCardsVisible(); Player3Hand.Deck.Sort();

            //zacznij grac tak jak normalnie
            //moze napisac po prostu funkcje BeginPlay() zamiast tych 4 linijek
            if (humanPlayer == true)
                BeginTrick();
            else
                PlayAutoTrick();
        }

        public void PlayPreppedHand(List<Card> preppedHand)
        {
            GivePreppedHands(preppedHand, 1, true);

            this.trumpCardBlock.DataContext = "(" + ((DealerNumber - 1) % 4 + 1).ToString() + ")" + " "
                                                + TrumpCard.ToString();

            //Wlacz widzenie kart
            Player4Hand.Deck.AllCardsVisible();
            Player4Hand.Deck.Sort();

            //odwroc karty wszystkich graczy
            Player1Hand.Deck.AllCardsVisible(); Player1Hand.Deck.Sort();
            Player2Hand.Deck.AllCardsVisible(); Player2Hand.Deck.Sort();
            Player3Hand.Deck.AllCardsVisible(); Player3Hand.Deck.Sort();

            //sprawdz kto zaczyna
            WhoStarts();
            //policz na poczatku gry
            BeginGameCalculations();

            //zacznij grac tak jak normalnie
            //moze napisac po prostu funkcje BeginPlay() zamiast tych 4 linijek
            if (humanPlayer == true)
                BeginTrick();
            else
                PlayAutoTrick();
        }

        #endregion
    }

    class Research
    {
    }
}
