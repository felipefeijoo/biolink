using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using System.Reflection;
using BioLink.Client.Utilities;
using BioLink.Data.Model;

namespace BioLink.Client.Extensibility {

    /// <summary>
    /// Abstract base class for view model objects that can change. It basically supports the INotifyPropertyChanged interface
    /// as well as support for tracking if the instance has been "changed" (i.e. had any of its members modified).
    /// </summary>
    public abstract class ChangeableModelBase : IChangeable, INotifyPropertyChanged {

        private bool _changed;  // Dirty flag

        /// <summary>
        /// Helper to set a property that actually wraps a property on different instance
        /// </summary>
        /// <typeparam name="T">The type of the property</typeparam>
        /// <param name="wrappedPropertyExpr">An expression that yields the property to be set (on the wrapped instance)</param>
        /// <param name="wrappedObj">The wrapped instance</param>
        /// <param name="value">The new value</param>
        /// <param name="doIfChanged">An optional Action to be performed if the the property value was changed</param>
        /// <returns>true if the property has changed</returns>
        protected bool SetProperty<T>(Expression<Func<T>> wrappedPropertyExpr, object wrappedObj, T value, Action doIfChanged = null, bool changeAgnostic = false) {

            var destProp = (PropertyInfo)( (MemberExpression) wrappedPropertyExpr.Body).Member;
            T currVal = (T) destProp.GetValue(wrappedObj, null);

            var changed = !EqualityComparer<T>.Default.Equals(currVal, value);

            if (changed) {
                destProp.SetValue(wrappedObj, value, null);
                if (doIfChanged != null) {
                    doIfChanged();
                }
                RaisePropertyChanged(destProp.Name);
                if (!SuspendChangeMonitoring && !changeAgnostic) {
                    IsChanged = true;
                }
            }
            return changed;
        }

        /// <summary>
        /// Helper function to be called from within property setters that will automatically fire the property changed event (if required),
        /// and sets the dirty flag
        /// </summary>
        /// <typeparam name="T">Type of the property</typeparam>
        /// <param name="propertyName">Name of the property being set</param>
        /// <param name="backingField">A ref to the property backing member</param>
        /// <param name="value">The new value</param>
        /// <returns>true if the property has changed</returns>
        protected bool SetProperty<T>(string propertyName, ref T backingField, T value, bool changeAgnostic = false) {
            var changed = !EqualityComparer<T>.Default.Equals(backingField, value);
            if (changed) {
                backingField = value;
                RaisePropertyChanged(propertyName);
                if (!SuspendChangeMonitoring && !changeAgnostic) {
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
                    DataChanged(this);
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

    public delegate void DataChangedHandler(ChangeableModelBase viewmodel);

    public abstract class ViewModelBase : ChangeableModelBase {

        private bool _selected;
        private bool _deleted;
        private bool _renaming;

        public ViewModelBase() {
        }

        public bool IsSelected {
            get { return _selected; }
            set { SetProperty("IsSelected", ref _selected, value, true); }
        }

        public bool IsDeleted {
            get { return _deleted; }
            set { SetProperty("IsDeleted", ref _deleted, value); }
        }

        public bool IsRenaming {
            get { return _renaming; }
            set { SetProperty("IsRenaming", ref _renaming, value); }
        }

        public virtual string DisplayLabel { get; set; }
        public virtual BitmapSource Icon { get; set; }

        public object Tag { get; set; }        
    }

    public abstract class GenericViewModelBase<T> : ViewModelBase where T : GUIDObject {

        private BitmapSource _icon;

        protected GenericViewModelBase(T model) {
            this.Model = model;
        }

        protected void SetProperty<K>(Expression<Func<K>> wrappedPropertyExpr, K value, Action doIfChanged = null, bool changeAgnostic = false) {
            SetProperty(wrappedPropertyExpr, Model, value, doIfChanged, changeAgnostic);
        }

        protected virtual string RelativeImagePath {
            get { return null; }
        }

        public override BitmapSource Icon {
            get {
                if (_icon == null && RelativeImagePath != null) {
                    string assemblyName = this.GetType().Assembly.GetName().Name;
                    _icon = ImageCache.GetImage(String.Format("pack://application:,,,/{0};component/{1}", assemblyName, RelativeImagePath));
                }
                return _icon;
            }
            set {
                _icon = value;
            }
        }
        

        public T Model { get; private set; }
    }

}
