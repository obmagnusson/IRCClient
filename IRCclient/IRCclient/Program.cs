using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;

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

		StreamWriter ircWriter;
		StreamReader ircReader;
		String hostName;
		String nick;
		String currentChannel;
		Socket sock;
		StreamWriter log;

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

		private void IrcChangeNick(String line) 
		{
			// NICK :NewNick
			if (line.ToLower().StartsWith("/nick"))
			{
				String[] split = line.Split(' ');
				String command = split[0]; // /nick
				String newNick = split[1]; //new nickname				
				ircWriter.WriteLine(command.ToUpper().TrimStart('/') + " :" +newNick);
				ircWriter.Flush();
			}
		}

		private void IrcJoinChannel(String line)
		{
			if (line.ToLower().StartsWith("/join"))
			{
				// JOIN #tsam
				// MODE #tsam
				String[] split = line.Split(' ');
				String command = split[0]; // /join
				String channel = split[1]; //#tsam				
				ircWriter.WriteLine(command.ToUpper().TrimStart('/')+" "+channel);
				ircWriter.Flush();
				ircWriter.WriteLine("MODE "+channel);
				ircWriter.Flush();
				currentChannel = channel;
			}
		}

		public void IrcQuit(String line)
		{
			// QUIT :
			if (line.ToLower().StartsWith("/quit"))
			{
				ircWriter.WriteLine(line.ToUpper().TrimStart('/') + " :" );
				ircWriter.Flush();
				Environment.Exit(0);	
			}
			
		}

		public void IrcLeaveChannel(String line)
		{
			if(line.ToLower().StartsWith("/leave"))
			{
				// PART #channel
				String[] split = line.Split(' ');
				String channel = split[1]; //#channel				
				ircWriter.WriteLine("PART"+" " +channel);
				ircWriter.Flush();
			}
		}

		public void IrcGetNames(String line)
		{
			if (line.ToLower().StartsWith("/names"))
			{
				// NAMES #channel
				String[] split = line.Split(' ');
				String command = split[0]; // /names
				String channel = split[1]; //#channel				
				ircWriter.WriteLine(command.ToUpper().TrimStart('/') + " " +channel);
				ircWriter.Flush();
			}
		}

		private void IrcPong(String line)
		{
			// PING :calvino.freenode.net
			// PONG :calvino.freenode.net
			if (line.StartsWith("PING"))
			{
				String[] split = line.Split(':');
				String server = split[1];
				ircWriter.WriteLine("PONG :"+server);
				ircWriter.Flush();
			}
		}
		void ServerThread()
		{	
			ircReader = new StreamReader(new NetworkStream(sock));		
			
		    Console.WriteLine(ircReader.ReadLine());

			//Start IrcServer Session
			String output = null;
			IrcInit(ircWriter, nick);
			while (true)
			{
				output = ircReader.ReadLine();	
				Console.WriteLine("IRC SERVER : " + output);
				log.WriteLine(GetDate() + "GMT : Server: " + output);
				IrcPong(output);
				//log.Close();
			}		
			//serverStream.Close();
			//log.Close();

		}
		void IrcChatMsg(String currentChannel, String line)
		{
			//PRIVMSG #CHANEL NO5 :"innihaldið"
			//PRIVMSG nickið :"innihaldið"
			if (!currentChannel.Equals(null))
			{
				ircWriter.WriteLine("PRIVMSG "+currentChannel+" :"+line);
				ircWriter.Flush();
			}
		}

		void IrcCTCP(String line)
		{
			if (line.StartsWith("/CTCP"))
			{

				if(line.EndsWith("VERSION"))
				{
					//PRIVMSG winston_lights :.VERSION.
					String[] split = line.Split(' ');
					String nickName = split[1];

					ircWriter.WriteLine("PRIVMSG " + nickName + " :.VERSION.");
					ircWriter.Flush();					
				}
				

				if(line.EndsWith("TIME"))
				{
					String[] split = line.Split(' ');
					String nickName = split[1];
					
					ircWriter.WriteLine("PRIVMSG "+nickName+ " :.TIME");
					ircWriter.Flush();
					ircWriter.WriteLine("NOTICE " + nickName + " :.TIME "+GetDate());
					ircWriter.Flush();
				}
				// /CTCP winston_ TIME
				//PRIVMSG winston_ :.TIME.
				//:winston_!~winston_@fire-out.ru.is PRIVMSG winston_ :.TIME
				//NOTICE winston_ :.TIME Tue Nov 27 13:38:08 2012.
				//:winston_!~winston_@fire-out.ru.is NOTICE winston_ :.TIME Tue Nov 27 13:38:08 2012.
				
			
					
			}

		}


		void InputThread()
		{
			ircWriter = new StreamWriter(new NetworkStream(sock));
			while (true)
			{			
				string userInput = Console.ReadLine();
				//Console.WriteLine(userInput);
				log.WriteLine(GetDate() + "GMT : Client: " + userInput);
				if (!userInput.StartsWith("/"))
				{
					IrcChatMsg(currentChannel, userInput);
				}
				IrcJoinChannel(userInput);
				IrcChangeNick(userInput);
				IrcGetNames(userInput);
				IrcLeaveChannel(userInput);
				IrcCTCP(userInput);
				IrcQuit(userInput);							
				//ircWriter.Flush();
				//log.Close();
				
			}

		}

		static void Main(string[] cmdLine)
		{		
			Program irc = new Program();
			irc.hostName = (String)cmdLine.GetValue(0);
            irc.nick = (String)cmdLine.GetValue(1);
			irc.log = irc.OpenLogFile();

			IPHostEntry iphostinfo = Dns.GetHostEntry(irc.hostName);
			IPAddress ipaddr = iphostinfo.AddressList[0];
			IPEndPoint ep = new IPEndPoint(ipaddr, 6667);

			irc.sock = new Socket(ep.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			irc.currentChannel = null;
			irc.sock.Connect(ep);

			Thread thread = new Thread(new ThreadStart(irc.ServerThread)); 
			thread.Start();
			Thread Secondthread = new Thread(new ThreadStart(irc.InputThread));
			Secondthread.Start();

			Console.WriteLine("Press any key to continue ....");
			Console.ReadKey(true);
		}
	}
}