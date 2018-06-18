using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Whist;
using Whist.Core.Controls;
using Whist.Core.Model;

namespace Whist
{
    public enum AlgorithmTypes
    {
        Random = 1,
        Greedy,
        Heuristic,
    }


    /* Metody odpowiedzialne za sztuczna inteligencje znajduja sie w tym pliku.
     * Korzystaja z funkcji znajdujacych sie w GameRules.cs. Parametrami kazdego
     * algorytmu sa zmienne unikalne dla kazdego gracza, czyli:
     * - mozliwe ruchy
     * - pozycja w lewie (od 1 do 4)
     * - karty lezace na stole w obecnej lewie, ustawione w kolejnosci ich zagrywki
     * 
     *  W kierkach:
     *  - zwyciestwo = nie wziecie lewy
     *  - przegrana = wziecie lewy
     */
    public partial class MainWindow : Window
    {

        /// <summary>
        /// Zwraca obecnie wygrywajaca lewe karte. Jezeli jest tylko 1 karta w lewie to zwroci ja.
        /// Kolorem obowiazujacym jest kolor wistu (czyli 1sza karta na liscie).
        /// </summary>
        /// <param name="curTrickCards">Obecna lewa. Moze sie skladac z od 0 do 4 kart.</param>
        /// <param name="outHighestCard">Zwracana karta w out'ie ktora jest najwysza.</param>
        /// <returns>PRAWDA jezeli jest jakas karta w lewie. Jezeli lewa jest pusta to nie ma co zwroc
        /// i zwraca FALSE, zas w out'ie jest null.</returns>
        public bool curTrickHighestCard(List<Card> curTrickCards, out Card outHighestCard)
        {
            Card highestCard = null;

            //jezeli pusty to zwroc nulla i false (nie ma najwyzszej)
            if (curTrickCards.Count <= 0)
            {
                outHighestCard = highestCard;
                return false;
            }

            highestCard = curTrickCards.First();
            //jezeli jeden element to zwroc go
            if (curTrickCards.Count == 1)
            {
                outHighestCard = highestCard;
                return true;
            }

            //jezeli wiecej niz 1 element to porownaj po kolei
            for (int i = 0; i < curTrickCards.Count; i++)
            {
                GameRules.CompareCards(highestCard, curTrickCards[i], TrumpColor, out highestCard);
            }

            outHighestCard = highestCard;
            return true;
        }

        /// <summary>
        /// Zlicza liczbe kart zagranych w danym kolorze. Uwzglednia skonczone lewy oraz obecna lewe
        /// (tzn. to co lezy na stole)
        /// </summary>
        /// <param name="chosenSuit">Wybrany kolor.</param>
        /// <param name="curTrickCards">Karty lezace na stole.</param>
        /// <returns>Liczbe kart juz zagranych w danym kolorze.</returns>
        public int CardsPlayedInChosenSuit(CardSuit chosenSuit, List<Card> curTrickCards)
        {
            int cardSuit = 0;

            //obecna liczba kart w kolorze na stole
            cardSuit += curTrickCards.Sum(e =>
            {
                int result = 0;
                if (e.Suit == chosenSuit) result++;
                return result;
            });

            //liczba kart o wybranym kolorze w juz zagranych lewach
            cardSuit += PlayedCards.Deck.Cards.Sum(e =>
            {
                int result = 0;
                if (e.Suit == chosenSuit) result++;
                return result;
            });

            return cardSuit;
        }

        /// <summary>
        /// Zwraca wszystkie karty lepsze od currentHighest.
        /// </summary>
        /// <param name="possibleMoves">Zbior kart z ktorych wybieramy te lepsze.</param>
        /// <param name="currentHighest">Karta do ktorej porownujemy.</param>
        /// <param name="trumpColor">Kolor atutu.</param>
        /// <returns>Liste kart wyzszych od wskazanej.</returns>
        public List<Card> BetterCardsThan(Deck possibleMoves, Card currentHighest, CardSuit trumpColor)
        {
            List<Card> betterCards = new List<Card>();

            if (currentHighest.Suit == trumpColor)
            {
                betterCards = possibleMoves.Cards.FindAll(e => (e.Suit == trumpColor && e.CompareToBool(currentHighest)));
            }
            else
            {
                betterCards.AddRange(possibleMoves.Cards.FindAll(e => (e.Suit == currentHighest.Suit && e.CompareToBool(currentHighest) )));
                betterCards.AddRange( possibleMoves.Cards.FindAll(e => e.Suit == trumpColor));
            }

            return betterCards;
        }

