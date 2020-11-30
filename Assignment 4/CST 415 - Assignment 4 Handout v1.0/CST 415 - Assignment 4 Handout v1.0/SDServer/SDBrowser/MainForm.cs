﻿// MainForm.cs
//
// Pete Myers
// CST 415
// Fall 2019
// 

using System;
using System.Windows.Forms;

namespace SDBrowser
{
    public partial class MainForm : Form
    {
        private ContentFetcher fetcher;

        public MainForm()
        {
            // TODO: MainForm.MainForm()
            // default command line values
            string prsIP = "127.0.0.1";
            ushort prsPort = 30000;

            // parse the command line and get the PRS Server's IP Address and Port number
            // -prs < PRS IP address>:< PRS port >
            // NOTE: args[0] is the name of the program, first true argument is at args[1]
            //string[] args = Environment.GetCommandLineArgs();


            // instantiate the fetcher and add the support SD and FT protocols
            fetcher = new ContentFetcher();
            // TODO: add FT protocol to the fetcher
            fetcher.AddProtocol("SD", new SDProtocolClient(prsIP, prsPort));

            InitializeComponent();
        }

        private void buttonGo_Click(object sender, EventArgs e)
        {
            // user clicked the Go! button

            try
            {
                // grab the address from the address bar
                string address = textboxAddress.Text;

                // fetch the content
                string content = fetcher.Fetch(address);

                // put the content in the content box
                textboxContent.Text = content;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            // close the fetcher so it can close it's sessions with the servers
            fetcher.Close();
        }
    }
}
