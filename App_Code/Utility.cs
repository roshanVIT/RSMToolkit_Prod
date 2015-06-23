using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Text;
using RSMTool.Pages;
using System.IO;
using Newtonsoft.Json;
using System.Configuration;
using System.Resources;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.Build.BuildEngine;
using System.Runtime.InteropServices;
using System.Globalization;
using AGI.Logger;

namespace RSMTool.App_Code
{
    public class Utility
    {
        [DllImport("ExportKrigingFitMethods.dll", EntryPoint = "callKrigingFitClassWithoutSweepTheta", CallingConvention = CallingConvention.Cdecl)]
        public static extern int callKrigingFitClassWithoutSweepTheta(double[] inputVar, Int32 inputSize, double[] outputVar, Int32 outputSize, Int32 moOfinputs, double[] thetaValues);

        public static double[] GetArrayFromDataTable(DataTable inputTable)
        {
            double[] inputArray =new double[inputTable.Rows.Count * inputTable.Columns.Count];
            int f = 0;
            foreach (DataRow row in inputTable.Rows)
            {
                foreach (var item in row.ItemArray)
                {
                    inputArray[f++] = Convert.ToDouble(item);
                }
            }
            return inputArray;
        }
        //GetMaxValue of a column selected in the Excel Sheet
        public static string GetMaxValueofColumns(string colName)
        {
            try
            {
                DataTable dt = (DataTable)HttpContext.Current.Session["dataSetResults"];
                object[] colValues = dt.AsEnumerable().Select(s => s.Field<object>(colName)).ToArray<object>();
                double[] clValues = Array.ConvertAll<object, double>(colValues,Convert.ToDouble);
               double[] myNum=new double[colValues.Length];
              double maxColValues = clValues.Max();
                return maxColValues.ToString();
            }
            catch (Exception ex)
            {
                Log.WriteToLog(ex.Message, ex.StackTrace);
                return string.Empty;
            }
            //NumericUpDown
        }
        public static string GetMinValueofColumns(string colName)
        {
            try
            {
                DataTable dt = (DataTable)HttpContext.Current.Session["dataSetResults"];
                object[] colValues = dt.AsEnumerable().Select(s => s.Field<object>(colName)).ToArray<object>();
                double[] clValues = Array.ConvertAll<object, double>(colValues, Convert.ToDouble);

                //double[] clValues = colValues.Sp(',').Select(Double.Parse).ToArray();
                double minColValues = clValues.Min();
                //if (colName.Trim().Equals("var1")) return "-1.0";
                //else 
                return minColValues.ToString();
            }
            catch (Exception ex)
            {
                Log.WriteToLog(ex.Message, ex.StackTrace);
                return string.Empty;
            }

        }
        public static string GetPointsForBubble(RSMViewer.ContourParams ctParams, double precisionPercent, string[] contourRange, out double rMax)
        {
            int numIntervals = 50;
            int i = 0;
             double[] firstI = new double[2704];
            double[] secondI = new double[2704];
            double[] results = new double[2704];
            string colors = string.Empty;
            var jsonData = new
            {
                rValues = new double[numIntervals + 1][]
            };

            KrigingEvalDelegate krigingDelegate = new KrigingExecutor(ctParams.ModelIndex, ctParams.XParamIndex, ctParams.YParamIndex, ctParams.ParamArray).CallKrigingEval;
            double xInt = (ctParams.XMax - ctParams.XMin) / numIntervals;
            double yInt = (ctParams.YMax - ctParams.YMin) / numIntervals;
            double rMin = double.MaxValue;
            rMax = double.MinValue;

            int xIndex = 0;

            for (double xCurr = ctParams.XMin; xCurr <= ctParams.XMax; xCurr = ((xCurr != ctParams.XMax) && ((xCurr + xInt) > ctParams.XMax) ? ctParams.XMax : xCurr + xInt))
            {
                int yIndex = numIntervals;
                if (xIndex > numIntervals)
                    xIndex = numIntervals;

                for (double yCurr = ctParams.YMin; yCurr <= ctParams.YMax; yCurr = ((yCurr != ctParams.YMax) && ((yCurr + yInt) > ctParams.YMax) ? ctParams.YMax : yCurr + yInt))
                {

                    if (yIndex < 0)
                    {
                        yIndex = 0;
                    }
                    else if (xIndex == 0)
                    {
                        jsonData.rValues[yIndex] = new double[numIntervals + 1];
                    }
                    //firstinputVars = firstinputVars + xCurr + ",";
                    // secondinputVars = secondinputVars + yCurr + ",";
                    firstI[i] = xCurr;
                    secondI[i] = yCurr;
            
                    double r = krigingDelegate(xCurr, yCurr);
                    results[i] = r;
                    if (r > -105 && r < -40)
                    {
                        colors = colors + "blue" + ",";
                    }
                    else if (r > -40 && r < 0)
                    {
                        colors = colors + "lightBlue" + ",";
                    }
                    else if (r >= 0 && r < 60)
                    {
                        colors = colors + "green" + ",";
                    }
                    else if (r > 60 && r < 120)
                    {
                        colors = colors + "yellow" + ",";
                    }
                    else if (r > 120 && r < 180)
                    {
                        colors = colors + "orange" + ",";
                    }
                    //result = result + r + ",";
                    jsonData.rValues[yIndex][xIndex] = r;
                    if (r < rMin) rMin = r;
                    if (r > rMax) rMax = r;
                    yIndex--;
                    i++;
                }
                xIndex++;
            }

            double rInt = (rMax - rMin) / (ctParams.NumContours);
            for (int rIndex = 0; rIndex < ctParams.NumContours; rIndex++)
            {
                contourRange[rIndex] = (rMin + rIndex * rInt).ToString();
            }


            StringBuilder sb = new StringBuilder();
            using (StringWriter sw = new StringWriter(sb))
            using (JsonWriter jw = new JsonTextWriter(sw))
            {
                jw.Formatting = Formatting.None;
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(jw, jsonData.rValues);
            }

            HttpContext.Current.Session["firstInputVars"] = String.Join(",", firstI.Select(p => p.ToString()).ToArray());
            //firstinputVars.TrimEnd(new char[] { ',' });
            HttpContext.Current.Session["secondInputVars"] = String.Join(",", secondI.Select(p => p.ToString()).ToArray());
            //secondinputVars.TrimEnd(new char[] { ',' });
            HttpContext.Current.Session["result"] = String.Join(",", results.Select(p => p.ToString()).ToArray());
            //result.TrimEnd(new char[] { ',' });
            HttpContext.Current.Session["colors"] = colors.TrimEnd(new char[] { ',' });
            return sb.ToString();

        }
        public static string GetContours(RSMViewer.ContourParams ctParams, double precisionPercent, string[] contourRange, out double rMax)
        // public static double[][] GetContours(RSMViewer.ContourParams ctParams, double precisionPercent, string[] contourRange, out double rMax)
        {
            int numIntervals = 300;
            var jsonData = new
            {
                rValues = new double[numIntervals + 1][]
            };

            KrigingEvalDelegate krigingDelegate = new KrigingExecutor(ctParams.ModelIndex, ctParams.XParamIndex, ctParams.YParamIndex, ctParams.ParamArray).CallKrigingEval;
            double xInt = (ctParams.XMax - ctParams.XMin) / numIntervals;
            double yInt = (ctParams.YMax - ctParams.YMin) / numIntervals;
            double rMin = double.MaxValue;
            rMax = double.MinValue;

            int xIndex = 0;

            for (double xCurr = ctParams.XMin; xCurr <= ctParams.XMax; xCurr = ((xCurr != ctParams.XMax) && ((xCurr + xInt) > ctParams.XMax) ? ctParams.XMax : xCurr + xInt))
            {
                int yIndex = numIntervals;
                if (xIndex > numIntervals)
                    xIndex = numIntervals;

                for (double yCurr = ctParams.YMin; yCurr <= ctParams.YMax; yCurr = ((yCurr != ctParams.YMax) && ((yCurr + yInt) > ctParams.YMax) ? ctParams.YMax : yCurr + yInt))
                {
                    if (yIndex < 0)
                    {
                        yIndex = 0;
                    }
                    else if (xIndex == 0)
                    {
                        jsonData.rValues[yIndex] = new double[numIntervals + 1];
                    }
                    double r = krigingDelegate(xCurr, yCurr);
                    jsonData.rValues[yIndex][xIndex] = r;
                    if (r < rMin) rMin = r;
                    if (r > rMax) rMax = r;
                    yIndex--;
                }
                xIndex++;
            }

            double rInt = (rMax - rMin) / (ctParams.NumContours);
            for (int rIndex = 0; rIndex < ctParams.NumContours; rIndex++)
            {
                contourRange[rIndex] = (rMin + rIndex * rInt).ToString();
            }


            StringBuilder sb = new StringBuilder();
            using (StringWriter sw = new StringWriter(sb))
            using (JsonWriter jw = new JsonTextWriter(sw))
            {
                jw.Formatting = Formatting.None;
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(jw, jsonData.rValues);
            }
            return sb.ToString();
          //  return jsonData.rValues;

        }
        /* Called in the GenerateJSON File.Dynamically generate DLL containing the selected input output and thetaValues 
            for the EXE*/
        public static void WriteModelConfig(string asmName, string resourceFileName, string resourceName, string resourceData)
        {
            try
            {
                string jsonFolderPath = ConfigurationManager.AppSettings["jsonFilePath"];
                string pathtoJSONDLL = AppDomain.CurrentDomain.BaseDirectory +Constant.JSONDLLFOLDERNAME;
                string asmFileName = asmName + ".dll";
                AssemblyName assembly = new AssemblyName();
                assembly.Name = asmName;
                AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(
                assembly,
                System.Reflection.Emit.AssemblyBuilderAccess.RunAndSave, pathtoJSONDLL);

                ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(assembly.Name, asmFileName);
                IResourceWriter resWriter = moduleBuilder.DefineResource(resourceFileName + ".resources", "Json data");
                resWriter.AddResource(resourceName, resourceData);
                assemblyBuilder.Save(asmFileName);
            }
            catch (Exception e)
            {
                Log.WriteToLog(e.Message, e.StackTrace);
            }

        }
        public static object GetDataForJSONCreation()
        {
            string[] outputcolNames = { };
            Dictionary<string, double[]> dictoutputColnameValues = new Dictionary<string, double[]>();
            string[] inputColnames = HttpContext.Current.Session["inputArraycolNames"].ToString().Split(new char[] { ',' });
            DataTable dt = (DataTable)HttpContext.Current.Session["dataSetResults"];
            DataTable inputDatatable = dt.DefaultView.ToTable(false, inputColnames);
            List<object> lstOutputfileldColValues = new List<object>();
            if (HttpContext.Current.Session["outputArraycolNames"].ToString().Contains(','))
                outputcolNames = HttpContext.Current.Session["outputArraycolNames"].ToString().Split(new char[] { ',' });
            else
            {
                outputcolNames = new string[1];
                outputcolNames[0] = HttpContext.Current.Session["outputArraycolNames"].ToString();
            }
            Dictionary<string, double[]> thetaValues = new Dictionary<string, double[]>();
            if (HttpContext.Current.Session["thetaValues"] != null)
            {
                thetaValues = (Dictionary<string, double[]>)HttpContext.Current.Session["thetaValues"];
            }
            for (int i = 0; i < outputcolNames.Length; i++)
            {
                lstOutputfileldColValues = new List<object>();
                lstOutputfileldColValues = (from r in dt.AsEnumerable()
                                            select r.Field<object>(outputcolNames[i])).ToList();
                dictoutputColnameValues.Add(outputcolNames[i], lstOutputfileldColValues.Select(x => double.Parse(x.ToString())).ToArray());
            }
            double[] dinputArray = Utility.GetArrayFromDataTable(inputDatatable);
            var data = new
            {
                inputArray = dinputArray,
                inputArrayColNames = inputColnames,
                outputArray = dictoutputColnameValues,
                thetaValues = thetaValues,
                inpuFieldNameCount = inputColnames.Length,
                filePath = HttpContext.Current.Session["filePath"],
                headerClientIDs = HttpContext.Current.Session["headerClinetIDs"],
                outputheaderClinetIDs=HttpContext.Current.Session["outputheaderClinetIDs"],
                contourParams = HttpContext.Current.Session["saveRSMViewerSettings"]
            };
            return data;
        }
        public static double[] getInputArray(DataTable dt)
        {
            //DataTable dt = (DataTable)HttpContext.Current.Session["dataSetResults"];
            string[] inputColnames = HttpContext.Current.Session["inputArraycolNames"].ToString().Split(new char[] { ',' });
            DataTable inputDatatable = dt.DefaultView.ToTable(false, inputColnames);
            double[] dinputArray = Utility.GetArrayFromDataTable(inputDatatable);
            return dinputArray;

        }
        public static bool SetSessionVariables(string filePath,System.Web.UI.WebControls.Label lbImport)
        {
            using (StreamReader jsonReader = new StreamReader(filePath))
            {
                string json = jsonReader.ReadToEnd();
                JsonData js = JsonConvert.DeserializeObject<JsonData>(json);
                if (js != null)
                {
                    HttpContext.Current.Session["thetaValues"] = js.thetaValues;
                    HttpContext.Current.Session["headerClinetIDs"] = js.headerClientIDs;
                    Dictionary<string, double[]> outputValues = js.outputArray;
                    HttpContext.Current.Session["outputheaderClinetIDs"] = js.outputheaderClinetIDs;
                    HttpContext.Current.Session["inputArraycolNames"] = string.Join(",", js.inputArrayColNames);
                    HttpContext.Current.Session["outputArraycolNames"] = string.Join(",", js.outputArray.Keys.ToArray());
                    string[] outputVariables = js.outputArray.Keys.ToArray();
                    Dictionary<string, int> KrigingFitObjValues = new Dictionary<string, int>();
                    for (int i = 0; i < outputVariables.Length; i++)
                    {
                        int krigingIndex = callKrigingFitClassWithoutSweepTheta(js.inputArray, js.inputArray.Length, js.outputArray[outputVariables[i]], js.outputArray[outputVariables[i]].Length, js.inputArrayColNames.Length, js.thetaValues[outputVariables[i]]);
                        KrigingFitObjValues.Add(outputVariables[i], krigingIndex);
                        HttpContext.Current.Session["krigingFitObjValues"] = KrigingFitObjValues;
                    }
                    WriteModelConfig(Constant.JSONDLLNAME, Constant.JSONRESOURCEFILENAME, Constant.JSONRESOURCENAME,json);
                    lbImport.Text = lbImport.Text + js.filePath;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}