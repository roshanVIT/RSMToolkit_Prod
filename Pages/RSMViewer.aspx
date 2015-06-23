<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Site.master" CodeBehind="RSMViewer.aspx.cs" Inherits="RSMTool.Pages.RSMViewer" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<asp:Content ID="HeadContent" runat="server" ContentPlaceHolderID="HeadContent">

    <link href="../Styles/jslider.css" rel="stylesheet" type="text/css" />
    <link href="../Styles/jslider.round.css" rel="stylesheet" type="text/css" />
         
	<script src="../Scripts/d3.v3.min.js" type="text/javascript"></script>
    <script src="../Scripts/d3.geom.contour.v0.min.js" type="text/javascript"></script>
    <script src="../Scripts/dimple.v2.1.0.min.js" type="text/javascript"></script>
	<script src="../Scripts/jquery.mousewheel.min.js" type="text/javascript"></script>
    <script src="../Scripts/jshashtable-2.1_src.js" type="text/javascript"></script>
    <script src="../Scripts/jquery.numberformatter-1.2.3.js" type="text/javascript"></script>
    <script src="../Scripts/tmpl.js" type="text/javascript"></script>
    <script src="../Scripts/jquery.dependClass-0.1.js" type="text/javascript"></script>
    <script src="../Scripts/draggable-0.1.js" type="text/javascript"></script>
    <script src="../Scripts/jquery.slider.js" type="text/javascript"></script>

    <script type="text/javascript">
            
          
            // function to retrive count of numbers before decimal point
            function retr_dec(numStr,isbeforeDot) {
                var pieces = numStr.split(".");
                if (isbeforeDot)
                    return pieces[0].length;
                else if (pieces.length==1)
                    return 0;
                else
                    return pieces[1].length;
                    
            }

        //function for jquery slider to initalize
            function multiSlider(divID, min, max, stepValue) {
                var roundCount = 0;
                var numberCountbeforeDecimal = retr_dec(max, true);
                var numberCountafterDecimal = retr_dec(max, false);
                var numFormat = "";
                if (Number(min) < 0) {
                    numFormat = "-";
                }
                for (var j = 0; j < numberCountbeforeDecimal; j++) {
                    numFormat = numFormat + "#";
                }

                for (var k = 0; k < numberCountafterDecimal; k = k + 2) {
                    roundCount++;
                    if (k == 0) {
                        numFormat = numFormat + ".";
                    }
                    numFormat = numFormat + "0";
                }

                jQuery("#" + divID).slider(
        { from: Number(min), to: Number(max),round:roundCount,step:Number(stepValue), smooth: false, skin: "round"
        });
            }

        //function to set id of drop down list on change
            function setIDforddlChange(id) {
                var hidSourceID = document.getElementById("<%=hddlInputChange.ClientID%>");
                hidSourceID.value = id;
                document.getElementById('<%=ReDrawAreaChart.ClientID %>').disabled = true;
                document.getElementById('<%=ReDrawAreaChart.ClientID %>').style.opacity = "0.5";
            }

        // function that redraws the contour plot on change of inputs
            function Redraw(id) {
                document.getElementById("<%=hddlInputChange.ClientID%>").value = id;
                var rangeSliderclientIDs = $('#<%=dynamicaControlIDs.ClientID %>')[0].value.split(',');
                var xValues = document.getElementById(rangeSliderclientIDs[0].toString()).value.split(';');
                var yValues = document.getElementById(rangeSliderclientIDs[1].toString()).value.split(';');

                _xMin = xValues[0]; _xMax = xValues[1]; _yMin = yValues[0]; _yMax = yValues[1];

                for (var i = 0; i < rangeSliderclientIDs.length - 1; i++) {
                    

                    document.getElementById('dynamicHiddenValues').value = document.getElementById('dynamicHiddenValues').value + $("#" + rangeSliderclientIDs[i])[0].getAttribute("uniqueid") + "|" + $("#" + rangeSliderclientIDs[i]).slider("value") + ",";
                  
                    
                }
                document.getElementById('<%=ReDrawAreaChart.ClientID %>').style.display = "none";
                document.getElementById('loading').style.display = "block";
                document.getElementById('waitingCursor').src = "../Images/ajax-loader.gif"; 
                document.getElementById('dynamicHiddenInput').value = document.getElementById('dynamicHiddenValues').value;
                document.getElementById('dynamicHiddenValues').value = "";
            }

            var _xMin;
            var _xMax;
            var _yMin;
            var _yMax;
            var _xInt;
            var _yInt;
            var _zArray;
            var _sizeOfImage = 400;
            var _ctx;
            var _firstTime = true;
            var _xAxisGridArray = [[50, 425], [100, 425], [150, 425], [200, 425], [250, 425], [300, 425], [350, 425], [400, 425], [450, 425]];
            var _yAxisGridArray = [[50, 25], [50, 75], [50, 125], [50, 175], [50, 225], [50, 275], [50, 325], [50, 375], [50, 425]];
       
            var _xAxisGridTickValues = [];
            var _yAxisGridTickValues = [];


        // Function tha plots the contour graph for the first time
            function PlotNVD3AreaGraph(xAxisName, yAxisName, xAxisminValue, xAxisMaxValue, yAxisMinValue, yAxisMaxValue) {

                if (_firstTime) {
                    _xMax = xAxisMaxValue;
                    _xMin = xAxisminValue;
                    _yMin = yAxisMinValue;
                    _yMax = yAxisMaxValue;
                    _firstTime = false;
                }
                var c = document.getElementById("contourCanvas");
                _ctx = c.getContext("2d");
                var heatmap = JSON.parse(document.getElementById("<%= hfldContours.ClientID%>").value);
                _zArray = JSON.parse(document.getElementById("<%= hfldContours.ClientID%>").value);
                var range = document.getElementById("<%=hfldRange.ClientID%>").value.split(',');
              
                DrawContourMap(_xMin, _xMax, _yMin, _yMax, range);

                var ccolors = ["#00f", "#add8e6", "#228b22", "#6c0", "#ffff00", "#ff4500"];
           
                    var cnames = document.getElementById("<%=hfldCNames.ClientID%>").value.split(',');
                    var legendWidth = 200;
                  
                    var margin = { top: 20, right: 160, bottom: 40, left: 40 },
                                    width = 800 - legendWidth - margin.left - margin.right,
                                    height = 400 - margin.top - margin.bottom;

                    var x = d3.scale.linear().domain([Number(xAxisminValue), Number(xAxisMaxValue)])
                            .range([0, width]);

                    var y = d3.scale.linear().domain([Number(yAxisMinValue), Number(yAxisMaxValue)])
                            .range([height, 0]);

               
                    var color = d3.scale.linear()
                                .domain(range)
                                .range(ccolors);

                    var xAxis = d3.svg.axis()
                                .scale(x)
                                .orient("bottom");

                    var yAxis = d3.svg.axis()
                                .scale(y)
                                .orient("left");

                    d3.select("#chart1").select("g").remove();
                   
                    var svg = d3.select("#chart1");
                    svg.selectAll("text").remove();

                //adding x-axis grid line tick lines
                  _xAxisGridArray.forEach(xAxisGridlinesAdderFunction);
                    function xAxisGridlinesAdderFunction(element, index, array)
                    {
                        svg.append("line")
                        .attr("x1", element[0])
                        .attr("y1", element[1])
                        .attr("x2", element[0])
                            .attr("y2", element[1] + 10)
                        .attr("style", "stroke: rgb(0, 0, 0); stroke-width: 2;");
                        svg.append("text")
                           .attr("x", _xAxisGridArray[index][0] - 10)
                           .attr("y", _xAxisGridArray[index][1] + 25)
                           .text(_xAxisGridTickValues[index]);
                    }

                //adding yaxis gridline tick lines

                    _yAxisGridArray.forEach(yAxisGridlinesAdderFunction);
                    function yAxisGridlinesAdderFunction(element, index, array) {
                        svg.append("line")
                        .attr("x1", element[0])
                        .attr("y1", element[1])
                        .attr("x2", element[0]-8)
                            .attr("y2", element[1])
                        .attr("style", "stroke: rgb(0, 0, 0); stroke-width: 2;");

                        svg.append("text")
                       .attr("x", _yAxisGridArray[index][0] - 40)
                       .attr("y", _yAxisGridArray[index][1] + 2)
                       .text(_yAxisGridTickValues[index]);
                    }
                    
                
                //adding x and y axis variable names
                    svg.append("text")
                    .text(document.getElementById('<%=ddlFirstInput.ClientID%>').value.trim())
                    .attr("x", "225")
                    .attr("y", "465")
                    .attr("style", "font-size:130%");

                    svg.append("text")
                    .text(document.getElementById('<%=ddlSecondInput.ClientID%>').value.trim())
                    .attr("x", "5")
                    .attr("style", "font-size:130%")
                    .attr("y", "245")
                    .attr("transform", "rotate(270 20,253)");



                // .attr("width", width + margin.left + margin.right+18)
                    svg.attr("height", height + margin.top + margin.bottom)
                    .append("g")
                    .attr("transform", "translate(" + margin.left + "," + margin.top + ")");



                   svg.selectAll("#legend")
                    .data(ccolors).enter()
                    .append("rect")
                    .attr("id", "legend")
                    .attr("class", "legend")
                    .attr("x", width + 80)
                    .attr("y", function (d, i) { return (height * 5 / 6 - i * height / 6)+40; })
                    .attr("width", 20)
                    .attr("height", height / 6)
                    .attr("style", function (d) { return "fill:" + d; });
                

                    svg.selectAll("#legendnames")
                    .data(cnames).enter()
                    .append("text")
                    .attr("class", "legendnames")
                    .attr("id", "legendnames")
                    .attr("x", width + 110)
                    .attr("y", function(d, i) { return (height * 11 / 12 - i * height / 6 - 10)+40; })
                    .attr("dy", "0em")
                    .attr("fill", "black")
                    .text(function(d) { return d; })
                    .call(wrap, 50);

                    function isoline(min) {
                        return function(x, y) {
                            return x >= 0 && y >= 0 && x < dx && y < dy && heatmap[y][x] >= min;
                        };
                    }

                    function transform(point) {
                        return [point[0] * width / dx, point[1] * height / dy];
                    }

                    function wrap(text, width) {

                        text.each(function() {
                            var text = d3.select(this),
        words = text.text().split(/\s+/).reverse(),
        word,
        line = [],
        lineNumber = 0,
        lineHeight = 1.1, // ems
        y = text.attr("y"),
        x = text.attr("x"),
        dy = parseFloat(text.attr("dy")),
        tspan = text.text(null).append("tspan").attr("x", x).attr("y", y).attr("dy", dy + "em");
                            while (word = words.pop()) {
                                line.push(word);
                                tspan.text(line.join(" "));
                                if (tspan.node().getComputedTextLength() > width) {
                                    line.pop();
                                    tspan.text(line.join(" "));
                                    line = [word];
                                    tspan = text.append("tspan").attr("x", x).attr("y", y).attr("dy", ++lineNumber * lineHeight + dy + "em").text(word);
                                }
                            }
                        });
                    }

                    document.getElementById('<%=ReDrawAreaChart.ClientID %>').style.display = "block";
                    document.getElementById('loading').style.display = "none";
            }

            
            function GetMousePos(canvas, evt) {
                var rect = canvas.getBoundingClientRect();
                return {
                    x: evt.clientX - rect.left,
                    y: evt.clientY - rect.top
                };
            }

        // Drawing contour plot on canvas
            function DrawContourMap(xMin, xMax, yMin, yMax, cRanges) {
                var c = document.getElementsByName("contourCanvas")[0];
                var _ctx = c.getContext("2d");
                
                //   drawing iso bands
                var i, j;

                 _xPInt = (xMax - xMin) / _sizeOfImage;
                 _yQInt = (yMax - yMin) / _sizeOfImage;

                _xInt = ((xMax - xMin) / (_zArray.length - 1));
                _yInt = ((yMax - yMin) / (_zArray[0].length - 1));

                // for mouse movement on canvas

                c.addEventListener('mousemove', function (evt) {
                    var mousePos = GetMousePos(c, evt);

                    _xPInt = (xMax - xMin) / _sizeOfImage;
                    _yQInt = (yMax - yMin) / _sizeOfImage;

                     _xInt = ((xMax - xMin) / (_zArray.length - 1));
                    _yInt = ((yMax - yMin) / (_zArray[0].length - 1));

                    var xPos = mousePos.x;
                    // var yPos = _sizeOfImage - mousePos.y;
                    var yPos = mousePos.y;

                    var x = parseFloat(xMin) + parseFloat(_xPInt * xPos);
                    var y = parseFloat(yMin) + parseFloat(_yQInt * yPos);

                    //first build square accross the given xy points
                    var il, ir, jt, jb;
                    il = parseInt(Math.floor((parseFloat(x) - parseFloat(xMin)) / _xInt));
                    var xFactor = (x - (_xInt * il) - xMin) / _xInt;
                    ir = il + 1;
                    jb = parseInt(Math.floor((parseFloat(y) - parseFloat(yMin)) / _yInt));
                    var yFactor = (y - (_yInt * jb) - yMin) / _yInt;
                    jt = jb + 1;

                    var iMaxInZArray = _zArray.length - 1;
                    var jMaxInZArray = _zArray[0].length - 1;

                    if (il == iMaxInZArray) {
                        ir = il;
                        xFactor = 0;
                    }

                    if (jb == jMaxInZArray) {
                        jt = jb;
                        yFactor = 0;
                    }

                    if (il > iMaxInZArray)
                        debugger;
                    if (ir > iMaxInZArray)
                        debugger;
                    if (jt > jMaxInZArray)
                        debugger;
                    if (jb > jMaxInZArray)
                        debugger;

                    var z1 = _zArray[jt][il];
                    var z2 = _zArray[jt][ir];
                    var z3 = _zArray[jb][ir];
                    var z4 = _zArray[jb][il];

                    var newPointZvalue = z1 * (1 - xFactor) * (yFactor)
                        + z2 * xFactor * (yFactor)
                        + z3 * (xFactor) * (1 - yFactor)
                        + z4 * (1 - xFactor) * (1 - yFactor);

                    
                    $('#textForMousePosition').css({

                        left: mousePos.x + parseFloat($("#contourCanvas").css('margin-left')) + 3,
                        top: mousePos.y + parseFloat($("#contourCanvas").css('margin-top')) + 3,

                    });

                    if (mousePos.x + parseFloat($("#contourCanvas").css('margin-left')) > 275) {
                        $('#textForMousePosition').css({
                            left: mousePos.x + parseFloat($("#contourCanvas").css('margin-left')) - 125
                        });

                    }

                    if (mousePos.y + parseFloat($("#contourCanvas").css('margin-top')) > 360) {
                        $('#textForMousePosition').css({
                            top: mousePos.y + parseFloat($("#contourCanvas").css('margin-top')) - 73,
                        });

                    }

                    
                    var message = '<span style="color:black">' + document.getElementById('<%=ddlFirstInput.ClientID%>').value.trim() + ' : ' + (x).toFixed(4)
                        + '</span><br/><span style="color:black">    ' + document.getElementById('<%=ddlSecondInput.ClientID%>').value.trim() + ' : ' + (parseFloat(yMin) + parseFloat(_yQInt * (_sizeOfImage - mousePos.y))).toFixed(4)
                        + '</span><br/><span style="color:black">  ' + document.getElementById('<%=ddlOutput.ClientID%>').value.trim() + ' : ' + newPointZvalue.toFixed(4)
                        + '</span>';
                   
                    document.getElementById("textForMousePosition").innerHTML = message;
                }, false);

                c.addEventListener('mouseout', function () {
                    document.getElementById("textForMousePosition").innerText = '';
                }, false);

                //storing x and y grid ticks values after calculating at tick positons

                _xAxisGridArray.forEach(calculateXGridTickValues);
                function calculateXGridTickValues(element, index, array) {

                     _xPInt = (xMax - xMin) / _sizeOfImage;
                     _yQInt = (yMax - yMin) / _sizeOfImage;

                    _xInt = ((xMax - xMin) / (_zArray.length - 1));
                    _yInt = ((yMax - yMin) / (_zArray[0].length - 1));

                    var xPos = element[0] - 50;
                    var yPos = _sizeOfImage - (element[1] - 25);

                    _xAxisGridTickValues[index] = (parseFloat(xMin) + parseFloat(_xPInt * xPos)).toFixed(2);

                }
                _yAxisGridArray.forEach(calculateYGridTickValues);
                function calculateYGridTickValues(element, index, array) {

                    _xPInt = (xMax - xMin) / _sizeOfImage;
                    _yQInt = (yMax - yMin) / _sizeOfImage;

                    _xInt = ((xMax - xMin) / (_zArray.length - 1));
                    _yInt = ((yMax - yMin) / (_zArray[0].length - 1));

                    var xPos = element[0] - 50;
                    var yPos = _sizeOfImage - (element[1] - 25);

                    _yAxisGridTickValues[index] = (parseFloat(yMin) + parseFloat(_yQInt * yPos)).toFixed(2);

                }


                //drawing iso bands...

                for (i = 0; i < _sizeOfImage; i++) {

                    for (j = 0; j < _sizeOfImage; j++) {
                        var p = i;
                        //  var q = _sizeOfImage - j;
                        var q = j;
                        if (p < 0 || p > 400) debugger;
                        if (q < 0 || q > 400) debugger;

                        var x = parseFloat(xMin) + parseFloat(_xPInt * p);
                        var y = parseFloat(yMin) + parseFloat(_yQInt * q);

                        //first build square accross the given xy points
                        var il, ir, jt, jb;
                        il = parseInt(Math.floor((parseFloat(x) - parseFloat(xMin)) / _xInt));
                        var xFactor = (x - (_xInt * il) - xMin) / _xInt;
                        ir = il + 1;
                        jb = parseInt(Math.floor((parseFloat(y) - parseFloat(yMin)) / _yInt));
                        var yFactor = (y - (_yInt * jb) - yMin) / _yInt;
                        jt = jb + 1;

                        var iMaxInZArray = _zArray.length - 1;
                        var jMaxInZArray = _zArray[0].length - 1;

                        if (il == iMaxInZArray) {
                            ir = il;
                            xFactor = 0;
                        }

                        if (jb == jMaxInZArray) {
                            jt = jb;
                            yFactor = 0;
                        }

                        if (il > iMaxInZArray)
                            debugger;
                        if (ir > iMaxInZArray)
                            debugger;
                        if (jt > jMaxInZArray)
                            debugger;
                        if (jb > jMaxInZArray)
                            debugger;

                       

                        var z1 = _zArray[jt][il];
                        var z2 = _zArray[jt][ir];
                        var z3 = _zArray[jb][ir];
                        var z4 = _zArray[jb][il];

                        var newPointZvalue = z1 * (1 - xFactor) * (yFactor)
                            + z2 * xFactor * (yFactor)
                            + z3 * (xFactor) * (1 - yFactor)
                            + z4 * (1 - xFactor) * (1 - yFactor);

                        var colorIndex = -1;
                        var colorArray = ["#00f", "#add8e6", "#228b22", "#6c0", "#ffff00", "#ff4500"];
                        for (var m = 0; m < cRanges.length - 1; m++) {
                            if (newPointZvalue > cRanges[m] && newPointZvalue < cRanges[m + 1]) {
                                colorIndex = m;
                                break;
                            }
                        }

                        if (colorIndex == -1) colorIndex = cRanges.length - 1;
                        if (colorIndex > colorArray.length) debugger;

                        _ctx.fillStyle = colorArray[colorIndex];
                        _ctx.fillRect(i, j, 1, 1);
                    }
                }

                // drawing contour lines algorithm

                var width = 9;
                var height = 9;

                for (i = 0; i < _sizeOfImage - 10; i += 9)
                    for (j = 0; j < _sizeOfImage - 10; j += 9) {

                        var x1 = i;
                        var y1 = j;
                        var x2 = (i + width >= _sizeOfImage) ? _sizeOfImage - 1 : i + width;
                        var y2 = j;
                        var x3 = (i + width >= _sizeOfImage) ? _sizeOfImage - 1 : i + width;
                        var y3 = ((j) + height >= _sizeOfImage) ? _sizeOfImage - 1 : (j) + height;
                        var x4 = i;
                        var y4 = ((j) + height >= _sizeOfImage) ? _sizeOfImage - 1 : (j) + height;

                        var A = [x1, y1];
                        var B = [x2, y2];
                        var C = [x3, y3];
                        var D = [x4, y4];

                        var zA = GetZByXY((parseFloat(xMin) + parseFloat(_xPInt * A[0])), (parseFloat(yMin) + parseFloat(_yQInt * A[1])));
                        var zB = GetZByXY((parseFloat(xMin) + parseFloat(_xPInt * B[0])), (parseFloat(yMin) + parseFloat(_yQInt * B[1])));
                        var zC = GetZByXY((parseFloat(xMin) + parseFloat(_xPInt * C[0])), (parseFloat(yMin) + parseFloat(_yQInt * C[1])));
                        var zD = GetZByXY((parseFloat(xMin) + parseFloat(_xPInt * D[0])), (parseFloat(yMin) + parseFloat(_yQInt * D[1])));
                        var zSaddlePoint;

                        var caseNumber = "";

                        var point1ToBeDrawn;
                        var point2TOBeDrawn;
                        var saddlePoint;

                        cRanges.forEach(rangeloop);
                        function rangeloop(range, index, array) {
                            caseNumber = "";
                            if (zA > range) caseNumber += "1";
                            else
                                caseNumber += "0";
                            if (zB > range) caseNumber += "1";
                            else
                                caseNumber += "0";
                            if (zC > range) caseNumber += "1";
                            else
                                caseNumber += "0";
                            if (zD > range) caseNumber += "1";
                            else
                                caseNumber += "0";


                            switch (caseNumber) {
                                case "1111":
                                case "0000": break;

                                case "1100":
                                case "0011":
                                    {
                                        point1ToBeDrawn = GetPointBetweenTwoPoints(A, D, zA, zD, range);
                                        point2TOBeDrawn = GetPointBetweenTwoPoints(B, C, zB, zC, range);

                                        DrawLineBetweenTwoPoints(point1ToBeDrawn, point2TOBeDrawn);

                                        break;
                                    }

                                case "1001":
                                case "0110":
                                    {
                                        point1ToBeDrawn = GetPointBetweenTwoPoints(A, B, zA, zB, range);
                                        point2TOBeDrawn = GetPointBetweenTwoPoints(D, C, zD, zC, range);

                                        DrawLineBetweenTwoPoints(point1ToBeDrawn, point2TOBeDrawn);

                                        break;

                                    }

                                case "1110":
                                case "0001":
                                    {
                                        point1ToBeDrawn = GetPointBetweenTwoPoints(A, D, zA, zD, range);
                                        point2TOBeDrawn = GetPointBetweenTwoPoints(D, C, zD, zC, range);

                                        DrawLineBetweenTwoPoints(point1ToBeDrawn, point2TOBeDrawn);

                                        break;
                                    }

                                case "1101":
                                case "0010":
                                    {
                                        point1ToBeDrawn = GetPointBetweenTwoPoints(B, C, zB, zC, range);
                                        point2TOBeDrawn = GetPointBetweenTwoPoints(D, C, zD, zC, range);

                                        DrawLineBetweenTwoPoints(point1ToBeDrawn, point2TOBeDrawn);

                                        break;
                                    }

                                case "1011":
                                case "0100":
                                    {
                                        point1ToBeDrawn = GetPointBetweenTwoPoints(A, B, zA, zB, range);
                                        point2TOBeDrawn = GetPointBetweenTwoPoints(B, C, zB, zC, range);

                                        DrawLineBetweenTwoPoints(point1ToBeDrawn, point2TOBeDrawn);

                                        break;
                                    }
                                case "0111":
                                case "1000":
                                    {
                                        point1ToBeDrawn = GetPointBetweenTwoPoints(A, B, zA, zB, range);
                                        point2TOBeDrawn = GetPointBetweenTwoPoints(A, D, zA, zD, range);

                                        DrawLineBetweenTwoPoints(point1ToBeDrawn, point2TOBeDrawn);

                                        break;
                                    }
                                case "1010":
                                    {
                                        saddlePoint = [(i + (parseFloat(width / 2))), (j + (parseFloat(height / 2)))];

                                        zSaddlePoint = GetZByXY((parseFloat(xMin) + (_xPInt * saddlePoint[0])), (parseFloat(yMin) + (_yQInt * saddlePoint[1])));
                                        if (zSaddlePoint > range) {
                                            point1ToBeDrawn = GetPointBetweenTwoPoints(A, D, zA, zD, range);
                                            point2TOBeDrawn = GetPointBetweenTwoPoints(D, C, zD, zC, range);

                                            DrawLineBetweenTwoPoints(point1ToBeDrawn, point2TOBeDrawn);

                                            point1ToBeDrawn = GetPointBetweenTwoPoints(A, B, zA, zB, range);
                                            point2TOBeDrawn = GetPointBetweenTwoPoints(D, C, zD, zC, range);

                                            DrawLineBetweenTwoPoints(point1ToBeDrawn, point2TOBeDrawn);

                                            break;
                                        }
                                        else {

                                            point1ToBeDrawn = GetPointBetweenTwoPoints(A, B, zA, zB, range);
                                            point2TOBeDrawn = GetPointBetweenTwoPoints(A, D, zA, zD, range);

                                            DrawLineBetweenTwoPoints(point1ToBeDrawn, point2TOBeDrawn);

                                            point1ToBeDrawn = GetPointBetweenTwoPoints(B, C, zB, zC, range);
                                            point2TOBeDrawn = GetPointBetweenTwoPoints(C, D, zC, zD, range);

                                            DrawLineBetweenTwoPoints(point1ToBeDrawn, point2TOBeDrawn);

                                            break;

                                        }

                                    }

                                case "0101":
                                    {
                                        saddlePoint = [(i + parseFloat(width / 2)), (j + parseFloat(height / 2))];
                                        zSaddlePoint = GetZByXY((parseFloat(xMin) + (_xPInt * saddlePoint[0])), (parseFloat(yMin) + (_yQInt * saddlePoint[1])));
                                        if (zSaddlePoint > range) {
                                            point1ToBeDrawn = GetPointBetweenTwoPoints(B, C, zB, zC, range);
                                            point2TOBeDrawn = GetPointBetweenTwoPoints(C, D, zC, zD, range);

                                            DrawLineBetweenTwoPoints(point1ToBeDrawn, point2TOBeDrawn);

                                            point1ToBeDrawn = GetPointBetweenTwoPoints(A, B, zA, zB, range);
                                            point2TOBeDrawn = GetPointBetweenTwoPoints(A, D, zA, zD, range);

                                            DrawLineBetweenTwoPoints(point1ToBeDrawn, point2TOBeDrawn);

                                            break;
                                        }
                                        else {

                                            point1ToBeDrawn = GetPointBetweenTwoPoints(A, B, zA, zB, range);
                                            point2TOBeDrawn = GetPointBetweenTwoPoints(C, D, zC, zD, range);

                                            DrawLineBetweenTwoPoints(point1ToBeDrawn, point2TOBeDrawn);

                                            point1ToBeDrawn = GetPointBetweenTwoPoints(A, D, zA, zD, range);
                                            point2TOBeDrawn = GetPointBetweenTwoPoints(C, D, zC, zD, range);

                                            DrawLineBetweenTwoPoints(point1ToBeDrawn, point2TOBeDrawn);

                                            break;
                                        }

                                    }

                            }
                        }

                    }
            }

        //Function that calculates the Z point by using bilenear interpolation method
            function GetZByXY(x, y) {
                //first build square accross the given xy points
                var il, ir, jt, jb;

                il = parseInt(Math.floor((parseFloat(x) - parseFloat(_xMin)) / parseFloat(_xInt)));
                var xFactor = (parseFloat(x) - (_xInt * il) - _xMin) / _xInt;
                ir = il + 1;
                jb = parseInt(Math.floor((parseFloat(y) - parseFloat(_yMin)) / parseFloat(_yInt)));
                var yFactor = (parseFloat(y) - (_yInt * jb) - _yMin) / _yInt;
                jt = jb + 1;

                var iMaxInZArray = _zArray.length;
                var jMaxInZArray = _zArray[0].length;

                if (il == iMaxInZArray) {
                    ir = il;
                    xFactor = 0;
                }

                if (jb == jMaxInZArray) {
                    jt = jb;
                    yFactor = 0;
                }

                if (il > iMaxInZArray) {
                    debugger;
                }
                if (ir > iMaxInZArray) {
                    debugger;
                }
                if (jt > jMaxInZArray) {
                    debugger;
                }
                if (jb > jMaxInZArray) {
                    debugger;
                }

                //calculate zvalues for the points of square generated.

                var z1 = _zArray[jt][il];
                var z2 = _zArray[jt][ir];
                var z3 = _zArray[jb][ir];
                var z4 = _zArray[jb][il];

                var newPointZvalue = z1 * (1 - xFactor) * (yFactor)
                    + z2 * xFactor * (yFactor)
                    + z3 * (xFactor) * (1 - yFactor)
                    + z4 * (1 - xFactor) * (1 - yFactor);
                return newPointZvalue;
            }

            function GetPointBetweenTwoPoints(point1, point2, z1, z2, range) {
                var zfactor = (range - z1) / (z2 - z1);
                var point = [(point1[0] + ((point2[0] - point1[0]) * zfactor)), (point1[1] + ((point2[1] - point1[1]) * zfactor))];
                return point;

            }

        //function that draws line between the two points 

            function DrawLineBetweenTwoPoints(point1ToBeDrawn, point2TOBeDrawn) {

                var point1 = [parseFloat(point1ToBeDrawn[0]), parseFloat(point1ToBeDrawn[1])];
                var point2 = [parseFloat(point2TOBeDrawn[0]), parseFloat(point2TOBeDrawn[1])];

                _ctx.beginPath();
                _ctx.moveTo(point1[0], point1[1]);
                _ctx.lineTo(point2[0], point2[1]);
                _ctx.stroke();
            }
  </script>

   </asp:Content>
        <asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
        
         <asp:ToolkitScriptManager ID="toolScript"  AsyncPostBackTimeout="360000" runat="server"></asp:ToolkitScriptManager>
         <asp:UpdatePanel ID="update" runat="server">
         <ContentTemplate>
        
         <div class="selectedVariables">
  <table id="tbContents" runat="server" cellspacing="5" cellpadding="5">
  
   <tr>
   <td>
   <asp:Label ID="lbX" runat="server" CssClass="dynLabel">X :</asp:Label>
   </td>
    <td>
    <asp:DropDownList ID="ddlFirstInput" runat="server" AutoPostBack="true" onchange="setIDforddlChange(this.id);" OnSelectedIndexChanged="ddlInputChange">
    </asp:DropDownList>
    </td>
    </tr>
    <tr>
    <td>
    <asp:Label ID="lbY" runat="server" CssClass="dynLabel">Y :</asp:Label> 
    </td>
    <td>
    <asp:DropDownList ID="ddlSecondInput" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlInputChange" onchange="setIDforddlChange(this.id);">
    </asp:DropDownList>
        
   </td>
   </tr>
   <tr>
   <td> 
      <asp:Label ID="Label1" runat="server" CssClass="dynLabel">Plot :</asp:Label>&nbsp;&nbsp;
    </td>
     <td>
     <asp:DropDownList ID="ddlOutput" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlOutputChange" onchange="setIDforddlChange(this.id);">
    </asp:DropDownList>
    </td>
    </tr>
     </table>
     <br />
     <div id="tbdynamicControls" runat="server">
     </div>
     <br />
    <asp:Button ID="ReDrawAreaChart" CssClass="btnNext" runat="server" Text="Refresh" OnClick="btnClick_PlotContours" OnClientClick="Redraw(this.id);" Enabled="false" />
     <div id="loading" style="display:none;"><img src='' id='waitingCursor' /> </div>
