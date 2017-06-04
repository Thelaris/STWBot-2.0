using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Reflection;
using System.Net;
using System.IO;
using System.Linq;
using Discord;
using Discord.WebSocket;
using Discord.Net.Providers.WS4Net;
using Discord.Commands;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace STWBot_2
{
	class MainClass
	{
		private CommandService commands;
		private DiscordSocketClient client;

		private Token tokenRef = new Token();
		//private IServiceProvider map;
		//private Discord map;
		public IServiceProvider map;

		public static void Main(string[] args) => new MainClass().MainAsync().GetAwaiter().GetResult();

		public async Task MainAsync()
		{
			client = new DiscordSocketClient(new DiscordSocketConfig
			{
				WebSocketProvider = WS4NetProvider.Instance,
			});
			commands = new CommandService();

			map = null;

			await InstallCommands();
			//await CheckInvasionTimes();



			client.Log += Log;
			//client.MessageReceived += MessageReceived;

			await client.LoginAsync(TokenType.Bot, tokenRef.token);
			await client.StartAsync();

			// Block this task until the program is closed (infinite delay)
			await Task.Delay(-1);
		}

		public async Task InstallCommands()
		{
			//Hook the MessageReceived Event into our Command Handler
			client.MessageReceived += HandleCommand;


			//await commands.AddModuleAsync<Info>();
			//await commands.AddModuleAsync<Sample>();

			await commands.AddModulesAsync(Assembly.GetEntryAssembly());
		}

		public async Task HandleCommand(SocketMessage messageParam)
		{
			var message = messageParam as SocketUserMessage;
			if (message == null) return;

			int argPos = 0;

			if (!(message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(client.CurrentUser, ref argPos))) return;

			var context = new CommandContext(client, message);

			var result = await commands.ExecuteAsync(context, argPos, map);
			if (!result.IsSuccess)
				await context.Channel.SendMessageAsync(result.ErrorReason);
		}

		/*
		private async Task MessageReceived(SocketMessage message)
		{
			if (message.Content == "!ping")
			{
				await message.Channel.SendMessageAsync("Pong!");
			}
		}
		*/

		private Task Log(LogMessage msg)
		{
			Console.WriteLine(msg.ToString());
			return Task.FromResult(false);
		}

		/* Moved to CommandModules
		public Task CheckInvasionTimes()
		{
			WebClient webClient = new WebClient();
			string htmlCode = webClient.DownloadString("http://wowhead.com");
			System.IO.File.WriteAllText(@"test.txt", htmlCode);
			string legionAssaultsLine = GetLine("<script>$WH.news.addInvasionDisplay(\"US\", {\"id\":\"legion-assaults\"", "test.txt");

			char[] charArray = legionAssaultsLine.ToCharArray();

			string cleanLegionAssaults = "";

			int i = 0;

			while (charArray[i].ToString() == " ")
			{
				i++;
			}

			for (int j = i; j < charArray.Count(); j++)
			{
				cleanLegionAssaults += charArray[j].ToString();
			}

			Console.WriteLine(cleanLegionAssaults);

			string[] words = cleanLegionAssaults.Split(',');

			foreach (string word in words)
			{
				//Console.WriteLine(word);
			}

			//string name = words[2].Remove(0, 7);
			//string url = words[3];
			string zoneName = words[5].Remove(0, 11).Trim('"');
			long[] upcomingTimesEpoch = { Convert.ToInt64(words[6].Remove(0, 12)), Convert.ToInt64(words[7]), Convert.ToInt64(words[8]), Convert.ToInt64(words[9]), Convert.ToInt64(words[10].Trim(']')) };

			//char[] trimChars = { '}', ')', ';', '<', '/', 's', 'c', 'r', 'i', 'p', 't' };

			int length = Convert.ToInt32(words[11].Remove(0, 9).Remove(5, 12));
			//string cleanLength = length.TrimEnd(trimChars);

			//Console.WriteLine(upcomingTimesEpoch[4]);

			long epochTimeNow = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

			Console.WriteLine(length);

			foreach (long epochTime in upcomingTimesEpoch)
			{
				long tempEpochTime = epochTime + (length);
				//Console.WriteLine(tempEpochTime);
				if (epochTimeNow < tempEpochTime)
				{
					if (epochTimeNow > epochTime)
					{
						long epochTimeLeft = (tempEpochTime - epochTimeNow);
						TimeSpan t = TimeSpan.FromSeconds(epochTimeLeft);
						string hoursLeft = t.ToString(@"hh").TrimStart('0'); 
						string minutesLeft = t.ToString(@"mm").TrimStart('0');
						string zone = "";
						if (zoneName != "")
						{
							zone = zoneName;
						}
						else 
						{
							zone = "A zone";
						}
						Console.WriteLine(zone + " is being assaulted by the legion! " + hoursLeft + " hours and " + minutesLeft + " minutes remaining...");
					}
					else
					{
						Console.WriteLine(ConvertUnixEpochTime(epochTime));
					}
				}
				//Console.WriteLine(epochTime);
			}

			//System.IO.File.WriteAllText(@"test.txt", htmlCode);

			//Console.WriteLine(htmlCode);
			/* Need updated chromium/chromedriver.exe
			 * IWebDriver driver = new ChromeDriver(@"C:\Users\Kurt\AppData\Local\Google\Chrome SxS\Application");
			driver.Navigate().GoToUrl("https://wowhead.com");

			return Task.FromResult(false);
		}
		*/
		/*
		DateTime ConvertUnixEpochTime(long seconds)
		{
			long convertedSeconds = Convert.ToInt64(seconds);
			DateTime time = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

			return time.AddSeconds(seconds).ToLocalTime();
		}

		string GetLine(string keyword, string file)
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

			//if (results.Count < 1)
			return "No invasions found...";

		}
		*/

		/*
		string GetLine(string text, int lineNo)
		{
			string[] lines = text.Replace("\r", "").Split('\n');
			return lines.Length >= lineNo ? lines[lineNo - 1] : null;
		}
		*/
	}
}
