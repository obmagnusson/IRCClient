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
		public IPAddress getIp(String hostName, IPAddress myIp)
		{
			try
			{
				IPHostEntry myiHe = Dns.GetHostEntry(hostName);
				myIp = myiHe.AddressList[0];
				Console.WriteLine("dfdfdf");
			}
			catch (System.Net.Sockets.SocketException ex)
			{
				Console.Error.WriteLine("Could not find Host..");

			}
			return myIp;
		}

		static void Main(string[] cmdLine)
		{
			Program irc = new Program();
			int i = 1;
			foreach (string s in cmdLine)
			{
				Console.WriteLine("Argument " + i + " : " + s);
				i++;
			}

			int port = 6667;
			String hostName = (String)cmdLine.GetValue(0);
            String Nick = (String)cmdLine.GetValue(1);
            Console.WriteLine("óli er vitlaus" + Nick);
			IPAddress ipAddress = null;

			ipAddress = irc.getIp(hostName, ipAddress);

			TcpClient client = null;
			try
			{
				client = new TcpClient(hostName, port);

				Stream s = client.GetStream();
				StreamWriter log;
				StreamReader sr = new StreamReader(s);
				StreamWriter sw = new StreamWriter(s);
				Console.WriteLine(sr.ReadLine());

				if (!File.Exists("irc.log"))
				{
					log = new StreamWriter("irc.log");
				}
				else
				{
					log = File.AppendText("irc.log");
				}

				sw.WriteLine("CAP LS");
				sw.Flush();
				sw.WriteLine("NICK " + Nick);
				sw.Flush();
				sw.WriteLine("USER " + Nick + " 0 * :...");
				sw.Flush();
				sw.WriteLine("CAP REQ :multi-prefix");
				sw.Flush();
				sw.WriteLine("CAP END");
				sw.Flush();
				sw.WriteLine("USERHOST " + Nick);
				sw.Flush();


				while (true)
				{
					Console.Write(">: ");
					string cin = Console.ReadLine();
					Console.WriteLine(cin);

					sw.WriteLine(cin);
					log.WriteLine(DateTime.Now + ": " + cin);
					sw.Flush();


					if (cin == "QUIT")
					{
						client.Close();
						break;
					}
					Console.WriteLine("IRC SERVER : " + sr.ReadLine());
					log.WriteLine(DateTime.Now + ": IRC SERVER : " + sr.ReadLine());
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

			if (ipAddress != null)
				Console.WriteLine("IP : " + ipAddress.ToString());

			Console.WriteLine("Press any key to continue ....");
			Console.ReadKey(true);
		}
	}
}
