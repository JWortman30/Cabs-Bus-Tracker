using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CABSBusTracker.Model;
using System.Collections.ObjectModel;


namespace CABSBusTracker.Functions
{
    public static class CollectionHelpers
    {
        /// <summary>
        /// Groups a passed Contacts ObservableCollection
        /// </summary>
        /// <param name="InitialContactsList">Unordered collection of Contacts</param>
        /// <returns>Grouped Observable Collection of Arrival Times suitable for the LongListSelector</returns>
        public static ObservableCollection<GroupedOC<ArrivalTime>> CreateGroupedOC(ObservableCollection<ArrivalTime> InitialTimesList)
        {

            //Initialise the Grouped OC to populate and return
            ObservableCollection<GroupedOC<ArrivalTime>> GroupedArrivalTimes = new ObservableCollection<GroupedOC<ArrivalTime>>();
            ObservableCollection<GroupedOC<ArrivalTime>> emptyGroupedArrivalTimes = new ObservableCollection<GroupedOC<ArrivalTime>>();

            //Sort the 
            var SortedList = (from con in InitialTimesList
                              orderby con.RouteName
                              select con).ToList();

            //Now enumerate throw the alphabet creating empty groups objects
            //This ensure that the whole alphabet exists even if we never populate them
            List<string> Alpha = new List<string>{ "BV", "CC", "CLN", "CLS", "ER", "MC", "NE" };
            foreach (string stp in Alpha)
            {
                //Create GroupedOC for given route
                GroupedOC<ArrivalTime> thisGOC = new GroupedOC<ArrivalTime>(stp);

                //Create a temp list with the appropriate Arrival Times that have this RouteName
                var SubsetOfTimes = (from time in SortedList
                                    where time.RouteName == stp
                                    select time).ToList<ArrivalTime>();

                //Populate the GroupedOC
                foreach (ArrivalTime art in SubsetOfTimes)
                {
                    var query = from busNameID in thisGOC

                                where busNameID.busID.Contains(art.busID)

                                select busNameID;

                    if(!query.Any())
                    {
                        thisGOC.Add(art);
                    }
                }
                
                thisGOC.Title = stp;
                thisGOC.DispTitle = "Route: " + stp;
                switch(stp)
                {
                    case "BV":
                        thisGOC.DispColor = "#CC00803F";
                        break;
                    case "CC":
                        thisGOC.DispColor = "#CC4680D3";
                        break;
                    case "CLN":
                        thisGOC.DispColor = "#CC77787A";
                        break;
                    case "CLS":
                        thisGOC.DispColor = "#CCC0262C";
                        break;
                    case "ER":
                        thisGOC.DispColor = "#CC113580";
                        break;
                    case "MC":
                        thisGOC.DispColor = "#CCF05994";
                        break;
                    case "NE":
                        thisGOC.DispColor = "#CCF26021";
                        break;
                }

                //thisGOC.DispColor = "#CCF26021";
                //Add this GroupedOC to the observable collection that is being returned 
                // and the LongListSelector can be bound to.
                if (thisGOC.Count != 0)
                {
                    GroupedArrivalTimes.Add(thisGOC);
                }
                else
                {
                    emptyGroupedArrivalTimes.Add(thisGOC);
                }
            }
            IEnumerable<GroupedOC<ArrivalTime>> sort;
            ObservableCollection<GroupedOC<ArrivalTime>> tempGroupedArrivalTimes = new ObservableCollection<GroupedOC<ArrivalTime>>();
            sort = GroupedArrivalTimes.OrderBy(item => item[0].expectedTimeMinutes);
            foreach (var item in sort)
            {
                tempGroupedArrivalTimes.Add(item);
            }
            foreach(var item in emptyGroupedArrivalTimes)
            {
                tempGroupedArrivalTimes.Add(item);
            }
            return tempGroupedArrivalTimes;
            //return GroupedArrivalTimes;
        }
    }

}
