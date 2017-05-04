
Public Class Asmbl_Description
    Inherits Grasshopper.Kernel.GH_AssemblyInfo
    Public Overrides ReadOnly Property AuthorContact As String
        Get
            Return "Henrik Leander Evers heve@kadk.dk" & vbCrLf & "Mateusz Zwierzycki mzwi@kadk.dk"
        End Get
    End Property
    Public Overrides ReadOnly Property Description As String
        Get
            Dim str As String = "Volvox - Point Cloud editing plugin."

            str += vbCrLf
            str += "The plugin is developed in the frame of the DURAARK project: http://duraark.eu/"
            str += vbCrLf
            str += vbCrLf
            str += "DURAARK (Durable Architectural Knowledge) is a collaborative project "
            str += "developing methods and tools for the semantic enrichment and long-term "
            str += "preservation of architectural knowledge and data. It focuses on establishing working "
            str += "practices and links between semantically rich BIM models and unstructured Point Cloud data. It is funded through the "
            str += "European Commision's FP7 Programme and is running between 02/2013 - 01/2016."
            str += " (Grant Agreement no: 600908)"

            Return str
        End Get
    End Property
    Public Overrides ReadOnly Property AuthorName As String
        Get
            Return "CITA"
        End Get
    End Property

    Public Overrides ReadOnly Property Icon As Drawing.Bitmap
        Get
            Return My.Resources.Icon_Volvox
        End Get
    End Property

    Public Overrides ReadOnly Property Id As Guid
        Get
            Return New Guid("{17910781-A7F8-4985-8D04-D7CB544C66FE}")
        End Get
    End Property
    Public Overrides ReadOnly Property Name As String
        Get
            Return "Volvox"
        End Get
    End Property
    Public Overrides ReadOnly Property Version As String
        Get
            Return "0.3.0.0"
        End Get
    End Property
End Class
