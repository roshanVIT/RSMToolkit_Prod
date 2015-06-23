using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using AGI.Logger;
using System.Configuration;
using System.Data;
using RSMTool.App_Code;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Resources;
using Microsoft.Build.BuildEngine;
using Ionic.Zip;
using System.Threading;
using System.Diagnostics;
//using ICSharpCode.SharpZipLib.Zip;

namespace RSMTool.Pages
{
    public partial class RSMPreview : System.Web.UI.Page
    {
 
        string[] outputColnames;
        Dictionary<string, int> m_KrigingFitObjValues = new Dictionary<string, int>();
        Dictionary<string, double[]> m_ThetaValues = new Dictionary<string, double[]>();
        Dictionary<string, Tuple<string, string>> m_ActualFittedValues = new Dictionary<string, Tuple<string, string>>();
        [DllImport("ExportKrigingFitMethods.dll", EntryPoint = "callKrigingFitClass", CallingConvention = CallingConvention.Cdecl)]
        public static extern int callKrigingFitClass(double[] inputVar, Int32 inputSize, double[] outputVar, Int32 outputSize, Int32 moOfinputs);

        [DllImport("ExportKrigingFitMethods.dll", EntryPoint = "callKrigingFitClassWithoutSweepTheta", CallingConvention = CallingConvention.Cdecl)]
        public static extern int callKrigingFitClassWithoutSweepTheta(double[] inputVar, Int32 inputSize, double[] outputVar, Int32 outputSize, Int32 moOfinputs,double[] thetaValues);

        [DllImport("ExportKrigingFitMethods.dll", EntryPoint = "callKrigingFitEvalMethod", CallingConvention = CallingConvention.Cdecl)]
        public static extern double callKrigingFitEvalMethod(double[] inputs,int evalinputLength,int krigingObjectIndex);

        [DllImport("ExportKrigingFitMethods.dll", EntryPoint = "getSweepthetaValues", CallingConvention = CallingConvention.Cdecl)]
        //[return: MarshalAs(UnmanagedType.LPStr)]
        public static extern IntPtr getSweepthetaValues(int size);
       
