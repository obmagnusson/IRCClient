using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace IRCclient
{

	/*
	 *  IRC Client ----------------------------------------------------------------------------
	 *  Usage instructions: to connect to a server the usage is following from the command line:
	 *
	 *  c:\> IRCclient.exe "the name of the server" "Your nickname"
	 * 
	 *	This client receives one line from the server and waits for input from the user and then 
	 *	then receives the next line from the server and so on. To terminate the session the QUIT 
	 *	command is issued. 
	 * 
	 *	Written by : 
	 *  Edward Alexander Eiríksson 
	 *  Ólafur Björn Magnússon 
	 *  Sindri Már Sigfússon
	 *  --------------------------------------------------------------------------------------
	 */	  
	class Program
	{
		public string GetDate() 
		{
			DateTime time = DateTime.Now;
			string format = "ddd M yyyy hh:mm:ss ";
			return time.ToString(format);
		}

		public void IrcInit(StreamWriter serverWrite, string nick)
		{
			serverWrite.WriteLine("CAP LS");
			serverWrite.Flush();
			serverWrite.WriteLine("NICK " + nick);
			serverWrite.Flush();
			serverWrite.WriteLine("USER " + nick + " 0 * :...");
			serverWrite.Flush();
			serverWrite.WriteLine("CAP REQ :multi-prefix");
			serverWrite.Flush();
			serverWrite.WriteLine("CAP END");
			serverWrite.Flush();
			serverWrite.WriteLine("USERHOST " + nick);
			serverWrite.Flush();
		}

        public StreamWriter OpenLogFile()
        {
            StreamWriter log;
            if (!File.Exists("irc.log"))
            {
                log = new StreamWriter("irc.log");
            }
            else
            {
                log = File.AppendText("irc.log");
            }
            return log;
        }

		static void Main(string[] cmdLine)
		{
			Program irc = new Program();
			int port = 6667;
			String hostName = (String)cmdLine.GetValue(0);
            String nick = (String)cmdLine.GetValue(1);		
			TcpClient client = null;

			try
			{
				client = new TcpClient(hostName, port);
				Stream serverStream = client.GetStream();
                StreamWriter log = irc.OpenLogFile();
				StreamReader serverRead = new StreamReader(serverStream);
				StreamWriter serverWrite = new StreamWriter(serverStream);
				Console.WriteLine(serverRead.ReadLine());

				//Start IrcServer Session
				irc.IrcInit(serverWrite, nick);
				while (true)
				{
					Console.Write(">: ");
					string userInput = Console.ReadLine();
					Console.WriteLine(userInput);

					log.WriteLine(irc.GetDate() + "GMT : Client: " + userInput);
					serverWrite.Flush();

					if (userInput == "QUIT")
					{
						client.Close();
						break;
					}
					Console.WriteLine("IRC SERVER : " + serverRead.ReadLine());
					log.WriteLine(irc.GetDate() + "GMT : Server: " + serverRead.ReadLine());
				}
				serverStream.Close();
				log.Close();
			}
			catch (System.Net.Sockets.SocketException ex)
			{
				Console.Error.WriteLine("Error : IRC connection failed!");
				Console.Error.WriteLine("Please contact technical support(Freysteinn).");
			}
			finally
			{
				if(client != null)
				client.Close();
			}
			Console.WriteLine("Press any key to continue ....");
			Console.ReadKey(true);
		}
	}
}