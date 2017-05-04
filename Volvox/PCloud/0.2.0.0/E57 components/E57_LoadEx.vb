
Imports Grasshopper.Kernel
Imports Grasshopper.Kernel.Parameters
Imports System.IO
Imports Rhino.Geometry
Imports Volvox_Cloud
Imports Grasshopper
Imports Grasshopper.Kernel.Data
Imports System.Drawing
Imports GH_IO.Serialization
Imports System.Windows.Forms

Public Class E57_LoadEx
    Inherits GH_Component

    Sub New()
        MyBase.New("Load E57 Ex", "LoadE57Ex", "Loads selected scans from E57 file together with all available metadata.", "Volvox", "I/O")
    End Sub

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return GuidsRelease2.Comp_E57LoadEx
        End Get
    End Property

    Protected Overrides ReadOnly Property Icon As Bitmap
        Get
            Return My.Resources.Icon_LoadE57EX
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As GH_Exposure
        Get
            Return GH_Exposure.secondary
        End Get
    End Property

    Private Check As Boolean = False

    Public Overrides Function Read(reader As GH_IReader) As Boolean
        If Not reader.TryGetBoolean("Check", Check) Then
            Check = True
        End If
        Return MyBase.Read(reader)
    End Function

    Public Overrides Function Write(writer As GH_IWriter) As Boolean
        writer.SetBoolean("Check", Check)
        Return MyBase.Write(writer)
    End Function

    Protected Overrides Sub AppendAdditionalComponentMenuItems(menu As ToolStripDropDown)
        Menu_AppendItem(menu, "Check file", AddressOf CheckSwitch, True, Check)
        MyBase.AppendAdditionalComponentMenuItems(menu)
    End Sub

    Private Sub CheckSwitch()
        Check = Not Check
    End Sub

    Protected Overrides Sub RegisterInputParams(pManager As GH_InputParamManager)
        pManager.AddParameter(New Param_FilePath, "FilePath", "F", "E57 File Path", GH_ParamAccess.item)
        pManager.AddIntegerParameter("Index", "I", "Scan index, set to -1 to load all scans from the file", GH_ParamAccess.list, -1)
        pManager.AddNumberParameter("Percent", "%", "Percent of points to load", GH_ParamAccess.item, 1)
        pManager.AddBooleanParameter("Run", "R", "Load file", GH_ParamAccess.item, False)
        pManager.AddBooleanParameter("Abort", "A", "Abort loading", GH_ParamAccess.item, False)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As GH_OutputParamManager)
        pManager.AddParameter(New Param_Cloud, "Cloud", "Cloud", "Loaded Cloud", GH_ParamAccess.list)           'done in constuctor
        pManager.AddPlaneParameter("Pose", "Pose", "Scan pose", GH_ParamAccess.list)                       'done in constuctor

        pManager.AddTextParameter("Guid", "Guid", "Guid", GH_ParamAccess.list)                              'done in constuctor
        pManager.AddTextParameter("Name", "Name", "Scan name", GH_ParamAccess.list)                         'done in constuctor
        pManager.AddTextParameter("Description", "Desc", "Scan description", GH_ParamAccess.list)           'done in constuctor

        pManager.AddNumberParameter("CartesianB", "CartB", "Cartesian bounds", GH_ParamAccess.tree)         'done in constuctor
        pManager.AddNumberParameter("SphericalB", "SphB", "Spherical bounds", GH_ParamAccess.tree)          'done in constuctor
        pManager.AddNumberParameter("ColorB", "ColB", "Color limits R/G/B", GH_ParamAccess.tree)          'done in constuctor
        pManager.AddNumberParameter("IntensityB", "IntB", "Intensity limits", GH_ParamAccess.list)        'done in constuctor

        pManager.AddTextParameter("Vendor", "Vendor", "Sensor vendor", GH_ParamAccess.list)                 'done in constuctor
        pManager.AddTextParameter("Model", "Model", "Sensor model", GH_ParamAccess.list)                    'done in constuctor
        pManager.AddTextParameter("Serial", "Serial", "Sensor serial number", GH_ParamAccess.list)          'done in constuctor
        pManager.AddTextParameter("Hardware", "HW", "Sensor hardware version", GH_ParamAccess.list)         'done in constuctor
        pManager.AddTextParameter("Software", "SW", "Sensor software version", GH_ParamAccess.list)         'done in constuctor
        pManager.AddTextParameter("Firmware", "FW", "Sensor firmware version", GH_ParamAccess.list)         'done in constuctor

        pManager.AddNumberParameter("Temperature", "Temp", "Temperature", GH_ParamAccess.list)              'done in constuctor
        pManager.AddNumberParameter("Humidity", "Humid", "Humidity", GH_ParamAccess.list)                   'done in constuctor
        pManager.AddNumberParameter("Pressure", "Press", "Pressure", GH_ParamAccess.list)                   'done in constuctor

        pManager.AddNumberParameter("StartTime", "STime", "Acquisition start Time/Atomic clock referenced", GH_ParamAccess.tree)   'done in constuctor
        pManager.AddNumberParameter("EndTime", "ETime", "Acquisition end Time/Atomic clock referenced", GH_ParamAccess.tree)       'done in constuctor

    End Sub

    Public Overrides Sub RemovedFromDocument(document As GH_Document)
        If Reconstructor IsNot Nothing Then Reconstructor.Dispose()
        MyBase.RemovedFromDocument(document)
    End Sub

    WithEvents Reconstructor As E57_CloudConstructorEx = Nothing

    Private MyClouds As New List(Of GH_Cloud)
    Private MyPlanes As New List(Of Plane)
    Private MyGuids As New List(Of Guid)
    Private MyNames As New List(Of String)
    Private MyDescr As New List(Of String)
    Private MyCartes As New DataTree(Of Double)
    Private MySpheri As New DataTree(Of Double)
    Private MyColor As New DataTree(Of Double)
    Private MyIntens As New DataTree(Of Double)
    Private MyVendor As New List(Of String)
    Private MyModel As New List(Of String)
    Private MySerial As New List(Of String)
    Private MyHardware As New List(Of String)
    Private MySoftware As New List(Of String)
    Private MyFirmware As New List(Of String)
    Private MyTemp As New List(Of Double)
    Private MyPres As New List(Of Double)
    Private MyHumid As New List(Of Double)
    Private MyStart As New DataTree(Of Double)
    Private MyEnd As New DataTree(Of Double)

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)

        Dim CompFilePath As String = String.Empty
        Dim CompIndices As New List(Of Integer)
        Dim CompPercent As Double = 1
        Dim CompRun As Boolean = False
        Dim CompAbort As Boolean = False

        If Not DA.GetData(0, CompFilePath) Then Return
        If Not DA.GetDataList(1, CompIndices) Then Return
        If Not DA.GetData(2, CompPercent) Then Return
        If Not DA.GetData(3, CompRun) Then Return
        If Not DA.GetData(4, CompAbort) Then Return

        If Reconstructor IsNot Nothing Then
            If Reconstructor.Finished Then
                CopyFromReconstructor()
                Reconstructor.Dispose()
                Reconstructor = Nothing
                OutputMyData(DA)
                GC.Collect()
            Else
                DA.SetDataList(0, Reconstructor.Clouds)
                DA.SetDataList(1, Reconstructor.Planes)
            End If

            If CompAbort Then
                Reconstructor.Dispose()
                Reconstructor = Nothing
                ClearMyData()
                GC.Collect()
                Me.Message = "Aborted"
            End If
        Else
            If CompRun And Not CompAbort Then
                Me.Message = ""
                If File.Exists(CompFilePath) Then
                    If Utils_IO.IsFileLocked(New FileInfo(CompFilePath)) Then
                        Me.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "File is in use by another process")
                        Me.Message = "File in use"
                        Exit Sub
                    Else
                        ClearMyData()
                        Reconstructor = New E57_CloudConstructorEx(CompFilePath)
                        Reconstructor.Indices = CompIndices
                        Reconstructor.Percent = CompPercent
                        Reconstructor.Start(Check)
                    End If
                End If
            End If

            If CompAbort Then
                ClearMyData()
                Me.Message = "Aborted"
            End If
            OutputMyData(DA)
            GC.Collect()
        End If

    End Sub

    Private Sub ClearMyData()
        MyClouds.Clear()
        MyPlanes.Clear()
        MyGuids.Clear()
        MyNames.Clear()
        MyDescr.Clear()
        MyCartes.Clear()
        MySpheri.Clear()
        MyColor.Clear()
        MyIntens.Clear()
        MyVendor.Clear()
        MyModel.Clear()
        MySerial.Clear()
        MyHardware.Clear()
        MySoftware.Clear()
        MyFirmware.Clear()
        MyTemp.Clear()
        MyPres.Clear()
        MyHumid.Clear()
        MyStart.Clear()
        MyEnd.Clear()
    End Sub

    Private Sub OutputMyData(DA As IGH_DataAccess)
        DA.SetDataList(0, MyClouds)
        DA.SetDataList(1, MyPlanes)
        DA.SetDataList(2, MyGuids)
        DA.SetDataList(3, MyNames)
        DA.SetDataList(4, MyDescr)
        DA.SetDataTree(5, MyCartes)
        DA.SetDataTree(6, MySpheri)
        DA.SetDataTree(7, MyColor)
        DA.SetDataTree(8, MyIntens)
        DA.SetDataList(9, MyVendor)
        DA.SetDataList(10, MyModel)
        DA.SetDataList(11, MySerial)
        DA.SetDataList(12, MyHardware)
        DA.SetDataList(13, MySoftware)
        DA.SetDataList(14, MyFirmware)
        DA.SetDataList(15, MyTemp)
        DA.SetDataList(16, MyHumid)
        DA.SetDataList(17, MyPres)
        DA.SetDataTree(18, MyStart)
        DA.SetDataTree(19, MyEnd)
    End Sub

    Sub CopyFromReconstructor()
        MyClouds.AddRange(Reconstructor.Clouds)
        MyPlanes.AddRange(Reconstructor.Planes)
        MyGuids.AddRange(Reconstructor.Guids)
        MyNames.AddRange(Reconstructor.Names)
        MyDescr.AddRange(Reconstructor.Descriptions)
        MyVendor.AddRange(Reconstructor.Vendors)
        MyModel.AddRange(Reconstructor.Models)
        MySerial.AddRange(Reconstructor.SerialNumbers)
        MyHardware.AddRange(Reconstructor.Hardwares)
        MyFirmware.AddRange(Reconstructor.Firmwares)
        MyTemp.AddRange(Reconstructor.Temperatures)
        MyPres.AddRange(Reconstructor.Pressures)
        MyHumid.AddRange(Reconstructor.Humidities)
        MySoftware.AddRange(Reconstructor.Softwares)

        For i As Integer = 0 To MyClouds.Count - 1 Step 1
            If Reconstructor.CartesianBounds(i) IsNot Nothing Then
                MyCartes.AddRange(Reconstructor.CartesianBounds(i), New GH_Path(i))
            Else
                MyCartes.Add(0, New GH_Path(i))
            End If

            If Reconstructor.SphericalBounds(i) IsNot Nothing Then
                MySpheri.AddRange(Reconstructor.SphericalBounds(i), New GH_Path(i))
            Else
                MySpheri.Add(0, New GH_Path(i))
            End If

            If Reconstructor.ColorLimits(i) IsNot Nothing Then
                MyColor.AddRange(Reconstructor.ColorLimits(i), New GH_Path(i))
            Else
                MyColor.Add(0, New GH_Path(i))
            End If

            If Reconstructor.IntensityLimits(i) IsNot Nothing Then
                MyIntens.AddRange(Reconstructor.IntensityLimits(i), New GH_Path(i))
            Else
                MyIntens.Add(0, New GH_Path(i))
            End If

            If Reconstructor.AqcuisitionStarts(i) IsNot Nothing Then
                MyStart.AddRange(Reconstructor.AqcuisitionStarts(i), New GH_Path(i))
            Else
                MyStart.Add(0, New GH_Path(i))
            End If

            If Reconstructor.AqcuisitionEnds(i) IsNot Nothing Then
                MyEnd.AddRange(Reconstructor.AqcuisitionEnds(i), New GH_Path(i))
            Else
                MyEnd.Add(0, New GH_Path(i))
            End If

        Next
    End Sub

    Sub ReconstructorFinished() Handles Reconstructor.FinishedLoading
        If Reconstructor.Finished Then Threading.Thread.Sleep(100)
        Rhino.RhinoApp.MainApplicationWindow.Invoke(Expire)
    End Sub

    Dim MyMessage As String = String.Empty
    Dim CurrentCloud As String = String.Empty
    Dim CurrentPercent As String = "0"

    Sub ReconstructorMessage(sender As E57_CloudConstructorEx, e As E57MessageEventArgs) Handles Reconstructor.Message
        Select Case e.Type
            Case E57MessageEventArgs.MessageType.ErrorMessage
                Me.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, e.Message)
                MyMessage = e.Message
            Case E57MessageEventArgs.MessageType.WarningMessage
                Me.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, e.Message)
                MyMessage = e.Message
            Case E57MessageEventArgs.MessageType.SubPercentReport
                CurrentPercent = e.Message
                MyMessage = CurrentCloud & vbCrLf & CurrentPercent & "%"
                CurrentPercent = "0"
            Case E57MessageEventArgs.MessageType.CurrentCloudReport
                CurrentCloud = e.Message
                MyMessage = CurrentCloud & vbCrLf & CurrentPercent & "%"
            Case E57MessageEventArgs.MessageType.CustomMessage
                MyMessage = e.Message
        End Select

        Rhino.RhinoApp.MainApplicationWindow.Invoke(DrawMessage)
    End Sub

    Dim DrawMessage As Action = AddressOf ExpireDisplay
    Dim Expire As Action = AddressOf ExpireComponent

    Private Sub ExpireDisplay()
        Me.Message = MyMessage
        Me.OnDisplayExpired(False)
    End Sub

    Private Sub ExpireComponent()
        Me.ExpireSolution(True)
    End Sub

