#region namespaces
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
#endregion

namespace STWBot_2
{
	public class MainClass
	{
#region fields
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private CommandService commands;
		public DiscordSocketClient client;
		private Utilities util = new Utilities();
		private Wow commander = new Wow();
		SQLiteConnection m_dbConnection;
		private Token tokenRef = new Token();
		public IServiceProvider map;
		public static MainClass instance = null;
		private int checkTimerInit = 600000; //600000 = 10mins
		private int checkTimer = 0;
		private Timer t = new Timer();
#endregion

#region Inital Startup / Main
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

			await RunChecks();

			client.Log += Log;
			client.JoinedGuild += JoinedNewGuild;
			client.LeftGuild += LeftGuild;
			//client.MessageReceived += MessageReceived;
			//client.UserJoined += UserJoined;

			await client.LoginAsync(TokenType.Bot, tokenRef.token);
			await client.StartAsync();

			client.SetGameAsync("Testing");


			t.Interval = checkTimer; //In milliseconds here //600000
			t.AutoReset = true; //Stops it from repeating
			t.Elapsed += new ElapsedEventHandler(TimerElapsed);
			t.Start();

			// Block this task until the program is closed (infinite delay)
			await Task.Delay(-1);
		}
#endregion

#region Methods
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

		public async Task RunChecks()
		{
			checkTimer = checkTimerInit;
			t.Interval = checkTimer;
			log.Debug($"Check timer is set for every {checkTimer} milliseconds");
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

				await RunChecks();

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

#region WoW Head Checks
		public async Task CheckEmissaries()
		{
			try
			{
				log.Debug("Running CheckEmissaries");
				List<string> dbEmissariesList = GetDBResults("name", "emissaries");
				List<string> dbTimeLeftList = GetDBResults("timeleft", "emissaries");

				List<string> emissariesList = GetInnerText("//a[contains(@id,'US-emissary-')]");
				List<string> timeLeftList = GetInnerText("//a[contains(@id,'US-emissary-')]/../script");

				if (emissariesList.Count == 0) throw new Exception("No emissaries were returned");

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
			catch (Exception ex)
			{
				Console.WriteLine("");
				Console.WriteLine("***----- FAILURE -----***");
				log.Fatal($"There was an error trying to check Emissaries from wowhead. Exception: {ex}");
				checkTimer = 60000; //1min
				t.Interval = checkTimer;
			}
		}

		public async Task CheckMenagerie()
		{
			try
			{
				log.Debug("Running CheckMenagerie");
				List<string> dbPetNameList = GetDBResults("petname", "menagerie");
				List<string> petsList = GetInnerText("//a[contains(@id,'US-menagerie-')]");

				if (petsList.Count == 0) throw new Exception("No pets were returned");

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
			catch (Exception ex)
			{
				Console.WriteLine("");
				Console.WriteLine("***----- FAILURE -----***");
				log.Fatal($"There was an error trying to check Menagerie from wowhead. Exception: {ex}");
				checkTimer = 60000; //1min
				t.Interval = checkTimer;
			}
		}

		public async Task CheckVHBosses()
		{
			try
			{
				log.Debug("Running CheckVHBosses");
				List<string> dbVhBossesList = GetDBResults("bossname", "vhbosses");
				List<string> vhBossesList = GetInnerText("//a[contains(@id,'US-violethold-')]");
				string[] vhBosses = vhBossesList.ToArray();

				if (vhBossesList.Count == 0) throw new Exception("No Violet Hold bosses were returned");

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
			catch (Exception ex)
			{
				Console.WriteLine("");
				Console.WriteLine("***----- FAILURE -----***");
				log.Fatal($"There was an error trying to check Violet Hold Bosses from wowhead. Exception: {ex}");
				checkTimer = 60000; //1min
				t.Interval = checkTimer;
			}
		}

		public async Task CheckDailyReset()
		{
			try
			{
				log.Debug("Running CheckDailyReset");
				List<string> dbDailyResetList = GetDBResults("time", "dailyreset");
				List<string> dailyResetList = GetAttributeValue("//div[contains(@id,'tiw-timer-US')]", "data-timestamp");

				if (dailyResetList.Count == 0) throw new Exception("No daily reset times were returned");

				long epochTimeNow = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

				DBDropTable("dailyreset");

				string dailyReset = "";
				string dailyResetEpochTime = dailyResetList.FirstOrDefault();

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
			catch(Exception ex)
			{
				Console.WriteLine("");
				Console.WriteLine("***----- FAILURE -----***");
				log.Fatal($"There was an error trying to check Daily Reset from wowhead. Exception: {ex}");
				checkTimer = 60000; //1min
				t.Interval = checkTimer;
			}
		}

		public async Task CheckWowToken()
		{
			try
			{
				log.Debug("Running CheckWowToken");
				List<string> tokenGoldList = GetInnerText("//span[contains(@class,'moneygold')]", 1);
				string tokenGold = tokenGoldList.FirstOrDefault();

				if (tokenGoldList.Count == 0) throw new Exception("No WoW token prices were returned");

				DBDropTable("wowtoken");

				InsertToDB("price", tokenGold, null, null, null, null, "wowtoken");
			}
			catch (Exception ex)
			{
				Console.WriteLine("");
				Console.WriteLine("***----- FAILURE -----***");
				log.Fatal($"There was an error trying to check WoW Tokens from wowhead. Exception: {ex}");
				checkTimer = 60000; //1min
				t.Interval = checkTimer;
			}
		}

		public async Task CheckWorldBosses()
		{
			try
			{
				log.Debug("Running CheckWorldBosses");
				List<string> dbWorldBossesList = GetDBResults("bossname", "worldbosses");
				List<string> bosses = GetInnerText("//a[contains(@id,'US-epiceliteworld-')]");
				string[] bossesArray = bosses.ToArray();

				if (bosses.Count == 0) throw new Exception("No World Bosses were returned");

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
			catch (Exception ex)
			{
				Console.WriteLine("");
				Console.WriteLine("***----- FAILURE -----***");
				log.Fatal($"There was an error trying to check World Bosses from wowhead. Exception: {ex}");
				checkTimer = 60000; //1min
				t.Interval = checkTimer;
			}
		}

		public async Task CheckWorldEvents()
		{
			try
			{
				log.Debug("Running CheckWorldEvents");
				List<string> dbWorldEvents = GetDBResults("eventname", "worldevents");
				List<string> worldEvents = GetInnerText("//a[contains(@id,'US-holiday-')]"); ;
				string[] worldEventsArray = worldEvents.ToArray();

				if (worldEvents.Count == 0) throw new Exception("No World Events were returned");

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
			catch (Exception ex)
			{
				Console.WriteLine("");
				Console.WriteLine("***----- FAILURE -----***");
				log.Fatal($"There was an error trying to check World Events from wowhead. Exception: {ex}");
				checkTimer = 60000; //1min
				t.Interval = checkTimer;
			}
		}

		public async Task CheckXurios()
		{
			try
			{
				log.Debug("Running CheckXurios");
				List<string> dbXurios = GetDBResults("itemname", "xurios");
				List<string> xurios = GetInnerText("//a[contains(@id,'US-xurios-')]");
				string[] xuriosArray = xurios.ToArray();

				if (xurios.Count == 0) throw new Exception("No Xurios items were returned");

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
			catch (Exception ex)
			{
				Console.WriteLine("");
				Console.WriteLine("***----- FAILURE -----***");
				log.Fatal($"There was an error trying to check Xur'ios from wowhead. Exception: {ex}");
				checkTimer = 60000; //1min
				t.Interval = checkTimer;
			}
		}

		public async Task CheckAffixes()
		{
			try
			{
				log.Debug("Running CheckAffixes");
				List<string> dbAffixes = GetDBResults("affixname", "dungeonaffixes");
				List<string> mythicAffixesList = GetInnerText("//a[contains(@id,'US-mythicaffix-')]");
				string[] mythicAffixes = mythicAffixesList.ToArray();

				string[] affixNumbers = GetAttributeValue("//a[contains(@id,'US-mythicaffix-')]", "href").ToArray();

				if (mythicAffixesList.Count == 0) throw new Exception("No Mythic+ Affixes were returned");

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
			catch (Exception ex)
			{
				Console.WriteLine("");
				Console.WriteLine("***----- FAILURE -----***");
				log.Fatal($"There was an error trying to check Mythic+ Affixes from wowhead. Exception: {ex}");
				checkTimer = 60000; //1min
				t.Interval = checkTimer;
			}
		}

		public async Task CheckInvasions()
		{
			try
			{
				log.Debug("Running CheckInvasions");
				List<string> dbAssaults = GetDBResults("zonename", "legionassaults");
				List<string> legionAssaultsList = GetInnerText("//div[contains(@id,'tiw-assault-US')]/../script[contains(.,'legion-assaults')]");
				string legionAssaultsLine = legionAssaultsList.FirstOrDefault();

				if (legionAssaultsList.Count == 0) throw new Exception("No Legion Assaults were returned");

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
			catch (Exception ex)
			{
				Console.WriteLine("");
				Console.WriteLine("***----- FAILURE -----***");
				log.Fatal($"There was an error trying to check Legion Invasions from wowhead. Exception: {ex}");
				checkTimer = 60000; //1min
				t.Interval = checkTimer;
			}
		}

		public async Task CheckBrokenShoreBuildings()
		{
			try
			{
				log.Debug("Running CheckBrokenShoreBuildings");
				List<string> dbBrokenShore = GetDBResults("buildingstate", "brokenshorebuildings");

				string[] buildingNames = GetInnerText("//div[contains(@data-region,'US')]/../../div/a").ToArray();
				string[] buildingProgress = GetAttributeValue("//div[contains(@data-region,'US')]/../..//div/div[contains(@class, 'tiw-bs-status-state')]/../div[contains(@class, 'tiw-bs-status-progress')]/span", "title").ToArray();
				string[] buildingStates = GetInnerText("//div[contains(@data-region,'US')]/../..//div/div[contains(@class, 'tiw-bs-status-state')]").ToArray();

				GetInnerText("//div[contains(@data-region,'US')]/../../div/a");
				GetInnerText("//div[contains(@data-region,'US')]/../..//div/div[contains(@class, 'tiw-bs-status-state')]");
				GetAttributeValue("//div[contains(@data-region,'US')]/../..//div/div[contains(@class, 'tiw-bs-status-state')]/../div[contains(@class, 'tiw-bs-status-progress')]/span", "title");

				if (buildingNames.Length == 0) throw new Exception("No Broken Shore Buildings were returned");

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
			catch (Exception ex)
			{
				Console.WriteLine("");
				Console.WriteLine("***----- FAILURE -----***");
				log.Fatal($"There was an error trying to check Broken Shore Buildings from wowhead. Exception: {ex}");
				checkTimer = 60000; //1min
				t.Interval = checkTimer;
			}
		}
#endregion
#endregion //Methods
	}
}
