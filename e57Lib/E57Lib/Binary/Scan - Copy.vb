Imports e57Lib.XmlReflection
Imports e57Lib.XmlReflection.Collections
Imports System.IO
Imports e57Lib.Binary
Imports e57Lib.XmlReflection.Terminal
Imports e57Lib.ScanData

Public Class Scan

    Public FileOffset As Long = 0
    Private RecordCount As UInt64 = 0
    Private FPath As String = String.Empty
    Private MySubsampling As Double = 1

    Private MyData As ScanData = Nothing
    Private MyPrototype As e57Node = Nothing
    'Private ThisProto As e57Node = Nothing
    Dim Protos As New List(Of e57Node)

    Private MyPose(6) As Double
    Private MyGuid As Guid = System.Guid.Empty

    Private MyName As String = String.Empty
    Private MyDescription As String = String.Empty

    Private MyCartesianBounds(5) As Double
    Private MySphericalBounds(5) As Double
    Private MyIntensityLimits(1) As Double
    Private MyColorLimits(5) As Double

    Private MyAcqStartTime As Double
    Private MyAcqEndTime As Double
    Private MyAcqStartAtom As Boolean
    Private MyAcqEndAtom As Boolean

    Private MySensorVendor As String = String.Empty
    Private MySensorModel As String = String.Empty
    Private MySensorSerialNumber As String = String.Empty
    Private MySensorHWVer As String = String.Empty
    Private MySensorSWVer As String = String.Empty
    Private MySensorFWVer As String = String.Empty

    Private MyTemp As Double
    Private MyHumid As Double
    Private MyPress As Double

    Private rndval As New List(Of Double)


    Sub New(VectorChild As e57Structure, Document As e57Document)

        FPath = Document.DocumentFilePath

        Dim Points As e57CompressedVector = VectorChild.GetChild("points")
        MyData = New ScanData(Points.Prototype)
        MyPrototype = Points.Prototype

        Dim pose As e57Structure = VectorChild.GetChild("pose")
        If pose IsNot Nothing Then
            Dim rot As e57Structure = pose.GetChild("rotation")
            Dim trans As e57Structure = pose.GetChild("translation")

            MyPose(0) = rot.GetChild("w").Value
            MyPose(1) = rot.GetChild("x").Value
            MyPose(2) = rot.GetChild("y").Value
            MyPose(3) = rot.GetChild("z").Value
            MyPose(4) = trans.GetChild("x").Value
            MyPose(5) = trans.GetChild("y").Value
            MyPose(6) = trans.GetChild("z").Value

            For i As Integer = 0 To 6 Step 1
                If MyPose(i) = Double.NaN Then MyPose(i) = 0
            Next

        End If

        Dim tempguid As String = VectorChild.GetString("guid")
        If tempguid <> String.Empty Then MyGuid = New Guid(tempguid)

        MyName = VectorChild.GetString("name")
        MyDescription = VectorChild.GetString("description")

        MySensorVendor = VectorChild.GetString("sensorVendor")
        MySensorModel = VectorChild.GetString("sensorModel")
        MySensorSerialNumber = VectorChild.GetString("sensorSerialNumber")
        MySensorSWVer = VectorChild.GetString("sensorSoftwareVersion")
        MySensorHWVer = VectorChild.GetString("sensorHardwareVersion")
        MySensorFWVer = VectorChild.GetString("sensorFirmwareVersion")

        MyTemp = VectorChild.GetDouble("temperature")
        MyHumid = VectorChild.GetDouble("relativeHumidity")
        MyPress = VectorChild.GetDouble("atmosphericPressure")

        Dim TempCart As e57Structure = VectorChild.GetChild("cartesianBounds")
        If TempCart IsNot Nothing Then
            MyCartesianBounds(0) = TempCart.GetDouble("xMinimum")
            MyCartesianBounds(1) = TempCart.GetDouble("xMaximum")
            MyCartesianBounds(2) = TempCart.GetDouble("yMinimum")
            MyCartesianBounds(3) = TempCart.GetDouble("yMaximum")
            MyCartesianBounds(4) = TempCart.GetDouble("zMinimum")
            MyCartesianBounds(5) = TempCart.GetDouble("zMaximum")
        End If

        Dim TempSphe As e57Structure = VectorChild.GetChild("sphericalBounds")
        If TempSphe IsNot Nothing Then
            MySphericalBounds(0) = TempSphe.GetDouble("rangeMinimum")
            MySphericalBounds(1) = TempSphe.GetDouble("rangeMaximum")
            MySphericalBounds(2) = TempSphe.GetDouble("elevationMinimum")
            MySphericalBounds(3) = TempSphe.GetDouble("elevationMaximum")
            MySphericalBounds(4) = TempSphe.GetDouble("azimuthMinimum")
            MySphericalBounds(5) = TempSphe.GetDouble("azimuthMaximum")
        End If

        Dim TempIntens As e57Structure = VectorChild.GetChild("intensityLimits")
        If TempIntens IsNot Nothing Then
            MyIntensityLimits(0) = TempIntens.GetChild("intensityMinimum").Value
            MyIntensityLimits(1) = TempIntens.GetChild("intensityMaximum").Value
        End If

        Dim TempColor As e57Structure = VectorChild.GetChild("colorLimits")
        If TempColor IsNot Nothing Then
            MyColorLimits(0) = TempColor.GetChild("colorRedMinimum").Value
            MyColorLimits(1) = TempColor.GetChild("colorRedMaximum").Value

            MyColorLimits(2) = TempColor.GetChild("colorGreenMinimum").Value
            MyColorLimits(3) = TempColor.GetChild("colorGreenMaximum").Value

            MyColorLimits(4) = TempColor.GetChild("colorBlueMinimum").Value
            MyColorLimits(5) = TempColor.GetChild("colorBlueMaximum").Value
        End If

        Dim TempStart As e57Structure = VectorChild.GetChild("acquisitionStart")
        Dim TempEnd As e57Structure = VectorChild.GetChild("acquisitionEnd")

        If TempStart IsNot Nothing And TempEnd IsNot Nothing Then

            Dim atomstart As Double = TempStart.GetChild("isAtomicClockReferenced").Value
            If atomstart <> Double.NaN Then
                MyAcqStartAtom = CBool(atomstart)
            End If

            MyAcqStartTime = TempStart.GetDouble("dateTimeValue")

            Dim atomEnd As Double = TempEnd.GetChild("isAtomicClockReferenced").Value
            If atomEnd <> Double.NaN Then
                MyAcqEndAtom = CBool(atomEnd)
            End If

            MyAcqEndTime = TempEnd.GetDouble("dateTimeValue")

        End If

        FileOffset = Points.FileOffset
        RecordCount = Points.RecordCount

    End Sub

    Public Function ReadScanData(Optional SubsamplingRate As Double = 1) As Boolean
        MySubsampling = SubsamplingRate

        If MySubsampling <> 1 Then
            Dim rnd As New Random
            rndval.Clear()
            For i As Integer = 0 To Int16.MaxValue Step 1
                rndval.Add(rnd.NextDouble)
            Next
        End If

        Using Str As FileStream = New FileStream(FPath, FileMode.Open, FileAccess.Read)
            Dim headerBytes() As Byte = ReadAndSkip(Str, FileOffset, 32)

            Dim SectionID As Byte
                Dim SectionLength As ULong
                Dim DataStartOffset As ULong
                Dim IndexStartOffset As ULong

                ReadCompressedVectorHeader(headerBytes, SectionID, Nothing, SectionLength, DataStartOffset, IndexStartOffset)
                ReadPackets(Str, DataStartOffset, SectionLength)
        End Using

        Return True
    End Function

