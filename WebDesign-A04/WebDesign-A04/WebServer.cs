/*
 * 
 * Author:      Arie Kraayenbrink, Kyle Horsley
 * Date:        Nov, 2018
 * Project:     Assignment 6
 * File:        WebServer.cs
 * Description: This is the server for assignment 6.
 * 
*/



using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;



namespace WebDesign_A04
{

    /*
    * Name:    
    * Purpose:
    */
    class WebServer
    {
        //for 400 errors like file not found
        string badRequest = "HTTP/1.1 400 Bad Request\r\nConnection: close\r\nContent-Length:" +
            " 180\r\n\r\n<!DOCTYPE HTML>\r\n<HTML><HEAD><TITLE>Bad Request</TITLE>\r\n</HEAD>\r\n" +
            "<BODY><h2>Bad Request - Invalid URL</h2>\r\n<hr><p>HTTP Error 400. The request URL is invalid.</p>\r\n</BODY></HTML>\r\n";

        //for 501 errors like a post request
        string notImplemented = "HTTP/1.1 501 Not Implemented\r\nCache-Control: private\r\nContent-Type: text/html; charset=utf-8\r\n" +
            "Server: MyOwnWebServer\r\nDate: " + DateTime.Now.ToString("r") + "\r\nContent-Length: 436\r\n\r\n" +
            "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Strict//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd\"> \n" +
            "<html xmlns=\"http://www.w3.org/1999/xhtml\"> \n<head> \n" +
            "<title>Error - 501.0 - Not Implemented</title> \n \n \n</head> \n" +
            "<body>  \n  <h3>HTTP Error 501.0 - Not Implemented</h3> \n  " +
            "<h4>The page you are looking for cannot be displayed because a header value in the request does not match configuration settings.</h4>\n" +
            "</body> \n</html> \n";

        // Buffer for reading data
        Byte[] bytes = new Byte[1500000];   //make sure we can handle larger file sizes
        String requestString = null;
        string responseMessage = null;



        /*
        * Name:    
        * Purpose: 
        * Inputs:  
        * Outputs: 
        * Returns:
        * Credit: https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets.tcplistener?view=netframework-4.7.2
        */
        public bool ServerStart(string[] args)
        {
            if(args.Length < 3)
            {                
                Logger.Log("Not Enough Commands Were Entered To Retrieve The Wanted Information.\n");
                return false;
            }
            else if (args.Length > 3)
            {
                Logger.Log("Too many Commands Were Entered as arguments.\n");
                return false;
            }

            string webRoot = null;
            string webIP = null;
            string webPort = null;
            
            foreach(string argument in args)
            {
                if(argument.Contains("-webRoot="))
                {
                    webRoot = argument;
                    webRoot = webRoot.Replace("-webRoot=", "");
                }
                else if(argument.Contains("-webIP="))
                {
                    webIP = argument;
                    webIP = webIP.Replace("-webIP=", "");
                }
                else if(argument.Contains("-webPort="))
                {
                    webPort = argument;
                    webPort = webPort.Replace("-webPort=", "");
                }
                else
                {
                    Logger.Log(argument + "is not a valid command line argument.");
                    return false;
                }
            }

            if(webRoot == null || webIP == null || webPort == null)
            {
                Logger.Log("A mandatory field has not been entered! ''-webRoot='' or ''-webIP='' or ''-webPort=''");
                return false;
            }

            TcpListener server = null;
            try
            {
                // Set the TcpListener on port.
                Int32 port = Convert.ToInt32(webPort);
                IPAddress localAddr = IPAddress.Parse(webIP);

                // TcpListener server = new TcpListener(port);
                server = new TcpListener(localAddr, port);

                // Start listening for client requests.
                server.Start();                               

                // Enter the listening loop.
                while (true)
                {    
                    // Perform a blocking call to accept requests.
                    // You could also user server.AcceptSocket() here.
                    TcpClient client = server.AcceptTcpClient();

                    Logger.Log("Connected to client" + client.ToString());
                    
                    requestString = null;

                    // Get a stream object for reading and writing
                    NetworkStream stream = client.GetStream();

                    int i;

                    // Loop to receive all the data sent by the client.
                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        // Translate data bytes to a ASCII string.
                        requestString = System.Text.Encoding.ASCII.GetString(bytes, 0, i);

                        string path = null;
                            
                        //check that this request is the GET method
                        if (this.parseRequest(requestString, out path))
                        {
                            path = webRoot + path;
                            handleRequest(stream, path);                            
                        }
                        else
                        {
                            //501 error since not GET method
                            responseMessage = notImplemented;
                            SendMessage(responseMessage, stream);
                        }
                    }

                    // Shutdown and end connection
                    client.Close();
                }
            }
            catch (SocketException e)
            {
                Logger.Log("SocketException: " + e.Message.ToString());
            }
            catch(Exception e)
            {
                Logger.Log(e.Message.ToString());
            }
            finally
            {
                // Stop listening for new clients.
                server.Stop();
            }

