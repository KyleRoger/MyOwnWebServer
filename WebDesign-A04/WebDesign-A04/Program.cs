/*
 * 
 * Author:      Kyle Horsley      
 * Date:        November 19, 2018     
 * Project:     WebDesign-A04
 * File:        Program.cs
 * Description: Will work as a server and get information, parse it and display an html file.
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
            Logger.Log("Starting web server.");

            WebServer server = new WebServer();
            
            Logger.Log("Arguments:");
            foreach (var arg in args)
            {
                Logger.Log("\t Receaved (" + arg.ToString() + ") as argument.");
            }
            
            server.ServerStart(args);

            Logger.Log("Closing web server");
        }
    }
}
