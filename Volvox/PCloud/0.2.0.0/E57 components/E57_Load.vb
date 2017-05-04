Imports E57LibReader
Imports Grasshopper.Kernel
Imports Grasshopper.Kernel.Parameters
Imports System.IO
Imports Rhino.Geometry
Imports Volvox_Cloud
Imports System.Drawing
Imports GH_IO.Serialization
Imports System.Windows.Forms

Public Class E57_Load
    Inherits GH_Component

    Sub New()
        MyBase.New("Load E57", "LoadE57", "Loads selected scans from E57 file.", "Volvox", "I/O")
    End Sub

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return GuidsRelease2.Comp_E57Load
        End Get
    End Property

    Protected Overrides ReadOnly Property Icon As Bitmap
        Get
            Return My.Resources.Icon_LoadE57
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As GH_Exposure
        Get
            Return GH_Exposure.secondary
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As GH_InputParamManager)
        pManager.AddParameter(New Param_FilePath, "FilePath", "F", "E57 File Path", GH_ParamAccess.item)
        pManager.AddIntegerParameter("Index", "I", "Scan index, set to -1 to load all scans from the file", GH_ParamAccess.list, -1)
        pManager.AddNumberParameter("Percent", "%", "Percent of points to load", GH_ParamAccess.item, 1)
        pManager.AddBooleanParameter("Run", "R", "Load file", GH_ParamAccess.item, False)
        pManager.AddBooleanParameter("Abort", "A", "Abort loading", GH_ParamAccess.item, False)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As GH_OutputParamManager)
        pManager.AddParameter(New Param_Cloud, "Cloud", "C", "Loaded Cloud", GH_ParamAccess.list)
        pManager.AddPlaneParameter("Pose", "P", "Scan pose", GH_ParamAccess.list)
    End Sub

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

    Public Overrides Sub RemovedFromDocument(document As GH_Document)
        If Reconstructor IsNot Nothing Then Reconstructor.Dispose()
        MyBase.RemovedFromDocument(document)
    End Sub

    WithEvents Reconstructor As E57_CloudConstructor = Nothing

    Private MyClouds As New List(Of GH_Cloud)
    Private MyPlanes As New List(Of Plane)

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
                MyClouds.AddRange(Reconstructor.Clouds)
                MyPlanes.AddRange(Reconstructor.Planes)
                Reconstructor.Dispose()
                Reconstructor = Nothing
                DA.SetDataList(0, MyClouds)
                DA.SetDataList(1, MyPlanes)
                GC.Collect()
            Else
                DA.SetDataList(0, Reconstructor.Clouds)
                DA.SetDataList(1, Reconstructor.Planes)
            End If

            If CompAbort Then
                Reconstructor.Dispose()
                Reconstructor = Nothing
                MyClouds.Clear()
                MyPlanes.Clear()
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
                        MyClouds.Clear()
                        MyPlanes.Clear()
                        Reconstructor = New E57_CloudConstructor(CompFilePath)
                        Reconstructor.Indices = CompIndices
                        Reconstructor.Percent = CompPercent
                        Reconstructor.Start(Check)
                    End If
                End If
            End If

            If CompAbort Then
                MyClouds.Clear()
                MyPlanes.Clear()
                Me.Message = "Aborted"
            End If

            DA.SetDataList(0, MyClouds)
            DA.SetDataList(1, MyPlanes)
            GC.Collect()
        End If

    End Sub

    Sub ReconstructorFinished() Handles Reconstructor.FinishedLoading
        If Reconstructor.Finished Then Threading.Thread.Sleep(100)
        Rhino.RhinoApp.MainApplicationWindow.Invoke(Expire)
    End Sub

    Dim MyMessage As String = String.Empty
    Dim CurrentCloud As String = String.Empty
    Dim CurrentPercent As String = "0"

    Sub ReconstructorMessage(sender As E57_CloudConstructor, e As E57MessageEventArgs) Handles Reconstructor.Message

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


'If CompRun And (Reconstructor Is Nothing) And (Not CompAbort) Then
'    Me.Message = ""
'    GC.Collect()
'    If File.Exists(CompFilePath) Then
'        If Utils_IO.IsFileLocked(New FileInfo(CompFilePath)) Then
'            Me.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "File Is In use by another process")
'            Me.Message = "File In use"
'            Exit Sub
'        Else
'            MyClouds.Clear()
'            MyPlanes.Clear()
'            Reconstructor = New E57_CloudConstructor(CompFilePath)
'            Reconstructor.Indices = CompIndices
'            Reconstructor.Percent = CompPercent
'            Reconstructor.Start()
'        End If
'    End If
'End If

'If CompRun And Reconstructor IsNot Nothing Then
'    If Reconstructor.Finished Then
'        Reconstructor.Dispose()
'        Reconstructor = Nothing
'        GC.Collect()
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
'    GC.Collect()
'End If

