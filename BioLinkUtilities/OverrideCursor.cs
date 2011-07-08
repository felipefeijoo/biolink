using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace BioLink.Client.Utilities {

    /// <summary>
    /// Thanks to Dennis Roche (taken from http://stackoverflow.com/questions/307004/changing-the-cursor-in-wpf-sometimes-works-sometimes-doesnt)
    /// 
    /// </summary>
    public class OverrideCursor : IDisposable {

        static Stack<Cursor> _stack = new Stack<Cursor>();

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
