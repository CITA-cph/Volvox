Imports System.Drawing
Imports System.IO
Imports Grasshopper.Kernel
Imports Rhino.Geometry

Public Class Util_SaveE57
    Inherits GH_Component

    Sub New()
        MyBase.New("Save E57 Ex", "SaveE57Ex", "Save E57 file.", "Volvox", "I/O")
    End Sub

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return New Guid("ff7b9def-b6ee-401d-9880-eadd3ccd36f9")
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As GH_Exposure
        Get
            Return GH_Exposure.tertiary
        End Get
    End Property

    Protected Overrides ReadOnly Property Icon As Bitmap
        Get
            Return My.Resources.Icon_SaveE57Ex
        End Get
    End Property


    Private Function RetrieveData(ByRef Da As IGH_DataAccess) As Boolean
        If Not (Da.GetData(0, X.fpath)) Then Return False

        X.fpath = RemoveIllegal(X.fpath)
        X.fpath = ValidatePath(X.fpath)

        If Not (Da.GetData(1, X.fguid)) Then
            X.fguid = Guid.NewGuid.ToString
        End If

        Da.GetData(2, X.fmeta)

        If Not (Da.GetDataList(3, X.fpclo)) Then Return False
        If Not (Da.GetDataList(4, X.spose)) Then
            For i As Integer = 0 To X.fpclo.Count - 1 Step 1
                If X.fpclo(i) Is Nothing Then Return False
                X.spose.Add(Plane.WorldXY)
            Next
        End If
        If Not (Da.GetDataList(5, X.sguid)) Then
            For i As Integer = 0 To X.fpclo.Count - 1 Step 1
                X.sguid.Add(Guid.NewGuid.ToString)
            Next
        End If

        If Not (Da.GetDataList(6, X.sname)) Then AddEmpty(X.sname, X.fpclo.Count)
        If Not (Da.GetDataList(7, X.sdesc)) Then AddEmpty(X.sdesc, X.fpclo.Count)
        If Not (Da.GetDataList(8, X.svend)) Then AddEmpty(X.svend, X.fpclo.Count)
        If Not (Da.GetDataList(9, X.smode)) Then AddEmpty(X.smode, X.fpclo.Count)
        If Not (Da.GetDataList(10, X.sseri)) Then AddEmpty(X.sseri, X.fpclo.Count)
        If Not (Da.GetDataList(11, X.shard)) Then AddEmpty(X.shard, X.fpclo.Count)
        If Not (Da.GetDataList(12, X.ssoft)) Then AddEmpty(X.ssoft, X.fpclo.Count)
        If Not (Da.GetDataList(13, X.sfirm)) Then AddEmpty(X.sfirm, X.fpclo.Count)
        If Not (Da.GetDataList(14, X.stemp)) Then AddEmpty(X.stemp, X.fpclo.Count)
        If Not (Da.GetDataList(15, X.shumi)) Then AddEmpty(X.shumi, X.fpclo.Count)
        If Not (Da.GetDataList(16, X.spres)) Then AddEmpty(X.spres, X.fpclo.Count)
        If Not (Da.GetDataList(17, X.sstim)) Then AddEmpty(X.sstim, X.fpclo.Count)
        If Not (Da.GetDataList(18, X.setim)) Then AddEmpty(X.setim, X.fpclo.Count)
        If Not (Da.GetDataList(19, X.ssboo)) Then AddEmpty(X.ssboo, X.fpclo.Count)
        If Not (Da.GetDataList(20, X.seboo)) Then AddEmpty(X.seboo, X.fpclo.Count)
        If Not (Da.GetData(21, X.spher)) Then Return False
        Return True
    End Function

    Protected Overrides Sub RegisterInputParams(pManager As GH_InputParamManager)
        pManager.AddParameter(New Param_SaveE57)
        pManager.AddTextParameter("Guid", "Guid", "File Guid", GH_ParamAccess.item)
        pManager.AddTextParameter("Metadata", "Metadata", "Information describing the Coordinate Reference System used for the file", GH_ParamAccess.item)
        pManager.AddParameter(New Param_Cloud(), "Scans", "Scans", "Point clouds to save in the file", GH_ParamAccess.list)
        pManager.AddPlaneParameter("Pose", "Pose", "Point cloud positions", GH_ParamAccess.list)

        pManager.AddTextParameter("Scan Guid", "Scan Guid", "Guid for each scan", GH_ParamAccess.list)
        pManager.AddTextParameter("Name", "Name", "Name for each scan", GH_ParamAccess.list)
        pManager.AddTextParameter("Description", "Desc", "Description for each scan", GH_ParamAccess.list)
        pManager.AddTextParameter("Vendor", "Vendor", "Sensor vendor for each scan", GH_ParamAccess.list)
        pManager.AddTextParameter("Model", "Model", "Sensor model for each scan", GH_ParamAccess.list)

        pManager.AddTextParameter("Serial", "Serial", "Sensor serial number for each scan", GH_ParamAccess.list)
        pManager.AddTextParameter("Hardware", "Hardware", "Sensor hardware version for each scan", GH_ParamAccess.list)
        pManager.AddTextParameter("Software", "Software", "Sensor software version for each scan", GH_ParamAccess.list)
        pManager.AddTextParameter("Firmware", "Firmware", "Sensor firmware version for each scan", GH_ParamAccess.list)
        pManager.AddNumberParameter("Temperature", "Temp", "Temperature", GH_ParamAccess.list)

        pManager.AddNumberParameter("Humidity", "Humid", "Humidity", GH_ParamAccess.list)
        pManager.AddNumberParameter("Pressure", "Press", "Pressure", GH_ParamAccess.list)
        pManager.AddTimeParameter("StartTime", "STime", "Acquisition start Time", GH_ParamAccess.list)
        pManager.AddTimeParameter("EndTime", "ETime", "Acquisition end Time", GH_ParamAccess.list)
        pManager.AddBooleanParameter("AtomS", "AtomS", "Is start date referenced with atomic clock?", GH_ParamAccess.list, False)

        pManager.AddBooleanParameter("AtomE", "AtomE", "Is end date referenced with atomic clock?", GH_ParamAccess.list, False)
        pManager.AddBooleanParameter("Spherical", "Spher", "Save as spherical coordinates", GH_ParamAccess.item, False)
        pManager.AddBooleanParameter("Run", "R", "Save the file", GH_ParamAccess.item, False)
        pManager.AddBooleanParameter("Abort", "A", "Abort", GH_ParamAccess.item, False)

        Me.Params.Input(1).Optional = True
        Me.Params.Input(2).Optional = True

        For i As Integer = 4 To Me.Params.Input.Count - 1 Step 1
            Me.Params.Input(i).Optional = True
        Next

    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As GH_OutputParamManager)

    End Sub

    Private run As Boolean = False
    Private abort As Boolean = False
    Private WithEvents X As E57_CloudExporter = Nothing
    Private done As Boolean = False

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)

        If Not (DA.GetData(22, run)) Then Return
        If Not (DA.GetData(23, abort)) Then Return

        If X Is Nothing Then
            If run Then
                done = False
                X = New E57_CloudExporter(Me)
                If RetrieveData(DA) Then
                    Me.Message = "Saving..."
                    X.Start()
                Else
                    Me.Message = "Failed to retrieve the data"
                    X.Dispose()
                    X = Nothing
                End If
            End If
        Else
            If X.IsRunning Then
                If abort Then
                    done = False
                    X.Abort()
                    X.Dispose()
                    X = Nothing
                    Me.Message = "Aborted"
                End If
            Else
                X.Dispose()
                X = Nothing
                done = False
                Me.Message = ""
            End If
        End If

        If done Then
            If X IsNot Nothing Then
                X.Dispose()
                X = Nothing
            End If
            done = False
        End If

    End Sub

    Dim Expire As Action = AddressOf ExpireComponent

    Private Sub ExpireComponent()
        Me.ExpireSolution(True)
    End Sub

    Private Sub XFinished() Handles X.FinishedLoading
        If X.IsRunning Then Threading.Thread.Sleep(100)
        Rhino.RhinoApp.MainApplicationWindow.Invoke(Expire)
    End Sub


    Private Sub AddEmpty(ByRef L As List(Of String), Count As Integer)
        For i As Integer = 0 To Count - 1 Step 1
            L.Add("")
        Next
    End Sub

    Private Sub AddEmpty(ByRef L As List(Of Boolean), Count As Integer)
        For i As Integer = 0 To Count - 1 Step 1
            L.Add(False)
        Next
    End Sub

    Private Sub AddEmpty(ByRef L As List(Of Date), Count As Integer)
        For i As Integer = 0 To Count - 1 Step 1
            L.Add(Date.Now)
        Next
    End Sub

    Private Sub AddEmpty(ByRef L As List(Of Double), Count As Integer)
        For i As Integer = 0 To Count - 1 Step 1
            L.Add(Double.NaN)
        Next
    End Sub



End Class
