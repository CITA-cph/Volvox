Imports E57LibCommon

Public Class ScanData

    Private IntegerData As New SortedList(Of String, List(Of Int64))
    Private DoubleData As New SortedList(Of String, List(Of Double))
    Private SubCounters As New SortedList(Of String, ULong)
    Private SubRandom() As Boolean = Nothing

    Sub New(Prototype As e57Node)
        Dim Elements As New List(Of e57Node)
        Prototype.Downstream(Elements, False)

        For Each el As e57Node In Elements
            SubCounters.Add(el.Name, 0)

            Select Case el.Type
                Case e57Type.e57ScaledInteger
                    DoubleData.Add(el.Name, New List(Of Double))
                    BufferData.Add(el.Name, New List(Of Boolean))
                Case e57Type.e57Integer
                    IntegerData.Add(el.Name, New List(Of Int64))
                    BufferData.Add(el.Name, New List(Of Boolean))
                Case e57Type.e57Float
                    DoubleData.Add(el.Name, New List(Of Double))
            End Select
        Next

    End Sub

    Sub New()

    End Sub

    ''' <summary>
    ''' Doesn't keep track of parent.
    ''' </summary>
    ''' <returns></returns>
    Public Function DuplicateDataOnly() As ScanData
        Dim nd As New ScanData

        For Each k As String In IntegerData.Keys
            nd.IntegerData.Add(k, IntegerData(k))
        Next

        For Each k As String In DoubleData.Keys
            nd.DoubleData.Add(k, DoubleData(k))
        Next

        Return nd
    End Function

    Public Sub Clear()
        DoubleData.Clear()
        IntegerData.Clear()
        DoubleData = Nothing
        IntegerData = Nothing
    End Sub

    Public ReadOnly Property HasSpherical() As ElementType
        Get
            If DoubleData.ContainsKey(ElementName.SphericalRange) Then
                Return ElementType.Double
            End If

            If IntegerData.ContainsKey(ElementName.SphericalRange) Then
                Return ElementType.Integer
            End If

            Return ElementType.Empty
        End Get
    End Property

    Public ReadOnly Property HasCartesian() As ElementType
        Get
            If DoubleData.ContainsKey(ElementName.CartesianX) Then
                Return ElementType.Double
            End If

            If IntegerData.ContainsKey(ElementName.CartesianX) Then
                Return ElementType.Integer
            End If

            Return ElementType.Empty
        End Get
    End Property

    Public ReadOnly Property HasColor() As ElementType
        Get
            If DoubleData.ContainsKey(ElementName.ColorRed) Then
                Return ElementType.Double
            End If

            If IntegerData.ContainsKey(ElementName.ColorRed) Then
                Return ElementType.Integer
            End If

            Return ElementType.Empty
        End Get
    End Property

    Public ReadOnly Property HasIntensity() As ElementType
        Get
            If DoubleData.ContainsKey(ElementName.Intensity) Then
                Return ElementType.Double
            End If

            If IntegerData.ContainsKey(ElementName.Intensity) Then
                Return ElementType.Integer
            End If

            Return ElementType.Empty
        End Get
    End Property

    Public ReadOnly Property HasTimeStamp() As ElementType
        Get
            If DoubleData.ContainsKey(ElementName.TimeStamp) Then
                Return ElementType.Double
            End If

            If IntegerData.ContainsKey(ElementName.TimeStamp) Then
                Return ElementType.Integer
            End If

            Return ElementType.Empty
        End Get
    End Property

    Public Function HasData(ElementName As String) As ElementType
        If DoubleData.ContainsKey(ElementName) Then Return ElementType.Double
        If IntegerData.ContainsKey(ElementName) Then Return ElementType.Integer
        Return ElementType.Empty
    End Function

    Public Function GetList(ElementName As String, ByRef DoubleList As List(Of Double)) As Boolean
        If DoubleData.ContainsKey(ElementName) Then
            Return DoubleData.TryGetValue(ElementName, DoubleList)
        ElseIf IntegerData.ContainsKey(ElementName) Then
            Dim intl As New List(Of Long)
            Dim res As Boolean = IntegerData.TryGetValue(ElementName, intl)
            DoubleList.Clear()
            For Each int As Int64 In intl
                DoubleList.Add(int)
            Next
            Return res
        End If
        Return False
    End Function

    Public Function GetList(ElementName As String, ByRef IntegerList As List(Of Int64)) As Boolean
        If IntegerData.ContainsKey(ElementName) Then
            Return IntegerData.TryGetValue(ElementName, IntegerList)
        ElseIf DoubleData.ContainsKey(ElementName) Then
            Dim dbll As New List(Of Double)
            Dim res As Boolean = DoubleData.TryGetValue(ElementName, dbll)
            IntegerList.Clear()
            For Each dbl As Double In dbll
                IntegerList.Add(dbl)
            Next
            Return res
        End If
        Return False
    End Function

    Public Overloads Function ToString() As String

        Dim str As String = String.Empty

        str &= "Double values:" & vbCrLf

        For Each s As String In DoubleData.Keys
            str &= Separator & s & vbCrLf
        Next

        str &= "Int64 values:" & vbCrLf

        For Each s As String In IntegerData.Keys
            str &= Separator & s & vbCrLf
        Next

        Return str

    End Function

    Public Function TypeOfElement(ElementName As String) As ElementType
        If DoubleData.ContainsKey(ElementName) Then Return ElementType.Double
        If IntegerData.ContainsKey(ElementName) Then Return ElementType.Integer
        Return ElementType.Empty
    End Function

    Private BufferData As New SortedList(Of String, List(Of Boolean))

    Friend Sub GenerateRandoms(SubsamplingRate As Double, Count As ULong)

        If SubsamplingRate = 1 Then Return

        Dim rnd As New Random()
        ReDim SubRandom(Count - 1)

        For i As Integer = 0 To Count - 1 Step 1
            If rnd.NextDouble + SubsamplingRate > 1 = True Then
                SubRandom(i) = True
            End If
        Next

        For i As Integer = 0 To SubCounters.Count - 1 Step 1
            SubCounters(SubCounters.Keys(i)) = 0
        Next

    End Sub


    Friend Sub CleanRandoms()
        SubRandom = Nothing
    End Sub


    ''' <summary>
    ''' Adds Doubles and Singles
    ''' </summary>
    ''' <param name="ElementName"></param>
    ''' <param name="Bytes"></param>
    ''' <param name="IsDoublePrecision"></param>
    ''' <param name="SubSampleRate"></param>
    ''' <returns></returns>
    Friend Function AppendBytes(ElementName As String, Bytes As Byte(), IsDoublePrecision As Boolean, Optional SubSampleRate As Double = 1) As Boolean

        If SubSampleRate < 1 Then
            'If ShouldGenerateRandoms() Then GenerateRandoms(SubSampleRate, True)

            Dim thisl As New List(Of Double)
            Dim thiscounter As ULong = SubCounters(ElementName)

            If Not IsDoublePrecision Then                                            'single prec
                For i As Integer = 0 To Bytes.Length - 1 Step 4
                    If SubRandom(thiscounter) Then thisl.Add(CDbl(BitConverter.ToSingle(Bytes, i)))
                    thiscounter += 1 '(thiscounter + 1) Mod SubRandom.Length
                Next
            Else                                                                      'double prec 
                For i As Integer = 0 To Bytes.Length - 1 Step 8
                    If SubRandom(thiscounter) Then thisl.Add(CDbl(BitConverter.ToDouble(Bytes, i)))
                    thiscounter += 1 ' (thiscounter + 1) Mod SubRandom.Length
                Next
            End If

            SubCounters(ElementName) = thiscounter
            DoubleData(ElementName).AddRange(thisl)
        Else
            Dim thisl As New List(Of Double)
            If Not IsDoublePrecision Then                                            'single prec
                For i As Integer = 0 To Bytes.Length - 1 Step 4
                    thisl.Add(CDbl(BitConverter.ToSingle(Bytes, i)))
                Next
            Else                                                                     'double prec 
                For i As Integer = 0 To Bytes.Length - 1 Step 8
                    thisl.Add(CDbl(BitConverter.ToDouble(Bytes, i)))
                Next
            End If
            DoubleData(ElementName).AddRange(thisl)
        End If

        Return True
    End Function
    ''' <summary>
    ''' Adds Scaled Integers
    ''' </summary>
    ''' <param name="ElementName"></param>
    ''' <param name="Bytes"></param>
    ''' <param name="Minimum"></param>
    ''' <param name="Maximum"></param>
    ''' <param name="Scale"></param>
    ''' <param name="Offset"></param>
    ''' <param name="SubSampleRate"></param>
    ''' <returns></returns>
    Friend Function AppendBytes(ElementName As String, Bytes As Byte(), Minimum As Long, Maximum As Long, Scale As Double, Offset As Double, Optional SubSampleRate As Double = 1) As Boolean

        'int64
        Dim ba As New BitArray(Bytes)
        Dim bb(ba.Count - 1) As Boolean
        ba.CopyTo(bb, 0)
        ba = Nothing
        BufferData(ElementName).AddRange(bb)
        bb = Nothing

        ProcessBuffer(ElementName, BitsPerInteger(Minimum, Maximum), True, Minimum, Maximum, Scale, Offset, SubSampleRate)

        Return True
    End Function
    ''' <summary>
    ''' Adds Integers
    ''' </summary>
    ''' <param name="ElementName"></param>
    ''' <param name="Bytes"></param>
    ''' <param name="Minimum"></param>
    ''' <param name="Maximum"></param>
    ''' <param name="SubSampleRate"></param>
    ''' <returns></returns>
    Friend Function AppendBytes(ElementName As String, Bytes As Byte(), Minimum As Long, Maximum As Long, Optional SubSampleRate As Double = 1) As Boolean

        'int64
        Dim ba As New BitArray(Bytes)
        Dim bb(ba.Count - 1) As Boolean
        ba.CopyTo(bb, 0)
        ba = Nothing
        BufferData(ElementName).AddRange(bb)
        bb = Nothing

        ProcessBuffer(ElementName, BitsPerInteger(Minimum, Maximum), False, Minimum, Maximum, 1, 0, SubSampleRate)

        Return True
    End Function

    Private Sub ProcessBuffer(ElementName As String, BitsPerItem As Integer, IsScaled As Boolean, Minimum As Long, Maximum As Long, Optional Scale As Double = 1, Optional Offset As Double = 0, Optional SubSampleRate As Double = 1)
        Dim thisl As List(Of Boolean) = BufferData(ElementName)
        Dim tempi As New List(Of Int64)
        Dim tot As Integer = 0

        If SubSampleRate < 1 Then
            Dim thiscounter As ULong = SubCounters(ElementName)
            Dim arr(BitsPerItem - 1) As Boolean
            For i As Integer = 0 To thisl.Count - BitsPerItem - BitsPerItem Step BitsPerItem
                tot += 1
                If SubRandom(thiscounter) Then
                    thisl.CopyTo(i, arr, 0, BitsPerItem)
                    tempi.Add(IntFromBits(arr) + Minimum)
                End If
                thiscounter += 1 ' (thiscounter + 1) Mod SubRandom.Length
            Next
            SubCounters(ElementName) = thiscounter
        Else
            Dim arr(BitsPerItem - 1) As Boolean
            For i As Integer = 0 To thisl.Count - BitsPerItem - BitsPerItem Step BitsPerItem
                thisl.CopyTo(i, arr, 0, BitsPerItem)
                tempi.Add(IntFromBits(arr) + Minimum)
            Next
            tot = tempi.Count
        End If

        If IsScaled Then
            Dim tempd As New List(Of Double)
            For Each num As Int64 In tempi
                tempd.Add(ScaledIntegerToDouble(num, Scale, Offset))
            Next
            DoubleData(ElementName).AddRange(tempd)
        Else
            IntegerData(ElementName).AddRange(tempi)
        End If

        BufferData(ElementName).RemoveRange(0, tot * BitsPerItem)
        thisl = Nothing
    End Sub

    Function IntFromBits(myBits As Boolean()) As Int64
        Dim numeral As Int64
        For i As Integer = myBits.Length - 1 To 0 Step -1
            If myBits(i) Then
                numeral = numeral Or (1 << i)
            End If
        Next
        Return numeral
    End Function

    ''' <summary>
    ''' Empty means there is no such entry
    ''' </summary>
    Public Enum ElementType
        [Empty] = 0
        [Integer] = 1
        [Double] = 2
    End Enum

End Class