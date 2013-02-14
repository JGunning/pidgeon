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
using System.ComponentModel;
using System.Threading;
using System.Text;
using System.IO;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;


namespace Client
{
    public partial class Scrollback
    {
        public void Click_R(string adds, string data)
        {
            try
            {
                if (data.StartsWith("http"))
                {
                    ViewLn(data, false, true);
                }
                if (data.StartsWith("pidgeon://text"))
                {
                    ViewLn(adds, false, true);
                }
                if (data.StartsWith("pidgeon://join"))
                {
                    ViewLn(data, true, false);
                }
                if (data.StartsWith("pidgeon://ident"))
                {
                    ViewLn("*!" + adds + "@*", false, false);
                }
                if (data.StartsWith("pidgeon://user"))
                {
                    if (this.owner.isChannel)
                    {
                        Channel channel = owner._Network.getChannel(owner.name);
                        if (channel != null)
                        {
                            User user = channel.userFromName(adds);
                            if (user != null)
                            {
                                if (user.Host != "")
                                {
                                    ViewLn("*@" + user.Host, false, false, true, adds);
                                    return;
                                }
                            }
                        }
                    }
                    ViewLn(adds + "!*@*", false, false, true, adds);
                }
                if (data.StartsWith("pidgeon://hostname"))
                {
                    ViewLn("*@" + adds, false, false);
                }
            }
            catch (Exception fail)
            {
                Core.handleException(fail);
            }
        }

        public void ViewLn(string content, bool channel, bool link = true, bool menu = false, string name = "")
        {
            if (owner != null && owner.isChannel)
            {
                toolStripMenuItem1.Visible = true;
                Link = content;
                kickToolStripMenuItem.Visible = false;
                whoisToolStripMenuItem.Visible = false;
                whowasToolStripMenuItem.Visible = false;
                toolStripMenuItem2.Visible = false;
                if (channel)
                {
                    mode1b2ToolStripMenuItem.Visible = false;
                    openLinkInBrowserToolStripMenuItem.Visible = false;
                    mode1e2ToolStripMenuItem.Visible = false;
                    mode1I2ToolStripMenuItem.Visible = false;
                    mode1q2ToolStripMenuItem.Visible = false;
                    copyLinkToClipboardToolStripMenuItem.Visible = false;
                    openLinkInBrowserToolStripMenuItem.Visible = false;
                    joinToolStripMenuItem.Visible = true;
                    return;
                }
                if (menu)
                {
                    toolStripMenuItem2.Visible = true;
                    kickToolStripMenuItem.Visible = true;
                    whoisToolStripMenuItem.Visible = true;
                    whowasToolStripMenuItem.Visible = true;
                }
                mode1b2ToolStripMenuItem.Visible = true;
                copyLinkToClipboardToolStripMenuItem.Visible = false;
                openLinkInBrowserToolStripMenuItem.Visible = false;
                joinToolStripMenuItem.Visible = false;
                toolStripMenuItem1.Visible = true;
                mode1q2ToolStripMenuItem.Text = "/mode " + owner.name + " +q " + content;
                mode1I2ToolStripMenuItem.Text = "/mode " + owner.name + " +I " + content;
                mode1e2ToolStripMenuItem.Text = "/mode " + owner.name + " +e " + content;
                mode1b2ToolStripMenuItem.Text = "/mode " + owner.name + " +b " + content;
                kickToolStripMenuItem.Text = "/raw KICK " + owner.name + " " + name + " :" + Configuration.DefaultReason;
                whoisToolStripMenuItem.Text = "/whois " + name;
                whowasToolStripMenuItem.Text = "/whowas " + name;
                mode1e2ToolStripMenuItem.Visible = true;
                mode1I2ToolStripMenuItem.Visible = true;
                mode1q2ToolStripMenuItem.Visible = true;
                if (link)
                {
                    mode1b2ToolStripMenuItem.Visible = false;
                    openLinkInBrowserToolStripMenuItem.Visible = false;
                    mode1e2ToolStripMenuItem.Visible = false;
                    mode1I2ToolStripMenuItem.Visible = false;
                    mode1q2ToolStripMenuItem.Visible = false;
                    copyLinkToClipboardToolStripMenuItem.Visible = true;
                    openLinkInBrowserToolStripMenuItem.Visible = true;
                }
            }
        }

