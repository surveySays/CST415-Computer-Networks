// PRSTestClientProgram.cs
//
// Pete Myers
// CST 415
// Fall 2019
//
// Connects to a PRSServer and runs TC1 through TC6
// Assumes the PRSServer is run with the following command line arguments:
//     PRSServer.exe -p 30000 -s 40000 -e 40100 -t 10
//

using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using PRSLib;

using System.Text.RegularExpressions;

namespace PRSTestClient
{
    class PRSTestClientProgram
    {
        static void Usage()
        {
            Console.WriteLine("usage: PRSTestClient [options]");
            Console.WriteLine("\t-prs <serverIP>:<serverPort>");
        }
      

        static void Main(string[] args)
        {
           
            // defaults
            string SERVER_IP = "127.0.0.1";
            int SERVER_PORT = 30000;
    

            //process command options
            //try
            //{
            //    for (int i = 0; i < args.Length; ++i)
            //    {
            //        if (args[i] == "-prs")
            //        {
            //            SERVER_IP = (args[i + 1]);
            //            SERVER_PORT = Int32.Parse(args[i + 1]);
            //        }
            //    }

            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.Message);
            //}
            
       
            // tell user what we're doing
            Console.WriteLine("Test Client started...");
            Console.WriteLine("  ServerIP = " + SERVER_IP.ToString());
            Console.WriteLine("  ServerPort = " + SERVER_PORT.ToString());

            // recommend proper PRSServer command line arguments
            Console.WriteLine();
            Console.WriteLine("Assumes the PRSServer is running with the following command line arguments:");

            // create the socket for sending messages to the server
            Socket clientSocket = new Socket(SocketType.Dgram, ProtocolType.Udp);

            // construct the server's address and port
            IPEndPoint serverEP = new IPEndPoint(IPAddress.Parse(SERVER_IP), SERVER_PORT);

          
            try
            {
                // call each test case method
                TestCase1(clientSocket, serverEP); //PASSED
                TestCase2(clientSocket, serverEP); //PASSED
                TestCase3(clientSocket, serverEP); //PASSED

            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
                Console.WriteLine(ex.StackTrace);
            }

            // close the client socket and quit
            clientSocket.Close();
            
            // wait for a keypress from the user before closing the console window
            Console.WriteLine("Press Enter to exit");
            Console.ReadKey();
        }

        private static void SendMessage(Socket clientSocket, IPEndPoint endPt, PRSMessage msg)
        {
            msg.SendMessage(clientSocket, endPt);
        }

        private static PRSMessage ExpectMessage(Socket clientSocket, string expectedMessage)
        {
            // receive message and validate that expected PRSMessage was received
            EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
            PRSMessage msg = PRSMessage.ReceiveMessage(clientSocket, ref remoteEP);
            if (msg.ToString() != expectedMessage)
                throw new Exception("Test failed! Expected " + expectedMessage);

            return msg;
        }

        private static void TestCase1(Socket clientSocket, IPEndPoint endPt)
        {
            Console.WriteLine("TestCase 1 Started...");

            // {REQUEST_PORT, “SVC1”, 0, 0} send message
            PRSMessage msg = new PRSMessage(PRSMessage.MESSAGE_TYPE.REQUEST_PORT, "SVC1", 0, 0);
            SendMessage(clientSocket, endPt, msg);

            // {RESPONSE, “SVC1”, 40000, SUCCESS} recieve message
            ExpectMessage(clientSocket, "{RESPONSE, SVC1, 40000, SUCCESS}");

            // {KEEP_ALIVE, “SVC1”, 40000, 0} send message
            msg = new PRSMessage(PRSMessage.MESSAGE_TYPE.KEEP_ALIVE, "SVC1", 40000, 0);
            SendMessage(clientSocket, endPt, msg);

            // {RESPONSE, “SVC1”, 40000, SUCCESS} recieve message
            ExpectMessage(clientSocket, "{RESPONSE, SVC1, 40000, SUCCESS}");

            // {CLOSE_PORT, “SVC1”, 40000, 0} send message
            msg = new PRSMessage(PRSMessage.MESSAGE_TYPE.CLOSE_PORT, "SVC1", 40000, 0);
            SendMessage(clientSocket, endPt, msg);

            // {RESPONSE, “SVC1”, 40000, SUCCESS} recieve message
            ExpectMessage(clientSocket, "{RESPONSE, SVC1, 40000, SUCCESS}");

            Console.WriteLine("TestCase 1 Passed!");
            Console.WriteLine();
        }

        private static void TestCase2(Socket clientSocket, IPEndPoint endPt)
        {
           
            // Simulates two PRS clients, SVC1 and C1, where SVC1 requests a port, and C1 looks up the port.

            Console.WriteLine("TestCase 2 Started...");

            // {REQUEST_PORT, “SVC1”, 0, 0} send message
            PRSMessage msg = new PRSMessage(PRSMessage.MESSAGE_TYPE.REQUEST_PORT, "SVC1", 0, 0);
            SendMessage(clientSocket, endPt, msg);

            // {RESPONSE, “SVC1”, 40000, SUCCESS} recieve message
            ExpectMessage(clientSocket, "{RESPONSE, SVC1, 40000, SUCCESS}");

            // {LOOKUP_PORT, “SVC1”, 0, 0} send message
            msg = new PRSMessage(PRSMessage.MESSAGE_TYPE.LOOKUP_PORT, "SVC1", 0, 0);
            SendMessage(clientSocket, endPt, msg);

            // {RESPONSE, “SVC1”, 40000, SUCCESS} recieve message
            ExpectMessage(clientSocket, "{RESPONSE, SVC1, 40000, SUCCESS}");

            // {CLOSE_PORT, “SVC1”, 40000, 0} send message
            msg = new PRSMessage(PRSMessage.MESSAGE_TYPE.CLOSE_PORT, "SVC1", 40000, 0);
            SendMessage(clientSocket, endPt, msg);

            // {RESPONSE, “SVC1”, 40000, SUCCESS} recieve message
            ExpectMessage(clientSocket, "{RESPONSE, SVC1, 40000, SUCCESS}");

            Console.WriteLine("TestCase 2 Passed!");
            Console.WriteLine();
        }