            return true;
        }



        /*
        * Name:    handleRequest
        * Purpose: This method checks for supported file formats and sends back the requested file to the client if supported.
        * Inputs:  NetworkStream stream; The client's socket stream to send the message to.
        *          string path: The path to the file to read.
        * Outputs: N/A
        * Returns: N/A
        */
        private void handleRequest(NetworkStream stream, string path)
        {
            string mimeType = this.GetMimeType(path);
            
            string requestMessage = null;

            //Check for supported file formats
            if (this.readFile(path, out requestMessage))
            {
                if (mimeType.Equals("text/html"))
                {
                    int contentLength = requestMessage.Length;
                    responseMessage = "HTTP/1.1 200 OK\r\nContent-Type: text/html\r\nAccept-Ranges: bytes\r\nETag: \"a25cf8d78583d41:0\"\r\n" +
                        "Server: MyOwnWebServer\r\nX-Powered-By: C#\r\nDate: " + DateTime.Now.ToString("r") +
                        "\r\nContent-Length:" + contentLength + "\r\n\r\n" + requestMessage;
                    SendMessage(responseMessage, stream);
                }
                else if (mimeType.Equals("image/gif") && readFile(path, out bytes))
                {
                    int contentLength = bytes.Length;
                    responseMessage = "HTTP/1.1 200 OK\r\nContent-Type: " + mimeType + "\r\nAccept-Ranges: bytes\r\nETag: \"a25cf8d78583d41:0\"\r\n" +
                        "Server: Simple-Server\r\nDate: " + DateTime.Now.ToString("r") + "\r\nContent-Length:" + contentLength + "\r\n\r\n";
                    SendMessage(responseMessage, stream);
                    SendMessage(bytes, stream);             //image file contents
                }
                else if (mimeType.Equals("image/jpeg") && readFile(path, out bytes))
                {
                    int contentLength = bytes.Length;
                    responseMessage = "HTTP/1.1 200 OK\r\nContent-Type: " + mimeType + "\r\nAccept-Ranges: bytes\r\nETag: \"a25cf8d78583d41:0\"\r\n" +
                        "Server: MyOwnWebServer\r\nDate: " + DateTime.Now.ToString("r") + "\r\nContent-Length:" + contentLength + "\r\n\r\n";
                    SendMessage(responseMessage, stream);   //
                    SendMessage(bytes, stream);             //image file contents
                }
                else if (mimeType.Equals("text/plain"))
                {
                    int contentLength = requestMessage.Length;
                    responseMessage = "HTTP/1.1 200 OK\r\nContent-Type: text/html\r\nAccept-Ranges: bytes\r\nETag: \"a25cf8d78583d41:0\"\r\n" +
                        "Date: " + DateTime.Now.ToString("r") + "\r\nContent-Length:" + contentLength + "\r\n\r\n" + requestMessage;
                    SendMessage(responseMessage, stream);
                }
                else
                {
                    //501 error since not a supported file type
                    responseMessage = notImplemented;
                    SendMessage(responseMessage, stream);
                }
            }
            else
            {
                //400 error since file not found.
                responseMessage = badRequest;
                SendMessage(responseMessage, stream);
            }
        }



        /*
        * Name:    
        * Purpose: This method sends out a string message to a connected client through TCP sockets.
        * Inputs:  string message: The message (as a string) to be sent out.
        *          NetworkStream stream: The client's socket stream to send the message to.
        * Outputs: N/A
        * Returns: N/A
        */
        private void SendMessage(string message, NetworkStream stream)
        {
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(message);

            // Send back a response.
            try
            {
                stream.Write(msg, 0, msg.Length);
                Logger.Log("Sent string to client");
            }
            catch (Exception e)
            {
                Logger.Log(e.Message.ToString());
            }
        }



