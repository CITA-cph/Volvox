'Imports System.IO
'Imports Grasshopper.Kernel
'Imports Grasshopper.Kernel.Data
'Imports Grasshopper.Kernel.Types
'Imports Rhino.Geometry

'Public Class File_Engine
'    Inherits GH_Component
'    Implements IGH_VariableParameterComponent

'    Public Sub New()
'        MyBase.New("File Engine", "FileE", "Point Cloud manipulation engine operating directly on file", "Cloud", "Engine")
'    End Sub

'    'Protected Overrides ReadOnly Property Icon As Bitmap
'    '    Get
'    '        Return My.Resources.EngineIcon
'    '    End Get
'    'End Property

'    Public Overrides ReadOnly Property ComponentGuid As Guid
'        Get
'            Return GuidsRelease1.Comp_FileEngine
'        End Get
'    End Property

'    'Public Overrides Sub CreateAttributes()
'    '    m_attributes = New Cloud_EngineAtt(Me)
'    'End Sub

'    Protected Overrides Sub RegisterInputParams(pManager As Grasshopper.Kernel.GH_Component.GH_InputParamManager)
'        pManager.AddParameter(New Param_Setup())

'        pManager.AddParameter(New Param_Instr(), "Instruction0", "I0", "Instruction to execute, data tree is flattened internally.", GH_ParamAccess.tree)
'        pManager.Param(1).Optional = True

'    End Sub

'    Protected Overrides Sub RegisterOutputParams(pManager As Grasshopper.Kernel.GH_Component.GH_OutputParamManager)
'        pManager.AddParameter(New Param_Cloud, "Cloud", "C", "Processed cloud", GH_ParamAccess.list)
'        pManager.AddGenericParameter("Data_0", "D0", "Data output", GH_ParamAccess.list)
'    End Sub

'    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)

'        Dim setup As File_Setup = Nothing
'        If Not DA.GetData(0, setup) Then Return

'        Dim nl As New List(Of Integer)
'        nl.Clear()

'        Dim count As Integer = 0

'        For i As Integer = 1 To Me.Params.Input.Count - 1 Step 1
'            Dim tempdt As New GH_Structure(Of Instr_Base)
'            If DA.GetDataTree(i, tempdt) Then
'                For Each instruction As Instr_Base In tempdt.AllData(True)
'                    count += 1
'                Next
'            End If
'        Next

'        Dim TempFile As String = setup.SaveTo & "_temp"

'        File.Delete(setup.SaveTo)
'        File.Delete(TempFile)
'        File.Copy(setup.LoadFile, setup.SaveTo)
'        File.Copy(setup.LoadFile, TempFile)

'        Dim ppp As New PointCloud()
'        Dim ContinueReading As Boolean = True
'        Dim counter As Integer = 0

'        For i As Integer = 1 To Me.Params.Input.Count - 1 Step 1
'            Dim tempdt As New GH_Structure(Of Instr_Base)
'            If DA.GetDataTree(i, tempdt) Then
'                For Each instruction As Instr_Base In tempdt.AllData(True)
'                    Using SourceFile As StreamReader = New StreamReader(TempFile)
'                        Using TargetFile As StreamWriter = New StreamWriter(setup.SaveTo)
'                            While ContinueReading

'                                'iterate over file 
'                                Dim tempPCloud As New PointCloud
'                                ContinueReading = GetPointsFromFile(setup.ChunckSize, setup.Mask, setup.Separate, SourceFile, tempPCloud)
'                                counter += tempPCloud.Count
'                                ppp.Merge(tempPCloud.Duplicate)
'                                tempPCloud.Dispose()

'                            End While
'                        End Using
'                    End Using
'                Next
'            End If
'        Next

'        DA.SetData(0, New GH_Cloud(ppp))
'        DA.SetData(1, New GH_Integer(counter))

'    End Sub

'#Region "Variable parameter"


'    Function GetPointsFromFile(Amount As Integer, Mask As String, Separator As String, ByRef Reader As StreamReader, ByRef PCloud As Rhino.Geometry.PointCloud) As Boolean

'        PCloud.Dispose()
'        PCloud = New PointCloud()

'        For i As Integer = 0 To Amount - 1 Step 1
'            Dim str As String = Reader.ReadLine()
'            If str = Nothing Then
'                Return False
'                Exit For
'            Else
'                Dim m As Multipoint = TextToMultipoint(str, Mask, Separator)
'                PCloud.AppendNew()
'                PCloud(PCloud.Count - 1).Location = New Point3d(m.X, m.Y, m.Z)
'                If m.ContainsColors Then PCloud(PCloud.Count - 1).Color = Drawing.Color.FromArgb(m.R, m.G, m.B)
'                If m.ContainsNormals Then PCloud(PCloud.Count - 1).Normal = New Vector3d(m.U, m.V, m.W)
'                If m.ContainsIntensity Then
'                    If m.ContainsColors Then
'                        Dim multiplier As Double = m.A / 255
'                        PCloud(PCloud.Count - 1).Color = Drawing.Color.FromArgb(m.R * multiplier, m.G * multiplier, m.B * multiplier)
'                    Else
'                        PCloud(PCloud.Count - 1).Color = Drawing.Color.FromArgb(m.A, m.A, m.A)
'                    End If
'                End If
'            End If
'        Next

'        Return True
'    End Function


'    Public Function CanInsertParameter(side As GH_ParameterSide, index As Integer) As Boolean Implements IGH_VariableParameterComponent.CanInsertParameter
'        If side = GH_ParameterSide.Input Then
'            If index <> 0 Then Return True
'        End If
'        Return False
'    End Function

'    Public Function CanRemoveParameter(side As GH_ParameterSide, index As Integer) As Boolean Implements IGH_VariableParameterComponent.CanRemoveParameter
'        If side = GH_ParameterSide.Input Then
'            If Me.Params.Input.Count = 2 Then
'                If index <> 0 And index <> 1 Then Return True
'            Else
'                If index <> 0 Then Return True
'            End If
'        End If
'        Return False
'    End Function

'    Public Function CreateParameter(side As GH_ParameterSide, index As Integer) As IGH_Param Implements IGH_VariableParameterComponent.CreateParameter
'        Dim np As New Param_Instr()
'        np.Access = GH_ParamAccess.tree
'        np.Optional = True
'        Me.Params.RegisterOutputParam(New Grasshopper.Kernel.Parameters.Param_GenericObject, index)
'        VariableParameterMaintenance()
'        Return np
'    End Function

'    Public Function DestroyParameter(side As GH_ParameterSide, index As Integer) As Boolean Implements IGH_VariableParameterComponent.DestroyParameter
'        Me.Params.UnregisterOutputParameter(Me.Params.Output(index))
'        VariableParameterMaintenance()
'        Return True
'    End Function

'    Public Sub VariableParameterMaintenance() Implements IGH_VariableParameterComponent.VariableParameterMaintenance
'        For i As Integer = 1 To Me.Params.Input.Count - 1 Step 1
'            Dim thisp As IGH_Param = Me.Params.Input(i)
'            thisp.Name = "Instruction" & i - 1
'            thisp.NickName = "I" & i - 1
'            thisp.Description = "Instruction to execute, data tree is flattened internally."
'        Next

'        For i As Integer = 1 To Me.Params.Output.Count - 1 Step 1
'            Dim thisp As IGH_Param = Me.Params.Output(i)
'            thisp.Name = "Data" & i - 1
'            thisp.NickName = "D" & i - 1
'            thisp.Description = "Data resulting from an instruction (if there is any)."
'        Next

'        ' Me.ExpireSolution(True)
'    End Sub

'#End Region

'End Class
