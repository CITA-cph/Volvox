Imports System.IO
Imports System.Security
Imports System.Security.Permissions
Imports Microsoft.VisualBasic.FileIO

Public Class InstallerWindow

    Private MyFolder As String = String.Empty

    Private Function IsInstalled() As Boolean
        Dim FolderPath As String = My.Computer.Registry.GetValue("HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Uninstall\Volvox", "InstallLocation", Nothing)
        If FolderPath IsNot Nothing Then Return True
        Return False
    End Function

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        MyFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) & "\Grasshopper\Libraries"
        PathBut.Text = "Install in: " & MyFolder

        If MsgBox("If you already have any previous version of Volvox installed, please delete manually the files from Grasshopper libraries." & vbCrLf & vbCrLf & "Please do it before installing this version of Volvox.", MsgBoxStyle.OkCancel, "Volvox installer") <> MsgBoxResult.Ok Then
            Me.Close()
        End If
    End Sub

    Private Sub PathBut_Click(sender As Object, e As EventArgs) Handles PathBut.Click
        Dim npath As New FolderBrowserDialog
        npath.Description = "Select Grasshopper Libraries folder"
        npath.RootFolder = Environment.SpecialFolder.MyComputer
        npath.ShowNewFolderButton = True
        If npath.ShowDialog = DialogResult.OK Then
            MyFolder = npath.SelectedPath
            PathBut.Text = "Install in: " & MyFolder
        End If
    End Sub

    Private Sub InstallBut_Click(sender As Object, e As EventArgs) Handles InstallBut.Click

        Dim nlic As New LicenceAgreement()

        If nlic.ShowDialog(Me) = DialogResult.OK Then

            If IsInstalled() Then
                Dim msgboxres As MsgBoxResult = MsgBox("Volvox is already installed. Do you want to uninstall the current version ?", MsgBoxStyle.OkCancel, "Volvox installer")

                If msgboxres = MsgBoxResult.Ok Then
                    Uninstall()
                ElseIf msgboxres = MsgBoxResult.Cancel Then
                    MsgBox("Installation failed", MsgBoxStyle.OkOnly, "Volvox installer")
                    Me.Close()
                    Exit Sub
                End If

            End If

            If Install(MyFolder) Then
                MsgBox("Installation successful", MsgBoxStyle.OkOnly, "Volvox installer")
            End If

            Me.Close()
        Else
            MsgBox("Installation failed", MsgBoxStyle.OkOnly, "Volvox installer")
            Me.Close()
        End If

    End Sub

    Private Sub Form1_Escape(sender As Object, e As KeyEventArgs) Handles Me.KeyDown
        If e.KeyCode = Keys.Escape Then
            Me.Close()
        End If
    End Sub

    Public Function GetByteArrayFromIcon(ByVal icoSource As Icon) As Byte()
        Dim byteArray As Byte()
        Using msIcon As New System.IO.MemoryStream
            icoSource.Save(msIcon)
            byteArray = msIcon.ToArray
        End Using
        Return byteArray
    End Function

    Function Install(FolderPath As String) As Boolean

        If Not Directory.Exists(FolderPath) Then Directory.CreateDirectory(FolderPath)

        Dim UninstallPath As New String(FolderPath)
        FolderPath &= "\Volvox"
        Dim ExamplesPath As String = SpecialDirectories.Desktop & "\Volvox Examples"
        If Not Directory.Exists(ExamplesPath) Then Directory.CreateDirectory(ExamplesPath)

        Try
            If Not Directory.Exists(FolderPath) Then
                Directory.CreateDirectory(FolderPath)
            End If

            Dim RegKey As String = "HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Uninstall\Volvox"

            My.Computer.Registry.SetValue(RegKey, "InstallLocation", FolderPath)
            My.Computer.Registry.SetValue(RegKey, "DisplayName", "Volvox")
            My.Computer.Registry.SetValue(RegKey, "UninstallString", UninstallPath & "\UnVolvox.exe")

            My.Computer.Registry.SetValue(RegKey, "Publisher", "Centre for Information Technology and Architecture")
            My.Computer.Registry.SetValue(RegKey, "HelpLink", "http://duraark.eu/")
            My.Computer.Registry.SetValue(RegKey, "EstimatedSize", 900)
            My.Computer.Registry.SetValue(RegKey, "DisplayVersion", "0.3.0.0")
            My.Computer.Registry.SetValue(RegKey, "Comment", "Uninstall Volvox")

            IO.File.WriteAllBytes(UninstallPath & "\UnVolvox.exe", My.Resources.UnVolvox)

            IO.File.WriteAllText(FolderPath & "\Licence.txt", My.Resources.Licence)
            IO.File.WriteAllBytes(FolderPath & "\VolvoxIcon.ico", GetByteArrayFromIcon(My.Resources.VolvoxIcon))

            IO.File.WriteAllBytes(FolderPath & "\Volvox_Cloud.dll", My.Resources.Volvox_Cloud)
            IO.File.WriteAllBytes(FolderPath & "\Volvox_Instr.dll", My.Resources.Volvox_Instr)
            IO.File.WriteAllBytes(FolderPath & "\Volvox_Python.py", My.Resources.Volvox_Python)
            IO.File.WriteAllBytes(FolderPath & "\Volvox.gha", My.Resources.Volvox)

            IO.File.WriteAllBytes(FolderPath & "\E57LibCommon.dll", My.Resources.E57LibCommon)
            IO.File.WriteAllBytes(FolderPath & "\E57LibReader.dll", My.Resources.E57LibReader)
            IO.File.WriteAllBytes(FolderPath & "\E57LibWriter.dll", My.Resources.E57LibWriter)

            IO.File.WriteAllBytes(FolderPath & "\Crc32C.NET.dll", My.Resources.Crc32C_NET)
            IO.File.WriteAllBytes(FolderPath & "\Crc32C.NET.xml", System.Text.Encoding.UTF8.GetBytes(My.Resources.Crc32C_NET1))

            If Me.InstallExamples.Checked Then

                Dim nm As New List(Of String)
                Dim ex As List(Of Byte()) = Files.ExamplesBytes(nm)

                For i As Integer = 0 To nm.Count - 1 Step 1
                    IO.File.WriteAllBytes(ExamplesPath & "\" & nm(i), ex(i))
                Next

            End If

            My.Computer.Registry.SetValue(RegKey, "DisplayIcon", FolderPath & "\VolvoxIcon.ico")

        Catch ex As Exception
            MsgBox("Installation failed. Try to run the installer as administrator.", MsgBoxStyle.OkOnly, "Volvox Installer")
            Return False
        End Try

        Return True

    End Function

    Function Uninstall() As Boolean

        Dim FolderPath As String = My.Computer.Registry.GetValue("HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Uninstall\Volvox", "InstallLocation", Nothing)

        If FolderPath Is Nothing Then
            MsgBox("Failed to uninstall", MsgBoxStyle.OkOnly, "Volvox Uninstaller")
            Return False
        End If

        For Each f As String In Files.Libs(FolderPath)
            IO.File.Delete(f)
        Next

        Directory.Delete(FolderPath)
        My.Computer.Registry.CurrentUser.DeleteSubKeyTree("Software\Microsoft\Windows\CurrentVersion\Uninstall\Volvox")

        Return True
    End Function

    Private Sub PictureBox1_Click(sender As Object, e As EventArgs) Handles PictureBox1.Click
        Process.Start("http://duraark.eu/")
    End Sub
End Class