        /*Generate Radio Buttons for o/p variables if count>1 else plot the graph using js function*/
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (m_KrigingFitObjValues.Count == 0 && Session["krigingFitObjValues"] != null)
                {

                    m_KrigingFitObjValues = (Dictionary<string, int>)Session["krigingFitObjValues"];

                }
                if (m_ThetaValues.Count == 0 && Session["thetaValues"] != null)
                {
                    m_ThetaValues = (Dictionary<string, double[]>)Session["thetaValues"];
                }
                if (m_ActualFittedValues.Count == 0 && Session["actualFittedValues"] != null)
                {
                    m_ActualFittedValues = (Dictionary<string, Tuple<string, string>>)Session["actualFittedValues"];
                }
                if (Session["outputArraycolNames"] != null)
                {
                     outputColnames = Session["outputArraycolNames"].ToString().Split(new char[] { ',' });
                     if (!IsPostBack)
                     {
                         callKrigingFitMethod();
                     }
                    panelRadioButtons.Visible = true;
                    RadioButtonList rdList = new RadioButtonList();
                    rdList.ID = "OutputfiledNames";
                    rdList.AutoPostBack = true;
                    rdList.CssClass = "rdListClass";
                    for (int i = 0; i < outputColnames.Length; i++)
                    {
                        rdList.Items.Add(new ListItem(outputColnames[i], outputColnames[i]));
                    }
                    rdList.SelectedIndex =0;
                    rdList.SelectedIndexChanged += new EventHandler(CallKrigingFitEvaluateMethod);
                    if (!IsPostBack)
                    {
                       
                        CallKrigingFitEvaluateMethod(rdList, new EventArgs());
                    }
                    panelRadioButtons.Controls.Add(rdList);
                }

            }
            catch (Exception ex)
            {
                Log.WriteToLog(ex.Message, ex.StackTrace);
            }

        }
        
        /// <summary>
        /// function that calls kriging dll to get output values for input data
        /// </summary>
        public void callKrigingFitMethod()
        {
            DataTable dt = (DataTable)Session["dataSetResults"];
            int inputColumnCount = Session["inputArraycolNames"].ToString().Split(new char[] { ',' }).Length;
            double[] dinputArray = Utility.getInputArray(dt);
            for (int i = 0; i < outputColnames.Length; i++)
            {
                if (!m_KrigingFitObjValues.ContainsKey(outputColnames[i]))
                {
                    
                   
                    DataTable outputDataTable = dt.DefaultView.ToTable(false, outputColnames[i]);
                    double[] doutputArray = Utility.GetArrayFromDataTable(outputDataTable);
                    if (m_ThetaValues.Count>0 && m_ThetaValues.ContainsKey(outputColnames[i]))
                    {
                        double[] thetaValues = m_ThetaValues[outputColnames[i]];
                        int krigingFitIndex = callKrigingFitClassWithoutSweepTheta(dinputArray, dinputArray.Length, doutputArray, doutputArray.Length, inputColumnCount,thetaValues);
                        m_KrigingFitObjValues.Add(outputColnames[i], krigingFitIndex);
                        Session["krigingFitObjValues"] = m_KrigingFitObjValues;
                    }
                    else
                    {
                       int krigingFitIndex = callKrigingFitClass(dinputArray, dinputArray.Length, doutputArray, doutputArray.Length, inputColumnCount);
                       double[] sweepValues = new double[inputColumnCount];
                        Marshal.Copy(getSweepthetaValues(inputColumnCount), sweepValues, 0, inputColumnCount);
                        m_KrigingFitObjValues.Add(outputColnames[i], krigingFitIndex);
                        m_ThetaValues.Add(outputColnames[i], sweepValues);
                        Session["krigingFitObjValues"] = m_KrigingFitObjValues;
                        Session["thetaValues"] = m_ThetaValues;
                    }
                }
            }
           
        }
        /*Radio button checked change event, Call KrigingFit Eval Method for each of the o/p variable*/
        public void CallKrigingFitEvaluateMethod(object sender, EventArgs e)
        {

            try
            {

                RadioButtonList x = (RadioButtonList)sender;
                if (!m_ActualFittedValues.ContainsKey(x.SelectedValue))
                {
                    int inputColumnCount = Session["inputArraycolNames"].ToString().Split(new char[] { ',' }).Length;
                    DataTable dt = (DataTable)Session["dataSetResults"];
                    double[] dinputArray = Utility.getInputArray(dt);
                    DataTable outputDataTable = dt.DefaultView.ToTable(false, x.SelectedValue);
                    double[] doutputArray = Utility.GetArrayFromDataTable(outputDataTable);
                    string outputValues = string.Join(",", doutputArray.Select(p => p.ToString()).ToArray());

                    outputValues = outputValues.TrimEnd(new char[] { ',' });

                    string fittedValues = string.Empty;

                    for (int s = 0; s < dinputArray.Length; s = s + inputColumnCount)
                    {
                        double[] eval = new double[inputColumnCount];
                        int r = 0;
                        for (int k = s; k < s + inputColumnCount; k++)
                        {
                            eval[r] = dinputArray[k];
                            r++;
                        }
                        fittedValues = fittedValues + callKrigingFitEvalMethod(eval, eval.Length, m_KrigingFitObjValues[x.SelectedValue]).ToString() + ",";
                    }

                    fittedValues = fittedValues.TrimEnd(new char[] { ',' });
                    m_ActualFittedValues.Add(x.SelectedValue, new Tuple<string, string>(fittedValues, outputValues));
                    Session["actualFittedValues"] = m_ActualFittedValues;
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "CallFunction", "PlotGraph('" + fittedValues + "','" + outputValues + "')", true);
                }
                else
                {
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "CallFunction", "PlotGraph('" + m_ActualFittedValues[x.SelectedValue].Item1 + "','" + m_ActualFittedValues[x.SelectedValue].Item2 + "')", true);
                }
            }
            
            catch (Exception ex)
            {
                Log.WriteToLog(ex.Message, ex.StackTrace);
            }
        }


        protected void btnRSMViewer_Click(object sender, EventArgs e)
      {
          Response.Redirect("RSMViewer.aspx",false);
      }
       
    }
}