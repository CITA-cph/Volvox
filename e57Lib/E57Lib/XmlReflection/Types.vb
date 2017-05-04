Imports System.Xml
Imports e57Lib.EasyXml
Imports e57Lib.Types.Other
Imports e57Lib.Types.Collections

Namespace Types

    Public MustInherit Class e57Node

        Sub New()

        End Sub

        Public MustOverride Function Name() As String
        Public MustOverride Function Type() As e57Type
        Public MustOverride Function IsHomogeneous(Other As e57Node) As Boolean
        Public MustOverride Function Parent() As e57Node
        Public MustOverride Function ToSuperString(ThisLevel As Integer) As String
        Public MustOverride Function IsCollection() As Boolean

        ''' <summary>
        ''' </summary>
        ''' <param name="Path"> Has to follow "../child1/child2/child3" scheme</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function GetChildren(Path As e57Path, ByRef Kids As List(Of e57Node)) As Result

            If Path.IsThatMe(Me) Then
                Kids.Add(Me)
                Return Result.Found
            Else
                If Me.IsCollection Then

                    Dim AtLeastOnce As Result = Result.NotFound

                    Select Case Me.Type
                        Case e57Type.e57Structure
                            Dim meas As e57Structure = Me

                            For i As Integer = 0 To meas.Count - 1 Step 1
                                If meas.GetChild(i).GetChildren(Path.Increment, Kids) = Result.Found Then AtLeastOnce = Result.Found
                            Next

                        Case e57Type.e57Vector
                            Dim meas As e57Vector = Me

                            For i As Integer = 0 To meas.Count - 1 Step 1
                                If meas.GetChild(i).GetChildren(Path.Increment, Kids) = Result.Found Then AtLeastOnce = Result.Found
                            Next
                        Case e57Type.e57ScaledInteger
                            Dim meas As e57CompressedVector = Me

                            If meas.Prototype.GetChildren(Path.Increment, Kids) = Result.Found Then AtLeastOnce = Result.Found
                            If meas.Codecs.GetChildren(Path.Increment, Kids) = Result.Found Then AtLeastOnce = Result.Found

                    End Select

                    Return AtLeastOnce
                End If
            End If

            Return Result.NotFound

        End Function

        Public Function GetChildren(Path As String, ByRef Kids As List(Of e57Node)) As Result
            Return GetChildren(New e57Path(Path), Kids)
        End Function

    End Class

    Public Class e57Float
        Inherits e57Node

#Region "inherited"
        Private nam As String = String.Empty
        Private Par As e57Node = Nothing

        Public Overrides Function Type() As e57Type
            Return e57Type.e57Float
        End Function

        Public Overrides Function Name() As String
            Return nam
        End Function

        Public Overrides Function IsHomogeneous(Other As e57Node) As Boolean
            If Other.Type <> Me.Type Then Return False

            Dim AsFloat As e57Float = DirectCast(Other, e57Float)
            If AsFloat.HasMinimum <> Me.HasMinimum Then Return False
            If AsFloat.HasMaximum <> Me.HasMaximum Then Return False
            If AsFloat.HasPrecision <> Me.HasPrecision Then Return False
            If AsFloat.Name <> Me.Name Then Return False

            Return True
        End Function

        Public Overrides Function Parent() As e57Node
            Return Par
        End Function

        Public Overrides Function IsCollection() As Boolean
            Return False
        End Function

        Public Overrides Function ToSuperString(ThisLevel As Integer) As String

            Dim out As String = String.Empty
            For i As Integer = 0 To ThisLevel - 1 Step 1
                out &= "___"
            Next
            out &= Me.Type.ToString & ":" & Me.Name & ":" & Me.Value & vbCrLf
            Return out

        End Function

#End Region

        'over 
        Private Prec As Boolean = False
        Private Min As Double
        Private Max As Double
        Private Val As Double
        Private HasPrec As Boolean = False
        Private HasMin As Boolean = False
        Private HasMax As Boolean = False

        Sub New(xn As XmlNode, Parent As e57Node)
            MyBase.New()

            If MapType(xn) = e57Type.e57Float Then
                Par = Parent
                nam = xn.Name

                HasMin = GetDoubleAttribute(xn, "minimum", Min)
                HasMax = GetDoubleAttribute(xn, "maximum", Max)

                Dim tempPrec As String = String.Empty
                HasPrec = GetStringAttribute(xn, "precision", tempPrec)
                If tempPrec = "single" Then
                    Prec = False
                Else
                    Prec = True
                End If

                Double.TryParse(xn.InnerText, Val)
            End If

        End Sub

        Public ReadOnly Property DoublePrecision As Boolean
            Get
                Return Prec
            End Get
        End Property

        Public ReadOnly Property Minimum As Double
            Get
                Return Min
            End Get
        End Property

        Public ReadOnly Property Maximum As Double
            Get
                Return Max
            End Get
        End Property

        Public ReadOnly Property Value As Double
            Get
                Return Val
            End Get
        End Property

        Public ReadOnly Property HasMinimum As Boolean
            Get
                Return HasMin
            End Get
        End Property

        Public ReadOnly Property HasMaximum As Boolean
            Get
                Return HasMax
            End Get
        End Property

        Public ReadOnly Property HasPrecision As Boolean
            Get
                Return HasPrec
            End Get
        End Property

    End Class

    Public Class e57Integer
        Inherits e57Node

#Region "inherited"
        Private nam As String = String.Empty
        Private Par As e57Node = Nothing

        Public Overrides Function Type() As e57Type
            Return e57Type.e57Integer
        End Function

        Public Overrides Function Name() As String
            Return nam
        End Function

        Public Overrides Function IsHomogeneous(Other As e57Node) As Boolean
            If Other.Type <> Me.Type Then Return False

            Dim AsInteger As e57Integer = DirectCast(Other, e57Integer)
            If AsInteger.HasMin <> Me.HasMin Then Return False
            If AsInteger.HasMax <> Me.HasMax Then Return False
            If AsInteger.Name <> Me.Name Then Return False

            Return True
        End Function

        Public Overrides Function IsCollection() As Boolean
            Return False
        End Function

        Public Overrides Function Parent() As e57Node
            Return Par
        End Function

        Public Overrides Function ToSuperString(ThisLevel As Integer) As String

            Dim out As String = String.Empty
            For i As Integer = 0 To ThisLevel - 1 Step 1
                out &= "___"
            Next
            out &= Me.Type.ToString & ":" & Me.Name & ":" & Me.Value & vbCrLf
            Return out

        End Function

#End Region

        'over
        Private Val As Int64
        Private Min As Int64
        Private Max As Int64
        Private HasMin As Boolean = False
        Private HasMax As Boolean = False

        Sub New(xn As XmlNode, Parent As e57Node)
            MyBase.New()
            If xn.Attributes("type") IsNot Nothing Then
                If xn.Attributes("type").Value = "Integer" Then
                    Par = Parent
                    nam = xn.Name
                    HasMin = GetIntegerAttribute(xn, "minimum", Min)
                    HasMax = GetIntegerAttribute(xn, "maximum", Max)
                    Int64.TryParse(xn.InnerText, Val)
                End If
            End If
        End Sub

        Public ReadOnly Property Minimum As Int64
            Get
                Return Min
            End Get
        End Property

        Public ReadOnly Property Maximum As Int64
            Get
                Return Max
            End Get
        End Property

        Public ReadOnly Property Value As Int64
            Get
                Return Val
            End Get
        End Property

        Public ReadOnly Property HasMinimum As Boolean
            Get
                Return HasMin
            End Get
        End Property

        Public ReadOnly Property HasMaximum As Boolean
            Get
                Return HasMax
            End Get
        End Property

    End Class

    Public Class e57ScaledInteger
        Inherits e57Node

        'inherited
        Private nam As String = String.Empty
        Private Par As e57Node = Nothing

        Public Overrides Function Type() As e57Type
            Return e57Type.e57ScaledInteger
        End Function

        Public Overrides Function Name() As String
            Return nam
        End Function

        Public Overrides Function IsHomogeneous(Other As e57Node) As Boolean
            If Other.Type <> Me.Type Then Return False

            Dim AsScaled As e57ScaledInteger = DirectCast(Other, e57ScaledInteger)
            If AsScaled.HasMinimum <> Me.HasMinimum Then Return False
            If AsScaled.HasMaximum <> Me.HasMaximum Then Return False
            If AsScaled.HasOffset <> Me.HasOffset Then Return False
            If AsScaled.HasScale <> Me.HasScale Then Return False
            If AsScaled.Name <> Me.Name Then Return False

            Return True
        End Function

        Public Overrides Function Parent() As e57Node
            Return Par
        End Function

        Public Overrides Function IsCollection() As Boolean
            Return False
        End Function

        Public Overrides Function ToSuperString(ThisLevel As Integer) As String

            Dim out As String = String.Empty
            For i As Integer = 0 To ThisLevel - 1 Step 1
                out &= "___"
            Next
            out &= Me.Type.ToString & ":" & Me.Name & ":" & Me.Value & vbCrLf
            Return out

        End Function

        'over 
        Private Val As Int64 = 0
        Private Min As Int64 = Int64.MinValue
        Private HasMin As Boolean = False
        Private Max As Int64 = Int64.MaxValue
        Private HasMax As Boolean = False
        Private Sca As Double = 1
        Private HasSca As Boolean = False
        Private Off As Double = 0
        Private HasOff As Boolean = False

        Sub New(xn As XmlNode, Parent As e57Node)
            MyBase.New()
            If xn.Attributes("type") IsNot Nothing Then
                If xn.Attributes("type").Value = "ScaledInteger" Then
                    Par = Parent

                    nam = xn.Name
                    HasMin = GetIntegerAttribute(xn, "minimum", Min)
                    HasMax = GetIntegerAttribute(xn, "maximum", Max)
                    HasSca = GetDoubleAttribute(xn, "scale", Min)
                    HasOff = GetDoubleAttribute(xn, "offset", Off)

                    Int64.TryParse(xn.InnerText, Val)
                End If
            End If
        End Sub

        Public ReadOnly Property Scale As Double
            Get
                Return Sca
            End Get
        End Property

        Public ReadOnly Property Offset As Double
            Get
                Return Off
            End Get
        End Property

        Public ReadOnly Property Minimum As Int64
            Get
                Return Min
            End Get
        End Property

        Public ReadOnly Property Maximum As Int64
            Get
                Return Max
            End Get
        End Property

        Public ReadOnly Property Value As Int64
            Get
                Return Val
            End Get
        End Property

        Public ReadOnly Property HasMinimum As Boolean
            Get
                Return HasMin
            End Get
        End Property

        Public ReadOnly Property HasMaximum As Boolean
            Get
                Return HasMax
            End Get
        End Property

        Public ReadOnly Property HasScale As Boolean
            Get
                Return HasSca
            End Get
        End Property

        Public ReadOnly Property HasOffset As Boolean
            Get
                Return HasOff
            End Get
        End Property

    End Class

    Public Class e57String
        Inherits e57Node

        'inherited
        Private nam As String = String.Empty
        Private Par As e57Node = Nothing

        Public Overrides Function Type() As e57Type
            Return Types.e57Type.e57String
        End Function

        Public Overrides Function Name() As String
            Return nam
        End Function

        Public Overrides Function IsHomogeneous(Other As e57Node) As Boolean
            If Other.Type <> Me.Type Then Return False
            If Other.Name <> Me.Name Then Return False
            Return True
        End Function

        Public Overrides Function Parent() As e57Node
            Return Par
        End Function

        Public Overrides Function IsCollection() As Boolean
            Return False
        End Function

        Public Overrides Function ToSuperString(ThisLevel As Integer) As String

            Dim out As String = String.Empty
            For i As Integer = 0 To ThisLevel - 1 Step 1
                out &= "___"
            Next
            out &= Me.Type.ToString & ":" & Me.Name & ":" & Me.Value & vbCrLf
            Return out

        End Function

        'over
        Private str As String = String.Empty

        Sub New(xn As XmlNode, Parent As e57Node)
            MyBase.New()

            If xn.Attributes("type") IsNot Nothing Then
                If xn.Attributes("type").Value = "String" Then
                    Par = Parent
                    nam = xn.Name
                    str = xn.InnerText
                End If
            End If

        End Sub

        Public ReadOnly Property Value As String
            Get
                Return str
            End Get
        End Property

    End Class

    Public Enum e57Type
        e57Invalid = -1
        e57Integer = 0
        e57ScaledInteger = 1
        e57Float = 2
        e57String = 3
        e57Vector = 4
        e57CompressedVector = 5
        e57Structure = 6
    End Enum

    Public Enum Result
        NotFound = -1
        Terminal = 0
        Found = 1
    End Enum

End Namespace
