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
using System.IO;
using System.Xml;
using System.Text;
using System.Xml.Serialization;

namespace Client.Services
{
    public class Buffer
    {
        [Serializable]
        public class Window
        {
            public bool isChannel = false;
            public bool isPM = false;
            public List<Scrollback.ContentLine> lines = null;
            public string Name = null;
            public string topic = null;
            public bool Changed = true;

            public Window()
            { 
                
            }

            public Window(Client.Window owner)
            {
                Name = owner.name;
                isChannel = owner.isChannel;
                lines = new List<Scrollback.ContentLine>();
                lines.AddRange(owner.scrollback.Data);
            }
        }

        public class ChannelInfo
        {
            /// <summary>
            /// Name of a channel including the special prefix
            /// </summary>
            public string Name;
            /// <summary>
            /// Topic
            /// </summary>
            public string Topic = null;
            /// <summary>
            /// Whether channel is in proccess of dispose
            /// </summary>
            public bool dispose = false;
            /// <summary>
            /// User who set a topic
            /// </summary>
            public string TopicUser = "<Unknown user>";
            /// <summary>
            /// Date when a topic was set
            /// </summary>
            public int TopicDate = 0;
            /// <summary>
            /// Invites
            /// </summary>
            public List<Invite> Invites = null;
            /// <summary>
            /// List of bans set
            /// </summary>
            public List<SimpleBan> Bans = null;
            /// <summary>
            /// Exception list 
            /// </summary>
            public List<Except> Exceptions = null;
            /// <summary>
            /// If channel output is temporarily hidden
            /// </summary>
            public bool temporary_hide = false;
            /// <summary>
            /// If true the channel is processing the /who data
            /// </summary>
            public bool parsing_who = false;
            /// <summary>
            /// If true the channel is processing ban data
            /// </summary>
            public bool parsing_bans = false;
            /// <summary>
            /// If true the channel is processing exception data
            /// </summary>
            public bool parsing_xe = false;
            /// <summary>
            /// If true the channel is processing whois data
            /// </summary>
            public bool parsing_wh = false;
            /// <summary>
            /// Whether window needs to be redraw
            /// </summary>
            public bool Redraw = false;
            /// <summary>
            /// If true the window is considered usable
            /// </summary>
            public bool ChannelWork = false;
        }

        [Serializable]
        public class NetworkInfo
        {
            public string mode = "+i";
            public string Nick = null;
            public string NetworkID = null;
            public string Server = null;
            public int lastMQID = 0;
            /// <summary>
            /// User modes
            /// </summary>
            public List<char> UModes = new List<char> { 'i', 'w', 'o', 'Q', 'r', 'A' };
            /// <summary>
            /// Channel user symbols (oper and such)
            /// </summary>
            public List<char> UChars = new List<char> { '~', '&', '@', '%', '+' };
            /// <summary>
            /// Channel user modes
            /// </summary>
            public List<char> CUModes = new List<char> { 'q', 'a', 'o', 'h', 'v' };
            /// <summary>
            /// Channel modes
            /// </summary>
            public List<char> CModes = new List<char> { 'n', 'r', 't', 'm' };
            /// <summary>
            /// Special channel modes with parameter as a string
            /// </summary>
            public List<char> SModes = new List<char> { 'k', 'L' };
            /// <summary>
            /// Special channel modes with parameter as a number
            /// </summary>
            public List<char> XModes = new List<char> { 'l' };
            /// <summary>
            /// Special channel user modes with parameters as a string
            /// </summary>
            public List<char> PModes = new List<char> { 'b', 'I', 'e' };
            /// <summary>
            /// Descriptions for channel and user modes
            /// </summary>
            //public Dictionary<char, string> Descriptions = new Dictionary<char, string>();

            public List<Buffer.Window> _windows = new List<Window>();
            public List<Buffer.ChannelInfo> _channels = new List<ChannelInfo>();

            public NetworkInfo()
            { 
                
            }

            public Buffer.Window getW(string window)
            {
                lock (_windows)
                {
                    foreach (Buffer.Window xx in _windows)
                    {
                        if (xx.Name == window)
                        {
                            return xx;
                        }
                    }
                }
                return null;
            }

            public void recoverWindowText(Client.Window target, string source)
            {
                Buffer.Window Source = getW(source);
                if (Source == null)
                {
                    Core.DebugLog("Failed to recover " + source);
                    return;
                }
                target.scrollback.SetText(Source.lines);
                target.isChannel = Source.isChannel;
            }

            public ChannelInfo getChannel(string name)
            {
                lock (_channels)
                {
                    foreach (ChannelInfo ch in _channels)
                    {
                        if (ch.Name == name)
                        {
                            return ch;
                        }
                    }
                }
                return null;
            }

            public NetworkInfo(string nick)
            {
                Nick = nick;
            }

            public void MQID(string text)
            {
                int mqid = int.Parse(text);
                if (lastMQID < mqid)
                {
                    lastMQID = mqid;
                }
            }
        }

        public Dictionary<string, string> Networks = new Dictionary<string, string>();
        public Dictionary<string, NetworkInfo> networkInfo = new Dictionary<string,NetworkInfo>();
        public ProtocolSv protocol = null;
        public bool Loaded = false;
        public string Root = null;
        private List<string> Data = null;
        public bool Modified = true;

        public Buffer(ProtocolSv _s)
        {
            Root = Core.PermanentTemp + "buffer_" + _s.Server + Path.DirectorySeparatorChar;
            protocol = _s;
        }

        public string getUID(string server)
        {
            if (Networks.ContainsKey(server))
            {
                return Networks[server];
            }
            return null;
        }

