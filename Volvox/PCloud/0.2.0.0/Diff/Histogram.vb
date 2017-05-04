Imports System.Drawing
Imports Grasshopper.Kernel
Imports System.IO
Imports Rhino.Geometry

Public Class Histogram
    Inherits GH_Component
    Sub New()
        MyBase.New("Histogram", "Histogram", "Create histogram out of data", "Volvox", "Diff")
    End Sub
    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return New Guid("2713b99e-c151-4e52-a35a-075dca8a3ca2")
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As GH_InputParamManager)
        pManager.AddNumberParameter("Data", "D", "Data", GH_ParamAccess.list)
        pManager.AddTextParameter("Project folder", "F", "Project folder", GH_ParamAccess.item)
        pManager.AddTextParameter("Data name", "N", "Name", GH_ParamAccess.item)
        pManager.AddTextParameter("Name base", "B", "Name base", GH_ParamAccess.item)
        pManager.AddIntegerParameter("Pix", "P", "Width per data item", GH_ParamAccess.item)
        pManager.AddIntegerParameter("Height", "H", "Height", GH_ParamAccess.item)
        pManager.AddNumberParameter("Grid", "G", "Target grid spread", GH_ParamAccess.item, 1)
        pManager.AddNumberParameter("Min", "Min", "Minimal value", GH_ParamAccess.item, 0)
        pManager.AddNumberParameter("Max", "Max", "Maximal value", GH_ParamAccess.item, 0)
        pManager.AddNumberParameter("Floating", "P", "Floating points", GH_ParamAccess.item, 0)
        pManager.AddBooleanParameter("Run", "R", "Run", GH_ParamAccess.item, False)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As GH_OutputParamManager)

    End Sub

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)
        Dim d As New List(Of Double)
        Dim p As String = String.Empty
        Dim n As String = String.Empty
        Dim b As String = String.Empty
        Dim w As Integer = 0
        Dim h As Integer = 0
        Dim g As Double = 0
        Dim run As Boolean = False

        Dim min As Double
        Dim max As Double
        Dim fp As Double

        If Not DA.GetDataList(0, d) Then Return
        If Not DA.GetData(1, p) Then Return
        If Not DA.GetData(2, n) Then Return
        If Not DA.GetData(3, b) Then Return
        If Not DA.GetData(4, w) Then Return
        If Not DA.GetData(5, h) Then Return
        If Not DA.GetData(6, g) Then Return
        If Not DA.GetData(7, min) Then Return
        If Not DA.GetData(8, max) Then Return
        If Not DA.GetData(9, fp) Then Return
        If Not DA.GetData(10, run) Then Return

        If run Then DrawHistogram(b, n, p, d, h, g, w, min, max, fp)


    End Sub


    Public Sub DrawHistogram(NameBase As String, FileName As String, Dir As String, Data As List(Of Double), BmpH As Integer, GridSpread As Double, Optional PixPerItem As Integer = 1, Optional MinValue As Double = 0, Optional MaxValue As Double = 0, Optional FloatingPoints As Integer = 0)
        If GridSpread <= 0 Then Return
        If PixPerItem < 1 Then Return

        Dim st As Double = StandardData(Data)
        Dim av As Double = AverageData(Data)

        Dim stanDev As String = String.Format("{0:0.000}", st)
        Dim averDev As String = String.Format("{0:0.000}", av)

        If PixPerItem <> 1 Then
            Dim tempdata As New List(Of Double)
            For i As Integer = 0 To Data.Count - 1 Step 1
                For j As Integer = 0 To PixPerItem - 1 Step 1
                    tempdata.Add(Data(i))
                Next
            Next
            Data.Clear()
            Data.AddRange(tempdata)
            tempdata.Clear()
        End If


        Data.Sort()

        Dim itv As New Interval(Data(0), Data(Data.Count - 1))

        Dim minv As Double = Math.Floor(Data(0) / GridSpread) * GridSpread
        Dim maxv As Double = Math.Ceiling(Data(Data.Count - 1) / GridSpread) * GridSpread

        If Not (MaxValue = 0 And MinValue = 0) Then
            minv = Math.Floor(MinValue / GridSpread) * GridSpread
            maxv = Math.Ceiling(MaxValue / GridSpread) * GridSpread
        Else
            minv = Math.Floor(Data(0) / GridSpread) * GridSpread
            maxv = Math.Ceiling(Data(Data.Count - 1) / GridSpread) * GridSpread
        End If

        If Math.Abs((maxv - minv) / GridSpread) > 10 Then
            While Math.Abs((maxv - minv) / GridSpread) > 10
                GridSpread *= 2
            End While
        ElseIf Math.Abs((maxv - minv) / GridSpread) < 5 Then
            While Math.Abs((maxv - minv) / GridSpread) < 5
                GridSpread /= 2
            End While
        End If

        If Not (MaxValue = 0 And MinValue = 0) Then
            minv = (Math.Floor(MinValue / GridSpread) * GridSpread) - (GridSpread / 2)
            maxv = (Math.Ceiling(MaxValue / GridSpread) * GridSpread) + (GridSpread / 2)
        Else
            minv = (Math.Floor(Data(0) / GridSpread) * GridSpread) - (GridSpread / 2)
            maxv = (Math.Ceiling(Data(Data.Count - 1) / GridSpread) * GridSpread) + (GridSpread / 2)
        End If

        itv = New Interval(minv, maxv)

        Dim bmp As New Bitmap(Data.Count, BmpH)
        Dim pic As New Bitmap(Data.Count + 50, BmpH + 120)

        Using g As Graphics = Graphics.FromImage(bmp)
            Using b As SolidBrush = New SolidBrush(Color.Black)
                b.Color = Color.FromArgb(40, 0, 0, 0)
                g.Clear(Color.Transparent)

                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias

                Dim formatstr As String = "{0:0}"

                If FloatingPoints > 0 Then
                    formatstr = "{0:0."
                    For k As Integer = 0 To FloatingPoints - 1 Step 1
                        formatstr &= "0"
                    Next
                    formatstr &= "}"
                End If

                Using dotpen As Pen = New Drawing.Pen(Color.LightGray)
                    dotpen.DashPattern = New Single() {1.0F, 9.0F}
                    dotpen.Width = 1
                    Dim f As New System.Drawing.Font(FontFamily.GenericMonospace, 12, FontStyle.Bold)

                    For k As Double = itv.Min + GridSpread To itv.Max - GridSpread Step GridSpread
                        Dim nf As New PointF(0, (BmpH - 1 - itv.NormalizedParameterAt(k) * (BmpH - 1)))
                        g.DrawLine(dotpen, 0, nf.Y, bmp.Width - 1, nf.Y)
                    Next

                End Using

                Using dashpen As Pen = New Drawing.Pen(Color.LightGray)
                    dashpen.DashPattern = New Single() {5.0F, 5.0F}
                    dashpen.Width = 1.2
                    Dim f As New System.Drawing.Font(FontFamily.GenericMonospace, 12, FontStyle.Bold)

                    For k As Double = itv.Min + GridSpread / 2 To itv.Max - GridSpread / 2 Step GridSpread
                        Dim nf As New PointF(0, (BmpH - 1 - itv.NormalizedParameterAt(k) * (BmpH - 1)))
                        g.DrawLine(dashpen, 0, nf.Y, bmp.Width - 1, nf.Y)

                        Dim valstr As String = String.Format(formatstr, k)

                        g.DrawString(valstr, f, Brushes.DarkGray, 10, nf.Y)
                    Next

                End Using

                Dim ptf(bmp.Width + 4) As PointF
                Dim types(bmp.Width + 4) As Byte

                For k As Integer = 0 To bmp.Width - 1 Step 1
                    Dim thisvalue As Double = Data(k)
                    Dim nf As New PointF(k, (BmpH - 1 - itv.NormalizedParameterAt(thisvalue) * (BmpH - 1)))
                    ptf(k) = nf
                    types(k) = System.Drawing.Drawing2D.PathPointType.Line
                    If k = 0 Then types(k) = System.Drawing.Drawing2D.PathPointType.Start
                Next

                ptf(bmp.Width) = New PointF(ptf(bmp.Width - 1).X + 50, ptf(bmp.Width - 1).Y)
                ptf(bmp.Width + 1) = New PointF(ptf(bmp.Width - 1).X + 50, BmpH + 50)
                ptf(bmp.Width + 2) = New PointF(ptf(0).X - 50, BmpH + 50)
                ptf(bmp.Width + 3) = New PointF(ptf(0).X - 50, ptf(0).Y)
                ptf(bmp.Width + 4) = ptf(0)

                For k As Integer = 0 To 4 Step 1
                    types(bmp.Width + k) = System.Drawing.Drawing2D.PathPointType.Line
                Next

                Dim gp As New System.Drawing.Drawing2D.GraphicsPath(ptf, types)
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias
                g.FillPath(b, gp)
                Using histpen As New Drawing.Pen(Color.Black)
                    histpen.Width = 1.5
                    g.DrawPath(histpen, gp)
                End Using


            End Using
        End Using

        Using g As Graphics = Graphics.FromImage(pic)
            g.Clear(Color.Transparent)
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias
            g.DrawImage(bmp, 25, 25)
            g.DrawRectangle(Pens.Black, 25, 25, bmp.Width, bmp.Height)
            Dim f As New System.Drawing.Font(FontFamily.GenericMonospace, 10, FontStyle.Regular)

            g.DrawString(NameBase & vbCrLf & FileName & vbCrLf & "Standard Deviation: " & stanDev & vbCrLf & "Average Deviation: " & averDev, f, Brushes.Black, 25, pic.Height - 85)

            Directory.CreateDirectory(Dir & "\Histograms\" & NameBase)
            pic.Save(Dir & "\Histograms\" & NameBase & "\" & Math.Round(Data(Data.Count - 1), 1) & "-" & Math.Round(Data(0), 1) & " " & FileName & ".png")
        End Using

    End Sub


    Public Function AverageData(d As List(Of Double)) As Double
        Dim av As Double = 0
        For Each n As Double In d
            av += n
        Next
        Return av / d.Count
    End Function

    Public Function VarianceData(d As List(Of Double)) As Double
        Dim av As Double = AverageData(d)

        Dim v As Double
        For i As Integer = 0 To d.Count - 1 Step 1
            v += (d(i) - av) ^ 2
        Next

        Return v / d.Count
    End Function

    Public Function StandardData(d As List(Of Double)) As Double
        Return Math.Sqrt(VarianceData(d))
    End Function

End Class
