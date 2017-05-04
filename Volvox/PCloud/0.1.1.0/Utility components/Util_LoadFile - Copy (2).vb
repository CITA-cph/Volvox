Imports System.Drawing
Imports System.IO
Imports Grasshopper.Kernel
Imports Rhino.Geometry

Public Class Util_LoadFile
    Inherits GH_Component

    Dim Rnd As New Random

    Sub New()
        MyBase.New("Load Cloud", "CloudLoad", "Loads point cloud from file.", "Volvox", "Util")
    End Sub

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return GuidsRelease1.Comp_LoadFile
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As GH_Exposure
        Get
            Return GH_Exposure.secondary
        End Get
    End Property

    Protected Overrides ReadOnly Property Icon As Bitmap
        Get
            Return My.Resources.Icon_Load
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As GH_InputParamManager)
        pManager.AddParameter(New Grasshopper.Kernel.Parameters.Param_FilePath, "File path", "F", "File path", GH_ParamAccess.item)
        pManager.AddTextParameter("Mask", "M", "Mask to apply", GH_ParamAccess.item)
        pManager.AddNumberParameter("Percent", "%", "Percent of points to load, 0 to 1 range", GH_ParamAccess.item, 1)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As GH_OutputParamManager)
        pManager.AddParameter(New Param_Cloud, "Cloud", "C", "Cloud", GH_ParamAccess.item)
    End Sub

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)

        Dim fpath As String = String.Empty
        Dim fmask As String = String.Empty
        Dim perc As Double = 1

        If Not (DA.GetData(0, fpath)) Then Return
        If Not (DA.GetData(1, fmask)) Then Return
        If Not (DA.GetData(2, perc)) Then Return

        If Not File.Exists(fpath) Then
            Me.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "File doesn't exist.")
            Exit Sub
        End If

        If String.IsNullOrEmpty(fpath) Or String.IsNullOrEmpty(fmask) Then Return

        Dim pc As PointCloud = LoadPoints(fpath, fmask, perc, 123)

        DA.SetData(0, pc)

    End Sub

    Function LoadPoints(MyFilePAth As String, MyMask As String, MyPercent As Double, MySeed As Integer) As PointCloud

        Dim ThreaderList As New List(Of Threading_Load)
        Dim seedcounter As Integer = 0

        Using sread As StreamReader = New StreamReader(MyFilePAth)
            Dim count As Integer
            Dim templist As New List(Of String)

            Do
                If seedcounter >= Int32.MaxValue - 1000 Then seedcounter = -seedcounter
                seedcounter += 321


                Dim templine As String = sread.ReadLine
                If templine Is Nothing Then
                    If templist.Count > 0 Then
                        Dim nth As New Threading_Load(New List(Of String)(templist))
                        SyncLock ThreaderList
                            ThreaderList.Add(nth)
                            ThreaderList(ThreaderList.Count - 1).Start(MyMask, MyPercent, MySeed + seedcounter)
                        End SyncLock
                        templist.Clear()
                        count = 0
                    End If

                    Exit Do
                Else

                    templist.Add(templine)
                    count += 1

                    If count = 10000 Then
                        Dim nth As New Threading_Load(New List(Of String)(templist))
                        SyncLock ThreaderList
                            ThreaderList.Add(nth)
                            ThreaderList(ThreaderList.Count - 1).Start(MyMask, MyPercent, MySeed + seedcounter)
                        End SyncLock
                        templist.Clear()
                        count = 0
                    End If

                End If
            Loop

        End Using

        SyncLock ThreaderList
            For Each t As Threading_Load In ThreaderList
                t.t.Join()
            Next
        End SyncLock

        Dim newpointcloud As New PointCloud

        For Each t As Threading_Load In ThreaderList
            newpointcloud.Merge(t.outcloud)
        Next

        ThreaderList.Clear()

        Return newpointcloud

    End Function

End Class