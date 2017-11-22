using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using NLog;

namespace LogImporter
{
    public static class Repository
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private const string TABLE_CREATE_SQL = @"
            IF NOT EXISTS(
	            SELECT * FROM INFORMATION_SCHEMA.TABLES
	            WHERE TABLE_NAME = 'Logs'
            )
            BEGIN

	            CREATE TABLE [dbo].[Logs](
		            [LogId] [int] IDENTITY(1,1) NOT NULL,
		            [Application] [varchar](50) NOT NULL,
		            [FileName] [varchar](100) NOT NULL,
		            [CanParse] [bit] NOT NULL,
		            [Date] [date] NULL,
		            [Time] [varchar](8) NULL,
		            [Ip] [varchar](15) NULL,
		            [Method] [varchar](10) NULL,
		            [UriStem] [varchar](700) NULL,
		            [UriQuery] [varchar](700) NULL,
		            [Port] [int] NULL,
		            [ClientUsername] [varchar](100) NULL,
		            [ClientIp] [varchar](15) NULL,
		            [ClientUserAgent] [varchar](200) NULL,
		            [ClientReferer] [varchar](700) NULL,
		            [Status] [int] NULL,
		            [Substatus] [int] NULL,
		            [Win32Status] [int] NULL,
		            [TimeTaken] [int] NULL,
	             CONSTRAINT [PK_Logs] PRIMARY KEY CLUSTERED 
	            (
		            [LogId] ASC
	            )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	            ) ON [PRIMARY]

            END";

        private const string INSERT_LOG_SQL = @"
            INSERT INTO [dbo].[Logs]
                   ([Application]
                   ,[FileName]
                   ,[CanParse]
                   ,[Date]
                   ,[Time]
                   ,[Ip]
                   ,[Method]
                   ,[UriStem]
                   ,[UriQuery]
                   ,[Port]
                   ,[ClientUsername]
                   ,[ClientIp]
                   ,[ClientUserAgent]
                   ,[ClientReferer]
                   ,[Status]
                   ,[Substatus]
                   ,[Win32Status]
                   ,[TimeTaken])
             VALUES
                   (@Application
                   ,@FileName
                   ,@CanParse
                   ,@Date
                   ,@Time
                   ,@Ip
                   ,@Method
                   ,@UriStem
                   ,@UriQuery
                   ,@Port
                   ,@ClientUsername
                   ,@ClientIp
                   ,@ClientUserAgent
                   ,@ClientReferer
                   ,@Status
                   ,@Substatus
                   ,@Win32Status
                   ,@TimeTaken)";

        public static void EnsureDatabaseTable()
        {
            using (var db = GetConnection())
            {
                db.Open();
                db.Execute(TABLE_CREATE_SQL);
                db.Close();
            }
        }

        public static async Task AddLog(LogFileLine log)
        {
            try
            {
                using (var db = GetConnection())
                {
                    db.Open();
                    await db.ExecuteAsync(INSERT_LOG_SQL, log);
                    db.Close();
                }
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }
        }

        private static SqlConnection GetConnection()
        {
            return new SqlConnection(ConfigurationManager.ConnectionStrings["Logs"].ConnectionString);
        }
    }
}