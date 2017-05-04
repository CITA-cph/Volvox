Imports System.Drawing
Imports Grasshopper.Kernel
Imports Rhino.Geometry
Imports Grasshopper.Kernel.Parameters
Imports System.IO

Public Class Eng_SaveE57

    Inherits GH_Component

    Sub New()
        MyBase.New("Save E57", "Save E57", "Save E57 file within the engine.", "Volvox", "Engine")
    End Sub

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return New Guid("788c986f-7cb2-4bb6-a7a3-855566cd6e2e")
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As GH_Exposure
        Get
            Return GH_Exposure.quinary
        End Get
    End Property

    Protected Overrides ReadOnly Property Icon As Bitmap
        Get
            Return My.Resources.Icon_SaveE57Engine
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As Grasshopper.Kernel.GH_Component.GH_InputParamManager)
        pManager.AddParameter(New Param_SaveE57)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As Grasshopper.Kernel.GH_Component.GH_OutputParamManager)
        pManager.AddParameter(New Param_Instr)
    End Sub

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)

        Dim fpath As String = String.Empty
        If Not (DA.GetData(0, fpath)) Then Return

        fpath = RemoveIllegal(fpath)
        fpath = ValidatePath(fpath)

        DA.SetData(0, New Instr_SaveE57(fpath))

    End Sub

End Class
