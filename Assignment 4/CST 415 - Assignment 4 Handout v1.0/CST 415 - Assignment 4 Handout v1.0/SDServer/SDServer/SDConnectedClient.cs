﻿// SDConnectedClient.cs
//
// Brennen Boese
// CST 415
// Fall 2020
// 

using System;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.IO;

namespace SDServer
{
    class SDConnectedClient
    {
        // represents a single connected sd client
        // each client will have its own socket and thread while its connected
        // client is given it's socket from the SDServer when the server accepts the connection
        // this class creates it's own thread
        // the client's thread will process messages on the client's socket until it disconnects
        // NOTE: an sd client can connect/send messages/disconnect many times over it's lifetime

        private Socket clientSocket;
        private NetworkStream stream;
        private StreamReader reader;
        private StreamWriter writer;
        private Thread clientThread;
        private SessionTable sessionTable;      // server's session table
        private ulong sessionId;                // session id for this session, once opened or resumed

        public SDConnectedClient(Socket clientSocket, SessionTable sessionTable)
        {

            // save the client's socket
            this.clientSocket = clientSocket;

            // at this time, there is no stream, reader, write or thread
            stream = null;
            reader = null;
            writer = null;
            clientThread = null;

            // save the server's session table
            this.sessionTable = sessionTable;

            // at this time, there is no session open
            sessionId = 0;
            
        }

        public void Start()
        {
            // called by the main thread to start the clientThread and process messages for the client

            // create and start the clientThread, pass in a reference to this class instance as a parameter
            clientThread = new Thread(ThreadProc);
            clientThread.Start(this);
        }

        private static void ThreadProc(Object param)
        {

            // the procedure for the clientThread
            // when this method returns, the clientThread will exit

            // the param is a SDConnectedClient instance
            // start processing messages with the Run() method
            (param as SDConnectedClient).Run();
        }

        private void Run()
        {
            //this method is executed on the clientThread

            try
            {
                // create network stream, reader and writer over the socket
                stream = new NetworkStream(clientSocket);
                reader = new StreamReader(stream);
                writer = new StreamWriter(stream);
                
                // process client requests
                bool done = false;
                while (!done)
                {
                    // receive a message from the client
                    string msg = reader.ReadLine();
                    if (msg == null)
                    {
                        // no message means the client disconnected
                        // remember that the client will connect and disconnect as desired
                        Console.WriteLine("SDConnectedClient.Run() - client disconnected");
                        done = true;

                    }
                    else
                    {
                        // handle the message
                        Console.WriteLine("SDConnectedClient.Run() - read message: " + msg);
                        switch (msg)
                        {
                            case "open":
                                HandleOpen();
                                break;

                            case "resume":
                                HandleResume();
                                break;

                            case "close":
                                HandleClose();
                                break;

                            case "get":
                                HandleGet();
                                break;

                            case "post":
                                HandlePost();
                                break;

                            default:
                                {
                                    // error handling for an invalid message

                                    // this client is too broken to waste our time on!
                                    SendError("Unrecogized message: " + msg);
                                    done = true;
                                }
                                break;
                        }
                    }
                }
            }
            catch (SocketException se)
            {
                Console.WriteLine("[" + clientThread.ManagedThreadId.ToString() + "] " + "Error on client socket, closing connection: " + se.Message);
            }
            catch (IOException ioe)
            {
                Console.WriteLine("[" + clientThread.ManagedThreadId.ToString() + "] " + "IO Error on client socket, closing connection: " + ioe.Message);
            }

            // close the client's writer, reader, network stream and socket
            writer.Close();
            reader.Close();
            stream.Close();
            clientSocket.Disconnect(false);
            clientSocket.Close();
            
        }