        /*
        * Name:     SendMessage
        * Purpose:  This method sends out a byte array message to a connected client through TCP sockets.
        * Inputs:   byte[] msg: The message (as a byte array) to be sent out. 
        *           NetworkStream stream: The client's socket stream to send the message to.
        * Outputs:  N/A
        * Returns:  N/A
        */
        private void SendMessage(byte[] msg, NetworkStream stream)
        {           
            // Send back a response.
            try
            { 
                stream.Write(msg, 0, msg.Length);
                Logger.Log("Sent byte array to client");
            }
            catch (Exception e)
            {
                Logger.Log(e.Message.ToString());
            }
        }



        /*
        * Name:    
        * Purpose: 
        * Inputs:  
        * Outputs: 
        * Returns: 
        */
        private byte[] GetBytesFromImage(String imageFile)
        {
            MemoryStream ms = new MemoryStream();
            Image img = Image.FromFile(imageFile);
            img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);

            return ms.ToArray();
        }



        /*
        * Name:    readFile
        * Purpose: This method reads a specified file into a string.
        * Inputs:  string path: The path to the file to read
        * Outputs: string message: A string containing the contents read from the file.
        * Returns: bool: The success or failure of this method. True means everything is good, false means there was a problem.
        */
        private bool readFile(string path, out string message)
        {
            bool success = false;
            message = null;

            if (File.Exists(path))
            {
                try
                {
                    message = File.ReadAllText(path, Encoding.ASCII);
                    success = true;
                    Logger.Log("Read contents of (" + path + ")");
                }
                catch (Exception e)
                {
                    Logger.Log(e.Message.ToString());
                    success = false;
                }
            }
            else
            {
                Logger.Log("File (" + path + ") not found");
            }

            return success;
        }



        /*
        * Name:    readFile
        * Purpose: This method reads a specified file into a Byte array.
        * Inputs:  string path: The path to the file to read
        * Outputs: Byte[] message: The place to put the contents of the file after reading.
        * Returns: bool: The success or failure of this method. True means everything is good, false means there was a problem.
        */
        private bool readFile(string path, out Byte[] message)
        {
            bool success = false;
            message = null;

            if (File.Exists(path))
            {
                try
                {
                    message = File.ReadAllBytes(path);

                    success = true;

                    Logger.Log("Read contents of (" + path + ")");
                }
                catch (Exception e)
                {
                    Logger.Log(e.Message.ToString());
                    success = false;
                }
            }
            else
            {
                Logger.Log("File (" + path + ") not found");
            }

            return success;
        }


        /*
        * Name:    
        * Purpose: 
        * Inputs:  
        * Outputs: 
        * Returns: 
        * Credit: https://stackoverflow.com/questions/1029740/get-mime-type-from-filename-extension
        */
        private string GetMimeType(string fileName)
        {
            string mimeType = "application/unknown";
            string ext = System.IO.Path.GetExtension(fileName).ToLower();
            Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
            if (regKey != null && regKey.GetValue("Content Type") != null)
                mimeType = regKey.GetValue("Content Type").ToString();
            return mimeType;
        }

        //In here, look at the requested page... Check the page name for the extension. Check which extension .



        /*
        * Name:    parseRequest
        * Purpose: This method takes a string and parses out a file name and path if the message begins with GET
        * Inputs:  string message: The message to parse
        * Outputs: string path: The file path to get from the message
        * Returns: bool: The success or failure of this method. True means everything is good, false means there was a problem.
        */
        private bool parseRequest(string message, out string path)
        {
            bool success = false;
            path = null;
            string pathRex = @"((?<=GET )([\w./-0-9]+)(?= HTTP))"; 
            string methodRex = @"^(GET)";

            //only parse GET type messages
            if (Regex.IsMatch(message, methodRex))
            {
                path = Regex.Match(message, pathRex, RegexOptions.IgnoreCase).Value.ToString(); //strip out path from the message

                //watch out for black file names
                if (!path.Equals(null))
                {
                    string replacementReg = @"/";
                    string replacement = @"\";
                    Regex rgx = new Regex(replacementReg);
                    path = rgx.Replace(path, replacement);
                    success = true;
                }
            }
            else
            {
                Logger.Log("Error: method other then GET requested.");
            }

            return success;
        }
    }
}
