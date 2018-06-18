using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Whist;
using Whist.Core.Controls;
using Whist.Core.Model;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Threading;
using System.Diagnostics;

namespace Whist
{
    public static class ExtraFunctions
    {
        static public void WaitTime(int miliseconds)
        {
            int currentTick = Environment.TickCount;
            int endTick = currentTick;

            while (endTick - currentTick < miliseconds)
            {
                endTick = Environment.TickCount;
            }
            return;
        }

        /// <summary>
        /// Mierzy czas wykonania sie akcji.
        /// </summary>
        /// <param name="action">Akcja ktorej czas wykonania chcemy zmierzyc.</param>
        /// <returns>Struktura TimeSpan w postaci ktorej zwracany jest czas.</returns>
        public static TimeSpan Time(Action action)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            action();
            stopwatch.Stop();
            return stopwatch.Elapsed;
        }

        public static Stopwatch GlobalWatch;
        public static void BeginTime()
        {
            GlobalWatch = Stopwatch.StartNew();
        }
        public static TimeSpan EndTime()
        {
            GlobalWatch.Stop();
            return GlobalWatch.Elapsed;
        }

        /// <summary>
        /// Robi podloge i sufit z liczby number. Sufitem jest max, podloga jest min.
        /// </summary>
        /// <param name="number">Liczba z ktorej robiony jest sufit/podloga.</param>
        /// <param name="min">Wartosc minimalna.</param>
        /// <param name="max">Wartosc maksymalna.</param>
        /// <returns>Zwraca ta sama liczbe chyba ze jest poza przedzialem.</returns>
        public static int InRange(int number, int min, int max)
        {
            if (number > max)
                return max;
            else if (number < min)
                return min;
            else
                return number;

        }

        /// <summary>
        /// Zwraca liste wszystkich 13 kart w kolorze.
        /// </summary>
        /// <param name="chosenSuit">Wybrany kolor.</param>
        /// <param name="game">Gra na rzecz ktorej tworzone sa karty znajdujace sie w liscie.</param>
        /// <returns>Lista 13 kart w kolorze.</returns>
        public static List<Card> AllCardsInChosenSuit(CardSuit chosenSuit)
        {
            List<Card> allSuitCards = new List<Card>();

            for (int i = 1; i <= 13; i++)
            {
                allSuitCards.Add(new Card((CardRank)i, chosenSuit));
            }

            HighCardSuitComparer comparer = new HighCardSuitComparer();
            allSuitCards.Sort( comparer );
            return allSuitCards;
        }

        public static string AlgorithmName(AlgorithmTypes alg)
        {
            switch(alg)
            {
                case AlgorithmTypes.Greedy:
                    return "Greedy";
                case AlgorithmTypes.Random:
                    return "Random";
                case AlgorithmTypes.Heuristic:
                    return "Heuristic";
                default:
                    return "I do not know this algorithm";
            }
        }

        /// <summary>
        /// Zwraca boolowa wartosc true z prawdopodobienstwem percent.
        /// </summary>
        /// <param name="percent">Procenty podawane w intie. Przyklad percent=50
        /// daje szanse 50% na wylosowanie true.</param>
        /// <returns>Zwraca true lub false w zaleznosci od wylosowanych wartosci.</returns>
        public static bool RandomBool(int percent)
        {
            Random rnd = new Random();
            //zrzutuj procenty na 0-1
            percent = InRange(percent, 0, 100) / 100;
            //jezeli wylosowana jest w przedziale [0, percent] to zwroc true
            if (rnd.NextDouble() <= percent)
                return true;
            else
                return false;
        }
    }

    public partial class MainWindow : Window
    {
        public class Alpha
        {

            // This method that will be called when the thread is started
            public void Beta()
            {
                while (true)
                {
                    Console.WriteLine("Alpha.Beta is running in its own thread.");
                }
            }

            public void Theta()
            {
                ExtraFunctions.WaitTime(5000);
            }
        };
    }
}
