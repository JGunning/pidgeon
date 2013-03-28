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

namespace Client.GTK
{
    public class Menu
    {
        public bool Enabled = false;
        public bool Visible = false;
        public string Text;

        public Menu()
        { 
            Text = null;
        }

        public Menu(string id)
        {
            Text = id;
        }
    }

    public class PidgeonForm : Gtk.Window
    {
        public int Height
        {
            get
            {
                int height;
                int width;
                this.GetSize(out width, out height);
                return height;
            }
            set
            {
                this.SetSizeRequest(Width, value);
            }
        }

        public int Width
        {
            get
            {
                int height;
                int width;
                this.GetSize(out width, out height);
                return width;
            }
            set
            {
                this.SetSizeRequest(value, Height);
            }
        }	

        public bool Enabled
        {
            set
            {
                this.Sensitive = value;
            }
            get
            {
                return this.Sensitive;
            }
        }

        public PidgeonForm() : base(Gtk.WindowType.Toplevel)
        {
            
        }
    }
}
