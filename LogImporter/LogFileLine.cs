using System;

namespace LogImporter
{
    public class LogFileLine
    {   
        public string Application { get; set; }
        public string FileName { get; set; }
        public bool CanParse { get; set; }
        public DateTime Date { get; set; }
        public string Time { get; set; }
        public string Ip { get; set; }
        public string Method { get; set; }
        public string UriStem { get; set; }
        public string UriQuery { get; set; }
        public int Port { get; set; }
        public string ClientUsername { get; set; }
        public string ClientIp { get; set; }
        public string ClientUserAgent { get; set; }
        public string ClientReferer { get; set; }
        public int Status { get; set; }
        public int Substatus { get; set; }
        public int Win32Status { get; set; }
        public int TimeTaken { get; set; }
    }
}