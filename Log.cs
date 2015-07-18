/*
 * Created by SharpDevelop.
 * User: Tom
 * Date: 15.7.2015
 * Time: 21:05
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using System.Runtime.ExceptionServices;

namespace Konachan
{
	/// <summary>
	/// Shamelessly stolen from LegendaryClient LOL
	/// </summary>
	public static class Logger
	{
		public static void CurrentDomain_FirstChanceException(object sender, FirstChanceExceptionEventArgs e)
		{
			WriteToLog.Log("A first chance exception was thrown", "EXCEPTION");
			WriteToLog.Log(e.Exception.Message, "EXCEPTION");
			WriteToLog.Log(e.Exception.StackTrace, "EXCEPTION");
		}

		public static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs x)
		{
			WriteToLog.Log("An unhandled exception was thrown", "UNHANDLEDEXCEPTION");
			var ex = (Exception)x.ExceptionObject;
			WriteToLog.Log(ex.Message, "UNHANDLEDEXCEPTION");
			WriteToLog.Log(ex.StackTrace, "UNHANDLEDEXCEPTION");
		}
		
		public static void Log(string lines, string type = "LOG")
		{
			WriteToLog.Log(lines, type);
		}
	}
	
	public static class WriteToLog
	{
		public static string ExecutingDirectory;
		public static string LogfileName;
			
		public static void Log(string lines, string type = "LOG")
		{
			using (FileStream stream = File.Open(Path.Combine(ExecutingDirectory, "Logs", LogfileName), FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
			using (var file = new StreamWriter(stream))
				file.WriteLine("({0} {1}) [{2}]: {3}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString(), type.ToUpper(), lines);
		}

		public static void CreateLogFile()
		{
			//Generate A Unique file to use as a log file
			if (!Directory.Exists(Path.Combine(ExecutingDirectory, "Logs")))
				Directory.CreateDirectory(Path.Combine(ExecutingDirectory, "Logs"));
			LogfileName = string.Format("{0}T{1} {2}", DateTime.Now.ToShortDateString().Replace("/", "_"),
				DateTime.Now.ToShortTimeString().Replace(" ", "").Replace(":", "-"), "_" + LogfileName);
			FileStream file = File.Create(Path.Combine(ExecutingDirectory, "Logs", LogfileName));
			file.Close();
		}
	}
}
