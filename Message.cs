using System;
using System.Text.RegularExpressions;

namespace wapphtmlexport
{
    public class Message
    {
        private static readonly Regex filePattern = new Regex(@"(?<filename>[\w-\.]+) \(arquivo anexado\)$", RegexOptions.IgnoreCase);

        public Message(DateTime dateTime, string author, string content)
        {
            DateTime = dateTime;
            Author = author;
            Content = content;
            var fileMatch = filePattern.Match(content);
            if (fileMatch.Success)
            {
                File = fileMatch.Groups["filename"].Value;
            }
        }

        public DateTime DateTime { get; set; }
        public string Author { get; set; }
        public string Content { get; set; }
        public string File { get; set; }
        public bool IsFile 
        {
            get
            {
                return File != null;
            }
        }
    }
}