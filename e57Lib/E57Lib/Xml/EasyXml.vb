Imports System.Xml

Friend Structure BetterXml

    Private mypref As String
    Private mydoc As XmlDocument
    Private myns As XmlNamespaceManager

    Friend Sub New(doc As XmlDocument, namespacestring As String)
        mydoc = doc
        myns = New XmlNamespaceManager(mydoc.NameTable)
        mypref = "BetterXml"
        myns.AddNamespace(mypref, namespacestring)
    End Sub

    Friend Function SelectNodes(path As String) As XmlNodeList
        Return mydoc.SelectNodes(Normalize(path), myns)
    End Function

    Friend Function SelectNode(path As String) As XmlNode
        Return mydoc.SelectSingleNode(Normalize(path), myns)
    End Function

    Private Function Normalize(path As String) As String
        path = path.Replace("root", "/e57Root")
        Return path.Replace("/", "/" & mypref & ":")
    End Function

End Structure

Friend Module XmlMaps

    Friend Function MapType(xn As XmlNode) As e57Type

        If xn.Attributes("type") IsNot Nothing Then
            Select Case xn.Attributes("type").Value
                Case "Integer"
                    Return e57Type.e57Integer
                Case "ScaledInteger"
                    Return e57Type.e57ScaledInteger
                Case "Float"
                    Return e57Type.e57Float
                Case "String"
                    Return e57Type.e57String
                Case "Vector"
                    Return e57Type.e57Vector
                Case "CompressedVector"
                    Return e57Type.e57CompressedVector
                Case "Structure"
                    Return e57Type.e57Structure
            End Select
        End If

        Return e57Type.e57Invalid
    End Function

    Friend Function GetDoubleAttribute(xn As XmlNode, name As String, ByRef value As Double) As Boolean
        If xn.Attributes(name) IsNot Nothing Then
            Double.TryParse(xn.Attributes(name).Value, value)
            Return True
        End If
        Return False
    End Function

    Friend Function GetIntegerAttribute(xn As XmlNode, name As String, ByRef value As Int64) As Boolean
        If xn.Attributes(name) IsNot Nothing Then
            Int64.TryParse(xn.Attributes(name).Value, value)
            Return True
        End If
        Return False
    End Function

    Friend Function GetUIntegerAttribute(xn As XmlNode, name As String, ByRef value As UInt64) As Boolean
        If xn.Attributes(name) IsNot Nothing Then
            UInt64.TryParse(xn.Attributes(name).Value, value)
            Return True
        End If
        Return False
    End Function

    Friend Function GetStringAttribute(xn As XmlNode, name As String, ByRef text As String) As Boolean
        If xn.Attributes(name) IsNot Nothing Then
            text = xn.Attributes(name).Value
            Return True
        End If
        Return False
    End Function

End Module
