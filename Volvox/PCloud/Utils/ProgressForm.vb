Imports System.ComponentModel
Imports System.Windows.Forms

Public Class ProgressForm

    Friend currentPercent As Integer
    Friend currentMajor As String

    Private Sub ProgressForm_Close(ByVal sender As Object, ByVal e As FormClosingEventArgs) Handles Me.FormClosing
        e.Cancel = True
        Me.Hide()
    End Sub

    Friend Sub Report(Percent As Integer)
        currentPercent = Percent
        UpdateText()
    End Sub

    Friend Sub ChangeText(Major As String)
        currentMajor = Major
        UpdateText()
    End Sub

    Private Sub UpdateText()
        If currentPercent = -1 Then
            Me.Text = currentMajor
        Else
            Me.Text = currentMajor & " " & currentPercent & "%"
        End If

        ProgSimple.Percent = currentPercent

        Me.Refresh()
    End Sub

    Private Sub ProgSimple_Load(sender As Object, e As EventArgs) Handles ProgSimple.Load

    End Sub

    Private Sub ProgressForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub
End Class