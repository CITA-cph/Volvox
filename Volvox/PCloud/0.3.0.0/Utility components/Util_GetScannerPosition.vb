Imports System.Drawing
Imports Grasshopper.Kernel
Imports Volvox_Cloud

Public Class Util_GetScannerPosition
    Inherits GH_Component

    Sub New()
        MyBase.New("Get Position", "GetPose", "Get the scanner position (meaningful only if the position was inherited somehow)", "Volvox", "Cloud")
    End Sub

    Protected Overrides ReadOnly Property Icon As Bitmap
        Get
            Return My.Resources.Icon_GetPosition
        End Get
    End Property


    Public Overrides ReadOnly Property Exposure As GH_Exposure
        Get
            Return GH_Exposure.quarternary
        End Get
    End Property

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return New Guid("6bba3db1-fe45-4c03-9cee-93badafdc40b")
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As GH_InputParamManager)
        pManager.AddParameter(New Param_Cloud(), "Cloud", "C", "Cloud to get the position from", GH_ParamAccess.item)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As GH_OutputParamManager)
        pManager.AddPlaneParameter("Position", "P", "Scanner position", GH_ParamAccess.item)
    End Sub

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)
        Dim pc As GH_Cloud = Nothing
        If Not (DA.GetData(0, pc)) Then Return
        DA.SetData(0, pc.ScannerPosition)
    End Sub
End Class
