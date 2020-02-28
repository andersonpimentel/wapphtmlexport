using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace wapphtmlexport
{
    public class Parser
    {
        private static readonly Regex mainFilePattern = new Regex(@"(\w*\s)*whatsapp(\s\w*)*\.txt", RegexOptions.IgnoreCase);
        private static readonly Regex linePattern = new Regex(@"(?<datetime>\d{2}\/\d{2}\/\d{4} \d{2}:\d{2}) - (?<author>[^:]+): (?<content>.*)$", RegexOptions.IgnoreCase);
        private readonly DirectoryInfo baseDirectory;
        private FileInfo mainFile;
        private Message[] messages;
        private string[] authors;

        public Parser(string baseFolder)
        {
            baseDirectory = new DirectoryInfo(baseFolder);

            if (!baseDirectory.Exists)
            {
                throw new DirectoryNotFoundException();
            }
        }

        public FileInfo MainFile
        {
            get
            {
                if (mainFile == null)
                {
                    mainFile = baseDirectory.EnumerateFiles().FirstOrDefault(f => mainFilePattern.IsMatch(f.Name));
                }

                return mainFile;
            }
        }

        public Message[] Messages
        {
            get
            {
                if (messages == null)
                {
                    if (MainFile == null)
                    {
                        throw new ArgumentNullException(nameof(MainFile));
                    }

                    var output = new List<Message>();

                    foreach (var line in File.ReadAllLines(mainFile.FullName))
                    {
                        var match = linePattern.Match(line);

                        if (match.Success)
                        {
                            var message = new Message(DateTime.Parse(match.Groups["datetime"].Value),
                                                      match.Groups["author"].Value,
                                                      match.Groups["content"].Value);

                            output.Add(message);
                        }
                        else if (output.Any())
                        {
                            output.Last().Content += "\r\n" + line;
                        }
                    }

                    messages = output.ToArray();
                }

                return messages;
            }
        }

        public string[] Authors
        {
            get
            {
                if (authors == null)
                {
                    authors =  Messages.Select(m => m.Author).Distinct().OrderBy(a => a).ToArray();
                }
                
                return authors;
            }
        }
    }
}