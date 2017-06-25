using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Linq;

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
			return "No results found...";

		}


		public void DownloadNewWowHead()
		{
			WebClient webClient = new WebClient();
			webClient.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
			string htmlCode = webClient.DownloadString("https://wowhead.com");
			System.IO.File.WriteAllText(@"test.txt", htmlCode);
		}

		public void DownloadNewMageTower()
		{
			WebClient webClient = new WebClient();
			webClient.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
			string htmlCode = webClient.DownloadString("https://data.magetower.info");
			System.IO.File.WriteAllText(@"magetower.txt", htmlCode);
		}

	}
}
