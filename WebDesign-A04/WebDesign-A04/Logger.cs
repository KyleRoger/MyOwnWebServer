/*
 * 
 * Author:      Arie Kraayenbrink, Kyle Horsley
 * Date:        Nov 25, 2018
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
    /*
    * Name:    Logger
    * Purpose: TO log All Relevant information about a webserver to a file for documentation.
    */
    class Logger
    {
        /*
        * Name:    Log
        * Purpose: This function logs all application events, both successful and unsuccessful.
        *           All of the events are timestamped for help with records
        * Inputs:  message - the information that will be sent to the logger.
        * Outputs: N/A
        * Returns: N/A
        */
        public static void Log(string message)
        {
            //The name of the folder where the logger will be stored.
            string folderName = @"C:\WebServerLog";

            string path = folderName + @"\webServer.log";

            if (!Directory.Exists(folderName))  //Create a directory if it doesn't exists.
            {
                Directory.CreateDirectory(folderName);
            }
            using (StreamWriter streamWriter = new StreamWriter(path, append: true))
            {
                //Time-Stamp and write to logger.
                streamWriter.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ") + message);
                streamWriter.Close();
            }
        }
    }
}
