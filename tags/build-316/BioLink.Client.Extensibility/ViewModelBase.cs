using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.ComponentModel;
using System.Windows.Media;
using System.Reflection;
using BioLink.Client.Utilities;
using BioLink.Data;
using BioLink.Data.Model;
using System.Windows;
using System.Windows.Controls;

namespace BioLink.Client.Extensibility {

    /// <summary>
    /// Abstract base class for view model objects that can change. It basically supports the INotifyPropertyChanged interface
    /// as well as support for tracking if the instance has been "changed" (i.e. had any of its members modified).
    /// </summary>
    public abstract class ChangeableModelBase : System.Windows.DependencyObject, IChangeable, INotifyPropertyChanged {

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

            bool changed = false;
            var ignoreRtfAttr = Attribute.GetCustomAttribute(destProp, typeof(IgnoreRTFFormattingChanges));
            if (ignoreRtfAttr != null) {
                changed = CompareRTF(currVal as string, value as string);
            } else {
                changed = !EqualityComparer<T>.Default.Equals(currVal, value);
            }

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

        private bool CompareRTF(string current, string newval) {

            if (current == null) {
                current = "";
            }

            if (newval == null) {
                newval = "";
            }

            // basic tests first
            if (current == newval) {
                return false;
            }

            string lhs = RTFUtils.StripMarkup(current);
            string rhs = RTFUtils.StripMarkup(newval);

            return lhs != rhs;
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

                var destProp = this.GetType().GetProperty(propertyName);
                
                var ignoreRtfAttr = Attribute.GetCustomAttribute(destProp, typeof(IgnoreRTFFormattingChanges));
                if (ignoreRtfAttr != null) {
                    changed = CompareRTF(backingField as string, value as string);
                }

                if (changed) {
                    backingField = value;
                    RaisePropertyChanged(propertyName);
                    if (!SuspendChangeMonitoring && !changeAgnostic) {
                        IsChanged = true;
                    }
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
        private ImageSource _icon;

        public ViewModelBase() {            
            DataChanged += new DataChangedHandler(ViewModelBase_DataChanged);
        }

        void ViewModelBase_DataChanged(ChangeableModelBase viewmodel) {
            RaisePropertyChanged("DisplayLabel");
            RaisePropertyChanged("Icon");
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

        public virtual ImageSource Icon {
            get {                
                if (_icon == null && RelativeImagePath != null) {
                    string assemblyName = this.GetType().Assembly.GetName().Name;
                    _icon = ImageCache.GetImage(String.Format("pack://application:,,,/{0};component/{1}", assemblyName, RelativeImagePath));
                }

                if (IsChanged) {
                    return ImageCache.ApplyOverlay(_icon, String.Format("pack://application:,,,/BioLink.Client.Extensibility;component/images/ChangedOverlay.png"));
                } else {
                    return _icon;
                }
            }
            set {
                _icon = value;
            }
        }

        public virtual FrameworkElement TooltipContent {
            get {
                var control = new StackPanel();
                control.Orientation = Orientation.Horizontal;
                var label = new Label();
                label.Content = this.DisplayLabel;
                control.Children.Add(label);
                return control;
            }
        }

        protected virtual string RelativeImagePath {
            get { return null; }
        }

        public object Tag { get; set; }

        public virtual string DisplayLabel {
            get { return ToString(); }
        }

        public abstract int? ObjectID { get; }

    }

    public class DefaultTooltipContent : TooltipContentBase {

        public DefaultTooltipContent(int objectId, ViewModelBase viewModel, OwnedDataObject model) : base(objectId, viewModel) {
            this.Model = model;
        }

        protected OwnedDataObject Model { get; private set; }


        protected override OwnedDataObject GetModel() {
            return Model;
        }

        protected override void GetDetailText(OwnedDataObject model, TextTableBuilder builder) {            
        }
    }

    public abstract class GenericViewModelBase<T> : ViewModelBase {

        private Expression<Func<int>> _objectIDExpr = null;

        protected GenericViewModelBase(T model, Expression<Func<int>> objectIDExpr) {
            this.Model = model;
            _objectIDExpr = objectIDExpr;
        }

        public override FrameworkElement TooltipContent {
            get {
                if (Model is OwnedDataObject) {
                    return new DefaultTooltipContent(ObjectID.Value, this as ViewModelBase, this.Model as OwnedDataObject);
                } else {
                    return base.TooltipContent;
                }
            }
        }

        protected void SetProperty<K>(Expression<Func<K>> wrappedPropertyExpr, K value, Action doIfChanged = null, bool changeAgnostic = false) {
            SetProperty(wrappedPropertyExpr, Model, value, doIfChanged, changeAgnostic);
        }
        
        public T Model { get; private set; }

        public override int? ObjectID {
            get {
                if (_objectIDExpr == null) {
                    return null;                    
                } else {
                    var destProp = (PropertyInfo)((MemberExpression)_objectIDExpr.Body).Member;                    
                    return (int)destProp.GetValue(Model, null);
                }
            }
        }

    }

    public abstract class GenericOwnedViewModel<T> : GenericViewModelBase<T> where T : OwnedDataObject {

        public GenericOwnedViewModel(T model, Expression<Func<int>> objectIDExpr) : base(model, objectIDExpr) { }

        public DateTime DateCreated {
            get { return Model.DateCreated; }
            set { SetProperty(() => Model.DateCreated, value); }
        }

        public string WhoCreated {
            get { return Model.WhoCreated; }
            set { SetProperty(() => Model.WhoCreated, value); }
        }

        public DateTime DateLastUpdated {
            get { return Model.DateLastUpdated; }
            set { SetProperty(() => Model.DateLastUpdated, value); }
        }

        public string WhoLastUpdated {
            get { return Model.WhoLastUpdated; }
            set { SetProperty(() => Model.WhoLastUpdated, value); }
        }

    }

}
