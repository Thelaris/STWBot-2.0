using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using System.Data.SQLite;
using System.Timers;
using Discord;
using Discord.WebSocket;
using Discord.Net.Providers.WS4Net;
using Discord.Commands;
using log4net;
using HtmlAgilityPack;
using System.Linq;


namespace STWBot_2
{
	public class MainClass
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private CommandService commands;
		public DiscordSocketClient client;
		private Utilities util = new Utilities();
		private Wow commander = new Wow();
		SQLiteConnection m_dbConnection;
		private Token tokenRef = new Token();
		public IServiceProvider map;
		public static MainClass instance = null;

		public static void Main(string[] args) => new MainClass().MainAsync().GetAwaiter().GetResult();

		public async Task MainAsync()
		{
			log.Info("Starting STWBot-2.0!");
			if (instance == null)
				instance = this;

			client = new DiscordSocketClient(new DiscordSocketConfig
			{
				WebSocketProvider = WS4NetProvider.Instance,
			});
			commands = new CommandService();

			map = null;

#region InitalStartup
			util.DownloadNewWowHead();

			log.Debug("");
			log.Debug("---------- START INTIAL STARTUP SELF TEST ----------");
			log.Debug("     ----- Testing Emissaries -----");
			GetInnerText("//a[contains(@id,'US-emissary-')]");
			GetInnerText("//a[contains(@id,'US-emissary-')]/../script");
			log.Debug("     ----- Testing Menagerie -----");
			GetInnerText("//a[contains(@id,'US-menagerie-')]");
			log.Debug("     ----- Testing Mythic+ Affixes -----");
			GetInnerText("//a[contains(@id,'US-mythicaffix-')]");
			log.Debug("     ----- Testing World Bosses -----");
			GetInnerText("//a[contains(@id,'US-epiceliteworld-')]");
			log.Debug("     ----- Testing World Events -----");
			GetInnerText("//a[contains(@id,'US-holiday-')]");
			log.Debug("     ----- Testing Xur'ios -----");
			GetInnerText("//a[contains(@id,'US-xurios-')]");
			log.Debug("     ----- Testing Violet Hold Bosses -----");
			GetInnerText("//a[contains(@id,'US-violethold-')]");
			log.Debug("     ----- Testing Daily Reset -----");
			GetAttributeValue("//div[contains(@id,'tiw-timer-US')]", "data-timestamp");
			log.Debug("     ----- Testing WoW Token -----");
			GetInnerText("//span[contains(@class,'moneygold')]", 1);
			log.Debug("     ----- Testing Broken Shore Buildings -----");
			GetInnerText("//div[contains(@data-region,'US')]/../../div/a");
			GetInnerText("//div[contains(@data-region,'US')]/../..//div/div[contains(@class, 'tiw-bs-status-state')]");
			GetAttributeValue("//div[contains(@data-region,'US')]/../..//div/div[contains(@class, 'tiw-bs-status-state')]/../div[contains(@class, 'tiw-bs-status-progress')]/span", "title");
			log.Debug("     ----- Testing Legion Assaults -----");
			GetInnerText("//div[contains(@id,'tiw-assault-US')]/../script[contains(.,'legion-assaults')]");
			log.Debug("---------- END INTIAL STARTUP SELF TEST ----------");
			log.Debug("");

			await InstallCommands();
			await ConnectToDB();
			await CheckEmissaries2();
			await CheckMenagerie2();
			await CheckVHBosses2();
			await CheckDailyReset2();
			await CheckWowToken2();
			await CheckWorldBosses2();
			await CheckWorldEvents2();
			await CheckXurios2();
			await CheckAffixes2();
			await CheckInvasions2();
			await CheckBrokenShoreBuildings2();

			client.Log += Log;
			client.JoinedGuild += JoinedNewGuild;
			client.LeftGuild += LeftGuild;
			//client.MessageReceived += MessageReceived;
			//client.UserJoined += UserJoined;

			await client.LoginAsync(TokenType.Bot, tokenRef.token);
			await client.StartAsync();





			client.SetGameAsync("Testing");

			Timer t = new Timer();
			t.Interval = 600000; //In milliseconds here //600000
			t.AutoReset = true; //Stops it from repeating
			t.Elapsed += new ElapsedEventHandler(TimerElapsed);
			t.Start();

			// Block this task until the program is closed (infinite delay)
			await Task.Delay(-1);
		}
#endregion

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

		public async Task ConnectToDB()
		{
			string sql;
			SQLiteCommand command;
			Directory.CreateDirectory(@"DB");
			if (!File.Exists(@"DB\\bot.sqlite"))
			{
				SQLiteConnection.CreateFile(@"DB\bot.sqlite");

				sql = "CREATE TABLE testing (test VARCHAR(20))";
				command = new SQLiteCommand(sql, m_dbConnection);
				command.ExecuteNonQuery();

				sql = "INSERT INTO testing (test) VALUES ('This is a test')";
				command = new SQLiteCommand(sql, m_dbConnection);
				command.ExecuteNonQuery();
			}

			m_dbConnection = new SQLiteConnection(@"Data Source=DB\bot.sqlite;Version=3;");
			m_dbConnection.Open();

			sql = "SELECT * FROM testing";
			command = new SQLiteCommand(sql, m_dbConnection);
			SQLiteDataReader reader = command.ExecuteReader();
			while (reader.Read())
				Console.WriteLine("Result = " + reader["test"]);

			m_dbConnection.Close();
		}

		public async Task CheckWowhead()
		{
			util.DownloadNewWowHead();

			//await AddToDB();
		}

		private Task UserJoined(SocketGuildUser user)
		{
			IRole membersRole = client.GetGuild(303997236482801670).GetRole(305921894513770499);
			IRole pugsRole = client.GetGuild(303997236482801670).GetRole(306107573277425664);

			Console.WriteLine(user.Id);

			//SocketUser newUser = client.GetUser(user.Id).;
			//client.GetGuild().GetVoiceChannel().Users.

			//if (newUser..VoiceChannel.Id == client.GetChannel(306107185145053194).Id)
			//{
			Console.WriteLine("PUG!");
			user.AddRoleAsync(pugsRole);
			//}
			//else
			//{
			Console.WriteLine("MEMBER!");
			user.AddRoleAsync(membersRole);
			//}

			return Task.FromResult(true);
		}

		private Task Log(LogMessage msg)
		{
			log.Info(msg.ToString());
			return Task.FromResult(false);
		}

		private Task JoinedNewGuild(SocketGuild guild)
		{
			m_dbConnection = new SQLiteConnection(@"Data Source=DB\bot.sqlite;Version=3;");
			m_dbConnection.Open();

			string sql;
			SQLiteCommand command;

			sql = "INSERT INTO autoalerts (value, guildid) VALUES ('false', '" + guild.Id + "')";
			command = new SQLiteCommand(sql, m_dbConnection);
			command.ExecuteNonQuery();

			return null;
		}

		private Task LeftGuild(SocketGuild guild)
		{
			m_dbConnection = new SQLiteConnection(@"Data Source=DB\bot.sqlite;Version=3;");
			m_dbConnection.Open();

			string sql;
			SQLiteCommand command;

			sql = "DELETE FROM autoalerts WHERE guildid = '" + guild.Id + "'";
			command = new SQLiteCommand(sql, m_dbConnection);
			command.ExecuteNonQuery();

			return null;
		}

