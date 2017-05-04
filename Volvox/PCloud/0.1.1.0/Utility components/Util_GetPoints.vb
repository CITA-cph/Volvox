Imports System.Drawing
Imports Grasshopper.Kernel
Imports Volvox_Cloud
Imports Rhino.Geometry

Public Class Util_GetPoints
    Inherits GH_Component

    Sub New()
        MyBase.New("Get Points", "GetPoints", "Get specific points from a cloud.", "Volvox", "Analysis")
    End Sub

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return GuidsRelease1.Comp_GetPoints
        End Get
    End Property

    Protected Overrides ReadOnly Property Icon As Bitmap
        Get
            Return My.Resources.Icon_GetPoint
        End Get
    End Property


    Protected Overrides Sub RegisterInputParams(pManager As GH_InputParamManager)
        pManager.AddParameter(New Param_Cloud, "Cloud", "C", "Cloud to deconstruct", GH_ParamAccess.item)
        Dim thisparam As Param_Cloud = pManager.Param(0)
        thisparam.Hidden = True
        pManager.AddIntegerParameter("Indices", "I", "Indices to get", GH_ParamAccess.list)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As GH_OutputParamManager)
        pManager.AddPointParameter("Points", "P", "Points", GH_ParamAccess.list)
        pManager.AddVectorParameter("Normals", "N", "Normals", GH_ParamAccess.list)
        pManager.AddColourParameter("Colors", "C", "Colors", GH_ParamAccess.list)
    End Sub

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)

        Dim cl As GH_Cloud = Nothing
        If Not DA.GetData(0, cl) Then Return
        If Not cl.IsValid Then Return

        Dim nl As New List(Of Integer)
        If Not DA.GetDataList(1, nl) Then Return
        If nl.Count < 1 Then Return

        Dim ptl As New List(Of Point3d)
        Dim col As New List(Of Color)
        Dim nor As New List(Of Vector3d)

        For i As Integer = 0 To nl.Count - 1 Step 1
            ptl.Add(cl.Value(nl(i)).Location)
        Next

        If cl.Value.ContainsNormals Then
            For i As Integer = 0 To nl.Count - 1 Step 1
                nor.Add(cl.Value(nl(i)).Normal)
            Next
        End If

        If cl.Value.ContainsColors Then
            For i As Integer = 0 To nl.Count - 1 Step 1
                col.Add(cl.Value(nl(i)).Color)
            Next
        End If

        DA.SetDataList(0, ptl)
        DA.SetDataList(1, nor)
        DA.SetDataList(2, col)

    End Sub
End Class
