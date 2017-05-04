Imports System.Drawing
Imports System.IO
Imports Grasshopper.Kernel
Imports Volvox_Cloud

Public Class Util_SaveFile_OBSOLETE
    Inherits GH_Component

    Sub New()

        MyBase.New("Save Cloud", "CloudSave", "Save cloud to XYZ file.", "Volvox", "Util")
    End Sub

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return GuidsRelease1.Comp_SaveFile
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As GH_Exposure
        Get
            Return GH_Exposure.hidden
        End Get
    End Property

    Protected Overrides ReadOnly Property Icon As Bitmap
        Get
            Return My.Resources.Icon_Save
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As GH_InputParamManager)
        pManager.AddParameter(New Param_Cloud, "Cloud", "C", "Cloud to deconstruct", GH_ParamAccess.item)
        pManager.AddParameter(New Grasshopper.Kernel.Parameters.Param_FilePath, "File path", "F", "File path", GH_ParamAccess.item)
        pManager.AddTextParameter("Mask", "M", "Mask to apply", GH_ParamAccess.item, "x y z")
        pManager.AddNumberParameter("Precision", "P", "Precision", GH_ParamAccess.item)
        pManager.AddBooleanParameter("Double-quotes", "D", "Surround values with double-quotes", GH_ParamAccess.item, False)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As GH_OutputParamManager)
    End Sub

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)

        Dim pc As GH_Cloud = Nothing
        Dim fpath As String = String.Empty
        Dim fmask As String = String.Empty
        Dim prec As Double = 1
        Dim dq As Boolean = False

        If Not (DA.GetData(0, pc)) Then Return
        If Not (DA.GetData(1, fpath)) Then Return
        If Not (DA.GetData(2, fmask)) Then Return
        If Not (DA.GetData(3, prec)) Then Return
        If Not (DA.GetData(4, dq)) Then Return

        Dim nparse As New Parse_Save(fmask, prec, dq)

        Using w As StreamWriter = New StreamWriter(fpath)

            For i As Integer = 0 To pc.Value.Count - 1 Step 1
                w.WriteLine(nparse.PCloudItemToText(pc.Value, i))
            Next

        End Using


    End Sub


End Class
