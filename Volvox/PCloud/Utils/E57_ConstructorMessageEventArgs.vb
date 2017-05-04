Public Class E57MessageEventArgs
    Inherits System.EventArgs


    Private MyMessageType As MessageType = MessageType.Empty
    Private MyMessage As String = String.Empty

    Sub New(Type As MessageType, Message As String)
        Me.MyMessageType = Type
        Me.MyMessage = Message
    End Sub

    Public ReadOnly Property Type As MessageType
        Get
            Return MyMessageType
        End Get
    End Property

    Public ReadOnly Property Message As String
        Get
            Return MyMessage
        End Get
    End Property

    Public Enum MessageType
        Empty = -1
        CurrentCloudReport = 0
        ErrorMessage = 1
        WarningMessage = 2
        SubPercentReport = 3
        CustomMessage = 4
    End Enum

End Class