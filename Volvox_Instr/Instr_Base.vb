Imports Rhino.Geometry

Public Class Instr_Base

    Inherits Grasshopper.Kernel.Types.GH_Goo(Of Instr_Base)

    Sub New()

    End Sub

    ''' <summary>
    ''' GUID to differentiate between instructions. Please store the GUIDs in the GUID_Space.vb file.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overridable ReadOnly Property InstructionGUID As Guid
        Get
            Return Guid.Empty
        End Get
    End Property

    ''' <summary>
    ''' Displayed name of the instruction.
    ''' Instruction type is displayed to end user, please stay calm with this one.
    ''' Also note - provide any name for the instruction, otherwise the instruction will be invalid.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overridable ReadOnly Property InstructionType As String
        Get
            Return ""
        End Get
    End Property

    ''' <summary>
    ''' This is what the instruction actually does.
    ''' </summary>
    ''' <param name="PointCloud">Please note point cloud is passed with ByRef.</param>
    ''' <remarks>Return true if execution is successful</remarks>
    Public Overridable Function Execute(ByRef PointCloud As PointCloud) As Boolean
        Return True
    End Function

    Public Overrides Function Duplicate() As Grasshopper.Kernel.Types.IGH_Goo
        Return Me.MemberwiseClone()
    End Function


    Public Overrides ReadOnly Property IsValid As Boolean
        Get
            If InstructionType = String.Empty Then Return False
            Return True
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return InstructionType()
    End Function

    Public Overrides ReadOnly Property TypeDescription As String
        Get
            Return InstructionType()
        End Get
    End Property

    Public Overrides ReadOnly Property TypeName As String
        Get
            Return InstructionType()
        End Get
    End Property

End Class
