Imports Grasshopper.Kernel
Imports Rhino.Geometry
Imports Volvox_Cloud

Public Class Util_MultiMerge
    Inherits GH_Component

    Sub New()
        MyBase.New("MultiMerge", "MMerge", "Merges clouds into multiple clouds.", "Volvox", "Cloud")
    End Sub

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return GuidsRelease2.Comp_MultiMerge
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As GH_Exposure
        Get
            Return GH_Exposure.tertiary
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As GH_InputParamManager)
        pManager.AddParameter(New Param_Cloud, "Clouds", "C", "Clouds to process", GH_ParamAccess.list)
        pManager.AddIntegerParameter("Number", "N", "Number of resulting clouds", GH_ParamAccess.item)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As GH_OutputParamManager)
        pManager.AddParameter(New Param_Cloud, "Clouds", "C", "Resulting clouds", GH_ParamAccess.list)
    End Sub

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)

        Dim pcl As New List(Of PointCloud)
        If Not DA.GetDataList(0, pcl) Then Return

        Dim num As Integer
        If Not DA.GetData(1, num) Then Return

        Dim totcount As Integer

        For Each p As PointCloud In pcl
            totcount += p.Count
        Next

        If num <= 0 Then
            Me.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Number of resulting clouds has to be > 0")
            Return
        End If

        Dim chunk As Integer = Math.Ceiling(totcount / num)
        Dim thiscount As Integer = 0
        Dim outpc As New List(Of PointCloud)
        Dim pts(chunk - 1) As Point3d
        Dim newp As New PointCloud(pts)

        For i As Integer = 0 To pcl.Count - 1 Step 1
            Dim thispc As PointCloud = pcl(i)

            For j As Integer = 0 To thispc.Count - 1 Step 1

                Dim previtem As PointCloudItem = thispc.Item(j)
                Dim thisitem As PointCloudItem = newp.Item(thiscount)
                thisitem.Location = previtem.Location
                If thispc.ContainsColors Then thisitem.Color = previtem.Color
                If thispc.ContainsNormals Then thisitem.Normal = previtem.Normal

                thiscount += 1

                If thiscount >= chunk Or (i = pcl.Count - 1 And j = thispc.Count - 1) Then
                    If Not (i = pcl.Count - 1 And j = thispc.Count - 1) Then
                        thiscount = 0
                        outpc.Add(newp)
                        newp = Nothing
                        newp = New PointCloud(pts)
                    Else
                        For k As Integer = newp.Count - 1 To thiscount Step -1
                            newp.RemoveAt(k)
                        Next

                        outpc.Add(newp)
                        newp = Nothing
                    End If
                End If
            Next
        Next

        DA.SetDataList(0, outpc)


    End Sub

End Class
