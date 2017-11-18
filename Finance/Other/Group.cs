using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Finance.Other
{
    public class Group<T> : ObservableCollection<T>
    {
        public Group(string key, IEnumerable<T> items)
            : base(items)
        {
            Key = key;
        }

        public string Key { get; set; }

        public static ObservableCollection<Group<T>> GetItemGroups<T>(IEnumerable<T> itemList,
            Func<T, string> getKeyFunc)
        {
            var groupList = from item in itemList
                group item by getKeyFunc(item)
                into g
                select new Group<T>(g.Key, g);
            return new ObservableCollection<Group<T>>(groupList);
        }
    }
}