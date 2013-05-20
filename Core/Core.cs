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

using System.IO;
using System.Threading;
using System.Net;
using System.Windows.Forms;
using System.Xml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Client
{
    public partial class Core
    {
        /// <summary>
        /// Represent a domain for program
        /// </summary>
        public class Domain
        {
            /// <summary>
            /// Domain
            /// </summary>
            public AppDomain domain;
            /// <summary>
            /// Name
            /// </summary>
            public string name;

            /// <summary>
            /// Creates a new instance of this class
            /// </summary>
            /// <param name="_appDomain"></param>
            /// <param name="_name"></param>
            public Domain(AppDomain _appDomain, string _name)
            {
                domain = _appDomain;
                name = _name;
            }
        }

        /// <summary>
        /// Profiler
        /// </summary>
        public class Profiler
        {
            private Stopwatch time = new Stopwatch();
            /// <summary>
            /// Function that is being checked
            /// </summary>
            public string Function = null;

            /// <summary>
            /// Creates a new instance with name of function
            /// </summary>
            /// <param name="function"></param>
            public Profiler(string function)
            {
                Function = function;
                time.Start();
            }

            /// <summary>
            /// Called when profiler is supposed to be stopped
            /// </summary>
            public void Done()
            {
                time.Stop();
                if (time.ElapsedMilliseconds > Configuration.Kernel.Profiler_Minimal)
                {
                    Core.Ringlog("PROFILER: " + Function + ": " + time.ElapsedMilliseconds.ToString());
                }
            }
        }

        /// <summary>
        /// This represents a shortcut defined in pidgeon
        /// </summary>
        public class Shortcut
        {
            /// <summary>
            /// Control
            /// </summary>
            public bool control;
            /// <summary>
            /// Alt
            /// </summary>
            public bool alt;
            /// <summary>
            /// Shift
            /// </summary>
            public bool shift;
            /// <summary>
            /// Keycode
            /// </summary>
            public Gdk.Key keys;
            /// <summary>
            /// Execution command
            /// </summary>
            public string data;

            /// <summary>
            /// Creates a new instance of shortcut
            /// </summary>
            /// <param name="Value"></param>
            /// <param name="Control"></param>
            /// <param name="Alt"></param>
            /// <param name="Shift"></param>
            /// <param name="Data"></param>
            public Shortcut(Gdk.Key Value, bool Control = false, bool Alt = false, bool Shift = false, string Data = "")
            {
                control = Control;
                shift = Shift;
                alt = Alt;
                data = Data;
                keys = Value;
            }
        }

        private static Thread KernelThread = null;
        /// <summary>
        /// Thread of core
        /// </summary>
        public static Thread _KernelThread
        {
            get
            {
                return KernelThread;
            }
        }
        /// <summary>
        /// Exact time of system load
        /// </summary>
        public static DateTime LoadTime;
        /// <summary>
        /// Configuration path
        /// </summary>
        public static string ConfigFile = System.Windows.Forms.Application.LocalUserAppDataPath + Path.DirectorySeparatorChar + "configuration.dat";
        /// <summary>
        /// Root dir
        /// </summary>
        public static string Root = System.Windows.Forms.Application.LocalUserAppDataPath + Path.DirectorySeparatorChar;
        /// <summary>
        /// Language used in system
        /// </summary>
        public static string SelectedLanguage = "en";
        /// <summary>
        /// List of active networks in system
        /// </summary>
        public static List<IProtocol> Connections = new List<IProtocol>();
        /// <summary>
        /// Thread for IO logs
        /// </summary>
        public static Thread Thread_logs = null;
        /// <summary>
        /// Thread for update system
        /// </summary>
        public static Thread ThUp = null;
        /// <summary>
        /// Module layers
        /// </summary>
        public static List<Domain> domains = new List<Domain>();
        /// <summary>
        /// Pointer to exception class during recovery
        /// </summary>
        public static Exception recovery_exception = null;
        /// <summary>
        /// Recovery thread
        /// </summary>
        public static Thread _RecoveryThread = null;
        /// <summary>
        /// Timers
        /// </summary>
        public static List<Timer> TimerDB = new List<Timer>();
        /// <summary>
        /// Notification box
        /// </summary>
        private static Forms.Notification notification = null;
        /// <summary>
        /// Threads currently allocated in kernel
        /// </summary>
        public static List<Thread> SystemThreads = new List<Thread>();
        /// <summary>
        /// Ring log
        /// </summary>
        private static List<string> Ring = new List<string>();
        /// <summary>
        /// Selected network
        /// </summary>
        public static Network network = null;
        private static Forms.Main _main = null;
        /// <summary>
        /// Gets the system form
        /// </summary>
        /// <value>
        /// The system form.
        /// </value>
        public static Forms.Main SystemForm
        {
            get
            {
                return _main;
            }
        }
        /// <summary>
        /// Wheter notification is waiting
        /// </summary>
        public static bool notification_waiting = false;
        /// <summary>
        /// Data of notification (text)
        /// </summary>
        private static string notification_data = "";
        /// <summary>
        /// Caption
        /// </summary>
        private static string notification_caption = "";
        /// <summary>
        /// This is index of last random number, you should never write or read this value, except for its own function
        /// </summary>
        private static int randomuq = 0;
        /// <summary>
        /// Path to skin
        /// </summary>
        public static string SkinPath = "skins";
        /// <summary>
        /// Packet scan
        /// </summary>
        public static Forms.TrafficScanner trafficscanner;
        /// <summary>
        /// System is blocked - if this is set to true, all subsystems and kernel are supposed to freeze
        /// </summary>
        public static bool blocked = false;
        /// <summary>
        /// Ignore errors - all exceptions and debug logs are ignored and pidgeon is flagged as unstable
        /// </summary>
        public static bool IgnoreErrors = false;
        /// <summary>
        /// If this is true the recovery window will not allow to ignore
        /// </summary>
        public static bool recovery_fatal = false;
        /// <summary>
        /// Parameters that were retrieved in console (deprecated)
        /// </summary>
        public static string[] startup = null;
        /// <summary>
        /// Cache of current params
        /// </summary>
        private static List<string> startup_params = new List<string>();
        /// <summary>
        /// Extensions loaded in core
        /// </summary>
        public static List<Extension> Extensions = new List<Extension>();
        /// <summary>
        /// Status of core
        /// </summary>
        public static Status _Status = Status.Running;
        private static DateTime lastActivityTime;
        /// <summary>
        /// Parameters that were retrieved in console
        /// </summary>
        public static List<string> Parameters
        {
            get
            {
                List<string> data = new List<string>();
                lock (startup)
                {
                    data.AddRange(startup_params);
                }
                return data;
            }
        }

        /// <summary>
        /// Time for which the user is idle
        /// </summary>
        public static TimeSpan IdleTime
        {
            get
            {
                return DateTime.Now - lastActivityTime;
            }
        }

        /// <summary>
        /// Location of temporary stuff
        /// </summary>
        public static string PermanentTemp
        {
            get
            {
                return Root + "Temp" + System.IO.Path.DirectorySeparatorChar;
            }
        }

        /// <summary>
        /// Retrieve a read only copy of ring
        /// </summary>
        public static List<string> RingBuffer
        {
            get
            {
                List<string> rb = new List<string>();
                lock (Ring)
                {
                    rb.AddRange(Ring);
                }
                return rb;
            }
        }
  
        /// <summary>
        /// Sets the main
        /// </summary>
        /// <param name='form'>
        /// Form.
        /// </param>
        public static void setMain(Forms.Main form)
        {
            _main = form;
        }
        
        /// <summary>
        /// Load
        /// </summary>
        /// <returns></returns>
        public static bool Load()
        {
            try
            {
                KernelThread = System.Threading.Thread.CurrentThread;
                LoadTime = DateTime.Now;
                // turn on debugging until we load the config
                Configuration.Kernel.Debugging = true;
                Ringlog("Pidgeon " + Application.ProductVersion.ToString() + " loading core");
                foreach (string data in startup)
                {
                    startup_params.Add(data);
                }
                if (Application.LocalUserAppDataPath.EndsWith(Application.ProductVersion))
                {
                    Root = Application.LocalUserAppDataPath.Substring(0, Application.LocalUserAppDataPath.Length - Application.ProductVersion.Length);
                    ConfigFile = Application.LocalUserAppDataPath.Substring(0, Application.LocalUserAppDataPath.Length - Application.ProductVersion.Length) + "configuration.dat";
                }
                Configuration.Logs.logs_dir = Root + "logs";
                Ringlog("Root path is " + Root);
                Ringlog("Config file: " + ConfigFile);
                string is64 = " which is a 32 bit system";
                if (Environment.Is64BitOperatingSystem)
                {
                    is64 = " which is a 64 bit system";
                }
                Ringlog("This pidgeon is compiled for " + Configuration.CurrentPlatform.ToString() + " and running on " + Environment.OSVersion.ToString() + is64);
                DebugLog("Loading messages");
                messages.Read();
                trafficscanner = new Forms.TrafficScanner();
                if (!System.IO.File.Exists(Application.StartupPath + System.IO.Path.DirectorySeparatorChar + "pidgeon.dat"))
                {
                    LoadSkin();
                    DebugLog("Loading configuration file");
                    Configuration.Kernel.Debugging = false;
                    Core._Configuration.ConfigurationLoad();
                    if (!Directory.Exists(PermanentTemp))
                    {
                        Directory.CreateDirectory(PermanentTemp);
                    }
                    DebugLog("Running updater");
                    ThUp = new Thread(Updater.Run);
                    ThUp.Name = "pidgeon service";
                    ThUp.Start();
                    SystemThreads.Add(ThUp);
                    DebugLog("Loading log writer thread");
                    Thread_logs = new Thread(IO.Load);
                    Thread_logs.Name = "Logs";
                    SystemThreads.Add(Thread_logs);
                    Thread_logs.Start();
                    DebugLog("Loading commands");
                    Commands.Initialise();
                    notification = new Forms.Notification();
                    //DebugLog("Loading scripting core");
                    //ScriptingCore.Load();
                    DebugLog("Loading extensions");
                    Extension.Init();
                    DebugLog("Loading http");
                    Hyperlink.Initialize();
                    if (Directory.Exists(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + Path.DirectorySeparatorChar + "modules"))
                    {
                        foreach (string dll in Directory.GetFiles(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + Path.DirectorySeparatorChar + "modules", "*.pmod"))
                        {
                            DebugLog("Registering plugin " + dll);
                            RegisterPlugin(dll);
                        }
                    }
                    if (!File.Exists(ConfigFile))
                    {
                        Network.Highlighter simple = new Network.Highlighter();
                        simple.enabled = true;
                        simple.text = "$nick";
                        Configuration.HighlighterList.Add(simple);
                    }
                    ResetMainActivityTimer();
                    Hooks._Sys.AfterCore();
                    return true;
                }
                Updater _finalisingupdater = new Updater();
                _finalisingupdater.update.Visible = false;
                _finalisingupdater.finalize = true;
                _finalisingupdater.lStatus.Text = messages.get("update2");
                System.Windows.Forms.Application.Run(_finalisingupdater);
            }
            catch (Exception panic)
            {
                Core.DebugLog("Failed to Core.Load: " + panic.Message + panic.StackTrace);
            }
            return false;
        }

        /// <summary>
        /// This will reset the activity timer
        /// </summary>
        public static void ResetMainActivityTimer()
        {
            lastActivityTime = DateTime.Now;
        }

        /// <summary>
        /// Connect to irc link
        /// </summary>
        /// <param name="text">link</param>
        /// <param name="services"></param>
        public static void ParseLink(string text, ProtocolSv services = null)
        {
            if (text.StartsWith("irc://"))
            {
                text = text.Substring("irc://".Length);
                string network = text;
                string channel = null;
                bool ssl = false;
                int PORT = 6667;
                if (network.StartsWith("$"))
                {
                    network = network.Substring(1);
                    ssl = true;
                }
                if (network.Contains("#"))
                {
                    channel = network.Substring(network.IndexOf("#"));
                }
                network = network.Substring(0, network.IndexOf("#"));
                if (network.Contains(":"))
                {
                    string port = network.Substring(network.IndexOf(":") + 1);
                    network = network.Substring(0, network.IndexOf(port));
                    if (port.Contains("/"))
                    {
                        port = port.Substring(0, port.IndexOf("/"));
                    }
                    if (port.Contains("#"))
                    {
                        port = port.Substring(0, port.IndexOf("#"));
                    }
                    if (!int.TryParse(port, out PORT))
                    {
                        PORT = 6667;
                    }
                }
                while (network.EndsWith("/"))
                {
                    network.Substring(0, network.Length - 1);
                }
                ProtocolIrc server = null;
                foreach (Protocol protocol in Connections)
                {
                    if (typeof(ProtocolIrc) == protocol.GetType() && protocol.Server == network)
                    {
                        server = (ProtocolIrc)protocol;
                        break;
                    }
                }

                if (server == null)
                {
                    server = connectIRC(network, PORT, "", ssl);
                }

                if (channel != null)
                {
                    server._IRCNetwork.Join(channel);
                }
            }
        }

        /// <summary>
        /// Recover from crash
        /// </summary>
        public static void Recover()
        {
            Client.Recovery x = new Client.Recovery();
            System.Windows.Forms.Application.Run(x);
        }

        /// <summary>
        /// Kill thread or only remove it from system thread table
        /// </summary>
        /// <param name="name"></param>
        /// <param name="remove"></param>
        public static void killThread(Thread name, bool remove = false)
        {
            if (name == null)
            {
                return;
            }
            // we need to lock it here so that we prevent multiple calls of this function at same time
            lock (SystemThreads)
            {
                if (name != Thread.CurrentThread)
                {
                    if (!remove && (name.ThreadState == System.Threading.ThreadState.Running || name.ThreadState == System.Threading.ThreadState.WaitSleepJoin))
                    {
                        name.Abort();
                        Core.DebugLog("Killed thread " + name.Name);
                    }
                    else
                    {
                        Core.DebugLog("Ignored request to abort thread in " + name.ThreadState.ToString() + name.Name);
                    }
                }
                else
                {
                    Core.DebugLog("Ignored request to abort thread from within the same thread " + name.Name);
                }

                if (Core.IgnoreErrors)
                {
                    DebugLog("Not removing thread from thread queue " + name.Name + " because system is shutting down");
                    return;
                }
                if (SystemThreads.Contains(name))
                {
                    SystemThreads.Remove(name);
                }
            }
        }

        /// <summary>
        /// Remove special symbols
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string RemoveSpecial(string text)
        {
            return text.Replace("%/USER%", "")
                .Replace("%USER%", "")
                .Replace("%H%", "")
                .Replace("%/H%", "")
                .Replace("%D%", "")
                .Replace("%L%", "")
                .Replace("%/L%", "")
                .Replace("%/D%", "")
                .Replace(((char)001).ToString(), "")
                .Replace(((char)002).ToString(), "")
                .Replace(((char)003).ToString(), "")
                .Replace(((char)004).ToString(), "");
        }

        /// <summary>
        /// Remove the ring data
        /// </summary>
        public static void ClearRingBufferLog()
        {
            lock (Ring)
            {
                Ring.Clear();
            }
        }

        /// <summary>
        /// Insert a text to log
        /// </summary>
        /// <param name="data"></param>
        public static void Ringlog(string data)
        {
            string text = "[" + DateTime.Now.ToString() + "] " + data;
            lock (Ring)
            {
                Ring.Add(text);
                while (Ring.Count > Configuration.Kernel.MaximalRingLogSize)
                {
                    Ring.RemoveAt(0);
                }
            }
            if (Configuration.Kernel.Debugging)
            {
                Console.WriteLine(text);
            }
        }

        /// <summary>
        /// Insert text in to debug log
        /// </summary>
        /// <param name="data"></param>
        /// <param name="verbosity"></param>
        public static void DebugLog(string data, int verbosity)
        {
            try
            {
                System.Diagnostics.Debug.Print(data);
                if (Configuration.Kernel.Debugging)
                {
                    if (Core.SystemForm != null && !Core.blocked)
                    {
                        if (Core.SystemForm.main != null)
                        {
                            Core.SystemForm.main.scrollback.InsertText("DEBUG: " + data, Client.ContentLine.MessageStyle.System, false);
                        }
                    }
                }
                Ringlog("DEBUG: " + data);
            }
            catch (Exception er)
            {
                Core.handleException(er);
            }
        }

        /// <summary>
        /// Insert text in to debug log
        /// </summary>
        /// <param name="data">Text to insert</param>
        public static void DebugLog(string data)
        {
            DebugLog(data, 1);
        }

        /// <summary>
        /// Draw all text into a scrollback
        /// </summary>
        /// <param name="window">Window to which it should be printed</param>
        /// <param name="write">Whether it should be written to file</param>
        public static void PrintRing(Graphics.Window window, bool write = true)
        {
            lock (Ring)
            {
                foreach (string item in Ring)
                {
                    window.scrollback.InsertText(item, Client.ContentLine.MessageStyle.System, write, 0, true);
                }
            }
        }

        /// <summary>
        /// Parse a command from input box
        /// </summary>
        /// <param name="command">Command, example /connect</param>
        /// <returns></returns>
        public static bool ProcessCommand(string command)
        {
            try
            {
                if (command.StartsWith(Configuration.CommandPrefix))
                {
                    command = command.Substring(1);
                }

                // in case that it's known command we return
                if (Commands.Process(command))
                {
                    return true;
                }

                // if not we can try to pass it to server
                if (Core.SystemForm.Chat != null && !Core.SystemForm.Chat.IsDestroyed && Core.SystemForm.Chat._Network != null && Core.SystemForm.Chat._Network._Protocol != null)
                {
                    if (SystemForm.Chat._Network._Protocol.IsConnected)
                    {
                        Core.SystemForm.Chat._Network._Protocol.Command(command);
                        return false;
                    }
                }
                SystemForm.Chat.scrollback.InsertText(messages.get("invalid-command", SelectedLanguage), Client.ContentLine.MessageStyle.System);
                return false;
            }
            catch (Exception f)
            {
                handleException(f);
            }
            return false;
        }

        /// <summary>
        /// Restore a file
        /// </summary>
        /// <param name="file">File path</param>
        /// <returns></returns>
        public static bool Restore(string file)
        {
            if (!File.Exists(file + "~"))
            {
                return false;
            }
            string backup = System.IO.Path.GetRandomFileName();
            DebugLog("Restoring file " + file + " from a backup");
            if (File.Exists(file))
            {
                File.Copy(file, backup);
                DebugLog("Stored previous version: " + backup);
            }
            File.Copy(file + "~", file, true);
            return true;
        }

        /// <summary>
        /// Finalize displaying of notice box or show the last displayed box
        /// </summary>
        public static void DisplayNote()
        {
            if (Configuration.Kernel.Notice == false)
            {
                return;
            }
            if (Configuration.Media.NotificationSound)
            {
                System.Media.SystemSounds.Asterisk.Play();
            }
            if (_KernelThread == Thread.CurrentThread)
            {
                if (notification_waiting)
                {
                    bool Focus = false;
                    notification.text.Text = notification_data;
                    notification.title.Markup = "<span size='18000'>" + notification_caption + "</span>";
                    notification_waiting = false;
                    if (Core.SystemForm.Chat != null)
                    {
                        if (Core.SystemForm.Chat.textbox.richTextBox1.IsFocus)
                        {
                            Focus = true;
                        }
                    }
                    if (!notification.Visible)
                    {
                        notification.Relocate();
                        notification.Show();
                        if (Focus)
                        {
                            Core.SystemForm.setFocus();
                            if (Core.SystemForm.Chat != null)
                            {
                                notification.focus = true;
                                Core.SystemForm.Chat.textbox.setFocus();
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Show a notice box, if started from non kernel thread, then it's finalized in that
        /// </summary>
        /// <param name="data">Information</param>
        /// <param name="caption">Text</param>
        public static void DisplayNote(string data, string caption)
        {
            if (Configuration.Kernel.Notice == false)
            {
                return;
            }
            data = Protocol.decode_text(Core.RemoveSpecial(data));
            notification_waiting = true;
            notification_data = data;
            notification_caption = caption;
            if (_KernelThread == Thread.CurrentThread)
            {
                DisplayNote();
            }
        }

        /// <summary>
        /// Backup a file
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static bool Backup(string file)
        {
            if (File.Exists(file + "~"))
            {
                string backup = System.IO.Path.GetRandomFileName();
                File.Copy(file + "~", backup);
            }
            if (File.Exists(file))
            {
                File.Copy(file, file + "~", true);
            }
            return true;
        }

        /// <summary>
        /// Get a random unique string (it's not possible to get 2 same strings from this function)
        /// </summary>
        /// <returns></returns>
        public static string retrieveRandom()
        {
            int random = 0;
            bool lockWasTaken = false;
            try
            {
                Monitor.Enter(_KernelThread, ref lockWasTaken);
                randomuq++;
                random = randomuq;
            }
            finally
            {
                if (lockWasTaken)
                {
                    Monitor.Exit(_KernelThread);
                }
            }
            return ":" + random.ToString() + "*";
        }

        /// <summary>
        /// Editor
        /// </summary>
        /// <param name="script"></param>
        /// <param name="target"></param>
        public static void ProcessScript(string script, Network target)
        {
            try
            {
                Forms.ScriptEdit edit = new Forms.ScriptEdit();
                edit.textBox1.Buffer.Text += script;
                edit.network = target;
                edit.Show();
            }
            catch (Exception fail)
            {
                Core.handleException(fail);
            }
        }

        /// <summary>
        /// Remove a special symbols from html
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string normalizeHtml(string text)
        {
            return System.Web.HttpUtility.HtmlEncode(text);
        }

        /// <summary>
        /// Convert a key to upper case
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static Gdk.Key convertKey(Gdk.Key key)
        {
            switch (key)
            {
                case Gdk.Key.a:
                    return Gdk.Key.A;
                case Gdk.Key.b:
                    return Gdk.Key.B;
                case Gdk.Key.c:
                    return Gdk.Key.C;
                case Gdk.Key.d:
                    return Gdk.Key.D;
                case Gdk.Key.e:
                    return Gdk.Key.E;
                case Gdk.Key.f:
                    return Gdk.Key.F;
                case Gdk.Key.g:
                    return Gdk.Key.G;
                case Gdk.Key.h:
                    return Gdk.Key.H;
                case Gdk.Key.i:
                    return Gdk.Key.I;
                case Gdk.Key.j:
                    return Gdk.Key.J;
                case Gdk.Key.k:
                    return Gdk.Key.K;
                case Gdk.Key.l:
                    return Gdk.Key.L;
                case Gdk.Key.m:
                    return Gdk.Key.M;
                case Gdk.Key.n:
                    return Gdk.Key.N;
                case Gdk.Key.o:
                    return Gdk.Key.O;
                case Gdk.Key.p:
                    return Gdk.Key.P;
                case Gdk.Key.q:
                    return Gdk.Key.Q;
                case Gdk.Key.r:
                    return Gdk.Key.R;
                case Gdk.Key.s:
                    return Gdk.Key.S;
                case Gdk.Key.t:
                    return Gdk.Key.T;
                case Gdk.Key.u:
                    return Gdk.Key.U;
                case Gdk.Key.v:
                    return Gdk.Key.V;
                case Gdk.Key.w:
                    return Gdk.Key.W;
                case Gdk.Key.x:
                    return Gdk.Key.X;
                case Gdk.Key.y:
                    return Gdk.Key.Y;
                case Gdk.Key.z:
                    return Gdk.Key.Z;
            }

            return key;
        }

        /// <summary>
        /// Convert string to key
        /// </summary>
        /// <param name="Key">Key</param>
        /// <returns></returns>
        public static Gdk.Key parseKey(string Key)
        {
            switch (Key.ToLower())
            {
                case "a":
                    return Gdk.Key.A;
                case "b":
                    return Gdk.Key.B;
                case "c":
                    return Gdk.Key.C;
                case "d":
                    return Gdk.Key.D;
                case "e":
                    return Gdk.Key.E;
                case "f":
                    return Gdk.Key.F;
                case "g":
                    return Gdk.Key.G;
                case "h":
                    return Gdk.Key.H;
                case "i":
                    return Gdk.Key.I;
                case "j":
                    return Gdk.Key.J;
                case "k":
                    return Gdk.Key.K;
                case "l":
                    return Gdk.Key.L;
                case "m":
                    return Gdk.Key.M;
                case "n":
                    return Gdk.Key.N;
                case "o":
                    return Gdk.Key.O;
                case "p":
                    return Gdk.Key.P;
                case "q":
                    return Gdk.Key.Q;
                case "r":
                    return Gdk.Key.R;
                case "s":
                    return Gdk.Key.S;
                case "t":
                    return Gdk.Key.T;
                case "u":
                    return Gdk.Key.U;
                case "v":
                    return Gdk.Key.V;
                case "x":
                    return Gdk.Key.X;
                case "w":
                    return Gdk.Key.W;
                case "y":
                    return Gdk.Key.Y;
                case "z":
                    return Gdk.Key.Z;
                case "1":
                    return Gdk.Key.Key_1;
                case "2":
                    return Gdk.Key.Key_2;
                case "3":
                    return Gdk.Key.Key_3;
                case "4":
                    return Gdk.Key.Key_4;
                case "5":
                    return Gdk.Key.Key_5;
                case "6":
                    return Gdk.Key.Key_6;
                case "7":
                    return Gdk.Key.Key_7;
                case "8":
                    return Gdk.Key.Key_8;
                case "9":
                    return Gdk.Key.Key_9;
                case "0":
                    return Gdk.Key.Key_0;
                case "f1":
                    return Gdk.Key.F1;
                case "f2":
                    return Gdk.Key.F2;
                case "f3":
                    return Gdk.Key.F3;
                case "home":
                    return Gdk.Key.Home;
                case "end":
                    return Gdk.Key.End;
                case "pageup":
                    return Gdk.Key.Page_Up;
                case "delete":
                    return Gdk.Key.Delete;
            }
            return Gdk.Key.A;
        }

        /// <summary>
        /// Load skin
        /// </summary>
        /// <returns></returns>
        public static bool LoadSkin()
        {
            Configuration.SL.Clear();
            Configuration.SL.Add(new Skin());
            DebugLog("Loading skins");
            if (Directory.Exists(System.Windows.Forms.Application.StartupPath + Path.DirectorySeparatorChar + SkinPath))
            {
                string[] skin = Directory.GetFiles(System.Windows.Forms.Application.StartupPath + Path.DirectorySeparatorChar + SkinPath);
                {
                    foreach (string file in skin)
                    {
                        if (file.EndsWith(".ps"))
                        {
                            Skin curr = new Skin(System.Windows.Forms.Application.StartupPath + Path.DirectorySeparatorChar + SkinPath
                                + Path.DirectorySeparatorChar + file);
                            if (curr == null)
                            {
                                continue;
                            }
                            Configuration.SL.Add(curr);
                        }
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Connect to XMPP
        /// </summary>
        /// <param name="server">Hostname</param>
        /// <param name="port">Port</param>
        /// <param name="password">Password</param>
        /// <param name="secured">SSL</param>
        /// <returns></returns>
        public static bool connectXmpp(string server, int port, string password, bool secured = false)
        {
            ProtocolXmpp IM = new ProtocolXmpp();
            IM.Open();
            return false;
        }

        /// <summary>
        /// Connect to dcc
        /// </summary>
        /// <param name="server">Hostname</param>
        /// <param name="port">Port</param>
        /// <param name="password">Password</param>
        /// <param name="secured">SSL</param>
        /// <returns></returns>
        public static bool connectDcc(string server, int port, string password, bool secured = false)
        {
            ProtocolDCC IM = new ProtocolDCC();
            IM.Server = server;
            IM.Port = port;
            IM.SSL = secured;
            IM.Open();
            Connections.Add(IM);
            return false;
        }

        /// <summary>
        /// Connect to quassel
        /// </summary>
        /// <param name="server">Hostname</param>
        /// <param name="port">Port</param>
        /// <param name="password">Password</param>
        /// <param name="secured">SSL</param>
        /// <returns></returns>
        public static bool connectQl(string server, int port, string password = "xx", bool secured = false)
        {
            ProtocolQuassel _quassel = new ProtocolQuassel();
            _quassel.Port = port;
            _quassel.password = password;
            _quassel.Server = server;
            _quassel.SSL = secured;
            _quassel.Open();
            Connections.Add(_quassel);
            return false;
        }

        /// <summary>
        /// Connect to pidgeon server
        /// </summary>
        /// <param name="server">Hostname</param>
        /// <param name="port">Port</param>
        /// <param name="password">Password</param>
        /// <param name="secured">SSL</param>
        /// <returns></returns>
        public static ProtocolSv connectPS(string server, int port = 8222, string password = "xx", bool secured = false)
        {
            ProtocolSv protocol = new ProtocolSv();
            protocol.Server = server;
            protocol.nick = Configuration.UserData.nick;
            protocol.Port = port;
            protocol.SSL = secured;
            protocol.password = password;
            Connections.Add(protocol);
            protocol.Open();
            return protocol;
        }

        /// <summary>
        /// Connect to IRC
        /// </summary>
        /// <param name="server">Hostname</param>
        /// <param name="port">Port</param>
        /// <param name="password">Password</param>
        /// <param name="secured">SSL</param>
        /// <returns></returns>
        public static ProtocolIrc connectIRC(string server, int port = 6667, string password = "", bool secured = false)
        {
            ProtocolIrc protocol = new ProtocolIrc();
            Connections.Add(protocol);
            protocol.Server = server;
            protocol.Port = port;
            protocol.Password = password;
            protocol.SSL = secured;
            protocol._IRCNetwork = new Network(server, protocol);
            network = protocol._IRCNetwork;
            protocol._IRCNetwork._Protocol = protocol;
            protocol.Open();
            return protocol;
        }

        /// <summary>
        /// Convert the xml collection to dictionary
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static Dictionary<string, string> XmlCollectionToDict(XmlAttributeCollection collection)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            int curr = 0;
            while (curr < collection.Count)
            {
                if (!data.ContainsKey(collection[curr].Name))
                {
                    data.Add(collection[curr].Name, collection[curr].Value);
                }
                curr++;
            }
            return data;
        }

        /// <summary>
        /// See the comments in recovery.cs for explanation how this work
        /// </summary>
        /// <param name="_exception"></param>
        /// <param name="ek"></param>
        /// <returns></returns>
        public static int handleException(Exception _exception, ExceptionKind ek)
        {
            if (IgnoreErrors || ek == ExceptionKind.Safe)
            {
                Console.WriteLine("EXCEPTION: " + _exception.StackTrace);
                return 2;
            }
            if (ek == ExceptionKind.Critical)
            {
                recovery_fatal = true;
            }
            DebugLog(_exception.Message + " at " + _exception.Source + " info: " + _exception.Data.ToString());
            blocked = true;
            recovery_exception = _exception;
            _RecoveryThread = new Thread(Recover);
            _RecoveryThread.Start();
            if (Thread.CurrentThread != _KernelThread)
            {
                DebugLog("Warning, the thread which raised the exception is not a core thread, identifier: " + Thread.CurrentThread.Name);
            }
            while (blocked || recovery_fatal)
            {
                Thread.Sleep(100);
            }
            return 0;
        }

        /// <summary>
        /// Recover from exception
        /// </summary>
        /// <param name="_exception"></param>
        /// <param name="fatal"></param>
        /// <returns></returns>
        public static int handleException(Exception _exception, bool fatal = false)
        {
            if (fatal)
            {
                handleException(_exception, ExceptionKind.Critical);
            }
            else
            {
                handleException(_exception, ExceptionKind.Normal);
            }
            return 0;
        }

        /// <summary>
        /// Exit
        /// </summary>
        /// <returns>true</returns>
        public static bool Quit()
        {
            try
            {
                if (_Status == Status.Quiting)
                {
                    Core.DebugLog("Multiple calls of Core.Quit() ignored");
                    return false;
                }
                _Status = Status.Quiting;
                if (!IgnoreErrors)
                {
                    IgnoreErrors = true;
                    if (Configuration.UserData.TrayIcon)
                    {
                        SystemForm.icon.Visible = false;
                        SystemForm.icon.Dispose();
                    }
                    SystemForm.Hide();
                    notification.Hide();
                    _Configuration.ConfigSave();
                    try
                    {
                        foreach (Protocol server in Connections)
                        {
                            Core.Ringlog("Exiting network " + server.Server);
                            server.Exit();
                        }
                    }
                    catch (Exception fail)
                    {
                        Core.handleException(fail);
                    }
                    foreach (Thread th in SystemThreads)
                    {
                        try
                        {
                            Core.Ringlog("CORE: Thread " + th.ManagedThreadId.ToString() + " needs to be terminated now");
                            Core.killThread(th);
                        }
                        catch (Exception fail)
                        {
                            Core.handleException(fail);
                        }
                    }
                    Thread.Sleep(800);
                    Core.DebugLog("Exiting with code 0");
                    Environment.Exit(0);
                }
            }
            catch (Exception fail)
            {
                Console.WriteLine(fail.StackTrace.ToString());
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }
            return true;
        }

        /// <summary>
        /// Type of exception
        /// </summary>
        public enum ExceptionKind
        {
            /// <summary>
            /// Normal
            /// </summary>
            Normal,
            /// <summary>
            /// Critical
            /// </summary>
            Critical,
            /// <summary>
            /// Safe
            /// </summary>
            Safe,
        }

        /// <summary>
        /// Platform
        /// </summary>
        public enum Platform
        {
            /// <summary>
            /// Linux
            /// </summary>
            Linuxx64,
            /// <summary>
            /// Linux
            /// </summary>
            Linuxx86,
            /// <summary>
            /// Windows
            /// </summary>
            Windowsx64,
            /// <summary>
            /// Windows
            /// </summary>
            Windowsx86,
            /// <summary>
            /// Mac OS
            /// </summary>
            MacOSx64,
            /// <summary>
            /// Max OS
            /// </summary>
            MacOSx86,
        }

        /// <summary>
        /// Which status the core is in
        /// </summary>
        public enum Status
        {
            /// <summary>
            /// Everything is OK
            /// </summary>
            Running,
            /// <summary>
            /// There is exception being handled
            /// </summary>
            Frozen,
            /// <summary>
            /// Terminating
            /// </summary>
            Quiting,
        }
    }

    /// <summary>
    /// This is an attribute that should be used for all new functions
    /// </summary>
    [Obsolete("This function is experimental and using it may result in errors")]
    public class Experimental : Attribute
    { }
}
