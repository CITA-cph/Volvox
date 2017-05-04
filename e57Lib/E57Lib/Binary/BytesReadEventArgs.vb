Public Class BytesReadEventArgs

    Inherits System.EventArgs

    Private btsCount As UInt64
    Private totLength As UInt64

    Sub New(Bytes As UInt64, Length As UInt64)
        btsCount = Bytes
        totLength = Length
    End Sub

    Public Property BytesRead As UInt64
        Get
            Return btsCount
        End Get
        Set(value As UInt64)
            btsCount = value
        End Set
    End Property

    Public Property TotalLength As UInt64
        Get
            Return totLength
        End Get
        Set(value As UInt64)
            totLength = value
        End Set
    End Property

    Public ReadOnly Property Percent As Integer
        Get
            Return Math.Round(btsCount / totLength, 2) * 100
        End Get
    End Property

End Class
