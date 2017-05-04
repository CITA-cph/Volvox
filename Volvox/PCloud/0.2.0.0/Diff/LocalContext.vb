Imports Grasshopper.Kernel
Imports Rhino.Geometry

Public Class LocalContext
    Inherits GH_Component

    Sub New()
        MyBase.New("Local Context", "LocalContext", "Show clouds within the local context", "Volvox", "Diff")
    End Sub
    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return New Guid("91ac1aa6-3625-48ff-912c-02653ae20800")
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As GH_InputParamManager)
        pManager.AddParameter(New Param_Cloud, "Cloud", "S", "Selected clouds", GH_ParamAccess.list)
        pManager.AddParameter(New Param_Cloud, "Context", "C", "Potential context", GH_ParamAccess.list)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As GH_OutputParamManager)
        pManager.AddParameter(New Param_Cloud, "Context", "C", "Context", GH_ParamAccess.list)
    End Sub

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)
        Dim sel As New List(Of PointCloud)
        Dim con As New List(Of PointCloud)

        If Not DA.GetDataList(0, sel) Then Return
        If Not DA.GetDataList(1, con) Then Return

        Dim det As New List(Of PointCloud)
        Dim rnd As New Random

        For i As Integer = 0 To sel.Count - 1 Step 1
            Dim tsel As PointCloud = sel(i)
            For j As Integer = 0 To con.Count - 1 Step 1
                Dim tcon As PointCloud = con(j)

                Dim bb1 As BoundingBox = tsel.GetBoundingBox(False)
                Dim bb2 As BoundingBox = tcon.GetBoundingBox(False)

                If bb1.GetCorners(0).DistanceTo(bb2.GetCorners(0)) < Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance Then
                    If bb1.GetCorners(6).DistanceTo(bb2.GetCorners(6)) < Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance Then
                        Continue For
                    End If
                End If

                Dim subs As New List(Of Point3d)

                For k As Integer = 0 To 999 Step 1
                    Dim thisp As Point3d = tcon(rnd.Next(0, tcon.Count)).Location
                    subs.Add(thisp)
                Next

                bb2 = New BoundingBox(subs)

                Dim inter As BoundingBox = BoundingBox.Intersection(bb1, bb2)

                If inter.IsValid Then
                    det.Add(tcon)
                End If

            Next
        Next

        DA.SetDataList(0, det)

    End Sub
End Class
