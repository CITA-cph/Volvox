Imports System.Drawing

Public Class Control_Display

    Public Event DynamicClicked()

    Private Sub DynamicBut_Click(sender As Object, e As EventArgs) Handles DynamicBut.Click
        RaiseEvent DynamicClicked()
    End Sub

    Public Event PlusClicked()

    Private Sub PlusBut_Click(sender As Object, e As EventArgs) Handles PlusBut.Click
        RaiseEvent PlusClicked()
    End Sub

    Public Event MinusClicked()

    Private Sub MinusBut_Click(sender As Object, e As EventArgs) Handles MinusBut.Click
        RaiseEvent MinusClicked()
    End Sub

    Friend Property DispButBack As Color
        Get
            Return DynamicBut.BackColor
        End Get
        Set(value As Color)
            DynamicBut.BackColor = value
        End Set
    End Property

    Friend Property DispButFrame As Color
        Get
            Return DynamicBut.FlatAppearance.BorderColor
        End Get
        Set(value As Color)
            DynamicBut.FlatAppearance.BorderColor = value
        End Set
    End Property

End Class
