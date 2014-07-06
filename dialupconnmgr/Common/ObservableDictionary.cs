using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEAppBuilder.Common
{
    class ObservableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        #region Constants

        private const string COUNT_PROPNAME = "Count";
        private const string KEYS_PROPNAME = "Keys";
        private const string VALUES_PROPNAME = "Values";

        #endregion

        #region Private Fileds

        private IDictionary<TKey, TValue> _dictionary;

        #endregion

        #region Events

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public ObservableDictionary()
        {
            _dictionary = new Dictionary<TKey, TValue>();
        }

        #region Properties

        public int Count
        {
            get { return _dictionary.Count; }
        }

        public bool IsReadOnly
        {
            get { return _dictionary.IsReadOnly; }
        }

        public ICollection<TKey> Keys
        {
            get { return _dictionary.Keys; }
        }

        public ICollection<TValue> Values
        {
            get { return _dictionary.Values; }
        }

        #endregion

        #region Indexers

        public TValue this[TKey key]
        {
            get
            {
                return _dictionary[key];
            }
            set
            {
                KeyValuePair<TKey, TValue> item, oldItem;
                if (_dictionary.ContainsKey(key))
                {
                    item = _dictionary.FirstOrDefault(i => i.Key.Equals(key));
                    oldItem = new KeyValuePair<TKey, TValue>(key, item.Value);
                    _dictionary[key] = value;
                    OnCollectionChanged(NotifyCollectionChangedAction.Replace, item, oldItem);
                }
                else
                {
                    item = _dictionary.FirstOrDefault(i => i.Key.Equals(key));
                    _dictionary[key] = value;
                    OnCollectionChanged(NotifyCollectionChangedAction.Add, item);
                }
            }
        }

        #endregion

        #region Public methods

        public void Add(TKey key, TValue value)
        {
            var item = new KeyValuePair<TKey, TValue>(key, value);
            Add(item);
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            _dictionary.Add(item);
            OnCollectionChanged(NotifyCollectionChangedAction.Add, item);
        }

        public bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        public bool Remove(TKey key)
        {
            bool result = false;
            if (_dictionary.ContainsKey(key))
            {
                var item = _dictionary.FirstOrDefault(i => i.Key.Equals(key));
                _dictionary.Remove(key);
                OnCollectionChanged(NotifyCollectionChangedAction.Remove, item);
            }

            return result;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        public void Clear()
        {
            _dictionary.Clear();
            OnCollectionChanged();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return _dictionary.Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            _dictionary.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            bool result = false;
            if (result = _dictionary.Remove(item))
            {
                OnCollectionChanged(NotifyCollectionChangedAction.Remove, item);
            }
            return result;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_dictionary).GetEnumerator();
        }

        #endregion

        private void OnCollectionChanged()
        {
            if (CollectionChanged != null)
            {
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                OnPropertyChanged();
            }
        }

        private void OnCollectionChanged(NotifyCollectionChangedAction action, IList items)
        {
            if (CollectionChanged != null)
            {
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(action, items));
                OnPropertyChanged();
            }
        }

        private void OnCollectionChanged(NotifyCollectionChangedAction action, IList items, IList oldItems = null)
        {
            if (CollectionChanged != null)
            {
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(action, items, oldItems));
                OnPropertyChanged();
            }
        }

        private void OnCollectionChanged(NotifyCollectionChangedAction action, KeyValuePair<TKey, TValue> item, KeyValuePair<TKey, TValue> oldItem)
        {
            if (CollectionChanged != null)
            {
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(action, item, oldItem));
                OnPropertyChanged();
            }
        }

        private void OnCollectionChanged(NotifyCollectionChangedAction action, KeyValuePair<TKey, TValue> item)
        {
            if (CollectionChanged != null)
            {
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(action, item));
                OnPropertyChanged();
            }
        }

        private void OnPropertyChanged(string propertyName = null)
        {
            if (PropertyChanged != null)
            {
                if (propertyName != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
                else
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(COUNT_PROPNAME));
                    PropertyChanged(this, new PropertyChangedEventArgs(KEYS_PROPNAME));
                    PropertyChanged(this, new PropertyChangedEventArgs(VALUES_PROPNAME));
                }
            }
        }
    }
}
