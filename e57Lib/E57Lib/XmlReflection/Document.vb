Imports System.Xml
Imports E57LibReader
Imports System.IO


Public Class e57Document

        Inherits e57Structure

        Private formName As e57String = Nothing
        Private e57Guid As e57String = Nothing
        Private vMajor As e57Integer = Nothing
        Private vMinor As e57Integer = Nothing
        Private e57LibVersion As e57String = Nothing
        Private coordMeta As e57String = Nothing
        Private vectors As New List(Of Scan)
        Private decodedScans As Integer = 0
        Private myXml As String = String.Empty
    Private valid As Boolean = True
    Private mydate As e57Float = Nothing
    Private mydateBool As e57Integer = Nothing


    Friend ReadOnly Property DocumentFilePath As String = String.Empty

        Public ReadOnly Property XmlText As String
            Get
                Return myXml
            End Get
        End Property

        Public ReadOnly Property IsValid As Boolean
            Get
                Return valid
            End Get
        End Property


        Sub New(FilePath As String, DecodeNow As Boolean)
            MyBase.New()
            DocumentFilePath = FilePath

        Dim reader As New e57FileReader(DocumentFilePath)
        reader.ReadFile()

            Dim xmlDoc As New XmlDocument
            xmlDoc.LoadXml(reader.xmlString)
            myXml = reader.xmlString

            'Dim xmlNice As New BetterXml(xmlDoc, "http://www.astm.org/COMMIT/e57/2010-e57-v1.0")
            'Me.Reinstantiate(xmlNice.SelectNode("/e57Root"), Nothing)

            'formName = New e57String(xmlNice.SelectNode("/e57Root/formatName"), Me)
            'e57Guid = New e57String(xmlNice.SelectNode("/e57Root/guid"), Me)
            'vMajor = New e57Integer(xmlNice.SelectNode("/e57Root/versionMajor"), Me)
            'vMinor = New e57Integer(xmlNice.SelectNode("/e57Root/versionMinor"), Me)
            'e57LibVersion = New e57String(xmlNice.SelectNode("/e57Root/e57LibraryVersion"), Me)
            'coordMeta = New e57String(xmlNice.SelectNode("/e57Root/coordinateMetadata"), Me)

            'Dim tempVec As New List(Of e57Node)
            'Me.GetChildren("/e57Root/data3D/vectorChild", tempVec)

            Dim xmlNice As New BetterXml(xmlDoc, "http://www.astm.org/COMMIT/E57/2010-e57-v1.0")
            Me.Reinstantiate(xmlNice.SelectNode("root"), Nothing)

            formName = New e57String(xmlNice.SelectNode("root/formatName"), Me)
        e57Guid = New e57String(xmlNice.SelectNode("root/guid"), Me)

        vMajor = New e57Integer(xmlNice.SelectNode("root/versionMajor"), Me)
        vMinor = New e57Integer(xmlNice.SelectNode("root/versionMinor"), Me)

        If xmlNice.SelectNode("root/e57LibraryVersion") IsNot Nothing Then
            e57LibVersion = New e57String(xmlNice.SelectNode("root/e57LibraryVersion"), Me)
        End If

        If xmlNice.SelectNode("root/coordinateMetadata") IsNot Nothing Then
            coordMeta = New e57String(xmlNice.SelectNode("root/coordinateMetadata"), Me)
        End If

        If xmlNice.SelectNode("root/creationDateTime") IsNot Nothing Then
            If xmlNice.SelectNode("root/creationDateTime/dateTimeValue") IsNot Nothing Then
                mydate = New e57Float(xmlNice.SelectNode("root/creationDateTime/dateTimeValue"), Me)
            End If
            If xmlNice.SelectNode("root/creationDateTime/isAtomicClockReferenced") IsNot Nothing Then
                mydateBool = New e57Integer(xmlNice.SelectNode("root/creationDateTime/isAtomicClockReferenced"), Me)
            End If
        End If


        Dim tempVec As New List(Of e57Node)
            Me.GetChildren("e57Root/data3D/vectorChild", tempVec)

            For Each v As e57Structure In tempVec
                vectors.Add(New Scan(v, Me))
            Next

            If DecodeNow Then
                DecodeScans()
            End If

        End Sub

        Public Function CheckFile() As Boolean

            Using str As FileStream = New FileStream(DocumentFilePath, FileMode.Open, FileAccess.Read)
                While str.Position <= (str.Length - 1024) + 1
                    Dim bytes(1019) As Byte
                    str.Read(bytes, 0, 1020)

                    Dim checks(3) As Byte
                    str.Read(checks, 0, 4)

                    Dim checksum As UInt32 = E57LibCommon.Binary.GenerateChecksum(bytes)
                    Dim readcheck As UInt32 = E57LibCommon.Binary.ChecksumFromBytes(checks)
                    If readcheck <> checksum Then
                        valid = False
                        Return False
                    End If
                End While
            End Using

            Return True

        End Function

        Public ReadOnly Property FormatName As e57String
            Get
                Return formName
            End Get
        End Property

        Public ReadOnly Property Guid As e57String
            Get
                Return e57Guid
            End Get
        End Property

        Public ReadOnly Property VersionMajor As e57Integer
            Get
                Return vMajor
            End Get
        End Property

        Public ReadOnly Property VersionMinor As e57Integer
            Get
                Return vMinor
            End Get
        End Property

        Public ReadOnly Property e57LibraryVersion As e57String
            Get
                Return e57LibVersion
            End Get
        End Property

        Public ReadOnly Property CoordinateMetaData As e57String
            Get
                Return coordMeta
            End Get
        End Property

        Public ReadOnly Property ScanCount As Integer
            Get
                Return vectors.Count
            End Get
        End Property

    Public ReadOnly Property Scans As List(Of Scan)
        Get
            Return vectors
        End Get
    End Property

    Public ReadOnly Property [Date] As e57Float
        Get
            Return mydate
        End Get
    End Property

    Public ReadOnly Property IsAtomicClockReferenced As e57Integer
        Get
            Return mydateBool
        End Get
    End Property

    Public Function DecodeScans() As Integer
            Dim count As Integer = 0
            For i As Integer = 0 To vectors.Count - 1 Step 1
                DecodeScan(i)
                count += 1
            Next
            Return count
        End Function

        Public Sub DecodeScan(Index As Integer)
            vectors(Index).ReadScanData()
        End Sub

    End Class

