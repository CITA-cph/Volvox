Imports Grasshopper.Kernel
Imports Grasshopper.Kernel.Data
Imports System.Drawing
Imports GH_IO.Serialization
Imports Volvox_Instr
Imports Rhino.Geometry

Public Class Cloud_Engine
    Inherits GH_Component
    Implements IGH_VariableParameterComponent

    Public Sub New()
        MyBase.New("Cloud Engine", "CloudE", "Point Cloud manipulation engine.", "Volvox", "Engine")
        AddHandler Me.Params.ParameterSourcesChanged, AddressOf InputChanged
    End Sub

    Protected Overrides ReadOnly Property Icon As Bitmap
        Get
            Return My.Resources.Icon_Engine
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As GH_Exposure
        Get
            Return GH_Exposure.primary
        End Get
    End Property

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return GuidsRelease1.Comp_Engine
        End Get
    End Property

    Public Overrides Sub CreateAttributes()
        m_attributes = New Cloud_EngineAtt(Me)
    End Sub

    Public Overrides Function Write(writer As GH_IWriter) As Boolean
        Return MyBase.Write(writer)
    End Function

    Public Overrides Function Read(reader As GH_IReader) As Boolean
        Return MyBase.Read(reader)
    End Function

    Protected Overrides Sub RegisterInputParams(pManager As Grasshopper.Kernel.GH_Component.GH_InputParamManager)
        pManager.AddParameter(New Param_Cloud(), "Cloud", "C", "Cloud to process", GH_ParamAccess.item)
        Dim thisparam As Param_Cloud = pManager.Param(0)
        thisparam.Hidden = True
        pManager.AddParameter(New Param_Instr(), "Instruction0", "I0", "Instruction to execute, data tree is flattened internally.", GH_ParamAccess.tree)
        pManager.Param(1).Optional = True
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As Grasshopper.Kernel.GH_Component.GH_OutputParamManager)
        pManager.AddParameter(New Param_Cloud, "Cloud", "C", "Processed cloud", GH_ParamAccess.item)
        ' pManager.AddGenericParameter("Data_0", "D0", "Data output", GH_ParamAccess.list)
    End Sub

    Protected Overrides Function HtmlHelp_Source() As String
        Return MyBase.HtmlHelp_Source()
    End Function

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

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)

        Dim count As Integer = 0

        For i As Integer = 1 To Me.Params.Input.Count - 1 Step 1
            Dim tempdt As New GH_Structure(Of Instr_Base)
            If DA.GetDataTree(i, tempdt) Then
                For Each instruction As Instr_Base In tempdt.AllData(True)
                    count += 1
                Next
            End If
        Next

        Dim InputCloud As PointCloud = Nothing
        If Not DA.GetData(0, InputCloud) Then Return

        For i As Integer = 1 To Me.Params.Input.Count - 1 Step 1
            Dim tempdt As New GH_Structure(Of Instr_Base)
            If DA.GetDataTree(i, tempdt) Then
                For Each instruction As Instr_Base In tempdt.AllData(True)
                    If Not instruction.Execute(InputCloud) Then
                        Me.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Instruction failed to execute, following instructions cancelled.")
                        Exit Sub
                    End If
                Next
            End If
        Next

        'get out all the data
        DA.SetData(0, InputCloud)

    End Sub

#Region "Variable parameter"

    Public Function CanInsertParameter(side As GH_ParameterSide, index As Integer) As Boolean Implements IGH_VariableParameterComponent.CanInsertParameter
        If side = GH_ParameterSide.Input Then
            If index <> 0 Then Return True
        End If
        Return False
    End Function

    Public Function CanRemoveParameter(side As GH_ParameterSide, index As Integer) As Boolean Implements IGH_VariableParameterComponent.CanRemoveParameter
        If side = GH_ParameterSide.Input Then
            If Me.Params.Input.Count = 2 Then
                If index <> 0 And index <> 1 Then Return True
            Else
                If index <> 0 Then Return True
            End If
        End If
        Return False
    End Function

    Public Function CreateParameter(side As GH_ParameterSide, index As Integer) As IGH_Param Implements IGH_VariableParameterComponent.CreateParameter
        Dim np As New Param_Instr()
        np.Access = GH_ParamAccess.tree
        np.Optional = True
        '  Me.Params.RegisterOutputParam(New Grasshopper.Kernel.Parameters.Param_GenericObject, index)
        VariableParameterMaintenance()
        Return np
    End Function

    Public Function DestroyParameter(side As GH_ParameterSide, index As Integer) As Boolean Implements IGH_VariableParameterComponent.DestroyParameter
        ' Me.Params.UnregisterOutputParameter(Me.Params.Output(index))
        VariableParameterMaintenance()
        Return True
    End Function

    Public Sub VariableParameterMaintenance() Implements IGH_VariableParameterComponent.VariableParameterMaintenance
        For i As Integer = 1 To Me.Params.Input.Count - 1 Step 1
            Dim thisp As IGH_Param = Me.Params.Input(i)
            thisp.Name = "Instruction" & i - 1
            thisp.NickName = "I" & i - 1
            thisp.Optional = True
            thisp.Description = "Instruction to execute, data tree is flattened internally."
        Next

        'For i As Integer = 1 To Me.Params.Output.Count - 1 Step 1
        '    Dim thisp As IGH_Param = Me.Params.Output(i)
        '    thisp.Name = "Data" & i - 1
        '    thisp.NickName = "D" & i - 1
        '    thisp.Description = "Data resulting from an instruction (if there is any)."
        'Next

    End Sub

#End Region

End Class
