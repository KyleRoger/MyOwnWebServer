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
                Byte[] bytes = new Byte[150000];
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

                        //for 400 errors like file not found
                        string badRequest = "HTTP/1.1 400 Bad Request\r\nConnection: close\r\nContent-Length: 180\r\n\r\n<!DOCTYPE HTML>\r\n<HTML><HEAD><TITLE>Bad Request</TITLE>\r\n</HEAD>\r\n<BODY><h2>Bad Request - Invalid URL</h2>\r\n<hr><p>HTTP Error 400. The request URL is invalid.</p>\r\n</BODY></HTML>\r\n";
                        
                        //for 501 errors like a post request
                        //*************************************need to clean up message.
                        string notImplemented = "HTTP/1.1 501 Not Implemented\r\nCache-Control: private\r\nContent-Type: text/html; charset=utf-8\r\nServer: Microsoft-IIS/10.0\r\nX-Powered-By: ASP.NET\r\nDate: Sun, 25 Nov 2018 03:14:28 GMT\r\nContent-Length: 5081\r\n\r\n<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Strict//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd\"> \n<html xmlns=\"http://www.w3.org/1999/xhtml\"> \n<head> \n<title>IIS 10.0 Detailed Error - 501.0 - Not Implemented</title> \n<style type=\"text/css\"> \n<!-- \nbody{margin:0;font-size:.7em;font-family:Verdana,Arial,Helvetica,sans-serif;} \ncode{margin:0;color:#006600;font-size:1.1em;font-weight:bold;} \n.config_source code{font-size:.8em;color:#000000;} \npre{margin:0;font-size:1.4em;word-wrap:break-word;} \nul,ol{margin:10px 0 10px 5px;} \nul.first,ol.first{margin-top:5px;} \nfieldset{padding:0 15px 10px 15px;word-break:break-all;} \n.summary-container fieldset{padding-bottom:5px;margin-top:4px;} \nlegend.no-expand-all{padding:2px 15px 4px 10px;margin:0 0 0 -12px;} \nlegend{color:#333333;;margin:4px 0 8px -12px;_margin-top:0px; \nfont-weight:bold;font-size:1em;} \na:link,a:visited{color:#007EFF;font-weight:bold;} \na:hover{text-decoration:none;} \nh1{font-size:2.4em;margin:0;color:#FFF;} \nh2{font-size:1.7em;margin:0;color:#CC0000;} \nh3{font-size:1.4em;margin:10px 0 0 0;color:#CC0000;} \nh4{font-size:1.2em;margin:10px 0 5px 0; \n}#header{width:96%;margin:0 0 0 0;padding:6px 2% 6px 2%;font-family:\"trebuchet MS\",Verdana,sans-serif; \n color:#FFF;background-color:#5C87B2; \n}#content{margin:0 0 0 2%;position:relative;} \n.summary-container,.content-container{background:#FFF;width:96%;margin-top:8px;padding:10px;position:relative;} \n.content-container p{margin:0 0 10px 0; \n}#details-left{width:35%;float:left;margin-right:2%; \n}#details-right{width:63%;float:left;overflow:hidden; \n}#server_version{width:96%;_height:1px;min-height:1px;margin:0 0 5px 0;padding:11px 2% 8px 2%;color:#FFFFFF; \n background-color:#5A7FA5;border-bottom:1px solid #C1CFDD;border-top:1px solid #4A6C8E;font-weight:normal; \n font-size:1em;color:#FFF;text-align:right; \n}#server_version p{margin:5px 0;} \ntable{margin:4px 0 4px 0;width:100%;border:none;} \ntd,th{vertical-align:top;padding:3px 0;text-align:left;font-weight:normal;border:none;} \nth{width:30%;text-align:right;padding-right:2%;font-weight:bold;} \nthead th{background-color:#ebebeb;width:25%; \n}#details-right th{width:20%;} \ntable tr.alt td,table tr.alt th{} \n.highlight-code{color:#CC0000;font-weight:bold;font-style:italic;} \n.clear{clear:both;} \n.preferred{padding:0 5px 2px 5px;font-weight:normal;background:#006633;color:#FFF;font-size:.8em;} \n--> \n</style> \n \n</head> \n<body> \n<div id=\"content\"> \n<div class=\"content-container\"> \n  <h3>HTTP Error 501.0 - Not Implemented</h3> \n  <h4>The page you are looking for cannot be displayed because a header value in the request does not match configuration settings.</h4> \n</div> \n<div class=\"content-container\"> \n <fieldset><h4>Most likely causes:</h4> \n  <ul> \t<li>The request contains the HTTP verb \"TRACE\" and the registry value to enable this method is not configured on the server.</li> </ul> \n </fieldset> \n</div> \n<div class=\"content-container\"> \n <fieldset><h4>Things you can try:</h4> \n  <ul> \t<li>           If the server should allow the \"TRACE\" method, create a new DWORD registry parameter named \"EnableTraceMethod\" and set its value to 1 at the following location in the registry.<br></br><br></br>HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\W3SVC\\Parameters         </li> \t<li>Create a tracing rule to track failed requests for this HTTP status code. For more information about creating a tracing rule for failed requests, click <a href=\"http://go.microsoft.com/fwlink/?LinkID=66439\">here</a>. </li> </ul> \n </fieldset> \n</div> \n \n<div class=\"content-container\"> \n <fieldset><h4>Detailed Error Information:</h4> \n  <div id=\"details-left\"> \n   <table border=\"0\" cellpadding=\"0\" cellspacing=\"0\"> \n    <tr class=\"alt\"><th>Module</th><td>&nbsp;&nbsp;&nbsp;ProtocolSupportModule</td></tr> \n    <tr><th>Notification</th><td>&nbsp;&nbsp;&nbsp;ExecuteRequestHandler</td></tr> \n    <tr class=\"alt\"><th>Handler</th><td>&nbsp;&nbsp;&nbsp;TRACEVerbHandler</td></tr> \n    <tr><th>Error Code</th><td>&nbsp;&nbsp;&nbsp;0x00000000</td></tr> \n     \n   </table> \n  </div> \n  <div id=\"details-right\"> \n   <table border=\"0\" cellpadding=\"0\" cellspacing=\"0\"> \n    <tr class=\"alt\"><th>Requested URL</th><td>&nbsp;&nbsp;&nbsp;http://localhost:80/index.html</td></tr> \n    <tr><th>Physical Path</th><td>&nbsp;&nbsp;&nbsp;C:\\inetpub\\wwwroot\\index.html</td></tr> \n    <tr class=\"alt\"><th>Logon Method</th><td>&nbsp;&nbsp;&nbsp;Anonymous</td></tr> \n    <tr><th>Logon User</th><td>&nbsp;&nbsp;&nbsp;Anonymous</td></tr> \n     \n   </table> \n   <div class=\"clear\"></div> \n  </div> \n </fieldset> \n</div> \n \n<div class=\"content-container\"> \n <fieldset><h4>More Information:</h4> \n  This error is returned only when the HTTP verb is Trace and the EnableTraceMethod registry value is not set to 1. If this error is occurring for another reason, the registry value is probably set by a third-party application.  \n  <p><a href=\"https://go.microsoft.com/fwlink/?LinkID=62293&amp;IIS70Error=501,0,0x00000000,17134\">View more information &raquo;</a></p> \n   \n </fieldset> \n</div> \n</div> \n</body> \n</html> \n";  
                        
                        if (this.parseRequest(data, out path))
                        {
                            string mimeType = this.GetMimeType(path);
                            path = webRoot + path;

                            string buf = null;
                        
                            //string mimeType = GetMimeType(data);

                            if (this.readFile(path, out buf))
                            {
                                if (mimeType.Equals("text/html"))
                                {
                                    int contentLength = buf.Length;
                                    responseMessage = "HTTP/1.1 200 OK\r\nContent-Type: text/html\r\nLast-Modified: " + DateTime.Now.ToString("r") + " GMT\r\nAccept-Ranges: bytes\r\nETag: \"a25cf8d78583d41:0\"\r\nServer: Simple-Server\r\nX-Powered-By: C#\r\nDate: " + DateTime.Now.ToString("r") + "\r\nContent-Length:" + contentLength + "\r\n\r\n" + buf;
                                }
                            
                                else if (mimeType.Equals("image/gif"))
                                {
                                    int contentLength = buf.Length;
                                    responseMessage = "HTTP/1.1 200 OK\r\nContent-Type: " + mimeType + "\r\nLast-Modified: " + DateTime.Now.ToString("r") + " GMT\r\nAccept-Ranges: bytes\r\nETag: \"a25cf8d78583d41:0\"\r\nServer: Simple-Server\r\nX-Powered-By: C#\r\nDate: " + DateTime.Now.ToString("r") + "\r\nContent-Length:" + contentLength + "\r\n\r\n" + buf;

                                }
                                else if (mimeType.Equals("image/jpeg"))
                                {
                                    //byte[] imageByteArray;
                                    
                                    //imageByteArray = GetBytesFromImage(path);
                                    //Image ii = Image.FromFile(path);
                                    Byte[] bt = new Byte[150000];
                                    bt = File.ReadAllBytes(path);

                                    int contentLength = bt.Length;
                                    responseMessage = "HTTP/1.1 200 OK\r\nContent-Type: " + mimeType + "\r\nLast-Modified: " + DateTime.Now.ToString("r") + " GMT\r\nAccept-Ranges: bytes\r\nETag: \"a25cf8d78583d41:0\"\r\nServer: Simple-Server\r\nX-Powered-By: C#\r\nDate: " + DateTime.Now.ToString("r") + "\r\nContent-Length:" + contentLength + "\r\n\r\n";
                                    Byte[] temp = new Byte[1500];
                                    temp = System.Text.Encoding.ASCII.GetBytes(responseMessage);                                    
                                    stream.Write(temp, 0, temp.Length);
                                    stream.Write(bt, 0, bt.Length);

                                }
                                else if (mimeType.Equals("text/plain"))
                                {
                                    
                                }
                            }

                            else
                            {
                                //400 error since file not found.
                                responseMessage = badRequest;
                            }
                        }
                        else
                        {
                            responseMessage = notImplemented;
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
        private byte[] GetBytesFromImage(String imageFile)
        {
            MemoryStream ms = new MemoryStream();
            Image img = Image.FromFile(imageFile);
            img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);

            return ms.ToArray();
        }

        private bool readFile(string path, out string message)
        {
            bool success = false;
            message = null;

            if (File.Exists(path))
            {
                try
                {
                    message = File.ReadAllText(path, Encoding.ASCII);
                    string mes = File.ReadAllText(path, Encoding.Default);
                    string mess = File.ReadAllText(path, Encoding.Unicode);
                    success = true;
                    Image i = Image.FromFile(path);

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
            string pathRex = @"((?<=GET )([\w./-0-9]+)(?= HTTP))";   //((?<=GET )([A-Za-z0-9/.-]*)(?= HTTP))";
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

        ////Credit:https://www.codeproject.com/Articles/452052/Build-Your-Own-Web-Server
        //private void notImplemented(Socket clientSocket)
        //{

        //    sendResponse(clientSocket, "<html><head><meta http - equiv =\"Content-Type\" content=\"text/html; charset = utf - 8\"> </ head > \r\n< body >\r\n< h2 >Simple Web Server </ h2 >\r\n< div > 501 - Method Not Implemented </ div >\r\n</ body >\r\n</ html > ", "501 Not Implemented", "text/html");
        //}

        ////Credit:https://www.codeproject.com/Articles/452052/Build-Your-Own-Web-Server
        //private void notFound(Socket clientSocket)
        //{

        //    sendResponse(clientSocket, "<html><head><meta http - equiv =\"Content-Type\" content=\"text/html; charset = utf - 8\"></head><body><h2>Atasoy Simple Web Server </ h2 >< div > 404 - Not Found </ div ></ body ></ html > ", "404 Not Found", "text/html");
        //}

        ////Credit:https://www.codeproject.com/Articles/452052/Build-Your-Own-Web-Server
        //private void sendOkResponse(Socket clientSocket, byte[] bContent, string contentType)
        //{
        //    sendResponse(clientSocket, bContent, "200 OK", contentType);
        //}

        ////Credit:https://www.codeproject.com/Articles/452052/Build-Your-Own-Web-Server
        //private Encoding charEncoder = Encoding.UTF8; // To encode string
        //// For strings
        //private void sendResponse(Socket clientSocket, string strContent, string responseCode,
        //                          string contentType)
        //{
        //    byte[] bContent = charEncoder.GetBytes(strContent);
        //    sendResponse(clientSocket, bContent, responseCode, contentType);
        //}

        ////Credit:https://www.codeproject.com/Articles/452052/Build-Your-Own-Web-Server
        //// For byte arrays
        //private void sendResponse(Socket clientSocket, byte[] bContent, string responseCode,
        //                          string contentType)
        //{
        //    try
        //    {
        //        byte[] bHeader = charEncoder.GetBytes(
        //                            "HTTP/1.1 " + responseCode + "\r\n"
        //                          + "Server: Atasoy Simple Web Server\r\n"
        //                          + "Content-Length: " + bContent.Length.ToString() + "\r\n"
        //                          + "Connection: close\r\n"
        //                          + "Content-Type: " + contentType + "\r\n\r\n");
        //        clientSocket.Send(bHeader);
        //        clientSocket.Send(bContent);
        //        clientSocket.Close();
        //    }
        //    catch { }
        //}
    }
}
