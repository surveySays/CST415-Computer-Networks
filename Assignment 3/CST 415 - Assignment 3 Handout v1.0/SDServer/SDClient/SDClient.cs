﻿// SDClient.cs
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

namespace SDClient
{
    class SDClient
    {
        private string sdServerAddress;
        private ushort sdServerPort;
        private bool connected;
        private ulong sessionID;
        Socket clientSocket;
        NetworkStream stream;
        StreamReader reader;
        StreamWriter writer;

        public SDClient(string sdServerAddress, ushort sdServerPort)
        {
            // save server address/port
            this.sdServerAddress = sdServerAddress;
            this.sdServerPort = sdServerPort;

            // initialize to not connected to server
            connected = false;
            clientSocket = null;
            stream = null;
            reader = null;
            writer = null;

            // no session open at this time
            sessionID = 0;
        }

        public ulong SessionID { get { return sessionID; } set { sessionID = value; } }

        public void Connect()
        {
            ValidateDisconnected();

            // create a client socket and connect to the FT Server's IP address and port
            clientSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            clientSocket.Connect(new IPEndPoint(IPAddress.Parse(sdServerAddress), sdServerPort));

            // establish the network stream, reader and writer
            stream = new NetworkStream(clientSocket);
            reader = new StreamReader(stream);
            writer = new StreamWriter(stream);

            // now connected
            connected = true;
            
        }

        public void Disconnect()
        {
            ValidateConnected();

            // close writer, reader and stream
            writer.Close();
            reader.Close();
            stream.Close();

            // disconnect and close socket
            clientSocket.Disconnect(false);
            clientSocket.Close();

            // now disconnected
            connected = false;
            
        }

        public void OpenSession()
        {
            ValidateConnected();

            // send open command to server
            SendOpen();

            // receive server's response, hopefully with a new session id
            sessionID = ReceiveSessionResponse();
            
        }

        public void ResumeSession(ulong trySessionID)
        {
            // TODO: SDClient.ResumeSession()

            ValidateConnected();

            // send resume session to the server
            
            // receive server's response, hopefully confirming our sessionId
            
            // verify that we received the same session ID that we requested
            
            // save opened session
            
        }

        public void CloseSession()
        {
            ValidateConnected();

            // send close session to the server
            SendClose(sessionID);

            //verify closed response
            ulong closedId = ReceiveSessionResponse();

            if (closedId != sessionID)
            {
                throw new Exception("Server closed id " + closedId.ToString() + " but we asked to close " + sessionID.ToString());
            }

            // no session open
            sessionID = 0;
            
        }

        public string GetDocument(string documentName)
        {
            // TODO: SDClient.GetDocument()

            ValidateConnected();

            // send get to the server
            
            // get the server's response
            return "TODO";
        }

        public void PostDocument(string documentName, string documentContents)
        {
            // TODO: SDClient.PostDocument()

            ValidateConnected();

            // send the document to the server
            
            // get the server's response
            
        }

        private void ValidateConnected()
        {
            if (!connected)
                throw new Exception("Connot perform action. Not connected to server!");
        }

        private void ValidateDisconnected()
        {
            if (connected)
                throw new Exception("Connot perform action. Already connected to server!");
        }

        private void SendOpen()
        {
            // send open message to SD server
            writer.Write("open\n");
            writer.Flush();
            Console.WriteLine("Sent open to server");
            
        }

        private void SendClose(ulong sessionId)
        {
            // send close message to SD server
            writer.Write("close\n" + sessionID.ToString() + "\n");
            writer.Flush();
            Console.WriteLine("Sent close to server: " + sessionID.ToString());

        }

        private void SendResume(ulong sessionId)
        {
            // TODO: SDClient.SendResume()

            // send resume message to SD server
            
        }

        private ulong ReceiveSessionResponse()
        {
            // get SD server's response to our last session request (open or resume)
            string line = reader.ReadLine();
            if (line == "accepted")
            {
                // yay, server accepted our session!
                // get the sessionID
                line = reader.ReadLine();
                Console.WriteLine("Received accepted: " + line);
                return ulong.Parse(line);
              
            }
            else if (line == "rejected")
            {
                // boo, server rejected us!
                line = reader.ReadLine();
                Console.WriteLine("Received rejected: " + line);
                throw new Exception(line);
            }
            else if (line == "closed")
            {
                // yay, server closed the session
                line = reader.ReadLine();
                Console.WriteLine("Received closed: " + line);
                return ulong.Parse(line);
            }
            else if (line == "error")
            {
                // boo, server sent us an error!
                line = reader.ReadLine();
                Console.WriteLine("Received error: " + line);
                throw new Exception(line);
            }
            else
            {
                throw new Exception("Expected to receive a valid session response, instead got... " + line);
            }
        }

        private void SendPost(string documentName, string documentContents)
        {
            // TODO: SDClient.SendPost()

            // send post message to SD erer, including document name, length and contents

        }

        private void SendGet(string documentName)
        {
            // TODO: SDClient.SendGet()

            // send get message to SD server

        }

        private void ReceivePostResponse()
        {
            // TODO: SDClient.ReceivePostResponse()

            // get server's response to our last post request
            string line = reader.ReadLine();
            if (line == "success")
            {
                // yay, server accepted our request!
                
            }
            else if (line == "error")
            {
                // boo, server sent us an error!
                throw new Exception("TODO");
            }
            else
            {
                throw new Exception("Expected to receive a valid post response, instead got... " + line);
            }
        }

        private string ReceiveGetResponse()
        {
            // TODO: SDClient.ReceiveGetResponse()

            // get server's response to our last get request and return the content received
            string line = reader.ReadLine();
            if (line == "success")
            {
                // yay, server accepted our request!
                
                // read the document name, content length and content
                
                // return the content
                return "TODO";
            }
            else if (line == "error")
            {
                // boo, server sent us an error!
                throw new Exception("TODO");
            }
            else
            {
                throw new Exception("Expected to receive a valid get response, instead got... " + line);
            }
        }

        private string ReceiveDocumentContent(int length)
        {
            // TODO: SDClient.ReceiveDocumentContent()

            // read from the reader until we've received the expected number of characters
            // accumulate the characters into a string and return those when we received enough

            return "TODO";
        }
    }
}
