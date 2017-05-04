Imports Rhino.Geometry

Class Parse_MultiLoad

    Private CurrentMask() As Integer
    Private ContainsColors As Boolean = False
    Private ContainsNormals As Boolean = False
    Private ContainsInensity As Boolean = False
    Private Separator As String
    Private HasMask As Boolean = False

    Sub New(Mask As String)
        MaskParse(Mask)
    End Sub

    Private Sub MaskParse(Mask As String)

        ContainsColors = False
        ContainsNormals = False
        ContainsInensity = False

        Separator = String.Empty

        For Each c As Char In Mask
            If Char.IsSeparator(c) Or Char.IsPunctuation(c) Or Char.IsWhiteSpace(c) Then
                Separator = c.ToString
                Exit For
            End If
        Next

        If String.IsNullOrEmpty(Separator) Then Exit Sub

        Dim MaskString() As String = Mask.Split(Separator)
        Dim MaskInt(9) As Integer

        For i As Integer = 0 To 9 Step 1
            MaskInt(i) = 0
        Next

        For i As Integer = 0 To MaskString.Length - 1 Step 1
            Select Case MaskString(i)
                Case = "x"
                    MaskInt(0) = i
                Case = "y"
                    MaskInt(1) = i
                Case = "z"
                    MaskInt(2) = i
                Case = "r"
                    ContainsColors = True
                    MaskInt(3) = i
                Case = "g"
                    MaskInt(4) = i
                Case = "b"
                    MaskInt(5) = i
                Case = "u"
                    ContainsNormals = True
                    MaskInt(6) = i
                Case = "v"
                    MaskInt(7) = i
                Case = "w"
                    MaskInt(8) = i
                Case = "a"
                    ContainsInensity = True
                    MaskInt(9) = i
            End Select
        Next

        CurrentMask = Nothing
        CurrentMask = MaskInt

    End Sub

    Function TextToMultipoint(Line As String) As Multipoint
        Dim texts() As Double = (ManualSplit(Line, Separator))

        Dim nm As New Multipoint((texts(CurrentMask(0))), (texts(CurrentMask(1))), (texts(CurrentMask(2))),
                                  (texts(CurrentMask(3))), (texts(CurrentMask(4))), (texts(CurrentMask(5))),
                                  (texts(CurrentMask(6))), (texts(CurrentMask(7))), (texts(CurrentMask(8))),
                                  (texts(CurrentMask(9))))

        nm.ContainsColors = ContainsColors
        nm.ContainsIntensity = ContainsInensity
        nm.ContainsNormals = ContainsNormals

        Return nm

    End Function

    Function ManualSplit(Line As String, Separate As Char) As Double()

        Dim str(9) As Double

        Dim thisnum As New String("")
        Dim count As Integer = 0

        For Each c As Char In Line

            If c = Separate Then
                If thisnum.Length > 0 Then
                    str(count) = Conversion.Val(thisnum)
                    thisnum = New String("")
                    count += 1
                End If
            Else
                If Not c = Chr(34) Then thisnum &= c
            End If

        Next

        If thisnum.Length > 0 Then str(count) = (Conversion.Val(thisnum))

        Return str
    End Function

End Class
