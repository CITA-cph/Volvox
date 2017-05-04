Imports Volvox_Cloud
Imports System.Drawing
Imports System.IO
Imports Grasshopper.Kernel
Imports Rhino.Geometry
Imports System.Threading

Public Class Util_LoadXYZEx
    Inherits GH_Component

    Dim Rnd As New Random

    Sub New()
        MyBase.New("Load .xyz Ex", "xyzLoadEx", "Loads point cloud from .xyz file.", "Volvox", "I/O")
    End Sub

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return Comp_IOLoadXYZEx
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As GH_Exposure
        Get
            Return GH_Exposure.quarternary
        End Get
    End Property

    Protected Overrides ReadOnly Property Icon As Bitmap
        Get
            Return My.Resources.Icon_LoadEX
        End Get
    End Property

    Public Overrides Sub RemovedFromDocument(document As GH_Document)

        If MasterThread IsNot Nothing Then
            MasterThread.Abort()
            For Each T As ParseThread In ParseThreads
                T.Abort()
            Next
            ParseThreads.Clear()
            OutputClouds.Clear()
            For Each pc As PointCloud In ThreadClouds
                pc.Dispose()
            Next
            MasterFinished = False
            ThreadStopwatch.Reset()
            MasterThread = Nothing
            'GC.Collect()
        End If

        MyBase.RemovedFromDocument(document)
    End Sub

    Protected Overrides Sub RegisterInputParams(pManager As GH_InputParamManager)
        pManager.AddParameter(New Grasshopper.Kernel.Parameters.Param_FilePath, "File path", "F", "File path", GH_ParamAccess.item)
        pManager.AddTextParameter("Mask", "M", "Parsing mask", GH_ParamAccess.item)
        pManager.AddNumberParameter("Percent", "%", "Percent of points to load", GH_ParamAccess.item, 1)
        pManager.AddBooleanParameter("Run", "R", "Run component", GH_ParamAccess.item, False)
        pManager.AddBooleanParameter("Abort", "A", "Abort loading", GH_ParamAccess.item, False)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As GH_OutputParamManager)
        pManager.AddParameter(New Param_Cloud, "Cloud", "C", "Cloud", GH_ParamAccess.list)
    End Sub

    Dim ThreadStopwatch As New System.Diagnostics.Stopwatch
    Dim MasterThread As Threading.Thread = Nothing
    Dim MasterFinished As Boolean = False
    Dim GlobalFilePath As String = Nothing
    Dim ParseThreads As New List(Of ParseThread)
    Dim ThreadClouds As New List(Of PointCloud)
    Dim OutputClouds As New List(Of PointCloud)

    Dim DrawMessage As Action = AddressOf PaintAmountMessage

    Private Sub PaintAmountMessage()
        If Not DAAbort Then Me.Message = LoadedPointsCount & " points"
        Me.OnDisplayExpired(False)
    End Sub

    Friend Sub InvokeDraw()
        If Rhino.RhinoApp.MainApplicationWindow.InvokeRequired Then Rhino.RhinoApp.MainApplicationWindow.Invoke(DrawMessage)
    End Sub

    Friend LoadedPointsCount As Integer
    Dim DAAbort As Boolean
    Dim GlobalPercent As Double

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)

        Dim DAFilepath As String = String.Empty
        Dim DAMask As String = String.Empty
        Dim DARun As Boolean

        Dim DAPercent As Double = 1
        GlobalPercent = DAPercent

        If Not (DA.GetData(0, DAFilepath)) Then Return
        If Not (DA.GetData(1, DAMask)) Then Return
        If Not (DA.GetData(2, DAPercent)) Then Return
        If Not (DA.GetData(3, DARun)) Then Return
        If Not (DA.GetData(4, DAAbort)) Then Return

        If Not File.Exists(DAFilepath) Then
            Me.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "File doesn't exist.")
            Exit Sub
        End If

        If DAAbort Then
            If MasterThread IsNot Nothing Then
                LoadedPointsCount = 0
                MasterThread.Abort()
                For Each T As ParseThread In ParseThreads
                    T.Abort()
                Next
                ParseThreads.Clear()
                OutputClouds.Clear()
                For Each pc As PointCloud In ThreadClouds
                    pc.Dispose()
                Next
                MasterFinished = False
                ThreadStopwatch.Reset()
                MasterThread = Nothing
            End If
            Me.Message = "Aborted"
        End If

        If DARun Then
            If MasterThread Is Nothing Then
                LoadedPointsCount = 0
                Me.Message = "Loading..."
                ThreadClouds.Clear()
                OutputClouds.Clear()
                ParseThreads.Clear()
                MasterFinished = False
                ThreadStopwatch.Start()
                GlobalFilePath = DAFilepath
                MasterThread = New Threading.Thread(AddressOf LoadPoints)
                MasterThread.Priority = Threading.ThreadPriority.Highest
                MasterThread.IsBackground = True
                MasterThread.Start(DAMask)
            End If
        End If

        If MasterFinished Then
            ParseThreads.Clear()
            MasterFinished = False
            MasterThread = Nothing
            LoadedPointsCount = 0
        End If

        DA.SetDataList(0, OutputClouds)

    End Sub


    Sub LoadPoints(TextMask As String)

        If Utils_IO.IsFileLocked(New FileInfo(GlobalFilePath)) Then
            Me.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "File is in use by another process")
            Me.Message = "File in use"
            MasterFinished = True
            Exit Sub
        End If

        Using SRead As StreamReader = New StreamReader(GlobalFilePath)

            Dim Count As Integer
            Dim TempList As New List(Of String)

            Do
                Dim TempLine As String = SRead.ReadLine
                If TempLine Is Nothing Then
                    If TempList.Count > 0 Then
                        Dim Nth As New ParseThread(New List(Of String)(TempList), Me, GlobalPercent)
                        SyncLock ParseThreads
                            ParseThreads.Add(Nth)
                            ParseThreads(ParseThreads.Count - 1).Start(TextMask)
                        End SyncLock
                        TempList.Clear()
                        Count = 0
                    End If

                    Exit Do
                Else

                    TempList.Add(TempLine)
                    Count += 1

                    If Count = 10000 Then
                        Dim Nth As New ParseThread(New List(Of String)(TempList), Me, GlobalPercent)
                        SyncLock ParseThreads
                            ParseThreads.Add(Nth)
                            ParseThreads(ParseThreads.Count - 1).Start(TextMask)
                        End SyncLock
                        TempList.Clear()
                        Count = 0
                    End If

                End If
            Loop


        End Using

        SyncLock ParseThreads
            For Each SubThread As ParseThread In ParseThreads
                SubThread.MyThread.Join()
            Next

            SyncLock ThreadClouds
                For Each SubThread As ParseThread In ParseThreads
                    ThreadClouds.Add(SubThread.OutCloud)
                Next
            End SyncLock
        End SyncLock

        Dim CollectingCloud As New PointCloud

        Dim CloudCount As Integer = 0
        Dim LastCount As Integer = 0

        For Each l As PointCloud In ThreadClouds
            CollectingCloud.Merge(l)
            CloudCount += 1
            LastCount += 1
            If CloudCount = 10000 Or LastCount = ThreadClouds.Count Then
                Dim JoiningCloud As New PointCloud(CollectingCloud)
                SyncLock OutputClouds
                    OutputClouds.Add(JoiningCloud)
                End SyncLock
                CollectingCloud.Dispose()
                CollectingCloud = New PointCloud
                CloudCount = 0
            End If
        Next

        SyncLock ThreadClouds
            For Each pc As PointCloud In ThreadClouds
                pc.Dispose()
            Next
        End SyncLock

        MasterFinished = True
        Me.Message = (ThreadStopwatch.ElapsedMilliseconds & " ms")
        ThreadStopwatch.Reset()

        Dim Expire As Action = AddressOf ExpireComponent
        Rhino.RhinoApp.MainApplicationWindow.Invoke(Expire)
        Return

    End Sub

    Private Sub ExpireComponent()
        Me.ExpireSolution(True)
    End Sub

