Public Structure Multipoint

    Public Property X As Double
    Public Property Y As Double
    Public Property Z As Double
    Public Property R As Integer
    Public Property G As Integer
    Public Property B As Integer
    Public Property U As Double
    Public Property V As Double
    Public Property W As Double
    Public Property A As Integer
    Public Property ContainsColors As Boolean
    Public Property ContainsNormals As Boolean
    Public Property ContainsIntensity As Boolean


    Sub New(x As Double, y As Double, z As Double, r As Integer, g As Integer, b As Integer, u As Double, v As Double, w As Double, a As Integer)
        Me.X = x
        Me.Y = y
        Me.Z = z

        Me.R = Math.Max(0, Math.Min(255, r))
        Me.G = Math.Max(0, Math.Min(255, g))
        Me.B = Math.Max(0, Math.Min(255, b))

        Me.U = u
        Me.V = v
        Me.W = w

        Me.A = Math.Max(0, Math.Min(255, a))

        ContainsColors = False
        ContainsNormals = False
        ContainsIntensity = False
    End Sub

    Overrides Function ToString() As String
        Dim nstr As New String("")
        nstr &= X & " " & Y & " " & Z & " " & R & " " & G & " " & B & " " & U & " " & V & " " & W & " " & A
        Return nstr 
    End Function

End Structure
