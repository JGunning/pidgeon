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
using System.Linq;
using System.Text;

namespace Client
{
    public class Protocol
    {
        public char delimiter = (char)001;
        public Window Current;
        public Dictionary<string, Window> windows = new Dictionary<string, Window>();
        public bool Connected = false;
        public int type = 0;
        public class Mode
        {
            public List<string> _Mode = new List<string>();
            public Network network;
            public int ModeType = 0;
            public override string ToString()
            {
                string _val = "";
                int curr = 0;
                while (curr < _Mode.Count)
                {
                    _val += _Mode[curr];
                    curr++;
                }
                return "+" + _val;
            }

            public Mode(int MT, Network _Network)
            {
                if (MT != 0)
                {
                    ModeType = MT;
                }
                network = _Network;
            }

            public Mode(string DefaultMode)
            {
                mode(DefaultMode);
            }

            public Mode()
            {
                network = null;
            }

            public bool mode(string text)
            {
                char prefix = ' ';
                foreach (char _x in text)
                {
                    if (ModeType != 0 && network != null)
                    {
                        switch (ModeType)
                        {
                            case 2:
                                if (network.CModes.Contains(_x))
                                {
                                    continue;
                                }
                                break;
                            case 1:
                                if (network.UModes.Contains(_x))
                                {
                                    continue;
                                }
                                break;
                        }
                    }
                    if (_x == ' ')
                    {
                        return true;
                    }
                    if (_x == '-')
                    {
                        prefix = _x;
                        continue;
                    }
                    if (_x == '+')
                    {
                        prefix = _x;
                        continue;
                    }
                    switch (prefix)
                    { 
                        case '+':
                            if (!_Mode.Contains(_x.ToString()))
                            {
                                this._Mode.Add(_x.ToString());
                            }
                            continue;
                        case '-':
                            if (_Mode.Contains(_x.ToString()))
                            {
                                this._Mode.Remove(_x.ToString());
                            }
                            continue;
                    }continue;
                }
                return false;
            }
        }

        public string Server;
        public int Port;
        public bool SSL;

        public void CreateChat(string _name, bool _Focus, Network network, bool _writable = false, bool channelw = false)
        {
            Main._WindowRequest request = new Main._WindowRequest();
            request.owner = this;
            request.name = _name;
            request.writable = _writable;
            request.window = new Window();
            request._Focus = _Focus;
            request.window._Network = network;
            request.window.name = _name;
            request.window.writable = _writable;
            windows.Add(_name, request.window);
            if (channelw == true)
            {
                request.window.isChannel = true;
            }
            lock (Core._Main.W)
            {
                Core._Main.W.Add(request);
            }
        }

        /// <summary>
        /// Request window to be shown
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool ShowChat(string name)
        {
            if (windows.ContainsKey(name))
            {
                
                Current = windows[name];
                Current.Visible = true;
                if (Current != Core._Main.Chat)
                {
                    if (Core._Main.Chat != null)
                    {
                        Core._Main.Chat.Visible = false;
                    }
                }
                Current.Redraw();
                Core._Main.toolStripStatusChannel.Text = name;
                if (Current.Making == false)
                {
                    Current.textbox.Focus();
                }
                Core._Main.Chat = windows[name];
                Current.BringToFront();
                Current.Making = false;
                Core._Main.UpdateStatus();
            }
            return true;
        }

        /// <summary>
        /// Format a message to given style selected by skin
        /// </summary>
        /// <param name="user"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public string PRIVMSG(string user, string text)
        {
            return Configuration.format_nick.Replace("$1", user) + text;
        }

        /// <summary>
        /// Deliver raw data to server
        /// </summary>
        /// <param name="data"></param>
        /// <param name="_priority"></param>
        public virtual void Transfer(string data, Configuration.Priority _priority = Configuration.Priority.Normal)
        {
            
        }

        /// <summary>
        /// /me
        /// </summary>
        /// <param name="text"></param>
        /// <param name="to"></param>
        /// <param name="_priority"></param>
        /// <returns></returns>
        public virtual int Message2(string text, string to, Configuration.Priority _priority = Configuration.Priority.Normal)
        {
            return 0;
        }

        /// <summary>
        /// Send a message to server
        /// </summary>
        /// <param name="text">Message</param>
        /// <param name="to">User or a channel (needs to be prefixed with #)</param>
        /// <param name="_priority"></param>
        /// <returns></returns>
        public virtual int Message(string text, string to, Configuration.Priority _priority = Configuration.Priority.Normal)
        {
            return 0;
        }

        public virtual int requestNick(string _Nick)
        {
            return 2;
        }

        /// <summary>
        /// Parse a command
        /// </summary>
        /// <param name="cm"></param>
        /// <returns></returns>
        public virtual bool Command(string cm)
        {
            return false;
        }

        /// <summary>
        /// Write a mode
        /// </summary>
        /// <param name="_x"></param>
        /// <param name="target"></param>
        /// <param name="network"></param>
        public virtual void WriteMode(Mode _x, string target, Network network = null)
        {
            return;
        }

        public virtual void Join(string name, Network network = null)
        {
            return;
        }

        public virtual bool ConnectTo(string server, int port)
        {
            return false;
        }

        public virtual void Part(string name, Network network = null)
        {
        
        }

        public virtual void Exit() { }  

        public class UserMode : Mode
        {
            
        }

        public virtual bool Open()
        {
            return false;
        }
    }
}
