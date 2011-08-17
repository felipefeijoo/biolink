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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using BioLink.Client.Utilities;

namespace BioLink.Client.Extensibility {

    public class DragDropScrollViewer : ScrollViewer {

        private static readonly double DRAG_INTERVAL = 10; // milliseconds
        private static readonly double DRAG_ACCELERATION = 0.002; // pixels per millisecond^2
        private static readonly double DRAG_MAX_VELOCITY = 2.0; // pixels per millisecond
        private static readonly double DRAG_INITIAL_VELOCITY = 0.05; // pixels per millisecond
        private static readonly double DRAG_MARGIN = 20.0;

        private DispatcherTimer _dragScrollTimer = null;
        private double _dragVelocity;

        public DragDropScrollViewer() {
            Loaded += new RoutedEventHandler(DragDropScrollViewer_Loaded);
        }

        void DragDropScrollViewer_Loaded(object sender, RoutedEventArgs e) {
            var child = Content as FrameworkElement;

            if (child != null) {
                child.AddHandler(MouseWheelEvent, new RoutedEventHandler(MouseWheelHandler), true);
            }
        }

        private void MouseWheelHandler(object sender, RoutedEventArgs e) {
            MouseWheelEventArgs wheelArgs = (MouseWheelEventArgs)e;
            ScrollToVerticalOffset(VerticalOffset - wheelArgs.Delta);
        }

        protected override void OnPreviewQueryContinueDrag(QueryContinueDragEventArgs args) {
            base.OnPreviewQueryContinueDrag(args);

            if (args.Action == DragAction.Cancel || args.Action == DragAction.Drop) {
                CancelDrag();
            } else if (args.Action == DragAction.Continue) {
                Point p = MouseUtilities.GetMousePosition(this);
                if ((p.Y < DRAG_MARGIN) || (p.Y > RenderSize.Height - DRAG_MARGIN)) {
                    if (_dragScrollTimer == null) {
                        _dragVelocity = DRAG_INITIAL_VELOCITY;
                        _dragScrollTimer = new DispatcherTimer();
                        _dragScrollTimer.Tick += TickDragScroll;
                        _dragScrollTimer.Interval = new TimeSpan(0, 0, 0, 0, (int)DRAG_INTERVAL);
                        _dragScrollTimer.Start();
                    }
                }
            }
        }

        private void TickDragScroll(object sender, EventArgs e) {
            bool isDone = true;

            if (this.IsLoaded) {
                Rect bounds = new Rect(RenderSize);
                Point p = MouseUtilities.GetMousePosition(this);
                if (bounds.Contains(p)) {
                    if (p.Y < DRAG_MARGIN) {
                        DragScroll(DragDirection.Up);
                        isDone = false;
                    } else if (p.Y > RenderSize.Height - DRAG_MARGIN) {
                        DragScroll(DragDirection.Down);
                        isDone = false;
                    }
                }
            }

            if (isDone) {
                CancelDrag();
            }
        }

        private void CancelDrag() {
            if (_dragScrollTimer != null) {
                _dragScrollTimer.Tick -= TickDragScroll;
                _dragScrollTimer.Stop();
                _dragScrollTimer = null;
            }
        }

        private enum DragDirection {
            Down,
            Up
        };

        private void DragScroll(DragDirection direction) {
            bool isUp = (direction == DragDirection.Up);
            double offset = Math.Max(0.0, VerticalOffset + (isUp ? -(_dragVelocity * DRAG_INTERVAL) : (_dragVelocity * DRAG_INTERVAL)));
            ScrollToVerticalOffset(offset);
            _dragVelocity = Math.Min(DRAG_MAX_VELOCITY, _dragVelocity + (DRAG_ACCELERATION * DRAG_INTERVAL));
        }
    }

}
