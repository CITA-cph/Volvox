Imports System.IO

Public Class Uninstall

    Private Sub YesBut_Click(sender As Object, e As EventArgs) Handles YesBut.Click

        Dim FolderPath As String = My.Computer.Registry.GetValue("HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Uninstall\Volvox", "InstallLocation", Nothing)

        If FolderPath Is Nothing Then
            MsgBox("Failed to uninstall", MsgBoxStyle.OkOnly, "Volvox Uninstaller")
            Exit Sub
        End If

        For Each f As String In Files.Libs(FolderPath)
            IO.File.Delete(f)
        Next

        Directory.Delete(FolderPath)

        My.Computer.Registry.CurrentUser.DeleteSubKeyTree("Software\Microsoft\Windows\CurrentVersion\Uninstall\Volvox")
        MsgBox("Unistallation complete", MsgBoxStyle.OkOnly, "Volvox Uninstaller")

        Dim info As New ProcessStartInfo()
        info.Arguments = "/C choice /C Y /N /D Y /T 3 & Del " & Application.ExecutablePath
        info.WindowStyle = ProcessWindowStyle.Hidden
        info.CreateNoWindow = True
        info.FileName = "cmd.exe"

        Process.Start(info).Dispose()
        Me.Close()
    End Sub

    Private Sub NoBut_Click(sender As Object, e As EventArgs) Handles NoBut.Click
        Me.Close()
    End Sub

End Class
