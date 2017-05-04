Imports System.Drawing
Imports Grasshopper.Kernel
Imports Rhino.Geometry
Imports Volvox_Cloud

Public Class Util_MultiMerge
    Inherits GH_Component

    Sub New()
        MyBase.New("MultiMerge", "MMerge", "Merge clouds into multiple clouds.", "Volvox", "Cloud")
    End Sub

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return GuidsRelease2.Comp_MultiMerge
        End Get
    End Property

    Protected Overrides ReadOnly Property Icon As Bitmap
        Get
            Return My.Resources.Icon_mmerge
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As GH_Exposure
        Get
            Return GH_Exposure.secondary
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As GH_InputParamManager)
        pManager.AddParameter(New Param_Cloud, "Clouds", "C", "Clouds to process", GH_ParamAccess.list)
        pManager.AddIntegerParameter("Number", "N", "Number of resulting clouds", GH_ParamAccess.item)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As GH_OutputParamManager)
        pManager.AddParameter(New Param_Cloud, "Clouds", "C", "Resulting clouds", GH_ParamAccess.list)
    End Sub

    Dim globpc As List(Of GH_Cloud)
    Dim globnew() As PointCloud = Nothing
    Dim totcount As Integer
    Dim proccount As Integer = 0

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)

        Dim pcl As New List(Of GH_Cloud)
        If Not DA.GetDataList(0, pcl) Then Return
        globpc = pcl


        Dim num As Integer
        If Not DA.GetData(1, num) Then Return

        ReDim globnew(num - 1)

        If num = pcl.Count Then
            Dim counter As Integer
            For Each p As GH_Cloud In pcl
                globnew(counter) = p.Value.Duplicate
                counter += 1
            Next
            DA.SetDataList(0, globnew)
            Exit Sub
        End If

        totcount = 0
        For Each p As GH_Cloud In pcl
            totcount += p.Value.Count
        Next

        If num <= 0 Then
            Me.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Number of resulting clouds has to be > 0")
            Return
        End If

        proccount = num

        Dim ThreadList As New List(Of Threading.Thread)

        For i As Integer = 0 To proccount - 1 Step 1
            Dim nt As New Threading.Thread(AddressOf GetPoints)
            nt.IsBackground = True
            ThreadList.Add(nt)
        Next

        For i As Integer = 0 To proccount - 1 Step 1
            ThreadList(i).Start(i)
        Next

        For Each t As Threading.Thread In ThreadList
            t.Join()
        Next

        DA.SetDataList(0, globnew)

        globnew = Nothing
        globpc = Nothing

    End Sub

    Sub GetPoints(myindex As Integer)

        Dim mypc As New PointCloud

        Dim i0 As Integer = myindex * Math.Ceiling(totcount / proccount)
        Dim i1 As Integer = Math.Min((myindex + 1) * Math.Ceiling(totcount / proccount) - 1, totcount - 1)

        Dim last1 As Integer = 0
        Dim last2 As Integer = 0
        Dim itvs As New List(Of Interval)

        For Each p As GH_Cloud In globpc
            last2 += p.Value.Count - 1
            itvs.Add(New Interval(last1, last2))
            last1 += p.Value.Count
            last2 += 1
        Next

        Dim pccount As Integer = 0
        Dim mycount As Integer = i0

        For i As Integer = 0 To itvs.Count - 1 Step 1
            If itvs(i).IncludesParameter(i0) Then
                pccount = i
                mycount -= itvs(i).T0
                Exit For
            End If
        Next

        For i As Integer = i0 To i1 Step 1

            If mycount > globpc(pccount).Value.Count - 1 Then
                mycount = 0
                pccount += 1
                If pccount > globpc.Count - 1 Then Exit For
            End If

            Dim thisp As PointCloudItem = globpc(pccount).Value.Item(mycount)

            mypc.AppendNew()
            mypc(mypc.Count - 1).Location = thisp.Location
            If globpc(pccount).Value.ContainsColors Then mypc(mypc.Count - 1).Color = thisp.Color
            If globpc(pccount).Value.ContainsNormals Then mypc(mypc.Count - 1).Normal = thisp.Normal

            mycount += 1
        Next

        globnew(myindex) = mypc

    End Sub

End Class
