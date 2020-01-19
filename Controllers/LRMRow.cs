﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Controllers
{
    class LRMRow
    {
        public int routerID1;
        public int routerID2;
        public SortedSet<int> frequencySlots;
        public LRMRow(int routerID1, int routerID2, int MAXSLOTS)
        {
            this.routerID1 = routerID1;
            this.routerID2 = routerID2;
            for (int i = 0; i < MAXSLOTS; i++)
            {
                frequencySlots.Add(i);
            }
        }
    }
}
