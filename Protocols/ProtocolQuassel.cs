﻿/***************************************************************************
 *   This program is free software; you can redistribute it and/or modify  *
 *   it under the terms of the GNU General Public License as published by  *
 *   the Free Software Foundation; either version 2 of the License, or     *
 *   (at your option) version 3.                                           *
 *                                                                         *
 *   This program is distributed in the hope that it will be useful,       *
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of        *
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the         *
 *   GNU General Public License for more details.                          *
 *                                                                         *
 *   You should have received a copy of the GNU General Public License     *
 *   along with this program; if not, write to the                         *
 *   Free Software Foundation, Inc.,                                       *
 *   51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.         *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;

namespace Client
{
    public class Quassel_Parser
    {
        private string Input = "";

        public void Proccess()
        {
            switch (Input)
            { 
                case "error":
                    break;
            }
        }

        public Quassel_Parser(ProtocolQuassel parent, string input)
        { 
            
        }
    }

    /// <summary>
    /// Quassel protocol
    /// </summary>
    public class ProtocolQuassel : Protocol
    {
        private System.Threading.Thread _Thread = null;
        private System.Net.Sockets.NetworkStream _network ;
        private System.Net.Security.SslStream _networks;
        private System.IO.StreamReader _reader;
        private System.IO.StreamWriter _writer;
        /// <summary>
        /// Password
        /// </summary>
        public string password = "";
        /// <summary>
        /// Name
        /// </summary>
        public string name = "";

        /// <summary>
        /// Exit
        /// </summary>
        public override void Exit()
        {
            base.Exit();
            return;
        }

        private void Start()
        {
            try
            {
                _network = new System.Net.Sockets.TcpClient(Server, Port).GetStream();
                _networks = new System.Net.Security.SslStream(new System.Net.Sockets.TcpClient(Server, Port).GetStream(), false);
                _writer = new System.IO.StreamWriter(_network);
                _reader = new System.IO.StreamReader(_network, Encoding.UTF8);

                Connected = true;
                while (!_reader.EndOfStream)
                {
                    string text = null;
                    while (Core.blocked)
                    {
                        System.Threading.Thread.Sleep(100);
                    }
                    
                    text = _reader.ReadLine();
                    Core.trafficscanner.insert(Server, " >> " + text);
                    Quassel_Parser parser = new Quassel_Parser(this, text);
                    parser.Proccess();
                }
            }
            catch (Exception h)
            {
                Core.handleException(h);
            }
        }

        private void Send(string ms)
        {
            try
            {
                _writer.WriteLine(ms);
                Core.trafficscanner.insert(Server, " << " + ms);
                _writer.Flush();
            }
            catch (Exception fail)
            {
                Core.handleException(fail);
            }
        }

        /// <summary>
        /// Connect to quassel core
        /// </summary>
        /// <returns></returns>
        public override bool Open()
        {
            _Thread = new System.Threading.Thread(Start);
            Core._Main.Status(messages.get("connecting", Core.SelectedLanguage));
            _Thread.Start();
            Core.SystemThreads.Add(_Thread);
            return true;
        }
    }
}