#Region "Properties"
    Public ReadOnly Property Data As ScanData
        Get
            Return MyData
        End Get
    End Property

    Public ReadOnly Property Translation As Double()
        Get
            Dim trans(2) As Double
            trans(0) = MyPose(4)
            trans(1) = MyPose(5)
            trans(2) = MyPose(6)
            Return trans
        End Get
    End Property

    Public ReadOnly Property Rotation As Double()
        Get
            Dim rot(3) As Double
            rot(0) = MyPose(0)
            rot(1) = MyPose(1)
            rot(2) = MyPose(2)
            rot(3) = MyPose(3)
            Return rot
        End Get
    End Property

    Public ReadOnly Property Name As String
        Get
            Return MyName
        End Get
    End Property

    Public ReadOnly Property Description As String
        Get
            Return MyDescription
        End Get
    End Property

    Public ReadOnly Property GUID As Guid
        Get
            Return MyGuid
        End Get
    End Property

    Public ReadOnly Property SensorVendor As String
        Get
            Return MySensorVendor
        End Get
    End Property

    Public ReadOnly Property SensorModel As String
        Get
            Return MySensorModel
        End Get
    End Property

    Public ReadOnly Property SensorSerialNumber As String
        Get
            Return MySensorSerialNumber
        End Get
    End Property

    Public ReadOnly Property SensorHardwareVersion As String
        Get
            Return MySensorHWVer
        End Get
    End Property

    Public ReadOnly Property SensorSoftwareVersion As String
        Get
            Return MySensorSWVer
        End Get
    End Property

    Public ReadOnly Property SensorFirmwareVersion As String
        Get
            Return MySensorFWVer
        End Get
    End Property

    Public ReadOnly Property Temperature As Double
        Get
            Return MyTemp
        End Get
    End Property

    Public ReadOnly Property RelativeHumidity As Double
        Get
            Return MyHumid
        End Get
    End Property

    Public ReadOnly Property AtmosphericPressure As Double
        Get
            Return MyPress
        End Get
    End Property

    Public ReadOnly Property AcquisitionStart As Double
        Get
            Return MyAcqStartTime
        End Get
    End Property

    Public ReadOnly Property AcquisitionEnd As Double
        Get
            Return MyAcqEndTime
        End Get
    End Property

    Public ReadOnly Property IsStartAtomicClockReferenced As Boolean
        Get
            Return MyAcqStartAtom
        End Get
    End Property

    Public ReadOnly Property IsEndAtomicClockReferenced As Boolean
        Get
            Return MyAcqEndAtom
        End Get
    End Property

    ''' <summary>
    ''' xMinimum/xMaximum/yMinimum/yMaximum/zMinimum/zMaximum
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property CartesianBounds As Double()
        Get
            Return MyCartesianBounds
        End Get
    End Property

    ''' <summary>
    ''' rangeMinimum/rangeMaximum/elevationMinimum/elevationMaximum/azimuthStart/azimuthStart
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property SphericalBounds As Double()
        Get
            Return MySphericalBounds
        End Get
    End Property

    ''' <summary>
    ''' intensityMinimum/intensityMaximum
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property IntensityLimits As Double()
        Get
            Return MyIntensityLimits
        End Get
    End Property

    ''' <summary>
    ''' colorRedMinimum/colorRedMaximum/colorGreenMinimum/colorGreenMaximum/colorBlueMinimum/colorBlueMaximum
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property ColorLimits As Double()
        Get
            Return MyColorLimits
        End Get
    End Property

