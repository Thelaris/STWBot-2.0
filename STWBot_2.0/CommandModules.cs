using System;
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
				string[] words = emissary.Split('>');
				emissaries[i] = words[1].TrimEnd('<', '/', 'a');
				i++;
			}

			int j = 0;

			foreach (string time in timeLeft)
			{
				string[] numbers = time.Split(',');
				timeLeft[j] = numbers[1].TrimStart('"', ' ').TrimEnd('"').Replace("hr", "hours").Replace("min", "minutes").Replace("day", "days");
				j++;
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
	}
}