</div>  
<asp:HiddenField ID="hfldCNames" runat="server" />
<asp:HiddenField ID="hfldContours" runat="server" /> 
        <asp:HiddenField ID="hfldRange" runat="server" />
        <input type="hidden"  id="dynamicHiddenInput" name="dynamicHiddenInput" />
        <input type="hidden" id="dynamicHiddenValues" />
        <asp:HiddenField ID="hddlInputChange" runat="server" />
        <asp:HiddenField ID="dynamicaControlIDs" runat="server" />
    </ContentTemplate>
    </asp:UpdatePanel>
                  <div style="width:5%; display:inline-block;"></div>
                  <div id="chartContainer" class="chartContainer" style="position:relative">
     <svg xmlns="http://www.w3.org/2000/svg" id="chart1" style="position:absolute;min-width:500px;min-height:500px">
       <line xmlns="http://www.w3.org/2000/svg" style="stroke: rgb(0, 0, 0); stroke-width: 3;" x1="50" y1="25" x2="50" y2="425" />
<line xmlns="http://www.w3.org/2000/svg" style="stroke: rgb(0, 0, 0); stroke-width: 3;" x1="50" y1="425" x2="450" y2="425" />
                      <img id="contourImageImg" style=" display:none; height: 90%;position: relative;top: 5.5%;left: 5.5%;"  />
         <canvas xmlns="http://www.w3.org/2000/svg" name="contourCanvas" id="contourCanvas" width="400" height="400" style="cursor: crosshair; position: relative; margin-top: 25px; margin-left: 50px;"></canvas>
         <div id="textForMousePosition" style=" background-color:rgba(255, 255, 255, 0.5);position:absolute;font-size:140%;font-weight:normal;text-align:left;overflow:hidden"></div>
         
     </svg>
                 
                  </div> 
                  <div class="footer">
                  <asp:Button ID="btnFinish" Text="Finish" CssClass="btnNext" runat="server" OnClick="btnFinish_Click" />
                 &nbsp;&nbsp;<asp:Button ID="btnExe" Text="Generate Exe"  CssClass="btnNext" runat="server"  OnClick="btnClick_GenerateJsonFile" /> 
                 

                 </div>
                  </asp:Content>

                