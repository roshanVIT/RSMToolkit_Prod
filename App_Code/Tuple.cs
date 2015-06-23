﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RSMTool.App_Code
{
    public class Tuple<T1,T2>
    {
        public T1 Item1 { get; set; }
        public T2 Item2 { get; set; }

        public Tuple(T1 Item1, T2 Item2)
        {
            this.Item1 = Item1;
            this.Item2 = Item2;
        }
    }
    public class Tuple<T1, T2,T3>
    {
        public T1 Item1 { get; set; }
        public T2 Item2 { get; set; }
        public T3 Item3 { get; set; }

        public Tuple(T1 Item1, T2 Item2, T3 Item3)
        {
            this.Item1 = Item1;
            this.Item2 = Item2;
            this.Item3 = Item3;
        }
    }

}
