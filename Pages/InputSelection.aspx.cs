using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using System.Data;
using AGI.Logger;
using System.Configuration;
using System.Runtime.InteropServices;
using System.Reflection;

namespace RSMTool.Pages
{
    
    public partial class InputSelection : System.Web.UI.Page
    {
        [DllImport("ExportKrigingFitMethods.dll", EntryPoint = "freeAllocatedMemory", CallingConvention = CallingConvention.Cdecl)]
        public static extern void freeAllocatedMemory();

        //call Init method to check whether the refresh request is from javascript.If it is from javascript remove the nodes from Sitemapnode.
        protected void Page_Init(object sender, EventArgs e)
        {
            try
            {
                if (Request.QueryString["selectedCols"] != null)
                {
                    Session["removeaddedNodes"] = "true";
                    hiddenSiteMap.Value = "false";
                    Session["outputheaderClinetIDs"] = null;
                    Session["outputArraycolNames"] = null;
                    Session["krigingFitObjValues"] = null;
                    Session["thetaValues"] = null;
                    Session["contourPoints"] = null;
                    Session["headerClinetIDs"] = Request.QueryString["selectedCols"].TrimEnd(new char[] { ',' });
                    freeAllocatedMemory();
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
                    if (Session["headerClinetIDs"] != null)
                    {
                       // Session["isSiteMapClick"] = "true";
                        if (Request.QueryString["selectedCols"] == null)
                        {
                            hiddenSiteMap.Value = "true";
                        }
                        else
                        {
                            PropertyInfo isreadonly = typeof(System.Collections.Specialized.NameValueCollection).GetProperty("IsReadOnly", BindingFlags.Instance | BindingFlags.NonPublic);
                            // make collection editable
                            isreadonly.SetValue(this.Request.QueryString, false, null);
                            // remove
                            this.Request.QueryString.Remove("selectedCols"); 
                        }
                       // ScriptManager.RegisterStartupScript(Page, GetType(), Guid.NewGuid().ToString(), "<script>FillColumnColor('" + Session["headerClinetIDs"] + "','" + "true"+ "');</script>", false);
                        Page.ClientScript.RegisterStartupScript(this.GetType(), "CallFunction", "FillColumnColor('" + Session["headerClinetIDs"] + "','" + "true"+"')", true);


                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteToLog(ex.Message, ex.StackTrace);
            }
        }
        protected void GridViewFile_PageIndexChanged(object sender, GridViewPageEventArgs e)
        {
            try
            {
                GridViewFile.PageIndex = e.NewPageIndex;
                GridViewFile.DataSource = Session["dataSetResults"];
                GridViewFile.DataBind();
            }
            catch(Exception ex)
            {
                Log.WriteToLog(ex.Message, ex.StackTrace);
            }
        }
        /*Store the selected input arrays and rearranging in the sequence to be sent to KrigingFit class
         *  and store it in a session*/
        protected void btnNext_Clicked(object sender, EventArgs e)
        {
            try
            {
                DataTable dt = (DataTable)Session["dataSetResults"];
                if (Session["headerClinetIDs"] != null)
                {
                    if (!Enumerable.SequenceEqual(hidColumnIds.Value.ToString().TrimEnd(new char[] { ',' }).Split(new char[] { ',' }), Session["headerClinetIDs"].ToString().Split(new char[] { ',' })))
                    {
                        Session["thetaValues"] = null;
                        Session["krigingFitObjValues"] = null;
                        Session["contourPoints"] = null;
                        freeAllocatedMemory();
                    }
                }
                Session["inputArraycolNames"] = hiddenColName.Value.ToString().TrimEnd(new char[] { ',' });
                Session["headerClinetIDs"] = hidColumnIds.Value.ToString().TrimEnd(new char[] { ',' });
                Response.Redirect("OutputSelection.aspx",false);
            }
            catch (Exception ex)
            {
                Log.WriteToLog(ex.Message, ex.StackTrace);
            }
        }
       

        /*Adding the GridView header on click event to get the col header for input */
        protected void GridViewFile_RowDataBound(object sender, EventArgs e)
        {
            try
            {
                GridView gridView = (GridView)sender;

                TableCell[] arrHeaderColumnID = new TableCell[gridView.HeaderRow.Cells.Count];
                for (int i = 0; i < gridView.HeaderRow.Cells.Count; i++)
                {
                    // hidColumnIds.Value += gridView.HeaderRow.Cells[i].ClientID + ",";

                    arrHeaderColumnID[i] = gridView.HeaderRow.Cells[i];
                    arrHeaderColumnID[i].Attributes.Add("onclick", "FillColumnColor('" + arrHeaderColumnID[i].ClientID + "','" + false + "')");
                }

            }
            catch (Exception ex)
            {
                Log.WriteToLog(ex.Message, ex.StackTrace);
            }
           
            
        }
    }
}