		public async void TimerElapsed(object sender, ElapsedEventArgs e)
		{
			if (client.ConnectionState == ConnectionState.Connected)
			{
				//Console.WriteLine("FIRED!");
				log.Debug("Timer has elapsed - firing checks");
				m_dbConnection = new SQLiteConnection(@"Data Source=DB\bot.sqlite;Version=3;");


				string sql;
				SQLiteCommand command;

				List<string> tablenames = new List<string>();

				await CheckWowhead();

				await CheckEmissaries2();
				await CheckMenagerie2();
				await CheckVHBosses2();
				await CheckDailyReset2();
				await CheckWowToken2();
				await CheckWorldBosses2();
				await CheckWorldEvents2();
				await CheckXurios2();
				await CheckAffixes2();
				await CheckInvasions2();
				await CheckBrokenShoreBuildings2();

				log.Debug("Checking complete!");
				log.Debug("Start sending auto alerts");

				m_dbConnection.Open();

				sql = "SELECT tablename FROM tablestoalert";
				command = new SQLiteCommand(sql, m_dbConnection);
				SQLiteDataReader readtablenames = command.ExecuteReader();

				while (readtablenames.Read())
				{
					tablenames.Add(readtablenames["tablename"].ToString());
					//Console.WriteLine(readtablenames["tablename"]);
				}

				sql = "SELECT value, guildid, channelid FROM autoalerts WHERE value = 'true'";
				command = new SQLiteCommand(sql, m_dbConnection);
				SQLiteDataReader reader = command.ExecuteReader();

				while (reader.Read())
				{
					Console.WriteLine("READ!");
					log.Debug("Reading which guilds and channels to send alerts to from DB");
					foreach (string table in tablenames)
					{
						log.Debug("There was a change in the " + table + " table - alerting!");
						await client.GetGuild(Convert.ToUInt64(reader["guildid"])).GetTextChannel(Convert.ToUInt64(reader["channelid"])).SendMessageAsync("There was a change in " + table);
					}
				}

				log.Debug("Alerts completed! Cleaning up...");

				sql = "DELETE FROM tablestoalert";
				command = new SQLiteCommand(sql, m_dbConnection);
				command.ExecuteNonQuery();

				m_dbConnection.Close();
			}
		}

#region Old Check Methods
		public async Task CheckEmissaries()
		{
			//util.DownloadNewWowHead();

			m_dbConnection = new SQLiteConnection(@"Data Source=DB\bot.sqlite;Version=3;");
			m_dbConnection.Open();

			string sql;
			SQLiteCommand command;

			List<string> emissariesList = new List<string>();
			List<string> timeLeftList = new List<string>();

			List<string> dbEmissariesList = new List<string>();
			List<string> dbTimeLeftList = new List<string>();

			List<string> emissariesAndTimesList = util.ReturnAllLines("US-emissary", "test.txt");

			sql = "SELECT name, timeleft FROM emissaries";
			command = new SQLiteCommand(sql, m_dbConnection);

			SQLiteDataReader reader = command.ExecuteReader();

			while (reader.Read())
			{
				dbEmissariesList.Add(reader["name"].ToString());
				dbTimeLeftList.Add(reader["timeleft"].ToString());
			}


			int i = 1;
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
			}
			//string[] timeLeft = { util.GetLine("\'US--1\'", "test.txt"), util.GetLine("\'US--2\'", "test.txt"), util.GetLine("\'US--3\'", "test.txt") };

			string[] emissaries = emissariesList.ToArray();
			string[] timeLeft = timeLeftList.ToArray();

			//string msg = "Current active emissaries are:\n\n";

			int j = 0;

			foreach (string emissary in emissaries)
			{
				Console.WriteLine(emissary);
				string[] words = emissary.Split('>');
				emissaries[j] = words[1].TrimEnd('<', '/', 'a');


				j++;
			}

			Console.WriteLine(DateTime.Now + " - Updating emissaries...");

			int k = 0;

			sql = "DELETE FROM emissaries";
			command = new SQLiteCommand(sql, m_dbConnection);
			command.ExecuteNonQuery();
			//goto

			foreach (string time in timeLeft)
			{
				sql = "SELECT name, timeleft FROM emissaries WHERE id = " + k;
				command = new SQLiteCommand(sql, m_dbConnection);

				string[] numbers = time.Split(',');
				timeLeft[k] = numbers[1].TrimStart('"', ' ').TrimEnd('"').Replace("hr", "hours").Replace("min", "minutes").Replace("day", "days");

				/*if (emissariesList[k] != dbEmissariesList[k] || timeLeftList[k] != dbTimeLeftList[k])
				{
					//sql = "DELETE * FROM emissaries WHERE id = '" + k + "'";
					//command = new SQLiteCommand(sql, m_dbConnection);
					//command.ExecuteNonQuery();
				}
				*/

				sql = "INSERT INTO emissaries (name, timeleft) VALUES ('" + emissaries[k] + "', '" + timeLeft[k] + "')";
				command = new SQLiteCommand(sql, m_dbConnection);
				command.ExecuteNonQuery();

				//msg += "**" + emissaries[k] + "** - __" + timeLeft[k] + "__ remaining to complete\n\n";
				k++;
			}

			int l = 0;

			foreach (string emissary in dbEmissariesList)
			{
				if (emissary != emissaries[l] || dbEmissariesList.Count != emissaries.Length)
				{
					sql = "INSERT INTO tablestoalert (tablename) VALUES ('emissaries')";
					command = new SQLiteCommand(sql, m_dbConnection);
					command.ExecuteNonQuery();
					break;
				}
				l++;
			}

