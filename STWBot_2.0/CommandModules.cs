using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;

namespace STWBot_2
{
	public class CommandModules
	{
		public CommandModules()
		{
		}
	}

	public class Info : ModuleBase
	{
		[Command("say"), Summary("Echos a message.")]
		public async Task Say([Remainder, Summary("The text to echo")] string echo)
		{
			await ReplyAsync(echo);
		}
	}

	[Group("sample")]
	public class Sample : ModuleBase
	{
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
		[Command("invasion"), Summary("Replies when the next invasions are about to begin")]
		[Alias("invasions")]
		public async Task Invasion()
		{
			Utilities util = new Utilities();
			util.DownloadNewWowHead();
			string legionAssaultsLine = util.GetLine("<script>$WH.news.addInvasionDisplay(\"US\", {\"id\":\"legion-assaults\"", "test.txt");
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
			string zoneName = words[5].Remove(0, 11).Trim('"');
			long[] upcomingTimesEpoch = { Convert.ToInt64(words[6].Remove(0, 12)), Convert.ToInt64(words[7]), Convert.ToInt64(words[8]), Convert.ToInt64(words[9]), Convert.ToInt64(words[10].Trim(']')) };

			//char[] trimChars = { '}', ')', ';', '<', '/', 's', 'c', 'r', 'i', 'p', 't' };

			int length = Convert.ToInt32(words[11].Remove(0, 9).Remove(5, 12));
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

			//System.IO.File.WriteAllText(@"test.txt", htmlCode);

			//Console.WriteLine(htmlCode);
			/* Need updated chromium/chromedriver.exe
			 * IWebDriver driver = new ChromeDriver(@"C:\Users\Kurt\AppData\Local\Google\Chrome SxS\Application");
			driver.Navigate().GoToUrl("https://wowhead.com");
			*/
			await Context.Channel.SendMessageAsync(msg);
		}


		[Command("affix"), Summary("Shows the Mythic+ Affixes active for the current week")]
		[Alias("affixes")]
		public async Task Affixes()
		{
			Utilities util = new Utilities();
			util.DownloadNewWowHead();
			string[] mythicAffixes = { util.GetLine("id=\"US-mythicaffix-1\"", "test.txt").TrimStart(), util.GetLine("id=\"US-mythicaffix-2\"", "test.txt").TrimStart(), util.GetLine("id=\"US-mythicaffix-3\"", "test.txt").TrimStart() };
			int[] affixNumbers = { 0, 0, 0 };
			string msg = "";

			int i = 0;

			foreach (string affix in mythicAffixes)
			{
				string[] words = affix.Split(' ');
				string[] cleanedWords = words[6].Split('<');

				string[] numbers = words[1].Split('=');
				int cleanedNumber = Convert.ToInt32(numbers[2].TrimEnd('"'));

				//Console.WriteLine(mythicAffixes[i]);
				mythicAffixes[i] = cleanedWords[0];
				affixNumbers[i] = cleanedNumber;
				i++;
			}

			Console.WriteLine(mythicAffixes[0] + " " + affixNumbers[0]);
			Console.WriteLine(mythicAffixes[1] + " " + affixNumbers[1]);
			Console.WriteLine(mythicAffixes[2] + " " + affixNumbers[2]);

			msg = "This week's Mythic+ Dungeon Affixes are:\n\n**" + mythicAffixes[0] + "** - Mythic Keystone Level 4+\nMore Info: <http://www.wowhead.com/affix=" + affixNumbers[0] + ">\n\n**" + mythicAffixes[1] + "** - Mythic Keystone Level 7+\nMore Info: <http://www.wowhead.com/affix=" + affixNumbers[1] + ">\n\n**" + mythicAffixes[2] + "** - Mythic Keystone Level 10+\nMore Info: <http://www.wowhead.com/affix=" + affixNumbers[2] + ">";
			await Context.Channel.SendMessageAsync(msg);
		}

