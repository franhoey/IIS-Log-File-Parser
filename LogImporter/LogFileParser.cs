using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;

namespace LogImporter
{
    public static class LogFileParser
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private static readonly Dictionary<string, Action<LogFileLine, string>> FieldMapping = new Dictionary<string, Action<LogFileLine, string>>()
        {
            { "date", (l, v) => l.Date = DateTime.Parse(v) },
            { "time", (l, v) => l.Time = v  },
            { "s-ip", (l, v) => l.Ip = v  },
            { "cs-method", (l, v) => l.Method = v  },
            { "cs-uri-stem", (l, v) => l.UriStem = v  },
            { "cs-uri-query", (l, v) => l.UriQuery = v  },
            { "s-port", (l, v) => l.Port = int.Parse(v)  },
            { "cs-username", (l, v) => l.ClientUsername = v  },
            { "c-ip", (l, v) => l.ClientIp = v  },
            { "cs(User-Agent)", (l, v) => l.ClientUserAgent = v  },
            { "cs(Referer)", (l, v) => l.ClientReferer = v  },
            { "sc-status", (l, v) => l.Status = int.Parse(v)  },
            { "sc-substatus", (l, v) => l.Substatus = int.Parse(v)  },
            { "sc-win32-status", (l, v) => l.Win32Status = int.Parse(v)  },
            { "time-taken", (l, v) => l.TimeTaken = int.Parse(v)  }
        };
            
        public static IEnumerable<LogFileLine> ParseLogFile(string applicationName, string filePath)
        {
            Console.WriteLine($"Begin processing file {filePath}");
            var lines = File.ReadAllLines(filePath);
            string[] fields = null;
            bool cannotParseFile = false;
            foreach (var line in lines)
            {
                if (line.StartsWith("#Fields"))
                {
                    fields = ParseFields(line);
                    if (fields.Except(FieldMapping.Keys).Any())
                    {
                        cannotParseFile = true;
                        break;
                    }
                }
                else if (!line.StartsWith("#"))
                {
                    if (fields == null)
                    {
                        cannotParseFile = true;
                        break;
                    }

                    yield return ParseLog(applicationName, filePath, line, fields);
                }
            }

            if (cannotParseFile)
                yield return new LogFileLine()
                {
                    Application = applicationName,
                    FileName = filePath,
                    CanParse = false
                };
        }

        private static string[] ParseFields(string fieldsLine)
        {
            return fieldsLine.Substring(9).Split(' ');
        }

        private static LogFileLine ParseLog(string applicationName, string filePath, string logLine, string[] fields)
        {
            try
            {
                var log = new LogFileLine()
                {
                    Application = applicationName,
                    CanParse = true,
                    FileName = filePath
                };

                var values = logLine.Split(' ');

                for (int i = 0; i < fields.Length; i++)
                {
                    var mappingAction = FieldMapping[fields[i]];
                    if (mappingAction == null)
                    {
                        log.CanParse = false;
                    }
                    else
                    {
                        try
                        {
                            mappingAction(log, values[i]);
                        }
                        catch (Exception e)
                        {
                            log.CanParse = false;
                        }
                    }
                }

                return log;
            }
            catch (Exception e)
            {
                _logger.Error(e);
                return new LogFileLine()
                {
                    Application = applicationName,
                    FileName = filePath,
                    CanParse = false
                };
            }
        }

        
    }
}