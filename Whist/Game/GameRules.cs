using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Whist;
using Whist.Core.Controls;
using Whist.Core.Model;
using System.Threading;

namespace Whist
{
    public partial class MainWindow : Window
    {
        private List<List<int>> scorePerRound = new List<List<int>>();
        public List<List<int>> ScorePerRound
        {
            get { return scorePerRound; }
            set 
            { 
                scorePerRound = value;
                //scoreWinRef.ShowResults(); 
            }
        }

        /// <summary>
        /// Zwraca DeckShape odpowiadajacy numerowi gracza wistujacego.
        /// </summary>
        /// <param name="wistPlayerNumber">Numer gracza wistujacego</param>
        /// <returns>DeckShape odpowiadajacy argumentowi wistPlayerNumber</returns>
        public DeckShape DetermineWhichPlayerHand(int wistPlayerNumber)
        {
            int wistPlayerNumberLocal = Math.Max(wistPlayerNumber, 1);
            wistPlayerNumberLocal = Math.Min(wistPlayerNumberLocal, 4);

            switch (wistPlayerNumberLocal)
            {
                case 1:
                    {
                        return Player1Hand;
                    }
                case 2:
                    {
                        return Player2Hand;
                    }
                case 3:
                    {
                        return Player3Hand;
                    }
                case 4:
                    {
                        return Player4Hand;
                    }
                default:
                    {
                        return null;
                    }
            }
        }

        /// <summary>
        /// Zwraca DeckShape Trick odpowiadajacy numerowi wistujacego gracza.
        /// </summary>
        /// <param name="wistPlayerNumber">Number gracza wistujacego.</param>
        /// <returns>DeckShape Trick odpowiadajacy argumentowi wistPlayerNumber</returns>
        public DeckShape DetermineWhichPlayerTrick(int wistPlayerNumber)
        {
            int wistPlayerNumberLocal = Math.Max(wistPlayerNumber, 1);
            wistPlayerNumberLocal = Math.Min(wistPlayerNumberLocal, 4);

            switch (wistPlayerNumberLocal)
            {
                case 1:
                    {
                        return Player1Trick;
                    }
                case 2:
                    {
                        return Player2Trick;
                    }
                case 3:
                    {
                        return Player3Trick;
                    }
                case 4:
                    {
                        return Player4Trick;
                    }
                default:
                    {
                        return null;
                    }
            }
        }

        /// <summary>
        /// Zwraca Hand zuzytych kart odpowiadajacy numerowi wistujacego gracza
        /// </summary>
        /// <param name="wistPlayerNumber">Number gracza wistujacego.</param>
        /// <returns>Hand zuzytywch kart odpowiadajacy argumentowi wistPlayerNumber</returns>
        public Hand DetermineWhichPlayerUsedCards(int wistPlayerNumber)
        {
            int wistPlayerNumberLocal = Math.Max(wistPlayerNumber, 1);
            wistPlayerNumberLocal = Math.Min(wistPlayerNumberLocal, 4);

            switch (wistPlayerNumberLocal)
            {
                case 1:
                    {
                        return Player1UsedCards;
                    }
                case 2:
                    {
                        return Player2UsedCards;
                    }
                case 3:
                    {
                        return Player3UsedCards;
                    }
                case 4:
                    {
                        return Player4UsedCards;
                    }
                default:
                    {
                        return null;
                    }
            }
        }

        /// <summary>
        /// Liczy kto zaczyna.
        /// </summary>
        public void WhoStarts()
        {
            //dealer chodzi o 1 pozycje zgodnie ze wskazowkami zegara
            WistPlayerNumber = 1;   //wyzeruj zeby policzyl kto jest dealerem
            int index = IndexOfPlayer(DealerNumber);
            WistPlayerNumber = index + 1;   //ustaw wistujacego
        }

        /// <summary>
        /// Zlicza liczbe wzietych lew przez gracza.
        /// </summary>
        /// <param name="takenCards">Lewy wziete przez gracza.</param>
        /// <returns>Liczba wzietych lew.</returns>
        public int PointsTaken(List<Card> takenCards)
        {
            return takenCards.Count / 4;
        }

        public void EndGame(bool @rotationGame = false)
        {
            ScorePerRound.Add(new List<int>());
            int size = ScorePerRound.Count - 1;

            //PlayerXUsedCards -> CardsInHand w tym fieldzie przechowywane sa wziete lewy
            ScorePerRound[size].Add(PointsTaken(Player1UsedCards.CardsInHand));
            ScorePerRound[size].Add(PointsTaken(Player2UsedCards.CardsInHand));
            ScorePerRound[size].Add(PointsTaken(Player3UsedCards.CardsInHand));
            ScorePerRound[size].Add(PointsTaken(Player4UsedCards.CardsInHand));

            //zeby sie wyswietlil wynik
            scoreWinRef.ShowResults(rotationGame);
            CleanMemory();
        }
    }

