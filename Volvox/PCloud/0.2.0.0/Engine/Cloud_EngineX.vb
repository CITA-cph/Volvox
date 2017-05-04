Imports Grasshopper.Kernel
Imports Grasshopper.Kernel.Data
Imports System.Drawing
Imports GH_IO.Serialization
Imports Volvox_Instr
Imports Rhino.Geometry
Imports Volvox_Cloud
Imports System.Windows.Forms
Imports Grasshopper
Imports System.IO

Public Class Cloud_Enginex
    Inherits GH_Component
    Implements IGH_VariableParameterComponent

    Public Sub New()
        MyBase.New("Cloud EngineX", "CloudX", "Point Cloud manipulation engine.", "Volvox", "Engine")
        AddHandler Me.Params.ParameterSourcesChanged, AddressOf InputChanged
    End Sub

    Protected Overrides ReadOnly Property Icon As Bitmap
        Get
            Return My.Resources.Icon_EngineX
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As GH_Exposure
        Get
            Return GH_Exposure.primary
        End Get
    End Property

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return GuidsRelease2.Comp_EngineX
        End Get
    End Property

    Public Overrides Sub CreateAttributes()
        m_attributes = New Cloud_EngineAttx(Me)
    End Sub

    Public Overrides Function Write(writer As GH_IWriter) As Boolean
        writer.SetBoolean("showinfo", ShowInfo)
        Return MyBase.Write(writer)
    End Function

    Public Overrides Function Read(reader As GH_IReader) As Boolean
        ShowInfo = reader.GetBoolean("showinfo")
        Return MyBase.Read(reader)
    End Function

    Protected Overrides Sub RegisterInputParams(pManager As Grasshopper.Kernel.GH_Component.GH_InputParamManager)
        pManager.AddParameter(New Param_Cloud(), "Cloud", "C", "Cloud to process", GH_ParamAccess.tree)
        pManager.AddBooleanParameter("Run", "R", "Run engine", GH_ParamAccess.item, False)
        pManager.AddBooleanParameter("Abort", "A", "Abort execution", GH_ParamAccess.item, False)

        Dim thisparam As Param_Cloud = pManager.Param(0)
        thisparam.Hidden = True
        pManager.AddParameter(New Param_Instr(), "Instruction0", "I0", "Instruction to execute, data tree is flattened internally.", GH_ParamAccess.tree)
        pManager.Param(3).Optional = True
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As Grasshopper.Kernel.GH_Component.GH_OutputParamManager)
        pManager.AddParameter(New Param_Cloud, "Cloud", "C", "Processed cloud", GH_ParamAccess.item)
    End Sub

    Protected Overrides Function HtmlHelp_Source() As String
        Return MyBase.HtmlHelp_Source()
    End Function

    Public Overrides Sub AppendAdditionalMenuItems(menu As ToolStripDropDown)
        MyBase.AppendAdditionalMenuItems(menu)
        Menu_AppendItem(menu, "Show Progress", AddressOf ChangeShowInfo, True, ShowInfo)

    End Sub

    Sub ChangeShowInfo()
        ShowInfo = Not ShowInfo
        Me.OnDisplayExpired(False)
    End Sub

    Sub InputChanged(sender As Object, e As GH_ParamServerEventArgs)
        If e.ParameterIndex = Me.Params.Input.Count - 1 Then
            Dim np As New Param_Instr()
            np.Access = GH_ParamAccess.tree
            np.Optional = True
            Me.Params.RegisterInputParam(np)
            ' Me.Params.RegisterOutputParam(New Grasshopper.Kernel.Parameters.Param_GenericObject)
            VariableParameterMaintenance()
            Params.OnParametersChanged()
        End If
    End Sub


    Friend ShowInfo As Boolean = True

    Dim InputClouds As New DataTree(Of PointCloud)
    Dim finished As Boolean = False
    Dim failed As Boolean = False
    Dim running As Boolean = False
    Dim inlist As New List(Of Instr_Base)
    Dim tmaster As Threading.Thread = Nothing

    Friend MessagePercent As Integer = -1
    Friend MessageTitle As String = "..."
    Friend MessageCustom As String = String.Empty

    Dim DrawMessage As Action = AddressOf ExpireDisplay
    Dim st As New System.Diagnostics.Stopwatch

    Public Overrides Sub RemovedFromDocument(document As GH_Document)

        running = False
        If tmaster IsNot Nothing Then tmaster.Abort()
        tmaster = Nothing
        AbortAllThreads()
        finished = False
        failed = False
        inlist.Clear()

        InputClouds.Clear()

        GC.Collect()

        MyBase.RemovedFromDocument(document)
    End Sub

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)

        Dim run As Boolean = False
        If Not DA.GetData(1, run) Then Return

        Dim abort As Boolean = False
        If Not DA.GetData(2, abort) Then Return

        If Not run And Not abort And tmaster IsNot Nothing Then
            Rhino.RhinoApp.MainApplicationWindow.Invoke(DrawMessage)
        End If

        If run And tmaster Is Nothing Then
            st.Start()
            running = True
            MessagePercent = -1
            MessageTitle = "..."
            MessageCustom = String.Empty
            finished = False
            failed = False

            InputClouds.Clear()

            Dim Input As New GH_Structure(Of GH_Cloud)
            If Not DA.GetDataTree(0, Input) Then Return

            For i As Integer = 0 To Input.Branches.Count - 1 Step 1
                For j As Integer = 0 To Input.Branch(i).Count - 1 Step 1
                    Dim thispc As GH_Cloud = Input.Branch(i).Item(j)
                    If thispc IsNot Nothing Then
                        InputClouds.Add(thispc.Value.Duplicate, Input.Paths(i))
                    Else
                        InputClouds.Add(Nothing, Input.Paths(i))
                    End If
                Next
            Next

            For i As Integer = 3 To Me.Params.Input.Count - 1 Step 1
                Dim tempdt As New GH_Structure(Of Instr_Base)
                If DA.GetDataTree(i, tempdt) Then
                    For Each instruction As Instr_Base In tempdt.AllData(True)
                        inlist.Add(instruction.Duplicate)
                    Next
                End If
            Next

            tmaster = New Threading.Thread(AddressOf ThreadEngine)
            tmaster.Name = "CloudEngineX_MasterThread"
            tmaster.IsBackground = True
            tmaster.Priority = Threading.ThreadPriority.Highest
            tmaster.Start()
        End If

        If failed Then
            running = False
            If tmaster IsNot Nothing Then tmaster.Abort()
            tmaster = Nothing
            AbortAllThreads()
            finished = False
            failed = False
            inlist.Clear()
            InputClouds.Clear()
            st.Reset()
            GC.Collect()

            MessageCustom = String.Empty
            Me.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Instruction failed to extecute.")
        End If

        If finished Then
            running = False
            MessagePercent = -1
            MessageTitle = String.Empty
            tmaster = Nothing
            finished = False
            inlist.Clear()
            MessageCustom = st.ElapsedMilliseconds & " ms"
            st.Reset()
            GC.Collect()
        End If

        If abort Then
            running = False
            If tmaster IsNot Nothing Then tmaster.Abort()
            tmaster = Nothing
            AbortAllThreads()
            MessagePercent = -1
            MessageTitle = "Aborted"
            MessageCustom = String.Empty
            finished = False
            failed = False
            inlist.Clear()
            InputClouds.Clear()
            st.Reset()
            GC.Collect()
        End If

        If (Not failed) And (Not abort) And (Not running) Then DA.SetDataTree(0, InputClouds)

    End Sub

    Sub ThreadEngine()
        Dim count As Integer = 0
        failed = False

        Dim cloudcount As Integer = InputClouds.DataCount
        Dim cloudthis As Integer = 1

        SyncLock inlist
            SyncLock InputClouds

                For ipath As Integer = 0 To InputClouds.Paths.Count - 1 Step 1
                    For iitem As Integer = 0 To InputClouds.Branch(ipath).Count - 1 Step 1

                        Dim cloud As PointCloud = InputClouds.Branch(ipath).Item(iitem)
                        If cloud Is Nothing Then Continue For

                        If cloud.UserDictionary.ContainsKey("VolvoxFileMask") Then

                            Dim filepath As String = cloud.UserDictionary.GetString("VolvoxFilePath")
                            Dim mask As String = cloud.UserDictionary.GetString("VolvoxFileMask")
                            Dim seed As Integer = cloud.UserDictionary.GetInteger("VolvoxFileSeed")
                            Dim percent As Double = cloud.UserDictionary.GetDouble("VolvoxFilePercent")

                            Dim instr As New Instr_LoadMulti(filepath, mask, percent, seed)
                            inlist.Insert(0, instr)

                        ElseIf cloud.UserDictionary.ContainsKey("VolvoxFilePath") Then
                            Dim filepath As String = cloud.UserDictionary.GetString("VolvoxFilePath")
                            Dim percent As Double = cloud.UserDictionary.GetDouble("VolvoxFilePercent")
                            Dim check As Boolean = cloud.UserDictionary.GetBool("VolvoxFileCheck")
                            Dim instr As New Instr_LoadE57(filepath, percent, check)
                            inlist.Insert(0, instr)
                        End If

                        Dim inputcloud As PointCloud = cloud

                        For Each instruction As Instr_Base In inlist

                            Dim rep_instruction = TryCast(instruction, Instr_BaseReporting)

                            If rep_instruction Is Nothing Then
                                MessagePercent = -1
                                MessageCustom = String.Empty
                                MessageTitle = "Cloud " & cloudthis & "/" & cloudcount & vbCrLf & instruction.InstructionType
                                Rhino.RhinoApp.MainApplicationWindow.Invoke(DrawMessage)

                                If Not instruction.Execute(inputcloud) Then
                                    MessagePercent = -1
                                    MessageTitle = "Instruction failed"
                                    failed = True
                                    Exit For
                                End If

                            Else
                                MessageTitle = "Cloud " & cloudthis & "/" & cloudcount & vbCrLf & rep_instruction.InstructionType
                                AddHandler rep_instruction.ReportingPercent, AddressOf InvokePercent
                                AddHandler rep_instruction.ReportingCustom, AddressOf InvokeCustom

                                If Not rep_instruction.Execute(inputcloud) Then
                                    MessagePercent = -1
                                    MessageTitle = "Instruction failed"
                                    MessageCustom = String.Empty
                                    failed = True
                                    Exit For
                                End If

                                RemoveHandler rep_instruction.ReportingPercent, AddressOf InvokePercent
                                RemoveHandler rep_instruction.ReportingCustom, AddressOf InvokeCustom
                            End If

                            MessageCustom = String.Empty
                        Next

                        If inputcloud.UserDictionary.ContainsKey("VolvoxFilePath") Then
                            inlist.RemoveAt(0)
                            inputcloud.UserDictionary.Remove("VolvoxFilePath")
                            inputcloud.UserDictionary.Remove("VolvoxFileMask")
                            inputcloud.UserDictionary.Remove("VolvoxFilePercent")
                            inputcloud.UserDictionary.Remove("VolvoxFileSeed")
                        End If

                        InputClouds.Branch(ipath).Item(iitem) = inputcloud
                        cloudthis += 1

                    Next
                Next

            End SyncLock
        End SyncLock

        inlist.Clear()
        finished = True

        Dim dlg As Action = AddressOf ExpireComponent
        Rhino.RhinoApp.MainApplicationWindow.Invoke(dlg)
        Return

    End Sub

    Sub AbortAllThreads()

        For i As Integer = 0 To inlist.Count - 1 Step 1
            Dim rep_instruction = TryCast(inlist(i), Instr_BaseReporting)
            If rep_instruction Is Nothing Then
            Else
                rep_instruction.Abort()
            End If
        Next

    End Sub

    Private Sub InvokePercent(ByVal sender As Object, ByVal e As Instr_BaseReporting.ReportingPercentArgs)

        MessagePercent = e.Percent
        Dim exp As Action = AddressOf ExpireDisplay
        If Rhino.RhinoApp.MainApplicationWindow.InvokeRequired Then Rhino.RhinoApp.MainApplicationWindow.Invoke(exp)

    End Sub

    Private Sub InvokeCustom(ByVal sender As Object, ByVal e As Instr_BaseReporting.ReportingCustomArgs)

        MessageCustom = e.Custom
        Dim exp As Action = AddressOf ExpireDisplay
        If Rhino.RhinoApp.MainApplicationWindow.InvokeRequired Then Rhino.RhinoApp.MainApplicationWindow.Invoke(exp)

    End Sub

    Private Sub ExpireDisplay()
        Me.OnDisplayExpired(False)
    End Sub

    Private Sub ExpireComponent()
        Me.ExpireSolution(True)
    End Sub

