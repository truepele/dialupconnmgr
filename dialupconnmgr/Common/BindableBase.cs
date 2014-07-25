using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Xml.Serialization;
using dialupconnmgr;

namespace SEAppBuilder.Common
{
    public abstract class BindableBase : ApplicationSettingsBase, INotifyPropertyChanged
    {
        private bool _isUpgradable;
        protected BindableBase(bool isUpgradbleSettings = false)
        {
            _isUpgradable = isUpgradbleSettings;
        }

        /// <summary>
        /// Multicast event for property change notifications.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool _isHideWebViews = false;
        private bool _isBusy = false;

        public bool IsHideWebViews
        {
            get
            {
                return _isHideWebViews;
            }
            set { SetProperty(ref _isHideWebViews, value); }
        }


        public virtual bool IsBusy
        {
            get
            {
                return _isBusy;
            }
            set
            {
                SetProperty(ref _isBusy, value);
            }
        }

        /// <summary>
        /// Checks if a property already matches a desired value.  Sets the property and
        /// notifies listeners only when necessary.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="storage">Reference to a property with both getter and setter.</param>
        /// <param name="value">Desired value for the property.</param>
        /// <param name="propertyName">Name of the property used to notify listeners.  This
        /// value is optional and can be provided automatically when invoked from compilers that
        /// support CallerMemberName.</param>
        /// <returns>True if the value was changed, false if the existing value matched the
        /// desired value.</returns>
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] String propertyName = null)
        {
            if (object.Equals(storage, value)) return false;

            storage = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        protected bool SetSettingProperty<T>(T value, [CallerMemberName] String propertyName = null)
        {
            bool result = this[propertyName] == null || !this[propertyName].Equals(value);
            this[propertyName] = value;

            if(result)
                this.OnPropertyChanged(propertyName);

            return result;
        }

        public override void Upgrade()
        {
            if (!IsUpgraded && _isUpgradable)
            {
                base.Upgrade();
                IsUpgraded = true;
            }
        }

        [UserScopedSetting()]
        [DefaultSettingValue("False")]
        public bool IsUpgraded
        {
            get
            {
                return (bool)this["IsUpgraded"];
            }
            set
            {
                this["IsUpgraded"] = value;
            }
        }

        protected T GetSettingProperty<T>([CallerMemberName] String propertyName = null)
        {
            if(!IsUpgraded)
                Upgrade();

            return (T)(this[propertyName]??default(T));
        }

        protected bool SetPropertySync<T>(ref T storage, T value, [CallerMemberName] String propertyName = null)
        {
            if (object.Equals(storage, value)) return false;

            storage = value;
            this.OnPropertyChangedSync(propertyName);
            return true;
        }

        /// <summary>
        /// Notifies listeners that a property value has changed.
        /// </summary>
        /// <param name="propertyName">Name of the property used to notify listeners.  This
        /// value is optional and can be provided automatically when invoked from compilers
        /// that support <see cref="CallerMemberNameAttribute"/>.</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var eventHandler = this.PropertyChanged;
            if (eventHandler != null)
            {
                Delegate[] delegates = eventHandler.GetInvocationList();
                // Walk thru invocation list
                foreach (PropertyChangedEventHandler handler in delegates)
                {
                    var dispatcherObject = handler.Target as DispatcherObject;
                    Dispatcher dispatcher;
                    // If the subscriber is a DispatcherObject and different thread
                    if (dispatcherObject != null && dispatcherObject.CheckAccess() == false)
                    {
                        dispatcher = dispatcherObject.Dispatcher;
                    }
                    else
                    {
                        dispatcher = Dispatcher.CurrentDispatcher;
                    }

                    dispatcher.InvokeAsync(()=>handler(this, new PropertyChangedEventArgs(propertyName)), DispatcherPriority.DataBind);
                }
            }
        }

        protected void OnPropertyChangedSync([CallerMemberName] string propertyName = null)
        {
            var eventHandler = this.PropertyChanged;
            if (eventHandler != null)
            {
                eventHandler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected void SetIsBusy(bool val)
        {
            App.DispatcherInvokeAsync(()=>IsBusy=val);
        }
    }
}
