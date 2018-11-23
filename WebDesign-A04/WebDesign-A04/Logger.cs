/*
 * 
 * Author:      Arie Kraayenbrink
 * Date:        Nov, 2018
 * Project:     WebDesign-A04
 * File:        Logger.cs
 * Description: This is the logger class for assignment 6.
 * 
*/


using System.Diagnostics;



namespace WebDesign_A04
{
    class Logger
    {
        public static void Log(string message)
        {
            EventLog serverEventLog = new EventLog();
            if (!EventLog.SourceExists("ServerEventSource"))
            {
                EventLog.CreateEventSource("ServerEventSource", "ServerEventLog");
            }

            serverEventLog.Source = "ServerEventSource";
            serverEventLog.Log = "ServerEventLog";
            serverEventLog.WriteEntry(message);
        }
    }
}
