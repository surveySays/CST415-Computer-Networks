﻿// FTClient.cs
//
// Brennen Boese
// CST 415
// Fall 2019
// 

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace FTClient
{
    class FTClient
    {
        private string ftServerAddress;
        private ushort ftServerPort;
        private bool connected;
        private Socket clientSocket;
        private NetworkStream stream;
        private StreamReader reader;
        private StreamWriter writer;

        public FTClient(string ftServerAddress, ushort ftServerPort)
        {
            // save server address/port
            this.ftServerAddress = ftServerAddress;
            this.ftServerPort = ftServerPort;

            // initialize to not connected to server
            connected = false;
            clientSocket = null;
            stream = null;
            reader = null;
            writer = null;
        }

        public void Connect()
        {        
            if (!connected)
            {
                // create a client socket and connect to the FT Server's IP address and port
                clientSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                clientSocket.Connect(new IPEndPoint(IPAddress.Parse(ftServerAddress), ftServerPort));

                // establish the network stream, reader and 
                stream = new NetworkStream(clientSocket);
                reader = new StreamReader(stream);
                writer = new StreamWriter(stream);

                // now connected
                connected = true;

                Console.WriteLine("FTClient.Connect() - client is connected!");
                
            }
        }

        public void Disconnect()
        {
            if (connected)
            {
                // send exit to FT server
                SendExit();

                // close writer, reader and stream
                writer.Close();
                reader.Close();
                stream.Close();

                // disconnect and close socket
                clientSocket.Disconnect(false);
                clientSocket.Close();

                // now disconnected
                connected = false;
                clientSocket = null;
                stream = null;
                reader = null;
                writer = null;

                Console.WriteLine("FTClient.Disconnect() - client is disconnected!");

            }
        }

        public void GetDirectory(string directoryName)
        {
            // send get to the server for the specified directory and receive files
            if (connected)
            {
                // send get command for the directory
                SendGet(directoryName);

                // receive and process files
                while (ReceiveFile(directoryName))
                {
                    Console.WriteLine("FTClient.GetDirectory() - recieved a file");
                }

            }
        }

        #region implementation

        private void SendGet(string directoryName)
        {
            // send get message for the directory
            string get = "get\n" + directoryName + "\n";
            writer.Write(get);
            writer.Flush();
            Console.WriteLine("FTClient.SendGet() - sent!");

        }

        private void SendExit()
        {
            // send exit message
            string exit = "exit\n";
            writer.Write(exit);
            writer.Flush();
            Console.WriteLine("FTClient.SendExit() - sent!");

        }

        private void SendInvalidMessage()
        {
            // TODO: FTClient.SendInvalidMessage()
            // allows for testing of server's error handling code

        }

        private bool ReceiveFile(string directoryName)
        {
            // receive a single file from the server and save it locally in the specified directory

            // expect file name from server
            string fileName = reader.ReadLine();

            // when the server sends "done", then there are no more files!
            if (fileName == "done")
            {
                Console.WriteLine("FTClient.RecieveFile() - recieved done");
                return false;
            }

            // handle error messages from the server
            if (fileName == "error")
            {
                Console.WriteLine("FTClient.RecieveFile() - recieved error");
                string errorMsg = reader.ReadLine();
                Console.WriteLine("Error! " + errorMsg);
                return false;
            }

            // received a file name

            // receive file length from server

            // receive file contents

            // loop until all of the file contenst are received
            //while (charsToRead > 0)
            {
                // receive as many characters from the server as available

                // accumulate bytes read into the contents

            }

            // create the local directory if needed
            
            // save the file locally on the disk
            
            return true;
        }

        #endregion
    }
}
