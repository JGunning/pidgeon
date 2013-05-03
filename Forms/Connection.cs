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
using Gtk;

namespace Client.Forms
{
    /// <summary>
    /// Creates a new form
    /// </summary>
    public partial class Connection : Gtk.Window
    {
        /// <summary>
        /// New connection gtk dialog
        /// </summary>
        public Connection () : base(Gtk.WindowType.Toplevel)
        {
            this.Build ();
            messages.Localize(this);
            this.entry4.Visibility = false;
            this.DeleteEvent += new DeleteEventHandler(Unshow);
            entry3.Text = "6667";
            entry2.Text = Configuration.UserData.ident;
            entry1.Text = Configuration.UserData.nick;
            combobox1.Clear();
            CellRendererText cell = new CellRendererText();
            combobox1.PackStart(cell, false);
            combobox1.AddAttribute(cell, "text", 0);
            ListStore store = new ListStore(typeof(string));
            combobox1.Model = store;
            ListStore store2 = new ListStore(typeof(string));
            comboboxentry1.Model = store2;
            TreeIter iter = store.AppendValues("irc");
            store.AppendValues("quassel");
            store.AppendValues("pidgeon services");
            store.AppendValues("dcc");
            combobox1.SetActiveIter(iter);
            button1.Clicked += new EventHandler(bConnect_Click);
            checkbutton1.Active = Configuration.UserData.LastSSL;
            if (Configuration.UserData.LastPort != "")
            {
                entry3.Text = Configuration.UserData.LastPort;
            }
            if (Configuration.UserData.LastNick != "")
            {
                entry1.Text = Configuration.UserData.LastNick;
            }
            if (Configuration.UserData.LastHost != "")
            {
                TreeIter iter2 = store2.AppendValues(Configuration.UserData.LastHost);
                this.comboboxentry1.SetActiveIter(iter2);
            }
            this.Title = messages.get("connection", Core.SelectedLanguage);
        }

        private void Unshow(object main, Gtk.DeleteEventArgs closing)
        {
            try
            {
                this.Hide();
                closing.RetVal = true;
            }
            catch (Exception fail)
            {
                Core.handleException(fail);
            }
        }

        private void bConnect_Click(object sender, EventArgs e)
        {
            try
            {
                int port = 6667;
                Configuration.UserData.LastSSL = checkbutton1.Active;
                if (entry2.Text == "")
                {
                    GTK.MessageBox.Show(this, MessageType.Warning, ButtonsType.Ok, messages.get("nconnection-2", Core.SelectedLanguage), "Missing params");
                    return;
                }
                if (entry3.Text == "" || !int.TryParse(entry3.Text, out port))
                {
                    GTK.MessageBox.Show(this, MessageType.Warning, ButtonsType.Ok, messages.get("nconnection-3", Core.SelectedLanguage), "Missing params");
                    return;
                }
                if (entry1.Text == "")
                {
                    GTK.MessageBox.Show(this, MessageType.Warning, ButtonsType.Ok, messages.get("nconnection-1", Core.SelectedLanguage), "Missing params");
                    return;
                }
                Configuration.UserData.nick = entry1.Text;
                Configuration.UserData.ident = entry2.Text;
                Configuration.UserData.LastHost = comboboxentry1.ActiveText;
                Configuration.UserData.LastPort = entry3.Text;
                Configuration.UserData.LastNick = entry1.Text;
                switch (combobox1.Active)
                {
                    case 0:
                        Core.connectIRC(comboboxentry1.ActiveText, port, entry4.Text, checkbutton1.Active);
                        break;
                    case 1:
                        Core.connectQl(comboboxentry1.ActiveText, port, entry4.Text, checkbutton1.Active);
                        break;
                    case 2:
                        Core.connectPS(comboboxentry1.ActiveText, port, entry4.Text, checkbutton1.Active);
                        break;
                }
                Hide();
            }
            catch (Exception fail)
            {
                Core.handleException(fail);
            }
        }
    }
}

