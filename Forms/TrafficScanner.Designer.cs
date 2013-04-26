﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Client.Forms
{
    public partial class TrafficScanner
    {
        private global::Gtk.ScrolledWindow GtkScrolledWindow;
        private global::Gtk.TextView textview2;

        protected virtual void Build()
        {
            global::Stetic.Gui.Initialize(this);
            // Widget Client.Forms.TrafficScanner
            this.Name = "Client.Forms.TrafficScanner";
            this.Title = "TrafficScanner";
            this.WindowPosition = ((global::Gtk.WindowPosition)(4));
            // Container child Client.Forms.TrafficScanner.Gtk.Container+ContainerChild
            this.GtkScrolledWindow = new global::Gtk.ScrolledWindow();
            this.GtkScrolledWindow.Name = "GtkScrolledWindow";
            this.GtkScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
            // Container child GtkScrolledWindow.Gtk.Container+ContainerChild
            this.textview2 = new global::Gtk.TextView();
            this.textview2.CanFocus = true;
            this.textview2.Name = "textview2";
            this.textview2.Editable = false;
            this.GtkScrolledWindow.Add(this.textview2);
            this.Add(this.GtkScrolledWindow);
            if ((this.Child != null))
            {
                this.Child.ShowAll();
            }
            this.DefaultWidth = 579;
            this.DefaultHeight = 395;
            this.Hide();
        }
    }
}
