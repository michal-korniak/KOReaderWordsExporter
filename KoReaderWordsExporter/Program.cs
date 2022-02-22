using KoReaderWordsExporter;
using Microsoft.Extensions.Configuration;
using Renci.SshNet;
using System.Text.RegularExpressions;

namespace MyApp
{
    public class Program
    {
        static void Main()
        {
            var config = GetConfiguration();
            Console.WriteLine("Downloading file...");
            string fileContent = DownloadFile(config);
            Console.WriteLine("Parsing...");
            var wordEntries = MapFileContentToWordEntries(fileContent);
            var cleanedWordEntries = CleanWordEntries(wordEntries);
            Console.WriteLine("Exporting to CSV...");
            ExportWordEntriesToCsv(cleanedWordEntries, config.OutputDirectory);
            Console.WriteLine("Task finished.");
            Console.ReadKey();
        }

        static Config GetConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.json", true, true);
            var configurationRoot = builder.Build();

            var config = new Config(
                host: configurationRoot["Host"],
                userName: configurationRoot["Username"],
                password: configurationRoot["Password"],
                filePath: configurationRoot["FilePath"],
                outputDirectory: configurationRoot["OutputDirectory"],
                port: int.Parse(configurationRoot["Port"])
            );

            return config;
        }

        static string DownloadFile(Config config)
        {
            using SftpClient sftp = new SftpClient(config.Host, config.Port, config.UserName, config.Password);
            sftp.Connect();
            using MemoryStream fileMemoryStream = new MemoryStream();
            sftp.DownloadFile(config.FilePath, fileMemoryStream);
            byte[] fileBytes = fileMemoryStream.ToArray();
            string content = System.Text.Encoding.UTF8.GetString(fileBytes);

            return content;
        }

        static IEnumerable<WordEntry> MapFileContentToWordEntries(string fileContent)
        {
            var wordEntries = new List<WordEntry>();
            string wordsEntriesPattern = @"\[\d+]. *=. *{([\S\s]*?)}";
            var wordEntriesMatches = Regex.Matches(fileContent, wordsEntriesPattern);
            foreach (var wordEntryMatch in wordEntriesMatches.ToArray())
            {
                var wordEntryMatchValue = wordEntryMatch.Value;

                var wordValue = Regex.Match(wordEntryMatchValue, @"\[""word""\] *= *""([\S\s]*?)""").Groups[1].Value;
                var epochDateValue = Regex.Match(wordEntryMatchValue, @"\[""time""\] *= *(.*?),").Groups[1].Value;
                var bookTitleValue = Regex.Match(wordEntryMatchValue, @"\[""book_title""\] *= *""([\S\s]*?)""").Groups[1].Value;

                var wordEntry = new WordEntry()
                {
                    BookTitle = bookTitleValue,
                    Word = wordValue,
                    Date = DateTimeOffset.FromUnixTimeSeconds(int.Parse(epochDateValue)).LocalDateTime
                };
                wordEntries.Add(wordEntry);
            }

            return wordEntries;

        }

        static IEnumerable<WordEntry> CleanWordEntries(IEnumerable<WordEntry> wordEntries)
        {
            var cleanedWordEntries = wordEntries
             .OrderByDescending(x => x.Date)
             .GroupBy(wordEntry => wordEntry.Word)
             .Select(group => group.First())
             .Select(wordEntry => new WordEntry()
             {
                 Word = wordEntry.Word.RemoveNewLinesCharacters().Replace("↑", ""),
                 BookTitle = wordEntry.BookTitle,
                 Date = wordEntry.Date
             })
             .ToArray();


            return cleanedWordEntries;
        }

        static void ExportWordEntriesToCsv(IEnumerable<WordEntry> wordEntries, string directoryPath)
        {
            string fileName = DateTime.Now.ToString("yyyy-MM-dd_HH-mm") + ".csv";
            string filePath = Path.Combine(directoryPath, fileName);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            using StreamWriter streamWriter = File.CreateText(filePath);
            foreach (var wordEntry in wordEntries)
            {
                streamWriter.WriteLine(String.Join(',', wordEntry.Date, wordEntry.BookTitle, wordEntry.Word));
            }
        }

    }
}