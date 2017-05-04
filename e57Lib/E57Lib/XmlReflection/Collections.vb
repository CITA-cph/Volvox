Imports E57LibReader
Imports System.Xml


Public Class e57Vector
            Inherits e57Node


#Region "inherited"
            Private nam As String = String.Empty
            Private Par As e57Node = Nothing

            Public Overrides Function Type() As e57Type
                Return e57Type.e57Vector
            End Function

            Public Overrides Function Name() As String
                Return nam
            End Function

            Public Overrides Function IsHomogeneous(Other As e57Node) As Boolean
                If Other.Type <> Me.Type Then Return False

                Dim AsVector As e57Vector = DirectCast(Other, e57Vector)
                If AsVector.Count <> Me.Count Then Return False
                If AsVector.AllowHeterogeneousChildren <> Me.AllowHeterogeneousChildren Then Return False

                For i As Integer = 0 To AsVector.Count - 1 Step 1
                    Dim OtherChild As e57Node = AsVector.GetChild(i)
                    Dim MyChild As e57Node = Me.GetChild(i)
                    If Not OtherChild.IsHomogeneous(MyChild) Then Return False
                Next

                Return True
            End Function

            Public Overrides Function Parent() As e57Node
                Return Par
            End Function

            Public Overrides Function IsCollection() As Boolean
                Return True
            End Function

            Public Overrides Function ToSuperString(Optional ThisLevel As Integer = 0) As String

                Dim out As String = String.Empty
                For i As Integer = 0 To ThisLevel - 1 Step 1
                    out &= "___"
                Next
                out &= Me.Type.ToString & ":" & Me.Name & vbCrLf

                ThisLevel += 1

                For i As Integer = 0 To Me.ChildList.Count - 1 Step 1
                    out &= Me.ChildList(i).ToSuperString(ThisLevel)
                Next

                Return out

            End Function