        public List<Card> CardsLeftInSuit(CardSuit chosenSuit, List<Card> curTrickCards)
        {
            List<Card> LeftCardsChosenSuit = ExtraFunctions.AllCardsInChosenSuit(chosenSuit);

            for (int i = 0; i < curTrickCards.Count; i++)
            {
                if (curTrickCards[i].Suit == chosenSuit)
                {
                    //sprobuj usunac
                    try
                    {
                        int index = LeftCardsChosenSuit.FindIndex(e => e.Suit == curTrickCards[i].Suit
                                                                    && e.Rank == curTrickCards[i].Rank);
                        if (index >= 0)
                            LeftCardsChosenSuit.RemoveAt(index);
                    }
                    catch
                    {
                        //bledy zwiazane sa z niewystepowaniem elementu wiec nic nie trzeba robic
                    }
                }
            }

            for (int i = 0; i < PlayedCards.Deck.Cards.Count; i++)
            {
                if (PlayedCards.Deck.Cards[i].Suit == chosenSuit)
                {
                    int index = LeftCardsChosenSuit.FindIndex(e => e.Suit == PlayedCards.Deck.Cards[i].Suit
                                                                && e.Rank == PlayedCards.Deck.Cards[i].Rank);
                    if (index >= 0)
                        LeftCardsChosenSuit.RemoveAt(index);
                }
            }

            return LeftCardsChosenSuit;
        }

        public int CardsLeftInSuitOther(Deck playerPossibleMoves, CardSuit chosenSuit, List<Card> curTrickCards, out List<Card> outCardsLeft)
        {
            List<Card> cardsLeft = CardsLeftInSuit(chosenSuit, curTrickCards);

            for (int i = 0; i < playerPossibleMoves.Cards.Count; i++)
            {
                Card readCard = playerPossibleMoves.Cards[i];
                int found = cardsLeft.FindIndex(e => e.Suit == readCard.Suit && e.Rank == readCard.Rank);
                if (found >= 0)
                    cardsLeft.RemoveAt(found);
            }

            outCardsLeft = cardsLeft;
            return cardsLeft.Count;
        }

        /// <summary>
        /// Liczy punkty z obecnej lewy na stole.
        /// </summary>
        /// <param name="curTrickCards">Lista kart znajdujacych sie w obecnej lewie.</param>
        /// <returns>Liczba punktow w kartach na liscie podanej w argumencie.</returns>
        public int PointsOnTable(List<Card> curTrickCards)
        {
            int points = 0;

            points = curTrickCards.Sum(e =>
            {
                int result = 0;
                if (e.Suit == CardSuit.Hearts) result++;
                if (e.Suit == CardSuit.Spades && e.Rank == CardRank.Queen) result += 13;
                return result;
            });

            return points;
        }

        //NIESKONCZONA FUNKCJA
        //probuje estymowac wartosc lewy 
        public float PointsPredictOnTable(List<Card> curTrickCards)
        {
            float predictedPoints = 0;
            predictedPoints += (float)PointsOnTable(curTrickCards);

            //brak estymacji punktow do konca lewy

            return predictedPoints;
        }

