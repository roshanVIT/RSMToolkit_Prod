using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Ionic.Zip;
using System.IO;
using AGI.Logger;
using System.Runtime.InteropServices;
using System.Configuration;

namespace RSMTool.Pages
{
    public partial class DownloadExe : System.Web.UI.Page
    {
        [DllImport("ExportKrigingFitMethods.dll", EntryPoint = "freeAllocatedMemory", CallingConvention = CallingConvention.Cdecl)]
        public static extern void freeAllocatedMemory();

        protected void Page_Load(object sender, EventArgs e)
        {

        }
        /// <summary>
        /// function invoked on click of generate dll link
        /// </summary>

        protected void lnk_DownloadFile(object sender, EventArgs e)
        {
            try
            {
                string pathtoDLL = AppDomain.CurrentDomain.BaseDirectory+@"\\Assemblies";
                              
                Response.ClearContent();
                Response.AddHeader("Content-Disposition", "attachment; filename=KrigingDLLs");
                Response.ContentType = "application/zip";
                using (ZipFile zip = new ZipFile())
                {
                    string[] files = Directory.GetFiles(pathtoDLL);
                    zip.AddFiles(files, "");
                    zip.Save(Response.OutputStream);
                }
               
            }
            catch (Exception ex)
            {
                Log.WriteToLog(ex.Message, ex.StackTrace);
            }
        }

        /// <summary>
        /// function invoked on click of finish button
        /// </summary>
       
        protected void btnClick_RedirectToExcelReader(object sender, EventArgs e)
        {
            freeAllocatedMemory();
            Session.RemoveAll();
            Response.Redirect("Home.aspx",false);
        }

        /// <summary>
        ///function invoked onclick of export session button 
        /// </summary>

        protected void btnClick_DownloadJSON(object sender, EventArgs e)
        {
            string jsonfolderPath = ConfigurationManager.AppSettings["jsonFilePath"];
            string jsonfileName = Constant.JSONFILENAME;
            System.Web.HttpResponse response = System.Web.HttpContext.Current.Response;
            response.ClearContent();
            response.Clear();
            response.ContentType = "text/plain";
            response.AddHeader("Content-Disposition", "attachment; filename="+jsonfileName+";");
            response.TransmitFile(Server.MapPath(@"~/" + jsonfolderPath + jsonfileName));
            response.Flush();
            response.End();
        }
    }
}
