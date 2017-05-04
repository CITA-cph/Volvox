Imports E57LibReader

Public MustInherit Class e57Node

        Sub New()

        End Sub

        Public MustOverride Function Name() As String
        Public MustOverride Function Type() As e57Type
        Public MustOverride Function IsHomogeneous(Other As e57Node) As Boolean
        Public MustOverride Function Parent() As e57Node
        Public MustOverride Function ToSuperString(Optional ThisLevel As Integer = 0) As String
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
                    If Path.Current = Me.Name Then
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
                            Case e57Type.e57CompressedVector
                                Dim meas As e57CompressedVector = Me

                                If meas.Prototype.GetChildren(Path.Increment, Kids) = Result.Found Then AtLeastOnce = Result.Found
                                If meas.Codecs.GetChildren(Path.Increment, Kids) = Result.Found Then AtLeastOnce = Result.Found

                        End Select

                        Return AtLeastOnce
                    End If
                End If
            End If

            Return Result.NotFound

        End Function

        Public Function GetChildren(Path As String, ByRef Kids As List(Of e57Node)) As Result
            Return GetChildren(New e57Path(Path), Kids)
        End Function

        Public Overridable ReadOnly Property Value() As Double
            Get
                Return Double.NaN
            End Get
        End Property

        Public Sub Downstream(ByRef Store As List(Of e57Node), IncludeCollections As Boolean)

            Select Case Me.IsCollection
                Case True
                    If IncludeCollections Then Store.Add(Me)
                    Select Case Me.Type
                        Case e57Type.e57CompressedVector
                            Dim meas As e57CompressedVector = Me
                            meas.Prototype.Downstream(Store, IncludeCollections)
                            meas.Codecs.Downstream(Store, IncludeCollections)
                        Case e57Type.e57Structure
                            Dim meas As e57Structure = Me
                            For i As Integer = 0 To meas.Count - 1 Step 1
                                meas.GetChild(i).Downstream(Store, IncludeCollections)
                            Next
                        Case e57Type.e57Vector
                            Dim meas As e57Vector = Me
                            For i As Integer = 0 To meas.Count - 1 Step 1
                                meas.GetChild(i).Downstream(Store, IncludeCollections)
                            Next
                    End Select
                Case False
                    Store.Add(Me)
            End Select

        End Sub

        Public Overrides Function ToString() As String
            Return Me.Type.ToString & ":" & Me.Name
        End Function



    End Class
