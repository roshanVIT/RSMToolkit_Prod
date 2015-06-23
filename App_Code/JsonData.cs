using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RSMTool.App_Code
{
    public class JsonData
    {
        public string filePath
        {
            get;
            set;
        }
        public string[] inputArrayColNames
        {
            get;
            set;
        }
        public Dictionary<string, double[]> outputArray
        {
            get;
            set;
        }
        public Dictionary<string, double[]> thetaValues
        {
            get;
            set;
        }
        public string headerClientIDs
        {
            get;
            set;
        }
        public string outputheaderClinetIDs
        {
            get;
            set;
        }
        public double[] inputArray
        {
            get;
            set;
        }
      
    }
}
