Imports System.IO
Imports Grasshopper.Kernel
Imports Rhino.Geometry
Imports System.Threading
Imports Volvox_Cloud

Public Class FLSCloud
    Inherits GH_Component

    Sub New()
        MyBase.New("FLS Cloud", "FLS Cloud", "Loads point cloud from FARO .fls file.", "Volvox", "FARO")
    End Sub

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return New Guid("1632fe8e-e47a-46ca-bc09-cad84a7fa8bd")
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As GH_Exposure
        Get
            Return GH_Exposure.secondary
        End Get
    End Property

    Public Overrides Sub RemovedFromDocument(document As GH_Document)

        If Thread IsNot Nothing Then
            AbortThread = New Threading.Thread(AddressOf Abort)
            AbortThread.Priority = Threading.ThreadPriority.Highest
            AbortThread.IsBackground = True
            AbortThread.Start()
        End If

        MyBase.RemovedFromDocument(document)
    End Sub

    Protected Overrides Sub RegisterInputParams(pManager As GH_InputParamManager)
        pManager.AddParameter(New Grasshopper.Kernel.Parameters.Param_FilePath, "File path", "F", "File path", GH_ParamAccess.list)
        pManager.AddIntegerParameter("Step", "S", "Subsampling (pick every Nth point)", GH_ParamAccess.item, 1)
        pManager.AddBooleanParameter("Run", "R", "Run component", GH_ParamAccess.item, False)
        '   pManager.AddBooleanParameter("Abort", "A", "Abort loading", GH_ParamAccess.item, False)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As GH_OutputParamManager)
        pManager.AddParameter(New Param_Cloud, "Cloud", "C", "Cloud", GH_ParamAccess.list)
    End Sub

    Dim Thread As Threading.Thread = Nothing
    Dim AbortThread As Threading.Thread = Nothing
    Dim ThreadFinished As Boolean = False

    Dim GlobalFilePaths As List(Of String) = Nothing
    Dim GlobalStep As Integer = 1
    Dim Clouds As New List(Of PointCloud)

    Dim DAAbort As Boolean
    Dim DAStep As Integer

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)

        Dim DAFilepath As New List(Of String)
        Dim DARun As Boolean

        If Not (DA.GetDataList(0, DAFilepath)) Then Return
        If Not (DA.GetData(1, DAStep)) Then Return
        If Not (DA.GetData(2, DARun)) Then Return
        'If Not (DA.GetData(3, DAAbort)) Then Return

        For Each fp As String In DAFilepath
            If Not File.Exists(fp) Then
                Me.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "File " & fp & " doesn't exist.")
                Exit Sub
            End If
        Next

        If DAAbort Then
            If Thread IsNot Nothing Then
                AbortThread = New Threading.Thread(AddressOf Abort)
                AbortThread.Priority = Threading.ThreadPriority.Highest
                AbortThread.IsBackground = True
                AbortThread.Start()
            End If
        End If

        If DARun Then
            If Thread Is Nothing OrElse Thread.ThreadState = ThreadState.Stopped Then
                Me.Message = "Loading..."

                For Each c As PointCloud In Clouds
                    c.Dispose()
                Next

                ThreadFinished = False

                GlobalFilePaths = DAFilepath
                GlobalStep = DAStep

                Thread = New Threading.Thread(AddressOf loadFlsCloud)
                Thread.Priority = Threading.ThreadPriority.Highest
                Thread.IsBackground = True
                Thread.Start()
            End If
        End If

        If ThreadFinished Then
            Me.Message = "Done"
            ThreadFinished = False
            Thread = Nothing

            'output the data
            If GlobalStep = 1 Then
                DA.SetDataList(0, Clouds)
            ElseIf GlobalStep > 1 Then
                InvokeMessage("Subsampling")

                Dim pcs As New List(Of PointCloud)
                For i As Integer = 0 To Clouds.Count - 1 Step 1
                    Dim pc As New PointCloud
                    For j As Integer = 0 To Clouds.Count - 1 Step GlobalStep
                        pc.Add(Clouds(i)(j).Location)
                        pc(pc.Count - 1).Color = Clouds(i)(j).Color
                    Next
                    Clouds(i).Dispose()
                Next

                DA.SetDataList(0, pcs)

                InvokeMessage("Done")
            End If
        End If

    End Sub

    Private Sub Abort()
        Thread.Abort()
        Me.Message = "Aborting"
        Thread.Join()

        For Each c As PointCloud In Clouds
            c.Dispose()
        Next

        ThreadFinished = False
        Thread = Nothing
        Me.Message = "Aborted"

        FaroS.Uninitialize()

        Dim Expire As Action = AddressOf ExpireComponent
        Rhino.RhinoApp.MainApplicationWindow.Invoke(Expire)
        Return
    End Sub

    Private Sub ClearClouds()
        For i As Integer = 0 To Clouds.Count - 1 Step 1
            Clouds(i).Dispose()
        Next
        Clouds.Clear()
    End Sub

    Private Sub loadFlsCloud()

        ClearClouds()

        If FaroS.Initialize() = FaroS.Result.Failure Then InvokeMessage("Init failed") : Return
        If FaroS.IsInitialized = FaroS.Result.Failure Then InvokeMessage("Init failed") : Return

        For k As Integer = 0 To GlobalFilePaths.Count - 1 Step 1
            InvokeMessage("Loading " & vbCrLf & GlobalFilePaths(k))

            SyncLock GlobalFilePaths
                If FaroS.Load(GlobalFilePaths(k)) = FaroS.Result.Failure Then InvokeMessage("Load failed") : Return
            End SyncLock

            Dim xyz As Double() = Nothing
            Dim col As Integer() = Nothing

            InvokeMessage("Copying data")

            If FaroS.GetXYZPoints(0, xyz, col, 1) = FaroS.Result.Failure Then InvokeMessage("Getting points failed") : Return
            Dim pc As New PointCloud

            InvokeMessage("Creating cloud")

            For i As Integer = 0 To col.Length - 1 Step 1
                pc.Add(New Point3d(xyz(i * 3), xyz(i * 3 + 1), xyz(i * 3 + 2)))
                pc(pc.Count - 1).Color = Color.FromArgb(col(i), col(i), col(i))
            Next

            SyncLock Clouds
                Clouds.Add(pc)
            End SyncLock

            FaroS.UnloadAll()
        Next

        FaroS.Uninitialize()

        ThreadFinished = True
        Dim dlg As Action = AddressOf ExpireComponent
        Rhino.RhinoApp.MainApplicationWindow.Invoke(dlg)

    End Sub

    Private Sub InvokeMessage(Message As String)
        m_mess = Message
        Dim dlg As Action = AddressOf ExpireDisplay
        Rhino.RhinoApp.MainApplicationWindow.Invoke(dlg)
    End Sub

    Private Sub ExpireComponent()
        Me.ExpireSolution(True)
    End Sub

    Private m_mess As String = ""

    Private Sub ExpireDisplay()
        Me.Message = m_mess
        Me.OnDisplayExpired(True)
    End Sub

    Public Enum Result
        Success
        Failure
    End Enum

End Class