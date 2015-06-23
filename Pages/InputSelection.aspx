<%@ Page Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeBehind="InputSelection.aspx.cs" Inherits="RSMTool.Pages.InputSelection" %>
 <%@ MasterType VirtualPath="~/Site.Master" %>
 <asp:Content ID="HeadContent" runat="server" ContentPlaceHolderID="HeadContent">
    
        <script type="text/javascript">
            var count = 0;
            var removeColumn = new Array();

            //function to validate the input variables selected
            function Validate() {
                if (count <= 1) {
                    $('#ValidateInputs').fadeIn().delay(10000).fadeOut();
                    $('#ValidateInputs')[0].innerHTML = "<i>Please select atleast two input variables</i>";
                    return false;
                }
                else {
                    return true;
                }
            }
           
            var constantColors = {
                GridHeaderBgColor:"rgb(230,77,0)",
                GridHeaderBgColorNormal: "rgb(66,66,66)",
                GridHeaderForeColor: "rgb(230, 77, 0)",
                GridHeaderForeColorNormal: "rgb(255,255,255)"
            };
            
            //function invoked when user unselects a column
            function removeColumns(list, value) {
       
                var values = list.split(',');
                for (var i = 0; i < values.length; i++) {
                    if (values[i] == value) {
                        values.splice(i, 1);
                        return values.join(',');
                    }
                }
                return list;
            }

            //function used to fill the color to selected column
            function fillColor(columnHeaderId, x1, clientIDs) {
                if (document.getElementById(columnHeaderId) != null) {
                    var columnName = document.getElementById(columnHeaderId).innerHTML;
                }
                if (document.getElementById(columnHeaderId) != null) {
                    if (document.getElementById(columnHeaderId).bgColor == constantColors.GridHeaderBgColor) {
                        count--;
                        x1.value = removeColumns(x1.value, columnName);

                        clientIDs.value = removeColumns(clientIDs.value, columnHeaderId);
                        document.getElementById(columnHeaderId).bgColor = constantColors.GridHeaderBgColorNormal;
                    }
                    else {
                        count++;
                        x1.value = x1.value + document.getElementById(columnHeaderId).innerHTML  + ",";
                        clientIDs.value = clientIDs.value + columnHeaderId + ",";
                    
                        document.getElementById(columnHeaderId).bgColor = constantColors.GridHeaderBgColor;
                    }
                    if (document.getElementById(columnHeaderId).style.color == constantColors.GridHeaderForeColor) {

                        document.getElementById(columnHeaderId).style.color = constantColors.GridHeaderForeColorNormal;
                    }
                    else {
                        document.getElementById(columnHeaderId).style.color = constantColors.GridHeaderForeColor;
                    }
                }

            }

            //this function will be invoked when user selects the header column.
                function FillColumnColor(columnHeaderId, isRefresh) {
                    var x1 = document.getElementById("<%= hiddenColName.ClientID %>");
                    var clientIDs = document.getElementById("<%= hidColumnIds.ClientID %>");
                    if (isRefresh.toLowerCase() == "true" && columnHeaderId != "") {
                        var colvalues = columnHeaderId.split(',');
                        for (var i = 0; i < colvalues.length; i++) {
                            count++;

                            x1.value = x1.value + document.getElementById(colvalues[i]).innerHTML + ",";
                            clientIDs.value = clientIDs.value + colvalues[i] + ",";
                            document.getElementById(colvalues[i]).bgColor = constantColors.GridHeaderBgColor;
                            document.getElementById(colvalues[i]).style.color = constantColors.GridHeaderForeColor;
                        }
                    }
                    else {
                        if (document.getElementById("<%= hiddenSiteMap.ClientID %>").value == "true" && isRefresh != "true") {
                            var isConfirm = confirm("Click on  next to save your changes.If the model has been genearated,it has to be regenerated for the new input variables");
                            if (isConfirm) {
                                fillColor(columnHeaderId,x1,clientIDs);
                                document.getElementById("<%= hiddenSiteMap.ClientID %>").value = "false";
                                window.location = "InputSelection.aspx?selectedCols=" + clientIDs.value;
                            }
                            else {
                                return;
                            }
                        }
                        else {
                            fillColor(columnHeaderId,x1,clientIDs);
                        }

                        
                    }
                }

                

            
           </script>
 </asp:Content>

 <asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
  <div class="content">
 <asp:Label ID="lbl" runat="server" CssClass="label" Font-Italic="true">Click on the column header to select the INPUT VARIABLES</asp:Label><br />
<br />
<div id="ValidateInputs" style="color:Red;font-weight:bold"></div>
  <asp:GridView ID="GridViewFile"   
        Width="100%" AllowPaging="true"  runat="server" PageSize="15"
        EmptyDataText="No records"  PagerStyle-CssClass="pgr" 
        AlternatingRowStyle-CssClass="alt"
        CssClass="mGrid" ondatabound="GridViewFile_RowDataBound" 
        OnPageIndexChanging="GridViewFile_PageIndexChanged">
                  </asp:GridView><br />
      <div class="footer">
                  <asp:Button ID="btnNext" Text="Next" runat="server" OnClientClick="return Validate();" CssClass="btnNext" OnClick="btnNext_Clicked" />
                
</div>
<asp:HiddenField ID="hidColumnIds" runat="server" />
<asp:HiddenField ID="hiddenColName" runat="server"/>
<asp:HiddenField ID="hiddenSiteMap" runat="server" />
</div>
  </asp:Content>