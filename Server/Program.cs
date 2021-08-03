/*
- FILE : Program.cs
- PROJECT : PROG2121 - Assignment #05 - Server Side
- PROGRAMMER : Sky Roth
- FIRST VERSION : November 4, 2020
- LAST UPDATE   : November 9, 2020
- DESCRIPTION : This program will act as the server for the client chat programs, it will create a new server and handle the clients
-                   that connect to it. When new messages are sent, the server will read it and send it to the other connected clients
*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Server
{
    class Program
    {
        readonly static Dictionary<int, TcpClient> clientList = new Dictionary<int, TcpClient>();
        private static readonly int kMAXINPUTBYTE = 256;
        private static readonly Int32 kPORT = 5000;

        static void Main()
        {

            TcpListener server = null;

            //count how many clients are connected
            int count = 1;


            //taken from Demo #3 in TCP/IP
            try
            {
                //any ip address so that the user can input anything they want
                server = new TcpListener(IPAddress.Any, kPORT);

                // Start listening for client requests
                server.Start();

                //if the server cannot be connect, then catch the exception and return the function
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.ToString());
                return;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return;
            }
            //removed the 'finally' block from the examples so we don't 'stop' listening for new clients

            // Enter the listening loop
            while (true)
            {
                Console.WriteLine("Waiting for a connection...");

                //if another client is found, try to connect it
                TcpClient client = server.AcceptTcpClient();

                //add a new client to the dictionary
                clientList.Add(count, client);

                //start a new thread to the worker function, this will read a network stream for any new messages
                Thread thread = new Thread(Worker);
                thread.Start(count);

                Console.WriteLine("There are {0} clients connected", count);

                //add one to the count -> this is the key for the dictionary
                count += 1;
            }
        }

        public static void Worker(Object obj)
        {
            //this will be the messages that are passed
            String data = "";
            Byte[] bytes = new Byte[kMAXINPUTBYTE];

            //cast the passed object as an integer, this will act as the ID for the client list dictionary
            int id = (int)obj;
            int msg = 0;

            //find the client with the ID
            TcpClient client = clientList[id];

            // Get a stream object for reading and writing
            NetworkStream stream = client.GetStream();

            while ((msg = stream.Read(bytes, 0, bytes.Length)) > 0)
            {
                //convert the bytes array into a regular string
                data = System.Text.Encoding.ASCII.GetString(bytes, 0, msg);

                //Broadcast the data to all of the clients
                Broadcast(data);
            }

            clientList.Remove(id);
            client.Client.Shutdown(SocketShutdown.Both);
            client.Close();

            Console.WriteLine("[{0}] client disconnected", id);
        }



        public static void Broadcast(string data)
        {
            //this Broadcast function was taken from: https://stackoverflow.com/questions/43431196/c-sharp-tcp-ip-simple-chat-with-multiple-clients

            //grab the data that is found in the parameter (the message) and convert it into a byte array
            byte[] buffer = Encoding.ASCII.GetBytes(data + Environment.NewLine);

            //for each client that is running we will Broadcast a message to each one
            foreach (TcpClient c in clientList.Values)
            {
                //create a new network stream for each client, this will allow us to write to it
                NetworkStream stream = c.GetStream();
                //write to the stream with the byte array that we grabbed from above
                stream.Write(buffer, 0, buffer.Length);

            }

        }
    }
}
