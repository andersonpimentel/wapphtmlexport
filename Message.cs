using System;

namespace wapphtmlexport
{
    public class Message
    {
        public Message(DateTime dateTime, string author, string content)
        {
            DateTime = dateTime;
            Author = author;
            Content = content;
        }

        public DateTime DateTime { get; set; }
        public string Author { get; set; }
        public string Content { get; set; }
    }
}