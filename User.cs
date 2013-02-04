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
    public class User : IComparable
    {
        public string Host;
        public Network _Network;
        public string Ident;
        public NetworkMode ChannelMode = new NetworkMode();
        public string Nick;
        public List<Channel> ChannelList;
        public User(string nick, string host, Network network, string ident)
        {
            ChannelList = new List<Channel>();
            _Network = network;
            if (nick != "")
            {
                char prefix = nick[0];
                if (network.UChars.Contains(prefix))
                {
                    int Mode = network.UChars.IndexOf(prefix);
                    if (network.CUModes.Count >= Mode + 1)
                    {
                        ChannelMode.mode("+" + network.CUModes[Mode].ToString());
                        nick = nick.Substring(1);
                    }
                }
            }
            Nick = nick;
            Ident = ident;
            Host = host;
        }

        /// <summary>
        /// Converts a user object to string
        /// </summary>
        /// <returns>[nick!ident@host]</returns>
        public override string ToString()
        {
            return Nick + "!" + Ident + "@" + Host;
        }

        public int CompareTo(object obj)
        {
            if (obj is User)
            {
                return this.Nick.CompareTo((obj as User).Nick);
            }
            return 0;
        }
    }
}
