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
		public void getIp(String hostName, IPAddress myIp)
		{
			try
			{
				IPHostEntry myiHe = Dns.GetHostEntry(hostName);
				myIp = myiHe.AddressList[0];
				Console.WriteLine("inni í falli :; " + myIp.ToString());
			}
			catch(System.Net.Sockets.SocketException ex)
			{
				Console.Error.WriteLine("Could not find Host..");
				
			}				
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
			IPAddress ipAddress = null;
			
			irc.getIp(hostName, ipAddress);			

			if( ipAddress != null)
			Console.WriteLine("Ip : "+ ipAddress.ToString());
			
			Console.WriteLine("Press any key to continue ....");
			Console.ReadKey(true);
		}
	}
}