        private static void TestCase3(Socket clientSocket, IPEndPoint endPt)
        {
           
            // Simulates two PRS clients, SVC1 and SVC2, where SVC1 requests a port, then SVC2 requests a port and receives its own port.

            Console.WriteLine("TestCase 3 Started...");

            // {REQUEST_PORT, “SVC1”, 0, 0} send message
            PRSMessage msg = new PRSMessage(PRSMessage.MESSAGE_TYPE.REQUEST_PORT, "SVC1", 0, 0);
            SendMessage(clientSocket, endPt, msg);

            // {RESPONSE, “SVC1”, 40000, SUCCESS} recieve message
            ExpectMessage(clientSocket, "{RESPONSE, SVC1, 40000, SUCCESS}");
     
            // {REQUEST_PORT, “SVC1”, 0, 0} send message
            msg = new PRSMessage(PRSMessage.MESSAGE_TYPE.REQUEST_PORT, "SVC2", 0, 0);
            SendMessage(clientSocket, endPt, msg);

            // {RESPONSE, “SVC2”, 40001, SUCCESS} recieve message
            ExpectMessage(clientSocket, "{RESPONSE, SVC2, 40001, SUCCESS}");

            // {REQUEST_PORT, “SVC1”, 0, 0} send message
            msg = new PRSMessage(PRSMessage.MESSAGE_TYPE.CLOSE_PORT, "SVC1", 40000, 0);
            SendMessage(clientSocket, endPt, msg);

            // {RESPONSE, “SVC1”, 40000, SUCCESS} recieve message
            ExpectMessage(clientSocket, "{RESPONSE, SVC1, 40000, SUCCESS}");

            // {REQUEST_PORT, “SVC2”, 40001, 0} send message
            msg = new PRSMessage(PRSMessage.MESSAGE_TYPE.CLOSE_PORT, "SVC2", 40001, 0);
            SendMessage(clientSocket, endPt, msg);

            // {RESPONSE, “SVC2”, 40001, SUCCESS} recieve message
            ExpectMessage(clientSocket, "{RESPONSE, SVC2, 40001, SUCCESS}");


            Console.WriteLine("TestCase 3 Passed!");
            Console.WriteLine();
        }

        private static void TestCase4(Socket clientSocket, IPEndPoint endPt)
        {
            // Simulates two PRS clients, SVC1 and SVC2, where SVC1 requests a port, SVC1 fails to keep the port alive, then SVC2 requests a port and receives SVC1’s expired port.

            Console.WriteLine("TestCase 4 Started...");

            // {REQUEST_PORT, “SVC1”, 0, 0} send message
            PRSMessage msg = new PRSMessage(PRSMessage.MESSAGE_TYPE.REQUEST_PORT, "SVC1", 0, 0);
            SendMessage(clientSocket, endPt, msg);

            // {RESPONSE, “SVC1”, 40000, SUCCESS} recieve message
            ExpectMessage(clientSocket, "{RESPONSE, SVC1, 40000, SUCCESS}");

            //sleep for 15 sec'
            Console.WriteLine("Sleeping for 15 seconds...");
            Thread.Sleep(15000);

            // {REQUEST_PORT, “SVC1”, 0, 0} send message
            msg = new PRSMessage(PRSMessage.MESSAGE_TYPE.REQUEST_PORT, "SVC2", 0, 0);
            SendMessage(clientSocket, endPt, msg);

            // {RESPONSE, “SVC2”, 40000, SUCCESS} recieve message
            ExpectMessage(clientSocket, "{RESPONSE, SVC2, 40000, SUCCESS}");

            // {REQUEST_PORT, “SVC2”, 40000, 0} send message
            msg = new PRSMessage(PRSMessage.MESSAGE_TYPE.CLOSE_PORT, "SVC2", 40000, 0);
            SendMessage(clientSocket, endPt, msg);

            // {RESPONSE, “SVC2”, 40000, SUCCESS} recieve message
            ExpectMessage(clientSocket, "{RESPONSE, SVC2, 40000, SUCCESS}");

            Console.WriteLine("TestCase 4 Passed!");
            Console.WriteLine();
        }

        private static void TestCase5(Socket clientSocket, IPEndPoint endPt)
        {
            // TODO: PRSTestClientProgram.TestCase5()

            // Simulates two PRS clients, SVC1 and SVC2, where SVC1 requests a port, SVC1 keeps the port alive, then SVC2 requests a port and receives its own port.

            Console.WriteLine("TestCase 5 Started...");

            // See test cases doc
            // use Thread.Sleep();

            Console.WriteLine("TestCase 5 Passed!");
            Console.WriteLine();
        }

        private static void TestCase6(Socket clientSocket, IPEndPoint endPt)
        {
            // TODO: PRSTestClientProgram.TestCase6()

            // Simulates a PRS client, M, that tells the PRS to stop

            Console.WriteLine("TestCase 6 Started...");

            // See test cases doc

            Console.WriteLine("TestCase 6 Passed!");
            Console.WriteLine();
        }
    }

}
