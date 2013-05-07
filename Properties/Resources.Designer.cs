﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.296
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Client.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Client.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        internal static System.Drawing.Bitmap Arrow {
            get {
                object obj = ResourceManager.GetObject("Arrow", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap At {
            get {
                object obj = ResourceManager.GetObject("At", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This command will connect you to a server, when you are using pidgeon services
        ///
        ///Example: /connect irc.tm-irc.org
        ///
        ///Synopsys: /connect [$]hostname[:port]
        ///
        ///Parameters:
        ///
        ///- $ prefix - when used the connection will use SSL
        ///- port - Specifies the port to use for this connection
        ///
        ///This manual page was written by Petr Bena &lt;benapetr@gmail.com&gt;.
        /// </summary>
        internal static string Connect {
            get {
                return ResourceManager.GetString("Connect", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to window-menu-file=File;
        ///window-menu-quit=Exit;
        ///window-menu-conf=Nastaveni;
        ///window-menu-help=Napoveda;
        ///pidgeon-shut=Do you really want to exit?;
        ///invalid-server=This is not a valid server name;
        ///invalid-command=Invalid command;
        ///loading-server=Connecting to $1;
        ///nick-e1=This is not a valid nick;
        ///nick=Your nick has been set;
        ///invalid-channel=This is not a valid channel;
        ///error1=You need to be connected;
        ///join=$1 just joined the channel;
        ///left=The $1 just left the channel;
        ///topic-data=Topic was set at $2 b [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string cs_czech {
            get {
                return ResourceManager.GetString("cs_czech", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to window-menu-file=File;
        ///window-menu-quit=Exit;
        ///window-menu-conf=Preferences;
        ///window-menu-help=Help;
        ///
        ///pidgeon-shut=Do you really want to exit?;
        ///invalid-server=This is not a valid server name;
        ///invalid-command=Invalid command;
        ///loading-server=Connecting to $1;
        ///nick-e1=This is not a valid nick;
        ///nick=Your nick has been set;
        ///invalid-channel=This is not a valid channel;
        ///error1=You need to be connected;
        ///join=$1 just joined the channel;
        ///left=The $1 just left the channel;
        ///topic-data=Topic was set at $2 b [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string en_english {
            get {
                return ResourceManager.GetString("en_english", resourceCulture);
            }
        }
        
        internal static System.Drawing.Bitmap Exclamation_mark {
            get {
                object obj = ResourceManager.GetObject("Exclamation_mark", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap Hash {
            get {
                object obj = ResourceManager.GetObject("Hash", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap icon {
            get {
                object obj = ResourceManager.GetObject("icon", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap Image1 {
            get {
                object obj = ResourceManager.GetObject("Image1", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This command will give you oper flag on servers that support it
        ///
        ///Example: /oper username pw
        ///
        ///Synopsys: /oper username [pw]
        ///
        ///Parameters:
        ///
        ///- username - your username
        ///- pw - password
        ///
        ///This manual page was written by Petr Bena &lt;benapetr@gmail.com&gt;.
        /// </summary>
        internal static string Oper {
            get {
                return ResourceManager.GetString("Oper", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This command will display a manual page
        ///
        ///Example: /pidgeon.man connect
        ///
        ///Synopsys: /pidgeon.man page
        ///
        ///Parameters:
        ///
        ///- page - Manual page to display
        ///
        ///This manual page was written by Petr Bena &lt;benapetr@gmail.com&gt;.
        /// </summary>
        internal static string PidgeonMan {
            get {
                return ResourceManager.GetString("PidgeonMan", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This command load module to kernel
        ///
        ///Example: /pidgeon.module modules/pidgeon_tc.pmod
        ///
        ///Synopsys: /pidgeon.module module
        ///
        ///Parameters:
        ///
        ///- module
        ///
        ///This manual page was written by Petr Bena &lt;benapetr@gmail.com&gt;.
        /// </summary>
        internal static string PidgeonModule {
            get {
                return ResourceManager.GetString("PidgeonModule", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This command display up time
        ///
        ///Example: /pidgeon.uptime
        ///
        ///Synopsys: /pidgeon.uptime
        ///
        ///This manual page was written by Petr Bena &lt;benapetr@gmail.com.
        /// </summary>
        internal static string PidgeonUptime {
            get {
                return ResourceManager.GetString("PidgeonUptime", resourceCulture);
            }
        }
        
        internal static System.Drawing.Bitmap Pigeon_clip_art_hight {
            get {
                object obj = ResourceManager.GetObject("Pigeon_clip_art_hight", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Icon Pigeon_clip_art_hight1 {
            get {
                object obj = ResourceManager.GetObject("Pigeon_clip_art_hight1", resourceCulture);
                return ((System.Drawing.Icon)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This command will connect you to a server
        ///
        ///Example: /server irc.tm-irc.org
        ///
        ///Synopsys: /server [$]hostname[:port]
        ///
        ///Parameters:
        ///
        ///- $ prefix - when used the connection will use SSL
        ///- port - Specifies the port to use for this connection
        ///
        ///This manual page was written by Petr Bena &lt;benapetr@gmail.com&gt;.
        /// </summary>
        internal static string Server {
            get {
                return ResourceManager.GetString("Server", resourceCulture);
            }
        }
        
        internal static System.Drawing.Bitmap system1 {
            get {
                object obj = ResourceManager.GetObject("system1", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
    }
}
