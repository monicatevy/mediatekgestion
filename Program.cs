using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Mediatek86.controleur;
using Serilog;

namespace Mediatek86
{
    static class Program
    {
        const string DOSSIERLOGS = "C:\\Users\\Monica Tevy Sen\\Desktop\\Atelier C#\\logs";

        /// <summary>
        /// Point d'entrée principal de l'application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File(DOSSIERLOGS+"\\log.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            new Controle();
        }
    }
}
