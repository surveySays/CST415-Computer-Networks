//ClientProgram.cs
// Pete Myers
// CST415, OIT Fall 2019
//
// Sample UDP application
//
// This is the UDP Client program
// Creates an end point to the server by it's IP address and port (default IP 127.0.0.1 and port 30000)
// Sends a UDP datagram, including a message string converted to a character array and then to bytes
// Receives a UDP datagram, converts it to an array of characters and prints the array as a string
// Closes the socket
// 
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace SampleUDPClient
{
    class ClientProgram
    {
        static void Main(string[] args)
        {
            string ADDRESS = "127.0.0.1";
            int PORT = 30000;
            string MESSAGE_STRING = "Hello Server!";

            // create the socket for sending messages to the server
            Socket clientSocket = new Socket(SocketType.Dgram, ProtocolType.Udp);
            Console.WriteLine("Socket created");

            // construct the server's address and port
            IPEndPoint endPt = new IPEndPoint(IPAddress.Parse(ADDRESS), PORT);
            
            try
            {
                // send a message to the server
                Console.WriteLine("Sending message to server...");
                byte[] buffer = ASCIIEncoding.UTF8.GetBytes(MESSAGE_STRING);
                int result = clientSocket.SendTo(buffer, endPt);
                Console.WriteLine("Sent " + result.ToString() + " bytes: " + new string(ASCIIEncoding.UTF8.GetChars(buffer)));

                // receive a message from the server
                Console.WriteLine("Waiting for message from server...");
                buffer = new byte[54];
                EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                result = clientSocket.ReceiveFrom(buffer, ref remoteEP);
                Console.WriteLine("Received " + result.ToString() + " bytes: " + new string(ASCIIEncoding.UTF8.GetChars(buffer)));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception when receiving..." + ex.Message);
            }

            // close the socket and quit
            Console.WriteLine("Closing down");
            clientSocket.Close();
            Console.WriteLine("Closed!");

            Console.ReadKey();
        }
    }
}
