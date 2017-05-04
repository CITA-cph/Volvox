Imports System.Drawing
Imports System.Windows.Forms
Imports Grasshopper.Kernel
Imports Volvox

Public Class Param_SaveE57
    Inherits GH_PersistentParam(Of GH_SaveFileGoo)

    Sub New()
        MyBase.New("File Path", "F", "File Path", "Params", "Primitive")
    End Sub

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return New Guid("355c1ea7-b143-4928-8adc-370180cea7da")
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As GH_Exposure
        Get
            Return GH_Exposure.hidden
        End Get
    End Property

    Protected Overrides ReadOnly Property Icon As Bitmap
        Get
            Return My.Resources.Icon_ParamE57
        End Get
    End Property

    Protected Overrides Function Prompt_Plural(ByRef values As List(Of GH_SaveFileGoo)) As GH_GetterResult
        Return GH_GetterResult.cancel
    End Function

    Protected Overrides Function Prompt_Singular(ByRef value As GH_SaveFileGoo) As GH_GetterResult
        Dim sf As New SaveFileDialog
        sf.Filter = "E57 Files (*.e57)|*.e57"
        Dim res As DialogResult = sf.ShowDialog(Grasshopper.Instances.ActiveCanvas)
        If res = DialogResult.OK Then
            value = New GH_SaveFileGoo(sf.FileName)
            Return GH_GetterResult.success
        End If
        Return GH_GetterResult.cancel
    End Function

End Class
