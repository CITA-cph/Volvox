
''' <summary>
''' Path accepts only "/" as separator.
''' Path has to follow that scheme 
''' "../node1/node2/node3" (".." means the current node)
''' </summary>
''' <remarks></remarks>
Public Structure e57Path
            Private str As List(Of String)
            Private counter As Integer
            Private pathasstring As String

            Sub New(Path As String)
                str = New List(Of String)
                counter = 0
                Dim sp() As String = Path.Split("/")
                For Each s As String In sp
                    If s <> String.Empty Then str.Add(s)
                Next
                pathasstring = Path
                ' ../points/protytype/cartesianX

            End Sub

            Public ReadOnly Property Current As String
                Get
                    Return str(counter)
                End Get
            End Property

            Private Function GetNextCounter() As Integer
                Return counter + 1
            End Function

            Private Sub SetCounter(Val As Integer)
                Me.counter = Val
            End Sub

            Public Function Increment() As e57Path
                Dim np As New e57Path(pathasstring)
                np.SetCounter(Me.counter + 1)
                Return np
            End Function

            Public Function IsThatMe(Node As e57Node) As Boolean
                If counter = str.Count - 1 Then
                    If Node.Name = Current Then
                        Return True
                    End If
                End If
                Return False
            End Function

        End Structure

        Public Module StringUtils

            Public Function RepeatString(Str As String, Num As Integer) As String
                Dim out As New String("")
                For i As Integer = 0 To Num - 1 Step 1
                    out &= "___"
                Next
                Return out
            End Function

        End Module

