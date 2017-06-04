using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace STWBot_2
{
	public class Utilities
	{
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

		public void DownloadNewWowHead()
		{
			WebClient webClient = new WebClient();
			string htmlCode = webClient.DownloadString("http://wowhead.com");
			System.IO.File.WriteAllText(@"test.txt", htmlCode);
		}
	}
}
