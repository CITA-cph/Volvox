Public Class TestScan

    Private myGuid As String = String.Empty
        Private myName As String = String.Empty
        Private myDesc As String = String.Empty
        Public Sub New(ByVal guid As String, ByVal name As String, ByVal desc As String)
            myGuid = guid
            myName = name
            myDesc = desc
        End Sub

        Public Property guid As String
            Get
                Return myGuid
            End Get
            Set(value As String)
                myGuid = value
            End Set
        End Property

        Public Property name As String
            Get
                Return myName
            End Get
            Set(value As String)
                myName = value
            End Set
        End Property

        Public Property desc As String
            Get
                Return myDesc
            End Get
            Set(value As String)
                myDesc = value
            End Set
        End Property
    End Class

