/*
 * Created by SharpDevelop.
 * User: TomHoracek
 * Date: 07/15/2015
 * Time: 13:24
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Net;
using System.Timers;

namespace Konachan
{
	/// <summary>
	/// Description of Pinger.
	/// </summary>
	public class Pinger
	{
		private static Pinger _instance;
		private Timer timer;
		private WebsiteStatus status;
		public event WebsiteStatusEventHandler WebsiteStatusChanged;
		
		private Pinger()
		{
			status = WebsiteStatus.Unknown;
			
			timer = new Timer();
			timer.Interval = 1000;
			timer.AutoReset = true;
			timer.Elapsed += timer_Elapsed;
			timer.Start();
		}

		void timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			if (isWebSiteAvailable("http://konachan.com/"))
			{
				if (status != WebsiteStatus.Up)
				{
					var previousStatus = status;
					status = WebsiteStatus.Up;
				
					onWebsiteStatusChanged(previousStatus, status);
				}
			}
			else
			{
				if (status != WebsiteStatus.Down)
				{
					var previousStatus = status;
					status = WebsiteStatus.Down;
				
					onWebsiteStatusChanged(previousStatus, status);
				}
			}
		}
		
		private bool isWebSiteAvailable(string Url)
		{
			string Message = string.Empty;
			var request = (HttpWebRequest)HttpWebRequest.Create(Url);

			// Set the credentials to the current user account
			request.Credentials = System.Net.CredentialCache.DefaultCredentials;
			request.Method = "GET";

			try
			{
				using (var response = (HttpWebResponse)request.GetResponse())
				{
					// Do nothing; we're only testing to see if we can get the response
				}
			}
			catch (WebException ex)
			{
				Message += ((Message.Length > 0) ? "\n" : "") + ex.Message;
			}

			return (Message.Length == 0);
		}
		
		private void onWebsiteStatusChanged(WebsiteStatus changedFrom, WebsiteStatus changedTo)
		{
			if (WebsiteStatusChanged != null)
			{
				var args = new WebsiteStatusEventArgs();
				args.ChangedFrom = changedFrom;
				args.ChangedTo = changedTo;
				WebsiteStatusChanged(this, args);
			}
		}
		
		public static Pinger GetInstance()
		{
			if (_instance == null)
				_instance = new Pinger();
			
			return _instance;
		}
		
	}
	
	public delegate void WebsiteStatusEventHandler(object sender, WebsiteStatusEventArgs e);
	
	public class WebsiteStatusEventArgs : EventArgs
	{
		public WebsiteStatus ChangedFrom;
		public WebsiteStatus ChangedTo;
	}
	
	public enum WebsiteStatus
	{
		Unknown,
		Up,
		Down
	}
}