    static public class GameRules
    {
        /// <summary>
        /// Wartosc odpowiedzialna za koniecznosc wistowania nowego rozdania w 2 trefl.
        /// </summary>
        static private bool deuceFirst = true;
        /// <summary>
        /// Wartosc odpowiedzialna za koniecznosc wistowania nowego rozdania w 2 trefl.
        /// </summary>
        static public bool DeuceFirst
        {
            get { return deuceFirst; }
            set { deuceFirst = value; }
        }

        /// <summary>
        /// Generuje mozliwe ruchy.
        /// </summary>
        /// <param name="availableCards">Dostepne karty - np. reka gracza.</param>
        /// <param name="wistCard">Karta wistująca - 1sza karta lewy.</param>
        /// <param name="renons">Zwracany jest renons w kolorze wistu jezeli takowy zachodzi.</param>
        /// <returns>Talie z mozliwymi ruchami, utworzona na podstawie dostepnych kart.</returns>
        static public Deck PossibleMoves(Deck availableCards, Card wistCard, out bool renons)
        {
            Deck possibleMoves = new Deck(availableCards);
            renons = false;

            //jezeli nie ma wistujacej karty to znaczy ze ty wistujesz wiec mozesz zrobic co chcesz
            if (wistCard == null)
            {
                return possibleMoves;
            }

            //jezeli nie masz do koloru to mozesz grac co chcesz
            if (!possibleMoves.HasColor(wistCard.Suit))
            {
                renons = true;
                return possibleMoves;
            }
            //jezeli masz do koloru to mozesz grac tylko do koloru
            else
            {
                possibleMoves.CardsSet = availableCards.Cards.FindAll(card => card.Suit == wistCard.Suit);
                return possibleMoves;
            }
        }

        //NIEDOKONCZONA
        /// <summary>
        /// Porownuje ze soba 2 karty. Bierze pod uwage kolor atu.
        /// </summary>
        /// <param name="first">Pierwsza karta do porownania.</param>
        /// <param name="second">Druga karta do porownania.</param>
        /// <param name="biggerCard">Zwracana wieksza karta.</param>
        /// <returns>Prawda jezeli pierwsza jest wieksza, falsz w przeciwnym wypadku</returns>
        static public bool CompareCards(Card first, Card second, CardSuit trump, out Card biggerCard)
        {
            //jezeli roznego koloru
            if (first.Suit != second.Suit)
            {
                //jezeli druga karta jest atutem to ja zwroc i false
                if (second.Suit == trump)
                {
                    biggerCard = new Card(second);
                    return false;
                }
                //druga karta nie jest atutem wiec pierwsze wygrywa
                biggerCard = new Card(first);
                return true;
            }

            //jezeli tego samego koloru to porownaj
            if (first.CompareTo(second) > 0)
            {
                biggerCard = new Card(first);
                return true;
            }
            //pierwsza karta mniejsza od drugiej (ten sam kolor). zwroc false
            else
            {
                biggerCard = new Card(second);
                return false;
            }
        }

        /// <summary>
        /// Funkcja liczaca ktory z graczy wygral lewe.
        /// </summary>
        /// <returns>Numer gracza ktory wygral lewe.</returns>
        static public int TrickWinner(Card first, Card second, Card third, Card fourth, CardSuit trump, int wistNumber)
        {
            Card winnerCard;
            int winnerIndex = 1;
            List<Card> trickCards = new List<Card>();
            // 1 2 3 4 1 2 3 <- dzieki temu nie musze sie bawic w sprawdzanie
            trickCards.Add(first); trickCards.Add(second); trickCards.Add(third); trickCards.Add(fourth); trickCards.Add(first); trickCards.Add(second); trickCards.Add(third);

            winnerIndex = CompareCards(trickCards[wistNumber - 1], trickCards[wistNumber], trump, out winnerCard) ? wistNumber : wistNumber + 1;
            winnerIndex = CompareCards(winnerCard, trickCards[wistNumber + 1], trump, out winnerCard) ? winnerIndex : wistNumber + 2;
            winnerIndex = CompareCards(winnerCard, trickCards[wistNumber + 2], trump, out winnerCard) ? winnerIndex : wistNumber + 3;

            //zrob modulo zeby wynik byl w przedziale [1,4]
            return (winnerIndex - 1) % 4 + 1;
        }
    }
}