        public void Click_L(string http)
        {
            {
                if (http.StartsWith("https://"))
                {
                    try
                    {
                        System.Diagnostics.Process.Start(http);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Unable to open " + http, "Link", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    }
                }
                if (http.StartsWith("http://"))
                {
                    try
                    {
                        System.Diagnostics.Process.Start(http);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Unable to open " + http, "Link", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    }
                }
                if (http.StartsWith("pidgeon://"))
                {
                    string command = http.Substring("pidgeon://".Length);
                    if (command.EndsWith("/"))
                    {
                        command = command.Substring(0, command.Length - 1);
                    }
                    if (command.StartsWith("user/#"))
                    {
                        string nick = command.Substring(6);
                        if (owner != null && owner._Network != null)
                        {
                            if (owner.isChannel)
                            {
                                owner.textbox.richTextBox1.AppendText(nick + ": ");
                                owner.textbox.Focus();
                            }
                        }
                        return;
                    }
                    if (command.StartsWith("join/#"))
                    {
                        Parser.parse("/join " + command.Substring(5));
                        return;
                    }
                    if (command.StartsWith("ident/#"))
                    {
                        string nick = command.Substring(7);
                        if (owner != null && owner._Network != null)
                        {
                            if (owner.isChannel)
                            {
                                owner.textbox.richTextBox1.AppendText(nick);
                                owner.textbox.Focus();
                            }
                        }
                        return;
                    }
                    if (command.StartsWith("hostname/#"))
                    {
                        string nick = command.Substring(10);
                        if (owner != null && owner._Network != null)
                        {
                            if (owner.isChannel)
                            {
                                owner.textbox.richTextBox1.AppendText(nick);
                                owner.textbox.Focus();
                            }
                        }
                        return;
                    }
                    if (command.StartsWith("text/#"))
                    {
                        string nick = command.Substring(6);
                        if (owner != null && owner._Network != null)
                        {
                            if (owner.isChannel)
                            {
                                owner.textbox.richTextBox1.AppendText(nick);
                                owner.textbox.Focus();
                            }
                        }
                        return;
                    }
                    if (command.StartsWith("join#"))
                    {
                        Parser.parse("/join " + command.Substring(4));
                        return;
                    }
                }
            }
        }
        private void channelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (Core.network != null)
                {
                    if (Core.network.RenderedChannel != null)
                    {
                        Channel_Info info = new Channel_Info();
                        info.channel = Core.network.RenderedChannel;
                        info.Show();
                    }
                }
            }
            catch (Exception fail)
            {
                Core.handleException(fail);
            }
        }

        private void mrhToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (MessageBox.Show("Do you really want to clear this window", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    lock (ContentLines)
                    {
                        ContentLines.Clear();
                        lastDate = DateTime.MinValue;
                        Reload();
                        Recreate(true);
                    }
                }
            }
            catch (Exception fail)
            {
                Core.handleException(fail);
            }
        }