#End Region

            'over
            Private ChildList As New List(Of e57Node)
            Private AllowHeterogeneous As Boolean = False
            Private HasHeterogeneous As Boolean = False

            Sub New(xn As XmlNode, Parent As e57Node)
                Par = Parent
                nam = xn.Name

                If xn.Attributes("allowHeterogeneousChildren") IsNot Nothing Then
                    HasHeterogeneous = True
                    Dim temphet As Integer = 0
                    Integer.TryParse(xn.Attributes("allowHeterogeneousChildren").Value, temphet)
                    Select Case temphet
                        Case 0
                            AllowHeterogeneous = False
                        Case 1
                            AllowHeterogeneous = True
                    End Select
                End If

                For Each c As XmlNode In xn.ChildNodes
                    Select Case MapType(c)
                        Case e57Type.e57Float
                            AddChildren(New e57Float(c, Me))
                        Case e57Type.e57Integer
                            AddChildren(New e57Integer(c, Me))
                        Case e57Type.e57ScaledInteger
                            AddChildren(New e57ScaledInteger(c, Me))
                        Case e57Type.e57String
                            AddChildren(New e57String(c, Me))
                        Case e57Type.e57Vector
                            AddChildren(New e57Vector(c, Me))
                        Case e57Type.e57CompressedVector
                            AddChildren(New e57CompressedVector(c, Me))
                        Case e57Type.e57Structure
                            AddChildren(New e57Structure(c, Me))
                    End Select
                Next

            End Sub

            Public ReadOnly Property AllowHeterogeneousChildren As Boolean
                Get
                    Return AllowHeterogeneous
                End Get
            End Property

            Public ReadOnly Property HasHeterogeneousSet As Boolean
                Get
                    Return HasHeterogeneous
                End Get
            End Property

            Public ReadOnly Property GetChild(Index As Integer) As e57Node
                Get
                    Return ChildList(Index)
                End Get
            End Property

            Public ReadOnly Property Count() As Integer
                Get
                    Return ChildList.Count
                End Get
            End Property

            Public Function AddChildren(Children As e57Node) As Boolean

                If CanAddThis(Children) Then
                    ChildList.Add(Children)
                    Return True
                End If

                Return False
            End Function

            Public Function CanAddThis(Children As e57Node) As Boolean

                If AllowHeterogeneousChildren Then
                    Return True
                Else
                    If Me.ChildList.Count = 0 Then Return True
                    Dim MyFirst As e57Node = Me.ChildList(0)
                    If MyFirst.IsHomogeneous(Children) Then Return True
                End If

                Return False
            End Function

        End Class

        Public Class e57CompressedVector
            Inherits e57Node

            'inherited
            Dim nam As String = String.Empty
            Private Par As e57Node = Nothing

            Public Overrides Function Type() As e57Type
                Return e57Type.e57CompressedVector
            End Function

            Public Overrides Function Name() As String
                Return nam
            End Function

            Public Overrides Function IsHomogeneous(Other As e57Node) As Boolean
                If Other.Type <> Me.Type Then Return False

                Dim AsCompressed As e57CompressedVector = DirectCast(Other, e57CompressedVector)

                Return False
            End Function

            Public Overrides Function Parent() As e57Node
                Return Par
            End Function

            Public Overrides Function IsCollection() As Boolean
                Return True
            End Function

            Public Overrides Function ToSuperString(Optional ThisLevel As Integer = 0) As String

                Dim out As String = String.Empty
                For i As Integer = 0 To ThisLevel - 1 Step 1
                    out &= "___"
                Next
                out &= Me.Type.ToString & ":" & Me.Name & vbCrLf

                If MyPrototype IsNot Nothing Then
                    out &= MyPrototype.ToSuperString(ThisLevel + 1)
                End If

                If MyCodecs IsNot Nothing Then
                    out &= MyCodecs.ToSuperString(ThisLevel + 1)
                End If

                Return out

            End Function

            'over
            'attributes
            Private FileOff As Long = 0 'file offset
            Private RecCount As uint64 = 0 'record count

            'child elements
            Private MyPrototype As e57Node = Nothing
            Private MyCodecs As e57Vector = Nothing

            Sub New(xn As XmlNode, Parent As e57Node)

                nam = xn.Name
                Par = Parent

                GetIntegerAttribute(xn, "fileOffset", FileOff)
                GetUIntegerAttribute(xn, "recordCount", RecCount)

                For Each c As XmlNode In xn.ChildNodes
                    Select Case c.Name
                        Case "prototype"
                            Dim ptype As New String("")
                            If GetStringAttribute(c, "type", ptype) Then
                                Select Case ptype
                                    Case "Structure"
                                        MyPrototype = New e57Structure(c, Me)
                                    Case "Vector"
                                        MyPrototype = New e57Vector(c, Me)
                                    Case "Integer"
                                        MyPrototype = New e57Integer(c, Me)
                                    Case "ScaledInteger"
                                        MyPrototype = New e57ScaledInteger(c, Me)
                                    Case "String"
                                        MyPrototype = New e57String(c, Me)
                                    Case "Float"
                                        MyPrototype = New e57Float(c, Me)
                                End Select
                            End If
                        Case "codecs"
                            MyCodecs = New e57Vector(c, Me)
                    End Select
                Next
            End Sub

            Public ReadOnly Property Prototype As e57Node
                Get
                    Return MyPrototype
                End Get
            End Property

            Public ReadOnly Property Codecs As e57Vector
                Get
                    Return MyCodecs
                End Get
            End Property

            Public ReadOnly Property FileOffset As Int64
                Get
                    Return FileOff
                End Get
            End Property

            Public ReadOnly Property RecordCount As uint64
                Get
                    Return RecCount
                End Get
            End Property

            Public ReadOnly Property HasCodec As Boolean
                Get
                    If MyCodecs IsNot Nothing Then Return True
                    Return False
                End Get
            End Property

        End Class

        Public Class e57Structure
            Inherits e57Node