End Class


'If CompRun And Reconstructor IsNot Nothing Then
'    If Reconstructor.Finished Then
'        Reconstructor.Dispose()
'        Reconstructor = Nothing
'        GC.Collect()
'    End If
'End If

'If CompRun And (Reconstructor Is Nothing) And (Not CompAbort) Then
'    Me.Message = ""
'    GC.Collect()
'    If File.Exists(CompFilePath) Then
'        If Utils_IO.IsFileLocked(New FileInfo(CompFilePath)) Then
'            Me.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "File is in use by another process")
'            Me.Message = "File in use"
'            Exit Sub
'        Else
'            MyClouds.Clear()
'            MyPlanes.Clear()
'            MyGuids.Clear()
'            MyNames.Clear()
'            MyDescr.Clear()
'            MyCartes.Clear()
'            MySpheri.Clear()
'            MyColor.Clear()
'            MyIntens.Clear()
'            MyVendor.Clear()
'            MyModel.Clear()
'            MySerial.Clear()
'            MyHardware.Clear()
'            MySoftware.Clear()
'            MyFirmware.Clear()
'            MyTemp.Clear()
'            MyPres.Clear()
'            MyHumid.Clear()
'            MyStart.Clear()
'            MyEnd.Clear()
'            Reconstructor = New E57_CloudConstructorEx(CompFilePath)
'            Reconstructor.Indices = CompIndices
'            Reconstructor.Percent = CompPercent
'            Reconstructor.Start()
'        End If
'    End If
'End If

