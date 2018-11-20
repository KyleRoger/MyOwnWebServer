using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

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
            byte[] data = new byte[8192];
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



            strRequest = "GET " + webRoot + " HTTP/1.1\r\n" + "HOST: " + "localhost" + "\r\n" + "\r\n";

            server.Send(Encoding.ASCII.GetBytes(strRequest));   // send off the request

            System.Threading.Thread.Sleep(1000);

            int recv = 0;
            while (server.Available > 0)                          // let's read the response and print it out
            {
                recv = server.Receive(data);

                stringData = Encoding.ASCII.GetString(data, 0, recv);

                // check if this is an image being returned ... the HTTPTool doesn't have the ability to 
                // support an image in the RESPONSE window ... so don't encode the returned data into ASCII 
                //   -- instead, output "IMAGE CONTENTS"
                //   -- assuming that the first occurance of the "\r\n\r\n" happens just before the encoded image contents
                //
                int isImage = stringData.IndexOf("Content-Type: image/jpeg");
                if (isImage > 0)
                {
                    // find the \r\n\r\n and cut the string short at that point
                    int imageStart = stringData.IndexOf("\r\n\r\n");
                    Console.WriteLine(stringData.Substring(0, (imageStart - 1)) + "\r\n\r\n[IMAGE DATA Found Here ...]\r\n");

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
    }
}
