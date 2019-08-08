using ConsoleTables;
using System;
using System.Collections.Generic;
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
                Console.WriteLine("Press any key to exit...");
                Console.ReadLine();
            }

            // If only want to get commit count stats, uncomment the following
            /*
            var tfsWork = new TFSWork(tfsUrl, personalaccesstoken, true);

            if (tfsWork != null)
            {
                ConsoleTable.From<UserCommitStat>(tfsWork.userCommitStats).Write();
                Console.WriteLine("Press any key to exit...");
                Console.ReadLine();
            }
            */
        }
    }
}
