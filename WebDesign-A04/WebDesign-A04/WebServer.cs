using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WebDesign_A04
{
    class WebServer
    {

        public bool ServerStart(string[] args)
        {
            if(args.Length != 4)
            {
                Console.WriteLine("Not Enough Commands Were Entered To Retrieve The Wanted Information.\n");
                return false;
            }
            byte[] data = new byte[15000];
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
            }

            if(webRoot == null || webIP == null || webPort == null)
            {
                Console.WriteLine("A mandatory field has not been entered! ''-webRoot='' or ''-webIP='' or ''-webPort=''");
                return false;
            }
            //USER WILL BE ENTERING -WebRoot= for the root of the website -WebIP-For the IP of the computer, -WebPort to enter the port
            //Also at the beginning entering a file to search.
            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(webIP), Convert.ToInt32(webPort));

            Socket server = new Socket(AddressFamily.InterNetwork,
                                       SocketType.Stream,
                                       ProtocolType.Tcp);

            try
            {
                server.Connect(ipep);
            }
            catch (SocketException ex)
            {
                Console.WriteLine("Unable to connect to server.");
                Console.WriteLine(ex.ToString());
                return false;
            }

            //REALLY NOT SURE IF THIS WORKS?? HARD TO TEST ON ONE COMPUTER
            string serverIP = localIPAddress(webIP);
            strRequest = "GET " + webRoot + " HTTP/1.1\r\n" + "HOST: " + serverIP + "\r\n" + "\r\n";

            server.Send(Encoding.ASCII.GetBytes(strRequest));   // send off the request

            System.Threading.Thread.Sleep(1000);

            int recv = 0;
            while (server.Available > 0)                          // let's read the response and print it out
            {
                recv = server.Receive(data);

                stringData = Encoding.ASCII.GetString(data, 0, recv);

                int isImage = stringData.IndexOf("Content-Type: image/jpeg");
                int isHTML = stringData.IndexOf("Content-Type: text/html");
                int isText = stringData.IndexOf("Content-Type: text/plain");

                if (isImage > 0)
                {
                    // find the \r\n\r\n and cut the string short at that point
                    int imageStart = stringData.IndexOf("\r\n\r\n");

                    //Will need to disect file path and just get end location... Then take that and save it.
                    int lastSlashIndex = stringData.LastIndexOf("\\", System.StringComparison.Ordinal);


                    string filePath = stringData.Substring(lastSlashIndex);
                        //"/temp/test.jpg";

                    Image x = (Bitmap)((new ImageConverter()).ConvertFrom(data[imageStart]));




                }
                else if (isHTML > 0)
                {
                    //Change file path to be less genric... But opens internet Explorer.
                        string filePath = "/temp/test.html";
                        int textStart = stringData.IndexOf("\r\n\r\n");
                        System.IO.File.WriteAllText(filePath, stringData.Substring((textStart)));

                    Process.Start("IExplore.exe", "file:///C:/temp/test.html");

                }
                else if (isText > 0)
                {
                    //MAY WANT TO FIX FILE LOCATION
                    string filePath = "/temp/test.txt";
                    int textStart  = stringData.IndexOf("\r\n\r\n");
                    System.IO.File.WriteAllText(filePath, stringData.Substring((textStart)));

                    string notepadPath = Environment.SystemDirectory + "\\notepad.exe";

                    var startInfo = new ProcessStartInfo(notepadPath)
                    {
                        WindowStyle = ProcessWindowStyle.Maximized,
                        Arguments = filePath
                    };

                    Process.Start(startInfo);
                        
                }
                else
                {
                    Console.WriteLine(stringData + "\r\n");      // simply add the entire response
                }
            }


            Console.WriteLine("Disconnecting from server...\r\n");
            server.Shutdown(SocketShutdown.Both);
            server.Close();

            return true;
        }


        /*
         * Name:    localIPAddress
         * Purpose: This method looks at the ip addresses on the client and compares each to the servers IP,
         *          The ip that matches the first octet of the server's ip is choosen as the client's ip.
         * Inputs:  string matchIP: The server's IP address provided by the user through the UI.
         * Outputs: N/A
         * Returns: string: the ip address of the client to use for the message queues.
         * 
         */
        //Credit: https://stackoverflow.com/questions/6803073/get-local-ip-address
        public string localIPAddress(string matchIP)
        {
            IPHostEntry host;
            string localIP = "";
            string choosenIP = null;
            host = Dns.GetHostEntry(Dns.GetHostName());
            string[] buff = matchIP.Split('.');

            foreach (IPAddress ip in host.AddressList)
            {
                localIP = ip.ToString();

                string[] temp = localIP.Split('.');

                if (ip.AddressFamily == AddressFamily.InterNetwork && temp[0] == buff[0])
                {
                    choosenIP = ip.ToString();
                }
                else
                {
                    localIP = null;
                }
            }

            return choosenIP;
        }
    }

}
