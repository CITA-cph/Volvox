Imports Volvox_Instr
Imports Volvox_Cloud
Imports Rhino.Geometry

Public Class Instr_Transform
    Inherits Instr_Base

    Private Property InstrMatrix As Rhino.Geometry.Transform

    Sub New()

    End Sub

    Sub New(xform As Rhino.Geometry.Transform)
        InstrMatrix = xform
    End Sub

    Public Overrides ReadOnly Property InstructionGUID As Guid
        Get
            Return GuidsRelease1.Instr_Transform
        End Get
    End Property

    Public Overrides ReadOnly Property InstructionType As String
        Get
            Return "Transform " & InstrMatrix.ToString
        End Get
    End Property

    Public Overrides Function Execute(ByRef PointCloud As PointCloud) As Boolean

        PointCloud.Transform(InstrMatrix)

        Return True
    End Function

End Class
