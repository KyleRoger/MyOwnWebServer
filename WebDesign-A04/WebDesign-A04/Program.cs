/*
 * 
 * Author:      Kyle Horsley, Arie Kraayenbrink      
 * Date:        November 25, 2018     
 * Project:     WebDesign-A04
 * File:        Program.cs
 * Description: Will work as a server and get information and send it back out to a client.
 * 
*/



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace WebDesign_A04
{
    class MyOwnWebServer
    {
     
        static void Main(string[] args)
        {
            //Initialize the log that the server is starting
            Logger.Log("Starting web server.");

            //Create a new Web Server
            WebServer server = new WebServer();
            
            //Log all command line arguments.
            Logger.Log("Arguments:");
            foreach (var arg in args)
            {
                Logger.Log("\t Received (" + arg.ToString() + ") as argument.");
            }
           
            //Start the server
            server.ServerStart(args);

            Logger.Log("Closing web server");
        }
    }
}
