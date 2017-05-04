Imports GH_IO.Serialization
Imports Grasshopper
Imports Grasshopper.Kernel.Types

Public Class GH_ScanSettings
    Inherits Grasshopper.Kernel.Types.GH_Goo(Of ScanSettings)

    Sub New()
        MyBase.New(New ScanSettings)
    End Sub

    Sub New(Settings As ScanSettings)
        MyBase.New(Settings)
    End Sub

    Public Overrides Function Read(reader As GH_IReader) As Boolean
        Dim n As New ScanSettings

        n.Rate = reader.GetInt32("rate")
        n.Resolution = reader.GetInt32("res")
        n.Compression = reader.GetInt32("noise")

        n.HorizontalAngleMin = reader.GetInt32("hmin")
        n.HorizontalAngleMax = reader.GetInt32("hmax")
        n.VerticalAngleMin = reader.GetInt32("vmin")
        n.VerticalAngleMax = reader.GetInt32("vmax")

        Return MyBase.Read(reader)
    End Function

    Public Overrides Function Write(writer As GH_IWriter) As Boolean
        writer.SetInt32("hmin", Me.Value.HorizontalAngleMin)
        writer.SetInt32("hmax", Me.Value.HorizontalAngleMax)
        writer.SetInt32("vmin", Me.Value.VerticalAngleMin)
        writer.SetInt32("vmax", Me.Value.VerticalAngleMax)

        writer.SetInt32("rate", Me.Value.Rate)
        writer.SetInt32("res", Me.Value.Resolution)
        writer.SetInt32("noise", Me.Value.Compression)

        Return MyBase.Write(writer)
    End Function

    Public Overrides ReadOnly Property IsValid As Boolean
        Get
            Return Me.Value.IsValid
        End Get
    End Property

    Public Overrides ReadOnly Property TypeDescription As String
        Get
            Return "FARO scan settings"
        End Get
    End Property

    Public Overrides ReadOnly Property TypeName As String
        Get
            Return "FARO scan settings"
        End Get
    End Property

    Public Overrides Function Duplicate() As IGH_Goo
        Return New GH_ScanSettings(Me.Value.Duplicate)
    End Function

    Public Overrides Function ToString() As String
        Return "FARO scan settings"
    End Function
End Class