'If CompAbort And (Reconstructor IsNot Nothing) And (Not CompRun) Then
'    Reconstructor.Dispose()
'    Reconstructor = Nothing
'    GC.Collect()
'End If

'If CompAbort Then
'    Me.Message = "Aborted"
'    MyClouds.Clear()
'    MyPlanes.Clear()
'    GC.Collect()
'End If

'If Reconstructor IsNot Nothing Then
'    If Reconstructor.Finished Then
'        MyClouds.AddRange(Reconstructor.Clouds)
'        MyPlanes.AddRange(Reconstructor.Planes)
'        MyGuids.AddRange(Reconstructor.Guids)
'        MyNames.AddRange(Reconstructor.Names)
'        MyDescr.AddRange(Reconstructor.Descriptions)
'        MyVendor.AddRange(Reconstructor.Vendors)
'        MyModel.AddRange(Reconstructor.Models)
'        MySerial.AddRange(Reconstructor.SerialNumbers)
'        MyHardware.AddRange(Reconstructor.Hardwares)
'        MyFirmware.AddRange(Reconstructor.Firmwares)
'        MyTemp.AddRange(Reconstructor.Temperatures)
'        MyPres.AddRange(Reconstructor.Pressures)
'        MyHumid.AddRange(Reconstructor.Humidities)
'        MySoftware.AddRange(Reconstructor.Softwares)

