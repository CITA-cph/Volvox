Imports System.Windows.Forms
Imports GH_IO.Serialization
Imports Grasshopper.Kernel
Imports Grasshopper.Kernel.Parameters


Public Class GUIDMatch
    Inherits GH_Component
    Implements IGH_VariableParameterComponent

    Sub New()
        MyBase.New("GUIDMatch", "GUIDMatch", "Match Guids in multiple ways", "Volvox", "Diff")
        Me.Message = "Unique"
    End Sub

    Protected Overrides Sub AppendAdditionalComponentMenuItems(menu As ToolStripDropDown)
        MyBase.AppendAdditionalComponentMenuItems(menu)

        Dim UniqueMode As Boolean = False
        Dim CommonMode As Boolean = False
        Dim MissingMode As Boolean = False
        Dim CombineMode As Boolean = False

        Select Case CurrentMode
            Case WorkMode.Unique
                UniqueMode = True
            Case WorkMode.Common
                CommonMode = True
            Case WorkMode.Missing
                MissingMode = True
            Case WorkMode.Combine
                CombineMode = True
        End Select

        Dim UniqueItem As ToolStripMenuItem = Menu_AppendItem(menu, "Unique", AddressOf UniqueItemClick, True, UniqueMode)
        UniqueItem.ToolTipText = "Get GUIDs unique for each list."

        Dim CommonItem As ToolStripMenuItem = Menu_AppendItem(menu, "Common", AddressOf CommonItemClick, True, CommonMode)
        UniqueItem.ToolTipText = "Get GUIDs common for all lists."

        Dim MissingItem As ToolStripMenuItem = Menu_AppendItem(menu, "Missing", AddressOf MissingItemClick, True, MissingMode)
        UniqueItem.ToolTipText = "Get GUIDs missing from each list."

        Dim AllItem As ToolStripMenuItem = Menu_AppendItem(menu, "Combine", AddressOf CombineItemClick, True, CombineMode)
        UniqueItem.ToolTipText = "Get combined GUIDs from all lists."

    End Sub

    Private CurrentMode As WorkMode = WorkMode.Unique

    Private Enum WorkMode As Integer
        Unique = 0
        Common = 1
        Missing = 2
        Combine = 3
    End Enum

    Private Sub CombineItemClick()
        Me.RecordUndoEvent("Combine Mode Enabled")
        CurrentMode = WorkMode.Combine
        Me.Message = "Combine"
        Me.VariableParameterMaintenance()
        Me.ExpireSolution(True)
    End Sub

    Private Sub UniqueItemClick()
        Me.RecordUndoEvent("Unique Mode Enabled")
        CurrentMode = WorkMode.Unique
        Me.Message = "Unique"
        Me.VariableParameterMaintenance()
        Me.ExpireSolution(True)
    End Sub

    Private Sub CommonItemClick()
        Me.RecordUndoEvent("Common Mode Enabled")
        CurrentMode = WorkMode.Common
        Me.Message = "Common"
        Me.VariableParameterMaintenance()
        Me.ExpireSolution(True)
    End Sub

    Private Sub MissingItemClick()
        Me.RecordUndoEvent("Missing Mode Enabled")
        CurrentMode = WorkMode.Missing
        Me.Message = "Missing"
        Me.VariableParameterMaintenance()
        Me.ExpireSolution(True)
    End Sub

    Public Overrides Function Read(reader As GH_IReader) As Boolean
        CurrentMode = reader.GetInt32("WorkMode")
        Me.Message = reader.GetString("Message")
        'Me.Params.OnParametersChanged()
        'Me.OnDisplayExpired(True)
        Return MyBase.Read(reader)
    End Function

    Public Overrides Function Write(writer As GH_IWriter) As Boolean
        writer.SetInt32("WorkMode", CurrentMode)
        writer.SetString("Message", Me.Message)

        Return MyBase.Write(writer)
    End Function

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return New Guid("4fb1b6f7-f25b-4424-b00b-20a21732ded1")
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As GH_InputParamManager)
        pManager.AddTextParameter("List_0", "L0", "List of GUIDs", GH_ParamAccess.list)
        pManager.AddTextParameter("List_1", "L1", "List of GUIDs", GH_ParamAccess.list)
        Me.Params.Input(0).MutableNickName = False
        Me.Params.Input(1).MutableNickName = False
        Me.Params.Input(0).Optional = True
        Me.Params.Input(1).Optional = True
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As GH_OutputParamManager)
        pManager.AddTextParameter("List_0", "L0", "List of GUIDs", GH_ParamAccess.list)
        pManager.AddTextParameter("List_1", "L1", "List of GUIDs", GH_ParamAccess.list)
        Me.Params.Output(0).MutableNickName = False
        Me.Params.Output(1).MutableNickName = False
        Me.Params.Output(0).Optional = True
        Me.Params.Output(1).Optional = True
    End Sub

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)

        Dim all As New SortedList(Of String, List(Of Integer))

        Dim inputs As New List(Of List(Of String))

        For i As Integer = 0 To Me.Params.Input.Count - 1 Step 1
            Dim data As New List(Of String)
            If DA.GetDataList(i, data) Then
                inputs.Add(data)
            End If
        Next

        For i As Integer = 0 To inputs.Count - 1 Step 1
            Dim thisl As List(Of String) = inputs(i)
            For j As Integer = 0 To thisl.Count - 1 Step 1
                If Not all.ContainsKey(thisl(j)) Then
                    Dim nl As New List(Of Integer)
                    nl.Add(i)
                    all.Add(thisl(j), nl)
                Else
                    all(thisl(j)).Add(i)
                End If
            Next
        Next

        Select Case CurrentMode
            Case WorkMode.Common
                Dim com As New List(Of String)
                For i As Integer = 0 To all.Keys.Count - 1 Step 1
                    If all(all.Keys(i)).Count = Me.Params.Input.Count Then
                        com.Add(all.Keys(i))
                    End If
                Next
                DA.SetDataList(0, com)
            Case WorkMode.Unique
                Dim outputs As New List(Of List(Of String))
                For i As Integer = 0 To Me.Params.Output.Count - 1 Step 1
                    outputs.Add(New List(Of String))
                Next

                For i As Integer = 0 To all.Keys.Count - 1 Step 1
                    Dim thisl As List(Of Integer) = all(all.Keys(i))
                    If thisl.Count = 1 Then
                        outputs(thisl(0)).Add(all.Keys(i))
                    End If
                Next

                For i As Integer = 0 To Me.Params.Output.Count - 1 Step 1
                    DA.SetDataList(i, outputs(i))
                Next
            Case WorkMode.Missing
                Dim outputs As New List(Of List(Of String))
                For i As Integer = 0 To Me.Params.Output.Count - 1 Step 1
                    outputs.Add(New List(Of String))
                Next

                For i As Integer = 0 To all.Keys.Count - 1 Step 1
                    Dim thisl As List(Of Integer) = all(all.Keys(i))
                    If thisl.Count <> Me.Params.Output.Count Then
                        For j As Integer = 0 To Me.Params.Output.Count - 1 Step 1
                            If Not thisl.Contains(j) Then outputs(j).Add(all.Keys(i))
                        Next
                    End If
                Next

                For i As Integer = 0 To Me.Params.Output.Count - 1 Step 1
                    DA.SetDataList(i, outputs(i))
                Next
            Case WorkMode.Combine
                DA.SetDataList(0, all.Keys)
        End Select

    End Sub

    Public Sub VariableParameterMaintenance() Implements IGH_VariableParameterComponent.VariableParameterMaintenance

        Dim changed As Boolean = False

        Select Case CurrentMode
            Case WorkMode.Unique, WorkMode.Missing

                Do
                    If Me.Params.Input.Count = Me.Params.Output.Count Then Exit Do
                    If Me.Params.Input.Count < Me.Params.Output.Count Then
                        Dim n As New Param_String()
                        n.Access = GH_ParamAccess.list
                        n.Optional = True
                        n.Name = "List_0"
                        n.NickName = "L0"
                        n.MutableNickName = False
                        Me.Params.RegisterInputParam(n)
                        changed = True
                    Else
                        Dim n As New Param_String()
                        n.Access = GH_ParamAccess.list
                        n.Optional = True
                        n.Name = "List_0"
                        n.NickName = "L0"
                        n.MutableNickName = False
                        Me.Params.RegisterOutputParam(n)
                        changed = True
                    End If
                Loop

                For i As Integer = 0 To Me.Params.Input.Count - 1 Step 1
                    Dim thisin As IGH_Param = Me.Params.Input(i)
                    thisin.Name = "List_" & i
                    thisin.NickName = "L" & i
                Next

                For i As Integer = 0 To Me.Params.Output.Count - 1 Step 1
                    Dim thisin As IGH_Param = Me.Params.Output(i)
                    thisin.Name = "List_" & i
                    thisin.NickName = "L" & i
                Next

            Case WorkMode.Common, WorkMode.Combine

                For i As Integer = Me.Params.Output.Count - 1 To 1 Step -1
                    Me.Params.UnregisterOutputParameter(Me.Params.Output(i), True)
                    changed = True
                Next

                For i As Integer = 0 To Me.Params.Input.Count - 1 Step 1
                    Dim thisin As IGH_Param = Me.Params.Input(i)
                    thisin.Name = "List_" & i
                    thisin.NickName = "L" & i
                Next

                Dim meout As IGH_Param = Me.Params.Output(0)
                meout.Name = "Common"
                meout.NickName = "C"

        End Select

        If changed Then Me.Params.OnParametersChanged()

    End Sub

    Public Function CanInsertParameter(side As GH_ParameterSide, index As Integer) As Boolean Implements IGH_VariableParameterComponent.CanInsertParameter
        If side = GH_ParameterSide.Input Then
            Return True
        End If
        Return False
    End Function

    Public Function CanRemoveParameter(side As GH_ParameterSide, index As Integer) As Boolean Implements IGH_VariableParameterComponent.CanRemoveParameter
        If side = GH_ParameterSide.Input And Params.Input.Count > 2 Then Return True
        Return False
    End Function

    Public Function CreateParameter(side As GH_ParameterSide, index As Integer) As IGH_Param Implements IGH_VariableParameterComponent.CreateParameter
        Dim n As New Param_String()
        n.Access = GH_ParamAccess.list
        n.Optional = True
        n.Name = "List_0"
        n.NickName = "L0"
        n.MutableNickName = False

        Select Case CurrentMode
            Case WorkMode.Unique, WorkMode.Missing
                Dim o As New Param_String()
                o.Access = GH_ParamAccess.list
                o.Optional = True
                o.Name = "List_0"
                o.NickName = "L0"
                o.MutableNickName = False
                Me.Params.RegisterOutputParam(o, index)
        End Select

        Return n
    End Function

    Public Function DestroyParameter(side As GH_ParameterSide, index As Integer) As Boolean Implements IGH_VariableParameterComponent.DestroyParameter

        Select Case CurrentMode
            Case WorkMode.Unique, WorkMode.Missing
                Me.Params.UnregisterOutputParameter(Me.Params.Output(index), True)
        End Select

        Return True
    End Function

End Class
