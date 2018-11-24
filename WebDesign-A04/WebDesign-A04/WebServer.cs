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
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace WebDesign_A04
{
    class WebServer
    {
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

            //byte[] data = new byte[15000];
            string strRequest, stringData;
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

            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            ///
            /// Credit: https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets.tcplistener?view=netframework-4.7.2

            //string responseMessage = "HTTP/1.1 200 OK\r\nContent-Length: 68\r\n<!DOCTYPE html>\r\n<html>\r\n<head></head>\r\n<body>\r\nHi\r\n</body>\r\n</html>";
            //string test = "HTTP/1.1 200 OK\r\nContent-Length: 148\r\n\r\n<!DOCTYPE html>\r\n<html>\r\n  \r\n  <head>\r\n\t<title>A test page</title>\r\n </head>\r\n  <body>\r\n<p>\r\nHello There\r\n</p>\r\n</body>\r\n</html>";
            //responseMessage = "HTTP/1.1 200 OK\r\nContent-Type: text/html\r\nLast-Modified: Fri, 23 Nov 2018 23:39:57 GMT\r\nAccept-Ranges: bytes\r\nETag: \"a25cf8d78583d41:0\"\r\nServer: Microsoft-IIS/10.0\r\nX-Powered-By: ASP.NET\r\nDate: Fri, 23 Nov 2018 23:56:29 GMT\r\nContent-Length: 142\r\n\r\n<!DOCTYPE html>\r\n<html>\r\n  \r\n  <head>\r\n\t<title>A Test page</title>\r\n\r\n  </head>\r\n  <body>\r\n\t\t<p>\r\n\t\t\tHello There\r\n\t\t</p>\r\n\r\n  </body>\r\n</html>";
            //int contentLength = this.readFile(@"C:\tmp\test.html").Length;
            //string root = @"C:\tmp";
            //string path = root + this.parseRequest();
            //responseMessage = "HTTP/1.1 200 OK\r\nContent-Type: text/html\r\nLast-Modified: Fri, 23 Nov 2018 23:39:57 GMT\r\nAccept-Ranges: bytes\r\nETag: \"a25cf8d78583d41:0\"\r\nServer: SET-Server\r\nX-Powered-By: ASP.NET\r\nDate: Fri, 23 Nov 2018 23:56:29 GMT\r\nContent-Length:" + contentLength + "\r\n\r\n" + this.readFile(root + @"\test.html");
            //"GET /test.html HTTP/1.1\r\nHost: 127.0.0.1:13000\r\nUser-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:63.0) Gecko/20100101 Firefox/63.0\r\nAccept: text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8\r\nAccept-Language: en-US,en;q=0.5\r\nAccept-Enc"

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

                // Buffer for reading data
                Byte[] bytes = new Byte[15000];
                String data = null;

                // Enter the listening loop.
                while (true)
                {
                    Console.Write("Waiting for a connection... ");

                    // Perform a blocking call to accept requests.
                    // You could also user server.AcceptSocket() here.
                    TcpClient client = server.AcceptTcpClient();
                    Console.WriteLine("Connected!");

                    data = null;

                    // Get a stream object for reading and writing
                    NetworkStream stream = client.GetStream();

                    int i;

                    // Loop to receive all the data sent by the client.
                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        // Translate data bytes to a ASCII string.
                        data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                        Console.WriteLine("Received: {0}", data);

                        string path = null;
                        string responseMessage = null;
                        string badRequest = "<!DOCTYPE html>< html >< head >< TITLE > Bad Request </ TITLE ></ head >\r\n< BODY >< h2 > Bad Request - Invalid URL </ h2 >\r\n< hr >< p > HTTP Error 400.The request URL is invalid.</ p >\r\n</ BODY ></ HTML >";

                        if (this.parseRequest(data, out path))
                        {
                            path = webRoot + path;

                            string buf = null;
                        
                            //string mimeType = GetMimeType(data);

                            if (this.readFile(path, out buf))
                            {
                                if (data.Contains(".html"))
                                {
                                    int contentLength = buf.Length;
                                    responseMessage = "HTTP/1.1 200 OK\r\nContent-Type: text/html\r\nLast-Modified: " + DateTime.Now.ToString("r") + " GMT\r\nAccept-Ranges: bytes\r\nETag: \"a25cf8d78583d41:0\"\r\nServer: Simple-Server\r\nX-Powered-By: C#\r\nDate: " + DateTime.Now.ToString("r") + "\r\nContent-Length:" + contentLength + "\r\n\r\n" + buf;
                                }
                            
                                else if (data.Contains(".txt"))
                                {
                                   
                                }
                                else if (data.Contains(".gif"))
                                {
                                    
                                }
                                else if (data.Contains(".jpeg"))
                                {
                                    
                                }
                            }

                            else
                            {
                                //404 error since file not found.
                                //buf = "HTTP/1.1 400 Bad Request\r\nContent - Type: text / html\r\ncharset = us - ascii\r\nServer: MyOwnWebServer Date: " + DateTime.Now.ToString("r") + " Connection: close\r\nContent - Length: ";
                                buf = "HTTP/1.1 400 Bad Request\r\nContent-Type: text/html; charset=us-ascii\r\nServer: Microsoft-HTTPAPI/2.0\r\nDate: " + DateTime.Now.ToString("r") + "\r\nConnection: close\r\nContent-Length: 324\r\n\r\n<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.01//EN\"\"http://www.w3.org/TR/html4/strict.dtd\">\r\n<HTML><HEAD><TITLE>Bad Request</TITLE>\r\n<META HTTP-EQUIV=\"Content-Type\" Content=\"text/html; charset=us-ascii\"></HEAD>\r\n<BODY><h2>Bad Request - Invalid URL</h2>\r\n<hr><p>HTTP Error 400. The request URL is invalid.</p>\r\n</BODY></HTML>\r\n";
                                //int contentLength = buf.Length;
                                //responseMessage = buf + contentLength;
                                responseMessage = buf;
                            }

                            //string path = webRoot + this.parseRequest(data);
                            //int contentLength = this.readFile(path).Length;
                            //string responseMessage = "HTTP/1.1 200 OK\r\nContent-Type: text/html\r\nLast-Modified: " + DateTime.Now.ToString("r") + " GMT\r\nAccept-Ranges: bytes\r\nETag: \"a25cf8d78583d41:0\"\r\nServer: Simple-Server\r\nX-Powered-By: C#\r\nDate: " + DateTime.Now.ToString("r") + "\r\nContent-Length:" + contentLength + "\r\n\r\n" + this.readFile(path);

                            // Process the data sent by the client.
                            //data = data.ToUpper();

                            //byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);
                            //byte[] msg = System.Text.Encoding.ASCII.GetBytes(responseMessage);

                            //// Send back a response.
                            //stream.Write(msg, 0, msg.Length);
                            //Console.WriteLine("Sent: {0}", data);
                        }
                        

                        byte[] msg = System.Text.Encoding.ASCII.GetBytes(responseMessage);

                        // Send back a response.
                        stream.Write(msg, 0, msg.Length);
                        Console.WriteLine("Sent: {0}", data);
                    }

                    // Shutdown and end connection
                    client.Close();
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                // Stop listening for new clients.
                server.Stop();
            }

            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            ////USER WILL BE ENTERING -WebRoot= for the root of the website -WebIP-For the IP of the computer, -WebPort to enter the port
            ////Also at the beginning entering a file to search.
            //IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(webIP), Convert.ToInt32(webPort));

            //Socket server = new Socket(AddressFamily.InterNetwork,
            //                           SocketType.Stream,
            //                           ProtocolType.Tcp);

            //try
            //{
            //    server.Connect(ipep);
            //}
            //catch (SocketException ex)
            //{
            //    Logger.Log("Unable to connect to server. Exception: " + ex.Message.ToString());

            //    Console.WriteLine("Unable to connect to server.");
            //    Console.WriteLine(ex.ToString());

            //    return false;
            //}
            //catch (Exception e)
            //{
            //    //catch general exceptions not already caught.
            //    Logger.Log("Exception: " + e.Message.ToString());
            //}

            ////Does Connect. Tested.
            //string serverIP = localIPAddress(webIP);
            //strRequest = "GET " + webRoot + " HTTP/1.1\r\n" + "HOST: " + serverIP + "\r\n" + "\r\n";

            //server.Send(Encoding.ASCII.GetBytes(strRequest));   // send off the request

            //System.Threading.Thread.Sleep(1000);

            //int recv = 0;
            //while (server.Available > 0)                          // let's read the response and print it out
            //{
            //    //How do we continue to enter bytes into the same file if more than 15000?
            //    recv = server.Receive(data);

            //    stringData = Encoding.ASCII.GetString(data, 0, recv);

            //    int isImage = stringData.IndexOf("Content-Type: image/jpeg");
            //    int isHTML = stringData.IndexOf("Content-Type: text/html");
            //    int isText = stringData.IndexOf("Content-Type: text/plain");

            //    if (isImage > 0)
            //    {
            //        // find the \r\n\r\n and cut the string short at that point
            //        int imageStart = stringData.IndexOf("\r\n\r\n");

            //        //Will need to disect file path and just get end location... Then take that and save it.
            //        int lastSlashIndex = stringData.LastIndexOf("\\", System.StringComparison.Ordinal);
                    
            //        string filePath = "/temp/test.jpg";
            //        //stringData.Substring(lastSlashIndex);
            //        //"/temp/test.jpg";
            //        System.IO.File.WriteAllText(filePath, stringData.Substring((imageStart)));                                   
            //    }
            //    else if (isHTML > 0)
            //    {
            //        //Change file path to be less genric... But opens internet Explorer.
            //            string filePath = "/temp/test.html";
            //            int textStart = stringData.IndexOf("\r\n\r\n");
            //            System.IO.File.WriteAllText(filePath, stringData.Substring((textStart)));

            //        Process.Start("IExplore.exe", "file:///C:/temp/test.html");
            //    }
            //    else if (isText > 0)
            //    {
            //        //MAY WANT TO FIX FILE LOCATION
            //        string filePath = "/temp/test.txt";
            //        int textStart  = stringData.IndexOf("\r\n\r\n");
            //        System.IO.File.WriteAllText(filePath, stringData.Substring((textStart)));

            //        string notepadPath = Environment.SystemDirectory + "\\notepad.exe";

            //        var startInfo = new ProcessStartInfo(notepadPath)
            //        {
            //            WindowStyle = ProcessWindowStyle.Maximized,
            //            Arguments = filePath
            //        };

            //        Process.Start(startInfo);                        
            //    }
            //    else
            //    {
            //        Console.WriteLine(stringData + "\r\n");      // simply add the entire response
            //    }
            //}
            
            //Console.WriteLine("Disconnecting from server...\r\n");
            //server.Shutdown(SocketShutdown.Both);
            //server.Close();

            return true;
        }



        /*
         * Name:    localIPAddress
         * Purpose: This method looks at the ip addresses on the client and compares each to the servers IP,
         *          The ip that matches the first octet of the server's ip is choosen as the client's ip.
         * Inputs:  string matchIP: The server's IP address provided by the user through the UI.
         * Outputs: N/A
         * Returns: string: the ip address of the client to use.
         * 
         */
        //Credit: https://stackoverflow.com/questions/6803073/get-local-ip-address
        //private string localIPAddress(string matchIP)
        //{
        //    IPHostEntry host;
        //    string localIP = "";
        //    string choosenIP = null;
        //    host = Dns.GetHostEntry(Dns.GetHostName());
        //    string[] buff = matchIP.Split('.');

        //    foreach (IPAddress ip in host.AddressList)
        //    {
        //        localIP = ip.ToString();

        //        string[] temp = localIP.Split('.');

        //        if (ip.AddressFamily == AddressFamily.InterNetwork && temp[0] == buff[0])
        //        {
        //            choosenIP = ip.ToString();
        //        }
        //        else
        //        {
        //            localIP = null;
        //        }
        //    }

        //    return choosenIP;
        //}
        

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

        //Credit: https://stackoverflow.com/questions/1029740/get-mime-type-from-filename-extension
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

        private bool parseRequest(string message, out string path)
        {
            bool success = false;
            path = null;
            string pathRex = @"((?<=GET )([A-Za-z0-9/.]*)(?= HTTP))";
            string methodRex = @"^(GET)";

            if (Regex.IsMatch(message, methodRex))
            {
                path = Regex.Match(message, pathRex, RegexOptions.IgnoreCase).Value.ToString();

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
