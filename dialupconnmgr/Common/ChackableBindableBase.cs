using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEAppBuilder.Common
{
    public class ChackableBindableBase : BindableBase
    {
        private bool _isChecked;

        protected ChackableBindableBase(bool isUpgradableSettings = false)
            : base(isUpgradableSettings)
        {
        }

        [UserScopedSetting()]
        [DefaultSettingValue("False")]
        public bool IsChecked
        {
            get { return GetSettingProperty<bool>(); }
            set
            {
                if (SetSettingProperty(value))
                {
                    OnIsCheckedChanged(value);
                }
            }
        }

        protected virtual void OnIsCheckedChanged(bool value)
        {
        }
    }
}
