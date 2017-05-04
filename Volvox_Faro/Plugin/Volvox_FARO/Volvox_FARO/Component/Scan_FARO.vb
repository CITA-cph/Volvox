Imports System.Runtime.InteropServices.ComTypes
Imports Grasshopper.Kernel
Imports IQOPENLib
Imports LSSDKLib
Imports System.Threading

Public Class Scan_FARO
    Inherits Grasshopper.Kernel.GH_Component
    Implements _IScanCtrlSDKEvents

    Sub New()
        MyBase.New("Scan", "Scan", "FARO scan", "Volvox", "FARO")
    End Sub

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return New Guid("7f9f411f-0ac0-451e-b5fe-bcac3fb7ab5c")
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As GH_InputParamManager)
        pManager.AddTextParameter("IP", "IP", "IP address of the scanner", GH_ParamAccess.item)
        pManager.AddTextParameter("Name", "N", "Scan name", GH_ParamAccess.item, "Scan")
        pManager.AddTextParameter("Directory", "D", "Directory", GH_ParamAccess.item, String.Empty)
        pManager.AddParameter(New Param_ScanSettings)
        pManager.AddBooleanParameter("Run", "R", "Scan", GH_ParamAccess.item, False)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As GH_OutputParamManager)
        pManager.AddTextParameter("File", "F", "File", GH_ParamAccess.item)
    End Sub

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)
        Dim run As Boolean = False

        If Not DA.GetData(4, run) Then Return

        Dim dorun As Boolean = False

        If (m_thread IsNot Nothing AndAlso m_thread.ThreadState = ThreadState.Stopped And run) Or (run And m_thread Is Nothing) Then
            dorun = True
        End If

        If dorun Then
            Me.Message = ""

            If Not DA.GetData(0, m_ip) Then Return
            If Not DA.GetData(1, m_name) Then Return
            If Not DA.GetData(2, m_dir) Then Return
            If Not System.IO.Directory.Exists(m_dir) Then System.IO.Directory.CreateDirectory(m_dir)

            Dim m_ghs As New GH_ScanSettings(New ScanSettings())
            If Not DA.GetData(3, m_ghs) Then Return

            m_sets = m_ghs.Value.Duplicate

            m_thread = New Threading.Thread(AddressOf scanStart)
            m_thread.Start()
        End If

        If Me.Message = "Complete" Then
            If m_dir <> String.Empty Then DA.SetData(0, m_dir.Replace("\", "/") & "/" & m_name & m_scannum.ToString("000") & ".fls")
        End If

    End Sub

    Dim m_thread As System.Threading.Thread = Nothing

    Dim m_name As String = ""
    Dim m_ip As String = ""
    Dim m_dir As String = ""
    Dim m_sets As ScanSettings = Nothing
    Dim m_scannum As Integer = 0

    Dim cookie As Integer
    Dim icp As IConnectionPoint = Nothing
    Dim licsdk As IiQLicensedInterfaceIf = Nothing 'New ScanCtrlSDKClass()

    Public Sub scanStart()
        If licsdk Is Nothing Then licsdk = New ScanCtrlSDKClass()
        Dim license As String = "FARO Open Runtime License" & vbLf + "Key:Q44ELPNKTAKXFM6T83ZUSZTPL" & vbLf & vbLf + "The software is the registered property of FARO Scanner Production GmbH, Stuttgart, Germany." & vbLf + "All rights reserved." & vbLf & "This software may only be used with written permission of FARO Scanner Production GmbH, Stuttgart, Germany."
        licsdk.License = license

        Dim sc As IScanCtrlSDK = DirectCast(licsdk, IScanCtrlSDK)
        sc.ScannerIP = m_ip

        InvokeMessage("Connecting...")

        sc.connect()

        If Not sc.Connected Then InvokeMessage("Can't connect") : Exit Sub

        Thread.Sleep(5000)

        SyncLock m_sets
            sc.HorizontalAngleMin = m_sets.HorizontalAngleMin
            sc.HorizontalAngleMax = m_sets.HorizontalAngleMax
            sc.VerticalAngleMin = m_sets.VerticalAngleMin
            sc.VerticalAngleMax = m_sets.VerticalAngleMax
            sc.MeasurementRate = m_sets.Rate
            sc.Resolution = m_sets.Resolution
            sc.NoiseCompression = m_sets.Compression
        End SyncLock

        sc.ScanBaseName = m_name

        If m_dir = String.Empty Then
            sc.StorageMode = StorageMode.SMLocal
        Else
            sc.StorageMode = StorageMode.SMRemote
            sc.RemoteScanStoragePath = m_dir.Replace("\", "/")
        End If

        m_scannum = sc.ScanFileNumber + 1

        InvokeMessage("Syncing")
        sc.syncParam()

        Thread.Sleep(5000)

        Dim icpc As IConnectionPointContainer = DirectCast(sc, IConnectionPointContainer)
        Dim g As Guid = GetType(_IScanCtrlSDKEvents).GUID
        icpc.FindConnectionPoint(g, icp)
        icp.Advise(Me, cookie)

        Threading.Thread.Sleep(5000)

        InvokeMessage("Scanning...")
        sc.startScan()

    End Sub

    Public Sub scanCompleted() Implements _IScanCtrlSDKEvents.scanCompleted
        icp.Unadvise(cookie)

        'figure out the file path 

        m_thread = Nothing
        InvokeMessage("Complete")
        ' Interlocked.Exchange(m_busy, 0)
        InvokeExpire()
    End Sub

    Sub InvokeMessage(Message As String)
        Me.Message = Message
        Dim exp As Action = AddressOf ExpireDisplay
        If Rhino.RhinoApp.MainApplicationWindow.InvokeRequired Then Rhino.RhinoApp.MainApplicationWindow.Invoke(exp)
    End Sub

    Sub ExpireDisplay()
        Me.OnDisplayExpired(True)
    End Sub

    Sub InvokeExpire()
        Dim exp As Action = AddressOf ExpireComponent
        If Rhino.RhinoApp.MainApplicationWindow.InvokeRequired Then Rhino.RhinoApp.MainApplicationWindow.Invoke(exp)
    End Sub

    Sub ExpireComponent()
        Me.ExpireSolution(True)
    End Sub


End Class
