Imports System
Imports System.Drawing
Imports Grasshopper.Kernel

Public Class Param_Instr
    Inherits GH_Param(Of Instr_Base)

    Public Sub New()
        MyBase.New(New GH_InstanceDescription("Instr", "I", "Cloud instruction", "Params", "Geometry"))
    End Sub

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return New Guid("{CE3C9BB4-E0EB-4A8A-A25B-8D78E4318CAD}")
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As GH_Exposure
        Get
            Return GH_Exposure.hidden
        End Get
    End Property

    Protected Overrides ReadOnly Property Icon As Bitmap
        Get
            Return My.Resources.Icon_Instruction
        End Get
    End Property

End Class