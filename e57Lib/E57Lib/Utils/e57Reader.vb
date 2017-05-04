Imports E57LibCommon

Public Class e57FileReader

#Region "Properties"

    Public ReadOnly Property HeaderString As String
        Get
            Return myHeaderString
        End Get
    End Property

    Public ReadOnly Property xmlString As String
        Get
            Return myXmlString
        End Get
    End Property

    Public ReadOnly Property FileSignature As String
        Get
            Return myFileSignature
        End Get
    End Property

    Public ReadOnly Property VersionMajor As UInt32
        Get
            Return myVersionMajor
        End Get
    End Property

    Public ReadOnly Property VersionMinor As UInt32
        Get
            Return myVersionMinor
        End Get
    End Property

    Public ReadOnly Property FileLength As UInt64
        Get
            Return myFileLength
        End Get
    End Property

    Public ReadOnly Property xmlOffset As UInt64
        Get
            Return myXmlOffset
        End Get
    End Property

    Public ReadOnly Property xmlLength As UInt64
        Get
            Return myXmlLength
        End Get
    End Property

    Public ReadOnly Property PageSize As UInt64
        Get
            Return myPageSize
        End Get
    End Property

    Public ReadOnly Property PageCount As UInt64
        Get
            Return myPageCount
        End Get
    End Property

#End Region

#Region "Fields"

    Private myHeaderString As String
    Private myXmlString As String

    Private FilePath As String

    Private myFileSignature As String
    Private myVersionMajor As UInt32
    Private myVersionMinor As UInt32
    Private myFileLength As UInt64
    Private myXmlOffset As UInt64
    Private myXmlLength As UInt64
    Private myPageSize As UInt64
    Private myPageCount As UInt64

#End Region

    Sub New(FilePath As String)
        Me.FilePath = FilePath
    End Sub

    Public Sub ReadFile()
        ReadHeader()
        ReadXml()
    End Sub

    Private Sub ReadHeader()
        myHeaderString = ReadFileHeader(FilePath, myFileSignature, myVersionMajor, myVersionMinor, myFileLength, myXmlOffset, myXmlLength, myPageSize)
        myPageCount = FileLength / PageSize
    End Sub

    Private Sub ReadXml()
        myXmlString = E57LibReader.ReadXmlPart(FilePath, myPageSize, myPageCount, myXmlOffset, myXmlLength)
    End Sub

End Class