#Region "inherited"
            Private nam As String = String.Empty
            Private Par As e57Node = Nothing

            Public Overrides Function Type() As e57Type
                Return e57Type.e57Structure
            End Function

            Public Overrides Function Name() As String
                Return nam
            End Function

            Public Overrides Function IsHomogeneous(Other As e57Node) As Boolean
                If Other.Type <> Me.Type Then Return False
                If Other.Name <> Me.Name Then Return False

                Dim AsStructure As e57Structure = DirectCast(Other, e57Structure)
                If AsStructure.Count <> Me.Count Then Return False

                For i As Integer = 0 To AsStructure.Count - 1 Step 1
                    Dim OtherChild As e57Node = AsStructure.GetChild(i)
                    Dim MyChild As e57Node = Me.GetChild(i)
                    If Not OtherChild.IsHomogeneous(MyChild) Then Return False
                Next

                Return False
            End Function

            Public Overrides Function Parent() As e57Node
                Return Par
            End Function

            Public Overrides Function IsCollection() As Boolean
                Return True
            End Function

            Public Overrides Function ToSuperString(Optional ThisLevel As Integer = 0) As String

                Dim out As String = String.Empty
                For i As Integer = 0 To ThisLevel - 1 Step 1
                    out &= "___"
                Next
                out &= Me.Type.ToString & ":" & Me.Name & vbCrLf

                ThisLevel += 1

                For i As Integer = 0 To Me.ChildList.Count - 1 Step 1
                    out &= Me.ChildList(i).ToSuperString(ThisLevel)
                Next

                Return out

            End Function
#End Region

            'over
            Private ChildList As New List(Of e57Node)

            Sub New()

            End Sub

            Sub Reinstantiate(xn As XmlNode, Parent As e57Node)

                Par = Parent
                nam = xn.Name

                For Each c As XmlNode In xn.ChildNodes
                    Select Case MapType(c)
                        Case e57Type.e57Float
                            AddChildren(New e57Float(c, Me))
                        Case e57Type.e57Integer
                            AddChildren(New e57Integer(c, Me))
                        Case e57Type.e57ScaledInteger
                            AddChildren(New e57ScaledInteger(c, Me))
                        Case e57Type.e57String
                            AddChildren(New e57String(c, Me))
                        Case e57Type.e57Vector
                            AddChildren(New e57Vector(c, Me))
                        Case e57Type.e57CompressedVector
                            AddChildren(New e57CompressedVector(c, Me))
                        Case e57Type.e57Structure
                            AddChildren(New e57Structure(c, Me))
                    End Select
                Next

            End Sub

            Sub New(xn As XmlNode, Parent As e57Node)

                Par = Parent
                nam = xn.Name

                For Each c As XmlNode In xn.ChildNodes
                    Select Case MapType(c)
                        Case e57Type.e57Float
                            AddChildren(New e57Float(c, Me))
                        Case e57Type.e57Integer
                            AddChildren(New e57Integer(c, Me))
                        Case e57Type.e57ScaledInteger
                            AddChildren(New e57ScaledInteger(c, Me))
                        Case e57Type.e57String
                            AddChildren(New e57String(c, Me))
                        Case e57Type.e57Vector
                            AddChildren(New e57Vector(c, Me))
                        Case e57Type.e57CompressedVector
                            AddChildren(New e57CompressedVector(c, Me))
                        Case e57Type.e57Structure
                            AddChildren(New e57Structure(c, Me))
                    End Select
                Next

            End Sub

            Public ReadOnly Property Count() As Integer
                Get
                    Return ChildList.Count
                End Get
            End Property

            Public Function AddChildren(Children As e57Node) As Boolean
                ChildList.Add(Children)
                Return True
            End Function

            Public Function GetChild(Index As Integer) As e57Node
                Return ChildList(Index)
            End Function

            Public Function GetChild(Name As String) As e57Node
                For Each node In ChildList
                    If node.Name = Name Then Return node
                Next
                Return Nothing
            End Function

            ''' <summary>
            ''' Valid with VectorChilds... looks up only in child elements(no recursion)
            ''' </summary>
            ''' <param name="Name"></param>
            ''' <returns></returns>
            Public Function GetString(Name As String) As String
                Dim stringnode As e57String = Me.GetChild(Name)
                If stringnode Is Nothing Then Return String.Empty
                Return stringnode.Text
            End Function

            ''' <summary>
            ''' Valid with VectorChilds... looks up only in child elements(no recursion)
            ''' </summary>
            ''' <param name="Name"></param>
            ''' <returns></returns>
            Public Function GetDouble(Name As String) As Double
                Dim floatnode As e57Float = Me.GetChild(Name)
                If floatnode Is Nothing Then Return Double.NaN
                Return floatnode.Value
            End Function

        End Class
