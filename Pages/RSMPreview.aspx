<%@ Page Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeBehind="RSMPreview.aspx.cs" Inherits="RSMTool.Pages.RSMPreview" %>

<asp:Content ID="HeadContent" runat="server" ContentPlaceHolderID="HeadContent">
         <script src="../Scripts/d3.v3.min.js"  type="text/javascript"></script>
         <script src="../Scripts/dimple.v2.0.0.min.js" type="text/javascript"></script>
        <script type="text/javascript">

            //function that shows waiting cursor on screen if application is waiting for a task to be done.
            function WaitingCursor() {
                document.getElementById('<%=btnRSMViewer.ClientID %>').style.display = "none";
                document.getElementById('loading').style.display = "block";
                document.getElementById('waitingCursor').src = "../Images/ajax-loader.gif"; 
            }
            var jfittedValues = [];
            var jactualValues = [];
            var Maximum = [];
            var Minimum = [];
            var flfittedValues = [];
            var fljactualValues = [];
            var svg;
            var bubbleChart;
            var cleanAxis = function (xaxis, zAxis) {

                bubbleChart.addSeries("ActualValues", dimple.plot.line, [xaxis, zAxis]);
            }

            //function to plot bubble chart
            function PlotGraph(fittedValues, actualValue) {
                
                jfittedValues = fittedValues.split(',');
                jactualValues = actualValue.split(',');
                flfittedValues = jfittedValues.map(Number);
                fljactualValues = jactualValues.map(Number);
                 for (var s = 0; s < 2; s++) {
                    if (s > 0) {
                        Maximum[s] = Math.min.apply(Math, fljactualValues);
                        Minimum[s] = Math.min.apply(Math, fljactualValues);
                    }
                    else {
                        Maximum[s] = Math.max.apply(Math, fljactualValues);
                        Minimum[s] = Math.max.apply(Math, fljactualValues);
                    }
                }
                var JSONarray = [];
                var JSONvalues = "";
                
                for (var i = 0; i < jfittedValues.length; i++) {

                    var item = {
                        "ActualValues": fljactualValues[i],
                        "FittedValues": flfittedValues[i]
                        };

                         JSONarray.push(item);
                     }
                      for (var j = 0; j < 2; j++) {
                          var maxminItems = {

                              "MaxValue": Maximum[j],
                              "MinValue": Minimum[j]


                          };
                          JSONarray.push(maxminItems);
                     }
                     JSONvalues = JSONarray;
               svg = dimple.newSvg("#chartContainer",600,400);
                bubbleChart = new dimple.chart(svg, JSONvalues);
                var xAxis = bubbleChart.addMeasureAxis("x", "ActualValues");
                var yAxis = bubbleChart.addMeasureAxis("y", "FittedValues");
                var x1Axis = bubbleChart.addMeasureAxis(xAxis, "MaxValue");
                var y1Axis = bubbleChart.addMeasureAxis(yAxis, "MinValue");
                xAxis.overrideMax = Math.max.apply(Math, fljactualValues);
                xAxis.overrideMin = Math.min.apply(Math, fljactualValues);
                yAxis.overrideMax = Math.max.apply(Math, flfittedValues);
                yAxis.overrideMin = Math.min.apply(Math, flfittedValues);
                bubbleChart.addSeries(["ActualValues", "FittedValues"], dimple.plot.bubble, [xAxis, yAxis]);
                bubbleChart.addSeries(["MaxValue"], dimple.plot.line,[x1Axis,y1Axis]);
                bubbleChart.draw();

                d3.selectAll("circle")
                    .style("fill", "#1F45FC")  
                    .attr("r", 3);

                d3.selectAll("line")
                    .style("stroke", "#C0C0C0");
                
            }
        </script>
    <style>
        input[type=radio]+label {
  margin: 0;
}
    </style>
        </asp:Content>
        
        <asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
        
              <div class="selectedVariables">
         <asp:Label ID="lblPanel" runat="server" CssClass="label" Font-Italic="true">OUTPUT VARIABLES</asp:Label><br />
        <asp:Panel ID="panelRadioButtons"  Width="100%"  runat="server">
        </asp:Panel>
        </div>
        <div id="chartContainer" class="chartContainer" style="width:606px;height:410px;">
                  </div>
                  <div class="footer">
                 
                  <%--<asp:Button ID="btnDimpleAreaGraph" Text="DimpleArea Chart"  CssClass="btnNext" runat="server" OnClick="RedirectToPage" /> 
                  <asp:Button ID="btnDimpleLineGraph" Text="Dimple Line Chart"  CssClass="btnNext" runat="server" OnClick="RedirectToLineGraphPage" /> --%>
                  <asp:Button ID="btnRSMViewer" Text="RSM Viewer"  CssClass="btnNext" runat="server" OnClientClick="WaitingCursor();" OnClick="btnRSMViewer_Click" /> 
                 <%-- &nbsp;&nbsp;<asp:Button ID="Button1" Text="Bubble"  CssClass="btnNext" runat="server"  OnClick="btnRedirect" /> --%>
                  <div id="loading" style="display:none;"><img src='' id='waitingCursor' /> </div>
                  </div>
                
        </asp:Content>
       
