<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DownloadExe.aspx.cs" MasterPageFile="~/Site.master"  Inherits="RSMTool.Pages.DownloadExe" %>
<asp:Content ID="HeadContent" runat="server" ContentPlaceHolderID="HeadContent">
     <link href="../Styles/enhanced.css" rel="stylesheet" type="text/css" />
    <script src="../Scripts/jquery-1.2.6.js" type="text/javascript"></script>
   </asp:Content>
    <asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
    <div class="content">
  <asp:Label ID="lblDownload" runat="server" CssClass="label" Font-Italic="true">Exe has been generated.<asp:LinkButton ID="lnkDownload" runat="server" CssClass="label" Font-Italic="true" ForeColor="Lime" OnClick="lnk_DownloadFile">Click here</asp:LinkButton>&nbsp;to download the file</asp:Label><br />
   
    </div>
    <div class="downloadFooter">
    <asp:Button ID="btnFinish" Text="Finish" CssClass="btnNext" runat="server" OnClick="btnClick_RedirectToExcelReader" />
    &nbsp;&nbsp;
    <asp:Button ID="btnExport" Text="Export Session" CssClass="btnNext" runat="server" OnClick="btnClick_DownloadJSON" />
    </div>
    </asp:Content>

