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
    public class ProcessorIRC
    {
        public Network _server = null;
        public Protocol protocol = null;
        public string text;
        public string sn;
        public DateTime pong;
        public long date = 0;
        public string system = "";
        public bool updated_text = true;

        private void Ping()
        {
            pong = DateTime.Now;
            return;
        }

        private bool Info(string command, string parameters, string value)
        {
            if (parameters.Contains("PREFIX=("))
            {
                string cmodes = parameters.Substring(parameters.IndexOf("PREFIX=(") + 8);
                cmodes = cmodes.Substring(0, cmodes.IndexOf(")"));
                lock (_server.CUModes)
                {
                    _server.CUModes.Clear();
                    _server.CUModes.AddRange(cmodes.ToArray<char>());
                }
                cmodes = parameters.Substring(parameters.IndexOf("PREFIX=(") + 8);
                cmodes = cmodes.Substring(cmodes.IndexOf(")") + 1, _server.CUModes.Count);

                _server.UChars.Clear();
                _server.UChars.AddRange(cmodes.ToArray<char>());

            }
            if (parameters.Contains("CHANMODES="))
            {
                string xmodes = parameters.Substring(parameters.IndexOf("CHANMODES=") + 11);
                xmodes = xmodes.Substring(0, xmodes.IndexOf(" "));
                string[] _mode = xmodes.Split(',');
                _server.parsed_info = true;
                if (_mode.Length == 4)
                {
                    _server.PModes.Clear();
                    _server.CModes.Clear();
                    _server.XModes.Clear();
                    _server.SModes.Clear();
                    _server.PModes.AddRange(_mode[0].ToArray<char>());
                    _server.XModes.AddRange(_mode[1].ToArray<char>());
                    _server.SModes.AddRange(_mode[2].ToArray<char>());
                    _server.CModes.AddRange(_mode[3].ToArray<char>());
                }

            }
            return true;
        }

        private bool ChannelInfo(string[] code, string command, string source, string parameters, string _value)
        {
            if (code.Length > 3)
            {
                string name = code[2];
                string topic = _value;
                Channel channel = _server.getChannel(code[3]);
                if (channel != null)
                {
                    Window curr = channel.retrieveWindow();
                    if (curr != null)
                    {
                        channel._mode.mode(code[4]);
                        channel.UpdateInfo();
                        curr.scrollback.InsertText("Mode: " + code[4], Scrollback.MessageStyle.Channel, true, date, !updated_text);
                    }
                    return true;
                }
            }
            return false;
        }

        private bool ChannelTopic(string[] code, string command, string source, string parameters, string value)
        {
            if (code.Length > 3)
            {
                string name = "";
                if (parameters.Contains("#"))
                {
                    name = parameters.Substring(parameters.IndexOf("#")).Replace(" ", "");
                }
                string topic = value;
                Channel channel = _server.getChannel(name);
                if (channel != null)
                {
                    Window curr = channel.retrieveWindow();
                    if (curr != null)
                    {
                        while (curr.scrollback == null)
                        {
                            System.Threading.Thread.Sleep(100);
                        }
                        curr.scrollback.InsertText("Topic: " + topic, Scrollback.MessageStyle.Channel, true, date, !updated_text);
                    }
                    channel.Topic = topic;
                    channel.UpdateInfo();
                    return true;
                }
            }
            return false;
        }

        private bool FinishChan(string[] code)
        {
            if (code.Length > 2)
            {
                Channel channel = _server.getChannel(code[3]);
                if (channel != null)
                {
                    channel.redrawUsers();
                    if (Configuration.HidingParsed && channel.parsing_who)
                    {
                        channel.parsing_who = false;
                        return true;
                    }
                    channel.parsing_who = false;
                    channel.UpdateInfo();
                }
            }
            return false;
        }

        private bool TopicInfo(string[] code, string parameters)
        {
            if (code.Length > 5)
            {
                string name = code[3];
                string user = code[4];
                string time = code[5];
                Channel channel = _server.getChannel(name);
                if (channel != null)
                {
                    channel.TopicDate = int.Parse(time);
                    channel.TopicUser = user;
                    Window curr = channel.retrieveWindow();
                    if (curr != null)
                    {
                        curr.scrollback.InsertText("Topic by: " + user + " date " + Network.convertUNIX(time).ToString(),
                            Scrollback.MessageStyle.Channel, !channel.temporary_hide, date, !updated_text);
                        return true;
                    }
                    channel.UpdateInfo();
                }
            }
            return false;
        }

        private bool ParseUs(string[] code)
        {
            if (code.Length > 8)
            {
                Channel channel = _server.getChannel(code[3]);
                string ident = code[4];
                string host = code[5];
                string nick = code[7];
                string server = code[6];
                string mode = "";
                if (code[8].Length > 0)
                {
                    mode = code[8][code[8].Length - 1].ToString();
                    if (mode == "G" || mode == "H")
                    {
                        mode = "";
                    }
                }
                if (channel != null)
                {
                    if (updated_text)
                    {
                        if (!channel.containsUser(nick))
                        {
                            User _user = new User(mode + nick, host, _server, ident);
                            channel.UserList.Add(_user);
                            return true;
                        }
                        foreach (User u in channel.UserList)
                        {
                            if (u.Nick == nick)
                            {
                                u.Ident = ident;
                                u.Host = host;
                                break;
                            }
                        }
                    }
                    if (Configuration.HidingParsed && channel.parsing_who)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool ParseInfo(string[] code, string[] data)
        {
            if (code.Length > 3)
            {
                string name = code[4];
                if (!updated_text)
                {
                    return true;
                }
                Channel channel = _server.getChannel(name);
                if (channel != null)
                {
                    string[] _chan = data[2].Split(' ');
                    foreach (var user in _chan)
                    {
                        if (!channel.containsUser(user) && user != "")
                        {
                            lock (channel.UserList)
                            {
                                channel.UserList.Add(new User(user, "", _server, ""));
                            }
                        }
                    }
                    channel.redrawUsers();
                    channel.UpdateInfo();
                    return true;
                }
            }
            return false;
        }

        private bool ProcessPM(string source, string parameters, string value)
        {
            string _nick;
            string _ident;
            string _host;
            string chan;
            _nick = source.Substring(0, source.IndexOf("!"));
            _host = source.Substring(source.IndexOf("@") + 1);
            _ident = source.Substring(source.IndexOf("!") + 1);
            _ident = _ident.Substring(0, _ident.IndexOf("@"));
            chan = parameters.Replace(" ", "");
            string message = value;
            if (!chan.Contains(_server.channel_prefix))
            {
                string uc;
                if (message.StartsWith(protocol.delimiter.ToString()))
                {
                    uc = message.Substring(1);
                    if (uc.Contains(protocol.delimiter))
                    {
                        uc = uc.Substring(0, uc.IndexOf(protocol.delimiter.ToString()));
                    }
                    uc = uc.ToUpper();
                    switch (uc)
                    {
                        case "VERSION":
                            protocol.Transfer("NOTICE " + _nick + " :" + protocol.delimiter.ToString() + "VERSION " + Configuration.Version + " http://pidgeonclient.org/wiki/",
                                Configuration.Priority.Low);
                            break;
                        case "TIME":
                            protocol.Transfer("NOTICE " + _nick + " :" + protocol.delimiter.ToString() + "TIME " + DateTime.Now.ToString(),
                                Configuration.Priority.Low);
                            break;
                        case "PING":
                            if (message.Length > 6)
                            {
                                string time = message.Substring(6);
                                if (time.Contains(_server._protocol.delimiter))
                                {
                                    time = message.Substring(0, message.IndexOf(_server._protocol.delimiter));
                                    protocol.Transfer("NOTICE " + _nick + " :" + protocol.delimiter.ToString() + "PING " + time,
                                        Configuration.Priority.Low);
                                }
                            }
                            break;
                    }
                    if (Configuration.DisplayCtcp)
                    {
                        protocol.windows[system].scrollback.InsertText("CTCP from (" + _nick + ") " + message,
                            Scrollback.MessageStyle.Message, true, date, !updated_text);
                        return true; ;
                    }
                    return true;
                }

            }
            User user = new User(_nick, _host, _server, _ident);
            Channel channel = null;
            if (chan.StartsWith(_server.channel_prefix))
            {
                channel = _server.getChannel(chan);
                if (channel != null)
                {
                    Window window;
                    window = channel.retrieveWindow();
                    if (window != null)
                    {
                        if (message.StartsWith(protocol.delimiter.ToString() + "ACTION"))
                        {
                            message = message.Substring("xACTION".Length);
                            channel.retrieveWindow().scrollback.InsertText(">>>>>>" + _nick + message, Scrollback.MessageStyle.Action,
                                !channel.temporary_hide, date, !updated_text);
                            return true;
                        }
                        channel.retrieveWindow().scrollback.InsertText(protocol.PRIVMSG(user.Nick, message),
                            Scrollback.MessageStyle.Message, !channel.temporary_hide, date, !updated_text);
                    }
                    channel.UpdateInfo();
                    return true;
                }
                return true;
            }
            if (chan == _server.nickname)
            {
                chan = source.Substring(0, source.IndexOf("!"));
                if (!protocol.windows.ContainsKey(_server.window + chan))
                {
                    _server.Private(chan);
                }
                protocol.windows[_server.window + chan].scrollback.InsertText(protocol.PRIVMSG(chan, message),
                    Scrollback.MessageStyle.Message, updated_text, date, !updated_text);
                return true;
            }
            return false;
        }

        private bool ChannelBans(string[] code)
        {
            if (code.Length > 6)
            {
                string chan = code[3];
                Channel channel = _server.getChannel(code[3]);
                if (channel != null)
                {
                    channel.UpdateInfo();
                    if (channel.Bl == null)
                    {
                        channel.Bl = new List<SimpleBan>();
                    }
                    if (!channel.containsBan(code[4]))
                    {
                        channel.Bl.Add(new SimpleBan(code[5], code[4], code[6]));
                        Core._Main.Status();
                    }
                }
            }
            return false;
        }

        private bool ProcessNick(string source, string parameters, string value)
        {
            string nick = source.Substring(0, source.IndexOf("!"));
            string _new = value;
            foreach (Channel item in _server.Channels)
            {
                if (item.ok)
                {
                    lock (item.UserList)
                    {
                        foreach (User curr in item.UserList)
                        {
                            if (curr.Nick == nick)
                            {
                                if (updated_text)
                                {
                                    curr.Nick = _new;
                                    item.redrawUsers();
                                }
                                Window x = item.retrieveWindow();
                                if (x != null)
                                {
                                    x.scrollback.InsertText(messages.get("protocol-nick", Core.SelectedLanguage,
                                        new List<string> { nick, _new }), Scrollback.MessageStyle.Channel,
                                        !item.temporary_hide, date, !updated_text);
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }

        private bool Mode(string source, string parameters, string value)
        {
            if (parameters.Contains(" "))
            {
                string chan = parameters.Substring(0, parameters.IndexOf(" "));
                chan = chan.Replace(" ", "");
                string user = source;
                if (chan.StartsWith(_server.channel_prefix))
                {
                    Channel channel = _server.getChannel(chan);
                    if (channel != null)
                    {
                        Window window;
                        window = channel.retrieveWindow();
                        string change = parameters.Substring(parameters.IndexOf(" "));
                        if (window != null)
                        {
                            channel.retrieveWindow().scrollback.InsertText(messages.get("channel-mode", Core.SelectedLanguage,
                                new List<string> { source, parameters.Substring(parameters.IndexOf(" ")) }),
                                Scrollback.MessageStyle.Action, !channel.temporary_hide, date, !updated_text);
                        }

                        while (change.StartsWith(" "))
                        {
                            change = change.Substring(1);
                        }

                        channel._mode.mode(change);

                        while (change.EndsWith(" ") && change.Length > 1)
                        {
                            change = change.Substring(0, change.Length - 1);
                        }

                        if (change.Contains(" "))
                        {
                            string header = change.Substring(0, change.IndexOf(" "));
                            List<string> parameters2 = new List<string>();
                            parameters2.AddRange(change.Substring(change.IndexOf(" ") + 1).Split(' '));
                            int curr = 0;

                            char type = ' ';

                            foreach (char m in header)
                            {

                                if (m == '+')
                                {
                                    type = '+';
                                }
                                if (m == '-')
                                {
                                    type = '-';
                                }
                                if (type == ' ')
                                {
                                    continue;
                                }
                                if (_server.CUModes.Contains(m) && curr <= parameters2.Count)
                                {
                                    User flagged_user = channel.userFromName(parameters2[curr]);
                                    if (flagged_user != null)
                                    {
                                        flagged_user.ChannelMode.mode(type.ToString() + m.ToString());
                                    }
                                    curr++;
                                    channel.redrawUsers();
                                }
                                if (parameters2.Count > curr)
                                {
                                    switch (m.ToString())
                                    {
                                        case "b":
                                            if (channel.Bl == null)
                                            {
                                                channel.Bl = new List<SimpleBan>();
                                            }
                                            lock (channel.Bl)
                                            {
                                                if (type == '-')
                                                {
                                                    SimpleBan br = null;
                                                    foreach (SimpleBan xx in channel.Bl)
                                                    {
                                                        if (xx.Target == parameters2[curr])
                                                        {
                                                            br = xx;
                                                            break;
                                                        }
                                                    }
                                                    if (br != null)
                                                    {
                                                        channel.Bl.Remove(br);
                                                    }
                                                    break;
                                                }
                                                channel.Bl.Add(new SimpleBan(user, parameters2[curr], ""));
                                            }
                                            curr++;
                                            break;
                                    }
                                }
                            }
                        }
                        channel.UpdateInfo();
                        return true;
                    }
                }
            }
            return false;
        }

        private bool Part(string source, string parameters, string value)
        {
            string chan = parameters;
            chan = chan.Replace(" ", "");
            string user = source.Substring(0, source.IndexOf("!"));
            string _ident;
            string _host;
            _host = source.Substring(source.IndexOf("@") + 1);
            _ident = source.Substring(source.IndexOf("!") + 1);
            _ident = _ident.Substring(0, _ident.IndexOf("@"));
            Channel channel = _server.getChannel(chan);
            if (!Hooks.BeforePart(_server, channel)) { return true; }
            if (channel != null)
            {
                Window window;
                window = channel.retrieveWindow();
                User delete = null;
                if (window != null)
                {
                    channel.retrieveWindow().scrollback.InsertText(messages.get("window-p1",
                        Core.SelectedLanguage, new List<string> { "%L%" + user + "%/L%!%D%" + _ident + "%/D%@%H%" + _host + "%/H%", value }),
                        Scrollback.MessageStyle.Part,
                        !channel.temporary_hide, date, !updated_text);

                    if (channel.containsUser(user))
                    {
                        lock (channel.UserList)
                        {
                            foreach (User _user in channel.UserList)
                            {
                                if (_user.Nick == user)
                                {
                                    delete = _user;
                                    break;
                                }
                            }
                        }

                        if (updated_text)
                        {
                            if (delete != null)
                            {
                                channel.UserList.Remove(delete);
                            }
                            channel.redrawUsers();
                            channel.UpdateInfo();
                        }
                        return true;
                    }
                    return true;
                }
            }
            return false;
        }

        private bool Topic(string source, string parameters, string value)
        {
            string chan = parameters;
            chan = chan.Replace(" ", "");
            string user = source.Substring(0, source.IndexOf("!"));
            Channel channel = _server.getChannel(chan);
            if (channel != null)
            {
                Window window;
                channel.Topic = value;
                window = channel.retrieveWindow();
                if (window != null)
                {
                    channel.retrieveWindow().scrollback.InsertText(messages.get("channel-topic",
                        Core.SelectedLanguage, new List<string> { source, value }), Scrollback.MessageStyle.Channel,
                        !channel.temporary_hide, date, !updated_text);
                    return true;
                }
                channel.UpdateInfo();
            }
            return false;
        }

        private bool Quit(string source, string parameters, string value)
        {
            string user = source.Substring(0, source.IndexOf("!"));
            string _ident;
            string _host;
            _host = source.Substring(source.IndexOf("@") + 1);
            _ident = source.Substring(source.IndexOf("!") + 1);
            _ident = _ident.Substring(0, _ident.IndexOf("@"));
            string _new = value;
            foreach (Channel item in _server.Channels)
            {
                if (item.ok)
                {
                    User target = null;
                    lock (item.UserList)
                    {
                        foreach (User curr in item.UserList)
                        {
                            if (curr.Nick == user)
                            {
                                target = curr;
                                break;
                            }
                        }
                    }
                    if (target != null)
                    {
                        Window x = item.retrieveWindow();
                        if (x != null && x.scrollback != null)
                        {
                            x.scrollback.InsertText(messages.get("protocol-quit", Core.SelectedLanguage,
                                new List<string> { "%L%" + user + "%/L%!%D%" + _ident + "%/D%@%H%" + _host + "%/H%", value }),
                                Scrollback.MessageStyle.Join,
                                !item.temporary_hide, date, !updated_text);
                        }
                        if (updated_text)
                        {
                            lock (item.UserList)
                            {
                                item.UserList.Remove(target);
                            }
                            item.UpdateInfo();
                            item.redrawUsers();
                        }
                    }
                }
            }
            return true;
        }

        private bool Kick(string source, string parameters, string value)
        {
            string user = parameters.Substring(parameters.IndexOf(" ") + 1);
            // petan!pidgeon@petan.staff.tm-irc.org KICK #support HelpBot :Removed from the channel
            Channel channel = _server.getChannel(parameters.Substring(0, parameters.IndexOf(" ")));
            if (channel != null)
            {
                Window window;
                window = channel.retrieveWindow();
                if (window != null)
                {
                    channel.retrieveWindow().scrollback.InsertText(messages.get("userkick", Core.SelectedLanguage,
                        new List<string> { source, user, value }),
                        Scrollback.MessageStyle.Join, !channel.temporary_hide, date, !updated_text);

                    if (updated_text && channel.containsUser(user))
                    {
                        User delete = null;
                        lock (channel.UserList)
                        {
                            foreach (User _user in channel.UserList)
                            {
                                if (_user.Nick == user)
                                {
                                    delete = _user;
                                    break;
                                }
                            }
                            if (delete != null)
                            {
                                channel.UserList.Remove(delete);
                            }
                        }
                    }
                    channel.redrawUsers();
                    channel.UpdateInfo();
                }
                return true;
            }
            return false;
        }

        private bool Join(string source, string parameters, string value)
        {
            string chan = parameters;
            chan = chan.Replace(" ", "");
            if (chan == "")
            {
                chan = value;
            }
            string user = source.Substring(0, source.IndexOf("!"));
            string _ident;
            string _host;
            _host = source.Substring(source.IndexOf("@") + 1);
            _ident = source.Substring(source.IndexOf("!") + 1);
            _ident = _ident.Substring(0, _ident.IndexOf("@"));
            Channel channel = _server.getChannel(chan);
            if (channel != null)
            {
                Window window;
                window = channel.retrieveWindow();
                if (window != null)
                {
                    channel.retrieveWindow().scrollback.InsertText(messages.get("join", Core.SelectedLanguage,
                        new List<string> { "%L%" + user + "%/L%!%D%" + _ident + "%/D%@%H%" + _host + "%/H%" }),
                        Scrollback.MessageStyle.Join, !channel.temporary_hide, date, !updated_text);
                    if (updated_text)
                    {
                        if (!channel.containsUser(user))
                        {
                            lock (channel.UserList)
                            {
                                channel.UserList.Add(new User(user, _host, _server, _ident));
                            }
                            channel.redrawUsers();
                        }
                    }
                    channel.UpdateInfo();
                    return true;
                }
            }
            return false;
        }

        private bool IdleTime(string source, string parameters, string value)
        {
            if (parameters.Contains(" "))
            {

                Core.DebugLog(parameters + "xx" + value);
                string name = parameters.Substring(parameters.IndexOf(" ") + 1);
                if (name.Contains(" ") != true)
                {
                    return false;
                }
                string idle = name.Substring(name.IndexOf(" ") + 1);
                if (idle.Contains(" ") != true)
                {
                    return false;
                }
                string uptime = idle.Substring(idle.IndexOf(" ") + 1);
                name = name.Substring(0, name.IndexOf(" "));
                idle = idle.Substring(0, idle.IndexOf(" "));
                DateTime logintime = Network.convertUNIX(uptime);
                if (logintime == null)
                {
                    return false;
                }
                protocol.windows[system].scrollback.InsertText("WHOIS " + name + " is online since " + logintime.ToString() + "(" + (DateTime.Now - logintime).ToString() + " ago) idle for " + idle + " seconds", Scrollback.MessageStyle.System, true, date, true);
                return true;
            }
            return false;
        }

        public bool Result()
        {
            try
            {
                if (text == null || text == "")
                {
                    return false;
                }
                if (text.StartsWith(":"))
                {
                    string[] data = text.Split(':');
                    if (data.Length > 1)
                    {
                        string command = "";
                        string parameters = "";
                        string command2 = "";
                        string source;
                        string _value;
                        source = text.Substring(1);
                        source = source.Substring(0, source.IndexOf(" "));
                        command2 = text.Substring(1);
                        command2 = command2.Substring(source.Length + 1);
                        if (command2.Contains(" :"))
                        {
                            command2 = command2.Substring(0, command2.IndexOf(" :"));
                        }
                        string[] _command = command2.Split(' ');
                        if (_command.Length > 0)
                        {
                            command = _command[0];
                        }
                        if (_command.Length > 1)
                        {
                            int curr = 1;
                            while (curr < _command.Length)
                            {
                                parameters += _command[curr] + " ";
                                curr++;
                            }
                            if (parameters.EndsWith(" "))
                            {
                                parameters = parameters.Substring(0, parameters.Length - 1);
                            }
                        }
                        _value = "";
                        if (text.Length > 3 + command2.Length + source.Length)
                        {
                            _value = text.Substring(3 + command2.Length + source.Length);
                        }
                        if (_value.StartsWith(":"))
                        {
                            _value = _value.Substring(1);
                        }
                        string[] code = data[1].Split(' ');
                        switch (command)
                        {
                            case "001":
                                return true;
                            case "002":
                            case "003":
                            case "004":
                            case "005":
                                if (Info(command, parameters, _value))
                                {
                                    //return true;
                                }
                                break;
                            case "317":
                                if (Configuration.FriendlyWho)
                                {
                                    if (IdleTime(command, parameters, _value))
                                    {
                                        return true;
                                    }
                                }
                                break;
                            case "PONG":
                                Ping();
                                return true;
                            case "INFO":
                                protocol.windows[system].scrollback.InsertText(text.Substring(text.IndexOf("INFO") + 5), Scrollback.MessageStyle.User, true, date, !updated_text);
                                return true;
                            case "NOTICE":
                                if (parameters.Contains(_server.channel_prefix))
                                {
                                    Channel channel = _server.getChannel(parameters);
                                    if (channel != null)
                                    {
                                        Window window;
                                        window = channel.retrieveWindow();
                                        if (window != null)
                                        {
                                            window.scrollback.InsertText("[" + source + "] " + _value, Scrollback.MessageStyle.Message, true, date, !updated_text);
                                            return true;
                                        }
                                    }
                                }
                                protocol.windows[system].scrollback.InsertText("[" + source + "] " + _value, Scrollback.MessageStyle.Message, true, date, !updated_text);
                                return true;
                            case "PING":
                                protocol.Transfer("PONG ", Configuration.Priority.High);
                                return true;
                            case "NICK":
                                if (ProcessNick(source, parameters, _value))
                                {
                                    return true;
                                }
                                break;
                            case "PRIVMSG":
                                if (ProcessPM(source, parameters, _value))
                                {
                                    return true;
                                }
                                break;
                            case "TOPIC":
                                if (Topic(source, parameters, _value))
                                {
                                    return true;
                                }
                                break;
                            case "MODE":
                                if (Mode(source, parameters, _value))
                                {
                                    return true;
                                }
                                break;
                            case "PART":
                                if (Part(source, parameters, _value))
                                {
                                    return true;
                                }
                                break;
                            case "QUIT":
                                if (Quit(source, parameters, _value))
                                {
                                    return true;
                                }
                                break;
                            case "JOIN":
                                if (Join(source, parameters, _value))
                                {
                                    return true;
                                }
                                break;
                            case "KICK":
                                if (Kick(source, parameters, _value))
                                {
                                    return true;
                                }
                                break;
                        }
                        if (data[1].Contains(" "))
                        {
                            switch (command)
                            {
                                case "315":
                                    if (FinishChan(code))
                                    {
                                        return true;
                                    }
                                    break;
                                case "324":
                                    if (ChannelInfo(code, command, source, parameters, _value))
                                    {
                                        return true;
                                    }
                                    break;
                                case "332":
                                    if (ChannelTopic(code, command, source, parameters, _value))
                                    {
                                        return true;
                                    }
                                    break;
                                case "333":
                                    if (TopicInfo(code, parameters))
                                    {
                                        return true;
                                    }
                                    break;
                                case "352":
                                    if (ParseUs(code))
                                    {
                                        return true;
                                    }
                                    break;
                                case "353":
                                    if (ParseInfo(code, data))
                                    {
                                        return true;
                                    }
                                    break;
                                case "366":
                                    return true;
                                case "367":
                                    if (ChannelBans(code))
                                    {
                                        return true;
                                    }
                                    break;
                            }
                        }
                        if (source.StartsWith(_server.nickname + "!"))
                        {
                            string[] _data2 = data[1].Split(' ');
                            if (_data2.Length > 2)
                            {
                                if (_data2[1].Contains("JOIN"))
                                {
                                    string channel = _data2[2];
                                    if (_data2[2].Contains("#") == false)
                                    {
                                        channel = data[2];
                                    }
                                    Channel curr = _server.getChannel(channel);
                                    if (curr == null)
                                    {
                                        curr = _server.Join(channel);
                                    }
                                    if (Configuration.aggressive_mode)
                                    {
                                        protocol.Transfer("MODE " + channel, Configuration.Priority.Low);
                                    }

                                    if (Configuration.aggressive_exception)
                                    {
                                        curr.parsing_xe = true;
                                        protocol.Transfer("MODE " + channel + " +e", Configuration.Priority.Low);
                                    }

                                    if (Configuration.aggressive_bans)
                                    {
                                        curr.parsing_xb = true;
                                        protocol.Transfer("MODE " + channel + " +b", Configuration.Priority.Low);
                                    }

                                    if (Configuration.aggressive_invites)
                                    {
                                        protocol.Transfer("MODE " + channel + " +I", Configuration.Priority.Low);
                                    }

                                    if (Configuration.aggressive_channel)
                                    {
                                        curr.parsing_who = true;
                                        protocol.Transfer("WHO " + channel, Configuration.Priority.Low);
                                    }
                                    return true;
                                }
                            }
                            if (_data2.Length > 2)
                            {
                                if (_data2[1].Contains("NICK"))
                                {
                                    protocol.windows[system].scrollback.InsertText(messages.get("protocolnewnick", Core.SelectedLanguage, new List<string> { _value }), Scrollback.MessageStyle.User, true, date);
                                    _server.nickname = _value;
                                }
                                if (_data2[1].Contains("PART"))
                                {
                                    string channel = _data2[2];
                                    if (_data2[2].Contains(_server.channel_prefix))
                                    {
                                        channel = _data2[2];
                                        Channel c = _server.getChannel(channel);
                                        if (c != null)
                                        {
                                            Window Chat = c.retrieveWindow();
                                            if (c != null)
                                            {
                                                if (c.ok)
                                                {
                                                    c.ok = false;
                                                    Chat.scrollback.InsertText(messages.get("part1", Core.SelectedLanguage),
                                                        Scrollback.MessageStyle.Message, !c.temporary_hide, date);
                                                }
                                                else
                                                {
                                                    Chat.scrollback.InsertText(messages.get("part2", Core.SelectedLanguage),
                                                        Scrollback.MessageStyle.Message, !c.temporary_hide, date);
                                                }
                                            }
                                            c.ok = false;
                                            c.UpdateInfo();
                                        }
                                    }
                                    return true;
                                }
                            }
                        }
                    }
                }
                protocol.windows[system].scrollback.InsertText(text, Scrollback.MessageStyle.System, true, date, true);
            }
            catch (Exception fail)
            {
                Core.handleException(fail);
            }
            return true;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="_network">Network</param>
        /// <param name="_protocol">Protocol</param>
        /// <param name="_text">Text</param>
        /// <param name="_sn"></param>
        /// <param name="ws"></param>
        /// <param name="_pong"></param>
        /// <param name="_date">Date of this message, if you specify 0 the current time will be used</param>
        /// <param name="updated">If true this text will be considered as newly obtained information</param>
        public ProcessorIRC(Network _network, Protocol _protocol, string _text, string _sn, string ws, ref DateTime _pong, long _date = 0, bool updated = true)
        {
            _server = _network;
            protocol = _protocol;
            text = _text;
            sn = _sn;
            system = ws;
            pong = _pong;
            date = _date;
            updated_text = updated;
        }
    }

}