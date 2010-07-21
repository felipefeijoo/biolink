using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace BioLink.Client.Utilities {

    public abstract class ChangeableModelBase : IChangeable, INotifyPropertyChanged {

        private bool _changed;

        protected bool SetProperty<T>(string propertyName, T backingField, T value) {
            var changed = !EqualityComparer<T>.Default.Equals(backingField, value);
            if (changed) {
                backingField = value;
                RaisePropertyChanged(propertyName);
                if (!SuspendChangeMonitoring) {
                    IsChanged = true;
                }
            }
            return changed;
        }

        protected bool SetProperty<T>(string propertyName, ref T backingField, T value) {
            var changed = !EqualityComparer<T>.Default.Equals(backingField, value);
            if (changed) {
                backingField = value;
                RaisePropertyChanged(propertyName);
                if (!SuspendChangeMonitoring) {
                    IsChanged = true;
                }
            }
            return changed;
        }

        public bool IsChanged {
            get { return _changed; }
            set { 
                SetProperty("IsChanged", ref _changed, value);
                if (value && DataChanged != null) {
                    DataChanged();                    
                }
            }
        }

        protected void RaisePropertyChanged(string propertyName) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        public bool SuspendChangeMonitoring { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public event DataChangedHandler DataChanged;
    }

    public delegate void DataChangedHandler();

    public abstract class ViewModelBase : ChangeableModelBase {

        public bool IsSelected { get; set; }

        public abstract string Label { get; }
        public abstract BitmapSource Icon { get; set; }
    }

}