		[Command("emissaries"), Summary("Shows which emissaries are available for completion")]
		[Alias("emissary")]
		public async Task Emissaries()
		{
			Utilities util = new Utilities();
			util.DownloadNewWowHead();
			string[] emissaries = { util.GetLine("\"US--1\"", "test.txt"), util.GetLine("\"US--2\"", "test.txt"), util.GetLine("\"US--3\"", "test.txt") };
			string[] timeLeft = { util.GetLine("\'US--1\'", "test.txt"), util.GetLine("\'US--2\'", "test.txt"), util.GetLine("\'US--3\'", "test.txt") };

			string msg = "";

			int i = 0;

			foreach (string emissary in emissaries)
			{
				if (emissary != "No results found...")
				{
					string[] words = emissary.Split('>');
					emissaries[i] = words[1].TrimEnd('<', '/', 'a');
					i++;
				}
			}

			int j = 0;

			foreach (string time in timeLeft)
			{
				if (time != "No results found...")
				{
					string[] numbers = time.Split(',');
					timeLeft[j] = numbers[1].TrimStart('"', ' ').TrimEnd('"').Replace("hr", "hours").Replace("min", "minutes").Replace("day", "days");
					j++;
				}
			}

			Console.WriteLine(emissaries[0]);
			Console.WriteLine(timeLeft[0]);
			Console.WriteLine(emissaries[1]);
			Console.WriteLine(timeLeft[1]);
			Console.WriteLine(emissaries[2]);
			Console.WriteLine(timeLeft[2]);

			msg = "Current active emissaries are:\n\n**" + emissaries[0] + "** - __" + timeLeft[0] + "__ remaining to complete\n\n**" + emissaries[1] + "** - __" + timeLeft[1] + "__ remaining to complete\n\n**" + emissaries[2] + "** - __" + timeLeft[2] + "__ remaining to complete";
			await Context.Channel.SendMessageAsync(msg);
		}

		[Command("brokenshorebuildings"), Summary("Shows the % for buildings on the Broken Shore!")]
		[Alias("bsb")]
		public async Task BrokenShoreBuildings()
		{
			Utilities util = new Utilities();
			util.DownloadNewWowHead();

			string[] buildings = { util.GetLineAfterNum("Mage Tower", "test.txt", 8), util.GetLineAfterNum("Command Center", "test.txt", 8), util.GetLineAfterNum("Nether Disruptor", "test.txt", 8) };
			string[] buildingStates = { util.GetLineAfterNum("Mage Tower", "test.txt", 6), util.GetLineAfterNum("Command Center", "test.txt", 6), util.GetLineAfterNum("Nether Disruptor", "test.txt", 6) };

			string msg = "";

			//string mageTowerPercentage = util.GetLineAfterNum("Mage Tower", "test.txt", 8);
			//string commandCenterPercentage = util.GetLineAfterNum("Command Center", "test.txt", 8);
			//string netherDisruptorPercentage = util.GetLineAfterNum("Nether Disruptor", "test.txt", 8);

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
				buildingStates[j] = words[1].Replace("</div", "");
				j++;
			}

			Console.WriteLine(buildings[0]);
			Console.WriteLine(buildingStates[0]);
			Console.WriteLine(buildings[1]);
			Console.WriteLine(buildingStates[1]);
			Console.WriteLine(buildings[2]);
			Console.WriteLine(buildingStates[2]);

			msg = "Current Broken Shore buildings stats are as follows: \n\n__**Mage Tower**__\n" + buildingStates[0] + "\n" + buildings[0] + "\n\n__**Command Center**__\n" + buildingStates[1] + "\n" + buildings[1] + "\n\n__**Nether Disruptor**__\n" + buildingStates[2] + "\n" + buildings[2];

			await Context.Channel.SendMessageAsync(msg);
		}

		[Command("menagerie"), Summary("Shows the % for buildings on the Broken Shore!")]
		[Alias("pets")]
		public async Task MenageriePets()
		{
			Utilities util = new Utilities();
			util.DownloadNewWowHead();

			string[] pets = { util.GetLine("US-menagerie-1", "test.txt"), util.GetLine("US-menagerie-2", "test.txt"), util.GetLine("US-menagerie-3", "test.txt") };

			string msg = "";

			int i = 0;

			foreach (string pet in pets)
			{
				if (pet != "No results found...")
				{
					string[] words = pet.Split('>');
					pets[i] = words[2].TrimStart(' ').Replace("</a", "");
				}
				i++;
			}

			//Console.WriteLine(pets[0]);

			msg = "Pets currently active in your Garrison's Managerie this week are:\n\n**" + pets[0] + "**\n\n**" + pets[1] + "**\n\n**" + pets[2] + "**";

			await Context.Channel.SendMessageAsync(msg);
		}

