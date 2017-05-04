Imports System.IO
Imports Grasshopper.Kernel
Imports Rhino.Geometry
Imports System.Threading
Imports Volvox_Cloud

Public Class LoadFLSimg
    Inherits GH_Component

    Sub New()
        MyBase.New("FLS Image", "FLS Image", "Loads point cloud from FARO .fls file and creates a panoramic image.", "Volvox", "FARO")
    End Sub

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return New Guid("79ea0344-faed-42dc-b08b-a8d0497abc66")
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
        '  pManager.AddBooleanParameter("Abort", "A", "Abort loading", GH_ParamAccess.item, False)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As GH_OutputParamManager)
        pManager.AddParameter(New Param_Cloud, "Cloud", "C", "Cloud", GH_ParamAccess.list)
        pManager.AddTextParameter("File path", "F", "Image file path", GH_ParamAccess.list)
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

                For i As Integer = 0 To Clouds.Count - 1 Step 1
                    Dim pc As New PointCloud
                    For j As Integer = 0 To Clouds(i).Count - 1 Step GlobalStep
                        pc.Add(Clouds(i)(j).Location)
                        pc(pc.Count - 1).Color = Clouds(i)(j).Color
                    Next
                    Clouds(i).Dispose()
                    Clouds(i) = pc
                Next

                DA.SetDataList(0, Clouds)

                InvokeMessage("Done")
            End If

            SyncLock GlobalFilePaths
                Dim imgs As New List(Of String)

                For Each fp As String In GlobalFilePaths
                    Dim pth As String = Path.GetDirectoryName(fp)
                    Dim fname As String = Path.GetFileNameWithoutExtension(fp)
                    imgs.Add(pth & "/" & fname & "_Image.png")
                Next

                DA.SetDataList(1, imgs)
            End SyncLock

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

        For k As Integer = 0 To GlobalFilePaths.Count - 1 Step 1
            If FaroS.Initialize() = FaroS.Result.Failure Then InvokeMessage("Init failed") : Return
            If FaroS.IsInitialized = FaroS.Result.Failure Then InvokeMessage("Init failed") : Return

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

            Dim nc As Integer = FaroS.columnCount(0)
            Dim nr As Integer = FaroS.rowCount(0)

            InvokeMessage("Creating bitmap")

            Dim bmp As New Bitmap(nc, nr)
            Dim bmpData As Imaging.BitmapData = bmp.LockBits(New Rectangle(0, 0, nc, nr), Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat)
            Dim ptr As IntPtr = bmpData.Scan0
            Dim bytes As Integer = Math.Abs(bmpData.Stride) * bmp.Height
            Dim rgbValues(bytes - 1) As Byte
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes)

            Dim count As Integer = 0
            Dim stride As Integer = bmpData.Stride

            Dim cnt1 As Integer = 0
            Dim cnt2 As Integer = 0

            For column As Integer = 0 To bmpData.Height - 1
                For row As Integer = 0 To bmpData.Width - 1
                    rgbValues((column * stride) + (row * 4)) = pc(cnt1).Color.R
                    rgbValues((column * stride) + (row * 4) + 1) = pc(cnt1).Color.R
                    rgbValues((column * stride) + (row * 4) + 2) = pc(cnt1).Color.R
                    rgbValues((column * stride) + (row * 4) + 3) = 255
                    cnt1 += nr
                Next
                cnt2 += 1
                cnt1 = cnt2
            Next

            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes)
            bmp.UnlockBits(bmpData)

            InvokeMessage("Bitmap created")

            Dim thisfp As String = ""

            SyncLock GlobalFilePaths
                Dim pth As String = Path.GetDirectoryName(GlobalFilePaths(k))
                If Not Directory.Exists(pth) Then Directory.CreateDirectory(pth)
                Dim fname As String = Path.GetFileNameWithoutExtension(GlobalFilePaths(k))
                bmp.Save(pth & "/" & fname & "_Image.png")
            End SyncLock

            SyncLock Clouds
                Clouds.Add(pc)
            End SyncLock

            FaroS.Uninitialize()

        Next

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