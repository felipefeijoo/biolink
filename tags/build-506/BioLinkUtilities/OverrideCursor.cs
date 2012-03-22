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
using System.Collections.Generic;
using System.Windows.Input;

namespace BioLink.Client.Utilities {

    /// <summary>
    /// This class provides a neat way of temporarily changing the mouse cursor (to wait, for example), and have it automatically revert.
    /// <para>
    /// Thanks to Dennis Roche (modified from http://stackoverflow.com/questions/307004/changing-the-cursor-in-wpf-sometimes-works-sometimes-doesnt)
    /// </para>
    /// </summary>
    public class OverrideCursor : IDisposable {

        static readonly Stack<Cursor> _stack = new Stack<Cursor>();

        public OverrideCursor(Cursor changeToCursor) {
            _stack.Push(changeToCursor);            
            if (Mouse.OverrideCursor != changeToCursor) {
                Mouse.OverrideCursor = changeToCursor;
            }
        }

        public void Dispose() {
            _stack.Pop();
            Cursor cursor = _stack.Count > 0 ? _stack.Peek() : null;
            if (cursor != Mouse.OverrideCursor) {
                Mouse.OverrideCursor = cursor;
            }
        }

    }
}