        /// <summary>
        /// Liczba punktow ktore zostaly na rekach graczy do konca rozdania.
        /// </summary>
        /// <returns>Liczba punktow.</returns>
        public int PointsLeftTillEnd()
        {
            int points = 0;

            points += PointsTaken(Player1UsedCards.CardsPlayed);
            points += PointsTaken(Player2UsedCards.CardsPlayed);
            points += PointsTaken(Player3UsedCards.CardsPlayed);
            points += PointsTaken(Player4UsedCards.CardsPlayed);

            //Maksymalna liczba punktow to 26. W points jest liczba punktow dotychczas zagranych
            //Zatem 26 - points to szukana liczba
            return 26 - points;
        }

        /// <summary>
        /// Zwraca srednia liczbe punktow na lewe do konca gry.
        /// Poczatkowo jest to 26 punktow na 13 lew = 2 punkty.
        /// </summary>
        /// <returns>Srednia liczba punktow na lewe do konca gry.</returns>
        public float PointsPerTrickTillEnd()
        {
            int tricksLeft = PlayedCards.Deck.Cards.Count;
            return (float)(52 - tricksLeft) / 4 / PointsLeftTillEnd();
        }

        /// <summary>
        /// Liczba lew do konca poza aktualnie rozgrywana. 
        /// Jezeli rozegralismy juz 6, teraz gramy siodma to zwroci 6.
        /// </summary>
        /// <returns>Liczbe pozostalych lew.</returns>
        public int TricksLeft()
        {
            int cardsLeft = PlayedCards.Deck.Cards.Count;
            return (52 - cardsLeft) / 4 - 1;
        }

        //wylicz w co wistowac
        //nie trzeba listy kart w obecnej lewie bo jest pusta
        public Card Greedy_WhistCard(Deck playerPossibleMoves)
        {
            Card whist = playerPossibleMoves.TopCard;
            //jezeli tylko 1 karta to zwroc (nie ma po co liczyc)
            if (playerPossibleMoves.Cards.Count == 1)
                return whist;

            //zwroc najsilniejsza karte w najdluzszym z kolorow
            CardSuit longColor = playerPossibleMoves.LongestColor(true);
            //odkrec zeby zagrac najwyzsza
            playerPossibleMoves.Cards.Reverse();
            whist = playerPossibleMoves.GetColor(longColor);
            //odkrec z powrotem
            playerPossibleMoves.Cards.Reverse();

            return whist;
        }

        //Wygrana jak najmniejszym kosztem; najnizsza karta pod wzgledem 
        //starszenstwa zapewniajaca wziecie lewy!
        public Card Greedy_MaxWinningCard(Deck playerPossibleMoves, List<Card> curTrickCards)
        {
            Card maxWin = playerPossibleMoves.TopCard;
            Card highestTillNow;
            List<Card> betterCards;

            //nie jestes wistujacym
            if (curTrickHighestCard(curTrickCards, out highestTillNow))
            {   
                //Jezeli najwyzsza karte do teraz ma twoj partner (czyli jestes albo 3ci albo 4ty)
                if ((curTrickCards.Count == 2 && highestTillNow == curTrickCards[0]) ||
                     (curTrickCards.Count == 3 && highestTillNow == curTrickCards[1]))
                {
                    //zrzuc najgorsza mozliwa karte
                    maxWin = Greedy_MinLoosingCard(playerPossibleMoves, curTrickCards);
                    return maxWin;
                }

                //wszystkie lepsze karty
                betterCards = BetterCardsThan(playerPossibleMoves, highestTillNow, TrumpColor);
                //masz jakas lepsza karte to zagraj ta najmniej warta
                if (betterCards.Count > 0)
                {
                    maxWin = betterCards.First();
                    return maxWin;
                }
                //nie masz zadnej - minimalizuj straty
                else
                {
                    maxWin = Greedy_MinLoosingCard(playerPossibleMoves, curTrickCards);
                    return maxWin;
                }
            }
            //jestes wistujacym
            else
            {
                maxWin = Greedy_WhistCard(playerPossibleMoves);
                return maxWin;
            }
        }