		[Command("violethold"), Summary("Shows the % for buildings on the Broken Shore!")]
		[Alias("vhbosses")]
		public async Task VioletHoldBosses()
		{
			Utilities util = new Utilities();
			util.DownloadNewWowHead();

			string msg = "";

			string[] vhBosses = { util.GetLine("US-violethold-1", "test.txt"), util.GetLine("US-violethold-2", "test.txt"), util.GetLine("US-violethold-3", "test.txt") };

			int i = 0;

			foreach (string boss in vhBosses)
			{
				string[] words = boss.Split('>');
				vhBosses[i] = words[1].Replace("</a", "");

				i++;
			}

			msg = "Current bosses active in the Violet Hold this week are:\n\n**" + vhBosses[0] + "**\n\n**" + vhBosses[1] + "**\n\n**" + vhBosses[2] + "**";

			await Context.Channel.SendMessageAsync(msg);
		}

		[Command("dailyreset"), Summary("Shows the % for buildings on the Broken Shore!")]
		[Alias("dailyquestreset")]
		public async Task DailyQuestReset()
		{
			Utilities util = new Utilities();
			util.DownloadNewWowHead();

			string msg = "";

			long epochTimeNow = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

			string dailyReset = util.GetLine("tiw-timer-US", "test.txt");
			string[] words = dailyReset.Split('"');
			dailyReset = words[5];

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
			util.DownloadNewWowHead();

			string msg = "";

			string tokenGold = util.GetLineAfterNum("tiw-timer-US", "test.txt", 2);

			string[] words = tokenGold.Split('>');
			tokenGold = words[2].Replace("</span", "");

			//Console.WriteLine(tokenGold);

			msg = "WoW Tokens are currently worth **" + tokenGold + "** gold.";

			await Context.Channel.SendMessageAsync(msg);
		}

		[Command("worldbosses"), Summary("Shows the % for buildings on the Broken Shore!")]
		[Alias("bosses")]
		public async Task WorldBosses()
		{
			Utilities util = new Utilities();
			util.DownloadNewWowHead();

			string msg = "Below are the current World Bosses that are active this week:\n\n";

			List<string> bosses = util.ReturnAllLines("US-epiceliteworld-", "test.txt");

			string[] bossesArray = bosses.ToArray();

			int i = 0;

			foreach (string boss in bossesArray)
			{
				string[] words = boss.Split('>');
				bossesArray[i] = words[1].Replace("</a", "");
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
			util.DownloadNewWowHead();

			string msg = "Below are the current World Events that are active this week:\n\n";

			List<string> worldEvents = util.ReturnAllLines("US-holiday-", "test.txt");

			string[] worldEventsArray = worldEvents.ToArray();

			int i = 0;

			foreach (string worldEvent in worldEventsArray)
			{
				string[] words = worldEvent.Split('>');
				worldEventsArray[i] = words[2].TrimStart(' ').Replace("</a", "");
				msg += "**" + worldEventsArray[i] + "**\n\n";
				i++;
			}

			msg.TrimEnd('\n');

			await Context.Channel.SendMessageAsync(msg);
		}

		[Command("xurios"), Summary("Shows the % for buildings on the Broken Shore!")]
		[Alias("curiouscoins")]
		public async Task Xurios()
		{
			Utilities util = new Utilities();
			util.DownloadNewWowHead();

			string msg = "This week Xur\'ios is selling the following for Curious Coins:\n\n";

			List<string> xurios = util.ReturnAllLines("US-xurios-", "test.txt");

			string[] xuriosArray = xurios.ToArray();

			int i = 0;

			foreach (string item in xuriosArray)
			{
				string[] words = item.Split('>');
				xuriosArray[i] = words[3].TrimStart(' ').Replace("</a", "");
				msg += "**" + xuriosArray[i] + "**\n\n";
				i++;
			}

			msg.TrimEnd('\n');

			await Context.Channel.SendMessageAsync(msg);
		}
	}
}
