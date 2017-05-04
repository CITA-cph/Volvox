Imports System.IO

Module Utils_IO

    Function IsFileLocked(file As FileInfo) As Boolean

        If Not file.Exists() Then Return False

        Dim stream As FileStream = Nothing
        If Not System.IO.File.Exists(file.ToString) Then Return False

        Try
            stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None)
        Catch generatedExceptionName As IOException
            Return True
        Finally
            If stream IsNot Nothing Then
                stream.Close()
            End If
        End Try

        Return False
    End Function

    Function RemoveIllegal(p As String) As String

        Dim nl As New List(Of String)
        nl.Add(vbCr)
        nl.Add(vbCrLf)
        nl.Add(vbLf)

        For Each c In nl
            p = p.Replace(c, "").Trim
        Next
        Return p
    End Function

    Function ValidatePath(path As String) As String
        Dim ext As String = System.IO.Path.GetExtension(path)
        If ext Is Nothing Or ext = "" Or ext = String.Empty Then
            path &= ".e57"
        End If
        Return path
    End Function

    Function IsValidFileNameOrPath(ByVal name As String) As Boolean
        ' Determines if the name is Nothing.
        If name Is Nothing Then
            Return False
        End If

        ' Determines if there are bad characters in the name.
        For Each badChar As Char In System.IO.Path.GetInvalidPathChars
            If InStr(name, badChar) > 0 Then
                Return False
            End If
        Next

        ' The name passes basic validation.
        Return True
    End Function

End Module
