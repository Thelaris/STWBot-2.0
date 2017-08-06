using System;
using System.Collections.Generic;
using System.IO;
using System.Data.SQLite;
using System.Net;
using System.Linq;
using System.Net.Mail;
using log4net;

namespace STWBot_2
{
	public class Utilities
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public Utilities()
		{
		}

		public DateTime ConvertUnixEpochTime(long seconds)
		{
			long convertedSeconds = Convert.ToInt64(seconds);
			DateTime time = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

			return time.AddSeconds(seconds).ToLocalTime();
		}

		public string GetLine(string keyword, string file)
		{
			string[] textLines = File.ReadAllLines(file);
			List<string> results = new List<string>();

			int i = 0;

			foreach (string line in textLines)
			{
				if (line.Contains(keyword))
				{
					results.Add(line);
					return line;
				}
			}

			/*
			foreach (string l in results)
			{
				//Console.WriteLine(l);
				return l;
			}
			*/
			//if (results.Count < 1)
			return "No results found...";

		}

		public string GetLineAfterNum(string keyword, string file, int linesAfter)
		{
			string[] textLines = File.ReadAllLines(file);
			List<string> results = new List<string>();

			int i = 0;

			foreach (string line in textLines)
			{
				if (line.Contains(keyword))
				{
					results.Add(line);
					int lineNumber = i;
					break;
				}

				i++;
			}

			return textLines[i + linesAfter];
		}

		public List<string> ReturnAllLines(string keyword, string file)
		{
			string[] textLines = File.ReadAllLines(file);
			List<string> results = new List<string>();

			int i = 0;

			foreach (string line in textLines)
			{
				if (line.Contains(keyword))
				{
					results.Add(line);
				}
			}

			return results;
		}


		public string BrokenShoreBuildingGetLine(string firstKeyword, string secondKeyword, string file)
		{
			string[] textLines = File.ReadAllLines(file);
			//List<string> textLines = File.ReadLines(file).ToList();
			//List<string> results = new List<string>();

			int i = 0;
			int lineNum = 0;

			foreach (string line in textLines)
			{
				if (line.Contains(firstKeyword))
				{
					//results.Add(line);
					lineNum += i;
					break;
				}

				i++;
			}

			string[] remainingLines = File.ReadAllLines(file).Skip(lineNum).ToArray();

			foreach (string line in remainingLines)
			{
				if (line.Contains(secondKeyword))
				{
					return line;
				}
			}


			/*
			foreach (string l in results)
			{
				//Console.WriteLine(l);
				return l;
			}
			*/
			//if (results.Count < 1)
			return "";

		}


		public void DownloadNewWowHead()
		{
			WebClient webClient = new WebClient();
			webClient.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
			try
			{
				log.Debug("Downloading HTML source from WoWHead.com");
				string htmlCode = webClient.DownloadString("https://wowhead.com");
				log.Debug("WoWHead.com HTML source downloaded!");
				log.Debug("Writing WoWHead.com HTML source to file");
				System.IO.File.WriteAllText(@"test.txt", htmlCode);
				log.Debug("Completed writing WoWHead.com HTML source to file!");
			}
			catch (Exception ex)
			{
				log.Fatal("WoWHead html source download failed: " + ex);
			}
		}

		public void DownloadNewMageTower()
		{
			//WebClient webClient = new WebClient();
			//webClient.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
			//string htmlCode = webClient.DownloadString("https://data.magetower.info");
			//System.IO.File.WriteAllText(@"magetower.txt", htmlCode);
		}

		public void SendEmail(string FROM, string TO, string subject, string body)
		{
			SQLiteConnection m_dbConnection;

			m_dbConnection = new SQLiteConnection(@"Data Source=DB\bot.sqlite;Version=3;"); //Make global
			m_dbConnection.Open();

			string sql; //make global
			SQLiteCommand command; //make global

			List<string> dbResults = new List<string>();

			sql = "SELECT smtpserver, smtpport, smtpssl, smtptimeout, emailfrom, emailto, smtpusername, smtppassword FROM systempreferences";
			command = new SQLiteCommand(sql, m_dbConnection);

			SQLiteDataReader reader = command.ExecuteReader();

			while (reader.Read())
			{
				dbResults.Add(reader["smtpserver"].ToString());
				dbResults.Add(reader["smtpport"].ToString());
				dbResults.Add(reader["smtpssl"].ToString());
				dbResults.Add(reader["smtptimeout"].ToString());
				dbResults.Add(reader["emailfrom"].ToString());
				dbResults.Add(reader["emailto"].ToString());
				dbResults.Add(reader["smtpusername"].ToString());
				dbResults.Add(reader["smtppassword"].ToString());
			}

			string smtpServer = dbResults[0];
			int smtpPort = Convert.ToInt32(dbResults[1]);
			bool smtpSSL;
			if (dbResults[2] == "true")
			{
				smtpSSL = true;
			}
			else
			{
				smtpSSL = false;
			}
			int smtpTimeout = Convert.ToInt32(dbResults[3]);
			string emailFrom = dbResults[4];
			string emailTo = dbResults[5];
			string smtpUsername = dbResults[6];
			string smtpPassword = dbResults[7];

			m_dbConnection.Close();

			MailMessage mail = new MailMessage(emailFrom, emailTo); //FROM, TO
			SmtpClient client = new SmtpClient();
			client.Port = smtpPort;
			client.EnableSsl = smtpSSL;
			client.Timeout = smtpTimeout;
			client.DeliveryMethod = SmtpDeliveryMethod.Network;
			client.UseDefaultCredentials = false;
			client.Credentials = new System.Net.NetworkCredential(smtpUsername, smtpPassword);
			client.Host = smtpServer;
			mail.Subject = subject;
			mail.Body = body;
			client.Send(mail);
		}

	}
}
