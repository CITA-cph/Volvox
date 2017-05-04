Imports System.Drawing

Public Class Control_Display

    Public Event DynamicClicked(sender As Object, e As System.EventArgs)

    Private Sub DynamicBut_Click(sender As Object, e As EventArgs) Handles DynamicBut.Click
        RaiseEvent DynamicClicked(Me, New EventArgs())
    End Sub

    Public Event PlusClicked(sender As Object, e As System.EventArgs)

    Private Sub PlusBut_Click(sender As Object, e As EventArgs) Handles PlusBut.Click
        RaiseEvent PlusClicked(Me, New EventArgs())
    End Sub

    Public Event MinusClicked(sender As Object, e As System.EventArgs)

    Private Sub MinusBut_Click(sender As Object, e As EventArgs) Handles MinusBut.Click
        RaiseEvent MinusClicked(Me, New EventArgs())
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
