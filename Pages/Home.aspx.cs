using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Data;
using System.Configuration;
using System.Globalization;
using Excel;
using System.Text;
using AGI.Logger;
using System.Runtime.InteropServices;
using RSMTool.App_Code;
using Newtonsoft.Json;

namespace RSMTool
{
    public partial class Home : System.Web.UI.Page
    {
        [DllImport("ExportKrigingFitMethods.dll", EntryPoint = "freeAllocatedMemory", CallingConvention = CallingConvention.Cdecl)]
        public static extern void freeAllocatedMemory();

        IExcelDataReader m_ExcelReader;
        bool isExcelhasMultipleSheets = false;
      
        DataSet m_ReadExcelresults;

        /// <summary>
        /// Page Load logic goes here
        /// </summary>
        
        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack && excelFileUpload.PostedFile != null && (!string.IsNullOrEmpty(excelFileUpload.FileName)))
            {
                //Postback due to file upload. Read the excel and load data for preview.
                LoadPreviewData();
                lbFilename.Text = excelFileUpload.FileName;
                Session["fileName"] = excelFileUpload.FileName;

            }
            else if (excelFileUpload.PostedFile == null && Session["dataSetResults"] != null)
            {
                //Getting the data from session
                DataTable dtSession = (DataTable)Session["dataSetResults"];
                var records = dtSession.AsEnumerable().Take(Convert.ToInt16(ConfigurationManager.AppSettings["Rows"])).CopyToDataTable();

                GridViewFile.DataSource = records;
                GridViewFile.DataBind();
                btNext.Visible = true;

                lbFilename.Text = Session["fileName"].ToString();
            }
            if (excelFileUpload.PostedFile == null && fileImport.PostedFile != null)
            {
                //Fresh Import of file
                try
                {
                    if (fileImport.PostedFile.FileName.Length > 0)
                    {
                        ReadJsonFile();
                    }
                }
                catch (Exception ex)
                {
                    Log.WriteToLog(ex.Message, ex.StackTrace);

                }
            }
            
        }
 
       
        
        /// <summary>
        /// /*ExcelFileReader Button Click Event*/
        /// </summary>
      protected void LoadPreviewData()
        {
            string filePath = string.Empty;
            try
            {
               if (excelFileUpload.PostedFile.FileName.Length > 0)
                {
                        freeAllocatedMemory();
                        if (Session["jsonfilePath"] == null)
                        {
                            Session.RemoveAll();
                        }
                        else
                        {
                            LoadAllSitemapNodes();
                            lbfilelocation.Visible = false;
                            Session["jsonfilePath"] = null;
                            lbCancelImportFileUpload.Visible = false;
                            lnkCancelImportFileUpload.Visible = false;
                            lnkImport.Visible = true;
                            spanImport.Visible = true;
                            lbltext.Visible = true;

                            lbfilelocation.Text = "Select the corresponding data file which was previously there in ";
                      
                        }
                    var fileName = excelFileUpload.FileName;
                    var fileExt = Path.GetExtension(fileName);
                    Session["filePath"] = excelFileUpload.PostedFile.FileName;
                    //Download a copy of selected file for further processing
                    string FolderPath = ConfigurationManager.AppSettings["FolderPath"];
                    filePath = HttpContext.Current.Server.MapPath(@"~/" + FolderPath + fileName);
                    if(File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                    excelFileUpload.SaveAs(filePath);

                    //Read the downloaded file for displaying the preview of worksheet
                    ReadExcelToGrid(filePath, fileExt);
                   
                }
            }
            catch (Exception ex)
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
                Log.WriteToLog(ex.Message, ex.StackTrace);
            }
        }

        /*Binding the Excel Data if it has more than one worksheet*/
        /*Dropdown onSelectIndexChanged event*/
        protected void BindGrid(object sender, EventArgs e)
        {
            try
            {
                GridViewFile.DataSource = null;
                m_ReadExcelresults = (DataSet)Session["dataSetResults"];
             
                var records = m_ReadExcelresults.Tables[ddlWorsheetList.SelectedValue].AsEnumerable().Take(Convert.ToInt16(ConfigurationManager.AppSettings["Rows"])).CopyToDataTable();
                GridViewFile.DataSource = records;
                GridViewFile.DataBind();
                ddlWorsheetList.Visible = true;
                lblWorksheet.Visible = true;
            }
            catch (Exception ex)
            {
                Log.WriteToLog(ex.Message, ex.StackTrace);
            }
        }

        /*Read Excel ,csv file and binds the dataset to griview ,Also stores the dataset in Session
         * for future use*/
        private void ReadExcelToGrid(string path, string extension)
        {
            try
            {
                btNext.Visible = true;
                switch (extension)
                {
                    case ".xls":
                        ProcessExcelData(path, false);
                        break;

                    case ".xlsx":
                        ProcessExcelData(path, true);
                        break;

                    case ".csv":
                        ProcessCSVData(path);
                        break;
                    default: File.Delete(path);
                        break;
                }
            }
            catch (Exception ex)
            {
                if(File.Exists(path))
                File.Delete(path);
                Log.WriteToLog(ex.Message, ex.StackTrace);
            }
        }


        /// <summary>
        /// Process data for CSV File types
        /// </summary>
      
        protected void ProcessCSVData(string filePath)
        {
            try
            {
                string[] data = File.ReadAllLines(filePath);
                if (data == null || data[0] == null)
                {
                    Log.WriteToLog("No data in CSV","");
                    return;
                }

                ddlWorsheetList.Visible = false;
                lblWorksheet.Visible = false;
                DataTable fulldataDataTable = new DataTable();
                string[] col = data[0].Split(',');

                //First load all the columns
                foreach (string s in col)
                {
                    fulldataDataTable.Columns.Add(s, typeof(string));
                }

                //Load all the rows and keep it in session for future usage
                for (int i = 1; i < data.Length; i++)
                {
                    string[] row = data[i].Split(',');
                    fulldataDataTable.Rows.Add(row);
                }
                Session["dataSetResults"] = fulldataDataTable;
               var records = fulldataDataTable.AsEnumerable().Take(Convert.ToInt16(ConfigurationManager.AppSettings["Rows"])).CopyToDataTable();
                GridViewFile.DataSource = records;
                GridViewFile.DataBind();
                File.Delete(filePath);
            }
            catch (Exception ex)
            {
                if (File.Exists(filePath))
                File.Delete(filePath);
                Log.WriteToLog(ex.Message, ex.StackTrace);
            }
        }

        /// <summary>
        /// Process data from Excel type files
        /// </summary>
       protected void ProcessExcelData(string filePath, bool isXlsx)
        {
            try
            {
                using (FileStream stream = File.Open(filePath, FileMode.Open))
                {
                    if (isXlsx)
                    {
                        m_ExcelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                    }
                    else
                    {
                        m_ExcelReader = ExcelReaderFactory.CreateBinaryReader(stream);
                    }

                    stream.Close();
                    m_ExcelReader.IsFirstRowAsColumnNames = true;
                    m_ReadExcelresults = m_ExcelReader.AsDataSet();
                    List<string> namesList = new List<string>();
                    if (m_ReadExcelresults.Tables.Count > 1)
                    {
                        isExcelhasMultipleSheets = true;
                        for (int i = 0; i < m_ReadExcelresults.Tables.Count; i++)
                        {
                            namesList.Add(m_ReadExcelresults.Tables[i].TableName);
                        }

                        ddlWorsheetList.Visible = true;
                        lblWorksheet.Visible = true;
                        ddlWorsheetList.DataSource = namesList;
                        ddlWorsheetList.DataBind();
                    }
                    else
                    {
                        ddlWorsheetList.Visible = false;
                        lblWorksheet.Visible = false;
                    }
                    var records=(object)"";
                    if (string.IsNullOrEmpty(ddlWorsheetList.SelectedValue))
                    {
                        Session["dataSetResults"] = m_ReadExcelresults.Tables[0];
                        records = m_ReadExcelresults.Tables[0].AsEnumerable().Take(Convert.ToInt16(ConfigurationManager.AppSettings["Rows"])).CopyToDataTable();
                    }
                    else
                    {
                        records = m_ReadExcelresults.Tables[ddlWorsheetList.SelectedValue].AsEnumerable().Take(Convert.ToInt16(ConfigurationManager.AppSettings["Rows"])).CopyToDataTable();
                    }
                    GridViewFile.DataSource = records;
                    GridViewFile.DataBind();

                    File.Delete(filePath);

                }
            }
            catch (Exception ex)
            {
                if(File.Exists(filePath))
                File.Delete(filePath);
                Log.WriteToLog(ex.Message, ex.StackTrace);
            }
 
        }

        //Show json Fileupload control and hide the Excel,csv Fileupload control
        protected void showjsonFileUploadControl(Object sender, EventArgs e)
        {
            lbltext.Visible = false;
            excelFileUpload.Visible = false;
            pnlImport.Visible = true;
            lnkImport.Visible = false;
            spanImport.Visible = false;
        
        }
        //Read the uploaded json file and set the session 
        public void ReadJsonFile()
        {
                var fileName = fileImport.FileName;
                string FolderPath = ConfigurationManager.AppSettings["FolderPath"];
                string jsonfilePath = HttpContext.Current.Server.MapPath(@"~/" + FolderPath + fileName);
                Session["jsonfilePath"] = jsonfilePath;
                if (File.Exists(jsonfilePath))
                {
                    File.Delete(jsonfilePath);
                }
                fileImport.SaveAs(jsonfilePath);
                bool isValidjsonFile = Utility.SetSessionVariables(jsonfilePath, lbfilelocation);
                if (isValidjsonFile == false)
                {
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "CallFunction", "DisplayValidationMessage();", true);
                    if (File.Exists(jsonfilePath))
                    {
                        File.Delete(jsonfilePath);
                    }
                }
                else
                {
                    pnlImport.Visible = false;
                    spanImport.Visible = false;
                    lbfilelocation.Visible = true;
                    lnkCancelImportFileUpload.Visible = true;
                    lbCancelImportFileUpload.Visible = true;
                    excelFileUpload.Visible = true;
                    if (File.Exists(jsonfilePath))
                    {
                        File.Delete(jsonfilePath);
                    }
                }
            
        }
       /*Redirecting page to select the inputs. This will be triggered when the user clicks on Next button in the screen*/
        protected void btnNext_Clicked(Object sender, EventArgs e)
        {
            try
            {
                if (isExcelhasMultipleSheets)
                {
                    Session["dataSetResults"] = m_ReadExcelresults.Tables[ddlWorsheetList.SelectedValue];
                }
               Response.Redirect("InputSelection.aspx",false);
                
            }
            catch (Exception ex)
            {
                Log.WriteToLog(ex.Message, ex.StackTrace);
            }
        }

        /*Load all site maps for importing the krigingfit model*/
        protected void LoadAllSitemapNodes()
        {
            SiteMapNode tempNode = SiteMap.CurrentNode; 
            SiteMapNode childNode = null;
            Stack<SiteMapNode> loadAllNodeStack = new Stack<SiteMapNode>();
            while (tempNode.HasChildNodes)
            {
                childNode = new SiteMapNode(tempNode.Provider, tempNode.ChildNodes[0].Url.ToLower(), tempNode.ChildNodes[0].Url, tempNode.ChildNodes[0].Title);
                loadAllNodeStack.Push(childNode);
                tempNode = tempNode.ChildNodes[0];
            }
            loadAllNodeStack.Push(SiteMap.RootNode);
            Session["loadallNodes"] = loadAllNodeStack.ToArray();
            
        }

        /*Cancel File upload after loading json Data,Upload new data file*/
        protected void CancelImportFileUpload(object sender, EventArgs e)
        {
            Session.RemoveAll();
            lbltext.Visible = true;
            lnkImport.Visible = true;
            spanImport.Visible = true;
            lbCancelImportFileUpload.Visible = false;
            lnkCancelImportFileUpload.Visible = false;
            lbfilelocation.Visible = false;
            lbfilelocation.Text = "Select the corresponding data file which was previously there in ";
            //lbjsonImport.Text = "";
        }

        /*Cancel Json file upload control,Upload new data File*/
        protected void CanceljsonFileUpload(object sender, EventArgs e)
        {
            pnlImport.Visible = false;
            lbltext.Visible = true;
            excelFileUpload.Visible = true;
            lnkImport.Visible = true;
            spanImport.Visible = true;
        }
    }
}