        //Zminimalizuj swoja przegrana - jezeli wiesz ze NIE wezmiesz lewy to zrzuc najmlodsza
        //karte w kolorze wistu (jezeli masz) lub w kolorze nieatutowym(jezeli nie masz wistu ani atutu)
        public Card Greedy_MinLoosingCard(Deck playerPossibleMoves, List<Card> curTrickCards)
        {
            Card minLoose = playerPossibleMoves.TopCard;
            Card highestTillNow;

            //nie jestes wistujacym
            if (curTrickHighestCard(curTrickCards, out highestTillNow))
            {
                //Jezeli masz dodac do koloru to dodaj najmniejsza
                if (curTrickCards[0].Suit == playerPossibleMoves.TopCard.Suit)
                {
                    minLoose = playerPossibleMoves.BottomCard;
                    return minLoose;
                }
                else
                {
                    //jak nie masz do koloru to zrzuc najkrotszy
                    //do heurystyki mozna dopisac ALE INNY NIZ ATUT
                    CardSuit shortColor = playerPossibleMoves.ShortestColor();
                    minLoose = playerPossibleMoves.GetColor(shortColor);
                    return minLoose;
                }
            }
            //jestes wistujacym
            else
            {
                minLoose = Greedy_WhistCard(playerPossibleMoves);
                return minLoose;
            }
        }

        /// <summary>
        /// Algorytm robiacy losowy ruch z wszystkich mozliwych
        /// </summary>
        /// <param name="playerPossibleMoves">Zbior wszystkich mozliwych ruchow gracza.</param>
        /// <param name="trickPosition">Pozycja w lewie; 1 = wistjacy, itd az do 4 - ostatni.</param>
        /// <param name="curTrickCards">Lista kart ktore zostaly zagrane w obecnej lewie (dlugosc od 0 do 3)</param>
        public Card RandomAlgorithm(Deck playerPossibleMoves, int trickPosition, List<Card> curTrickCards)
        {
            //w przypadku algorytmu losowego nic nie bierzemy pod uwage - z wszystkich mozliwych ruchow bierzemy jakikolwiek
            Card randomCard = playerPossibleMoves.RandomCard;            

            currentTrick.Add(randomCard);
            return randomCard;
        }

        public Card GreedyAlgorithm(Deck playerPossibleMoves, int trickPosition, List<Card> curTrickCards)
        {
            //probuj caly czas maksymalnie wygrywac
            Card greedyCard = Greedy_MaxWinningCard(playerPossibleMoves, curTrickCards);

            currentTrick.Add(greedyCard);
            return greedyCard;
        }

        //HeuristicAlgorithm znajduje sie w AI_Heuristic.cs

        /// <summary>
        /// Zwraca ruch wg wybranego algorytmu.
        /// </summary>
        /// <param name="playerPossibleMoves">Mozliwe ruchy gracza.</param>
        /// <param name="trickPosition">Pozycja w lewie.</param>
        /// <param name="curTrickCards">Karty w lewie(od 0 do 3).</param>
        /// <param name="alg">Wybrany algorytm - enum.</param>
        /// <returns>Zwraca wyliczona karte wg wybranego algorytmu.</returns>
        public Card ChooseAlgorithm(Deck playerPossibleMoves, int trickPosition, List<Card> curTrickCards, AlgorithmTypes alg)
        {
            switch (alg)
            {
                case AlgorithmTypes.Random:
                    {
                        return RandomAlgorithm(playerPossibleMoves, trickPosition, curTrickCards);
                    }
                case AlgorithmTypes.Greedy:
                    {
                        return GreedyAlgorithm(playerPossibleMoves, trickPosition, curTrickCards);
                    }
                case AlgorithmTypes.Heuristic:
                    {
                        //HeuristicAlgorithm(...) znajduje sie w AI_Heuristic.cs
                        return HeuristicAlgorithm(playerPossibleMoves, trickPosition, curTrickCards);
                    }
                default:
                    return RandomAlgorithm(playerPossibleMoves, trickPosition, curTrickCards);
            }
        }
    }
}

//inny sposob sprawdzania czy najwyzsza karta nalezy do partnera
//if (highestTillNow.Deck.Partner == playerPossibleMoves.Partner.Partner)
//{ 
//}
