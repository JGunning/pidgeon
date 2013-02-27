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
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Client
{
    public partial class ProtocolSv : Protocol
    {
        public System.Threading.Thread main = null;
        public System.Threading.Thread keep = null;
        public DateTime pong = DateTime.Now;

        private System.Net.Sockets.NetworkStream _networkStream;
        private System.IO.StreamReader _reader = null;
        /// <summary>
        /// List of networks loaded on server
        /// </summary>
        public List<Network> NetworkList = new List<Network>();
        private System.IO.StreamWriter _writer = null;
        public string password = "";
        public List<Cache> cache = new List<Cache>();
        public Status ConnectionStatus = Status.WaitingPW;



        public string nick = "";
        public bool auth = false;
        public class Cache
        {
            public int size = 0;
            public int current = 0;
        }

        public class Datagram
        {
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="Name">Name of a datagram</param>
            /// <param name="Text">Value</param>
            public Datagram(string Name, string Text = "")
            {
                _Datagram = Name;
                _InnerText = Text;
            }

            public string ToDocumentXmlText()
            {
                XmlDocument datagram = new XmlDocument();
                XmlNode b1 = datagram.CreateElement(_Datagram.ToUpper());
                foreach (KeyValuePair<string, string> curr in Parameters)
                {
                    XmlAttribute b2 = datagram.CreateAttribute(curr.Key);
                    b2.Value = curr.Value;
                    b1.Attributes.Append(b2);
                }
                b1.InnerText = this._InnerText;
                datagram.AppendChild(b1);
                return datagram.InnerXml;
            }

            public string _InnerText;
            public string _Datagram;
            public Dictionary<string, string> Parameters = new Dictionary<string, string>();
        }

        public enum Status
        {
            WaitingPW,
            Connected,
        }

        public void _Ping()
        {
            try
            {
                while (true)
                {
                    Deliver(new Datagram("PING"));
                    System.Threading.Thread.Sleep(480000);
                }
            }
            catch (Exception)
            {
                Core.DebugLog("ProtocolSv: Exception in Ping()");
            }
        }

        public override bool Command(string cm, Network network = null)
        {
            try
            {
                if (cm.StartsWith(" ") != true && cm.Contains(" "))
                {
                    // uppercase
                    string first_word = cm.Substring(0, cm.IndexOf(" ")).ToUpper();
                    string rest = cm.Substring(first_word.Length);
                    Transfer(first_word + rest, Configuration.Priority.Normal, network);
                    return true;
                }
                Transfer(cm.ToUpper(), Configuration.Priority.Normal, network);
                return true;
            }
            catch (Exception ex)
            {
                Core.handleException(ex);
            }
            return false;
        }

        public void Start()
        {
            Core._Main.Chat.scrollback.InsertText(messages.get("loading-server", Core.SelectedLanguage, new List<string> { this.Server }),
                Scrollback.MessageStyle.System);
            try
            {
                _networkStream = new System.Net.Sockets.TcpClient(Server, Port).GetStream();

                _writer = new System.IO.StreamWriter(_networkStream);
                _reader = new System.IO.StreamReader(_networkStream, Encoding.UTF8);

                Connected = true;

                Deliver(new Datagram("PING"));
                Deliver(new Datagram("LOAD"));


                Datagram login = new Datagram("AUTH", "");
                login.Parameters.Add("user", nick);
                login.Parameters.Add("pw", password);
                Deliver(login);
                Deliver(new Datagram("GLOBALNICK"));
                Deliver(new Datagram("NETWORKLIST"));
                Deliver(new Datagram("STATUS"));


                keep = new System.Threading.Thread(_Ping);
                Core.SystemThreads.Add(keep);
                keep.Name = "pinger thread";
                keep.Start();

            }
            catch (Exception b)
            {
                Core._Main.Chat.scrollback.InsertText(b.Message, Scrollback.MessageStyle.System);
                return;
            }
            string text = "";
            try
            {
                while (!_reader.EndOfStream)
                {
                    text = _reader.ReadLine();
                    Core.trafficscanner.insert(Server, " >> " + text);
                    while (Core.blocked)
                    {
                        System.Threading.Thread.Sleep(100);
                    }
                    if (Valid(text))
                    {
                        Process(text);
                        continue;
                    }
                }
            }
            catch (System.IO.IOException fail)
            {
                if (Connected)
                {
                    Core._Main.Chat.scrollback.InsertText("Quit: " + fail.Message, Scrollback.MessageStyle.System);
                }
            }
            catch (System.Threading.ThreadAbortException)
            {
                if (Core.IgnoreErrors)
                {
                    return;
                }
            }
            catch (Exception fail)
            {
                if (Connected)
                {
                    Core.handleException(fail);
                }
                Core.killThread(System.Threading.Thread.CurrentThread);
            }
        }

        private Network retrieveNetwork(string server)
        {
            foreach (Network i in NetworkList)
            {
                if (i.server == server)
                {
                    return i;
                }
            }
            return null;
        }

        public void SendData(string network, string data, Configuration.Priority priority = Configuration.Priority.Normal)
        {
            Datagram line = new Datagram("RAW", data);
            string Pr = "Normal";
            switch (priority)
            {
                case Configuration.Priority.High:
                    Pr = "High";
                    break;
                case Configuration.Priority.Low:
                    Pr = "Low";
                    break;
            }
            line.Parameters.Add("network", network);
            line.Parameters.Add("priority", Pr);
            Deliver(line);
        }

        public override void Part(string name, Network network = null)
        {
            Transfer("PART " + name, Configuration.Priority.High, network);
        }

        public bool Process(string dg)
        {
            try
            {
                System.Xml.XmlDocument datagram = new System.Xml.XmlDocument();
                datagram.LoadXml(dg);
                foreach (XmlNode curr in datagram.ChildNodes)
                {
                    switch (curr.Name.ToUpper())
                    {
                        case "SMESSAGE":
                            ResponsesSv.sMessage(curr, this);
                            break;
                        case "SLOAD":
                            ResponsesSv.sLoad(curr, this);
                            break;
                        case "SRAW":
                            ResponsesSv.sRaw(curr, this);
                            break;
                        case "SSTATUS":
                            ResponsesSv.sStatus(curr, this);
                            break;
                        case "SDATA":
                            ResponsesSv.sData(curr, this);
                            break;
                        case "SNICK":
                            ResponsesSv.sNick(curr, this);
                            break;
                        case "SREMOVE":
                            ResponsesSv.sRemove(curr, this);
                            break;
                        case "SCONNECT":
                            ResponsesSv.sConnect(curr, this);
                            break;
                        case "SGLOBALIDENT":
                            ResponsesSv.sGlobalident(curr, this);
                            break;
                        case "SBACKLOG":
                            ResponsesSv.sBacklog(curr, this);
                            break;
                        case "SGLOBALNICK":
                            ResponsesSv.sGlobalNick(curr, this);
                            break;
                        case "SNETWORKINFO":
                            ResponsesSv.sNetworkInfo(curr, this);
                            break;
                        case "SNETWORKLIST":
                            ResponsesSv.sNetworkList(curr, this);
                            break;
                        case "SCHANNELINFO":
                            ResponsesSv.sChannelInfo(curr, this);
                            break;
                        case "SAUTH":
                            ResponsesSv.sAuth(curr, this);
                            break;
                    }
                }
            }
            catch (System.Xml.XmlException xx)
            {
                Core.DebugLog("Unable to parse: " + xx.ToString());
            }
            catch (Exception fail)
            {
                Core.handleException(fail);
            }
            return true;
        }

        public override void Exit()
        {
            Connected = false;
            lock (NetworkList)
            {
                foreach (Network network in NetworkList)
                {
                    network.Connected = false;
                    if (Core.network == network)
                    {
                        Core.network = null;
                    }
                }
                NetworkList.Clear();
            }
            if (keep != null)
            {
                keep.Abort();
                Core.killThread(keep);
            }
            if (_writer != null) _writer.Close();
            if (_reader != null) _reader.Close();
            base.Exit();
        }

        public void Deliver(Datagram message)
        {
            Send(message.ToDocumentXmlText());
        }

        public override bool Open()
        {
            ProtocolType = 3;
            CreateChat("!root", true, null);
            main = new System.Threading.Thread(Start);
            Core._Main.ChannelList.insertSv(this);
            Core.SystemThreads.Add(main);
            main.Start();
            return true;
        }

        public override int requestNick(string _Nick, Network network = null)
        {
            Deliver(new Datagram("GLOBALNICK", _Nick));
            return 0;
        }

        public override int Message2(string text, string to, Configuration.Priority _priority = Configuration.Priority.Normal)
        {
            Core._Main.Chat.scrollback.InsertText(">>>>>>" + Core.network.Nickname + " " + text, Scrollback.MessageStyle.Action);
            Transfer("PRIVMSG " + to + " :" + delimiter.ToString() + "ACTION " + text + delimiter.ToString(), _priority);
            return 0;
        }

        public override int Message(string text, string to, Configuration.Priority _priority = Configuration.Priority.Normal, bool pmsg = false)
        {
            if (!pmsg)
            {
                //Core._Main.Chat.scrollback.InsertText(Core.network._protocol.PRIVMSG(Core.network.nickname, text),
                //    Scrollback.MessageStyle.Message, true, 0, true);
            }
            Datagram message = new Datagram("MESSAGE", text);
            if (Core.network != null && NetworkList.Contains(Core.network))
            {
                message.Parameters.Add("network", Core.network.server);
                message.Parameters.Add("priority", _priority.ToString());
                message.Parameters.Add("to", to);
                Deliver(message);
            }
            else
            {
                Core.DebugLog("Invalid network");
            }
            return 0;
        }

        public override bool ConnectTo(string server, int port)
        {
            while (server.StartsWith(" "))
            {
                server = server.Substring(1);
            }
            if (server == "")
            {
                return false;
            }
            if (ConnectionStatus == Status.Connected)
            {
                windows["!root"].scrollback.InsertText("Connecting to " + server, Scrollback.MessageStyle.User, true);
                Datagram request = new Datagram("CONNECT", server);
                request.Parameters.Add("port", port.ToString());
                Deliver(request);
            }
            return true;
        }

        /// <summary>
        /// Check if it's a valid data
        /// </summary>
        /// <param name="datagram"></param>
        /// <returns></returns>
        public bool Valid(string datagram)
        {
            if (datagram.StartsWith("<") && datagram.EndsWith(">"))
            {
                return true;
            }
            return false;
        }

        public void Disconnect(Network network)
        {
            Transfer("QUIT :" + network.Quit);
            Datagram request = new Datagram("REMOVE", network.server);
            Deliver(request);
        }

        /// <summary>
        /// Write raw data
        /// </summary>
        /// <param name="text"></param>
        public void Send(string text)
        {
            if (Connected)
            {
                try
                {
                    _writer.WriteLine(text);
                    Core.trafficscanner.insert(Server, " << " + text);
                    _writer.Flush();
                }
                catch (System.IO.IOException er)
                {
                    windows["!root"].scrollback.InsertText(er.Message, Scrollback.MessageStyle.User);
                    Exit();
                }
                catch (Exception f)
                {
                    if (Connected)
                    {
                        Core.handleException(f);
                    }
                }

            }
        }

        public override void Join(string name, Network network = null)
        {
            Transfer("JOIN " + name);
        }

        public override void Transfer(string text, Configuration.Priority Pr = Configuration.Priority.Normal, Network network = null)
        {
            if (network == null)
            {
                if (Core.network != null && NetworkList.Contains(Core.network))
                {
                    Datagram data = new Datagram("RAW", text);
                    data.Parameters.Add("network", Core.network.server);
                    Deliver(data);
                    return;
                }
            }
            else
            {
                if (NetworkList.Contains(network))
                {
                    Datagram data = new Datagram("RAW", text);
                    data.Parameters.Add("network", network.server);
                    Deliver(data);
                }
                else
                {
                    Core.DebugLog("Network is not a part of this services connection " + network.server);
                }
            }
        }
    }
}