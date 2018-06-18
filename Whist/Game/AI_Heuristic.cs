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
    public partial class MainWindow : Window
    {
        /* Wszystkie funkcje dotyczace herustyki znajduja
         * sie w tym pliku. W AI.cs znajduja sie wszystkie 
         * metody pomocnicze oraz proste algorytmy Random
         * i Greedy
         */

        //zwraca czy obaj przeciwnicy maja renons w atucie
        //mozna uzywac tylko gdy heurystyka wistuje
        public bool BothEnemiesShortTrump()
        {
            if (PlayersUsedCards[IndexOfPlayer(1)].CardRenons[TrumpColor] == true &&
                PlayersUsedCards[IndexOfPlayer(3)].CardRenons[TrumpColor] == true)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Nastepny najkrotszy
        /// </summary>
        /// <param name="playerPossibleMoves"></param>
        /// <param name="currentCard"></param>
        /// <param name="skip"></param>
        /// <param name="outNextShortCard"></param>
        /// <returns>TRUE jezeli uda sie znalezc, FALSE jezeli nie uda sie znalezc.</returns>
        public bool NextShortColor(Deck playerPossibleMoves, Card currentCard, int skip, out Card outNextShortCard)
        {
            //pobierz kolejny najkrotszy kolor
            CardSuit nextShortColor = playerPossibleMoves.ShortestColor(true, skip);
            Card shortCard = playerPossibleMoves.GetColor(nextShortColor);
            //jezeli nie ma kolejnego koloru to nic nie poradzimy
            if (currentCard == shortCard)
            {
                outNextShortCard = currentCard;
                return false;
            }

            currentCard = shortCard;
            outNextShortCard = currentCard;
            return true;
        }

        /// <summary>
        /// Nastepny najdluzszy
        /// </summary>
        /// <param name="playerPossibleMoves"></param>
        /// <param name="currentCard"></param>
        /// <param name="skip"></param>
        /// <param name="outNextLongCard"></param>
        /// <returns>TRUE jezeli uda sie znalezc, FALSE jezeli nie uda sie znalezc.</returns>
        public bool NextLongColor(Deck playerPossibleMoves, Card currentCard, int skip, out Card outNextLongCard)
        {
            //pobierz kolejny najkrotszy kolor
            CardSuit nextLongColor = playerPossibleMoves.LongestColor(true, skip);
            Card longCard = playerPossibleMoves.GetColor(nextLongColor);
            //jezeli nie ma kolejnego koloru to nic nie poradzimy
            if (currentCard == longCard)
            {
                outNextLongCard = currentCard;
                return false;
            }

            currentCard = longCard;
            outNextLongCard = currentCard;
            return true;
        }

        //NIESKONCZONA
        //wylicz w co wistowac
        //nie trzeba listy kart w obecnej lewie bo jest pusta
        //to trzeba mocno poprawic
        public Card Heuristic_WhistCard(Deck playerPossibleMoves)
        {
            Card whist = playerPossibleMoves.TopCard;
            Card otherWhist = whist;
            List<Card> cardsLeft = new List<Card>();
            int points = PlayersHandDecks[WistPlayerNumber - 1].Deck.Points;
            int tricksLeft = PlayersHandDecks[WistPlayerNumber - 1].Deck.Cards.Count;
            int longest = PlayersUsedCards[WistPlayerNumber - 1].startingColorsLength.Max(x => x.ColorLength);
            CardSuit longestColor = PlayersUsedCards[WistPlayerNumber - 1]
                                    .startingColorsLength.FirstOrDefault(e => e.ColorLength == longest).Color;

            //jezeli tylko 1 karta to zwroc (nie ma po co liczyc)
            if (playerPossibleMoves.Cards.Count == 1)
                return whist;

            //masz karte ktora zgarnia i nie jest atutem
            int safeIndex = playerPossibleMoves.Cards.FindIndex(e => e.Power == 13);
            //znajdz najkrotszy kolor
            CardSuit shortColor = playerPossibleMoves.ShortestColor(true);
            //znajdz najdluzszy kolor
            CardSuit longColor = playerPossibleMoves.LongestColor(true);

            var trumpCards = playerPossibleMoves.Cards.Where(e => e.Suit == TrumpColor);
            int trumpPower = playerPossibleMoves.PowerInSuit(TrumpColor);
            whist = null;
            // 11 zagraj w malego/duzego atu
            if (trumpCards.ToList().Count > 0 && trumpPower > 100)
            {
                trumpCards.OrderBy(e => e.Power);
                if (trumpCards.First().Power == 26)
                    whist = trumpCards.First();
                else
                    whist = trumpCards.ToList()[2];
            }

            // 22 zagraj w kolor kolegi
            if (whist == null)
            {
                //zagraj w kolor kolegi z druzyny
                if (PlayersUsedCards[IndexOfPlayer(2)].whistColors.Count > 0)
                {
                    //zagraj wysoko (wczesniej bylo nisko)
                    playerPossibleMoves.Cards.Reverse();
                    whist = playerPossibleMoves.GetColor(PlayersUsedCards[IndexOfPlayer(2)].whistColors.First());
                    playerPossibleMoves.Cards.Reverse();
                }
            }

            // 33 zacznij patrzec na swoje karty
            if (whist == null)
            {
                whist = playerPossibleMoves.TopCard;
                //masz karte ktora zgarnia i nie jest atutem
                if (safeIndex >= 0)
                {
                    whist = playerPossibleMoves.Cards[safeIndex];
                }
                //znajdz najkrotszy kolor
                //jezeli masz singletona to go zagraj
                else if (tricksLeft > 10)
                {
                    if (playerPossibleMoves.ColorLength(shortColor) == 1)
                        whist = playerPossibleMoves.GetColor(shortColor);
                }
                //zagraj w swoj najdluzszy poczatkowy kolor NISKO
                else if (points < 9 && playerPossibleMoves.HasColor(longestColor))
                {
                    whist = playerPossibleMoves.GetColor(longestColor);
                }
                //zagraj w swoj najdluzszy poczatkowy kolor WYSOKO
                else if (points >= 9 && playerPossibleMoves.HasColor(longestColor))
                {
                    playerPossibleMoves.Cards.Reverse();
                    whist = playerPossibleMoves.GetColor(longestColor);
                    //odkrec z powrotem
                    playerPossibleMoves.Cards.Reverse();
                }
                else
                {
                    //odkrec zeby zagrac najwyzsza
                    playerPossibleMoves.Cards.Reverse();
                    whist = playerPossibleMoves.GetColor(longColor);
                    //odkrec z powrotem
                    playerPossibleMoves.Cards.Reverse();
                }
            }
          
            int cardsLeftInSuit = 13 - CardsPlayedInChosenSuit(TrumpColor, currentTrick);
            int ownedTrumps = playerPossibleMoves.ColorLength(TrumpColor);
            //44 jezeli przeciwnicy nie maja atutu to nie graj atutem
            if (BothEnemiesShortTrump() || ownedTrumps >= cardsLeftInSuit )
            {
                int i = 1;
                //zagraj kolejna najwyzsza w swoim kolorze
                playerPossibleMoves.Cards.Reverse();
                while (whist.Suit == TrumpColor)
                {
                    if (!NextLongColor(playerPossibleMoves, whist, i, out whist))
                        break;
                    i++;
                }
                //odkrec
                playerPossibleMoves.Cards.Reverse();
            }

            return whist;
        }

        //Wygrana jak najmniejszym kosztem; najnizsza karta pod wzgledem 
        //starszenstwa zapewniajaca wziecie lewy!
        public Card Heuristic_TryToWinCard(Deck playerPossibleMoves, List<Card> curTrickCards)
        {
            Card heuristicWin = playerPossibleMoves.TopCard;
            Card highestTillNow;
            List<Card> betterCards;

            //Na pewno nie jestes wistujacym -> HeuristicAlgorithm sie tym zajmuje
            curTrickHighestCard(curTrickCards, out highestTillNow);

            //wszystkie lepsze karty
            betterCards = BetterCardsThan(playerPossibleMoves, highestTillNow, TrumpColor);

            //Jezeli najwyzsza karte do teraz ma twoj partner (jestes 3ci)
            if ((curTrickCards.Count == 2 && highestTillNow == curTrickCards[0]) )
            {
                //heurystyka
                //jezeli masz renons w kolorze w ktorym zagrywa ci kolega to przebij
                //najnizszym atutem
                if (PlayersUsedCards[IndexOfPlayer(2)].CardRenons[highestTillNow.Suit])
                {
                    if (highestTillNow.Power < 10)
                    {
                        heuristicWin = betterCards.FirstOrDefault(e => e.Suit == trumpColor);
                        if (heuristicWin != null)
                            return heuristicWin;
                    }
                }

                heuristicWin = Heuristic_MinLoosingCard(playerPossibleMoves, curTrickCards);
                return heuristicWin;
            }

            //Jezeli najwyzsza karte do teraz ma twoj partner (jestes 4ty)
            if ((curTrickCards.Count == 3 && highestTillNow == curTrickCards[1]))
            {
                heuristicWin = Heuristic_MinLoosingCard(playerPossibleMoves, curTrickCards);
                return heuristicWin;
            }

            //masz jakas lepsza karte to zagraj ta najmniej warta
            if (betterCards.Count > 0)
            {
                //granie najmniejszej lepszej karty nie jest zawsze takie dobre
                heuristicWin = betterCards.First();
                return heuristicWin;
            }
            //nie masz zadnej lepszej karty - minimalizuj straty
            else
            {
                heuristicWin = Heuristic_MinLoosingCard(playerPossibleMoves, curTrickCards);
                return heuristicWin;
            }   

            return heuristicWin;
        }

        //Zminimalizuj swoja przegrana - jezeli wiesz ze NIE wezmiesz lewy to zrzuc najmlodsza
        //karte w kolorze wistu (jezeli masz) lub w kolorze nieatutowym(jezeli nie masz wistu ani atutu)
        public Card Heuristic_MinLoosingCard(Deck playerPossibleMoves, List<Card> curTrickCards)
        {
            Card minLoose = playerPossibleMoves.TopCard;
            Card highestTillNow;

            //nie jestes wistujacym
            curTrickHighestCard(curTrickCards, out highestTillNow);

            //Jezeli masz dodac do koloru to dodaj najmniejsza
            if (curTrickCards[0].Suit == playerPossibleMoves.TopCard.Suit)
            {
                //nie ma lepszego ruchu niz dodanie najgorszej w kolorze
                minLoose = playerPossibleMoves.BottomCard;
                return minLoose;
            }
            else
            {
                //indeks karty o najmniejszym power
                int minIndex = playerPossibleMoves.Cards.Min(x => x.Power);

                //zrzucaj konsekwentnie najkrotszy kolor chyba ze zostanie ci As
                //jak nie masz do koloru to zrzuc najkrotszy
                //ALE INNY NIZ ATUT
                CardSuit shortColor = playerPossibleMoves.ShortestColor();
                int colorLength = playerPossibleMoves.ColorLength(shortColor);
                if (colorLength > 2)
                {
                    minLoose = playerPossibleMoves.Cards.FirstOrDefault(x => x.Power == minIndex);
                    return minLoose;
                }

                minLoose = playerPossibleMoves.GetColor(shortColor);

                //jezeli masz zrzucic atut to zamiast niego zrzuc karte o najmnijeszym power
                if (minLoose.Suit == TrumpColor)
                {
                    minLoose = playerPossibleMoves.Cards.FirstOrDefault(x => x.Power == minIndex);
                    return minLoose;
                }
                //int i = 1;
                ////jak zrzucasz zle karty to nie zrzucaj atutu ani karty ktora jest dobra
                ////while (minLoose.Suit == TrumpColor || minLoose.Power == 13)
                //{
                //    //pobierz kolejny najkrotszy kolor
                //    shortColor = playerPossibleMoves.ShortestColor(true, i);
                //    Card shortCard = playerPossibleMoves.GetColor(shortColor);
                //    //jezeli nie ma kolejnego koloru to nic nie poradzimy
                //    if (minLoose == shortCard)
                //        break;

                //    minLoose = shortCard;
                //    i++;
                //}
                return minLoose;
            }       
        }

        public Card HeuristicAlgorithm(Deck playerPossibleMoves, int trickPosition, List<Card> curTrickCards)
        {
            Card heuristicCard = playerPossibleMoves.TopCard;
            //wez pod uwage krotkosci
            //wez pod uwage liczbe lew do konca
            //wez pod uwage zagrane karty
            //wez pod uwage przyszlosc ?

            //int howMany = CardsPlayedInChosenSuit(randomCard.Suit, currentTrick);
            //List<Card> cardsleft = CardsLeftInChosenSuit(randomCard.Suit, currentTrick);

            //poza obecna!
            int tricksLeft = (52 - PlayedCards.Deck.Cards.Count) / 4;
            Card highestTillNow;
            curTrickHighestCard(curTrickCards, out highestTillNow);

            //jestes wistujacym
            if (trickPosition == 0)
            {
                heuristicCard = Heuristic_WhistCard(playerPossibleMoves);
                PlayersUsedCards[WistPlayerNumber - 1].whistColors.Add(heuristicCard.Suit);
            }
            //jezeli jestes po wistujacym
            else if (trickPosition == 1)
            {
                //jezeli masz mniej niz X punktow to zagraj nisko
                //if (PlayersHandDecks[IndexOfPlayer(trickPosition)].Deck.Points < 10)
                //    heuristicCard = Heuristic_MinLoosingCard(playerPossibleMoves, curTrickCards);
                //else
                heuristicCard = Heuristic_TryToWinCard(playerPossibleMoves, curTrickCards);
            }
            //jezeli jestes przedostatni
            else if (trickPosition == 2)
            {
                heuristicCard = Heuristic_TryToWinCard(playerPossibleMoves, curTrickCards);
            }
            //jezeli jestes ostatni
            //popracowac nad tym jeszcze
            else if (trickPosition == 3)
            {
                heuristicCard = Heuristic_TryToWinCard(playerPossibleMoves, curTrickCards);
            }
            //blad
            else
            {
                MessageBox.Show("Cos jest nie tak, nigdy nie powinienem tu byc", "Blad");
                Console.WriteLine("Cos jest nie tak, nigdy nie powinienem tu byc");
            }

            currentTrick.Add(heuristicCard);
            return heuristicCard;
        }
    }
}
