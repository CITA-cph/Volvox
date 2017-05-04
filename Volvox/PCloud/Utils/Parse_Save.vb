Imports Rhino.Geometry

Public Class Parse_Save

    Private Separator As String
    Private Mask() As String
    Private Prec As Double
    Private PrecDenom As Double
    Private Dq As Boolean

    Sub New(SaveMask As String, Precision As Double, DQuote As Boolean)

        For Each c As Char In SaveMask
            If Char.IsSeparator(c) Or Char.IsPunctuation(c) Or Char.IsWhiteSpace(c) Then
                Separator = c.ToString
                Exit For
            End If
        Next

        Mask = SaveMask.Split(Separator)
        Prec = Precision
        PrecDenom = 1 / Prec
        Dq = DQuote
    End Sub

    Function PCloudItemToText(pc As PointCloud, id As Integer) As String
        Dim thisline As String = String.Empty

        For i As Integer = 0 To Mask.GetUpperBound(0) Step 1
            If i > 0 Then thisline &= Separator

            If Dq Then thisline &= Chr(34)
            Select Case Mask(i)
                Case "x"
                    thisline &= CDbl(CInt(pc(id).Location.X * PrecDenom)) * Prec
                Case "y"
                    thisline &= CDbl(CInt(pc(id).Location.Y * PrecDenom)) * Prec
                Case "z"
                    thisline &= CDbl(CInt(pc(id).Location.Z * PrecDenom)) * Prec
                Case "u"
                    thisline &= CDbl(CInt(pc(id).Normal.X * PrecDenom)) * Prec
                Case "v"
                    thisline &= CDbl(CInt(pc(id).Normal.Y * PrecDenom)) * Prec
                Case "w"
                    thisline &= CDbl(CInt(pc(id).Normal.Z * PrecDenom)) * Prec
                Case "r"
                    thisline &= pc(id).Color.R
                Case "g"
                    thisline &= pc(id).Color.G
                Case "b"
                    thisline &= pc(id).Color.B
                Case "a"
                    thisline &= pc(id).Color.A
            End Select
            If Dq Then thisline &= Chr(34)

        Next

        Return thisline
    End Function



End Class
