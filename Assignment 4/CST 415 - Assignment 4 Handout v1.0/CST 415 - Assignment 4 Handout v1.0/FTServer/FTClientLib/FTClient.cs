﻿// FTClient.cs
//
// Brennen Boese
// CST 415
// Fall 2020
// 

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Collections.Generic;

namespace FTClientLib
{
    public class FTClient
    {
        public class FileContent
        { 
            public string Name { get; private set; }
            public string Content { get; private set; }

            public FileContent(string name, string content)
            {
                Name = name;
                Content = content;
            }
        }

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

        public FileContent[] GetDirectory(string directoryName)
        {
            // send get to the server for the specified directory and receive files
            if (connected)
            {
                // send get command for the directory
                SendGet(directoryName);

                // receive and process files
                List<FileContent> files = new List<FileContent>();
                FileContent file;
                while ((file = ReceiveFile(directoryName)) != null)
                {
                    files.Add(file);
                    Console.WriteLine("FTClient.GetDirectory() - recieved a file");
                }

                return files.ToArray();
            }

            throw new Exception("Can't get directory contents when not connected!");
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

        private FileContent ReceiveFile(string directoryName)
        {
            // receive a single file from the server and return it

            // expect file name from server
            string fileName = reader.ReadLine();

            // when the server sends "done", then there are no more files!
            if (fileName == "done")
            {
                Console.WriteLine("FTClient.RecieveFile() - recieved done");
                return null;
            }

            // handle error messages from the server
            if (fileName == "error")
            {
                Console.WriteLine("FTClient.RecieveFile() - recieved error");
                string errorMsg = reader.ReadLine();
                Console.WriteLine("Error! " + errorMsg);
                throw new Exception(errorMsg);
            }

            // received a file name
            // receive file length from server
            int fileLength = int.Parse(reader.ReadLine());

            // receive file contents
            int charsToRead = fileLength;
            StringBuilder fileContents = new StringBuilder();

            // loop until all of the file contenst are received
            while (charsToRead > 0)
            {
                // receive as many characters from the server as available
                char[] buffer = new char[charsToRead];
                int charsActuallyRead = reader.Read(buffer, 0, charsToRead);

                // accumulate bytes read into the contents
                charsToRead -= charsActuallyRead;
                fileContents.Append(buffer);
            }

            return new FileContent(fileName, fileContents.ToString());
        }

        #endregion
    }
}
