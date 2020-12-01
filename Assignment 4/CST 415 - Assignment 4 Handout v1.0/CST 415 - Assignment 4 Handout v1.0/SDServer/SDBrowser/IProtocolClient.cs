﻿// IProtocolClient.cs
//
// Brennen Boese
// CST 415
// Fall 2020
// 

namespace SDBrowser
{
    // interface of a client for retrieving "documents" from a server
    // classes that implement this interface...
    // implement the protcol used to connect, retrieve and disconnect

    interface IProtocolClient
    {
        string GetDocument(string serverIP, string documentName);
        void Close();
    }
}
