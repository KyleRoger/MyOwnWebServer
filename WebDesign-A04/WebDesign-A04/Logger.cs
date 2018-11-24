/*
 * 
 * Author:      Arie Kraayenbrink
 * Date:        Nov, 2018
 * Project:     WebDesign-A04
 * File:        Logger.cs
 * Description: This is the logger class for assignment 6.
 * 
*/



using System;
using System.IO;
using System.Diagnostics;
using System.Threading;



namespace WebDesign_A04
{
    class Logger
    {
        public static void ApplicationLog(string message)
        {
            EventLog serverEventLog = new EventLog();
            if (!EventLog.SourceExists("ServerEventSource"))
            {
                EventLog.CreateEventSource("ServerEventSource", "ServerEventLog");
                Thread.Sleep(1000); //Compensate for latency and allow log to be created.
            }

            serverEventLog.Source = "ServerEventSource";
            serverEventLog.Log = "ServerEventLog";
            serverEventLog.WriteEntry(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ") + message);
        }



        public static void Log(string message)
        {
            string path = @"c:\temp\webServer.log";

            using (StreamWriter streamWriter = new StreamWriter(path, append: true))
            {
                streamWriter.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ") + message);
                streamWriter.Close();
            }
        }
    }
}