			m_dbConnection.Close();
		}

		public async Task CheckMenagerie()
		{
			//Utilities util = new Utilities();
			//util.DownloadNewWowHead();

			m_dbConnection = new SQLiteConnection(@"Data Source=DB\bot.sqlite;Version=3;");
			m_dbConnection.Open();

			string sql;
			SQLiteCommand command;

			List<string> dbPetNameList = new List<string>();

			sql = "SELECT petname FROM menagerie";
			command = new SQLiteCommand(sql, m_dbConnection);

			SQLiteDataReader reader = command.ExecuteReader();

			while (reader.Read())
			{
				dbPetNameList.Add(reader["petname"].ToString());
			}

			List<string> petsList = util.ReturnAllLines("US-menagerie-", "test.txt");
			Console.WriteLine(petsList[0]);
			//string[] pets = { util.GetLine("US-menagerie-1", "test.txt"), util.GetLine("US-menagerie-2", "test.txt"), util.GetLine("US-menagerie-3", "test.txt") };

			string[] pets = petsList.ToArray();

			string msg = "Pets currently active in your Garrison's Managerie this week are:\n\n";

			Console.WriteLine(DateTime.Now + " - Updating menagerie...");

			sql = "DELETE FROM menagerie";
			command = new SQLiteCommand(sql, m_dbConnection);
			command.ExecuteNonQuery();

			int i = 0;

			foreach (string pet in pets)
			{
				string[] words = pet.Split('>');
				pets[i] = words[2].TrimStart(' ').Replace("</a", "");

				sql = "INSERT INTO menagerie (petname) VALUES ('" + pets[i].Replace("'", "''") + "')";
				command = new SQLiteCommand(sql, m_dbConnection);
				command.ExecuteNonQuery();


				msg += "**" + pets[i] + "**\n\n";



				i++;
			}

			int j = 0;

			foreach (string petname in dbPetNameList)
			{
				if (petname != pets[j] || dbPetNameList.Count != pets.Length)
				{
					sql = "INSERT INTO tablestoalert (tablename) VALUES ('menagerie')";
					command = new SQLiteCommand(sql, m_dbConnection);
					command.ExecuteNonQuery();
					break;
				}
				j++;
			}

			m_dbConnection.Close();

			//Console.WriteLine(pets[0]);

			//msg = "Pets currently active in your Garrison's Managerie this week are:\n\n**" + pets[0] + "**\n\n**" + pets[1] + "**\n\n**" + pets[2] + "**";

			//await Context.Channel.SendMessageAsync(msg);
		}

		public async Task CheckVHBosses()
		{
			m_dbConnection = new SQLiteConnection(@"Data Source=DB\bot.sqlite;Version=3;");
			m_dbConnection.Open();

			string sql;
			SQLiteCommand command;

			List<string> dbVhBossesList = new List<string>();

			sql = "SELECT bossname FROM vhbosses";
			command = new SQLiteCommand(sql, m_dbConnection);

			SQLiteDataReader reader = command.ExecuteReader();

			while (reader.Read())
			{
				dbVhBossesList.Add(reader["bossname"].ToString());
			}

			string[] vhBosses = { util.GetLine("US-violethold-1", "test.txt"), util.GetLine("US-violethold-2", "test.txt"), util.GetLine("US-violethold-3", "test.txt") }; //need to update to find all!

			Console.WriteLine(DateTime.Now + " - Updating Violet Hold Bosses...");

			sql = "DELETE FROM vhbosses";
			command = new SQLiteCommand(sql, m_dbConnection);
			command.ExecuteNonQuery();

			int i = 0;

			foreach (string boss in vhBosses)
			{
				if (boss != "No results found...")
				{
					string[] words = boss.Split('>');
					vhBosses[i] = words[1].Replace("</a", "");

					sql = "INSERT INTO vhbosses (bossname) VALUES ('" + vhBosses[i].Replace("'", "''") + "')";
					command = new SQLiteCommand(sql, m_dbConnection);
					command.ExecuteNonQuery();

					i++;
				}
			}

			//msg = "Current bosses active in the Violet Hold this week are:\n\n**" + vhBosses[0] + "**\n\n**" + vhBosses[1] + "**\n\n**" + vhBosses[2] + "**";

			//await Context.Channel.SendMessageAsync(msg);

			int j = 0;

			foreach (string boss in dbVhBossesList)
			{
				if (boss != vhBosses[j] || dbVhBossesList.Count != vhBosses.Length)
				{
					sql = "INSERT INTO tablestoalert (tablename) VALUES ('vhbosses')";
					command = new SQLiteCommand(sql, m_dbConnection);
					command.ExecuteNonQuery();
					break;
				}
				j++;
			}

			m_dbConnection.Close();
		}

		public async Task CheckDailyReset()
		{
			//Utilities util = new Utilities();
			//util.DownloadNewWowHead();

			m_dbConnection = new SQLiteConnection(@"Data Source=DB\bot.sqlite;Version=3;");
			m_dbConnection.Open();

			string sql;
			SQLiteCommand command;

			List<string> dbDailyResetList = new List<string>();

			sql = "SELECT time FROM dailyreset";
			command = new SQLiteCommand(sql, m_dbConnection);

			SQLiteDataReader reader = command.ExecuteReader();

			while (reader.Read())
			{
				dbDailyResetList.Add(reader["time"].ToString());
			}

			string msg = "";

			long epochTimeNow = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

			Console.WriteLine(DateTime.Now + " - Updating daily reset...");

			sql = "DELETE FROM dailyreset";
			command = new SQLiteCommand(sql, m_dbConnection);
			command.ExecuteNonQuery();

			string dailyReset = util.GetLine("tiw-timer-US", "test.txt");
			//Console.WriteLine(dailyReset);
			string[] words = dailyReset.Split('"');
			dailyReset = words[5];
			string dailyResetEpochTime = dailyReset;

			sql = "INSERT INTO dailyreset (time) VALUES ('" + dailyReset + "')";
			command = new SQLiteCommand(sql, m_dbConnection);
			command.ExecuteNonQuery();

			sql = "SELECT time FROM dailyreset";
			command = new SQLiteCommand(sql, m_dbConnection);
			reader = command.ExecuteReader();

			long epochTimeLeft = 0;

			while (reader.Read())
			{
				epochTimeLeft = Convert.ToInt64(reader["time"]) - epochTimeNow;

				dailyReset = util.ConvertUnixEpochTime(Convert.ToInt64(reader["time"])).ToString();
			}
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

			sql = "UPDATE dailyreset SET timeleft = '" + hoursLeft + minutesLeft + " minutes' WHERE id = 1";
			command = new SQLiteCommand(sql, m_dbConnection);
			command.ExecuteNonQuery();

			//Console.WriteLine(dailyReset);
			//Console.WriteLine(timeNow);

			msg = "There is **" + hoursLeft + minutesLeft + " minutes** remaining until dailies are reset.\n\n Dailies reset at **" + dailyReset + "**.";

			if (dbDailyResetList[0] != dailyResetEpochTime)
			{
				sql = "INSERT INTO tablestoalert (tablename) VALUES ('dailyreset')";
				command = new SQLiteCommand(sql, m_dbConnection);
				command.ExecuteNonQuery();
			}

			//await Context.Channel.SendMessageAsync(msg);
		}

		public async Task CheckWowToken()
		{
			//Utilities util = new Utilities();
			//util.DownloadNewWowHead();

			m_dbConnection = new SQLiteConnection(@"Data Source=DB\bot.sqlite;Version=3;");
			m_dbConnection.Open();

			string sql;
			SQLiteCommand command;

			//string msg = "";

			string tokenGold = util.GetLine("tiw-group-wowtoken", "test.txt");

			//Console.WriteLine(tokenGold);

			Console.WriteLine(DateTime.Now + " - Updating wow token...");

			sql = "DELETE FROM wowtoken";
			command = new SQLiteCommand(sql, m_dbConnection);
			command.ExecuteNonQuery();

			string[] words = tokenGold.Split('>');
			tokenGold = words[9].Replace("</span", "");

			sql = "INSERT INTO wowtoken (price) VALUES ('" + tokenGold.Replace("'", "''") + "')";
			command = new SQLiteCommand(sql, m_dbConnection);
			command.ExecuteNonQuery();
			//Console.WriteLine(tokenGold);

			//msg = "WoW Tokens are currently worth **" + tokenGold + "** gold.";

			//await Context.Channel.SendMessageAsync(msg);
		}

		public async Task CheckWorldBosses()
		{
			//Utilities util = new Utilities();
			//util.DownloadNewWowHead();

			//string msg = "Below are the current World Bosses that are active this week:\n\n";
			m_dbConnection = new SQLiteConnection(@"Data Source=DB\bot.sqlite;Version=3;");
			m_dbConnection.Open();

			string sql;
			SQLiteCommand command;

			List<string> dbWorldBossesList = new List<string>();

			sql = "SELECT bossname FROM worldbosses";
			command = new SQLiteCommand(sql, m_dbConnection);

			SQLiteDataReader reader = command.ExecuteReader();

			while (reader.Read())
			{
				dbWorldBossesList.Add(reader["bossname"].ToString());
			}

			List<string> bosses = util.ReturnAllLines("US-epiceliteworld-", "test.txt");

			string[] bossesArray = bosses.ToArray();

			Console.WriteLine(DateTime.Now + " - Updating world bosses...");

			sql = "DELETE FROM worldbosses";
			command = new SQLiteCommand(sql, m_dbConnection);
			command.ExecuteNonQuery();

			int i = 0;

			foreach (string boss in bossesArray)
			{
				string[] words = boss.Split('>');
				bossesArray[i] = words[1].Replace("</a", "");
				//msg += "**" + bossesArray[i] + "**\n\n";

				sql = "INSERT INTO worldbosses (bossname) VALUES ('" + bossesArray[i].Replace("'", "''") + "')";
				command = new SQLiteCommand(sql, m_dbConnection);
				command.ExecuteNonQuery();

				i++;
			}

			int j = 0;

			foreach (string bossname in dbWorldBossesList)
			{
				if (bossname != bossesArray[j] || dbWorldBossesList.Count != bossesArray.Length)
				{
					sql = "INSERT INTO tablestoalert (tablename) VALUES ('worldbosses')";
					command = new SQLiteCommand(sql, m_dbConnection);
					command.ExecuteNonQuery();
					break;
				}
				j++;
			}
			//msg.TrimEnd('\n');

			//Console.WriteLine(msg);

			//await Context.Channel.SendMessageAsync(msg);
		}

		public async Task CheckWorldEvents()
		{
			//Utilities util = new Utilities();
			//util.DownloadNewWowHead();

			//string msg = "Below are the current World Events that are active this week:\n\n";

			m_dbConnection = new SQLiteConnection(@"Data Source=DB\bot.sqlite;Version=3;");
			m_dbConnection.Open();

			string sql;
			SQLiteCommand command;

			List<string> dbWorldEvents = new List<string>();

			sql = "SELECT eventname FROM worldevents";
			command = new SQLiteCommand(sql, m_dbConnection);

			SQLiteDataReader reader = command.ExecuteReader();

			while (reader.Read())
			{
				dbWorldEvents.Add(reader["eventname"].ToString());
			}

			List<string> worldEvents = util.ReturnAllLines("US-holiday-", "test.txt");

			string[] worldEventsArray = worldEvents.ToArray();

			Console.WriteLine(DateTime.Now + " - Updating world events...");

			sql = "DELETE FROM worldevents";
			command = new SQLiteCommand(sql, m_dbConnection);
			command.ExecuteNonQuery();

			int i = 0;

			foreach (string worldEvent in worldEventsArray)
			{
				string[] words = worldEvent.Split('>');
				worldEventsArray[i] = words[2].TrimStart(' ').Replace("</a", "");
				//msg += "**" + worldEventsArray[i] + "**\n\n";

				sql = "INSERT INTO worldevents (eventname) VALUES ('" + worldEventsArray[i].Replace("'", "''") + "')";
				command = new SQLiteCommand(sql, m_dbConnection);
				command.ExecuteNonQuery();

				i++;
			}

			int j = 0;

			foreach (string worldevent in dbWorldEvents)
			{
				if (worldevent != worldEventsArray[j] || dbWorldEvents.Count != worldEventsArray.Length)
				{
					sql = "INSERT INTO tablestoalert (tablename) VALUES ('worldevents')";
					command = new SQLiteCommand(sql, m_dbConnection);
					command.ExecuteNonQuery();
					break;
				}
				j++;
			}

			//msg.TrimEnd('\n');

			//await Context.Channel.SendMessageAsync(msg);
		}

		public async Task CheckXurios()
		{
			//Utilities util = new Utilities();
			//util.DownloadNewWowHead();

			//string msg = "This week Xur\'ios is selling the following for Curious Coins:\n\n";

			m_dbConnection = new SQLiteConnection(@"Data Source=DB\bot.sqlite;Version=3;");
			m_dbConnection.Open();

			string sql;
			SQLiteCommand command;

			List<string> dbXurios = new List<string>();

			sql = "SELECT itemname FROM xurios";
			command = new SQLiteCommand(sql, m_dbConnection);

			SQLiteDataReader reader = command.ExecuteReader();

			while (reader.Read())
			{
				dbXurios.Add(reader["itemname"].ToString());
			}

			List<string> xurios = util.ReturnAllLines("US-xurios-", "test.txt");

			string[] xuriosArray = xurios.ToArray();

			Console.WriteLine(DateTime.Now + " - Updating Xur\'ios...");

			sql = "DELETE FROM xurios";
			command = new SQLiteCommand(sql, m_dbConnection);
			command.ExecuteNonQuery();

			int i = 0;

			foreach (string item in xuriosArray)
			{
				string[] words = item.Split('>');
				xuriosArray[i] = words[2].TrimStart(' ').Replace("</a", "");
				//if (xuriosArray[i] != "")
				//{
				//msg += "**" + xuriosArray[i] + "**\n\n";
				//}

				sql = "INSERT INTO xurios (itemname) VALUES ('" + xuriosArray[i].Replace("'", "''") + "')";
				command = new SQLiteCommand(sql, m_dbConnection);
				command.ExecuteNonQuery();

				i++;
			}

			int j = 0;

			foreach (string item in dbXurios)
			{
				if (item != xuriosArray[j] || dbXurios.Count != xuriosArray.Length)
				{
					sql = "INSERT INTO tablestoalert (tablename) VALUES ('xurios')";
					command = new SQLiteCommand(sql, m_dbConnection);
					command.ExecuteNonQuery();
					break;
				}
				j++;
			}
		}

		public async Task CheckAffixes()
		{
			//Utilities util = new Utilities();
			//util.DownloadNewWowHead();

			m_dbConnection = new SQLiteConnection(@"Data Source=DB\bot.sqlite;Version=3;");
			m_dbConnection.Open();

			string sql;
			SQLiteCommand command;

			List<string> dbAffixes = new List<string>();

			sql = "SELECT affixname FROM dungeonaffixes";
			command = new SQLiteCommand(sql, m_dbConnection);

			SQLiteDataReader reader = command.ExecuteReader();

			while (reader.Read())
			{
				dbAffixes.Add(reader["affixname"].ToString());
			}

			List<string> mythicAffixesList = util.ReturnAllLines("US-mythicaffix-", "test.txt");
			string[] mythicAffixes = mythicAffixesList.ToArray();
			//string[] mythicAffixes = { util.GetLine("id=\"US-mythicaffix-1\"", "test.txt").TrimStart(), util.GetLine("id=\"US-mythicaffix-2\"", "test.txt").TrimStart(), util.GetLine("id=\"US-mythicaffix-3\"", "test.txt").TrimStart() };
			int[] affixNumbers = { 0, 0, 0 };
			//string msg = "This week's Mythic+ Dungeon Affixes are:\n\n";
			//Console.WriteLine(mythicAffixes[1]);

			Console.WriteLine(DateTime.Now + " - Updating mythic+ dungeon affixes...");

			sql = "DELETE FROM dungeonaffixes";
			command = new SQLiteCommand(sql, m_dbConnection);
			command.ExecuteNonQuery();

			int i = 0;
			int j = 1;

			foreach (string affix in mythicAffixes)
			{
				string[] words = affix.Split('>');
				//string[] cleanedWords = words[6].Split('<');

				string[] numbers = words[0].Split('=');
				//int cleanedNumber = Convert.ToInt32(numbers[2].TrimEnd('"'));

				//Console.WriteLine(mythicAffixes[i]);
				mythicAffixes[i] = words[2].Replace(" ", "").Replace("</a", "");
				affixNumbers[i] = Convert.ToInt32(numbers[2].Replace("\" id", ""));

				//Console.WriteLine(mythicAffixes[i]);
				//Console.WriteLine(affixNumbers[i]);

				j += 3;

				sql = "INSERT INTO dungeonaffixes (affixname, affixnum, dungeonlevel) VALUES ('" + mythicAffixes[i].Replace("'", "''") + "', '" + affixNumbers[i] + "', '" + j + "')";
				command = new SQLiteCommand(sql, m_dbConnection);
				command.ExecuteNonQuery();

				//msg += "**" + mythicAffixes[i] + "** - Mythic Keystone Level " + j + "+\nMore Info: <http://www.wowhead.com/affix=" + affixNumbers[i] + ">\n\n";

				i++;
			}

			int k = 0;

			foreach (string affix in dbAffixes)
			{
				if (affix != mythicAffixes[k] || dbAffixes.Count != mythicAffixes.Length)
				{
					sql = "INSERT INTO tablestoalert (tablename) VALUES ('dungeonaffixes')";
					command = new SQLiteCommand(sql, m_dbConnection);
					command.ExecuteNonQuery();
					break;
				}
				k++;
			}


			//Console.WriteLine(mythicAffixes[0] + " " + affixNumbers[0]);
			//Console.WriteLine(mythicAffixes[1] + " " + affixNumbers[1]);
			//Console.WriteLine(mythicAffixes[2] + " " + affixNumbers[2]);

			//msg = "This week's Mythic+ Dungeon Affixes are:\n\n**" + mythicAffixes[0] + "** - Mythic Keystone Level 4+\nMore Info: <http://www.wowhead.com/affix=" + affixNumbers[0] + ">\n\n**" + mythicAffixes[1]; //+ "** - Mythic Keystone Level 7+\nMore Info: <http://www.wowhead.com/affix=" + affixNumbers[1] + ">\n\n**" + mythicAffixes[2] + "** - Mythic Keystone Level 10+\nMore Info: <http://www.wowhead.com/affix=" + affixNumbers[2] + ">";
			//await Context.Channel.SendMessageAsync(msg);
		}

		public async Task CheckInvasions()
		{
			//Utilities util = new Utilities();
			//util.DownloadNewWowHead();

			m_dbConnection = new SQLiteConnection(@"Data Source=DB\bot.sqlite;Version=3;");
			m_dbConnection.Open();

			string sql;
			SQLiteCommand command;

			List<string> dbAssaults = new List<string>();

			sql = "SELECT zonename FROM legionassaults";
			command = new SQLiteCommand(sql, m_dbConnection);

			SQLiteDataReader reader = command.ExecuteReader();

			while (reader.Read())
			{
				dbAssaults.Add(reader["zonename"].ToString());
			}

			string legionAssaultsLine = util.GetLine("<script>$WH.news.addAssaultDisplay(\"US\", {\"id\":\"legion-assaults\"", "test.txt");
			//string msg = "";

			Console.WriteLine(legionAssaultsLine);

			string[] words = legionAssaultsLine.Split(':');

			string[] upcomingAssaultsStr = words[13].TrimStart('[').Replace("],\"length\"", "").Split(',');
			int length = Convert.ToInt32(words[14].Replace(" ", "").Replace("});</script></div>", ""));
			string zoneName = words[12].Replace("\"", "").Replace(",upcoming", "");

			//Console.WriteLine(upcomingAssaultsStr[0]);
			//Console.WriteLine(length);

			List<long> upcomingTimesEpoch = new List<long>();

			int i = 0;

			foreach (string time in upcomingAssaultsStr)
			{
				upcomingTimesEpoch.Add(Convert.ToInt64(time));

				//Console.WriteLine(time);
			}

			long epochTimeNow = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

			//Console.WriteLine(length);

			Console.WriteLine(DateTime.Now + " - Updating legion invasions...");

			sql = "DELETE FROM legionassaults";
			command = new SQLiteCommand(sql, m_dbConnection);
			command.ExecuteNonQuery();

			string hoursLeft = "";
			string minutesLeft = "";
			string zone = "";

			int doOnce = 0;

			foreach (long epochTime in upcomingTimesEpoch)
			{
				long tempEpochTime = epochTime + (length);

				if (epochTimeNow < tempEpochTime)
				{
					if (epochTimeNow > epochTime)
					{
						/*long epochTimeLeft = (tempEpochTime - epochTimeNow);
						TimeSpan t = TimeSpan.FromSeconds(epochTimeLeft);
						hoursLeft = t.ToString(@"hh");
						minutesLeft = t.ToString(@"mm").TrimStart('0');
						if (hoursLeft != "00")
						{
							hoursLeft = hoursLeft.TrimStart('0') + " hours and ";
						}
						else
						{
							hoursLeft = "";
						} */
						//zone = "";
						/*
						if (zoneName != "")
						{
							zone = zoneName;
						}
						else
						{
							zone = "A zone";
						}
						*/


						//msg = "__**" + zone + "**__** is currently being assaulted by the Legion!**__**\n" + hoursLeft + minutesLeft + " minutes**__** remaining...**\n";
						//Console.WriteLine(zone + " is being assaulted by the legion! " + hoursLeft + " hours and " + minutesLeft + " minutes remaining...");
					}
					else
					{
						while (doOnce < 1)
						{
							//msg += "\n\nThe next invasion times are as follows: \n";
							doOnce++;
						}



						//msg += "\n" + util.ConvertUnixEpochTime(epochTime).ToString() + "\n";
						//Console.WriteLine(util.ConvertUnixEpochTime(epochTime));
					}

				}
				sql = "INSERT INTO legionassaults (assaulttime, timeleft, length) VALUES ('" + epochTime + "', '" + hoursLeft + minutesLeft + "', '" + length + "')";
				command = new SQLiteCommand(sql, m_dbConnection);
				command.ExecuteNonQuery();


			}

			int j = 1;

			foreach (long epochTime in upcomingTimesEpoch)
			{
				long tempEpochTime = epochTime + (length);

				if (epochTimeNow < tempEpochTime)
				{
					if (epochTimeNow > epochTime)
					{
						//zone = "";
						if (zoneName != "")
						{
							zone = zoneName;

						}
						else
						{
							zone = "A zone";

						}
						long epochTimeLeft = (tempEpochTime - epochTimeNow);
						TimeSpan t = TimeSpan.FromSeconds(epochTimeLeft);
						hoursLeft = t.ToString(@"hh");
						minutesLeft = t.ToString(@"mm").TrimStart('0');
						if (hoursLeft != "00")
						{
							hoursLeft = hoursLeft.TrimStart('0') + " hours and ";
						}
						else
						{
							hoursLeft = "";
						}

					}

					//goto NextEpochTime;
				}
				if (j < upcomingTimesEpoch.Count && epochTimeNow > epochTime && epochTimeNow < tempEpochTime)
				{

					Console.WriteLine(zone);
					Console.WriteLine(j);
					sql = "UPDATE legionassaults SET zonename = '" + zone.Replace("'", "''") + "', timeleft = '" + hoursLeft + minutesLeft + "' WHERE id = " + j;
					command = new SQLiteCommand(sql, m_dbConnection);
					command.ExecuteNonQuery();

				}

				//goto NextEpochTime;s
				//NextEpochTime:
				j++;
			}
			//await Context.Channel.SendMessageAsync(msg);

			int k = 0;

			foreach (string name in dbAssaults)
			{
				if (name != null && name != "" && name != zoneName)
				{
					sql = "INSERT INTO tablestoalert (tablename) VALUES ('legionassaults')";
					command = new SQLiteCommand(sql, m_dbConnection);
					command.ExecuteNonQuery();
					break;
				}
				k++;
			}

		}

		public async Task CheckBrokenShoreBuildings()
		{
			//Utilities util = new Utilities();
			//util.DownloadNewWowHead();

			//string[] buildings = { util.GetLineAfterNum("data-region=\"US\" data-building=\"1\"", "test.txt", 10), util.GetLineAfterNum("data-region=\"US\" data-building=\"2\"", "test.txt", 10), util.GetLineAfterNum("data-region=\"US\" data-building=\"3\"", "test.txt", 10) };
			//string[] buildingStates = { util.GetLineAfterNum("data-region=\"US\" data-building=\"1\"", "test.txt", 8), util.GetLineAfterNum("data-region=\"US\" data-building=\"2\"", "test.txt", 8), util.GetLineAfterNum("data-region=\"US\" data-building=\"3\"", "test.txt", 8) };

			m_dbConnection = new SQLiteConnection(@"Data Source=DB\bot.sqlite;Version=3;");
			m_dbConnection.Open();

			string sql;
			SQLiteCommand command;

			List<string> dbBrokenShore = new List<string>();

			sql = "SELECT buildingstate FROM brokenshorebuildings";
			command = new SQLiteCommand(sql, m_dbConnection);

			SQLiteDataReader reader = command.ExecuteReader();

			while (reader.Read())
			{
				dbBrokenShore.Add(reader["buildingstate"].ToString());
			}

			string[] buildings = { util.BrokenShoreBuildingGetLine("data-region=\"US\" data-building=\"1\"", "class=\"tiw-bs-status-progress", "test.txt"), util.BrokenShoreBuildingGetLine("data-region=\"US\" data-building=\"2\"", "class=\"tiw-bs-status-progress", "test.txt"), util.BrokenShoreBuildingGetLine("data-region=\"US\" data-building=\"3\"", "class=\"tiw-bs-status-progress", "test.txt") };
			string[] buildingStates = { util.BrokenShoreBuildingGetLine("data-region=\"US\" data-building=\"1\"", "class=\"tiw-bs-status-state", "test.txt"), util.BrokenShoreBuildingGetLine("data-region=\"US\" data-building=\"2\"", "class=\"tiw-bs-status-state", "test.txt"), util.BrokenShoreBuildingGetLine("data-region=\"US\" data-building=\"3\"", "class=\"tiw-bs-status-state", "test.txt") };

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

			Console.WriteLine(DateTime.Now + " - Updating Broken Shore buildings...");

			sql = "DELETE FROM brokenshorebuildings";
			command = new SQLiteCommand(sql, m_dbConnection);
			command.ExecuteNonQuery();

			string buildingName = "";

			int i = 0;

			foreach (string building in buildings)
			{
				switch (i + 1)
				{
					case 1:
						buildingName = "Mage Tower";
						break;
					case 2:
						buildingName = "Command Center";
						break;
					case 3:
						buildingName = "Nether Disruptor";
						break;
				}

				string[] words = building.Split('>');

				Console.WriteLine(buildings[i]);
				buildings[i] = words[4].Replace("</span", "");

				sql = "INSERT INTO brokenshorebuildings (buildingname, buildingpercentage) VALUES ('" + buildingName + "', '" + buildings[i] + "')";
				command = new SQLiteCommand(sql, m_dbConnection);
				command.ExecuteNonQuery();

				i++;
			}

			int j = 0;

			foreach (string buildingState in buildingStates)
			{
				string[] words = buildingState.Split('>');
				buildingStates[j] = words[2].Replace("</div", "");

				int id = j + 1;

				sql = "UPDATE brokenshorebuildings SET buildingstate = '" + buildingStates[j] + "' WHERE id = '" + id + "'";
				command = new SQLiteCommand(sql, m_dbConnection);
				command.ExecuteNonQuery();

				j++;
			}

			/*
			Console.WriteLine(buildings[0]);
			Console.WriteLine(buildingStates[0]);
			Console.WriteLine(buildings[1]);
			Console.WriteLine(buildingStates[1]);
			Console.WriteLine(buildings[2]);
			Console.WriteLine(buildingStates[2]);
			*/
			//msg = "Current Broken Shore buildings stats are as follows: \n\n__**Mage Tower**__\n" + buildingStates[0] + "\n" + buildings[0] + "\n\n__**Command Center**__\n" + buildingStates[1] + "\n" + buildings[1] + "\n\n__**Nether Disruptor**__\n" + buildingStates[2] + "\n" + buildings[2];

			//await Context.Channel.SendMessageAsync(msg);
			int k = 0;

			foreach (string state in dbBrokenShore)
			{
				if (state != buildingStates[k])
				{
					sql = "INSERT INTO tablestoalert (tablename) VALUES ('brokenshorebuildings')";
					command = new SQLiteCommand(sql, m_dbConnection);
					command.ExecuteNonQuery();
					break;
				}
				k++;
			}
		}

