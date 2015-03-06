using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.ObjectModel;

namespace CABSBusTracker.Model
{
    public class GroupedOC<T> : ObservableCollection<T>
    {
        /// <summary>
        /// The Group Title
        /// </summary>
        public string Title
        {
            get;
            set;
        }

        public string DispTitle
        {
            get;
            set;
        }

        public string DispColor
        {
            get;
            set;

        }

        /// <summary>
        /// Constructor ensure that a Group Title is included
        /// </summary>
        /// <param name="name">string to be used as the Group Title</param>
        public GroupedOC(string name)
        {
            this.Title = name;
        }

        /// <summary>
        /// Returns true if the group has a count more than zero
        /// </summary>
        public bool HasItems
        {
            get
            {
                return (Count != 0);
            }
            private set
            {
            }
        }
    }
}
