using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Timers;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Diagnostics;
using Discord.Audio;
using Discord.Commands;
using log4net;

namespace STWBot_2
{
	public class CommandModules
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public CommandModules()
		{
		}

	}

	public class Info : ModuleBase
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ConcurrentDictionary<ulong, IAudioClient> ConnectedChannels = new ConcurrentDictionary<ulong, IAudioClient>();

		[Command("say"), Summary("Echos a message.")]
		public async Task Say([Remainder, Summary("The text to echo")] string echo)
		{
			await ReplyAsync(echo);
		}

		[Command("hello", RunMode = RunMode.Async)]
		public async Task JoinChannel(Discord.IVoiceChannel channel = null)
		{
			// Get the audio channel
			channel = (Context.Message.Author as Discord.IGuildUser).VoiceChannel;
			if (channel == null) { await Context.Message.Channel.SendMessageAsync("User must be in a voice channel, or a voice channel must be passed as an argument."); return; }

			// For the next step with transmitting audio, you would want to pass this Audio Client in to a service.
			var audioClient = await channel.ConnectAsync();

			ConnectedChannels.TryAdd(Context.Guild.Id, audioClient);

			//Console.WriteLine("Attempting to send audio...");
			await SendAsync(audioClient, "LWC-Hello.mp3");
			System.Threading.Thread.Sleep(1000);
			await audioClient.StopAsync();
		}

		[Command("youarenotprepared", RunMode = RunMode.Async)]
		public async Task YouAreNotPrepared(Discord.IVoiceChannel channel = null)
		{
			// Get the audio channel
			channel = (Context.Message.Author as Discord.IGuildUser).VoiceChannel;
			if (channel == null) { await Context.Message.Channel.SendMessageAsync("User must be in a voice channel, or a voice channel must be passed as an argument."); return; }

			// For the next step with transmitting audio, you would want to pass this Audio Client in to a service.
			var audioClient = await channel.ConnectAsync();

			ConnectedChannels.TryAdd(Context.Guild.Id, audioClient);

			//Console.WriteLine("Attempting to send audio...");
			await SendAsync(audioClient, "Illidan-YANP.mp3");
			System.Threading.Thread.Sleep(1000);
			await audioClient.StopAsync();
		}

		[Command("nihao", RunMode = RunMode.Async)]
		public async Task MeiNiHao(Discord.IVoiceChannel channel = null)
		{
			// Get the audio channel
			channel = (Context.Message.Author as Discord.IGuildUser).VoiceChannel;
			if (channel == null) { await Context.Message.Channel.SendMessageAsync("User must be in a voice channel, or a voice channel must be passed as an argument."); return; }

			// For the next step with transmitting audio, you would want to pass this Audio Client in to a service.
			var audioClient = await channel.ConnectAsync();

			ConnectedChannels.TryAdd(Context.Guild.Id, audioClient);

			//Console.WriteLine("Attempting to send audio...");
			await SendAsync(audioClient, "Mei_-_Nǐ_hao.ogg");
			System.Threading.Thread.Sleep(1000);
			await audioClient.StopAsync();
		}

		[Command("corn", RunMode = RunMode.Async)]
		public async Task UncleGoaCorn(Discord.IVoiceChannel channel = null)
		{
			// Get the audio channel
			channel = (Context.Message.Author as Discord.IGuildUser).VoiceChannel;
			if (channel == null) { await Context.Message.Channel.SendMessageAsync("User must be in a voice channel, or a voice channel must be passed as an argument."); return; }

			// For the next step with transmitting audio, you would want to pass this Audio Client in to a service.
			var audioClient = await channel.ConnectAsync();

			ConnectedChannels.TryAdd(Context.Guild.Id, audioClient);

			//Console.WriteLine("Attempting to send audio...");
			await SendAsync(audioClient, "Corn.mp3");
			System.Threading.Thread.Sleep(1000);
			await audioClient.StopAsync();

		}

		[Command("peppers", RunMode = RunMode.Async)]
		public async Task UncleGoaPeppers(Discord.IVoiceChannel channel = null)
		{
			// Get the audio channel
			channel = (Context.Message.Author as Discord.IGuildUser).VoiceChannel;
			if (channel == null) { await Context.Message.Channel.SendMessageAsync("User must be in a voice channel, or a voice channel must be passed as an argument."); return; }

			// For the next step with transmitting audio, you would want to pass this Audio Client in to a service.
			var audioClient = await channel.ConnectAsync();

			ConnectedChannels.TryAdd(Context.Guild.Id, audioClient);

			//Console.WriteLine("Attempting to send audio...");
			await SendAsync(audioClient, "Peppers.mp3");
			System.Threading.Thread.Sleep(1000);
			await audioClient.StopAsync();

		}

		[Command("perfectbrew", RunMode = RunMode.Async)]
		public async Task UncleGoaPerfectBrew(Discord.IVoiceChannel channel = null)
		{
			// Get the audio channel
			channel = (Context.Message.Author as Discord.IGuildUser).VoiceChannel;
			if (channel == null) { await Context.Message.Channel.SendMessageAsync("User must be in a voice channel, or a voice channel must be passed as an argument."); return; }

			// For the next step with transmitting audio, you would want to pass this Audio Client in to a service.
			var audioClient = await channel.ConnectAsync();

			ConnectedChannels.TryAdd(Context.Guild.Id, audioClient);

			//Console.WriteLine("Attempting to send audio...");
			await SendAsync(audioClient, "PerfectBrew.mp3");
			System.Threading.Thread.Sleep(1000);
			await audioClient.StopAsync();

		}

		[Command("whyskinny", RunMode = RunMode.Async)]
		public async Task ChenWhyWouldIWantToBeSkinny(Discord.IVoiceChannel channel = null)
		{
			// Get the audio channel
			channel = (Context.Message.Author as Discord.IGuildUser).VoiceChannel;
			if (channel == null) { await Context.Message.Channel.SendMessageAsync("User must be in a voice channel, or a voice channel must be passed as an argument."); return; }

			// For the next step with transmitting audio, you would want to pass this Audio Client in to a service.
			var audioClient = await channel.ConnectAsync();

			ConnectedChannels.TryAdd(Context.Guild.Id, audioClient);

			//Console.WriteLine("Attempting to send audio...");
			await SendAsync(audioClient, "WhyWouldIWantToBeSkinny.mp3");
			//System.Threading.Thread.Sleep(5000);
			await audioClient.StopAsync();
			audioClient = await channel.ConnectAsync();
			await SendAsync(audioClient, "LookAtBigBelly.mp3");
			System.Threading.Thread.Sleep(1000);
			await audioClient.StopAsync();

		}

		[Command("leave", RunMode = RunMode.Async)]
		public async Task LeaveCmd(Discord.IVoiceChannel channel = null)
		{
			Console.WriteLine("Trying to leave");
			ulong guildID = (Context.Guild as Discord.IGuild).Id;
			IAudioClient client;
			ConnectedChannels.TryRemove(Context.Guild.Id, out client);
			Console.WriteLine(ConnectedChannels.TryGetValue(Context.Guild.Id, out client));
			//ffmpeg.Dispose();
			//await discord.FlushAsync();
			//await DisconnectAsync();
			//await audioClient.StopAsync();
		}

		private Process CreateStream(string path)
		{
			var ffmpeg = new ProcessStartInfo
			{
				FileName = "ffmpeg",
				Arguments = $"-i {path} -ac 2 -f s16le -ar 48000 pipe:1",
				UseShellExecute = false,
				RedirectStandardOutput = true,
			};
			return Process.Start(ffmpeg);
		}

		private async Task SendAsync(IAudioClient client, string path)
		{
			var audioClient = client;
			// Create FFmpeg using the previous example
			var ffmpeg = CreateStream(path);
			var output = ffmpeg.StandardOutput.BaseStream;
			//playing = true;
			var discord = client.CreatePCMStream(AudioApplication.Mixed, default(int?) ,10);
			await output.CopyToAsync(discord);
			await discord.FlushAsync();

		}
	}


	[Group("sample")]
	public class Sample : ModuleBase
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		[Command("square"), Summary("Squares a number.")]
		public async Task Square([Summary("The number to square.")] int num)
		{
			await Context.Channel.SendMessageAsync($"{num}^2 = {Math.Pow(num, 2)}");
		}

		[Command("ping"), Summary("Send ping to bot")]
		public async Task Ping()
		{
			await ReplyAsync("Pong!");
		}
	}

	public class Wow : ModuleBase
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		SQLiteConnection m_dbConnection;


		[Command("invasion"), Summary("Replies when the next invasions are about to begin")]
		[Alias("invasions")]
		public async Task Invasion()
		{
			m_dbConnection = new SQLiteConnection(@"Data Source=DB\bot.sqlite;Version=3;");
			m_dbConnection.Open();

			string sql;
			SQLiteCommand command;


			Utilities util = new Utilities();
			//util.DownloadNewWowHead();
			//string legionAssaultsLine = util.GetLine("<script>$WH.news.addAssaultDisplay(\"US\", {\"id\":\"legion-assaults\"", "test.txt");
			string msg = "";

			//Console.WriteLine(legionAssaultsLine);

			//string[] words = legionAssaultsLine.Split(':');
			List<string> upcomingAssaultsList = new List<string>();
			//string[] upcomingAssaultsStr = words[6].TrimStart('[').Replace("],\"length\"", "").Split(',');
			//int length = Convert.ToInt32(words[7].Replace(" ", "").Replace("});</script></div>", ""));
			//string zoneName = words[5].Replace("\"", "").Replace(",upcoming", "");


			sql = "SELECT zonename, assaulttime, length FROM legionassaults";
			command = new SQLiteCommand(sql, m_dbConnection);

			SQLiteDataReader reader = command.ExecuteReader();

			int length = 0;
			string zoneName = "";

			while (reader.Read())
			{
				if (reader["zonename"].ToString() != "" && reader["zonename"].ToString() != null)
				{
					zoneName = reader["zonename"].ToString();
				}
				upcomingAssaultsList.Add(reader["assaulttime"].ToString());
				length = Convert.ToInt32(reader["length"]);
			}

			Console.WriteLine(length);
			string[] upcomingAssaultsStr = upcomingAssaultsList.ToArray();
			List<long> upcomingTimesEpoch = new List<long>();

			int i = 0;

			foreach (string time in upcomingAssaultsStr)
			{
				upcomingTimesEpoch.Add(Convert.ToInt64(time));
				Console.WriteLine(time);
			}

			long epochTimeNow = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

			Console.WriteLine(length);

			int doOnce = 0;

			foreach (long epochTime in upcomingTimesEpoch)
			{
				long tempEpochTime = epochTime + (length);

				if (epochTimeNow < tempEpochTime)
				{
					if (epochTimeNow > epochTime)
					{
						long epochTimeLeft = (tempEpochTime - epochTimeNow);
						TimeSpan t = TimeSpan.FromSeconds(epochTimeLeft);
						string hoursLeft = t.ToString(@"hh");
						string minutesLeft = t.ToString(@"mm").TrimStart('0');
						if (hoursLeft != "00")
						{
							hoursLeft = hoursLeft.TrimStart('0') + " hours and ";
						}
						else
						{
							hoursLeft = "";
						}
						string zone = "";
						if (zoneName != "" && zoneName != null)
						{
							zone = zoneName;
						}
						else
						{
							zone = "A zone";
						}

						msg = "__**" + zone + "**__** is currently being assaulted by the Legion!**__**\n" + hoursLeft + minutesLeft + " minutes**__** remaining...**\n";
						Console.WriteLine(zone + " is being assaulted by the legion! " + hoursLeft + " hours and " + minutesLeft + " minutes remaining...");
					}
					else
					{
						while (doOnce < 1)
						{
							msg += "\n\nThe next invasion times are as follows: \n";
							doOnce++;
						}

						msg += "\n" + util.ConvertUnixEpochTime(epochTime).ToString() + "\n";
						Console.WriteLine(util.ConvertUnixEpochTime(epochTime));
					}
				}
			}

			await Context.Channel.SendMessageAsync(msg);



		}

		/* Obsolete - WoWHead Website Changed
		[Command("invasion"), Summary("Replies when the next invasions are about to begin")]
		[Alias("invasions")]
		public async Task Invasion()
		{
			Utilities util = new Utilities();
			util.DownloadNewWowHead();
			string legionAssaultsLine = util.GetLine("<script>$WH.news.addAssaultDisplay(\"US\", {\"id\":\"legion-assaults\"", "test.txt");
			string msg = "";

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
			string zoneName = words[6].Remove(0, 11).Trim('"');
			long[] upcomingTimesEpoch = { Convert.ToInt64(words[8].Remove(0, 12)), Convert.ToInt64(words[8]), Convert.ToInt64(words[9]), Convert.ToInt64(words[10]), Convert.ToInt64(words[11].Trim(']')) };

			//char[] trimChars = { '}', ')', ';', '<', '/', 's', 'c', 'r', 'i', 'p', 't' };

			int length = Convert.ToInt32(words[13].Remove(0, 9).Remove(5, 12));
			//string cleanLength = length.TrimEnd(trimChars);

			//Console.WriteLine(upcomingTimesEpoch[4]);

			long epochTimeNow = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

			Console.WriteLine(length);

			int doOnce = 0;

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
						string hoursLeft = t.ToString(@"hh");
						string minutesLeft = t.ToString(@"mm").TrimStart('0');
						if (hoursLeft != "00")
						{
							hoursLeft = hoursLeft.TrimStart('0') + " hours and ";
						}
						else
						{
							hoursLeft = "";
						}
						string zone = "";
						if (zoneName != "")
						{
							zone = zoneName;
						}
						else
						{
							zone = "A zone";
						}

						msg = "__**" + zone + "**__** is currently being assaulted by the Legion!**__**\n" + hoursLeft + minutesLeft + " minutes**__** remaining...**\n";
						Console.WriteLine(zone + " is being assaulted by the legion! " + hoursLeft + " hours and " + minutesLeft + " minutes remaining...");
					}
					else
					{
						while(doOnce < 1)
						{
							msg += "\n\nThe next invasion times are as follows: \n";
							doOnce++;
						}

						msg += "\n" + util.ConvertUnixEpochTime(epochTime).ToString() + "\n";
						Console.WriteLine(util.ConvertUnixEpochTime(epochTime));
					}
				}
				//Console.WriteLine(epochTime);
			}

			await Context.Channel.SendMessageAsync(msg);
		}
	*/


		[Command("affix"), Summary("Shows the Mythic+ Affixes active for the current week")]
		[Alias("affixes")]
		public async Task Affixes()
		{
			Utilities util = new Utilities();
			//util.DownloadNewWowHead();
			//List<string> mythicAffixesList = util.ReturnAllLines("US-mythicaffix-", "test.txt");
			List<string> mythicAffixesList = new List<string>();
			List<string> affixNumberList = new List<string>();

			m_dbConnection = new SQLiteConnection(@"Data Source=DB\bot.sqlite;Version=3;");
			m_dbConnection.Open();

			string sql;
			SQLiteCommand command;

			sql = "SELECT affixname, affixnum, dungeonlevel FROM dungeonaffixes";
			command = new SQLiteCommand(sql, m_dbConnection);

			SQLiteDataReader reader = command.ExecuteReader();

			while (reader.Read())
			{
				mythicAffixesList.Add(reader["affixname"].ToString());
				affixNumberList.Add(reader["affixnum"].ToString());
			}

			string[] mythicAffixes = mythicAffixesList.ToArray();
			//string[] mythicAffixes = { util.GetLine("id=\"US-mythicaffix-1\"", "test.txt").TrimStart(), util.GetLine("id=\"US-mythicaffix-2\"", "test.txt").TrimStart(), util.GetLine("id=\"US-mythicaffix-3\"", "test.txt").TrimStart() };
			string[] affixNumbers = affixNumberList.ToArray();
			string msg = "This week's Mythic+ Dungeon Affixes are:\n\n";
			Console.WriteLine(mythicAffixes[1]);

			int i = 0;
			int j = 1;

			foreach (string affix in mythicAffixes)
			{
				//string[] words = affix.Split('>');
				//string[] cleanedWords = words[6].Split('<');

				//string[] numbers = words[0].Split('=');
				//int cleanedNumber = Convert.ToInt32(numbers[2].TrimEnd('"'));

				//Console.WriteLine(mythicAffixes[i]);
				//mythicAffixes[i] = words[2].Replace(" ", "").Replace("</a", "");
				//affixNumbers[i] = Convert.ToInt32(numbers[2].Replace ("\" id", ""));

				//Console.WriteLine(mythicAffixes[i]);
				//Console.WriteLine(affixNumbers[i]);

				j += 3;

				msg += "**" + affix + "** - Mythic Keystone Level " + j + "+\nMore Info: <http://www.wowhead.com/affix=" + affixNumbers[i] + ">\n\n";

				i++;
			}



			Console.WriteLine(mythicAffixes[0] + " " + affixNumbers[0]);
			Console.WriteLine(mythicAffixes[1] + " " + affixNumbers[1]);
			//Console.WriteLine(mythicAffixes[2] + " " + affixNumbers[2]);

			//msg = "This week's Mythic+ Dungeon Affixes are:\n\n**" + mythicAffixes[0] + "** - Mythic Keystone Level 4+\nMore Info: <http://www.wowhead.com/affix=" + affixNumbers[0] + ">\n\n**" + mythicAffixes[1]; //+ "** - Mythic Keystone Level 7+\nMore Info: <http://www.wowhead.com/affix=" + affixNumbers[1] + ">\n\n**" + mythicAffixes[2] + "** - Mythic Keystone Level 10+\nMore Info: <http://www.wowhead.com/affix=" + affixNumbers[2] + ">";
			await Context.Channel.SendMessageAsync(msg);
		}

		[Command("emissaries"), Summary("Shows which emissaries are available for completion")]
		[Alias("emissary")]
		public async Task Emissaries()
		{
			Utilities util = new Utilities();
			//util.DownloadNewWowHead();

			//string[] emissaries = { util.GetLine("\"US--1\"", "test.txt"), util.GetLine("\"US--2\"", "test.txt"), util.GetLine("\"US--3\"", "test.txt") };
			List<string> emissariesList = new List<string>();
			List<string> timeLeftList = new List<string>();
			//List<string> emissariesAndTimesList = util.ReturnAllLines("US-emissary", "test.txt");

			m_dbConnection = new SQLiteConnection(@"Data Source=DB\bot.sqlite;Version=3;");
			m_dbConnection.Open();

			string sql;
			SQLiteCommand command;

			sql = "SELECT name, timeleft FROM emissaries";
			command = new SQLiteCommand(sql, m_dbConnection);

			SQLiteDataReader reader = command.ExecuteReader();

			while (reader.Read())
			{
				emissariesList.Add(reader["name"].ToString());
				timeLeftList.Add(reader["timeleft"].ToString());
			}

			/*int i = 1;
			foreach (string record in emissariesAndTimesList)
			{
				if (i % 2 != 0)
				{
					emissariesList.Add(record);
				}
				else
				{
					timeLeftList.Add(record);
				}

				i++;
			} */
			//string[] timeLeft = { util.GetLine("\'US--1\'", "test.txt"), util.GetLine("\'US--2\'", "test.txt"), util.GetLine("\'US--3\'", "test.txt") };

			string[] emissaries = emissariesList.ToArray();
			string[] timeLeft = timeLeftList.ToArray();

			string msg = "Current active emissaries are:\n\n";

			int j = 0;

			/*
			foreach (string time in timeLeft)
			{
				string[] numbers = time.Split(',');
				timeLeft[j] = numbers[1].TrimStart('"', ' ').TrimEnd('"').Replace("hr", "hours").Replace("min", "minutes").Replace("day", "days");

				j++;
			}
			*/

			int k = 0;

			foreach (string emissary in emissaries)
			{
				//string[] words = emissary.Split('>');
				//emissaries[k] = words[1].TrimEnd('<', '/', 'a');

				msg += "**" + emissaries[k] + "** - __" + timeLeft[k] + "__ remaining to complete\n\n";

				k++;
			}

			//Console.WriteLine(emissaries[0]);
			//Console.WriteLine(timeLeft[0]);
			//Console.WriteLine(emissaries[1]);
			//Console.WriteLine(timeLeft[1]);
			//Console.WriteLine(emissaries[2]);
			//Console.WriteLine(timeLeft[2]);

			//msg = "Current active emissaries are:\n\n**" + emissaries[0] + "** - __" + timeLeft[0] + "__ remaining to complete\n\n**" + emissaries[1] + "** - __" + timeLeft[1] + "__ remaining to complete\n\n**" + emissaries[2] + "** - __" + timeLeft[2] + "__ remaining to complete";
			await Context.Channel.SendMessageAsync(msg);
		}

		[Command("brokenshorebuildings"), Summary("Shows the % for buildings on the Broken Shore!")]
		[Alias("bsb")]
		public async Task BrokenShoreBuildings()
		{
			Utilities util = new Utilities();
			//util.DownloadNewWowHead();

			//string[] buildings = { util.GetLineAfterNum("data-region=\"US\" data-building=\"1\"", "test.txt", 10), util.GetLineAfterNum("data-region=\"US\" data-building=\"2\"", "test.txt", 10), util.GetLineAfterNum("data-region=\"US\" data-building=\"3\"", "test.txt", 10) };
			//string[] buildingStates = { util.GetLineAfterNum("data-region=\"US\" data-building=\"1\"", "test.txt", 8), util.GetLineAfterNum("data-region=\"US\" data-building=\"2\"", "test.txt", 8), util.GetLineAfterNum("data-region=\"US\" data-building=\"3\"", "test.txt", 8) };

			m_dbConnection = new SQLiteConnection(@"Data Source=DB\bot.sqlite;Version=3;");
			m_dbConnection.Open();

			string sql;
			SQLiteCommand command;

			sql = "SELECT buildingstate, buildingpercentage FROM brokenshorebuildings";
			command = new SQLiteCommand(sql, m_dbConnection);

			SQLiteDataReader reader = command.ExecuteReader();

			List<string> buildingPercentList = new List<string>();
			List<string> buildingStatesList = new List<string>();

			while (reader.Read())
			{
				buildingPercentList.Add(reader["buildingpercentage"].ToString());
				buildingStatesList.Add(reader["buildingstate"].ToString());
			}
			//string[] buildings = { util.BrokenShoreBuildingGetLine("data-region=\"US\" data-building=\"1\"", "class=\"tiw-bs-status-progress", "test.txt"), util.BrokenShoreBuildingGetLine("data-region=\"US\" data-building=\"2\"", "class=\"tiw-bs-status-progress", "test.txt"), util.BrokenShoreBuildingGetLine("data-region=\"US\" data-building=\"3\"", "class=\"tiw-bs-status-progress", "test.txt") };
			//string[] buildingStates = { util.BrokenShoreBuildingGetLine("data-region=\"US\" data-building=\"1\"", "class=\"tiw-bs-status-state", "test.txt"), util.BrokenShoreBuildingGetLine("data-region=\"US\" data-building=\"2\"", "class=\"tiw-bs-status-state", "test.txt"), util.BrokenShoreBuildingGetLine("data-region=\"US\" data-building=\"3\"", "class=\"tiw-bs-status-state", "test.txt") };

			string[] buildings = buildingPercentList.ToArray();
			string[] buildingStates = buildingStatesList.ToArray();

			//Console.WriteLine(buildings[0]);
			//Console.WriteLine(buildingStates[0]);
			//Console.WriteLine(buildings[1]);
			//Console.WriteLine(buildingStates[1]);
			//Console.WriteLine(buildings[2]);
			//Console.WriteLine(buildingStates[2]);

			string msg = "";

			//string mageTowerPercentage = util.GetLineAfterNum("Mage Tower", "test.txt", 8);
			//string commandCenterPercentage = util.GetLineAfterNum("Command Center", "test.txt", 8);
			//string netherDisruptorPercentage = util.GetLineAfterNum("Nether Disruptor", "test.txt", 8);
			/*
			int i = 0;

			foreach (string building in buildings)
			{
				string[] words = building.Split('>');
				buildings[i] = words[4].Replace("</span", "");
				i++;
			}

			int j = 0;

			foreach (string buildingState in buildingStates)
			{
				string[] words = buildingState.Split('>');
				buildingStates[j] = words[2].Replace("</div", "");
				j++;
			}
			*/
			/*
			Console.WriteLine(buildings[0]);
			Console.WriteLine(buildingStates[0]);
			Console.WriteLine(buildings[1]);
			Console.WriteLine(buildingStates[1]);
			Console.WriteLine(buildings[2]);
			Console.WriteLine(buildingStates[2]);
			*/
			msg = "Current Broken Shore buildings stats are as follows: \n\n__**Mage Tower**__\n" + buildingStates[0] + "\n" + buildings[0] + "\n\n__**Command Center**__\n" + buildingStates[1] + "\n" + buildings[1] + "\n\n__**Nether Disruptor**__\n" + buildingStates[2] + "\n" + buildings[2];

			await Context.Channel.SendMessageAsync(msg);

		}

		[Command("menagerie"), Summary("Shows the % for buildings on the Broken Shore!")]
		[Alias("pets")]
		public async Task MenageriePets()
		{
			Utilities util = new Utilities();
			//util.DownloadNewWowHead();

			//List<string> petsList = util.ReturnAllLines("US-menagerie-", "test.txt");

			//string[] pets = { util.GetLine("US-menagerie-1", "test.txt"), util.GetLine("US-menagerie-2", "test.txt"), util.GetLine("US-menagerie-3", "test.txt") };

			List<string> petsList = new List<string>();

			m_dbConnection = new SQLiteConnection(@"Data Source=DB\bot.sqlite;Version=3;");
			m_dbConnection.Open();

			string sql;
			SQLiteCommand command;

			sql = "SELECT petname FROM menagerie";
			command = new SQLiteCommand(sql, m_dbConnection);

			SQLiteDataReader reader = command.ExecuteReader();

			while (reader.Read())
			{
				petsList.Add(reader["petname"].ToString());
			}

			string[] pets = petsList.ToArray();

			string msg = "Pets currently active in your Garrison's Managerie this week are:\n\n";

			int i = 0;

			foreach (string pet in pets)
			{
				//string[] words = pet.Split('>');
				//pets[i] = words[2].TrimStart(' ').Replace("</a", "");

				msg += "**" + pets[i] + "**\n\n";

				i++;
			}

			//Console.WriteLine(pets[0]);

			//msg = "Pets currently active in your Garrison's Managerie this week are:\n\n**" + pets[0] + "**\n\n**" + pets[1] + "**\n\n**" + pets[2] + "**";

			await Context.Channel.SendMessageAsync(msg);
		}

	 	[Command("violethold"), Summary("Shows the % for buildings on the Broken Shore!")]
		[Alias("vhbosses")]
		public async Task VioletHoldBosses()
		{
			Utilities util = new Utilities();
			//util.DownloadNewWowHead();

			string msg = "";
			List<string> vhBossesList = new List<string>();

			m_dbConnection = new SQLiteConnection(@"Data Source=DB\bot.sqlite;Version=3;");
			m_dbConnection.Open();

			string sql;
			SQLiteCommand command;

			sql = "SELECT bossname FROM vhbosses";
			command = new SQLiteCommand(sql, m_dbConnection);

			SQLiteDataReader reader = command.ExecuteReader();

			while (reader.Read())
			{
				vhBossesList.Add(reader["bossname"].ToString());
			}

			//string[] vhBosses = { util.GetLine("US-violethold-1", "test.txt"), util.GetLine("US-violethold-2", "test.txt"), util.GetLine("US-violethold-3", "test.txt") }; //need to update to find all!
			string[] vhBosses = vhBossesList.ToArray();
			int i = 0;

			msg = "Current bosses active in the Violet Hold this week are:";

			foreach (string boss in vhBosses)
			{
				msg += "\n\n**" + vhBosses[i] + "**";
				i++;
			}

			 

			await Context.Channel.SendMessageAsync(msg);
		}

		[Command("dailyreset"), Summary("Shows the % for buildings on the Broken Shore!")]
		[Alias("dailyquestreset")]
		public async Task DailyQuestReset()
		{
			Utilities util = new Utilities();
			//util.DownloadNewWowHead();

			string msg = "";

			long epochTimeNow = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

			string dailyReset = "";
			//string[] words = dailyReset.Split('"');
			//dailyReset = words[5];

			m_dbConnection = new SQLiteConnection(@"Data Source=DB\bot.sqlite;Version=3;");
			m_dbConnection.Open();

			string sql;
			SQLiteCommand command;

			sql = "SELECT time, timeleft FROM dailyreset";
			command = new SQLiteCommand(sql, m_dbConnection);

			SQLiteDataReader reader = command.ExecuteReader();

			while (reader.Read())
			{
				dailyReset = reader["time"].ToString();
			}

			long epochTimeLeft = Convert.ToInt64(dailyReset) - epochTimeNow;

			dailyReset = util.ConvertUnixEpochTime(Convert.ToInt64(dailyReset)).ToString();
			string timeNow = util.ConvertUnixEpochTime(epochTimeNow).ToString();

			TimeSpan t = TimeSpan.FromSeconds(epochTimeLeft);
			string hoursLeft = t.ToString(@"hh");
			string minutesLeft = t.ToString(@"mm").TrimStart('0');
			if (hoursLeft != "00")
			{
				hoursLeft = hoursLeft.TrimStart('0') + " hours and ";
			}
			else
			{
				hoursLeft = "";
			}

			//Console.WriteLine(dailyReset);
			//Console.WriteLine(timeNow);

			msg = "There is **" + hoursLeft + minutesLeft + " minutes** remaining until dailies are reset.\n\n Dailies reset at **" + dailyReset + "**.";

			await Context.Channel.SendMessageAsync(msg);
		}

		[Command("wowtoken"), Summary("Shows the % for buildings on the Broken Shore!")]
		[Alias("token")]
		public async Task WoWToken()
		{
			Utilities util = new Utilities();
			//util.DownloadNewWowHead();

			string msg = "";

			//string tokenGold = util.GetLine("tiw-group-wowtoken", "test.txt");
			string tokenGold = "";

			m_dbConnection = new SQLiteConnection(@"Data Source=DB\bot.sqlite;Version=3;");
			m_dbConnection.Open();

			string sql;
			SQLiteCommand command;

			sql = "SELECT price FROM wowtoken";
			command = new SQLiteCommand(sql, m_dbConnection);

			SQLiteDataReader reader = command.ExecuteReader();

			while (reader.Read())
			{
				tokenGold = reader["price"].ToString();
			}

			Console.WriteLine(tokenGold);

			//string[] words = tokenGold.Split('>');
			//tokenGold = words[9].Replace("</span", "");

			//Console.WriteLine(tokenGold);

			msg = "WoW Tokens are currently worth **" + tokenGold + "** gold.";

			await Context.Channel.SendMessageAsync(msg);
		}

		[Command("worldbosses"), Summary("Shows the % for buildings on the Broken Shore!")]
		[Alias("bosses")]
		public async Task WorldBosses()
		{
			Utilities util = new Utilities();
			//util.DownloadNewWowHead();

			string msg = "Below are the current World Bosses that are active this week:\n\n";

			//List<string> bossesList = util.ReturnAllLines("US-epiceliteworld-", "test.txt");

			List<string> bossesList = new List<string>();

			m_dbConnection = new SQLiteConnection(@"Data Source=DB\bot.sqlite;Version=3;");
			m_dbConnection.Open();

			string sql;
			SQLiteCommand command;

			sql = "SELECT bossname FROM worldbosses";
			command = new SQLiteCommand(sql, m_dbConnection);

			SQLiteDataReader reader = command.ExecuteReader();

			while (reader.Read())
			{
				bossesList.Add(reader["bossname"].ToString());
			}

			string[] bossesArray = bossesList.ToArray();

			int i = 0;

			foreach (string boss in bossesArray)
			{
				//string[] words = boss.Split('>');
				//bossesArray[i] = words[1].Replace("</a", "");
				msg += "**" + bossesArray[i] + "**\n\n";
				i++;
			}

			msg.TrimEnd('\n');

			//Console.WriteLine(msg);

			await Context.Channel.SendMessageAsync(msg);
		}

		[Command("worldevents"), Summary("Shows the % for buildings on the Broken Shore!")]
		[Alias("events")]
		public async Task WorldEvents()
		{
			Utilities util = new Utilities();
			//util.DownloadNewWowHead();

			string msg = "Below are the current World Events that are active this week:\n\n";

			//List<string> worldEvents = util.ReturnAllLines("US-holiday-", "test.txt");
			List<string> worldEventsList = new List<string>();

			m_dbConnection = new SQLiteConnection(@"Data Source=DB\bot.sqlite;Version=3;");
			m_dbConnection.Open();

			string sql;
			SQLiteCommand command;

			sql = "SELECT eventname FROM worldevents";
			command = new SQLiteCommand(sql, m_dbConnection);

			SQLiteDataReader reader = command.ExecuteReader();

			while (reader.Read())
			{
				worldEventsList.Add(reader["eventname"].ToString());
			}

			string[] worldEventsArray = worldEventsList.ToArray();

			int i = 0;

			foreach (string worldEvent in worldEventsArray)
			{
				//string[] words = worldEvent.Split('>');
				//worldEventsArray[i] = words[2].TrimStart(' ').Replace("</a", "");
				msg += "**" + worldEvent + "**\n\n";
				i++;
			}

			//msg.TrimEnd('\n');

			await Context.Channel.SendMessageAsync(msg);
		}

		[Command("xurios"), Summary("Shows the % for buildings on the Broken Shore!")]
		[Alias("curiouscoins")]
		public async Task Xurios()
		{
			Utilities util = new Utilities();
			//util.DownloadNewWowHead();

			string msg = "Currently Xur\'ios is selling the following for Curious Coins:\n\n";

			//List<string> xurios = util.ReturnAllLines("US-xurios-", "test.txt");
			List<string> xuriosList = new List<string>();

			m_dbConnection = new SQLiteConnection(@"Data Source=DB\bot.sqlite;Version=3;");
			m_dbConnection.Open();

			string sql;
			SQLiteCommand command;

			sql = "SELECT itemname FROM xurios";
			command = new SQLiteCommand(sql, m_dbConnection);

			SQLiteDataReader reader = command.ExecuteReader();

			while (reader.Read())
			{
				xuriosList.Add(reader["itemname"].ToString());
			}

			string[] xuriosArray = xuriosList.ToArray();

			int i = 0;

			foreach (string item in xuriosArray)
			{
				//string[] words = item.Split('>');
				//xuriosArray[i] = words[2].TrimStart(' ').Replace("</a", "");
				if (xuriosArray[i] != "")
				{
					msg += "**" + xuriosArray[i] + "**\n\n";
				}
				i++;
			}

			//msg.TrimEnd('\n');

			await Context.Channel.SendMessageAsync(msg);

		}

		[Command("autoalerts"), Summary("Shows the % for buildings on the Broken Shore!")]
		public async Task StartAutoAlerts()
		{

			m_dbConnection = new SQLiteConnection(@"Data Source=DB\bot.sqlite;Version=3;");
			m_dbConnection.Open();

			string sql;
			SQLiteCommand command;

			sql = "UPDATE autoalerts SET value = 'true', channelid = '" + Context.Channel.Id + "' WHERE guildid = '" + Context.Guild.Id + "'";
			command = new SQLiteCommand(sql, m_dbConnection);
			command.ExecuteNonQuery();

			m_dbConnection.Close();
             
		}

		[Command("stopautoalerts"), Summary("Shows the % for buildings on the Broken Shore!")]
		public async Task StopAutoAlerts()
		{

			m_dbConnection = new SQLiteConnection(@"Data Source=DB\bot.sqlite;Version=3;");
			m_dbConnection.Open();

			string sql;
			SQLiteCommand command;

			sql = "UPDATE autoalerts SET value = 'false', channelid = '" + Context.Channel.Id + "' WHERE guildid = '" + Context.Guild.Id + "'";
			command = new SQLiteCommand(sql, m_dbConnection);
			command.ExecuteNonQuery();

			m_dbConnection.Close();

		}

		void TimerElapsed(object sender, ElapsedEventArgs e)
		{
			Context.Channel.SendMessageAsync("This is a test");
		}

		public async Task Testing()
		{


		}
	}
}