        private void HandleOpen()
        {
            // handle an "open" request from the client

            // if no session currently open, then...
            if (sessionId == 0)
            {
                try
                {
                    // ask the SessionTable to open a new session and save the session ID
                    sessionId = sessionTable.OpenSession();
                    Console.WriteLine("SDConnctedClient.HandleOpen() - opened sessions on session table: "+ sessionId.ToString());

                    // send accepted message, with the new session's ID, to the client
                    SendAccepted(sessionId);

                }
                catch (SessionException se)
                {
                    SendError(se.Message);
                }
                catch (Exception ex)
                {
                    SendError(ex.Message);
                }
            }
            else
            {
                // error!  the client already has a session open!
                SendError("Session already open!");
            }
        }

        private void HandleResume()
        {
            // handle a "resume" request from the client

            // get the sessionId that the client just asked us to resume
            string line = reader.ReadLine();
            ulong resumeThis = ulong.Parse(line);

            try
            {
                // if we don't have a session open currently for this client...
                if (sessionId == 0)
                {
                    // try to resume the session in the session table
                    if (sessionTable.ResumeSession(resumeThis))
                    {
                        // if success, remember the session that we're now using and send accepted to client
                        sessionId = resumeThis;
                        SendAccepted(sessionId);
                        Console.WriteLine("SDConnectedClient.HandleResume() - resumed session: " + sessionId.ToString());
                    }
                    else
                    {
                        // if failed to resume session, send rejectetd to client
                        SendRejected("Unable to resume session");
                        Console.WriteLine("SDConnectedClient.HandleResume() - unable to resume session: " + resumeThis.ToString());
                    }


                }
                else
                {
                    // error! we already have a session open
                    SendError("Session already open, cannot resume!");
                    Console.WriteLine("SDConnectedClient.HandleResume() - session already open!");

                }
            }
            catch (SessionException se)
            {
                SendError(se.Message);
            }
            catch (Exception ex)
            {
                SendError(ex.Message);
            }
        }

        private void HandleClose()
        {
            // handle a "close" request from the client

            // get the sessionId that the client just asked us to close
            string line = reader.ReadLine();
            ulong closeThis = ulong.Parse(line);

            
            try
            {
                // the client currently has a session open,
                // make sure client has requested to close their own sessionId
                if (sessionId != 0 && closeThis != sessionId)
                {
                    throw new SessionException("Hey! You're closing the wrong session Id!");
                }

                // close the session in the session table
                sessionTable.CloseSession(closeThis);

                // send closed message back to client
                SendClosed(closeThis);

                // record that this client no longer has an open session
                sessionId = 0;

            }
            catch (SessionException se)
            {
                SendError(se.Message);
            }
            catch (Exception ex)
            {
                SendError(ex.Message);
            }
        }

        private void HandleGet()
        {
            // handle a "get" request from the client

            // get the document name from the client
            string documentName = reader.ReadLine();

            // if the client has a session open
            if (sessionId != 0)
            {
                try
                {
                   
                    string documentContent;
                    // check if request is for a file or a session variable
                    if (documentName.Length > 1 && documentName[0] == '/')
                    {
                        // get the file requested
                        string fileName = documentName.TrimStart('/').Replace('/','\\');
                        string filePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);
                        Console.WriteLine("SDConnectedClient.HandleGet() - get requested for file: " + filePath);
                        documentContent = File.ReadAllText(filePath);      // get from file
                    }
                    else if (documentName.Length > 0)
                    {
                        // get the document content from the session table
                        documentContent = sessionTable.GetSessionValue(sessionId, documentName);
                    }
                    else
                    {
                        // invalid document name!
                        throw new Exception("Invalid document name!");
                    }


                    // send success and document to the client
                    SendSuccess(documentName, documentContent);
                }
                catch (SessionException se)
                {
                    SendError(se.Message);
                }
                catch (Exception ex)
                {
                    SendError(ex.Message);
                }
            }
            else
            {
                // error, cannot get without a session
                SendError("Cannot get document without an open session!");
                
            }
        }

