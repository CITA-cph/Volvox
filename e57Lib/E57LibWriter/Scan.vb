Imports System.IO
Imports E57LibCommon


Public Class Scan

    Private Owner As e57Document = Nothing

    Sub New(ParentDocument As e57Document)
        Owner = ParentDocument
        MyPose(0) = 1
    End Sub

    'todo - for now some properties are missing (we just want to write simple xyz points for testing) 
    Public Property Guid As String = String.Empty
    Public Property OriginalGuids As List(Of String) = New List(Of String)
    Public Property Name As String = String.Empty
    Public Property Description As String = String.Empty

    'sensor stuff
    Public Property SensorVendor As String = String.Empty
    Public Property SensorModel As String = String.Empty
    Public Property SensorSerialNumber As String = String.Empty
    Public Property SensorHardwareVersion As String = String.Empty
    Public Property SensorSoftwareVersion As String = String.Empty
    Public Property SensorFirmwareVersion As String = String.Empty
    Public Property StartDate As Date = Nothing
    Public Property IsStartAtomicClockReferenced As Boolean = False
    Public Property EndDate As Date = Nothing
    Public Property IsEndAtomicClockReferenced As Boolean = False

    Private MeHasColor As Boolean = False
    Public ReadOnly Property HasColor As Boolean
        Get
            Return MeHasColor
        End Get
    End Property

    Private MeHasIntensity As Boolean = False
    Public ReadOnly Property HasIntensity As Boolean
        Get
            Return MeHasIntensity
        End Get
    End Property

    Private MeHasSpherical As Boolean = False
    Public ReadOnly Property HasSpherical As Boolean
        Get
            Return MeHasSpherical
        End Get
    End Property

    Private MeHasCartesian As Boolean = False
    Public ReadOnly Property HasCartesian As Boolean
        Get
            Return MeHasCartesian
        End Get
    End Property

    Private MeHasPose As Boolean = False
    Public ReadOnly Property HasPose As Boolean
        Get
            Return MeHasPose
        End Get
    End Property

    Private MyPose(6) As Double
    Private MyCartBounds(5) As Double
    Private MySpheBounds(5) As Double

    Private MyIntensLimits(1) As Double
    Private MyColorLimits(5) As Double

    Private MyTemp As Double = Double.NaN
    Private MyPres As Double = Double.NaN
    Private MyHumi As Double = Double.NaN

    Private MyData As New ScanData

    ''' <summary>
    ''' Length in bits of the whole binary part (that's for xml) 
    ''' </summary>
    ''' <returns></returns>
    Friend Property SectionLength As UInt64

    ''' <summary>
    ''' Offset of the whole binary part (that's for xml part) 
    ''' </summary>
    ''' <returns></returns>
    Friend Property CompressedVectorOffset As UInt64

    Friend Sub Encode(ByRef Str As FileStream)

        'temporary var for storing position
        Dim ThisPos As UInt64 = Str.Position

        'total bytecount for the section length property
        Dim SectionLng As UInt64 = 0

        'set this scan offset (used in the xml part as well as here for first data packet offset)
        CompressedVectorOffset = Str.Position

        'add header bytes (for now its zeroed, values will  be set in the actual writing sub OR LATER)
        SectionLng += WriteWithChecksum(Str, (WriteCompressedVectorHeader(0, CompressedVectorOffset + 32, 0)))

        Dim RecordBytes As Integer = 0
        Dim RecBytes As New List(Of UShort)

        For i As Integer = 0 To Me.Data.Prototype.Count - 1 Step 1
            Select Case Me.Data.Prototype(i).ElementType
                Case ElementType.Double
                    RecordBytes += 8
                    RecBytes.Add(8)
                Case ElementType.Integer
                    Dim thisel As IntegerElement = Me.Data.Prototype(i)
                    RecordBytes += BitsPerInteger(thisel.Minimum, thisel.Maximum) * 0.125
                    RecBytes.Add(BitsPerInteger(thisel.Minimum, thisel.Maximum) * 0.125)
                Case ElementType.Single
                    RecordBytes += 4
                    RecBytes.Add(4)
            End Select
        Next

        Dim RecordsLeft As Int64 = Me.Data.RecordCount
        Dim Capacity As UShort = UShort.MaxValue
        Dim BtsCapacity As UShort = Capacity - (2 * Me.Data.Prototype.Count) - 6 'Capacity - ByteStreamLengths - Header
        Dim CurrentRecord As Int64 = 0

        Dim RecSort As New List(Of UShort)(RecBytes)
        RecSort.Sort()

        While RecordsLeft > 0
            Dim BytesToTake As Int64 = 0
            Dim RecordsToTake As Int64 = 0
            Dim Counting As Boolean = True

            RecordsToTake = Math.Min(UShort.MaxValue, RecordsLeft)
            BytesToTake = Math.Min(BtsCapacity, RecordsToTake * RecordBytes)
            RecordsToTake = Math.Floor(BytesToTake / RecordBytes)
            BytesToTake = Math.Min(BtsCapacity, RecordsToTake * RecordBytes)

            'While Counting
            '    If BytesToTake + RecordBytes >= BtsCapacity Then Counting = False
            '    If RecordsToTake + 1 >= UShort.MaxValue Then Counting = False
            '    If Not Counting Then Exit While

            '    BytesToTake += RecordBytes
            '    RecordsToTake += 1

            '    If RecordsToTake = RecordsLeft Then Exit While
            'End While

            Dim LengthPrediction As UShort = 6 + (2 * Me.Data.Prototype.Count) + (BytesToTake) 'header+btslengths+data
            Dim modLength As UShort = (Math.Ceiling(LengthPrediction / 4) * 4) - LengthPrediction
            LengthPrediction += modLength

            'header
            SectionLng += WriteWithChecksum(Str, WriteDataPacketHeader(0, LengthPrediction - 1, Me.Data.Prototype.Count))

            'bytestream lengths
            For i As Integer = 0 To Me.Data.Prototype.Count - 1 Step 1
                SectionLng += WriteWithChecksum(Str, BitConverter.GetBytes(CUShort(RecordsToTake * RecBytes(i))))
            Next

            For i As Integer = 0 To Me.Data.Prototype.Count - 1 Step 1
                Select Case Me.Data.Prototype(i).ElementType
                    Case ElementType.Double
                        Dim dbl As List(Of Double) = Me.Data.DoubleData(Me.Data.Prototype(i).ElementName).GetRange(CurrentRecord, RecordsToTake)

                        Dim b((dbl.Count * 8) - 1) As Byte
                        For j As Integer = 0 To dbl.Count - 1 Step 1
                            BitConverter.GetBytes(dbl(j)).CopyTo(b, j * 8)
                        Next
                        SectionLng += WriteWithChecksum(Str, b)

                    Case ElementType.Integer
                        Dim Numbers As List(Of Int64) = Me.Data.IntegerData(Me.Data.Prototype(i).ElementName).GetRange(CurrentRecord, RecordsToTake)
                        Dim Proto As IntegerElement = Me.Data.Prototype.GetElement(Me.Data.Prototype(i).ElementName)
                        Dim bits As Integer = E57LibCommon.Binary.BitsPerInteger(Proto.Minimum, Proto.Maximum)
                        Dim OutNums As New List(Of Byte)

                        Dim ToA As New BitArray(bits)
                        Dim BitA As New BitArray(8)
                        Dim bitcount As Integer = 0

                        For j As Integer = 0 To Numbers.Count - 1 Step 1
                            Dim this As UInt64 = Numbers(j) - Proto.Minimum
                            Dim FromA As New BitArray(BitConverter.GetBytes(this))

                            For k As Integer = 0 To bits - 1 Step 1
                                BitA(bitcount) = FromA(k)
                                bitcount += 1

                                If bitcount = 8 Then
                                    Dim thisb(0) As Byte
                                    BitA.CopyTo(thisb, 0)
                                    OutNums.Add(thisb(0))
                                    bitcount = 0
                                    BitA.SetAll(0)
                                End If
                            Next
                        Next

                        If bitcount > 0 Then
                            Dim thisb(0) As Byte
                            BitA.CopyTo(thisb, 0)
                            OutNums.Add(thisb(0))
                        End If

                        SectionLng += WriteWithChecksum(Str, OutNums.ToArray)
                        OutNums.Clear()

                    Case ElementType.Single
                        Dim sgn As List(Of Single) = Me.Data.SingleData(Me.Data.Prototype(i).ElementName).GetRange(CurrentRecord, RecordsToTake)

                End Select
            Next

            If modLength > 0 Then
                SectionLng += WriteWithChecksum(Str, ZeroArray(modLength))
            End If

            RecordsLeft -= RecordsToTake
            CurrentRecord += RecordsToTake
        End While

        If SectionLng Mod 4 > 0 Then
            Dim howmany As Integer = (Math.Ceiling(SectionLng / 4) * 4) - SectionLng
            SectionLng += WriteWithChecksum(Str, ZeroArray(howmany))
        End If

        Me.SectionLength = SectionLng

        ThisPos = Str.Position
        Str.Position = CompressedVectorOffset + 8
        WriteWithChecksum(Str, BitConverter.GetBytes(Me.SectionLength))
        Str.Position = ThisPos


    End Sub

    ''' <summary>
    ''' Order according to the e57standard table (page 12) 
    ''' </summary>
    ''' <returns></returns>
    Public Property Pose As Double()
        Get
            Return MyPose
        End Get
        Set(value As Double())
            MyPose = value
            MeHasPose = True
        End Set
    End Property

    Public Property CartesianBounds As Double()
        Get
            Return MyCartBounds
        End Get
        Set(value As Double())
            MyCartBounds = value
            MeHasCartesian = True
        End Set
    End Property

    Public Property SphericalBounds As Double()
        Get
            Return MySpheBounds
        End Get
        Set(value As Double())
            MySpheBounds = value
            MeHasSpherical = True
        End Set
    End Property

    Public Property IntensityLimits As Double()
        Get
            Return MyIntensLimits
        End Get
        Set(value As Double())
            MyIntensLimits = value
            MeHasIntensity = True
        End Set
    End Property

    Public Property ColorLimits As Double()
        Get
            Return MyColorLimits
        End Get
        Set(value As Double())
            MyColorLimits = value
            MeHasColor = True
        End Set
    End Property

    Public Property Temperature As Double
        Get
            Return MyTemp
        End Get
        Set(value As Double)
            value = Math.Max(-273.15, value)
            MyTemp = value
        End Set
    End Property

    Public Property RelativeHumidity As Double
        Get
            Return MyHumi
        End Get
        Set(value As Double)
            value = Math.Max(0, Math.Min(100, value))
            MyHumi = value
        End Set
    End Property

    Public Property AtmosphericPressure As Double
        Get
            Return MyPres
        End Get
        Set(value As Double)
            value = Math.Max(0, value)
            MyPres = value
        End Set
    End Property

    Public Property Data As ScanData
        Get
            Return MyData
        End Get
        Set(value As ScanData)
            MyData = value
        End Set
    End Property

    Private Function FillInValues(V() As Double, Name As String) As Boolean

        Select Case Name

            Case ElementName.CartesianX, ElementName.CartesianY, ElementName.CartesianZ
                Me.MeHasCartesian = True
                Select Case Name
                    Case ElementName.CartesianX
                        V.CopyTo(Me.CartesianBounds, 0)
                    Case ElementName.CartesianY
                        V.CopyTo(Me.CartesianBounds, 2)
                    Case ElementName.CartesianZ
                        V.CopyTo(Me.CartesianBounds, 4)
                End Select

            Case ElementName.SphericalAzimuth, ElementName.SphericalElevation, ElementName.SphericalRange
                Me.MeHasSpherical = True
                Select Case Name
                    Case ElementName.SphericalRange
                        V.CopyTo(Me.SphericalBounds, 0)
                    Case ElementName.SphericalElevation
                        V.CopyTo(Me.SphericalBounds, 2)
                    Case ElementName.SphericalAzimuth
                        V.CopyTo(Me.SphericalBounds, 4)
                End Select

            Case ElementName.ColorBlue, ElementName.ColorGreen, ElementName.ColorRed
                Me.MeHasColor = True
                Select Case Name
                    Case ElementName.ColorRed
                        V.CopyTo(Me.ColorLimits, 0)
                    Case ElementName.ColorGreen
                        V.CopyTo(Me.ColorLimits, 2)
                    Case ElementName.ColorBlue
                        V.CopyTo(Me.ColorLimits, 4)
                End Select

            Case ElementName.Intensity
                Me.MeHasIntensity = True
                V.CopyTo(Me.IntensityLimits, 0)

        End Select
        Return True
    End Function

    Public Sub AppendData(ElementName As String, Data As List(Of Int64))
        Dim Vals() As Int64 = MyData.AppendData(ElementName, Data)
        Dim V(1) As Double
        V(0) = Vals(0)
        V(1) = Vals(1)
        FillInValues(V, ElementName)
    End Sub

    ''' <summary>
    ''' Append all ElementName values only once
    ''' </summary>
    ''' <param name="ElementName"></param>
    ''' <param name="Data"></param>
    Public Sub AppendData(ElementName As String, Data As List(Of Single))
        Dim Vals() As Single = MyData.AppendData(ElementName, Data)
        Dim V(1) As Double
        V(0) = Vals(0)
        V(1) = Vals(1)
        FillInValues(V, ElementName)
    End Sub

    ''' <summary>
    ''' SCALED INTEGERS NOT SUPPORTED !!! Append all ElementName values only once
    ''' </summary>
    ''' <param name="ElementName"></param>
    ''' <param name="Data"></param>
    ''' <param name="AsScaled"></param>
    Public Sub AppendData(ElementName As String, Data As List(Of Double), Optional AsScaled As Boolean = False)
        Dim Vals() As Double = MyData.AppendData(ElementName, Data, AsScaled)
        Dim V(1) As Double
        V(0) = Vals(0)
        V(1) = Vals(1)
        FillInValues(V, ElementName)
    End Sub

    Public Sub GenerateGuid()
        Me.Guid = System.Guid.NewGuid.ToString
    End Sub

End Class