End Class

Public Class ParseThread
    Friend TextList As New List(Of String)
    Friend OutCloud As PointCloud = Nothing
    Private LoadPercent As Double
    Friend MyThread As Threading.Thread = Nothing
    Dim Owner As Util_LoadXYZEx = Nothing

    Sub New(ParsingList As List(Of String), Owner As Util_LoadXYZEx, LoadPercent As Double)
        TextList = ParsingList
        Me.LoadPercent = LoadPercent
        MyThread = New System.Threading.Thread(AddressOf Parse)
        MyThread.IsBackground = True
        Me.Owner = Owner
    End Sub

    Sub Start(m As String)
        OutCloud = New PointCloud
        MyThread.Start(m)
    End Sub

    Sub Parse(mymask As String)

        Dim Parser As New Parse_MultiLoad(mymask)
        Dim Rnd As New Random

        For Each S As String In TextList
            Select Case Rnd.NextDouble() + LoadPercent
                Case Is > 1

                    Dim P As Multipoint = Parser.TextToMultipoint(S)

                    OutCloud.AppendNew()
                    OutCloud(OutCloud.Count - 1).Location = New Point3d(P.X, P.Y, P.Z)
                    If P.ContainsNormals Then OutCloud(OutCloud.Count - 1).Normal = New Vector3d(P.U, P.V, P.W)
                    If P.ContainsIntensity Then
                        If P.ContainsColors Then
                            Dim intens As Double = P.A / 255
                            OutCloud(OutCloud.Count - 1).Color = Color.FromArgb(P.R * intens, P.G * intens, P.B * intens)
                        Else
                            OutCloud(OutCloud.Count - 1).Color = Color.FromArgb(P.A, P.A, P.A)
                        End If
                    Else
                        If P.ContainsColors Then OutCloud(OutCloud.Count - 1).Color = Color.FromArgb(P.R, P.G, P.B)
                    End If

            End Select
        Next

        Owner.LoadedPointsCount += OutCloud.Count
        Owner.InvokeDraw()

        TextList.Clear()

    End Sub

    Sub Abort()
        If MyThread IsNot Nothing Then
            MyThread.Abort()
        End If
        TextList.Clear()
        OutCloud.Dispose()
    End Sub

End Class

