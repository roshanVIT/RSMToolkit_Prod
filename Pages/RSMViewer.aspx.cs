using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Script.Serialization;
using RSMTool.App_Code;
using System.Data;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using System.Drawing;
using System.Configuration;
using System.Resources;
using System.Text.RegularExpressions;
using AGI.Logger;
using Ionic.Zip;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace RSMTool.Pages
{
    public delegate double KrigingEvalDelegate(double x, double y);
    
    public class KrigingExecutor
    {
        private int _modelIndex;
        private int _xIndex;
        private int _yIndex;
        private double[] _paramArray;
        

        public KrigingExecutor(int modelIndex, int xIndex, int yIndex, double[] paramArray)
        {
            _modelIndex = modelIndex;
            _xIndex = xIndex;
            _yIndex = yIndex;
            _paramArray = paramArray;
        }

        public double CallKrigingEval(double x, double y)
        {
            _paramArray[_xIndex] = x;
            _paramArray[_yIndex] = y;
            return RSMViewer.callKrigingFitEvalMethod(_paramArray, _paramArray.Length, _modelIndex);
        }
    }
    public  partial class RSMViewer : System.Web.UI.Page
    {
        [DllImport("ExportKrigingFitMethods.dll", EntryPoint = "callKrigingFitClass", CallingConvention = CallingConvention.Cdecl)]
        public static extern int callKrigingFitClass(double[] inputVar, Int32 inputSize, double[] outputVar, Int32 outputSize, Int32 moOfinputs);

        [DllImport("ExportKrigingFitMethods.dll", EntryPoint = "callKrigingFitEvalMethod", CallingConvention = CallingConvention.Cdecl)]
        public static extern double callKrigingFitEvalMethod(double[] inputs, int evalinputLength, int krigingObjectIndex);

        [DllImport("ExportKrigingFitMethods.dll", EntryPoint = "getSweepthetaValues", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr getSweepthetaValues(int size);

        [DllImport("ExportKrigingFitMethods.dll", EntryPoint = "freeAllocatedMemory", CallingConvention = CallingConvention.Cdecl)]
        public static extern void freeAllocatedMemory();

        private const double THRESHOLD_DISTANCE = 0.01f;

        double[,] _zArray = { };
        
        string[] m_InputColnames = { };
        Dictionary<string, int> m_KrigingFitObjValues = new Dictionary<string, int>();
        Dictionary<ContourParams,Tuple<string,double,string[]>> m_ContourPoints;
        protected void Page_Init(object sender, EventArgs e)
        {
            try
            {
                //not ajax postback
                if (String.IsNullOrEmpty(Request.Form[hddlInputChange.UniqueID]))
                {

                    m_InputColnames = Session["inputArraycolNames"].ToString().Split(new char[] { ',' });

                    ddlFirstInput.DataSource = m_InputColnames;
                    ddlFirstInput.DataBind();
                    ddlSecondInput.DataSource = m_InputColnames;//inputColnames.Where(val => val != ddlFirstInput.SelectedItem.ToString()).ToArray();
                    ddlSecondInput.DataBind();
                    if (ddlFirstInput.SelectedIndex == ddlSecondInput.SelectedIndex)
                    {
                        ddlSecondInput.SelectedIndex = ddlFirstInput.SelectedIndex + 1 == m_InputColnames.Length ? ddlFirstInput.SelectedIndex - 1 : ddlFirstInput.SelectedIndex + 1;
                    }
                    ddlOutput.DataSource = Session["outputArraycolNames"].ToString().Split(new char[] { ',' });
                    ddlOutput.DataBind();

                    SetInputVariables(toolScript.IsInAsyncPostBack);
                    ReDrawAreaChart.Enabled = true;
                }
                    //ajax postback as hidden var is set to some value in javascript,if it is from the ajax controls
                else
                {
                    m_InputColnames = Session["inputArraycolNames"].ToString().Split(new char[] { ',' });
                }
            }
            catch (Exception ex)
            {
                Log.WriteToLog(ex.Message, ex.StackTrace);
            }
        }
        
       
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                m_KrigingFitObjValues = (Session["krigingFitObjValues"] as Dictionary<string, int>);
                m_ContourPoints = new Dictionary<ContourParams, Tuple<string,double,string[]>>(new ContourParams.EqualityComparer());
                if (Session["contourPoints"] != null)
                {
                    m_ContourPoints = (Session["contourPoints"] as Dictionary<ContourParams, Tuple<string, double, string[]>>);
                }
                //not ajax postback
                if (!toolScript.IsInAsyncPostBack)
                {

                    DataTable dt = (DataTable)Session["dataSetResults"];
                    int xIndex = GetindexValueofInput(ddlFirstInput.SelectedValue);
                    int yIndex = GetindexValueofInput(ddlSecondInput.SelectedValue);
                    int modelIndex = (Session["krigingFitObjValues"] as Dictionary<string, int>)[ddlOutput.SelectedValue];
                    string[] contourRange = new string[6];
                    double rMax = 0.0;

                    double[] paramchartValues = new double[m_InputColnames.Length];
                    for (int j = 0; j < m_InputColnames.Length; j++)
                    {
                        if (j == xIndex || j == yIndex)
                        {
                            paramchartValues[j] = 0.0;
                        }
                        else
                        {
                            string paramValue = Utility.GetMaxValueofColumns(m_InputColnames[j]);
                            int roundOffvalue = paramValue.Substring(paramValue.IndexOf(".") + 1).Length / 2;
                            paramchartValues[j] = Math.Round(Convert.ToDouble(Utility.GetMinValueofColumns(m_InputColnames[j])) * Math.Pow(10, roundOffvalue)) / Math.Pow(10, roundOffvalue);
                        }

                    }
                    /*setting the xMax,xMin,yMin,yMax and the paramValues to the same values 
                     *  as the slider control values(rounding off the values in slider.js for the slider control)*/
                    string xValue = Utility.GetMaxValueofColumns(ddlFirstInput.SelectedValue);
                    int xroundOffvalue = xValue.Substring(xValue.IndexOf(".") + 1).Length / 2;
                    double xMin = Convert.ToDouble(Utility.GetMinValueofColumns(ddlFirstInput.SelectedValue));
                    double xMax = Convert.ToDouble(Utility.GetMaxValueofColumns(ddlFirstInput.SelectedValue));
                    if (xroundOffvalue != 0)
                    {
                        xMin = Math.Round(xMin * Math.Pow(10, xroundOffvalue)) / Math.Pow(10, xroundOffvalue);
                        xMax = Math.Round(xMax * Math.Pow(10, xroundOffvalue)) / Math.Pow(10, xroundOffvalue);

                    }
                    string yValue = Utility.GetMaxValueofColumns(ddlSecondInput.SelectedValue);
                    int yroundOffvalue = yValue.Substring(yValue.IndexOf(".") + 1).Length / 2;
                    double yMin = Convert.ToDouble(Utility.GetMinValueofColumns(ddlSecondInput.SelectedValue));
                    double yMax = Convert.ToDouble(Utility.GetMaxValueofColumns(ddlSecondInput.SelectedValue));
                    if (yroundOffvalue != 0)
                    {

                        yMin = Math.Round(yMin * Math.Pow(10, yroundOffvalue)) / Math.Pow(10, yroundOffvalue);
                        yMax = Math.Round(yMax * Math.Pow(10, yroundOffvalue)) / Math.Pow(10, yroundOffvalue);

                    }
                    //setting the parameters to the GetContour method
                    ContourParams contourParams = new ContourParams(modelIndex, xIndex, yIndex, m_InputColnames.Length, 6, xMin, xMax, yMin, yMax, paramchartValues);

                    PlotContourGraph(ref contourRange, ref rMax, contourParams);
                }
            }
            catch (Exception ex)
            {
                Log.WriteToLog(ex.Message, ex.StackTrace);
            }
        }

        public class ContourParams
        {
            public int ModelIndex { get; set; }
            public int XParamIndex { get; set; }
            public int YParamIndex { get; set; }
            public int TotalParams { get; set; }
            public int NumContours { get; set; }
            public double XMin { get; set; }
            public double XMax { get; set; }
            public double YMin { get; set; }
            public double YMax { get; set; }
            public double[] ParamArray { get; set; }
            public ContourParams(int modelIndex, int xindex, int yindex, int totalParams, int NumContours, double xmin, double xmax, double ymin, double ymax, double[] paramArray)
            {
                try
                {
                    this.ModelIndex = modelIndex;
                    this.XParamIndex = xindex;
                    this.YParamIndex = yindex;
                    this.TotalParams = totalParams;
                    this.NumContours = NumContours;
                    this.XMin = xmin;
                    this.XMax = xmax;
                    this.YMin = ymin;
                    this.YMax = ymax;
                    this.ParamArray = paramArray;
                }
                catch (Exception ex)
                {
                    Log.WriteToLog(ex.Message, ex.StackTrace);
                }
            }
            /*overriding equals and gethashcode to contain object as the key 
            element in Dictionary*/
            public class EqualityComparer : IEqualityComparer<ContourParams>
            {
               
                public bool Equals(ContourParams x, ContourParams y)
                {
                    bool flag = true;
                    for (int i = 0; i < x.ParamArray.Length; i++)
                    {
                        if (i != x.XParamIndex && i != x.YParamIndex)
                        {
                            if (x.ParamArray[i] != y.ParamArray[i])
                            {
                                flag = false;
                                break;
                            }
                        }
                    }
                    return x.ModelIndex == y.ModelIndex && x.XMax == y.XMax && x.XMin == y.XMin && x.YMax == y.YMax && x.YMin == y.YMin && x.XParamIndex == y.XParamIndex && x.YParamIndex == y.YParamIndex && flag;
                }

                public int GetHashCode(ContourParams x)
                {
                    return x.ModelIndex.GetHashCode() + x.XMax.GetHashCode() + x.XMin.GetHashCode() + x.YMax.GetHashCode() + x.YMin.GetHashCode() + x.XParamIndex.GetHashCode() + x.YParamIndex.GetHashCode();
                }
            
            
            }
        
        }
       
        /*Dynamically generating the input variables and the slider control.Dynamically generated controls 
           will be lost on every postback.Recreate it on every postback*/
        private void SetInputVariables(bool IsAsynchPostBack)
        {
            // int counter = 0;
            try
            {
                dynamicaControlIDs.Value = string.Empty;
                //Temporary list having the input col name  
                List<string> tempinputCol = new List<string>();

                /*adding value to the dictionary from the hidden variable having the unique ids of 
                input variable with its values(slider values.has values only in case of postback)*/
                Dictionary<string, string> inputValues = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(Request.Form["dynamicHiddenInput"]))
                {
                    string[] chartValues = Request.Form["dynamicHiddenInput"].TrimEnd(new char[] { ',' }).Split(new char[] { ',' });

                    for (int k = 0; k < chartValues.Length; k++)
                    {
                        string[] keyValue = chartValues[k].Split(new char[] { '|' });
                        inputValues.Add(keyValue[0], keyValue[1]);
                    }
                }
                /* iterating through inputs to create slider controls for it*/
                for (int i = 0; i < m_InputColnames.Length; i++)
                {
                    /* creating a div for the slider control*/
                    tbdynamicControls.Controls.Add(new LiteralControl("<div style='height:35px;'>"));
                    tbdynamicControls.Controls.Add(new LiteralControl("<div style='width:40%;display:inline-block;'>"));
                    System.Web.UI.WebControls.Label lbInputs = new System.Web.UI.WebControls.Label();
                    /*switch case so that always the dropdown selected value will be in first and second position (slider controls) */ 
                    switch (i)
                    {
                       //always set range slider for the first ddl selected value
                        case 0:
                            if (ddlFirstInput.SelectedValue.Replace(" ","").Length <= Constant.MINLABELLENGTH)
                            {
                                lbInputs.Text = ddlFirstInput.SelectedValue;
                            }
                            else
                            {
                                lbInputs.Text = ddlFirstInput.SelectedValue.Substring(0, Constant.MINLABELLENGTH);
                                lbInputs.Text = lbInputs.Text + "..";
                            }
                            tempinputCol.Add(ddlFirstInput.SelectedValue);
                            lbInputs.CssClass = "dynLabel";
                            tbdynamicControls.Controls.Add(lbInputs);
                            tbdynamicControls.Controls.Add(new LiteralControl("</div>"));
                            tbdynamicControls.Controls.Add(new LiteralControl("<div style='display:inline-block;width:55%'>"));

                            System.Web.UI.HtmlControls.HtmlGenericControl divForSlider = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                            string divID = Regex.Replace(ddlFirstInput.SelectedValue, "[^a-zA-Z0-9_]+", "");
                            divForSlider.ID = "div" + divID;

                            //calculate the step value for the slider
                            double stepValue = (Convert.ToDouble(Utility.GetMaxValueofColumns(ddlFirstInput.SelectedValue)) - Convert.ToDouble(Utility.GetMinValueofColumns(ddlFirstInput.SelectedValue))) / 100;
                          
                            /*In case of ajax postback get the values stored in the hidden variable and set it 
                             * back to the slider control*/
                            if (IsAsynchPostBack)
                            {
                                if (inputValues.Count > 0)
                                {
                                    for (int h = 0; h < inputValues.Count; h++)
                                    {
                                        var item = inputValues.ElementAt(h);
                                        if (item.Key.ToLower() == ddlFirstInput.SelectedValue.ToLower())
                                        {
                                            divForSlider.Attributes.Add("value", item.Value);
                                            inputValues.Remove(item.Key);
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    divForSlider.Attributes.Add("value", Utility.GetMinValueofColumns(ddlFirstInput.SelectedValue) + ";" + Utility.GetMaxValueofColumns(ddlFirstInput.SelectedValue));
                                }

                                divForSlider.Attributes.Add("UniqueID", ddlFirstInput.SelectedValue);
                                tbdynamicControls.Controls.Add(divForSlider);
                                ScriptManager.RegisterStartupScript(Page, Page.GetType(), Guid.NewGuid().ToString(), "<script language='javascript'>multiSlider('" + divForSlider.ClientID + "','" + Utility.GetMinValueofColumns(ddlFirstInput.SelectedValue) + "','" + Utility.GetMaxValueofColumns(ddlFirstInput.SelectedValue) + "','" + stepValue + "');</script>", false);
                            }
                                //if not postback add min and max values and call the js function to create slider
                            else if (!IsAsynchPostBack)
                            {
                                divForSlider.Attributes.Add("value", Utility.GetMinValueofColumns(ddlFirstInput.SelectedValue) + ";" + Utility.GetMaxValueofColumns(ddlFirstInput.SelectedValue));
                                divForSlider.Attributes.Add("UniqueID", ddlFirstInput.SelectedValue);
                                tbdynamicControls.Controls.Add(divForSlider);
                               ScriptManager.RegisterStartupScript(Page, GetType(), Guid.NewGuid().ToString(), "<script>$(document).ready(function(){});</script>",false);
                                ScriptManager.RegisterStartupScript(Page, GetType(), Guid.NewGuid().ToString(), "<script>multiSlider('" + divForSlider.ClientID + "','" + Utility.GetMinValueofColumns(ddlFirstInput.SelectedValue) + "','" + Utility.GetMaxValueofColumns(ddlFirstInput.SelectedValue) + "','" + stepValue + "');</script>", false);
                            }
                           //set the clientId's to the hidden variable to get the slider values on post back
                            dynamicaControlIDs.Value = dynamicaControlIDs.Value + divForSlider.ClientID + ",";
                            break;

                            //always have range slider for the second ddl selected value
                        case 1:
                            if (ddlSecondInput.SelectedValue.Replace(" ","").Length <= Constant.MINLABELLENGTH)
                            {
                                lbInputs.Text = ddlSecondInput.SelectedValue;
                            }
                            else
                            {
                                lbInputs.Text = ddlSecondInput.SelectedValue.Substring(0, Constant.MINLABELLENGTH);
                                lbInputs.Text = lbInputs.Text + "..";
                            }
                            tempinputCol.Add(ddlSecondInput.SelectedValue);
                            lbInputs.CssClass = "dynLabel";
                            tbdynamicControls.Controls.Add(lbInputs);
                            tbdynamicControls.Controls.Add(new LiteralControl("</div>"));
                            tbdynamicControls.Controls.Add(new LiteralControl("<div style='display:inline-block;width:55%'>"));

                            System.Web.UI.HtmlControls.HtmlGenericControl divForSliderSecondInput = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                            string divIDSecondInput = Regex.Replace(ddlSecondInput.SelectedValue, "[^a-zA-Z0-9_]+", "");
                            divForSliderSecondInput.ID = "div" + divIDSecondInput;
                            double stepValueSecondInput = (Convert.ToDouble(Utility.GetMaxValueofColumns(ddlSecondInput.SelectedValue)) - Convert.ToDouble(Utility.GetMinValueofColumns(ddlSecondInput.SelectedValue))) / 100;

                             if (IsAsynchPostBack)
                            {
                                if (inputValues.Count > 0)
                                {
                                    for (int h = 0; h < inputValues.Count; h++)
                                    {
                                        var item = inputValues.ElementAt(h);
                                        if (item.Key.ToLower() == ddlSecondInput.SelectedValue.ToLower())
                                        {
                                           
                                            divForSliderSecondInput.Attributes.Add("value", item.Value);
                                            inputValues.Remove(item.Key);
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    divForSliderSecondInput.Attributes.Add("value", Utility.GetMinValueofColumns(ddlSecondInput.SelectedValue) + ";" + Utility.GetMaxValueofColumns(ddlSecondInput.SelectedValue));
                                }
                                divForSliderSecondInput.Attributes.Add("UniqueID", ddlSecondInput.SelectedValue);
                                tbdynamicControls.Controls.Add(divForSliderSecondInput);
                                ScriptManager.RegisterStartupScript(Page, Page.GetType(), Guid.NewGuid().ToString(), "<script language='javascript'>multiSlider('" + divForSliderSecondInput.ClientID + "','" + Utility.GetMinValueofColumns(ddlSecondInput.SelectedValue) + "','" + Utility.GetMaxValueofColumns(ddlSecondInput.SelectedValue) + "','" + stepValueSecondInput + "');</script>", false);
                            }

                            else if (!IsAsynchPostBack)
                            {
                                divForSliderSecondInput.Attributes.Add("value", Utility.GetMinValueofColumns(ddlSecondInput.SelectedValue) + ";" + Utility.GetMaxValueofColumns(ddlSecondInput.SelectedValue));
                                divForSliderSecondInput.Attributes.Add("UniqueID", ddlSecondInput.SelectedValue);
                                tbdynamicControls.Controls.Add(divForSliderSecondInput);
                                ScriptManager.RegisterStartupScript(Page,GetType(), Guid.NewGuid().ToString(), "<script>multiSlider('" + divForSliderSecondInput.ClientID + "','" + Utility.GetMinValueofColumns(ddlSecondInput.SelectedValue) + "','" + Utility.GetMaxValueofColumns(ddlSecondInput.SelectedValue) + "','" + stepValueSecondInput + "');</script>",false);
                            }
                            dynamicaControlIDs.Value = dynamicaControlIDs.Value + divForSliderSecondInput.ClientID + ",";
                            break;

                            //have only slider control for the inputs which is not there in the dropdowns
                        default:
                            if (m_InputColnames.ToList().Except(tempinputCol).First().Replace(" ","").Length <= Constant.MINLABELLENGTH)
                            {
                                lbInputs.Text = m_InputColnames.ToList().Except(tempinputCol).First();
                            }
                            else
                            {
                                lbInputs.Text = m_InputColnames.ToList().Except(tempinputCol).First().Substring(0, Constant.MINLABELLENGTH);
                                lbInputs.Text = lbInputs.Text + "..";
                            }

                            lbInputs.CssClass = "dynLabel";
                            tbdynamicControls.Controls.Add(lbInputs);
                            tbdynamicControls.Controls.Add(new LiteralControl("</div>"));
                            tbdynamicControls.Controls.Add(new LiteralControl("<div style='display:inline-block;width:55%'>"));

                            System.Web.UI.HtmlControls.HtmlGenericControl divForSliderOtherInputs = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                            string divIDOtherInputs = Regex.Replace(m_InputColnames.ToList().Except(tempinputCol).First(), "[^a-zA-Z0-9_]+", "");
                            divForSliderOtherInputs.ID = "div" + divIDOtherInputs;
                            double stepValueOtherInputs = (Convert.ToDouble(Utility.GetMaxValueofColumns(m_InputColnames.ToList().Except(tempinputCol).First())) - Convert.ToDouble(Utility.GetMinValueofColumns(m_InputColnames.ToList().Except(tempinputCol).First()))) / 100;
                            
                            if (IsAsynchPostBack)
                            {
                                if (inputValues.Count > 0)
                                {
                                    for (int h = 0; h < inputValues.Count; h++)
                                    {
                                        var item = inputValues.ElementAt(h);
                                        if (item.Key.ToLower() == m_InputColnames.ToList().Except(tempinputCol).First().ToLower())
                                        {
                                            divForSliderOtherInputs.Attributes.Add("value", item.Value);
                                            inputValues.Remove(item.Key);
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    divForSliderOtherInputs.Attributes.Add("value", Utility.GetMinValueofColumns(m_InputColnames.ToList().Except(tempinputCol).First()));
                                }

                                divForSliderOtherInputs.Attributes.Add("UniqueID", m_InputColnames.ToList().Except(tempinputCol).First());
                                tbdynamicControls.Controls.Add(divForSliderOtherInputs);
                                ScriptManager.RegisterStartupScript(Page, Page.GetType(), Guid.NewGuid().ToString(), "<script language='javascript'>multiSlider('" + divForSliderOtherInputs.ClientID + "','" + Utility.GetMinValueofColumns(m_InputColnames.ToList().Except(tempinputCol).First()) + "','" + Utility.GetMaxValueofColumns(m_InputColnames.ToList().Except(tempinputCol).First()) + "','" + stepValueOtherInputs + "');</script>", false);
                            }

                            else if (!IsAsynchPostBack)
                            {
                                divForSliderOtherInputs.Attributes.Add("value", Utility.GetMinValueofColumns(m_InputColnames.ToList().Except(tempinputCol).First()));
                                divForSliderOtherInputs.Attributes.Add("UniqueID", m_InputColnames.ToList().Except(tempinputCol).First());
                                tbdynamicControls.Controls.Add(divForSliderOtherInputs);
                                ScriptManager.RegisterStartupScript(Page, GetType(), Guid.NewGuid().ToString(), "<script>multiSlider('" + divForSliderOtherInputs.ClientID + "','" + Utility.GetMinValueofColumns(m_InputColnames.ToList().Except(tempinputCol).First()) + "','" + Utility.GetMaxValueofColumns(m_InputColnames.ToList().Except(tempinputCol).First()) + "','" + stepValueOtherInputs + "');</script>", false);
                            }
                            dynamicaControlIDs.Value = dynamicaControlIDs.Value + divForSliderOtherInputs.ClientID + ",";
                            tempinputCol.Add(m_InputColnames.ToList().Except(tempinputCol).First());
                            break;
                    }
                    tbdynamicControls.Controls.Add(new LiteralControl("</div>"));
                    tbdynamicControls.Controls.Add(new LiteralControl("</div>"));
                }
            }
            catch (Exception ex)
            {
                Log.WriteToLog(ex.Message, ex.StackTrace);
            }
        }

        //RSM View Button Click Event.Redraw the contours with different settings.
        public void btnClick_PlotContours(object sender, EventArgs e)
        {
            try
            {

                bool flag = true;
                string[] xAxisValues = new string[2];
                string[] yAxisValues = new string[2];

                /*Get the values of the slider control stored in the hidden variable*/
                string[] chartValues = Request.Form["dynamicHiddenInput"].TrimEnd(new char[] { ',' }).Split(new char[] { ',' });
                Dictionary<string, string> inputValues = new Dictionary<string, string>();
                for (int i = 0; i < chartValues.Length; i++)
                {
                    string[] keyValue = chartValues[i].Split(new char[] { '|' });
                    inputValues.Add(keyValue[0], keyValue[1]);
                }

                double[] paramchartValues = new double[m_InputColnames.Length];
                int xIndex = GetindexValueofInput(ddlFirstInput.SelectedValue);
                int yIndex = GetindexValueofInput(ddlSecondInput.SelectedValue);
                for (int j = 0; j < m_InputColnames.Length; j++)
                {
                    if (j == xIndex || j == yIndex)
                    {
                        //setting param values for the GetContours method
                        paramchartValues[j] = 0.0;
                    }
                   
                    /*getting the xMin,xMax,yMin,yMax and the paramvalues for 
                     * GetContours method*/
                    else if (flag)
                    {
                        flag = false;
                        for (int index = 0; index < inputValues.Count; index++)
                        {

                            var item = inputValues.ElementAt(index);
                             if (item.Value.IndexOf(";") == -1)
                            {
                                int indexforParamChartValues = GetindexValueofInput(item.Key);
                                paramchartValues[indexforParamChartValues] = Convert.ToDouble(item.Value);
                            }
                            else if (item.Value.IndexOf(";") != -1)
                            {

                                if (item.Key.ToLower() == ddlFirstInput.SelectedValue.ToLower())
                                {
                                    xAxisValues = item.Value.Split(new char[] { ';' });

                                }
                                else if (item.Key.ToLower() == ddlSecondInput.SelectedValue.ToLower())
                                {
                                    yAxisValues = item.Value.Split(new char[] { ';' });
                                }
                            }
                        }
                    }

                }
                if (m_InputColnames.Length == 2)
                {
                    for (int index = 0; index < inputValues.Count; index++)
                    {
                        var item = inputValues.ElementAt(index);

                        if (item.Key.ToLower() == ddlFirstInput.SelectedValue.ToLower())
                        {
                            xAxisValues = item.Value.Split(new char[] { ';' });

                        }
                        else if (item.Key.ToLower() == ddlSecondInput.SelectedValue.ToLower())
                        {
                            yAxisValues = item.Value.Split(new char[] { ';' });
                        }
                    }
                }
                    int krigingIndex = (Session["krigingFitObjValues"] as Dictionary<string, int>)[ddlOutput.SelectedValue];
                    string[] contourRange = new string[6];
                    double rMax=0.0;
                   
                    ContourParams ctParams = new ContourParams(krigingIndex, xIndex, yIndex, m_InputColnames.Length, 6,Convert.ToDouble(xAxisValues[0]), Convert.ToDouble(xAxisValues[1]), Convert.ToDouble(yAxisValues[0]), Convert.ToDouble(yAxisValues[1]),paramchartValues);

                    PlotContourGraph(ref contourRange, ref rMax, ctParams);

                    SetInputVariables(toolScript.IsInAsyncPostBack);
                    hddlInputChange.Value = "";
            }
            catch (Exception ex)
            {
                Log.WriteToLog(ex.Message, ex.StackTrace);
            }
        }

        private void PlotContourGraph(ref string[] contourRange, ref double rMax, ContourParams ctParams)
        {
            string contours = string.Empty;
            if (m_ContourPoints.Count > 0 && m_ContourPoints.ContainsKey(ctParams))
            {

                contours = m_ContourPoints[ctParams].Item1;
                rMax = m_ContourPoints[ctParams].Item2;
                contourRange = m_ContourPoints[ctParams].Item3;
            }
            else
            {
                contours = Utility.GetContours(ctParams, Constant.precisionPercent, contourRange, out rMax);
                m_ContourPoints.Add(ctParams, new Tuple<string, double, string[]>(contours, rMax, contourRange));
                Session["contourPoints"] = m_ContourPoints;
            }
            hfldRange.Value = string.Join(",", contourRange);
            hfldContours.Value = contours;
            hfldCNames.Value = BuildCNames(contourRange, rMax);
            JavaScriptSerializer jss1 = new JavaScriptSerializer();

            double[] cRanges = Array.ConvertAll(contourRange, element => Convert.ToDouble(element));
            Array.Clear(_zArray, 0, _zArray.Length);
            _zArray = JsonConvert.DeserializeObject<double[,]>(contours);

            ScriptManager.RegisterStartupScript(Page, GetType(), "PlotAreaGraph()", "<script>PlotNVD3AreaGraph('" + ddlFirstInput.SelectedValue + "','" + ddlSecondInput.SelectedValue + "','" + Utility.GetMinValueofColumns(ddlFirstInput.SelectedValue) + "','" + Utility.GetMaxValueofColumns(ddlFirstInput.SelectedValue) + "','" + Utility.GetMinValueofColumns(ddlSecondInput.SelectedValue) + "','" + Utility.GetMaxValueofColumns(ddlSecondInput.SelectedValue) + "')</script>", false);


        }
        
        private string BuildCNames(string[] contourRange, double rMax)
        {
            string cNames = string.Empty;
            for (int i = 1; i < contourRange.Length; i++)
            {
                cNames += ddlOutput.SelectedValue.Trim() + " < " + contourRange[i] + ",";
            }
            cNames += ddlOutput.SelectedValue.Trim() + " < " + rMax;
            return cNames;
        }
        //Returns the index of the input Column
        public int GetindexValueofInput(string colName)
        {
            
            return Array.IndexOf(m_InputColnames, colName);
        }

        /*InputDropDowns Onchange Event*/
        public void ddlInputChange(object sender, EventArgs e)
        {
            
            try
            {
                
                DropDownList x = (DropDownList)sender;
                string ID = x.ID;
                if (x.ID ==Constant.FIRSTINPUTDROPDOWNID)
                {
                    hddlInputChange.Value = "";
                    if (ddlFirstInput.SelectedIndex == ddlSecondInput.SelectedIndex)
                    {
                        ddlSecondInput.SelectedIndex = ddlFirstInput.SelectedIndex + 1 == m_InputColnames.Length ? ddlFirstInput.SelectedIndex - 1 : ddlFirstInput.SelectedIndex + 1;
                    }
                }
                else if (x.ID == Constant.SECONDINPUTDROPDOWNID)
                {
                    hddlInputChange.Value = "";
                    if (ddlFirstInput.SelectedIndex == ddlSecondInput.SelectedIndex)
                    {
                        ddlFirstInput.SelectedIndex = ddlSecondInput.SelectedIndex + 1 == m_InputColnames.Length ? ddlSecondInput.SelectedIndex - 1 : ddlSecondInput.SelectedIndex + 1;
                    }
                }
               
                SetInputVariables(toolScript.IsInAsyncPostBack);
                ReDrawAreaChart.Enabled = true;
                ReDrawAreaChart.Attributes.CssStyle.Add("opacity", "");
            }
            catch (Exception ex)
            {
                Log.WriteToLog(ex.Message, ex.StackTrace);
            }
        }
        public void ddlOutputChange(object sender, EventArgs e)
        {
            SetInputVariables(toolScript.IsInAsyncPostBack);
            ReDrawAreaChart.Enabled = true;
            ReDrawAreaChart.Attributes.CssStyle.Add("opacity", "");

        }

        
        /*Generate Exe button click event.Generate JSON File which contains the selected
              input,output and thetaValues */
        protected void btnClick_GenerateJsonFile(object sender, EventArgs e)
        {
            try
            {
                string jsonfolderPath = ConfigurationManager.AppSettings["jsonFilePath"];
                string jsonfileName = Constant.JSONFILENAME;
                string filepathtoJSON = HttpContext.Current.Server.MapPath(@"~/" + jsonfolderPath + jsonfileName);
                if (File.Exists(filepathtoJSON))
                {
                    File.Delete(filepathtoJSON);
                }
               
                using (FileStream fs = File.Open(filepathtoJSON, FileMode.CreateNew))
                using (StreamWriter sw = new StreamWriter(fs))
                using (JsonWriter jw = new JsonTextWriter(sw))
                {
                    jw.Formatting = Formatting.Indented;
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(jw, Utility.GetDataForJSONCreation());
                }

                using (StreamReader reader = new StreamReader(filepathtoJSON))
                {
                    string json = reader.ReadToEnd();
                    Utility.WriteModelConfig(Constant.JSONDLLNAME, Constant.JSONRESOURCEFILENAME,Constant.JSONRESOURCENAME, json);
                }
              
                if (m_ContourPoints.Count > 0)
                {
                    Session["saveRSMViewerSettings"] = m_ContourPoints.ToArray();
                }
                Response.Redirect("DownloadExe.aspx",false);
            }
            catch (Exception ex)
            {
                Log.WriteToLog(ex.Message, ex.StackTrace);
            }
        }
        protected void btnFinish_Click(object sender, EventArgs e)
        {
            freeAllocatedMemory();
            Session.RemoveAll();
            Response.Redirect("Home.aspx",false);
        }

      
    }
}
