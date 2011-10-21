/*******************************************************************************
 * Copyright (C) 2011 Atlas of Living Australia
 * All Rights Reserved.
 * 
 * The contents of this file are subject to the Mozilla Public
 * License Version 1.1 (the "License"); you may not use this file
 * except in compliance with the License. You may obtain a copy of
 * the License at http://www.mozilla.org/MPL/
 * 
 * Software distributed under the License is distributed on an "AS
 * IS" basis, WITHOUT WARRANTY OF ANY KIND, either express or
 * implied. See the License for the specific language governing
 * rights and limitations under the License.
 ******************************************************************************/
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;

// ReSharper disable FieldCanBeMadeReadOnly.Local
namespace BioLink.Client.Utilities {

    /// <summary>
    /// Help class for determining the mouse screen position relative to some control (visual)
    /// </summary>
    public class MouseUtilities {

        [StructLayout(LayoutKind.Sequential)]
        private struct Win32Point {
            public Int32 X;
            public Int32 Y;
        };

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(ref Win32Point pt);

        public static Point GetMousePosition(Visual relativeTo) {
            var mouse = new Win32Point();
            GetCursorPos(ref mouse);
            return relativeTo.PointFromScreen(new Point(mouse.X, mouse.Y));
        }


    }
}