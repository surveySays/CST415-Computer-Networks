// FTProtocolClient.cs
//
// Brennen Boese
// CST 415
// Fall 2020
// 

using System;
using System.Text;
using PRSLib;
using System.Net;
using System.Net.Sockets;
using System.IO;
using FTClientLib;

namespace SDBrowser
{
    // implements IProtocolClient
    // uses the FT protcol
    // retrieves an entire directory and represents it as a single text "document"

    class FTProtocolClient : IProtocolClient
    {
        private string prsIP;
        private ushort prsPort;

        public FTProtocolClient(string prsIP, ushort prsPort)
        {
            // save the PRS server's IP address and port
            // will be used later to lookup the port for the FT Server when needed
            this.prsIP = prsIP;
            this.prsPort = prsPort;
        }

        public string GetDocument(string serverIP, string documentName)
        {
            // make sure we have valid parameters
            // serverIP is the FT Server's IP address
            // documentName is the name of a directory on the FT Server
            // both should not be empty
            if (String.IsNullOrWhiteSpace(serverIP) || String.IsNullOrWhiteSpace(documentName))
            {
                throw new Exception("Empty server IP or document Name!");
            }

            // contact the PRS and lookup port for "FT Server"
            PRSClient prs = new PRSClient(prsIP, prsPort, "FT Server");
            ushort ftPort = prs.LookupPort();

            // connect to FT server by ipAddr and port
            FTClient ft = new FTClient(serverIP, ftPort);
            ft.Connect();

            // get the files from the requested directory
            FTClient.FileContent[] files = ft.GetDirectory(documentName);

            // accumulate file contents in a result string
            StringBuilder builder = new StringBuilder();
            foreach (FTClient.FileContent file in files)
            {
                // name, content and empty line for each file
                builder.AppendLine(file.Name);
                builder.AppendLine(file.Content);
                builder.AppendLine();
            }

            // disconnect from server
            ft.Disconnect();

            // return the content
            return builder.ToString();
        }

        public void Close()
        {
            // nothing to do here!
            // the FT Protocol does not expect a client to close a session
            // everything is handled in the GetDocument() method
        }
    }
}