        private void scrollToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                scrollToolStripMenuItem.Checked = !scrollToolStripMenuItem.Checked;
                ScrollingEnabled = scrollToolStripMenuItem.Checked;
            }
            catch (Exception fail)
            {
                Core.handleException(fail);
            }
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {

            }
            catch (Exception fail)
            {
                Core.handleException(fail);
            }
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Reload(true);
            }
            catch (Exception fail)
            {
                Core.handleException(fail);
            }
        }

        private void retrieveTopicToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (owner.isChannel)
                {
                    owner._Protocol.Transfer("TOPIC " + owner.name);
                }
            }
            catch (Exception fail)
            {
                Core.handleException(fail);
            }
        }

        private void toggleSimpleLayoutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Switch(false);
            }
            catch (Exception fail)
            {
                Core.handleException(fail);
            }
        }

        private void toggleAdvancedLayoutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Switch(true);
            }
            catch (Exception fail)
            {
                Core.handleException(fail);
            }
        }

        private void mode1b2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (owner != null)
                {
                    owner.textbox.richTextBox1.AppendText(mode1b2ToolStripMenuItem.Text);
                }
            }
            catch (Exception fail)
            {
                Core.handleException(fail);
            }
        }

        private void mode1q2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (owner != null)
                {
                    owner.textbox.richTextBox1.AppendText(mode1q2ToolStripMenuItem.Text);
                }
            }
            catch (Exception fail)
            {
                Core.handleException(fail);
            }
        }

        private void mode1I2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (owner != null)
                {
                    owner.textbox.richTextBox1.AppendText(mode1I2ToolStripMenuItem.Text);
                }
            }
            catch (Exception fail)
            {
                Core.handleException(fail);
            }
        }

        private void mode1e2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (owner != null)
                {
                    owner.textbox.richTextBox1.AppendText(mode1e2ToolStripMenuItem.Text);
                }
            }
            catch (Exception fail)
            {
                Core.handleException(fail);
            }
        }

        private void openLinkInBrowserToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (Link.StartsWith("http"))
                {
                    System.Diagnostics.Process.Start(Link);
                }
            }
            catch (Exception fail)
            {
                Core.handleException(fail);
            }
        }

        private void joinToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (Link.StartsWith("#"))
                {
                    Parser.parse("/join " + Link);
                }
            }
            catch (Exception fail)
            {
                Core.handleException(fail);
            }
        }

        private void whoisToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (owner != null)
                {
                    Parser.parse(whoisToolStripMenuItem.Text);
                }
            }
            catch (Exception fail)
            {
                Core.handleException(fail);
            }
        }

        private void whowasToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (owner != null)
                {
                    Parser.parse(whowasToolStripMenuItem.Text);
                }
            }
            catch (Exception fail)
            {
                Core.handleException(fail);
            }
        }

        private void kickToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (owner != null)
                {
                    if (Configuration.ConfirmAll)
                    {
                        if (MessageBox.Show(messages.get("window-confirm", Core.SelectedLanguage, new List<string> { "\n\n" + kickToolStripMenuItem.Text }), "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.No)
                        {
                            return;
                        }
                    }
                    Parser.parse(kickToolStripMenuItem.Text);
                }
            }
            catch (Exception fail)
            {
                Core.handleException(fail);
            }
        }

        private void copyTextToClipBoardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string text = null;
                if (simple)
                {
                    text = simpleview.Text;
                    Clipboard.SetText(text);
                    return;
                }
                text = RT.Text;
                if (text != null)
                {
                    Clipboard.SetText(RT.Text);
                }
            }
            catch (Exception fail)
            {
                Core.handleException(fail);
            }
        }

        private void copyEntireWindowToClipBoardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string text = "";
                lock (ContentLines)
                {
                    foreach (ContentLine _line in ContentLines)
                    {
                        text += Configuration.format_date.Replace("$1", _line.time.ToString(Configuration.timestamp_mask)) + Core.RemoveSpecial(_line.text) + "\n";
                    }
                }
                Clipboard.SetText(text);
            }
            catch (Exception fail)
            {
                Core.handleException(fail);
            }
        }

        private void listAllChannelsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (owner != null)
                {
                    Channels channels = new Channels(owner._Network);
                    channels.Show();
                }
            }
            catch (Exception fail)
            {
                Core.handleException(fail);
            }
        }

        private void copyLinkToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Clipboard.SetText(Link);
            }
            catch (Exception fail)
            {
                Core.handleException(fail);
            }
        }
    }
}