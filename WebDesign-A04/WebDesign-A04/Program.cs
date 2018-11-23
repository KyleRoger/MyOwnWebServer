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
        [STAThread]
        static void Main(string[] args)
        {
            Logger.Log("Starting web server.");

            WebServer server = new WebServer();
            bool accepted = true;
            Logger.Log("Arguments:");
            foreach (var arg in args)
            {
                Logger.Log("\t Receaved (" + arg.ToString() + ") as argument.");
            }
            
            accepted = server.ServerStart(args);
            Console.ReadKey();
            Logger.Log("Closing web server");
        }
    }
}
