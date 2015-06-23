<%@ Page Title="Home Page"   Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true"
    CodeBehind="Home.aspx.cs" Inherits="RSMTool.Home" %>
    <asp:Content ID="HeadContent" runat="server" ContentPlaceHolderID="HeadContent">
        <link href="../Styles/enhanced.css" rel="stylesheet" type="text/css" />
        <script src="../Scripts/jQuery.fileinput.js" type="text/javascript"></script>
        <script type="text/javascript">
            $(function() {
                $('#<%= excelFileUpload.ClientID %>').customFileInput();
                $('#<%= fileImport.ClientID %>').customFileInput();
            });
        
            //function to show validation message during upload of json file.
        function DisplayValidationMessage() {
            $('#<%=validateFileImport.ClientID%>').fadeIn().delay(10000).fadeOut();
            $('#<%=validateFileImport.ClientID%>')[0].innerHTML = "Please upload a valid Json file";
        }

            //function to check the format of the uploading file - json file
            function ValidateFilename(formid) {
                var validFilesTypes = ["json", "JSON"];
                var uploadcontrol = $('#<%= fileImport.ClientID %>').val();
                var extension = uploadcontrol.substring(uploadcontrol.lastIndexOf(".") + 1, uploadcontrol.length).toLowerCase();
                var isValidFile = false;
                for (var i = 0; i < validFilesTypes.length; i++) {
                    if (extension == validFilesTypes[i]) {
                        
                        isValidFile = true;
                        break;
                    }
                }

                if (isValidFile) {
                    formid.submit();
                }
                else {
                    $('#<%=validateFileImport.ClientID%>').fadeIn().delay(10000).fadeOut();
                    $('#<%=validateFileImport.ClientID%>')[0].innerHTML = "Please upload only .json file";
                    return false;
                }
            }

            //function to check the format of the uploading file - excel file
            function UploadFileNow(formid) {
                var validFilesTypes = ["xls","xlsx","csv","XLS","XLSX","CSV"];
                var uploadcontrol = $('#<%= excelFileUpload.ClientID %>').val();
                var extension = uploadcontrol.substring(uploadcontrol.lastIndexOf(".") + 1, uploadcontrol.length).toLowerCase()
                var isValidFile = false;
                for (var i = 0; i < validFilesTypes.length; i++) {
                    if (extension == validFilesTypes[i]) {
                        isValidFile = true;
                        break;
                    }
                }

                if (isValidFile) {
                    $('#<%=btNext.ClientID%>').removeAttr("disabled", "disabled");
                    $('#<%=btNext.ClientID%>').css('opacity', '');
                    formid.submit();
                    }
                    else {
                        $('#ValidateFile').fadeIn().delay(10000).fadeOut();
                        $('#ValidateFile')[0].innerHTML = "Only .xls,.xlsx and .csv files are allowed";
                        $('#<%=btNext.ClientID%>').attr("disabled", "disabled");
                        $('#<%=btNext.ClientID%>').css('opacity', '0.5');
                        $('#<%= excelFileUpload.ClientID %>').val("");
                        return false;
                    }
        

                

            };
        </script>
  </asp:Content>
<asp:Content ID="BodyContent"  runat="server" ContentPlaceHolderID="MainContent">
    <table border="0" cellpadding="5px" cellspacing="5px">
    <tr>
    <td>
    <asp:Label ID="lbltext" runat="server" CssClass="label" Font-Italic="true">Select only .csv,.xls or .xlsx file </asp:Label><br />
    </td>
    <asp:Label ID="lbfilelocation" runat="server" Visible="false" CssClass="label" Font-Italic="true">Select the corresponding data file which was previously there in </asp:Label><br />
   
    <asp:Panel ID="pnlImport" runat="server" Visible="false">
<asp:Label ID="lbjsonImport" runat="server" CssClass="label" Font-Italic="true">Select only .json file to import your session </asp:Label><br /><br />
<asp:FileUpload ID="fileImport" accept=".json"  onchange="ValidateFilename(this.form);" runat="server" /><br /><br />
<asp:Label ID="lbcanceljsonFileUpload" Font-Italic="true" runat="server" ForeColor="White"><asp:LinkButton ID="lnkcanceljsonFileUpload" runat="server" ForeColor="Lime" Font-Italic="true" CssClass="label" OnClick="CanceljsonFileUpload">Click here</asp:LinkButton> to upload a new data file if you do not have a json file</asp:Label>

<div id="validateFileImport"  runat="server" style="color:Red;font-weight:bold"></div>
</asp:Panel>

    </tr>
<tr>
<td>
 <asp:FileUpload ID="excelFileUpload"  accept=".csv,.xls,.xlsx" onchange="UploadFileNow(this.form);"  runat="server" /></td>
 
 <td>
 </td>
 <td><div id="ValidateFile" style="color:Red;font-weight:bold"></div>
 </td>

</tr>
<tr><td><asp:Label ID="lbCancelImportFileUpload" runat="server" Visible="false" ForeColor="White" Font-Italic="true"><asp:LinkButton ID="lnkCancelImportFileUpload" runat="server"  ForeColor="Lime" Font-Italic="true" CssClass="label" Onclick="CancelImportFileUpload">Click here</asp:LinkButton> to upload a new data file if the old file is not there</asp:Label></td></tr>
<tr>
<td><asp:LinkButton ID="lnkImport" OnClick="showjsonFileUploadControl" runat="server" ForeColor="Lime" Font-Italic="true" CssClass="label">Click here</asp:LinkButton><span id="spanImport" runat="server" style="color:White; font-style:italic">&nbsp;to import the session for uploaded file</span></td>
</tr>

</table>
<table border="0" cellpadding="5px" cellspacing="1px">
<tr>
<td>
<asp:Label ID="lblWorksheet" runat="server" CssClass="label" Visible="false" Font-Italic="true">Select the worksheet</asp:Label></td>
<td>
<asp:DropDownList ID="ddlWorsheetList" runat="server" AutoPostBack="true" OnSelectedIndexChanged="BindGrid" Visible="false"></asp:DropDownList>
</td></tr>
</table>
<div>
 <asp:Label ID="lbFilename"  runat="server" CssClass="lbFileName"></asp:Label>
  <asp:GridView ID="GridViewFile"  ShowFooter="true" HeaderStyle-Width="100%" FooterStyle-CssClass="pgr"
         runat="server" PagerStyle-CssClass="gridview"  Width="100%"
    AlternatingRowStyle-CssClass="alt"
        EmptyDataText="No records" 
        CssClass="mGrid">
                  </asp:GridView>
                  </div>
                  
<div class="footer">

<asp:Button ID="btNext" Text="Next" Visible="false" runat="server" CssClass="btnNext" OnClick="btnNext_Clicked" />
</div>
</asp:Content>