#End Region

#Region "Decoding"

    Public Event BytesRead(sender As Scan, e As BytesReadEventArgs)
    Private MyEventArgs As New BytesReadEventArgs(0, 0)

    Private Function ReadPackets(ByRef str As FileStream, Position As Long, Length As UInt64) As Boolean

        Dim BytesRead As UInt64 = 0
        str.Position = Position

        MyPrototype.Downstream(Protos, False)
        MyEventArgs.TotalLength = Length
        Dim LastPercent As Integer = 0
        Dim ThisPercent As Integer = 0

        While BytesRead < Length

            Dim moddd As Long = str.Position Mod 4

            Select Case moddd
                Case 0
                Case Else
                    str.Position += 4 - moddd
            End Select

            Dim prepos As Int64 = str.Position
            Dim packetType() As Byte = ReadAndSkip(str, str.Position, 1)
            str.Position = prepos

            Select Case packetType(0)
                Case 0
                    BytesRead += ReadIndexPacket(str, str.Position)
                Case 1
                    BytesRead += ReadDataPacket(str, str.Position)
                Case 2
                    BytesRead += ReadIgnoredPacket(str, str.Position)
                Case Else
                    Exit While
            End Select

            MyEventArgs.BytesRead = BytesRead
            ThisPercent = MyEventArgs.Percent

            If ThisPercent > LastPercent Or LastPercent = 0 Then
                RaiseEvent BytesRead(Me, MyEventArgs)
                LastPercent = ThisPercent
            End If

        End While

        Return True
    End Function

    Private Sub JitterRandom()
        For i As Integer = 0 To rndval.Count - 1 Step 1
            rndval(i) = (rndval(i) + 0.17) Mod 1
        Next
    End Sub

    Private Function ReadDataPacket(ByRef str As FileStream, Position As Long) As UInt64
        Try
            Dim PacketHeader() As Byte = ReadAndSkip(str, Position, 6)

            Dim PacketType As Byte
            Dim PacketFlags As Byte
            Dim PacketLengthMinus1 As UShort
            Dim BytestreamCount As UShort

            E57Lib.Binary.DecodingModules.ReadDataPacketHeader(PacketHeader, PacketType, PacketFlags, PacketLengthMinus1, BytestreamCount)
            PacketHeader = Nothing
            If PacketLengthMinus1 = 0 Then Return 0

            Dim ByteLenghts As New List(Of UInt16)

            For i As Integer = 0 To BytestreamCount - 1 Step 1
                Dim thisCount() As Byte = ReadAndSkip(str, str.Position, 2)
                ByteLenghts.Add(BitConverter.ToUInt16(thisCount, 0))
                thisCount = Nothing
            Next

            Dim mes As String = String.Empty
            Dim counter As Integer = 0

            If MySubsampling = 1 Then
                For i As Integer = 0 To Protos.Count - 1 Step 1
                    Dim thisn As e57Node = Protos(i)
                    Select Case thisn.Type
                        Case Enumerations.e57Type.e57Float
                            Dim meas As e57Float = thisn
                            Select Case meas.DoublePrecision
                                Case True
                                    MyData.AppendList(thisn.Name, EasyBinary.ReadDoubleBytestream(str, str.Position, ByteLenghts(i)))
                                Case False
                                    MyData.AppendList(thisn.Name, EasyBinary.ReadSingleBytestream(str, str.Position, ByteLenghts(i)))
                            End Select
                        Case Enumerations.e57Type.e57ScaledInteger
                            Dim meas As e57ScaledInteger = thisn
                            MyData.AppendList(thisn.Name, EasyBinary.ReadScaledIntegerBytestream(str, str.Position, ByteLenghts(i), meas.Minimum, meas.Maximum, meas.Scale, meas.Offset))
                        Case Enumerations.e57Type.e57Integer
                            Dim meas As e57Integer = thisn
                            MyData.AppendList(thisn.Name, EasyBinary.ReadIntegerBytestream(str, str.Position, ByteLenghts(i), meas.Minimum, meas.Maximum))
                    End Select
                Next
            Else
                JitterRandom()
                For i As Integer = 0 To Protos.Count - 1 Step 1
                    Dim thisn As e57Node = Protos(i)
                    Select Case thisn.Type
                        Case Enumerations.e57Type.e57Float
                            Dim meas As e57Float = thisn
                            Select Case meas.DoublePrecision
                                Case True
                                    Dim thislist As List(Of Double) = EasyBinary.ReadDoubleBytestream(str, str.Position, ByteLenghts(i))
                                    Dim thatlist As New List(Of Double)
                                    For j As Integer = 0 To thislist.Count - 1 Step 1
                                        If rndval(j) + MySubsampling > 1 Then
                                            thatlist.Add(thislist(j))
                                        End If
                                    Next
                                    MyData.AppendList(thisn.Name, thatlist)
                                Case False
                                    Dim thislist As List(Of Double) = EasyBinary.ReadSingleBytestream(str, str.Position, ByteLenghts(i))
                                    Dim thatlist As New List(Of Double)
                                    For j As Integer = 0 To thislist.Count - 1 Step 1
                                        If rndval(j) + MySubsampling > 1 Then
                                            thatlist.Add(thislist(j))
                                        End If
                                    Next
                                    MyData.AppendList(thisn.Name, thatlist)
                            End Select
                        Case Enumerations.e57Type.e57ScaledInteger
                            Dim meas As e57ScaledInteger = thisn
                            Dim thislist As List(Of Double) = EasyBinary.ReadScaledIntegerBytestream(str, str.Position, ByteLenghts(i), meas.Minimum, meas.Maximum, meas.Scale, meas.Offset)
                            Dim thatlist As New List(Of Double)
                            For j As Integer = 0 To thislist.Count - 1 Step 1
                                If rndval(j) + MySubsampling > 1 Then
                                    thatlist.Add(thislist(j))
                                End If
                            Next
                            MyData.AppendList(thisn.Name, thatlist)
                        Case Enumerations.e57Type.e57Integer
                            Dim meas As e57Integer = thisn
                            Dim thislist As List(Of Long) = EasyBinary.ReadIntegerBytestream(str, str.Position, ByteLenghts(i), meas.Minimum, meas.Maximum)
                            Dim thatlist As New List(Of Long)
                            For j As Integer = 0 To thislist.Count - 1 Step 1
                                If rndval(j) + MySubsampling > 1 Then
                                    thatlist.Add(thislist(j))
                                End If
                            Next
                            MyData.AppendList(thisn.Name, thatlist)
                    End Select
                Next

            End If

            ByteLenghts = Nothing

            Return PacketLengthMinus1
        Catch ex As Exception
            Return Nothing
        End Try

    End Function

    Private Function ReadIndexPacket(ByRef str As FileStream, Position As Long) As UInt64

        Dim PacketHeader As Byte() = ReadAndSkip(str, Position, 16)
        Dim PacketType As Byte
        Dim PacketLengthMinus1 As UShort
        Dim EntryCount As UShort
        Dim IndexLevel As Byte

        ReadIndexPacketHeader(PacketHeader, PacketType, Nothing, PacketLengthMinus1, EntryCount, IndexLevel, Nothing)
        PacketHeader = Nothing

        For i As Int32 = 0 To EntryCount - 1 Step 1

            Dim AddressBytes() As Byte = ReadAndSkip(str, str.Position, 16)
            Dim ChunkRecordIndex As ULong
            Dim PacketOffset As ULong

            ReadPacketAddressEntries(AddressBytes, ChunkRecordIndex, PacketOffset)

            If IndexLevel = 0 Then
                ReadDataPacket(str, Convert.ToInt64(PacketOffset))
            ElseIf IndexLevel < 6 Then
                ReadIndexPacket(str, Convert.ToInt64(PacketOffset))
            End If

        Next

        Return PacketLengthMinus1
    End Function

    Private Function ReadIgnoredPacket(ByRef str As FileStream, Position As Long) As UInt64
        Dim b() As Byte = ReadAndSkip(str, Position, 4)
        Dim PacketType As Byte
        Dim PacketLength As UInt16

        ReadIgnoredPacketHeader(b, PacketType, Nothing, PacketLength)
        b = Nothing
        str.Position += (PacketLength + 1 - 4)

        Return PacketLength
    End Function

#End Region

End Class
