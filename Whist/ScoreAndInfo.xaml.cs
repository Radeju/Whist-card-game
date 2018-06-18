using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Whist.Core.Controls;
using Whist.Core.Model;
using Microsoft.Win32;

namespace Whist
{
    /// <summary>
    /// Interaction logic for ScoreAndInfo.xaml
    /// </summary>
    public partial class ScoreAndInfo : Window
    {
        private Whist.MainWindow mainWinRef;
        public Whist.MainWindow MainWinRef
        {
            get { return mainWinRef; }
            set { mainWinRef = value; }
        }

        delegate string ListToString(List<int> numbers);

        ListToString strDel = n =>
        {
            string s = "";
            s += "\t" + n.ToString();
            return s;
        };

        public ScoreAndInfo(Whist.MainWindow @mWinRef = null)
        {
            InitializeComponent();
            mainWinRef = mWinRef;

            resultBox.Items.Add("Nr partii  \t Gracz 1(W)  \t Gracz 2(N)  \t Gacz 3(E)  \t Gracz 4(S)");
            resultBox.Items.Add("--------\n Suma");

            radioAIChecked.Checked -= radioAIChecked_Checked;
            radioAIChecked.IsChecked = true;
            radioAIChecked.Checked += radioAIChecked_Checked;

            radioAutoNo.Checked -= radioAutoNo_Checked;
            radioAutoNo.IsChecked = true;
            radioAutoNo.Checked += radioAutoNo_Checked;

            radioNotShow.Checked -= radioNotShow_Checked;
            radioNotShow.IsChecked = true;
            radioNotShow.Checked += radioNotShow_Checked;

            resultButton.Click += resultButton_Click;
            textBoxRepetitions.IsEnabled = false;
            textBoxRepetitionsRepeat.IsEnabled = false;

            //domyslnie random wszedzie
            radioPlayer1Random.IsChecked = true;
            radioPlayer2Random.IsChecked = true;
            radioPlayer3Random.IsChecked = true;

            radioAutoYesRepeat.Content += Environment.NewLine + "hands once";
        }

        public ScoreAndInfo()
            : this(null)
        {
        }

        //Dopisac zeby pokazywal ktore zagrania to byly rotacyjne
        public void ShowResults(bool @rotationGame = false)
        {
            string oneRound;
            string allRoundsSum;
            List<int> scoresList = new List<int>{ 0, 0 , 0 , 0};
            List<int> individualScore = new List<int> { 0, 0, 0, 0 };
            int currRoundNumber = 0;

            resultBox.Items.RemoveAt(resultBox.Items.Count - 1);

            //dopisuje tutaj calego stringa odpowiedzialnego za 1 rozdanie
            if (mainWinRef.ScorePerRound.Count > 0)
            {
                int NS = 0;
                int WE = 0;
                currRoundNumber = mainWinRef.ScorePerRound.Count;
                oneRound = "  " + currRoundNumber.ToString();
                if (rotationGame) oneRound += "(R)";
                //wylicz punkty kazdej pary ; liczba wzietych lew wspolnie - 6
                NS = Math.Max(NS, mainWinRef.ScorePerRound[currRoundNumber - 1][1] + mainWinRef.ScorePerRound[currRoundNumber - 1][3] - 6);
                WE = Math.Max(WE, mainWinRef.ScorePerRound[currRoundNumber - 1][0] + mainWinRef.ScorePerRound[currRoundNumber - 1][2] - 6);
                for (int j = 0; j < 2; j++)
                {
                    oneRound += "\t\t  " + WE.ToString() + " (" + mainWinRef.ScorePerRound[currRoundNumber - 1][0 + j*2] + ")";
                    oneRound += "\t\t  " + NS.ToString() + " (" + mainWinRef.ScorePerRound[currRoundNumber - 1][1 + j * 2] + ")";
                }
                resultBox.Items.Add(oneRound);
            }

            //sumaryczne wyniki, liczone na biezaco
            //resultBox.Items.Add("--------");
            for (int i = 0; i < currRoundNumber; i++)
            {
                int NS = 0;
                int WE = 0;
                //wylicz punkty kazdej pary ; liczba wzietych lew wspolnie - 6
                NS = Math.Max(NS, mainWinRef.ScorePerRound[i][1] + mainWinRef.ScorePerRound[i][3] - 6);
                WE = Math.Max(WE, mainWinRef.ScorePerRound[i][0] + mainWinRef.ScorePerRound[i][2] - 6);

                for (int j = 0; j < 2; j++)
                {
                    scoresList[0 + j*2] += WE;
                    scoresList[1 + j * 2] += NS;
                }

                for (int j = 0; j < mainWinRef.ScorePerRound[i].Count; j++)
                {
                    individualScore[j] += mainWinRef.ScorePerRound[i][j];
                }
            }
            allRoundsSum = "--------\n Suma";
            for (int i = 0; i < scoresList.Count; i++)
            {
                allRoundsSum += "\t\t  " + scoresList[i].ToString() + " (" + individualScore[i] + ")"; ;
            }

            allRoundsSum += "\nWynik WE: " + (scoresList[0]).ToString();
            allRoundsSum += "\nWynik NS: " + (scoresList[1]).ToString();

            resultBox.Items.Add(allRoundsSum);
        }

