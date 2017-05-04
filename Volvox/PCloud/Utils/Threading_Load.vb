
Imports System.Drawing
Imports Rhino.Geometry

Public Class Threading_Load
    Friend stringstoparse As New List(Of String)
    Friend outcloud As PointCloud = Nothing
    Friend t As Threading.Thread = Nothing
    Private myseed As Integer
    Private mypercent As Double

    Sub New(listofstrings As List(Of String))
        stringstoparse = listofstrings
        t = New System.Threading.Thread(AddressOf Parse)
        t.IsBackground = True
    End Sub

    Sub Start(mask As String, percent As Double, seed As Integer)
        outcloud = New PointCloud
        myseed = seed
        mypercent = percent
        t.Start(mask)
    End Sub

    Sub Parse(mymask As String)

        Dim parser As New Parse_MultiLoad(mymask)

        Dim rnd As New Random(myseed)

        For Each s As String In stringstoparse
            If mypercent <= 1 Then
                If rnd.NextDouble + mypercent > 1 Then
                    Dim p As Multipoint = parser.TextToMultipoint(s)

                    outcloud.AppendNew()
                    outcloud(outcloud.Count - 1).Location = New Point3d(p.X, p.Y, p.Z)
                    If p.ContainsNormals Then outcloud(outcloud.Count - 1).Normal = New Vector3d(p.U, p.V, p.W)
                    If p.ContainsIntensity Then
                        If p.ContainsColors Then
                            Dim intens As Double = p.A / 255
                            outcloud(outcloud.Count - 1).Color = Color.FromArgb(p.R * intens, p.G * intens, p.B * intens)
                        Else
                            outcloud(outcloud.Count - 1).Color = Color.FromArgb(p.A, p.A, p.A)
                        End If
                    Else
                        If p.ContainsColors Then outcloud(outcloud.Count - 1).Color = Color.FromArgb(p.R, p.G, p.B)
                    End If
                End If
            Else
                Dim p As Multipoint = parser.TextToMultipoint(s)

                outcloud.AppendNew()
                outcloud(outcloud.Count - 1).Location = New Point3d(p.X, p.Y, p.Z)
                If p.ContainsNormals Then outcloud(outcloud.Count - 1).Normal = New Vector3d(p.U, p.V, p.W)
                If p.ContainsIntensity Then
                    If p.ContainsColors Then
                        Dim intens As Double = p.A / 255
                        outcloud(outcloud.Count - 1).Color = Color.FromArgb(p.R * intens, p.G * intens, p.B * intens)
                    Else
                        outcloud(outcloud.Count - 1).Color = Color.FromArgb(p.A, p.A, p.A)
                    End If
                Else
                    If p.ContainsColors Then outcloud(outcloud.Count - 1).Color = Color.FromArgb(p.R, p.G, p.B)
                End If
            End If
        Next

        stringstoparse.Clear()

    End Sub


    Sub Abort()
        If t IsNot Nothing Then
            t.Abort()
        End If
        stringstoparse.Clear()
        outcloud.Dispose()
    End Sub

End Class