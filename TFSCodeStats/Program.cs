using ConsoleTables;
using ServiceStack.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFSCodeStats.Models;

namespace TFSCodeStats
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.SetWindowSize(Console.WindowWidth * 2, Console.WindowHeight);

            // TODO: Update these values as needed
            const string tfsUrl = "YOURBASEURL, for example https://accountname.visualstudio.com/";
            const string personalaccesstoken = "YOURPERSONALACCESSTOKEN";

            var tfsWork = new TFSWork(tfsUrl, personalaccesstoken);

            if (tfsWork != null)
            {
                ConsoleTable.From<UserStat>(tfsWork.userStats).Write();

                // Write results to a csv file
                WriteToCsv(tfsWork.userStats);

                Console.WriteLine("Press any key to exit...");
                Console.ReadLine();
            }

            // If only want to get commit count stats, uncomment the following
            /*
            var tfsWork = new TFSWork(tfsUrl, personalaccesstoken, true);

            if (tfsWork != null)
            {
                ConsoleTable.From<UserCommitStat>(tfsWork.userCommitStats).Write();

                // Write results to a csv file
                WriteToCsv(tfsWork.userCommitStats);

                Console.WriteLine("Press any key to exit...");
                Console.ReadLine();
            }
            */
        }

        private static void WriteToCsv(List<UserCommitStat> ucS)
        {
            TextWriter twa = new StreamWriter("usercommitstats.csv", true);
            CsvSerializer.SerializeToWriter<List<UserCommitStat>>(ucS, twa);
            twa.Close();
        }

        private static void WriteToCsv(List<UserStat> uS)
        {
            TextWriter twa = new StreamWriter("userstats.csv", true);
            CsvSerializer.SerializeToWriter<List<UserStat>>(uS, twa);
            twa.Close();
        }
    }
}