        public void Make(string network, string network_id)
        {
            if (!Networks.ContainsKey(network))
            {
                Networks.Add(network, network_id);
                networkInfo.Add(network_id, new NetworkInfo());
            }
        }

        public void ReadDisk()
        {
            try
            {
                lock (networkInfo)
                {
                    networkInfo.Clear();
                    if (!Directory.Exists(Root))
                    {
                        Core.DebugLog("There is no folder for buffer of " + protocol.Server);
                        Loaded = false;
                        return;
                    }
                    if (!File.Exists(Root + "data"))
                    {
                        Core.DebugLog("There is no main for buffer of " + protocol.Server);
                        Loaded = false;
                        return;
                    }
                    Data = new List<string>();
                    lock (Data)
                    {
                        Data.AddRange(File.ReadAllLines(Root + "data"));
                    }
                    foreach (string network in Data)
                    {
                        if (network != "")
                        {
                            string content = File.ReadAllText(Root + network);
                            NetworkInfo info = DeserializeNetwork(content);
                            networkInfo.Add(network, info);
                            Networks.Add(info.Server, network);
                        }
                    }
                }
                Modified = false;
                Loaded = true;
            }
            catch (Exception fail)
            {
                Core.DebugLog("Failed to load buffer, invalidating it " + fail.ToString());
                Loaded = false;
                Modified = true;
                networkInfo.Clear();
            }
        }

        public void Clear()
        {
            lock (networkInfo)
            {
                networkInfo.Clear();
            }
            Modified = true;
            Directory.Delete(Root, true);
        }

        public static string SerializeNetwork(NetworkInfo line)
        {
            XmlSerializer xs = new XmlSerializer(typeof(NetworkInfo));
            StringWriter data = new StringWriter();
            xs.Serialize(data, line);
            return data.ToString();
        }

        public static NetworkInfo DeserializeNetwork(string text)
        {
            XmlDocument document = new XmlDocument();
            document.LoadXml(text);
            XmlNodeReader reader = new XmlNodeReader(document.DocumentElement);
            XmlSerializer xs = new XmlSerializer(typeof(NetworkInfo));
            NetworkInfo info = (NetworkInfo)xs.Deserialize(reader);
            return info;
        }

        public void Snapshot()
        {
            lock (protocol.NetworkList)
            {
                foreach (Network network in protocol.NetworkList)
                {
                    string uid = protocol.sBuffer.getUID(network.server);
                    if (networkInfo.ContainsKey(uid))
                    {
                        networkInfo[uid]._windows.Clear();
                        networkInfo[uid]._channels.Clear();
                        networkInfo[uid]._windows.Add(new Buffer.Window(network.system));
                        networkInfo[uid].CModes = network.CModes;
                        networkInfo[uid].CUModes = network.CUModes;
                        networkInfo[uid].PModes = network.PModes;
                        networkInfo[uid].SModes = network.SModes;
                        //networkInfo[uid].Descriptions = network.Descriptions;
                        networkInfo[uid].XModes = network.XModes;
                        networkInfo[uid].UChars = network.UChars;
                        lock (network.Channels)
                        {
                            foreach (Channel xx in network.Channels)
                            {
                                Client.Window window = xx.retrieveWindow();
                                if (window != null)
                                {
                                    networkInfo[uid]._windows.Add(new Buffer.Window(window));
                                }
                                ChannelInfo info = new ChannelInfo();
                                info.Bans = xx.Bans;
                                info.dispose = xx.dispose;
                                info.Exceptions = xx.Exceptions;
                                info.Invites = xx.Invites;
                                info.Name = xx.Name;
                                info.parsing_bans = xx.parsing_bans;
                                info.parsing_wh = xx.parsing_wh;
                                info.parsing_who = xx.parsing_who;
                                info.parsing_xe = xx.parsing_xe;
                                info.Redraw = xx.Redraw;
                                info.temporary_hide = xx.temporary_hide;
                                info.Topic = xx.Topic;
                                info.TopicDate = xx.TopicDate;
                                info.TopicUser = xx.TopicUser;
                                networkInfo[uid]._channels.Add(info);
                            }

                            //foreach (User user in network.PrivateChat)
                            //{ 
                            //    network
                            //}
                        }
                    }
                }
            }
            WriteDisk();
        }

        public static void ListFile(List<string> list, string file)
        {
            StringBuilder data = new StringBuilder("");
            foreach (string line in list)
            {
                data.Append(line + Environment.NewLine);
            }
            File.WriteAllText(file, data.ToString());
        }

        public void PrintInfo()
        {
            if (Core._Main.Chat != null)
            {
                Core._Main.Chat.scrollback.InsertText("Information about cache:", Scrollback.MessageStyle.System, false);
                lock (networkInfo)
                {
                    foreach (KeyValuePair<string, NetworkInfo> xx in networkInfo)
                    {
                        Core._Main.Chat.scrollback.InsertText("Network: " + xx.Value.Server + " MQID: " + xx.Value.lastMQID.ToString(), Scrollback.MessageStyle.System, false);
                    }
                }
            }
        }

        public void WriteDisk()
        {
            try
            {
                if (!Directory.Exists(Root))
                {
                    Directory.CreateDirectory(Root);
                }
                Core.DebugLog("Saving cache for " + protocol.Server);
                List<string> files = new List<string>();
                lock (networkInfo)
                {
                    foreach (KeyValuePair<string, NetworkInfo> network in networkInfo)
                    {
                        string xx = SerializeNetwork(network.Value);
                        File.WriteAllText(Root + network.Key, xx);
                        files.Add(network.Key);
                    }
                }

                ListFile(files, Root + "data");
                Modified = false;
                Core.DebugLog("Done");
            }
            catch (Exception fail)
            {
                Core.handleException(fail);
            }
        }
    }
}