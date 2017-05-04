Imports System.Drawing
Imports Grasshopper.Kernel
Imports Rhino.Geometry

Public Class HitCountDeduction
    Inherits GH_Component

    Sub New()
        MyBase.New("DeductMissing", "DeductMissing", "Deducts missing building parts based on point cloud count and potential scan count.", "Volvox", "Diff")
    End Sub
    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return New Guid("be27f8f0-4077-4751-b37f-19f9150c46ad")
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As GH_InputParamManager)
        pManager.AddParameter(New Param_Cloud, "Cloud", "C", "Clouds to analyze", GH_ParamAccess.list)
        pManager.AddIntegerParameter("Hit Count", "H", "Potential hit count", GH_ParamAccess.list)
        pManager.AddNumberParameter("Tolerance", "T", "Actual hit count / potential hit count tolerance", GH_ParamAccess.item)
        pManager.AddColourParameter("Color", "C", "Optional highlight color", GH_ParamAccess.item)
        Me.Params.Input(2).Optional = True
        Me.Params.Input(3).Optional = True
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As GH_OutputParamManager)
        pManager.AddParameter(New Param_Cloud, "Cloud", "C", "Clouds to analyze", GH_ParamAccess.list)
        pManager.AddIntegerParameter("Indices", "I", "Selected cloud indices", GH_ParamAccess.list)
    End Sub

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)
        Dim cloudsin As New List(Of PointCloud)
        Dim hitsin As New List(Of Integer)
        Dim colin As New Color

        Dim cloudsout As New List(Of PointCloud)
        Dim intsout As New List(Of Integer)

        If Not DA.GetDataList(0, cloudsin) Then Return
        If Not DA.GetDataList(1, hitsin) Then Return

        If hitsin.Count <> cloudsin.Count Then
            Me.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Hits count and Clouds count has to have the same length")
            Return
        End If

        Dim toler As Double
        If Not DA.GetData(2, toler) Then Return

        Dim colorize As Boolean = False
        If DA.GetData(3, colin) Then colorize = True


        For i As Integer = 0 To cloudsin.Count - 1 Step 1
            Dim thiscount As Integer = cloudsin(i).Count
            Dim thishits As Integer = hitsin(i)

            If thiscount / thishits < toler Then
                cloudsout.Add(cloudsin(i))
                intsout.Add(i)
            End If
        Next

        If colorize Then
            For i As Integer = 0 To cloudsout.Count - 1 Step 1
                cloudsout(i) = cloudsout(i).Duplicate
                Dim thisc As PointCloud = cloudsout(i)
                thisc.ClearColors()
                For j As Integer = 0 To thisc.Count - 1 Step 1
                    thisc(j).Color = colin
                Next
            Next
        End If

        DA.SetDataList(0, cloudsout)
        DA.SetDataList(1, intsout)

    End Sub
End Class
