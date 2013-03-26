/***************************************************************************
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
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using Gtk;

namespace Client.Graphics
{
    [System.ComponentModel.ToolboxItem(true)]
    public partial class PidgeonList : Gtk.Bin
    {
        /// <summary>
        /// List of services which are currently in sidebar
        /// </summary>
        public Dictionary<ProtocolSv, TreeIter> ServiceList = new Dictionary<ProtocolSv, TreeIter>();
        /// <summary>
        /// List of servers which are currently in sidebar
        /// </summary>
        public Dictionary<Network, TreeIter> ServerList = new Dictionary<Network, TreeIter>();
        /// <summary>
        /// List of channels which are currently in sidebar
        /// </summary>
        public Dictionary<Channel, TreeIter> ChannelList = new Dictionary<Channel, TreeIter>();
        public Dictionary<User, TreeIter> UserList = new Dictionary<User, TreeIter>();
        private LinkedList<User> queueUsers = new LinkedList<User>();
        private LinkedList<Channel> queueChannels = new LinkedList<Channel>();
        private List<Network> queueNetwork = new List<Network>();
        public static bool Updated = false;
        private global::Gtk.ScrolledWindow GtkScrolledWindow;
        private global::Gtk.TreeView tv;
        private Gtk.TreeStore Values = new TreeStore(typeof(string), typeof(object), typeof(ItemType));
        private GLib.TimeoutHandler timer;

        protected virtual void Build()
        {
            global::Stetic.Gui.Initialize(this);
            global::Stetic.BinContainer.Attach(this);
            this.Name = "Client.Graphics.PidgeonList";
            this.GtkScrolledWindow = new global::Gtk.ScrolledWindow();
            this.GtkScrolledWindow.Name = "GtkScrolledWindow";
            this.GtkScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
            this.tv = new global::Gtk.TreeView();
            this.tv.CanFocus = true;
            this.tv.Name = "treeview1";
            Gtk.TreeViewColumn Column = new TreeViewColumn();
            Gtk.CellRendererText Item = new Gtk.CellRendererText();
            Column.Title = messages.get("list-active-conn", messages.Language);
            Column.PackStart(Item, true);
            tv.AppendColumn(Column);
            Column.AddAttribute(Item, "text", 0);
            this.tv.Model = Values;
            this.GtkScrolledWindow.Add(this.tv);
            timer = new GLib.TimeoutHandler(timer01_Tick);
            GLib.Timeout.Add(200, timer);

            this.Add(this.GtkScrolledWindow);
            if ((this.Child != null))
            {
                this.Child.ShowAll();
            }
            this.Hide();
        }

        public PidgeonList()
        {
            try
            {
                this.Build();
            }
            catch (Exception fail)
            {
                Core.handleException(fail);
            }
        }

        /// <summary>
        /// Insert a channel to list
        /// </summary>
        /// <param name="channel"></param>
        public void insertChannel(Channel channel)
        {
            lock (queueChannels)
            {
                if (queueChannels.Contains(channel))
                {
                    return;
                }
                queueChannels.AddLast(channel);
                Updated = true;
            }
        }

        /// <summary>
        /// Insert a user to list (thread safe)
        /// </summary>
        /// <param name="user"></param>
        public void insertUser(User user)
        {
            lock (queueUsers)
            {
                queueUsers.AddLast(user);
            }
        }

        private void _insertUser(User user)
        {
            lock (ServerList)
            {
                if (ServerList.ContainsKey(user._Network))
                {
                    //text.ImageIndex = 4;
                    TreeIter text = Values.AppendValues(ServerList[user._Network], user.Nick, user, ItemType.User);   //Nodes.Insert(ServerList[user._Network].Nodes.Count, text);
                    //text.Text = user.Nick;

                    lock (UserList)
                    {
                        UserList.Add(user, text);
                    }

                    //ServerList[user._Network].Expand();
                    if (user._Network._Protocol.Windows.ContainsKey(user._Network.window + user.Nick))
                    {
                        user._Network._Protocol.Windows[user._Network.window + user.Nick].treeNode = text;
                    }
                    Updated = true;
                }
            }
        }

        public void insertSv(ProtocolSv service)
        {
            TreeIter text = Values.AppendValues(service.Server, service, ItemType.Services);
            lock (ServiceList)
            {
                ServiceList.Add(service, text);
            }
            service.Windows["!root"].treeNode = text;
        }

        private void insertChan(Channel channel)
        {
            lock (ServerList)
            {
                if (ServerList.ContainsKey(channel._Network))
                {
                    TreeIter text = Values.AppendValues(ServerList[channel._Network], channel.Name, channel, ItemType.Channel);   //Nodes.Insert(ServerList[user._Network].Nodes.Count, text);

                    //ServerList[channel._Network].Expand();
                    lock (ChannelList)
                    {
                        ChannelList.Add(channel, text);
                    }
                    channel.TreeNode = text;
                    //text.ImageIndex = 6;
                    Graphics.Window xx = channel.retrieveWindow();
                    if (xx != null)
                    {
                        xx.treeNode = text;
                    }
                }
            }
        }

        private void _insertNetwork(Network network)
        {
            if (network.ParentSv == null)
            {
                TreeIter text = Values.AppendValues(network.ServerName, network, ItemType.Server);
                lock (ServerList)
                {
                    ServerList.Add(network, text);
                }
                //text.Expand();
                network.SystemWindow.treeNode = text;
                return;
            }
            if (this.ServiceList.ContainsKey(network.ParentSv))
            {
                TreeIter text = Values.AppendValues(ServiceList[network.ParentSv], network.ServerName, network, ItemType.Server);
                network.SystemWindow.treeNode = text;
                ServerList.Add(network, text);
                //ServiceList[network.ParentSv].Expand();
            }
        }

        /// <summary>
        /// insert network to lv (thread safe)
        /// </summary>
        /// <param name="network"></param>
        public void insertNetwork(Network network, ProtocolSv ParentSv = null)
        {
            if (queueNetwork.Contains(network)) return;
            lock (queueNetwork)
            {
                network.ParentSv = ParentSv;
                queueNetwork.Add(network);
                Updated = true;
            }
        }

        private bool timer01_Tick()
        {
            try
            {
                if (Core.notification_waiting)
                {
                    Core.DisplayNote();
                }

                // there is no update needed so skip
                if (!Updated)
                {
                    return true;
                }

                Updated = false;

                lock (queueNetwork)
                {
                    foreach (Network it in queueNetwork)
                    {
                        _insertNetwork(it);
                    }
                    queueNetwork.Clear();
                }

                lock (queueChannels)
                {
                    foreach (Channel item in queueChannels)
                    {
                        insertChan(item);
                    }
                    queueChannels.Clear();
                }

                List<Channel> _channels = new List<Channel>();
                lock (ChannelList)
                {
                    foreach (var chan in ChannelList)
                    {
                        if (chan.Key.dispose)
                        {
                            chan.Key._Network.Channels.Remove(chan.Key);
                            chan.Key.retrieveWindow().Dispose();
                            _channels.Add(chan.Key);
                        }
                    }
                }

                foreach (var chan in _channels)
                {
                    ChannelList.Remove(chan);
                }

                lock (Core._Main.WindowRequests)
                {
                    foreach (Forms.Main._WindowRequest item in Core._Main.WindowRequests)
                    {
                        Core._Main.CreateChat(item.window, item.owner, item.focus);
                        if (item.owner != null && item.focus)
                        {
                            item.owner.ShowChat(item.name);
                        }
                    }
                    Core._Main.WindowRequests.Clear();
                }

                lock (queueUsers)
                {
                    foreach (User user in queueUsers)
                    {
                        _insertUser(user);
                    }
                    queueUsers.Clear();
                }

                lock (ChannelList)
                {
                    foreach (var channel in ChannelList)
                    {
                        if (channel.Key.Redraw)
                        {
                            channel.Key.redrawUsers();
                        }
                    }
                }
            }
            catch (Exception fail)
            {
                Core.handleException(fail);
            }
            return true;
        }

        /*
        public void RedrawMenu()
        {
            partToolStripMenuItem.Visible = false;
            joinToolStripMenuItem.Visible = false;
            disconnectToolStripMenuItem.Visible = false;
        }

        private void items_AfterSelect(object sender, TreeViewEventArgs e)
        {
            try
            {
                RedrawMenu();
                items.SelectedNode.ForeColor = Configuration.CurrentSkin.fontcolor;

                lock (ServiceList)
                {
                    if (ServiceList.ContainsValue(e.Node))
                    {
                        foreach (var sv in ServiceList)
                        {
                            if (sv.Value == e.Node)
                            {
                                sv.Key.ShowChat("!root");
                                Core.network = null;
                                disconnectToolStripMenuItem.Visible = true;
                                Core._Main.UpdateStatus();
                                return;
                            }
                        }
                    }
                }

                lock (ServerList)
                {
                    if (ServerList.ContainsValue(e.Node))
                    {
                        foreach (var cu in ServerList)
                        {
                            if (cu.Value == e.Node)
                            {
                                if (cu.Key.ParentSv == null)
                                {
                                    cu.Key._Protocol.ShowChat("!system");
                                }
                                else
                                {
                                    cu.Key.ParentSv.ShowChat("!" + cu.Key.window);
                                }
                                Core.network = cu.Key;
                                disconnectToolStripMenuItem.Visible = true;
                                Core._Main.UpdateStatus();
                                return;
                            }
                        }
                    }
                }

                lock (UserList)
                {
                    if (UserList.ContainsValue(e.Node))
                    {
                        foreach (var cu in UserList)
                        {
                            if (cu.Value == e.Node)
                            {
                                Core.network = cu.Key._Network;
                                cu.Key._Network._Protocol.ShowChat(cu.Key._Network.window + cu.Key.Nick);
                                closeToolStripMenuItem.Visible = true;
                                Core._Main.UpdateStatus();
                                return;
                            }
                        }
                    }
                }

                lock (ChannelList)
                {
                    if (ChannelList.ContainsValue(e.Node))
                    {
                        foreach (var cu in ChannelList)
                        {
                            if (cu.Value == e.Node)
                            {
                                Core.network = cu.Key._Network;
                                partToolStripMenuItem.Visible = true;
                                closeToolStripMenuItem.Visible = true;
                                cu.Key._Network.RenderedChannel = cu.Key;
                                cu.Key._Network._Protocol.ShowChat(cu.Key._Network.window + cu.Key.Name);
                                Core._Main.UpdateStatus();
                                return;
                            }
                        }
                    }
                }
            }
            catch (Exception f)
            {
                Core.handleException(f);
            }
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (items.SelectedNode == null)
                {
                    return;
                }

                RemoveItem(items.SelectedNode);
            }
            catch (Exception fail)
            {
                Core.handleException(fail);
            }
        }
		
        public void RemoveAll(TreeNodeCollection Item, TreeNode node)
        {
            if (Item.Contains(node))
            {
                Item.Remove(node);
                return;
            }
            foreach (TreeNode Node in Item)
            {
                if (Node.Nodes.Count > 0)
                {
                    RemoveAll(Node.Nodes, node);
                }
            }
        }
		*/
        public void RemoveAll(TreeNode Item)
        {
            //if (items.Nodes.Contains(Item))
            {
                //items.Nodes.Remove(Item);
                return;
            }
            //foreach (TreeNode node in items.Nodes)
            {
                //if (node.Nodes.Count > 0)
                {
                    //    RemoveAll(node.Nodes, Item);
                }
            }
        }

        public void RemoveItem(object Item, ItemType type)
        {
            switch (type)
            {
                case ItemType.Services:
                    ProtocolSv service = (ProtocolSv)Item;
                    service.Exit();
                    lock (ServiceList)
                    {
                        if (ServiceList.ContainsKey(service))
                        {
                            lock (Values)
                            {
                                TreeIter iter = ServiceList[service];
                                Values.Remove(ref iter);
                            }
                            ServiceList.Remove(service);
                        }
                    }
                    break;
                case ItemType.Server:
                    Network network = (Network)Item;
                    if (network.Connected)
                    {
                        Core._Main.Chat.scrollback.InsertText("Server will not be removed from sidebar, because you are still using it, disconnect first", Scrollback.MessageStyle.System, false, 0, true);
                        return;
                    }

                    lock (Core.Connections)
                    {
                        if (Core.Connections.Contains(network._Protocol))
                        {
                            Core.Connections.Remove(network._Protocol);
                        }
                    }

                    lock (ServerList)
                    {
                        if (ServerList.ContainsKey(network))
                        {
                            TreeIter iter = ServerList[network];
                            Values.Remove(ref iter);
                            ServerList.Remove(network);
                        }
                    }

                    /*
                     foreach (TreeNode item in items.SelectedNode.Nodes)
                        {
                            RemoveItem(item);
                        }
                        lock (items.Nodes)
                        {
                            if (items.Nodes.Contains(Item))
                            {
                                items.Nodes.Remove(Item);
                            }
                        }
                     
                     */
                    break;
                case ItemType.User:
                    User user = (User)Item;

                    lock (user._Network.PrivateChat)
                    {
                        if (user._Network.PrivateChat.Contains(user))
                        {
                            lock (user._Network.PrivateWins)
                            {
                                if (user._Network.PrivateWins.ContainsKey(user))
                                {
                                    user._Network.PrivateWins.Remove(user);
                                }
                                else
                                {
                                    Core.DebugLog("There was no private window handle for " + user.Nick);
                                }
                            }
                            user._Network.PrivateChat.Remove(user);
                        }
                    }
                    if (user._Network._Protocol.Windows.ContainsKey(user._Network.window + user.Nick))
                    {
                        lock (user._Network._Protocol.Windows)
                        {
                            user._Network._Protocol.Windows[user._Network.window + user.Nick].Visible = false;
                            user._Network._Protocol.Windows[user._Network.window + user.Nick].Dispose();
                        }
                    }
                    lock (UserList)
                    {
                        if (UserList.ContainsKey(user))
                        {
                            TreeIter iter = UserList[user];
                            Values.Remove(ref iter);
                            UserList.Remove(user);
                        }
                    }
                    break;
                case ItemType.Channel:
                    Channel channel = (Channel)Item;

                    lock (channel._Network.Channels)
                    {
                        if (channel._Network.Channels.Contains(channel))
                        {
                            channel._Network.Channels.Remove(channel);
                        }
                    }
                    //RemoveAll(Item);
                    lock (ChannelList)
                    {
                        if (ChannelList.ContainsKey(channel))
                        {
                            TreeIter iter = ChannelList[channel];
                            Values.Remove(ref iter);
                            ChannelList.Remove(channel);
                        }
                    }
                    break;
            }

            /*lock (ChannelList)
            {
                if (ChannelList.ContainsValue(Item))
                {
                    Channel item = null;
                    foreach (var cu in ChannelList)
                    {
                        if (cu.Value == Item)
                        {
                            if (cu.Key.ChannelWork)
                            {
                                cu.Key._Network._Protocol.Part(cu.Key.Name);
                                //cu.Key.dispose = true;
                                return;
                            }
                            lock (cu.Key._Network.Channels)
                            {
                                if (cu.Key._Network.Channels.Contains(cu.Key))
                                {
                                    cu.Key._Network.Channels.Remove(cu.Key);
                                }
                            }
                            item = cu.Key;
                            RemoveAll(Item);
                            break;
                        }
                    }
                    if (item != null)
                    {
                        lock (items.Nodes)
                        {
                            if (items.Nodes.Contains(Item))
                            {
                                items.Nodes.Remove(Item);
                            }
                        }
                        lock (item.retrieveWindow())
                        {
                            if (item.retrieveWindow() != null)
                            {
                                item.retrieveWindow().Visible = false;
                                item.retrieveWindow().Dispose();
                            }
                            lock (item._Network.Channels)
                            {
                                item._Network.Channels.Remove(item);
                            }
                        }
                        lock (ChannelList)
                        {
                            ChannelList.Remove(item);
                        }
                    }
                }
            }
             

            lock (items.Nodes)
            {
                if (items.Nodes.Contains(Item))
                {
                    items.Nodes.Remove(Item);
                }
            }
             */
        }

        /*
        private void disconnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                lock (ServerList)
                {
                    if (ServerList.ContainsValue(items.SelectedNode))
                    {
                        foreach (var cu in ServerList)
                        {
                            if (cu.Value == items.SelectedNode)
                            {
                                cu.Key.Disconnect();
                                return;
                            }
                        }
                    }
                }
            }
            catch (Exception fail)
            {
                Core.handleException(fail);
            }
        }
		
		*/
        private void partToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                //if (ChannelList.ContainsValue(items.SelectedNode))
                {
                    foreach (var cu in ChannelList)
                    {
                        //        if (cu.Value == items.SelectedNode)
                        {
                            //            if (cu.Key.ChannelWork)
                            {
                                //                cu.Key._Network._Protocol.Part(cu.Key.Name);
                                //                cu.Key.ChannelWork = false;
                                return;
                            }
                            return;
                        }
                    }
                }
            }
            catch (Exception fail)
            {
                Core.handleException(fail);
            }
        }

        public enum ItemType
        {
            Server,
            Services,
            System,
            Channel,
            User,
        }
    }
}

