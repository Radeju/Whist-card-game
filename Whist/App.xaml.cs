using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace Whist
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        void App_Startup(object sender, StartupEventArgs e)
        {

            Whist.MainWindow mainW = new MainWindow();
            ScoreAndInfo scores = new ScoreAndInfo(mainW);

            //scores.MainWinRef = mainW;
            mainW.ScoreWinRef = scores;

            scores.Show();
            mainW.Show();

            mainW.Closed += new EventHandler(mainW_Closed);
            scores.Closed += new EventHandler(scores_Closed);
        }

        //zamknij wszystkie okienka
        void scores_Closed(object sender, EventArgs e)
        {
            int exitCode = 2;
            Environment.Exit(exitCode);
        }

        //zamknij wszystkie okienka
        void mainW_Closed(object sender, EventArgs e)
        {
            int exitCode = 1;
            Environment.Exit(exitCode);
        }

    }
}
