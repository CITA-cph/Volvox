Imports System.Drawing
Imports System.IO
Imports Grasshopper.Kernel

Public Class Util_XYZPreview
    Inherits GH_Component

    Sub New()
        MyBase.New("Preview .xyz", "PreviewXyz", "Displays the first 100 lines of a text file.", "Volvox", "I/O")
    End Sub

    Public Overrides ReadOnly Property Exposure As GH_Exposure
        Get
            Return GH_Exposure.primary
        End Get
    End Property

    Protected Overrides ReadOnly Property Icon As Bitmap
        Get
            Return My.Resources.Icon_Preview100
        End Get
    End Property


    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return GuidsRelease2.Comp_xyzHint
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As GH_InputParamManager)
        pManager.AddParameter(New Grasshopper.Kernel.Parameters.Param_FilePath, "File path", "F", "File path", GH_ParamAccess.item)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As GH_OutputParamManager)
        pManager.AddTextParameter("Print", "P", "First 100 lines of the file", GH_ParamAccess.list)
    End Sub

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)
        Dim fpath As String = String.Empty
        If Not DA.GetData(0, fpath) Then Return

        Dim nl As New List(Of String)

        Using str As New StreamReader(fpath)
            For i As Integer = 0 To 99 Step 1
                If str.EndOfStream Then Exit For
                nl.Add(str.ReadLine())
            Next
        End Using

        DA.SetDataList(0, nl)
    End Sub
End Class
