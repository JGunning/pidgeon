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
using System.Xml;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;
using System.Data;

namespace Client
{
    /// <summary>
    /// Skin
    /// </summary>
    public class Skin
    {
        /// <summary>
        /// Local font
        /// </summary>
        public string localfont = "Arial";
        /// <summary>
        /// Font size
        /// </summary>
        public int fontsize = 10;
        /// <summary>
        /// Join
        /// </summary>
        public System.Drawing.Color joincolor;
        /// <summary>
        /// Kick
        /// </summary>
        public System.Drawing.Color kickcolor;
        /// <summary>
        /// Everything
        /// </summary>
        public System.Drawing.Color miscelancscolor;
        /// <summary>
        /// Hl
        /// </summary>
        public System.Drawing.Color highlightcolor;
        /// <summary>
        /// Self
        /// </summary>
        public System.Drawing.Color selfcolor;
        /// <summary>
        /// Nick
        /// </summary>
        public System.Drawing.Color changenickcolor;
        /// <summary>
        /// Font
        /// </summary>
        public System.Drawing.Color fontcolor;
        /// <summary>
        /// +h
        /// </summary>
        public System.Drawing.Color colorh;
        /// <summary>
        /// Default
        /// </summary>
        public System.Drawing.Color colordefault;
        /// <summary>
        /// +v
        /// </summary>
        public System.Drawing.Color colorv;
        /// <summary>
        /// +q
        /// </summary>
        public System.Drawing.Color colorq;
        /// <summary>
        /// +a
        /// </summary>
        public System.Drawing.Color colora;
        /// <summary>
        /// +o
        /// </summary>
        public System.Drawing.Color coloro;
        /// <summary>
        /// Bg
        /// </summary>
        public System.Drawing.Color backgroundcolor;
        /// <summary>
        /// Other
        /// </summary>
        public System.Drawing.Color othercolor;
        /// <summary>
        /// Color
        /// </summary>
        public System.Drawing.Color colortalk;
        /// <summary>
        /// Color
        /// </summary>
        public System.Drawing.Color coloraway;
        /// <summary>
        /// Skin
        /// </summary>
        public string Name = "Default";
        /// <summary>
        /// Link
        /// </summary>
        public System.Drawing.Color link;
        /// <summary>
        /// mIRC colors
        /// </summary>
        public List<System.Drawing.Color> mrcl = new List<System.Drawing.Color>();
        /// <summary>
        /// Italic
        /// </summary>
        public bool italic;

        private Color colorFromXmlCode(XmlNode code)
        {
            Color color = Color.Black;
            try
            {
                color = System.Drawing.ColorTranslator.FromHtml(code.InnerText);
            }
            catch (Exception fail)
            {
                Core.handleException(fail);
            }
            return color;
        }

        /// <summary>
        /// Change a current skin to skin of another name
        /// </summary>
        /// <param name="path"></param>
        public static void ReloadSkin(string path)
        {
            lock (Configuration.SL)
            {
                foreach (Skin skin in Configuration.SL)
                {
                    if (skin.Name == path)
                    {
                        Configuration.CurrentSkin = skin;
                        return;
                    }
                }
            }
            Core.DebugLog("No such a skin is loaded: " + path);
        }

        /// <summary>
        /// Creates a new skin
        /// </summary>
        /// <param name="path"></param>
        public Skin(string path)
        {
            Core.DebugLog("Loading skin " + path);
            Defaults();
            string _p = path;
            Name = path;
            if (Name.Contains(Path.DirectorySeparatorChar.ToString()))
            {
                Name = Name.Substring(Name.LastIndexOf(Path.DirectorySeparatorChar.ToString()) + 1);
            }
            if (File.Exists(_p))
            {
                XmlDocument configuration = new XmlDocument();
                configuration.Load(_p);
                foreach (XmlNode curr in configuration.ChildNodes[0].ChildNodes)
                {
                    switch (curr.Name)
                    { 
                        case "fontcolor":
                            fontcolor = colorFromXmlCode(curr);
                            break;
                        case "link":
                            link = colorFromXmlCode(curr);
                            break;
                        case "joincolor":
                            joincolor = colorFromXmlCode(curr);
                            break;
                        case "miscelancscolor":
                            miscelancscolor = colorFromXmlCode(curr);
                            break;
                        case "highlightcolor":
                            highlightcolor = colorFromXmlCode(curr);
                            break;
                        case "selfcolor":
                            selfcolor = colorFromXmlCode(curr);
                            break;
                        case "changenickcolor":
                            changenickcolor = colorFromXmlCode(curr);
                            break;
                        case "backgroundcolor":
                            backgroundcolor = colorFromXmlCode(curr);
                            break;
                        case "othercolor":
                            othercolor = colorFromXmlCode(curr);
                            break;
                        case "colortalk":
                            colortalk = colorFromXmlCode(curr);
                            break;
                        case "fontname":
                            localfont = curr.InnerText;
                            break;
                        case "size":
                            fontsize = int.Parse(curr.InnerText);
                            break;
                    }
                }
            }
        }

        private void Defaults()
        {
            mrcl.Add(System.Drawing.Color.White);
            mrcl.Add(System.Drawing.Color.Gray);
            mrcl.Add(System.Drawing.Color.Blue);
            mrcl.Add(System.Drawing.Color.Green);
            mrcl.Add(System.Drawing.Color.Red);
            mrcl.Add(System.Drawing.Color.SandyBrown);
            mrcl.Add(System.Drawing.Color.Purple);
            mrcl.Add(System.Drawing.Color.Orange);
            mrcl.Add(System.Drawing.Color.Yellow);
            mrcl.Add(System.Drawing.Color.LightGreen);
            mrcl.Add(System.Drawing.Color.Cornsilk);
            mrcl.Add(System.Drawing.Color.Cyan);
            mrcl.Add(System.Drawing.Color.LightCoral);
            mrcl.Add(System.Drawing.Color.LightPink);
            mrcl.Add(System.Drawing.Color.Gray);
            mrcl.Add(System.Drawing.Color.LightGoldenrodYellow);
            fontcolor = System.Drawing.Color.White;
            joincolor = System.Drawing.Color.LightBlue;
            highlightcolor = System.Drawing.Color.LightPink;
            miscelancscolor = System.Drawing.Color.LightGreen;
            selfcolor = System.Drawing.Color.LightGray;
            changenickcolor = System.Drawing.Color.LightSteelBlue;
            link = System.Drawing.Color.LightBlue;
            colortalk = System.Drawing.Color.Yellow;
            kickcolor = System.Drawing.Color.White;
            colorq = System.Drawing.Color.LightBlue;
            colora = System.Drawing.Color.LightCyan;
            coloro = System.Drawing.Color.Yellow;
            colorh = System.Drawing.Color.Cyan;
            colorv = System.Drawing.Color.LightGreen;
            colordefault = System.Drawing.Color.White;
            coloraway = System.Drawing.Color.Gray;
            backgroundcolor = System.Drawing.Color.Black;
        }

        /// <summary>
        /// Creates a new skin
        /// </summary>
        public Skin()
        {
            Defaults();
        }

        /// <summary>
        /// LoadSkin
        /// </summary>
        /// <param name="name"></param>
        public static void LoadSkin(string name)
        {
            Skin skin = new Skin(name);
            lock (Configuration.SL)
            {
                Configuration.SL.Add(skin);
            }
        }
    }
}