#Region "Variable parameter"

    Public Function CanInsertParameter(side As GH_ParameterSide, index As Integer) As Boolean Implements IGH_VariableParameterComponent.CanInsertParameter
        If side = GH_ParameterSide.Input Then
            If index <> 0 And index <> 1 And index <> 2 Then Return True
        End If
        Return False
    End Function

    Public Function CanRemoveParameter(side As GH_ParameterSide, index As Integer) As Boolean Implements IGH_VariableParameterComponent.CanRemoveParameter
        If side = GH_ParameterSide.Input Then
            If Me.Params.Input.Count = 4 Then
                If index <> 0 And index <> 1 And index <> 2 And index <> 3 Then Return True
            Else
                If index <> 0 And index <> 1 And index <> 2 Then Return True
            End If
        End If
        Return False
    End Function

    Public Function CreateParameter(side As GH_ParameterSide, index As Integer) As IGH_Param Implements IGH_VariableParameterComponent.CreateParameter
        Dim np As New Param_Instr()
        np.Access = GH_ParamAccess.tree
        np.Optional = True
        VariableParameterMaintenance()
        Return np
    End Function

    Public Function DestroyParameter(side As GH_ParameterSide, index As Integer) As Boolean Implements IGH_VariableParameterComponent.DestroyParameter
        VariableParameterMaintenance()
        Return True
    End Function

    Public Sub VariableParameterMaintenance() Implements IGH_VariableParameterComponent.VariableParameterMaintenance
        For i As Integer = 3 To Me.Params.Input.Count - 1 Step 1
            Dim thisp As IGH_Param = Me.Params.Input(i)
            thisp.Name = "Instruction" & i - 3
            thisp.NickName = "I" & i - 3
            thisp.Optional = True
            thisp.Description = "Instruction to execute, data tree is flattened internally."
        Next

    End Sub

#End Region

End Class
