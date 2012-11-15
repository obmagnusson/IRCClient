using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace IRCclient
{
	class Program
	{
		public string getDate() 
		{
			DateTime time = DateTime.Now;
			string format = "ddd M yyyy hh:mm:ss ";
			return time.ToString(format);
		}

		public void ircInit(StreamWriter serverWrite, string Nick)
		{
			serverWrite.WriteLine("CAP LS");
			serverWrite.Flush();
			serverWrite.WriteLine("NICK " + Nick);
			serverWrite.Flush();
			serverWrite.WriteLine("USER " + Nick + " 0 * :...");
			serverWrite.Flush();
			serverWrite.WriteLine("CAP REQ :multi-prefix");
			serverWrite.Flush();
			serverWrite.WriteLine("CAP END");
			serverWrite.Flush();
			serverWrite.WriteLine("USERHOST " + Nick);
			serverWrite.Flush();
		}

		static void Main(string[] cmdLine)
		{
			Program irc = new Program();

			int port = 6667;
			String hostName = (String)cmdLine.GetValue(0);
            String Nick = (String)cmdLine.GetValue(1);
			
			TcpClient client = null;
			try
			{
				client = new TcpClient(hostName, port);
				Stream s = client.GetStream(); 
				StreamWriter log;
				StreamReader serverRead = new StreamReader(s);
				StreamWriter serverWrite = new StreamWriter(s);
				Console.WriteLine(serverRead.ReadLine());

				if (!File.Exists("irc.log"))
				{
					log = new StreamWriter("irc.log");
				}
				else
				{
					log = File.AppendText("irc.log");
				}

				//Start IrcServer Session
				irc.ircInit(serverWrite, Nick);

				while (true)
				{
					Console.Write(">: ");
					string userInput = Console.ReadLine();
					Console.WriteLine(userInput);

					log.WriteLine(irc.getDate() + "GMT : Client: " + userInput);
					serverWrite.Flush();

					if (userInput == "QUIT")
					{
						client.Close();
						break;
					}

					Console.WriteLine("IRC SERVER : " + serverRead.ReadLine());
					log.WriteLine(irc.getDate() + "GMT : Server: " + serverRead.ReadLine());
				}
				s.Close();
				log.Close();
			}
			catch (System.Net.Sockets.SocketException ex)
			{
				Console.Error.WriteLine("Could find host..");
			}
			finally
			{
				client.Close();
			}

			Console.WriteLine("Press any key to continue ....");
			Console.ReadKey(true);
		}
	}
}
