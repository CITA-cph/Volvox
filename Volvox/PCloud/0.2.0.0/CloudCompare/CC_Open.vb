Imports System.Drawing
Imports Grasshopper.Kernel
Imports Rhino.Geometry

Public Class CC_Open
    Inherits GH_Component


    Public Sub New()
        MyBase.New("Open File", "OpenCC", "Open file in CloudCompare." & vbCrLf & "http://cloudcompare.org/", "Volvox", "CloudCompare")
    End Sub

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return GuidsRelease2.CC_Open
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As GH_Exposure
        Get
            Return GH_Exposure.primary
        End Get
    End Property

    Protected Overrides ReadOnly Property Icon As Bitmap
        Get
            Return My.Resources.Icon_OpenCC
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As Grasshopper.Kernel.GH_Component.GH_InputParamManager)
        pManager.AddParameter(New Grasshopper.Kernel.Parameters.Param_FilePath, "File path", "F", "File path can't containt any spaces, because of the CloudCompare bug.", GH_ParamAccess.item)
        pManager.AddBooleanParameter("Run", "R", "Run script", GH_ParamAccess.item, False)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As Grasshopper.Kernel.GH_Component.GH_OutputParamManager)

    End Sub



    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)

        Dim filepath As New String("")
        Dim run As Boolean

        If Not DA.GetData(0, filepath) Then Return
        If Not DA.GetData(1, run) Then Return


        If run Then
            Dim process As System.Diagnostics.Process
            Dim p As New System.Diagnostics.ProcessStartInfo("C:\Program Files\CloudCompare\CloudCompare.exe", filepath)
            p.RedirectStandardError = False
            p.RedirectStandardOutput = False
            p.CreateNoWindow = False
            p.UseShellExecute = False

            process = System.Diagnostics.Process.Start(p)
        End If

    End Sub

End Class
