Imports System.ComponentModel
Imports e57Lib.XmlReflection

Public Class ScanData

    Private IntegerData As New SortedList(Of String, List(Of Int64))
    Private DoubleData As New SortedList(Of String, List(Of Double))

    Sub New(Prototype As e57Node)
        Dim Elements As New List(Of e57Node)
        Prototype.Downstream(Elements, False)

        For Each el As e57Node In Elements

            Select Case el.Type
                Case Enumerations.e57Type.e57ScaledInteger
                    DoubleData.Add(el.Name, New List(Of Double))
                Case Enumerations.e57Type.e57Integer
                    IntegerData.Add(el.Name, New List(Of Int64))
                Case Enumerations.e57Type.e57Float
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
        ElseIf IntegerData.ContainsKey(ElementName)
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
        ElseIf DoubleData.ContainsKey(ElementName)
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
            str &= "___" & s & vbCrLf
        Next

        str &= "Int64 values:" & vbCrLf

        For Each s As String In IntegerData.Keys
            str &= "___" & s & vbCrLf
        Next

        Return str

    End Function

    Public Function TypeOfElement(ElementName As String) As ElementType
        If DoubleData.ContainsKey(ElementName) Then Return ElementType.Double
        If IntegerData.ContainsKey(ElementName) Then Return ElementType.Integer
        Return ElementType.Empty
    End Function

    Friend Function AppendList(ElementName As String, DoubleList As List(Of Double)) As Boolean
        Try
            If Not DoubleData.ContainsKey(ElementName) Then Return False
            DoubleData(ElementName).AddRange(DoubleList)
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Friend Function AppendList(ElementName As String, IntegerList As List(Of Int64)) As Boolean
        Try
            If IntegerData Is Nothing Then Return False
            If Not IntegerData.ContainsKey(ElementName) Then Return False
            IntegerData(ElementName).AddRange(IntegerList)
            Return True
        Catch ex As Exception
            Return False
        End Try

    End Function

    ''' <summary>
    ''' Empty means there is no such entry
    ''' </summary>
    Public Enum ElementType
        [Empty] = 0
        [Integer] = 1
        [Double] = 2
    End Enum

    Public Structure ElementName

        Const CartesianX = "cartesianX"
        Const CartesianY = "cartesianY"
        Const CartesianZ = "cartesianZ"

        Const SphericalRange = "sphericalRange"
        Const SphericalAzimuth = "sphericalAzimuth"
        Const SphericalElevation = "sphericalElevation"

        Const ColorRed = "colorRed"
        Const ColorBlue = "colorBlue"
        Const ColorGreen = "colorGreen"
        Const Intensity = "intensity"

        Const TimeStamp = "timeStamp"

        Const CartesianInvalidState = "cartesianInvalidState"
        Const SphericalInvalidState = "sphericalInvalidState"
        Const IsTimeStampInvalid = "isTimeStampInvalid"

        Const RowIndex = "rowIndex"
        Const ColumnIndex = "columnIndex"
        Const ReturnCount = "returnCount"
        Const ReturnIndex = "returnIndex"
        Const IsIntensityInvalid = "isIntensityInvalid"
        Const IsColorInvalid = "isColorInvalid"

    End Structure

End Class