#endregion

		public List<string> GetInnerText(string xpath, int doOnce = 0)
		{
			var doc = new HtmlDocument();
			doc.Load("test.txt");

			var aTags = doc.DocumentNode.SelectNodes(xpath);

			List<string> list = new List<string>();

			if (aTags != null)
			{
				foreach (var aTag in aTags)
				{
					if (aTag.InnerText != "")
					{
						log.Info(aTag.InnerText);
						list.Add(aTag.InnerText);
					}
					if (doOnce != 0) break;
				}
			}

			//List<string> list = aTags.ToList();

			return list;
		}

		public List<string> GetAttributeValue(string xpath, string attribute)
		{
			var doc = new HtmlDocument();
			doc.Load("test.txt");

			var aTags = doc.DocumentNode.SelectNodes(xpath);

			List<string> list = new List<string>();

			foreach (var aTag in aTags)
			{
				if (aTag != null)
				{
					log.Info(aTag.Attributes[attribute].Value);
					list.Add(aTag.Attributes[attribute].Value);
				}
			}
			return list;
		}

		public async Task CheckWithHapOLD(string htmlTag, string attributeA, string attributeB, string value, string attributeC, int number = 0)
		{
			var doc = new HtmlDocument();
			doc.Load("test.txt");

			var aTags = doc.DocumentNode.SelectNodes("//" + htmlTag + "[contains(@" + attributeA + ",'" + value + "')]");

			if (aTags != null)
			{
				foreach (var aTag in aTags)	
				{
					if (aTag.InnerText != "")
					{
						log.Info(aTag.InnerText);
						if (number != 0) break;
					}

					if (attributeB != null)
					{
						log.Info(aTag.Attributes[attributeB].Value);
					}
				}

				if (attributeC != null)
				{
					aTags = doc.DocumentNode.SelectNodes("//" + htmlTag + "[contains(@" + attributeA + ",'" + value + "')]/../script");

					foreach (var aTag in aTags)
					{
						log.Info(aTag.InnerText);
					}
				}
			}

			return;

		}

		public async Task CheckWithHapAttribute(string htmlTag, string attributeA, string attributeB, string value = null)
		{
			var doc = new HtmlDocument();
			doc.Load("test.txt");

			var aTags = doc.DocumentNode.SelectNodes("//" + htmlTag + "[contains(@" + attributeA + ",'" + value + "')]");

			if (aTags != null)
			{
				foreach (var aTag in aTags)
				{
					//if (aTag.Attributes["id"].Value == "US-emissary-1")
					//{
					log.Info(aTag.Attributes[attributeB].Value);
					//log.Info(aTag.InnerText);
					//}
				}
			}
			else
			{
				log.Info("TAG IS NULL");
			}
		}

		public List<string> GetDBResults(string column, string table)
		{
			m_dbConnection = new SQLiteConnection(@"Data Source=DB\bot.sqlite;Version=3;"); //Make global
			m_dbConnection.Open();

			string sql; //make global
			SQLiteCommand command; //make global

			List<string> dbResults = new List<string>();

			sql = "SELECT " + column + " FROM " + table;
			command = new SQLiteCommand(sql, m_dbConnection);

			SQLiteDataReader reader = command.ExecuteReader();

			while (reader.Read())
			{
				dbResults.Add(reader[column].ToString());
			}

			m_dbConnection.Close();

			return dbResults;
		}

		public void DBDropTable(string table)
		{
			m_dbConnection = new SQLiteConnection(@"Data Source=DB\bot.sqlite;Version=3;"); //Make global
			m_dbConnection.Open();

			string sql; //make global
			SQLiteCommand command; //make global

			sql = "DELETE FROM " + table;
			command = new SQLiteCommand(sql, m_dbConnection);
			command.ExecuteNonQuery();

			m_dbConnection.Close();
		}

		public void InsertToDB(string columnOne, string valueOne, string columnTwo = null, string valueTwo = null, string columnThree = null, string valueThree = null, string table = null)
		{
			m_dbConnection = new SQLiteConnection(@"Data Source=DB\bot.sqlite;Version=3;"); //Make global
			m_dbConnection.Open();

			string sql; //make global
			SQLiteCommand command; //make global

			string columns = columnOne;
			string values = "'" + valueOne + "'";

			if (columnTwo != null)
			{
				columns += ", " + columnTwo;
				values += ", '" + valueTwo + "'";
			}

			if (columnThree != null)
			{
				columns += ", " + columnThree;
				values += ", '" + valueThree + "'";
			}

			sql = "INSERT INTO " + table + " (" + columns + ") VALUES (" + values + ")";

			command = new SQLiteCommand(sql, m_dbConnection);
			command.ExecuteNonQuery();

			m_dbConnection.Close();
		}

		public void UpdateDBRecord(string columnOne, string valueOne, string columnTwo = null, string valueTwo = null, string columnThree = null, string valueThree = null, string whereColumn = null, string whereValue = null, string table = null)
		{
			m_dbConnection = new SQLiteConnection(@"Data Source=DB\bot.sqlite;Version=3;"); //Make global
			m_dbConnection.Open();

			string sql; //make global
			SQLiteCommand command; //make global

			string setValues = columnOne + " = '" + valueOne + "'";

			if (columnTwo != null)
			{
				setValues += ", " + columnTwo + " = '" + valueTwo + "'";
			}

			if (columnThree != null)
			{
				setValues += ", " + columnThree + " = '" + valueThree + "'";
			}
		

			sql = "UPDATE " + table + " SET " + setValues + " WHERE " + whereColumn + " = " + whereValue;

			command = new SQLiteCommand(sql, m_dbConnection);
			command.ExecuteNonQuery();

			m_dbConnection.Close();
		}

		public async Task CheckEmissaries2()
		{
			List<string> dbEmissariesList = GetDBResults("name", "emissaries");
			List<string> dbTimeLeftList = GetDBResults("timeleft", "emissaries");

			List<string> emissariesList = GetInnerText("//a[contains(@id,'US-emissary-')]");
			List<string> timeLeftList = GetInnerText("//a[contains(@id,'US-emissary-')]/../script");

			string[] timeLeftArray = timeLeftList.ToArray();

			int i = 0;
			foreach (string time in timeLeftArray)
			{
				string[] split = time.Split('"');
				timeLeftArray[i] = split[1].Replace("hr", "hours").Replace("min", "minutes").Replace("day", "days");
				log.Debug(timeLeftArray[i]);
				i++;
			}

			log.Debug("Updating emissaries");

			DBDropTable("emissaries");

			int j = 0;
			foreach (string time in timeLeftArray)
			{
				log.Debug($"{emissariesList[j]} - {timeLeftArray[j]}");
				InsertToDB("name", emissariesList[j], "timeleft", timeLeftArray[j], null, null, "emissaries");
				j++;
			}

			int k = 0;
			foreach (string emissary in dbEmissariesList)
			{
				if (emissary != emissariesList[k] || dbEmissariesList.Count != emissariesList.Count)
				{
					InsertToDB("tablename", "emissaries", null, null, null, null, "tablestoalert");
					break;
				}
				k++;
			}
		}

		public async Task CheckMenagerie2()
		{
			List<string> dbPetNameList = GetDBResults("petname", "menagerie");
			List<string> petsList = GetInnerText("//a[contains(@id,'US-menagerie-')]");

			string[] pets = petsList.ToArray();

			DBDropTable("menagerie");

			int i = 0;
			foreach (string pet in pets)
			{
				//log.Debug($"{emissariesList[j]} - {timeLeftArray[j]}");
				InsertToDB("petname", pets[i], null, null, null, null, "menagerie");
				i++;
			}

			int j = 0;
			foreach (string petname in dbPetNameList)
			{
				if (petname != pets[j] || dbPetNameList.Count != pets.Length)
				{
					InsertToDB("tablename", "menagerie", null, null, null, null, "tablestoalert");
					break;
				}
				j++;
			}
		}

		public async Task CheckVHBosses2()
		{
			List<string> dbVhBossesList = GetDBResults("bossname", "vhbosses");
			List<string> vhBossesList = GetInnerText("//a[contains(@id,'US-violethold-')]");
			string[] vhBosses = vhBossesList.ToArray();

			DBDropTable("vhbosses");

			int i = 0;
			foreach (string boss in vhBosses)
			{
				InsertToDB("bossname", boss.Replace("'", "''"), null, null, null, null, "vhbosses");
				i++;
			}

			int j = 0;
			foreach (string boss in dbVhBossesList)
			{
				if (boss != vhBosses[j] || dbVhBossesList.Count != vhBosses.Length)
				{
					InsertToDB("tablename", "vhbosses", null, null, null, null, "tablestoalert");
					break;
				}
				j++;
			}
		}

		public async Task CheckDailyReset2()
		{
			List<string> dbDailyResetList = GetDBResults("time", "dailyreset");

			long epochTimeNow = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

			DBDropTable("dailyreset");

			string dailyReset = "";
			string dailyResetEpochTime = GetAttributeValue("//div[contains(@id,'tiw-timer-US')]", "data-timestamp").FirstOrDefault();

			long epochTimeLeft = 0;

			epochTimeLeft = Convert.ToInt64(dailyResetEpochTime) - epochTimeNow;

			dailyReset = util.ConvertUnixEpochTime(Convert.ToInt64(dailyResetEpochTime)).ToString();

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

			InsertToDB("time", dailyResetEpochTime, "timeleft", hoursLeft + minutesLeft + " minutes", null, null, "dailyreset");

			if (dbDailyResetList[0] != dailyResetEpochTime)
			{
				InsertToDB("tablename", "dailyreset", null, null, null, null, "tablestoalert");
			}
		}

		public async Task CheckWowToken2()
		{
			string tokenGold = GetInnerText("//span[contains(@class,'moneygold')]", 1).FirstOrDefault();

			DBDropTable("wowtoken");

			InsertToDB("price", tokenGold, null, null, null, null, "wowtoken");
		}

		public async Task CheckWorldBosses2()
		{
			List<string> dbWorldBossesList = GetDBResults("bossname", "worldbosses");

			List<string> bosses = GetInnerText("//a[contains(@id,'US-epiceliteworld-')]");

			string[] bossesArray = bosses.ToArray();

			DBDropTable("worldbosses");

			int i = 0;
			foreach (string boss in bossesArray)
			{
				InsertToDB("bossname", boss.Replace("'", "''"), null, null, null, null, "worldbosses");
				i++;
			}

			int j = 0;
			foreach (string bossname in dbWorldBossesList)
			{
				if (bossname != bossesArray[j] || dbWorldBossesList.Count != bossesArray.Length)
				{
					InsertToDB("tablename", "worldbosses", null, null, null, null, "tablestoalert");
					break;
				}
				j++;
			}
		}

		public async Task CheckWorldEvents2()
		{
			List<string> dbWorldEvents = GetDBResults("eventname", "worldevents");
			List<string> worldEvents = GetInnerText("//a[contains(@id,'US-holiday-')]");;
			string[] worldEventsArray = worldEvents.ToArray();

			DBDropTable("worldevents");

			int i = 0;
			foreach (string worldEvent in worldEventsArray)
			{
				InsertToDB("eventname", worldEvent.Replace("'", "''"), null, null, null, null, "worldevents");
				i++;
			}

			int j = 0;
			foreach (string worldevent in dbWorldEvents)
			{
				if (worldevent != worldEventsArray[j] || dbWorldEvents.Count != worldEventsArray.Length)
				{
					InsertToDB("tablename", "worldevents", null, null, null, null, "tablestoalert");
					break;
				}
				j++;
			}
		}

		public async Task CheckXurios2()
		{
			List<string> dbXurios = GetDBResults("itemname", "xurios");
			List<string> xurios = GetInnerText("//a[contains(@id,'US-xurios-')]");
			string[] xuriosArray = xurios.ToArray();

			DBDropTable("xurios");

			int i = 0;
			foreach (string item in xuriosArray)
			{
				InsertToDB("itemname", item.Replace("'", "''"), null, null, null, null, "xurios");
				i++;
			}

			int j = 0;
			foreach (string item in dbXurios)
			{
				if (item != xuriosArray[j] || dbXurios.Count != xuriosArray.Length)
				{
					InsertToDB("tablename", "xurios", null, null, null, null, "tablestoalert");
					break;
				}
				j++;
			}
		}

		public async Task CheckAffixes2()
		{
			List<string> dbAffixes = GetDBResults("affixname", "dungeonaffixes");
			List<string> mythicAffixesList = GetInnerText("//a[contains(@id,'US-mythicaffix-')]");
			string[] mythicAffixes = mythicAffixesList.ToArray();

			string[] affixNumbers = GetAttributeValue("//a[contains(@id,'US-mythicaffix-')]", "href").ToArray();

			DBDropTable("dungeonaffixes");

			int i = 0;
			int j = 1;
			foreach (string affix in mythicAffixes)
			{
				j += 3;
				InsertToDB("affixname", affix, "affixnum", affixNumbers[i].Replace("/affix=", ""), "dungeonlevel", j.ToString(), "dungeonaffixes");
				i++;
			}

			int k = 0;
			foreach (string affix in dbAffixes)
			{
				if (affix != mythicAffixes[k] || dbAffixes.Count != mythicAffixes.Length)
				{
					InsertToDB("tablename", "dungeonaffixes", null, null, null, null, "tablestoalert");
					break;
				}
				k++;
			}
		}

		public async Task CheckInvasions2()
		{
			List<string> dbAssaults = GetDBResults("zonename", "legionassaults");
			string legionAssaultsLine = GetInnerText("//div[contains(@id,'tiw-assault-US')]/../script[contains(.,'legion-assaults')]").FirstOrDefault();

			string[] words = legionAssaultsLine.Split(':');

			string[] upcomingAssaultsStr = words[6].TrimStart('[').Replace("],\"length\"", "").Split(',');
			int length = Convert.ToInt32(words[7].Replace(" ", "").Replace("});", ""));
			log.Debug(length);
			string zoneName = words[5].Replace("\"", "").Replace(",upcoming", "");

			List<long> upcomingTimesEpoch = new List<long>();

			int i = 0;
			foreach (string time in upcomingAssaultsStr)
			{
				upcomingTimesEpoch.Add(Convert.ToInt64(time));
			}

			long epochTimeNow = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

			DBDropTable("legionassaults");

			string hoursLeft = "";
			string minutesLeft = "";
			string zone = "";

			foreach (long epochTime in upcomingTimesEpoch)
			{
				InsertToDB("assaulttime", epochTime.ToString(), "timeleft", hoursLeft + minutesLeft, "length", length.ToString(), "legionassaults");
			}

			int j = 1;
			foreach (long epochTime in upcomingTimesEpoch)
			{
				long tempEpochTime = epochTime + (length);

				if (epochTimeNow < tempEpochTime)
				{
					if (epochTimeNow > epochTime)
					{
						if (zoneName != "" || zoneName != "null")
						{
							zone = zoneName;
						}
						else
						{
							zone = "A zone";
						}
						long epochTimeLeft = (tempEpochTime - epochTimeNow);
						TimeSpan t = TimeSpan.FromSeconds(epochTimeLeft);
						hoursLeft = t.ToString(@"hh");
						minutesLeft = t.ToString(@"mm").TrimStart('0');
						if (hoursLeft != "00")
						{
							hoursLeft = hoursLeft.TrimStart('0') + " hours and ";
						}
						else
						{
							hoursLeft = "";
						}
					}
				}
				if (j < upcomingTimesEpoch.Count && epochTimeNow > epochTime && epochTimeNow < tempEpochTime)
				{
					UpdateDBRecord("zonename", zone.Replace("'", "''"), "timeleft", hoursLeft + minutesLeft, null, null, "id", j.ToString(), "legionassaults");
				}
				j++;
			}

			int k = 0;
			foreach (string name in dbAssaults)
			{
				if (!string.IsNullOrEmpty(name) && name != zoneName)
				{
					InsertToDB("tablename", "legionassaults", null, null, null, null, "tablestoalert");
				}
				k++;
			}
		}

		public async Task CheckBrokenShoreBuildings2()
		{
			List<string> dbBrokenShore = GetDBResults("buildingstate", "brokenshorebuildings");

			string[] buildingNames = GetInnerText("//div[contains(@data-region,'US')]/../../div/a").ToArray();
			string[] buildingProgress = GetAttributeValue("//div[contains(@data-region,'US')]/../..//div/div[contains(@class, 'tiw-bs-status-state')]/../div[contains(@class, 'tiw-bs-status-progress')]/span", "title").ToArray();
			string[] buildingStates = GetInnerText("//div[contains(@data-region,'US')]/../..//div/div[contains(@class, 'tiw-bs-status-state')]").ToArray();

			GetInnerText("//div[contains(@data-region,'US')]/../../div/a");
			GetInnerText("//div[contains(@data-region,'US')]/../..//div/div[contains(@class, 'tiw-bs-status-state')]");
			GetAttributeValue("//div[contains(@data-region,'US')]/../..//div/div[contains(@class, 'tiw-bs-status-state')]/../div[contains(@class, 'tiw-bs-status-progress')]/span", "title");

			DBDropTable("brokenshorebuildings");

			int i = 0;
			foreach (string building in buildingNames)
			{
				InsertToDB("buildingname", buildingNames[i], "buildingstate", buildingStates[i], "buildingpercentage", buildingProgress[i], "brokenshorebuildings"); 
				i++;
			}

			int k = 0;
			foreach (string state in dbBrokenShore)
			{
				if (state != buildingStates[k])
				{
					InsertToDB("tablename", "brokenshorebuildings", null, null, null, null, "tablestoalert");
					break;
				}
				k++;
			}
		}
	}
}
