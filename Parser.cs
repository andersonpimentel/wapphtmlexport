using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace wapphtmlexport
{
    public class Parser
    {
        private static readonly Regex mainFilePattern = new Regex(@"(\w*\s)*whatsapp(\s\w*)*\.txt", RegexOptions.IgnoreCase);
        private static readonly Regex linePattern = new Regex(@"(?<datetime>\d{2}\/\d{2}\/\d{4} \d{2}:\d{2}) - (?<author>[^:]+): (?<content>.*)$", RegexOptions.IgnoreCase);
        private const string hiddenMediaFile = "<Arquivo de mídia oculto>";
        private const string escapedHiddenMediaFile = "&lt;Arquivo de mídia oculto&gt;";
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
                            output.Last().Content += "\n" + line;
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
                    authors = Messages.Select(m => m.Author).Distinct().OrderBy(a => a).ToArray();
                }

                return authors;
            }
        }

        public string ExportHtml()
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta charset='UTF-8'>");
            sb.AppendLine("<meta name='viewport' content='width=device-width, initial-scale=1'>");
            sb.AppendFormat("<title>{0}</title>", MainFile.Name.Replace(".txt", string.Empty));
            sb.AppendLine("<link rel='stylesheet' type='text/css' href='theme.css'>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");

            foreach (var message in Messages)
            {
                sb.AppendLine("<div class='message'>");
                sb.AppendFormat("<div class='messageheader author-{0}'>", Array.IndexOf(Authors, message.Author) + 1);
                sb.AppendFormat("<div class='author'>{0}</div>", message.Author);
                sb.AppendFormat("<div class='datetime'>{0:dd/MM/yyyy HH:mm}</div>", message.DateTime);
                sb.AppendLine("</div>");
                sb.AppendLine("<div class='messagecontent'>");
                if (message.IsFile)
                {
                    if (message.File.EndsWith(".opus") || message.File.EndsWith(".mp3"))
                    {
                        sb.AppendFormat("<audio controls class='file audio' src='{0}'></audio>", message.File);
                    }
                    else if (message.File.EndsWith(".jpg") || message.File.EndsWith(".jpeg"))
                    {
                        sb.AppendFormat("<img class='file image' src='{0}' />", message.File);
                    }
                    else if (message.File.EndsWith(".avi"))
                    {
                        sb.AppendFormat("<video controls class='file video' src='{0}'></video>", message.File);
                    }
                    else
                    {
                        sb.AppendFormat("<div class='file other'><a href='{0}' target='_blank'>{0}</a></div>", message.File);
                    }
                }
                else
                {
                    var content = message.Content.Equals(hiddenMediaFile) ?
                                  escapedHiddenMediaFile :
                                  message.Content.Replace("\r", string.Empty).Replace("\n", "<br>");

                    sb.AppendFormat("<div class='content'>{0}</div>", content);
                }
                sb.AppendLine("</div>");
                sb.AppendLine("</div>");
            }

            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }
    }
}