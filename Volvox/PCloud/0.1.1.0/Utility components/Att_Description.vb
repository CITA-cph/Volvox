Imports System.Drawing
Imports Grasshopper.GUI
Imports Grasshopper.GUI.Canvas
Imports Grasshopper.Kernel

Public Class Att_Description
    Inherits Grasshopper.Kernel.Attributes.GH_ComponentAttributes

    Dim myrect As New Size(300, 295)
    Dim myfont As New Font(New FontFamily("Arial"), 8)
    Dim str As String = "Volvox - Point Cloud editing plugin."

    Sub New(owner As Util_Description)
        MyBase.New(owner)

        str += vbCrLf
        str += "The plugin is developed in the frame of the DURAARK project: http://duraark.eu/"
        str += vbCrLf
        str += vbCrLf
        str += "DURAARK (Durable Architectural Knowledge) is a collaborative project "
        str += "developing methods and tools for the semantic enrichment and long-term "
        str += "preservation of architectural knowledge and data. It focuses on establishing working "
        str += "practices and links between semantically rich BIM models and unstructured Point Cloud data. It is funded through the "
        str += "European Commision's FP7 Programme and is running between 02/2013 - 01/2016."
        str += " (Grant Agreement no: 600908)"
        str += vbCrLf
        str += vbCrLf
        str += "Double click this window to open the DURAARK website."
    End Sub

    Protected Overrides Sub Render(canvas As GH_Canvas, graphics As Graphics, channel As GH_CanvasChannel)
        'MyBase.Render(canvas, graphics, channel)

        Select Case channel
            Case channel = GH_CanvasChannel.Objects

                Dim rect As New Rectangle(Me.Bounds.X, Me.Bounds.Y, myrect.Width, myrect.Height)
                Dim backrect As New Rectangle(Me.Bounds.X + 5, Me.Bounds.Y + 5, myrect.Width, myrect.Height)
                Dim layrect As New RectangleF(Me.Bounds.X + 5, Me.Bounds.Y + 100, myrect.Width - 10, myrect.Height - 80)

                Using nb As Brush = New SolidBrush(Color.FromArgb(100, 0, 0, 0))
                    graphics.FillRectangle(nb, backrect)
                End Using

                graphics.FillRectangle(Brushes.White, rect)

                If canvas.Viewport.Zoom > 0.5 Then
                    graphics.DrawImage(My.Resources.DURAARK_logo2, Me.Bounds.X, Me.Bounds.Y + 10, CInt(myrect.Width), CInt(myrect.Width * (130 / 580)))
                    graphics.DrawString(str, myfont, Brushes.Black, layrect)
                Else
                    graphics.FillRectangle(Brushes.LightGray, Me.Bounds.X + 10, Me.Bounds.Y + 10, CInt(myrect.Width) - 20, CInt(myrect.Width * (130 / 580)))
                End If

                If Me.Selected Then
                    graphics.DrawRectangle(Pens.DarkGray, rect)
                Else
                    graphics.DrawRectangle(Pens.Black, rect)
                End If

            Case GH_CanvasChannel.Last

        End Select

    End Sub

    Public Overrides Property Bounds As RectangleF
        Get
            Return New RectangleF(Me.Pivot.X, Me.Pivot.Y, myrect.Width, myrect.Height)
        End Get
        Set(value As RectangleF)
            MyBase.Bounds = value
        End Set
    End Property

    Public Overrides Function RespondToMouseDoubleClick(sender As GH_Canvas, e As GH_CanvasMouseEvent) As GH_ObjectResponse
        If (Me.Bounds.Contains(e.CanvasLocation)) Then
            Dim sc As Util_Description = DirectCast(Owner, Util_Description)

            Dim webAddress As String = "http://duraark.eu/"
            Process.Start(webAddress)

            Return Grasshopper.GUI.Canvas.GH_ObjectResponse.Handled
        End If
        Return Grasshopper.GUI.Canvas.GH_ObjectResponse.Ignore
    End Function


End Class
