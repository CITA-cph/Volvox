Imports System.Drawing
Imports System.Windows.Forms

Public Class ProgressSimple

    Private perc As Integer

    Public Property Percent As Integer
        Get
            Return perc
        End Get
        Set(value As Integer)
            perc = value
        End Set
    End Property

    Private Sub Panel1_Paint(sender As Object, e As PaintEventArgs) Handles Panel1.Paint
        Dim r As Rectangle = Panel1.Bounds

        Dim nrect As Rectangle = r
        nrect.Width = 1 + (perc / 100) * (r.Width)

        Using g As Graphics = e.Graphics
            g.FillRectangle(Brushes.LightGray, nrect)
        End Using
    End Sub

End Class
