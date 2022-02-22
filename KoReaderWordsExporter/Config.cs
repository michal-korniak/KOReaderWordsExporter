using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoReaderWordsExporter
{
    public class Config
    {
        public string Host { get; private init; }
        public string UserName { get; private init; }
        public string Password { get; private init; }
        public string FilePath { get; private init; }
        public string OutputDirectory { get; private init; }
        public int Port { get; private init; }

        public Config(string host, string userName, string password, string filePath, string outputDirectory, int port)
        {
            Host = host;
            UserName = userName;
            Password = password;
            FilePath = filePath;
            OutputDirectory = outputDirectory;
            Port = port;
        }
    }
}
