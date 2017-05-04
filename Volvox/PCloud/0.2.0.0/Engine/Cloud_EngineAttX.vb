
Imports System.Drawing
Imports System.Drawing.Text
Imports Grasshopper.GUI
Imports Grasshopper.GUI.Canvas

Public Class Cloud_EngineAttx
    Inherits Grasshopper.Kernel.Attributes.GH_ComponentAttributes

    Public Sub New(ByVal owner As Cloud_Enginex)
        MyBase.New(owner)

        myowner = owner
        myformat.Alignment = StringAlignment.Center
        myformat.LineAlignment = StringAlignment.Near
        myformat.Trimming = StringTrimming.EllipsisWord

    End Sub

    Dim myformat As New StringFormat
    Dim myrect As New Rectangle(0, 0, 10, 50)
    Dim myfont As New Font(Grasshopper.Kernel.GH_FontServer.FamilyStandard, 6, FontStyle.Regular)
    Dim myowner As Cloud_Enginex = Nothing

    Public Overrides Function RespondToMouseDoubleClick(sender As GH_Canvas, e As GH_CanvasMouseEvent) As GH_ObjectResponse
        If Me.ContentBox.Contains(e.CanvasLocation) Then
            myowner.ChangeShowInfo()
            Return GH_ObjectResponse.Handled
        End If

        Return GH_ObjectResponse.Ignore
    End Function

    Protected Overrides Sub Render(canvas As GH_Canvas, graphics As Graphics, channel As GH_CanvasChannel)

        Select Case channel
            Case GH_CanvasChannel.Objects

                If Not myowner.ShowInfo Then Exit Select

                Dim str As String = Nothing

                If myowner.MessagePercent = -1 OrElse myowner.MessageTitle = "..." OrElse myowner.MessageTitle = "Aborted" Then
                    str = (myowner.MessageTitle)
                Else
                    str = (myowner.MessageTitle & vbCrLf & vbCrLf & myowner.MessagePercent & "%")
                End If

                If myowner.MessageCustom <> String.Empty And myowner.MessageTitle = String.Empty Then
                    str = myowner.MessageCustom
                ElseIf myowner.MessageCustom <> String.Empty
                    str += vbCrLf & myowner.MessageCustom
                End If


                Dim rect As New Rectangle(Me.Bounds.X, Me.Bounds.Y, Me.Bounds.Width, graphics.MeasureString(str, myfont, Me.Bounds.Width - 4).Height + 4 + 10)

                rect.Location = New Point(rect.Location.X, rect.Location.Y - (rect.Height - 10))

                Dim layrect As New RectangleF(rect.Location.X + 2, rect.Location.Y + 2, rect.Width - 4, rect.Height - 4)

                Dim ghp As GH_Palette = GH_Palette.Transparent

                'If Owner.RuntimeMessages(Grasshopper.Kernel.GH_RuntimeMessageLevel.Error).Count > 0 Then
                '    ghp = GH_Palette.Error
                'ElseIf Owner.RuntimeMessages(Grasshopper.Kernel.GH_RuntimeMessageLevel.Warning).Count > 0 Then
                '    ghp = GH_Palette.Warning
                'End If

                Using nameCapsule As GH_Capsule = GH_Capsule.CreateCapsule(rect, ghp)
                    nameCapsule.Render(graphics, Me.Selected, Me.Owner.Locked, Me.Owner.Hidden)
                End Using

                Dim texth As TextRenderingHint = graphics.TextRenderingHint
                graphics.TextRenderingHint = TextRenderingHint.AntiAlias
                If canvas.Viewport.Zoom > 0.6 Then graphics.DrawString(str, myfont, Brushes.Black, layrect, myformat)
                graphics.TextRenderingHint = texth

        End Select

        MyBase.Render(canvas, graphics, channel)

    End Sub

End Class