'        For i As Integer = 0 To MyClouds.Count - 1 Step 1
'            If Reconstructor.CartesianBounds(i) IsNot Nothing Then
'                MyCartes.AddRange(Reconstructor.CartesianBounds(i), New GH_Path(i))
'            Else
'                MyCartes.Add(0, New GH_Path(i))
'            End If

'            If Reconstructor.SphericalBounds(i) IsNot Nothing Then
'                MySpheri.AddRange(Reconstructor.SphericalBounds(i), New GH_Path(i))
'            Else
'                MySpheri.Add(0, New GH_Path(i))
'            End If

'            If Reconstructor.ColorLimits(i) IsNot Nothing Then
'                MyColor.AddRange(Reconstructor.ColorLimits(i), New GH_Path(i))
'            Else
'                MyColor.Add(0, New GH_Path(i))
'            End If

'            If Reconstructor.IntensityLimits(i) IsNot Nothing Then
'                MyIntens.AddRange(Reconstructor.IntensityLimits(i), New GH_Path(i))
'            Else
'                MyIntens.Add(0, New GH_Path(i))
'            End If

'            If Reconstructor.AqcuisitionStarts(i) IsNot Nothing Then
'                MyStart.AddRange(Reconstructor.AqcuisitionStarts(i), New GH_Path(i))
'            Else
'                MyStart.Add(0, New GH_Path(i))
'            End If

