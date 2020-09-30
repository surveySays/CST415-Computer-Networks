// ServerProgram.cs
// Pete Myers
// CST415, OIT Fall 2019
//
// Sample UDP application
//
// This is the UDP Server program
// Binds a UDP socket to port 30000
// Goes into an infinite loop where it...
//    Receives a UDP datagram, converts it to an array of characters and prints the array as a string
//    Sends a UDP datagram, including a string converted to a character array and then to bytes
//
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace SampleUDPApp
{
    class ServerProgram
    {
        static void Main(string[] args)
        {
            int PORT = 30000;
            string RESPONSE_STRING = "Yes?";

            // create the socket for receiving messages at the server
            Socket listeningSocket = new Socket(SocketType.Dgram, ProtocolType.Udp);
            Console.WriteLine("Socket created");

            // bind the socket to a port
            listeningSocket.Bind(new IPEndPoint(IPAddress.Any, PORT));
            Console.WriteLine("Socket bound to port " + PORT.ToString());

            bool done = false;
            while (!done)
            {
                try
                {
                    // receive a message from a client
                    Console.WriteLine("Waiting for message from client...");
                    byte[] buffer = new byte[256];
                    EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                    int result = listeningSocket.ReceiveFrom(buffer, ref remoteEP);
                    Console.WriteLine("Received " + result.ToString() + " bytes: " + new string(ASCIIEncoding.UTF8.GetChars(buffer)));

                    // send message back to client
                    Console.WriteLine("Sending message to client...");
                    buffer = ASCIIEncoding.UTF8.GetBytes(RESPONSE_STRING);
                    result = listeningSocket.SendTo(buffer, remoteEP);
                    Console.WriteLine("Sent " + result.ToString() + " bytes: " + new string(ASCIIEncoding.UTF8.GetChars(buffer)));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception when receiving..." + ex.Message);
                }
            }

            // close the socket and quit
            Console.WriteLine("Closing down");
            listeningSocket.Close();
            Console.WriteLine("Closed!");

            Console.ReadKey();
        }
    }
}
