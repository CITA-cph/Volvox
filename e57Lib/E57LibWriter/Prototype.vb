Public Class Prototype

    Private Elements As New List(Of PrototypeElement)

    Sub New()

    End Sub

    Default Property Item(index As Integer) As PrototypeElement
        Get
            Return Elements(index)
        End Get
        Set(value As PrototypeElement)
            Elements(index) = value
        End Set
    End Property

    Public Sub Add(PrototypeElement As PrototypeElement)
        Elements.Add(PrototypeElement)
    End Sub

    Public Sub RemoveAt(index As Integer)
        Elements.RemoveAt(index)
    End Sub

    Public Sub Clear()
        Elements.Clear()
    End Sub

    Public ReadOnly Property GetElement(ElementName As String) As PrototypeElement
        Get
            For i As Integer = 0 To Elements.Count - 1 Step 1
                If Elements(i).ElementName = ElementName Then Return Elements(i)
            Next
            Return Nothing
        End Get
    End Property

    Public Function Count() As Integer
        Return Elements.Count
    End Function

End Class

Public MustInherit Class PrototypeElement

    Private Name As String = String.Empty

    Sub New(ElementName As String)
        Name = ElementName
    End Sub

    Public MustOverride ReadOnly Property ElementType As ElementType

    Public ReadOnly Property ElementName As String
        Get
            Return Name
        End Get
    End Property

End Class

Public Class IntegerElement
    Inherits PrototypeElement

    Private MyMini As Int64
    Private MyMaxi As Int64
    Private MyVal As Int64

    Public ReadOnly Property Minimum As Int64
        Get
            Return MyMini
        End Get
    End Property

    Public ReadOnly Property Maximum As Int64
        Get
            Return MyMaxi
        End Get
    End Property

    Public ReadOnly Property Value As Int64
        Get
            Return MyVal
        End Get
    End Property

    Public Overrides ReadOnly Property ElementType As ElementType
        Get
            Return ElementType.Integer
        End Get
    End Property

    Sub New(ElementName As String, Optional Minimum As Int64 = Int64.MinValue, Optional Maximum As Int64 = Int64.MaxValue, Optional Value As Int64 = 0)
        MyBase.New(ElementName)
        MyMini = Minimum
        MyMaxi = Maximum
        MyVal = Value
    End Sub

End Class

Public Class ScaledIntegerElement
    Inherits PrototypeElement

    Private MyMini As Int64
    Private MyMaxi As Int64
    Private MySca As Int64
    Private MyOff As Int64

    Public ReadOnly Property Minimum As Int64
        Get
            Return MyMini
        End Get
    End Property

    Public ReadOnly Property Maximum As Int64
        Get
            Return MyMaxi
        End Get
    End Property

    Public ReadOnly Property Scale As Double
        Get
            Return MySca
        End Get
    End Property

    Public ReadOnly Property Offset As Double
        Get
            Return MyOff
        End Get
    End Property

    Public Overrides ReadOnly Property ElementType As ElementType
        Get
            Return ElementType.ScaledInteger
        End Get
    End Property

    Sub New(ElementName As String, Optional Minimum As Int64 = Int64.MinValue, Optional Maximum As Int64 = Int64.MaxValue, Optional Scale As Double = 1, Optional Offset As Double = 0)
        MyBase.New(ElementName)
        MyMini = Minimum
        MyMaxi = Maximum
    End Sub

End Class

Public Class DoubleElement
    Inherits PrototypeElement

    Private MyMini As Double
    Private MyMaxi As Double

    Public ReadOnly Property Minimum As Double
        Get
            Return MyMini
        End Get
    End Property

    Public ReadOnly Property Maximum As Double
        Get
            Return MyMaxi
        End Get
    End Property

    Public Overrides ReadOnly Property ElementType As ElementType
        Get
            Return ElementType.Double
        End Get
    End Property

    Sub New(ElementName As String, Optional Minimum As Double = Double.MinValue, Optional Maximum As Double = Double.MaxValue)
        MyBase.New(ElementName)
        MyMini = Minimum
        MyMaxi = Maximum
    End Sub
End Class

Public Class SingleElement
    Inherits PrototypeElement

    Private MyMini As Double
    Private MyMaxi As Double

    Public ReadOnly Property Minimum As Double
        Get
            Return MyMini
        End Get
    End Property

    Public ReadOnly Property Maximum As Double
        Get
            Return MyMaxi
        End Get
    End Property

    Public Overrides ReadOnly Property ElementType As ElementType
        Get
            Return ElementType.Double
        End Get
    End Property

    Sub New(ElementName As String, Optional Minimum As Double = Single.MinValue, Optional Maximum As Double = Single.MaxValue)
        MyBase.New(ElementName)
        MyMini = Minimum
        MyMaxi = Maximum
    End Sub
End Class