<%@ Page Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeBehind="OutputSelection.aspx.cs" Inherits="RSMTool.Pages.OutputSelection" %>
 <asp:Content ID="HeadContent" runat="server" ContentPlaceHolderID="HeadContent">
        <script type="text/javascript">
          
            var constantColors = {
                GridHeaderBgColor: "rgb(230,77,0)", 
                GridHeaderBgColorNormal: "rgb(66,66,66)",
                GridHeaderForeColor: "rgb(230, 77, 0)",
                GridHeaderForeColorNormal: "rgb(255,255,255)"
            };
            var count = 0;

            //function to validate the output variables selected
            function Validate(id) {
                if (count == 0) {
                    $('#ValidateOutputs').fadeIn().delay(10000).fadeOut();
                    $('#ValidateOutputs')[0].innerHTML = "<i>Please select atleast one output variable</i>";
                    return false;
                }
                else {
                    document.getElementById('<%=btnEval.ClientID %>').style.display = "none";
                    document.getElementById('ajax_loader').style.display = "block";
                    document.getElementById('waitingCursor').src = "../Images/ajax-loader.gif"; 
                    return true;
                }
            }

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
                    if (document.getElementById(columnHeaderId).bgColor.toLowerCase() == constantColors.GridHeaderBgColor) {
                        document.getElementById(columnHeaderId).bgColor = constantColors.GridHeaderBgColorNormal;
                        count--;
                        x1.value = removeColumns(x1.value, columnName);
                        clientIDs.value = removeColumns(clientIDs.value, columnHeaderId);
                    }
                    else {
                        count++;
                        x1.value = x1.value + document.getElementById(columnHeaderId).innerHTML + ",";
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

            //function to set the color to selected column
            function FillColumnColor(columnHeaderId, isRefresh) {
                var x1 = document.getElementById("<%= hiddenColName.ClientID %>");
                var clientIDs = document.getElementById("<%= ophidColumnIds.ClientID %>");
                if (isRefresh.toLowerCase() == "true" && columnHeaderId!="") {
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
                        var isConfirm = confirm("Click on  evaluate to save your changes.The model has to be generated for the new output variables");
                        if (isConfirm) {
                            fillColor(columnHeaderId, x1, clientIDs);
                            document.getElementById("<%= hiddenSiteMap.ClientID %>").value = "false";
                            
                            window.location = "OutputSelection.aspx?selectedCols=" + clientIDs.value;
                        }
                        else {
                            return;
                        }
                    }
                    else {
                        fillColor(columnHeaderId, x1, clientIDs);
                    }
                }
                
                
            }
                    </script>
 </asp:Content>

 <asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
  <div class="content">
 <asp:Label ID="lbl" runat="server" CssClass="label" Font-Italic="true">Click on the column header to select the OUTPUT VARIABLES</asp:Label><br />
<br />
 <div id="ValidateOutputs" style="color:Red;font-weight:bold"></div>

  <asp:GridView ID="GridViewFile" 
        Width="100%"  AllowPaging="true"  runat="server" PageSize="15"
        EmptyDataText="No records"  PagerStyle-CssClass="pgr" 
        AlternatingRowStyle-CssClass="alt"
        CssClass="mGrid" ondatabound="GridViewOutputFile_RowDataBound" 
        OnPageIndexChanging="GridViewFile_PageIndexChanged">
                  </asp:GridView><br />
                  <div class="footer">                 
                   <asp:Button ID="btnEval" Text="Evaluate" CssClass="btnNext" OnClientClick="return Validate(this);" runat="server" OnClick="btnEval_Clicked"/></div>
                  <div id="ajax_loader" style="display:none;"><img src='' id='waitingCursor' /> </div>
                
                 
                  <asp:Panel ID="panelArray" runat="server"></asp:Panel>
<asp:HiddenField ID="hiddenColName" runat="server" />
<asp:HiddenField ID="ophidColumnIds" runat="server" />
<asp:HiddenField ID="hiddenSiteMap" runat="server" />
  </div>
  </asp:Content>
