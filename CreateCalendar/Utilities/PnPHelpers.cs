using PnP.Core.Model.SharePoint;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CreateCalendar
{
    internal static class PnPHelpers
    {
        public static async Task CreateCalendar(PnPContext context, string serverUrl, string fileName, Stream contents)
        {
            var folder = await context.Web.GetFolderByServerRelativeUrlAsync(serverUrl);
            await folder.Files.AddAsync(fileName, contents, true);
        }
        public static async Task<IEnumerable<IFile>> UseQueryToGetAllDocs(PnPContext context)
        {
            var uriLength = context.Uri.ToString().Length;
            // Get a reference to a list
            IList documentsList = await context.Web.Lists.GetByTitleAsync("Documents");

            // Get files from the list whose name contains "foo"
            List<IFile> foundFiles = await documentsList.FindFilesAsync("*Roster*.xlsx");
            if (foundFiles.Count == 0)
            {
                Console.WriteLine("No files found");
            }
            else
            {
                Console.WriteLine("Please select from the following files:");
                for (int i = 0; i < foundFiles.Count; ++i)
                {
                    Console.WriteLine($"[{i + 1}] {foundFiles[i].LinkingUrl.Substring(uriLength)}");
                }
                Console.WriteLine("Enter [A] for All, or the number correspoding to the file. Anything else will cancel");
                var answer = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(answer))
                {
                    answer = answer.Trim();
                    if (answer[0] == 'A' || answer[0] == 'a')
                    {
                        return foundFiles;
                    }
                    if (int.TryParse(answer, out int no))
                    {
                        --no;
                        if (no >= 0 && no < foundFiles.Count)
                        {
                            return new[] { foundFiles[no] };
                        }
                        Console.WriteLine("Not a valid number");
                    }
                }
            }
            return Enumerable.Empty<IFile>();
        }
    }
}