        private void resultButton_Click(object sender, RoutedEventArgs e)
        {
            //nic sie tutaj poki co nie dzieje
            //resultBox.Items.Add(MainWinRef.Dealer.Deck.Cards.Count.ToString());
            e.Handled = true;

            mainWinRef.ScorePerRound.Clear();
            mainWinRef.DealerNumber = 3;
            resultBox.Items.Clear();
            resultBox.Items.Add("Nr partii  \t Gracz 1(W)  \t Gracz 2(N)  \t Gacz 3(E)  \t Gracz 4(S)");
            resultBox.Items.Add("--------\n Suma");
        }

        #region Player 4 controller group box
        private void radioHumanChecked_Checked(object sender, RoutedEventArgs e)
        {
            mainWinRef.HumanPlayer = false;
        }

        private void radioAIChecked_Checked(object sender, RoutedEventArgs e)
        {
            mainWinRef.HumanPlayer = true;
        }
        #endregion

        #region Automatic play group box
        private void radioAutoNo_Checked(object sender, RoutedEventArgs e)
        {
            mainWinRef.AutoPlay = false;
            mainWinRef.Repeat = false;

            //wylacz oba boxy
            textBoxRepetitions.IsEnabled = false;
            textBoxRepetitionsRepeat.IsEnabled = false;
        }

        private void radioAutoYes_Checked(object sender, RoutedEventArgs e)
        {
            mainWinRef.AutoPlay = true;
            mainWinRef.Repeat = false;

            textBoxRepetitions.IsEnabled = true;
            textBoxRepetitionsRepeat.IsEnabled = false;
        }

        //graj z powtorzeniami
        private void radioAutoYesRepeat_Checked(object sender, RoutedEventArgs e)
        {
            mainWinRef.AutoPlay = true;
            mainWinRef.Repeat = true;

            textBoxRepetitions.IsEnabled = false;
            textBoxRepetitionsRepeat.IsEnabled = true;
        }