'            If Reconstructor.AqcuisitionEnds(i) IsNot Nothing Then
'                MyEnd.AddRange(Reconstructor.AqcuisitionEnds(i), New GH_Path(i))
'            Else
'                MyEnd.Add(0, New GH_Path(i))
'            End If

'        Next

'        Reconstructor.Dispose()
'        Reconstructor = Nothing
'    Else
'        DA.SetDataList(0, Reconstructor.Clouds)
'        DA.SetDataList(1, Reconstructor.Planes)
'    End If
'End If

'If Reconstructor Is Nothing Then
'    DA.SetDataList(0, MyClouds)
'    DA.SetDataList(1, MyPlanes)
'    DA.SetDataList(2, MyGuids)
'    DA.SetDataList(3, MyNames)
'    DA.SetDataList(4, MyDescr)
'    DA.SetDataTree(5, MyCartes)
'    DA.SetDataTree(6, MySpheri)
'    DA.SetDataTree(7, MyColor)
'    DA.SetDataTree(8, MyIntens)
'    DA.SetDataList(9, MyVendor)
'    DA.SetDataList(10, MyModel)
'    DA.SetDataList(11, MySerial)
'    DA.SetDataList(12, MyHardware)
'    DA.SetDataList(13, MySoftware)
'    DA.SetDataList(14, MyFirmware)
'    DA.SetDataList(15, MyTemp)
'    DA.SetDataList(16, MyHumid)
'    DA.SetDataList(17, MyPres)
'    DA.SetDataTree(18, MyStart)
'    DA.SetDataTree(19, MyEnd)
'    GC.Collect()
'End If