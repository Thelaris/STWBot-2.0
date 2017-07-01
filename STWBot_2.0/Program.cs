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

namespace STWBot_2
{
	class MainClass
	{
		private CommandService commands;
		private DiscordSocketClient client;

		private Utilities util = new Utilities();

		SQLiteConnection m_dbConnection;

		private Token tokenRef = new Token();
		//private IServiceProvider map;
		//private Discord map;
		public IServiceProvider map;

		string notFound = "No results found...";

		public static void Main(string[] args) => new MainClass().MainAsync().GetAwaiter().GetResult();

		public async Task MainAsync()
		{
			
			client = new DiscordSocketClient(new DiscordSocketConfig
			{
				WebSocketProvider = WS4NetProvider.Instance,
			});
			commands = new CommandService();

			map = null;

			util.DownloadNewWowHead();

			await InstallCommands();
			await ConnectToDB();
			await CheckEmissaries();
			await CheckMenagerie();
			await CheckVHBosses();
			await CheckDailyReset();
			await CheckWowToken();
			await CheckWorldBosses();
			await CheckWorldEvents();
			await CheckXurios();
			await CheckAffixes();
			await CheckInvasions();
			await CheckBrokenShoreBuildings();
			//await CheckInvasionTimes();



			client.Log += Log;
			client.JoinedGuild += JoinedNewGuild;
			client.LeftGuild += LeftGuild;
			//client.MessageReceived += MessageReceived;
			//client.UserJoined += UserJoined;

			await client.LoginAsync(TokenType.Bot, tokenRef.token);
			await client.StartAsync();


			//await 

			client.SetGameAsync("Testing");

			Timer t = new Timer();
			t.Interval = 600000; //In milliseconds here
			t.AutoReset = true; //Stops it from repeating
			t.Elapsed += new ElapsedEventHandler(TimerElapsed);
			t.Start();

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
				if (emissary != emissaries[l])
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
			Console.WriteLine(msg.ToString());
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
				Console.WriteLine("FIRED!");
				m_dbConnection = new SQLiteConnection(@"Data Source=DB\bot.sqlite;Version=3;");
				m_dbConnection.Open();

				string sql;
				SQLiteCommand command;

				List<string> tablenames = new List<string>();

				m_dbConnection.Close();

				await CheckWowhead();

				await CheckBrokenShoreBuildings();
				await CheckInvasions();
				await CheckEmissaries();
				await CheckMenagerie();
				await CheckAffixes();
				await CheckVHBosses();
				await CheckWorldBosses();
				await CheckWorldEvents();
				await CheckXurios();
				await CheckDailyReset();
				await CheckWowToken();


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
					foreach (string table in tablenames)
					{
						await client.GetGuild(Convert.ToUInt64(reader["guildid"])).GetTextChannel(Convert.ToUInt64(reader["channelid"])).SendMessageAsync("There was a change in " + table);
					}
				}

				sql = "DELETE FROM tablestoalert";
				command = new SQLiteCommand(sql, m_dbConnection);
				command.ExecuteNonQuery();
			}
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
				pets[i] = words[1].TrimStart(' ').Replace("</a", "");

				sql = "INSERT INTO menagerie (petname) VALUES ('" + pets[i].Replace("'", "''") + "')";
				command = new SQLiteCommand(sql, m_dbConnection);
				command.ExecuteNonQuery();


				msg += "**" + pets[i] + "**\n\n";



				i++;
			}

			int j = 0;

			foreach (string petname in dbPetNameList)
			{
				if (petname != pets[j])
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
				if (boss != vhBosses[j])
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
		}


		public async Task CheckAffixes()
		{
			//Utilities util = new Utilities();
			//util.DownloadNewWowHead();

			m_dbConnection = new SQLiteConnection(@"Data Source=DB\bot.sqlite;Version=3;");
			m_dbConnection.Open();

			string sql;
			SQLiteCommand command;

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

			string legionAssaultsLine = util.GetLine("<script>$WH.news.addAssaultDisplay(\"US\", {\"id\":\"legion-assaults\"", "test.txt");
			//string msg = "";

			Console.WriteLine(legionAssaultsLine);

			string[] words = legionAssaultsLine.Split(':');

			string[] upcomingAssaultsStr = words[13].TrimStart('[').Replace("],\"length\"", "").Split(',');
			int length = Convert.ToInt32(words[14].Replace(" ", "").Replace("});</script></div>", ""));
			string zoneName = words[12].Replace("\"", "").Replace(",upcoming", "");

			Console.WriteLine(upcomingAssaultsStr[0]);
			Console.WriteLine(length);

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
				sql = "INSERT INTO legionassaults (assaulttime, timeleft) VALUES ('" + epochTime + "', '" + hoursLeft + minutesLeft + "')";
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

				//goto NextEpochTime;
			//NextEpochTime:
				j++;
			}
			//await Context.Channel.SendMessageAsync(msg);



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
