Imports E57LibCommon

Public Class ScanData

    Friend IntegerData As New SortedList(Of String, List(Of Int64))
    Friend DoubleData As New SortedList(Of String, List(Of Double))
    Friend ScaledIntegerData As New SortedList(Of String, List(Of Double))
    Friend SingleData As New SortedList(Of String, List(Of Single))

    Private MyPrototype As Prototype = New Prototype()
    Private MyCount As UInt64 = 0
    Private MyOffset As UInt64 = 0

    Sub New()

    End Sub

    Public ReadOnly Property Prototype As Prototype
        Get
            Return MyPrototype
        End Get
    End Property

    Public ReadOnly Property RecordCount As UInt64
        Get
            Return MyCount
        End Get
    End Property

    ''' <summary>
    ''' Append all ElementName values only once
    ''' </summary>
    ''' <param name="ElementName"></param>
    ''' <param name="Data"></param>
    Friend Function AppendData(ElementName As String, Data As List(Of Int64)) As Int64()
        IntegerData.Add(ElementName, Data)
        MyCount = Data.Count

        Dim min As Int64 = Int64.MaxValue
        Dim max As Int64 = Int64.MinValue

        For Each i As Int64 In Data
            If Math.Floor(i) < min Then min = Math.Floor(i)
            If Math.Ceiling(i) > max Then max = Math.Ceiling(i)
        Next

        'this is just a "temporary" solution
        If BitsPerInteger(min, max) Mod 8 Then
            Dim oldmin As Int64 = min
            Dim oldmax As Int64 = max

            While (BitsPerInteger(min, max) Mod 8)
                max += 1
            End While
        End If

        Dim OutV(1) As Int64
        OutV(0) = min
        OutV(1) = max

        Dim Proto As IntegerElement = Nothing

        If min <= max Then
            MyPrototype.Add(New IntegerElement(ElementName, min, max, min * 0.5 + max * 0.5))
        Else
            MyPrototype.Add(New IntegerElement(ElementName))
        End If

        Return OutV
    End Function

    ''' <summary>
    ''' Append all ElementName values only once
    ''' </summary>
    ''' <param name="ElementName"></param>
    ''' <param name="Data"></param>
    Friend Function AppendData(ElementName As String, Data As List(Of Single)) As Single()
        SingleData.Add(ElementName, Data)
        MyCount = Data.Count

        Dim min As Single = Single.MaxValue
        Dim max As Single = Single.MinValue

        For Each s As Single In Data
            min = Math.Min(min, s)
            max = Math.Max(max, s)
        Next

        Dim OutV(1) As Single
        OutV(0) = min
        OutV(1) = max

        MyPrototype.Add(New SingleElement(ElementName, min, max))
        Return OutV
    End Function

    ''' <summary>
    ''' Append all ElementName values only once
    ''' </summary>
    ''' <param name="ElementName"></param>
    ''' <param name="Data"></param>
    ''' <param name="AsScaled"></param>
    Friend Function AppendData(ElementName As String, Data As List(Of Double), Optional AsScaled As Boolean = False) As Double()
        MyCount = Data.Count

        Dim OutV(1) As Double

        Dim min As Double = Double.MaxValue
        Dim max As Double = Double.MinValue

        For Each s As Double In Data
            min = Math.Min(min, s)
            max = Math.Max(max, s)
        Next

        OutV(0) = min
        OutV(1) = max

        If AsScaled Then
            ScaledIntegerData.Add(ElementName, Data)
        Else
            DoubleData.Add(ElementName, Data)
            MyPrototype.Add(New DoubleElement(ElementName, min, max))
        End If

        Return OutV
    End Function

    Public Function HasElement(ElementName As String) As Boolean
        If IntegerData.ContainsKey(ElementName) Then Return True
        If ScaledIntegerData.ContainsKey(ElementName) Then Return True
        If DoubleData.ContainsKey(ElementName) Then Return True
        If SingleData.ContainsKey(ElementName) Then Return True
        Return False
    End Function

    Public Sub Clear()
        DoubleData.Clear()
        IntegerData.Clear()
        ScaledIntegerData.Clear()
        SingleData.Clear()
        DoubleData = Nothing
        SingleData = Nothing
        ScaledIntegerData = Nothing
        IntegerData = Nothing
    End Sub

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

End Class

