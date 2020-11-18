﻿// SDServer.cs
//
// Pete Myers
// CST 415
// Fall 2019
// 

using System;
using System.Threading;
using System.Net.Sockets;
using System.Net;

namespace SDServer
{
    class SDServer
    {
        // represents the server and it's logic
        // the server uses the main program thread to listen and accept connections from client
        // when the server accepts a client connection, it will create the client's socket and thread

        private ushort listeningPort;
        private int clientBacklog;
        private SessionTable sessionTable;

        public SDServer(ushort listeningPort, int clientBacklog)
        {
            this.listeningPort = listeningPort;
            this.clientBacklog = clientBacklog;

            // initially empty session table
            sessionTable = new SessionTable();
        }

        public void Start()
        {
            // TODO: SDServer.Start()

            // create a listening socket for clients to connect
            // bind to the SD Server port
            // set the socket to listen
            
            bool done = false;
            while (!done)
            {
                try
                {
                    // accept a client connection
                    
                    // instantiate connected client to process messages
                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error while accepting and starting client: " + ex.Message);
                    Console.WriteLine("Waiting for 5 seconds and trying again...");
                    Thread.Sleep(5000);
                }
            }

            // close socket and quit
            
        }
    }
}
