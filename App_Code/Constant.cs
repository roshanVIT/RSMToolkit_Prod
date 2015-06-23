using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RSMTool
{
    public class Constant
    {
        public const string JSONDLLFOLDERNAME = @"\Assemblies";
        public const string MSIPROJECTPATH = @"\AssembliesSetup\AssembliesSetup.wixproj";
        public const string MSIDOWNLOADPATH = "\\AssembliesSetup\\bin\\Debug\\AssembliesSetup.msi";
        public const string JSONDLLPATHTODELETE = "\\AssembliesSetup\\Assemblies\\jsonContent.dll";

        public const int MINLABELLENGTH = 15;

        public const string FIRSTINPUTDROPDOWNID = "ddlFirstInput";
        public const string SECONDINPUTDROPDOWNID = "ddlSecondInput";

        //DLL DETAILS
        public const string JSONDLLNAME = "jsonContent";
        public const string JSONRESOURCEFILENAME = "resourceJson";
        public const string JSONRESOURCENAME = "resourceData";
        public const string JSONFILENAME = "jsonData.json";

        public const double precisionPercent = 0.001f;
    }
}
