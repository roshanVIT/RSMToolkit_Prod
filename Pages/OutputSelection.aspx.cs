using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using System.Web.UI.HtmlControls;
using System.Data;
using AGI.Logger;
using System.Configuration;
using RSMTool.App_Code;
using System.Runtime.InteropServices;
using System.Threading;
using System.Reflection;

namespace RSMTool.Pages
{
    public partial class OutputSelection : System.Web.UI.Page
    {
        [DllImport("ExportKrigingFitMethods.dll", EntryPoint = "freeAllocatedMemory", CallingConvention = CallingConvention.Cdecl)]
        public static extern void freeAllocatedMemory();

        protected void Page_Init(object sender, EventArgs e)
        {
            try
            {
                if (Request.QueryString["selectedCols"] != null)
                {
                    Session["removeaddedNodes"] = "true";
                    hiddenSiteMap.Value = "false";
                    Session["outputheaderClinetIDs"] = Request.QueryString["selectedCols"].TrimEnd(new char[] { ',' }); 
                   
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
                if (!IsPostBack)
                {
                    if (Session["dataSetResults"] != null)
                    {
                        GridViewFile.DataSource = Session["dataSetResults"];
                        GridViewFile.DataBind();
                    }
                    if (Session["outputheaderClinetIDs"] != null)
                    {
                        if (Request.QueryString["selectedCols"] == null)
                        {
                            hiddenSiteMap.Value = "true";
                        }
                        else
                        {
                            PropertyInfo isreadonly = typeof(System.Collections.Specialized.NameValueCollection).GetProperty("IsReadOnly", BindingFlags.Instance | BindingFlags.NonPublic);
                         
                            isreadonly.SetValue(this.Request.QueryString, false, null);
                        
                            this.Request.QueryString.Remove("selectedCols");
                        }
                        Page.ClientScript.RegisterStartupScript(this.GetType(), "CallFunction", "FillColumnColor('" + Session["outputheaderClinetIDs"] + "','" + "true" + "')", true);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteToLog(ex.Message, ex.StackTrace);
            }
        }

        /*Getting the selected outputs.Keep it in session variable.Keep the clientId's of
         * the selected column header name in the session */
        protected void btnEval_Clicked(object sender, EventArgs e)
        {
           
            try
            {
                Session["outputheaderClinetIDs"] = ophidColumnIds.Value.ToString().TrimEnd(new char[] { ',' });
                Session["outputArraycolNames"] = hiddenColName.Value.ToString().TrimEnd(new char[] { ',' });
                Response.Redirect("RSMPreview.aspx",false);
                    
            }
           
            catch (Exception ex)
            {
                Log.WriteToLog(ex.Message, ex.StackTrace);
            }
        }

        /*Adding the GridView header on click event to get the col header for output */
        protected void GridViewOutputFile_RowDataBound(object sender, EventArgs e)
        {
            try
            {
                GridView gridView = (GridView)sender;
                TableCell[] arrHeaderColumnID = new TableCell[gridView.HeaderRow.Cells.Count];
                for (int i = 0; i < gridView.HeaderRow.Cells.Count; i++)
                {
                    arrHeaderColumnID[i] = gridView.HeaderRow.Cells[i];
                    arrHeaderColumnID[i].Attributes.Add("onclick", "FillColumnColor('" + arrHeaderColumnID[i].ClientID +"','"+false+ "')");
                }
            }
            catch (Exception ex)
            {
                Log.WriteToLog(ex.Message, ex.StackTrace);
            }
        }

        protected void GridViewFile_PageIndexChanged(object sender, GridViewPageEventArgs  e)
        {
            try
            {
                GridViewFile.PageIndex = e.NewPageIndex;
                GridViewFile.DataSource = Session["dataSetResults"];
                GridViewFile.DataBind();
            }
            catch (Exception ex)
            {
                Log.WriteToLog(ex.Message, ex.StackTrace);
            }
        }
       

    }
}