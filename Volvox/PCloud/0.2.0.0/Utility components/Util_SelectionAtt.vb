Imports Grasshopper.GUI
Imports Grasshopper.GUI.Canvas
Imports Grasshopper.Kernel

Public Class Util_SelectionAtt
    Inherits Grasshopper.Kernel.Attributes.GH_ComponentAttributes

    Dim myowner As Util_Selection = Nothing

    Sub New(owner As Util_Selection)
        MyBase.New(owner)
        myowner = owner
    End Sub

    Public Overrides Function RespondToMouseDoubleClick(sender As GH_Canvas, e As GH_CanvasMouseEvent) As GH_ObjectResponse
        If Me.ContentBox.Contains(e.CanvasLocation) Then
            Grasshopper.Instances.DocumentEditorFadeOut()
            myowner.RunSelection()

            Return GH_ObjectResponse.Handled
        End If

        Return GH_ObjectResponse.Ignore
    End Function

End Class
