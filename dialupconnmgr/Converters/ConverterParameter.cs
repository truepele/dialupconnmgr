using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Converters
{
    public class ConverterParameter
    {
        
        private object _defaultValue;
        private object _targetValue;

        public object ValueToCompare { get; set; }

        public object TargetValue
        {
            get { return _targetValue; }
            set
            {
                ValidateTypes(value, DefaultValue);
                _targetValue = value;
            }
        }

        public object DefaultValue
        {
            get { return _defaultValue; }
            set
            {
                ValidateTypes(value, TargetValue);
                _defaultValue = value;
            }
        }

        private void ValidateTypes(object v1, object v2)
        {
            if (v1 != null && v2 != null && v1.GetType() != v2.GetType())
            {
                throw new ArgumentException("v1 != null && v2 != null && v1.GetType() != v2.GetType()");
            }
        }
    }
}
