using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace IRCclient
{
	class Program
	{
		public IPAddress getIp(String hostName) 
		{
			IPHostEntry myiHe = Dns.GetHostEntry(hostName);
			IPAddress myIp = myiHe.AddressList[0];
			return myIp;
		}

		static void Main(string[] cmdLine)
		{
			Program irc = new Program();
			int i = 1;
			foreach (string s in cmdLine) 
			{
				Console.WriteLine("Argument "+ i +" : "  + s);
				i++;	
			}

			int port = 194;
			String hostName = (String)cmdLine.GetValue(0);
			IPAddress ipAddress = irc.getIp(hostName);

			IPEndPoint ipEnd = new IPEndPoint(ipAddress, port);

			Socket myServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			myServer.Bind(ipEnd);
			myServer.Connect(ipAddress, port);
			Socket myClient = myServer.Accept();

			Console.WriteLine("Ip : "+ ipAddress.ToString());
			Console.WriteLine("Press any key to continue ....");
			Console.ReadKey(true);
		}
	}
}
