using CreateCalendar.ProcessXlCalendar;
using Microsoft.SharePoint.Client;
using System;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

namespace CreateCalendar
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            const string defaultUser = "brent.mcsharry@health.qld.gov.au";
            Console.WriteLine($"Username ([enter] for ${defaultUser}):");
            var userName = Console.ReadLine();
            if (string.IsNullOrEmpty( userName ) ) { userName = defaultUser; }
            Console.WriteLine($"Password for {userName}:");
            // var pwd = GetSecureString();
            var pwd = new SecureString();
            foreach (var c in "Galaxy13!") pwd.AppendChar( c );
            using (var ctx = new ClientContext("https://healthqld.sharepoint.com/teams/ICUPCCULeave/Shared%20Documents/Forms"))
            {
                ctx.Credentials = new SharePointOnlineCredentials(userName, pwd);
                var file = ctx.Web.GetFileByServerRelativeUrl("teams/ICUPCCULeave/Shared%20Documents/PCCU%20SMO/PCCU%20SMO%20Roster.xlsx");
                ClientResult<Stream> data = file.OpenBinaryStream();
                ctx.Load(file);
                await ctx.ExecuteQueryAsync();

                var roster = ReadXlData.Process(data.Value);
                Console.WriteLine(roster.Employees.First());
            }
            Console.ReadLine();
        }

        static SecureString GetSecureString()
        {
            ConsoleKeyInfo info;

            var returnVar = new SecureString();
            do
            {
                info = Console.ReadKey(true);
                if (info.Key == ConsoleKey.Backspace)
                {
                    if (returnVar.Length > 0)
                    {
                        returnVar.RemoveAt(returnVar.Length - 1);
                        Console.Write('\b');
                    }
                }
                else if (info.Key != ConsoleKey.Enter)
                {
                    Console.Write('*');
                    returnVar.AppendChar(info.KeyChar);
                }
            } while (info.Key != ConsoleKey.Enter);
            Console.WriteLine();
            returnVar.MakeReadOnly();
            return returnVar;
        }
    }
}
