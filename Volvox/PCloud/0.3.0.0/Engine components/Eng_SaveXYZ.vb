Imports System.Drawing
Imports Grasshopper.Kernel
Imports Rhino.Geometry
Imports Grasshopper.Kernel.Parameters

Public Class Eng_SaveXYZ

    Inherits GH_Component

    Sub New()
        MyBase.New("Save", "Save", "Save XYZ file within the engine.", "Volvox", "Engine")
    End Sub

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return New Guid("dbf10507-deb1-4403-abaa-746da34c4348")
        End Get
    End Property

    Protected Overrides ReadOnly Property Icon As Bitmap
        Get
            Return My.Resources.Icon_SaveXYZEngine
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As GH_Exposure
        Get
            Return GH_Exposure.senary
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As Grasshopper.Kernel.GH_Component.GH_InputParamManager)
        pManager.AddParameter(New Param_SaveFile)
        pManager.AddTextParameter("Mask", "M", "Mask to apply", GH_ParamAccess.item, "x y z")
        pManager.AddNumberParameter("Precision", "P", "Precision", GH_ParamAccess.item)
        pManager.AddBooleanParameter("Double-quotes", "D", "Surround values with double-quotes", GH_ParamAccess.item, False)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As Grasshopper.Kernel.GH_Component.GH_OutputParamManager)
        pManager.AddParameter(New Param_Instr)
    End Sub

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)

        Dim fpath As String = String.Empty
        Dim fmask As String = String.Empty
        Dim prec As Double = 1
        Dim dq As Boolean = False
        If Not (DA.GetData(0, fpath)) Then Return
        If Not (DA.GetData(1, fmask)) Then Return
        If Not (DA.GetData(2, prec)) Then Return
        If Not (DA.GetData(3, dq)) Then Return

        fpath = RemoveIllegal(fpath)

        DA.SetData(0, New Instr_SaveXYZ(fpath, prec, dq, fmask))

    End Sub

End Class
