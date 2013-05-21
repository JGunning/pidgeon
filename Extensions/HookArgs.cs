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
using System.Threading;
using System.Text;

namespace Client
{
    /// <summary>
    /// Extension
    /// </summary>
    public partial class Extension
    {
        /// <summary>
        /// This is a base for args
        /// </summary>
        public class HookArgs
        {
            /// <summary>
            /// Version
            /// </summary>
            public string ClientVersion = Configuration.Version.ToString();
        }

        /// <summary>
        /// This is a base for network args
        /// </summary>
        public class NetworkArgs : HookArgs
        {
            /// <summary>
            /// Network
            /// </summary>
            public Network network = null;
            /// <summary>
            /// This is only needed when bouncer is being used. If this contains 0 you can ignore it, otherwise you should convert it to
            /// binary time and consider the event to happen in that time.
            /// </summary>
            public long date = 0;
            /// <summary>
            /// This information is up to date and not retrieved from logs
            /// </summary>
            public bool updated = false;
            
        }

        /// <summary>
        /// Topic event args
        /// </summary>
        public class TopicArgs : NetworkArgs
        {
            /// <summary>
            /// Channel
            /// </summary>
            public Channel channel = null;
            /// <summary>
            /// This line represent a user in format user!ident@host
            /// </summary>
            public string Source = null;
            /// <summary>
            /// Changed topic
            /// </summary>
            public string Topic = null;
        }

        /// <summary>
        /// Network info args
        /// </summary>
        public class NetworkInfo : NetworkArgs
        {
            /// <summary>
            /// Command
            /// </summary>
            public string command = null;
            /// <summary>
            /// Parameters
            /// </summary>
            public string parameters = null;
            /// <summary>
            /// Value
            /// </summary>
            public string value = null;
        }

        /// <summary>
        /// User part
        /// </summary>
        public class NetworkPartArgs : NetworkArgs
        {
            /// <summary>
            /// User
            /// </summary>
            public User user = null;
            /// <summary>
            /// Channel
            /// </summary>
            public Channel channel = null;
            /// <summary>
            /// Message
            /// </summary>
            public string message = null;
        }

        /// <summary>
        /// User join
        /// </summary>
        public class NetworkJoinArgs : NetworkArgs
        {
            /// <summary>
            /// User
            /// </summary>
            public User user = null;
            /// <summary>
            /// Channel
            /// </summary>
            public Channel channel = null;
        }

        /// <summary>
        /// User talk
        /// </summary>
        public class NetworkTextArgs : NetworkArgs
        {
            /// <summary>
            /// User
            /// </summary>
            public User user = null;
            /// <summary>
            /// Channel
            /// </summary>
            public Channel channel = null;
            /// <summary>
            /// Message
            /// </summary>
            public string message = null;
        }

        /// <summary>
        /// User
        /// </summary>
        public class NetworkUserQuitArgs : NetworkArgs
        {
            /// <summary>
            /// User
            /// </summary>
            public User user = null;
            /// <summary>
            /// Message
            /// </summary>
            public string message = null;
            /// <summary>
            /// Window
            /// </summary>
            public Graphics.Window window = null;
        }
    }
}