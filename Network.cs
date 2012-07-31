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
    public class Network
    {
        public List<char> UModes = new List<char> { 'i', 'w', 'o', 'Q', 'r', 'A' };
        public List<char> UChars = new List<char> { '~', '&', '@', '%', '+' };
        public List<char> CUModes = new List<char> { 'q', 'a', 'o', 'h', 'v' };
        public List<char> CModes = new List<char> { 'n', 'r', 't', 'm' };
        public List<char> SModes = new List<char> { 'k', 'L' };
        public List<char> XModes = new List<char> { 'l' };
        public List<char> PModes = new List<char> { 'b', 'I', 'e' };
        public bool parsed_info = false;
        public string channel_prefix = "#";
        public bool Connected;
        public List<User> PrivateChat = new List<User>();
        public Window system;
        public string server;
        public Protocol.UserMode usermode = new Protocol.UserMode();
        public string username;
        public string randomuqid;
        public List<Channel> Channels = new List<Channel>();
        public Channel RenderedChannel = null;
        public string nickname;
        public string ident;
        public string quit;
        public Protocol _protocol;
        public ProtocolSv ParentSv = null;

        public class Highlighter
        {
            public bool simple;
            public string text;
            public bool enabled;
            public System.Text.RegularExpressions.Regex regex;
            public Highlighter()
            {
                simple = true;
                enabled = false;
                text = Configuration.user;
            }
        }

        public static DateTime convertUNIX(string time)
        {
            double unixtimestmp = 0;
            if (!double.TryParse(time, out unixtimestmp))
            {
                unixtimestmp = 0;
            }
            return (new DateTime(1970, 1, 1, 0, 0, 0)).AddSeconds(unixtimestmp);
        }

        /// <summary>
        /// Retrieve channel
        /// </summary>
        /// <param name="name">String</param>
        /// <returns></returns>
        public Channel getChannel(string name)
        {
            foreach (Channel cu in Channels)
            {
                if (cu.Name == name)
                {
                    return cu;
                }
            }
            return null;
        }

        /// <summary>
        /// Create pm
        /// </summary>
        /// <param name="user"></param>
        public void Private(string user)
        {
            Core._Main.userToolStripMenuItem.Visible = true;
            User u = new User(user, "", this, "");
            PrivateChat.Add(u);
            Core._Main.ChannelList.insertUser(u);
            _protocol.CreateChat(user, true, this, true);
            return;
        }

        public Channel Join(string channel, bool nf = false)
        {
            Channel _channel = new Channel(this);
            RenderedChannel = _channel;
            _channel.Name = channel;
            Channels.Add(_channel);
            Core._Main.ChannelList.insertChannel(_channel);
            _protocol.CreateChat(channel, !nf, this, true, true);
            return _channel;
        }

        public string window
        {
            get 
              {
                  if (_protocol.type != 3)
                  {
                      return "system";
                  }
                  else
                  {
                      return randomuqid + server;
                  }
              }
        }

        public Network(string Server, Protocol protocol)
        {
            randomuqid = Core.retrieveRandom();
            try
            {
                _protocol = protocol;
                server = Server;
                quit = Configuration.quit;
                nickname = Configuration.nick;

                username = Configuration.user;
                ident = Configuration.ident;
                if (protocol.type == 3)
                {
                    _protocol.CreateChat("!" + server, false, this, false, false, "!" + randomuqid + server);
                    system = _protocol.windows["!" + randomuqid + server];
                    Core._Main.ChannelList.insertNetwork(this, (ProtocolSv)protocol);
                }
                else
                {
                    _protocol.CreateChat("!system", true, this);
                    system = _protocol.windows["!system"];
                    Core._Main.ChannelList.insertNetwork(this);
                }
            }
            catch (Exception ex)
            {
                Core.handleException(ex);
            }
        }
    }
}