        private void textBoxRepetitions_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                mainWinRef.Repetitions = Convert.ToInt32(textBoxRepetitions.Text);
                mainWinRef.Repetitions = ExtraFunctions.InRange(mainWinRef.Repetitions, 1, 500);
            }
            catch
            {
                mainWinRef.Repetitions = 1;
            }
        }

        private void textBoxRepetitionsRepeat_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                mainWinRef.Repetitions = Convert.ToInt32(textBoxRepetitionsRepeat.Text);
                mainWinRef.Repetitions = ExtraFunctions.InRange(mainWinRef.Repetitions, 1, 500);
            }
            catch
            {
                mainWinRef.Repetitions = 1;
            }
        }
        #endregion

        #region Show call cards group box
        private void radioNotShow_Checked(object sender, RoutedEventArgs e)
        {
            mainWinRef.Player1Hand.CardSpacerY = 20;
            mainWinRef.Player2Hand.CardSpacerX = 20;
            mainWinRef.Player3Hand.CardSpacerY = 20;

            mainWinRef.Player1Hand.Deck.AllCardsNotVisible();
            mainWinRef.Player2Hand.Deck.AllCardsNotVisible();
            mainWinRef.Player3Hand.Deck.AllCardsNotVisible();
        }

        private void radioShow_Checked(object sender, RoutedEventArgs e)
        {
            mainWinRef.Player1Hand.CardSpacerY = 45;
            mainWinRef.Player2Hand.CardSpacerX = 45;
            mainWinRef.Player3Hand.CardSpacerY = 45;

            mainWinRef.Player1Hand.Deck.AllCardsVisible();
            mainWinRef.Player2Hand.Deck.AllCardsVisible();
            mainWinRef.Player3Hand.Deck.AllCardsVisible();
        }
        #endregion

        //obecnie nie uzywana, event do check boxa za Human
        private void HumanChecked_Checked(object sender, RoutedEventArgs e)
        {
            mainWinRef.HumanPlayer = true;
            //AIChecked.IsChecked = false;

            //HumanChecked.IsEnabled = false;
            //AIChecked.IsEnabled = true;
        }

        //obecnie nie uzywana, event do check boxa za AI
        private void AIChecked_Checked(object sender, RoutedEventArgs e)
        {
            mainWinRef.HumanPlayer = false;
            //HumanChecked.IsChecked = false;

            //HumanChecked.IsEnabled = true;
            //AIChecked.IsEnabled = false;
        }

        private void buttonPlay_Click(object sender, RoutedEventArgs e)
        {
            mainWinRef.MainWindow_DealButton_Click(sender, e);
        }

        #region Menu - File
        /// <summary>
        /// Zapisuje pod wskazana sciezke wyniki. Format pliku to .txt. Nie jest to dobrze
        /// przegladac w notatniku, lepiej w jakim calc'u albo excel'u.
        /// </summary>
        /// <param name="results">Wyniki do zapisania.</param>
        /// <param name="fileName">Sciezka do pliku.</param>
        private void SaveFile(List<List<int>> results, string fileName)
        {
            StringBuilder resultStrings = new StringBuilder();

            //zapisz algorytm kazdego gracza
            resultStrings.Append("         ");
            for (int i = 0; i < 4; i++)
                resultStrings.Append(" \t\t" + ExtraFunctions.AlgorithmName(mainWinRef.PlayersAlgorithms[i]));
            resultStrings.AppendLine();

            //zeby bylo latwiej skopiowac do excela / calca zmienia sie 1sza linia
            resultStrings.Append("Nr partii\t\tGracz 1(W)\t\tGracz 2(N)\t\tGacz 3(E)\t\tGracz 4(S)");
            for (int i = 1; i < resultBox.Items.Count; i++)
            {
                resultStrings.AppendLine();
                resultStrings.Append(resultBox.Items[i].ToString());
            }

            using (StreamWriter outfile = new StreamWriter(fileName, false, Encoding.Unicode))
            {
                outfile.Write(resultStrings.ToString());
            }
        }

        private void MenuItemSaveResults_Click(object sender, RoutedEventArgs e)
        {
            int resultSize = mainWinRef.ScorePerRound.Count;
            if (resultSize <= 0)
            {
                MessageBox.Show("There are no results to be saved.", "Cannot save results");
                return;
            }

            SaveFileDialog _SD = new SaveFileDialog();
            _SD.Filter = "Text File (*.txt)|*.txt|Show All Files (*.*)|*.*";
            _SD.FileName = "results" + DateTime.Now.Date.ToString();
            _SD.FileName = _SD.FileName.Remove(_SD.FileName.Length - 9);    //-9 bo tyle znakow uzywa na godzine
            _SD.Title = "Save results as";
            if (_SD.ShowDialog() == true)
            {
                //Console.WriteLine("udalo sie zapisac");
                SaveFile(mainWinRef.ScorePerRound, _SD.FileName);
            }
        }

        private void LoadFile(string filename)
        {
            string[] lines;
            List<Card> loadedCards = new List<Card>();

            try
            {
                lines = System.IO.File.ReadAllLines(filename);
                if (ParseFile(lines, out loadedCards))
                {
                    mainWinRef.CollectCards();
                    mainWinRef.CleanMemory();
                    mainWinRef.PlayPreppedHand(loadedCards);
                    //mainWinRef.GivePreppedHands(loadedCards, mainWinRef.WistPlayerNumber);
                }
                else
                    MessageBox.Show("Improper card format in loaded file.", "Cannot load prepared hands");
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not read the file:");
                Console.WriteLine(e.Message);
            }
        }

        //pamietac o wistujacym
        private bool ParseFile(string[] dataFromFile, out List<Card> outParsedCards)
        {
            try
            {
                List<Card> parsedCards = new List<Card>();
                CardRank cardRank = new CardRank();
                CardSuit parsedCardSuit = new CardSuit();

                for (int i = 0; i < 4; i++)
                {
                    string[] playerCards = dataFromFile[i].Split(' ');
                    for (int j = 0; j < playerCards.Length; j++)
                    {
                        if (playerCards[j].Length == 2 &&
                             CardEnumParser.Parse(playerCards[j][0].ToString(), out cardRank) &&
                             CardEnumParser.Parse(playerCards[j][1].ToString(), out parsedCardSuit))
                        {
                            Card readCard = new Card((CardRank)cardRank, parsedCardSuit);
                            parsedCards.Add(readCard);
                        }
                        else if (playerCards[j].Length == 3 &&
                                 CardEnumParser.Parse(playerCards[j].Substring(0, 2).ToString(), out cardRank) &&
                                 CardEnumParser.Parse(playerCards[j][2].ToString(), out parsedCardSuit))
                        {
                            Card readCard = new Card((CardRank)cardRank, parsedCardSuit);
                            parsedCards.Add(readCard);
                        }
                        else
                        {
                            throw new ArgumentNullException("Could not parse correctly - input file format may be wrong.");
                        }
                    }
                }
                mainWinRef.WistPlayerNumber = Convert.ToInt32(dataFromFile[4]); //z ostatniej linii odczytaj wistujacego
                outParsedCards = parsedCards;
                return true;
            }
            catch
            {
                outParsedCards = new List<Card>();
                return false;
            }
        }

        private void MenuItemLoadHand_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            //szukaj .txt
            dlg.DefaultExt = ".txt";
            dlg.Filter = "Text Files (*.txt)|*.txt";

            //wyswietl okienko do ladowania 
            dlg.Title = "Select prepared hand";
            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                //pobierz sciezke
                string filename = dlg.FileName;
                LoadFile(filename);
                //List<Card> prepHand = parse(filename) <- to trzeba dopisac
            }
        }

        private void SaveCards(List<Card> playedCards, string fileName)
        {
            StringBuilder resultStrings = new StringBuilder();
            int howMany = playedCards.Count / 4;

            //zapisz algorytm kazdego gracza

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < howMany ; j++)
                {
                    resultStrings.Append(playedCards[j + i * howMany].ToString());
                    resultStrings.Append(" ");
                }
                resultStrings.Remove(resultStrings.Length - 1, 1);
                resultStrings.AppendLine();
            }

            int index = mainWinRef.DealerNumber % 4;

            resultStrings.Append(index + 1);

            using (StreamWriter outfile = new StreamWriter(fileName, false, Encoding.Unicode))
            {
                outfile.Write(resultStrings.ToString());
            }
        }
    
        /// <summary>
        /// Trzeba tego uzyc przed zebraniem ostatniej lewy, tylko wtedy dziala.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemSaveHand_Click(object sender, RoutedEventArgs e)
        {
            List<Card> playedCards = mainWinRef.MakeListCardFromHands();

            int playedCardSize = playedCards.Count;
            if (playedCardSize <= 0 || playedCardSize % 4 != 0)
            {
                MessageBox.Show("There are no cards to be saved or the number cannot be divided by 4.", "Cannot save cards");
                return;
            }

            SaveFileDialog _SD = new SaveFileDialog();
            _SD.Filter = "Text File (*.txt)|*.txt|Show All Files (*.*)|*.*";
            _SD.FileName = "cards" + DateTime.Now.Date.ToString();
            _SD.FileName = _SD.FileName.Remove(_SD.FileName.Length - 9);    //-9 bo tyle znakow uzywa na godzine
            _SD.Title = "Save cards as";
            if (_SD.ShowDialog() == true)
            {
                //Console.WriteLine("udalo sie zapisac");
                SaveCards(playedCards, _SD.FileName);
            }
        }

        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            int exitCode = 0;
            Environment.Exit(exitCode);
        }
        #endregion

        #region Changing AI algorithms
        private void radioPlayer1Random_Checked(object sender, RoutedEventArgs e)
        {
            mainWinRef.PlayersAlgorithms[0] = AlgorithmTypes.Random;
        }

        private void radioPlayer1Greedy_Checked(object sender, RoutedEventArgs e)
        {
            mainWinRef.PlayersAlgorithms[0] = AlgorithmTypes.Greedy;
        }

        private void radioPlayer1Heuristic_Checked(object sender, RoutedEventArgs e)
        {
            mainWinRef.PlayersAlgorithms[0] = AlgorithmTypes.Heuristic;
        }

        private void radioPlayer2Random_Checked(object sender, RoutedEventArgs e)
        {
            mainWinRef.PlayersAlgorithms[1] = AlgorithmTypes.Random;
        }

        private void radioPlayer2Greedy_Checked(object sender, RoutedEventArgs e)
        {
            mainWinRef.PlayersAlgorithms[1] = AlgorithmTypes.Greedy;
        }

        private void radioPlayer2Heuristic_Checked(object sender, RoutedEventArgs e)
        {
            mainWinRef.PlayersAlgorithms[1] = AlgorithmTypes.Heuristic;
        }

        private void radioPlayer3Random_Checked(object sender, RoutedEventArgs e)
        {
            mainWinRef.PlayersAlgorithms[2] = AlgorithmTypes.Random;
        }

        private void radioPlayer3Greedy_Checked(object sender, RoutedEventArgs e)
        {
            mainWinRef.PlayersAlgorithms[2] = AlgorithmTypes.Greedy;
        }

        private void radioPlayer3Heuristic_Checked(object sender, RoutedEventArgs e)
        {
            mainWinRef.PlayersAlgorithms[2] = AlgorithmTypes.Heuristic;
        }

        private void radioPlayer4Random_Checked(object sender, RoutedEventArgs e)
        {
            mainWinRef.HumanPlayer = false;
            mainWinRef.PlayersAlgorithms[3] = AlgorithmTypes.Random;
        }

        private void radioPlayer4Greedy_Checked(object sender, RoutedEventArgs e)
        {
            mainWinRef.HumanPlayer = false;
            mainWinRef.PlayersAlgorithms[3] = AlgorithmTypes.Greedy;
        }

        private void radioPlayer4Heuristic_Checked(object sender, RoutedEventArgs e)
        {
            mainWinRef.HumanPlayer = false;
            mainWinRef.PlayersAlgorithms[3] = AlgorithmTypes.Heuristic;
        }
        #endregion



        #region not used region
        #endregion

    }
}
