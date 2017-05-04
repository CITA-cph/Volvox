Imports System.Drawing
Imports Grasshopper.Kernel
Imports Rhino.Geometry

Public Class Dict_ColorDictionary
    Inherits GH_Component

    Sub New()
        MyBase.New("Preview Data", "PrevData", "Assign colors to cloud according to user data.", "Volvox", "UserData")
    End Sub

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return GuidsRelease2.Comp_DictColorDictionary
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As GH_Exposure
        Get
            Return GH_Exposure.tertiary
        End Get
    End Property

    Protected Overrides ReadOnly Property Icon As Bitmap
        Get
            Return My.Resources.Icon_PreviewData
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As GH_InputParamManager)
        pManager.AddParameter(New Param_Cloud, "Cloud", "C", "Cloud to manipulate", GH_ParamAccess.item)
        pManager.AddTextParameter("Key", "K", "Key", GH_ParamAccess.item)
        pManager.AddNumberParameter("Values", "V", "Values", GH_ParamAccess.list)
        pManager.AddColourParameter("Colors", "C", "Colors", GH_ParamAccess.list)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As GH_OutputParamManager)
        pManager.AddParameter(New Param_Cloud, "Cloud", "C", "Modified cloud", GH_ParamAccess.item)
    End Sub

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)

        Dim ProcCount As Integer = Environment.ProcessorCount

        Dim ColVal() As Color = Nothing
        Dim Itv As Interval = Nothing

        Dim pars As New List(Of Double)
        Dim cols As New List(Of Color)

        Dim strdc As String = Nothing
        Dim GlobalCloud As PointCloud = Nothing

        If Not DA.GetData(0, GlobalCloud) Then Return
        If Not DA.GetData(1, strdc) Then Return
        If Not DA.GetDataList(2, pars) Then Return
        If Not DA.GetDataList(3, cols) Then Return

        GlobalCloud = GlobalCloud.Duplicate

        Dim bmp As New Bitmap(1000, 1)
        Dim rect As Rectangle = New Rectangle(0, 0, bmp.Width, bmp.Height)

        Dim cb As New System.Drawing.Drawing2D.ColorBlend()
        Dim pos(cols.Count - 1) As Single

        For i As Integer = 0 To pos.Count - 1 Step 1
            pos(i) = pars(i)
        Next

        Dim colors() As Color = cols.ToArray
        Array.Sort(pos, colors)
        Array.Sort(pos, pos)
        Dim normpos(cols.Count - 1) As Single
        Itv = New Interval(pos(0), pos(pos.Length - 1))

        For i As Integer = 0 To normpos.Length - 1 Step 1
            normpos(i) = Itv.NormalizedParameterAt(pos(i))
        Next

        cb.Positions = normpos
        cb.Colors = colors
        Dim lin As New System.Drawing.Drawing2D.LinearGradientBrush(rect, Color.Black, Color.Black, 0, False)
        lin.InterpolationColors = cb

        ColVal = Nothing
        ReDim ColVal(bmp.Width - 1)
        Dim counter As Integer

        Using g As Graphics = Graphics.FromImage(bmp)
            g.FillRectangle(lin, rect)
            For i As Integer = 0 To bmp.Width - 1 Step 1
                ColVal(counter) = bmp.GetPixel(i, 0)
                counter += 1
            Next
        End Using

        Select Case GlobalCloud.UserDictionary(strdc).GetType
            Case GetType(List(Of Double)), GetType(Double())

                Dim DictVal As New List(Of Double)
                DictVal.AddRange(GlobalCloud.UserDictionary(strdc))
                Dim idx As Integer = ColVal.Length - 1

                For i As Integer = 0 To GlobalCloud.Count - 1 Step 1
                    Dim thisnorm As Double = Itv.NormalizedParameterAt(DictVal(i))

                    Select Case thisnorm
                        Case < (0 - Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance)
                            GlobalCloud(i).Color = Color.Black
                        Case > (1 + Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance)
                            GlobalCloud(i).Color = Color.White
                        Case Else
                            GlobalCloud(i).Color = ColVal(thisnorm * idx)
                    End Select

                Next

                DictVal = Nothing
            Case GetType(List(Of Integer)), GetType(Integer())

                Dim DictVal As New List(Of Integer)
                DictVal.AddRange(GlobalCloud.UserDictionary(strdc))

                Dim idx As Integer = ColVal.Length - 1

                For i As Integer = 0 To GlobalCloud.Count - 1 Step 1
                    Dim thisnorm As Double = Itv.NormalizedParameterAt(DictVal(i))

                    Select Case thisnorm
                        Case < (0 - Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance)
                            GlobalCloud(i).Color = Color.Black
                        Case > (1 + Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance)
                            GlobalCloud(i).Color = Color.White
                        Case Else
                            GlobalCloud(i).Color = ColVal(thisnorm * idx)
                    End Select

                Next

                DictVal = Nothing
        End Select

        DA.SetData(0, GlobalCloud)
        GlobalCloud = Nothing
        ColVal = Nothing
        bmp.Dispose()
        Itv = Nothing

    End Sub


End Class