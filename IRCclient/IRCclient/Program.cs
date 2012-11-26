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
			}
		}

		private void IrcJoin(String line)
		{
			if (line.ToLower().StartsWith("/join"))
			{
				// JOIN #tsam
				// MODE #tsam
				String[] split = line.Split(' ');
				String command = split[0]; // /join
				String channel = split[1]; //#tsam				
				ircWriter.WriteLine(command.ToUpper().TrimStart('/')+" "+channel);
				ircWriter.WriteLine("MODE "+channel);
				currentChannel = channel;
			}
		}

		public void IrcQuit(String line)
		{
			// QUIT :
			if (line.ToLower().StartsWith("/quit"))
			{
				ircWriter.WriteLine(line.ToUpper().TrimStart('/') + " :" );
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
			}
		}

		private void IrcPong(String line)
		{
		// PING :calvino.freenode.net
		// PONG :calvino.freenode.net
		//	if(line.)
	
//			ircWriter.WriteLine("PONG " );
	//		ircWriter.Flush();
		}

		void ServerThread()
		{
			IPHostEntry iphostinfo = Dns.GetHostEntry("irc.freenode.net");
			IPAddress ipaddr = iphostinfo.AddressList[0];
			IPEndPoint ep = new IPEndPoint(ipaddr, 6667);
			Socket sock;
			Console.WriteLine("HAlló hér ");
			sock = new Socket(ep.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			currentChannel = null;
			sock.Connect(ep);

			ircWriter = new StreamWriter(new NetworkStream(sock));
			ircReader = new StreamReader(new NetworkStream(sock));
	 		Console.WriteLine("HAlló hér líka ");

			//				Stream serverStream = client.GetStream();
			StreamWriter log = OpenLogFile();


			//	StreamReader ircReader = new StreamReader(serverStream);
			//	StreamWriter ircWriter = new StreamWriter(serverStream);
			Console.WriteLine("þrír 3");
			Console.WriteLine(ircReader.ReadLine());


			//Start IrcServer Session
			IrcInit(ircWriter, nick);
			while (true)
			{
				Console.WriteLine("IRC SERVER : " + ircReader.ReadLine());
				log.WriteLine(GetDate() + "GMT : Server: " + ircReader.ReadLine());

				IrcPong(ircReader.ReadLine());

			}		
			//serverStream.Close();
			//log.Close();

		}
		void IrcChatMsg(String currentChannel, String line )
		{
			//PRIVMSG #CHANEL NO5 :"innihaldið"
			//PRIVMSG nickið :"innihaldið"
			if (!currentChannel.Equals(null))
			{
				ircWriter.WriteLine("PRIVMSG "+currentChannel+" :"+line);
			}

		}
		
		void InputThread()
		{

			while (true)
			{
				string userInput = Console.ReadLine();
				//Console.WriteLine(userInput);
				if (!userInput.StartsWith("/"))
				{
					IrcChatMsg(currentChannel, userInput);
				}
				IrcJoin(userInput);
				IrcChangeNick(userInput);
				IrcGetNames(userInput);
				IrcLeaveChannel(userInput);
				IrcQuit(userInput);
				
				//log.WriteLine(GetDate() + "GMT : Client: " + userInput);
				ircWriter.Flush();
			}

		}

		static void Main(string[] cmdLine)
		{		
			Program irc = new Program();
			irc.hostName = (String)cmdLine.GetValue(0);
            irc.nick = (String)cmdLine.GetValue(1);		
		
			Thread thread = new Thread(new ThreadStart(irc.ServerThread)); 
			thread.Start();
			Thread Secondthread = new Thread(new ThreadStart(irc.InputThread));
			Secondthread.Start();

			Console.WriteLine("Press any key to continue ....");
			Console.ReadKey(true);
		}
	}
}