        private void HandlePost()
        {
            // handle a "post" request from the client

            // get the document name, content length and contents from the client
            string documentName = reader.ReadLine();
            int contentLength = int.Parse(reader.ReadLine());
            string content = ReceiveDocument(contentLength);

            // if the client has a session open
            if (sessionId != 0)
            {
                try
                {
                    // check if request is for a file or a session variable
                    if (documentName.Length > 1 && documentName[0] == '/')
                    {
                        // append to the file requested
                        string fileName = documentName.TrimStart('/').Replace('/', '\\');
                        string filePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);
                        Console.WriteLine("SDConnectedClient.HandlePost() - post requested for file: " + filePath);
                        File.AppendAllText(filePath, "\n" + content);      // get from file
                    }
                    else if (documentName.Length > 0)
                    {
                        // put the document into the session
                        sessionTable.PutSessionValue(sessionId, documentName, content);
                    }
                    else
                    {
                        // invalid document name!
                        throw new Exception("Invalid document name!");
                    }

                    // send success to the client
                    SendSuccess();
                }
                catch (SessionException se)
                {
                    SendError(se.Message);
                }
                catch (Exception ex)
                {
                    SendError(ex.Message);
                }
            }
            else
            {
                // error, cannot post without a session
                SendError("Unable to post document, no session open!");
            }
        }

        private void SendAccepted(ulong sessionId)
        {
            // send accepted message to SD client, including session id of now open session
            writer.Write("accepted\n" + sessionId.ToString() + "\n");
            writer.Flush();
            Console.WriteLine("SDConnctedClient.SendAccepted() - sent accepted to client: " + sessionId.ToString());


        }

        private void SendRejected(string reason)
        {
            // send rejected message to SD client, including reason for rejection
            writer.Write("rejected\n" + reason + "\n");
            writer.Flush();
            Console.WriteLine("SDConnctedClient.SendRejected() - sent rejection to client: " + reason);

        }

        private void SendClosed(ulong sessionId)
        {

            // send closed message to SD client, including session id that was just closed
            writer.Write("closed\n" + sessionId.ToString() + "\n");
            writer.Flush();
            Console.WriteLine("SDConnctedClient.SendClosed() - sent closed to client: " + sessionId.ToString());

        }

        private void SendSuccess()
        {
            // send sucess message to SD client, with no further info
            // NOTE: in response to a post request
            writer.Write("success\n");
            writer.Flush();
            Console.WriteLine("SDConnctedClient.SendSuccess() - sent success to client");
        }

        private void SendSuccess(string documentName, string documentContent)
        {
            // send success message to SD client, including retrieved document name, length and content
            // NOTE: in response to a get request
            writer.Write("success\n" + documentName + "\n" + documentContent.Length.ToString() + "\n");
            writer.Write(documentContent);
            writer.Flush();
            Console.WriteLine("SDConnctedClient.SendSuccess() - sent success to client for " + documentName);

        }

        private void SendError(string errorString)
        {
            // send error message to SD client, including error string
            writer.Write("error\n" + errorString + "\n");
            writer.Flush();
            Console.WriteLine("SDConnctedClient.SendError() - sent error to client: " + errorString);
        }

        private string ReceiveDocument(int length)
        {
            // receive a document from the SD client, of expected length
            // NOTE: as part of processing a post request

            // read from the reader until we've received the expected number of characters
            // accumulate the characters into a string and return those when we got enough
            StringBuilder builder = new StringBuilder();
            int charactersToRead = length;
            while (charactersToRead > 0)
            {
                char[] buffer = new char[charactersToRead];
                int charactersRead = reader.Read(buffer, 0, charactersToRead);
                charactersToRead -= charactersRead;
                builder.Append(buffer, 0, charactersRead);
            }
            Console.WriteLine("SDConnctedClient.ReceiveDocument() - received " + length.ToString() + " characters from client");

            return builder.ToString();
        }
    }
}
