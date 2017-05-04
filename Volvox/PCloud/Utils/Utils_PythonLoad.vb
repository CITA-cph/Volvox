Imports System.IO
Imports System.Windows.Forms
Imports IronPython.Hosting

Namespace CC_Python

    Module PythonLoad

        Public Sub LoadPython()

            Dim path As String = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
            path += "\Grasshopper\Libraries\"

            For Each filename As String In IO.Directory.GetFiles(path, "*", IO.SearchOption.AllDirectories)
                If IO.Path.GetFileName(filename) = "Volvox_Python.py" Then
                    Dim ThisScript As String = SomePythonPart() & File.ReadAllText(filename)
                    Dim dir As String = IO.Path.GetDirectoryName(filename)

                    Dim newfile As String = dir & "\Volvox_" & Process.GetCurrentProcess.Id

                    Using fstr As TextWriter = New StreamWriter(newfile)
                        Dim tempstr() As String = ThisScript.Split(vbCrLf)

                        For Each l As String In tempstr
                            fstr.WriteLine(l)
                        Next
                    End Using

                    MySettings.PyScripts = Python.CreateEngine.ExecuteFile(newfile)
                    File.Delete(newfile)

                    Exit For
                End If
            Next

        End Sub

        Function SomePythonPart() As String

            Dim intro As New String("")

            Dim appstart As String = Application.StartupPath
            Dim ironpy As String = Application.StartupPath
            ironpy = ironpy.Replace("System", "Plug-ins\IronPython\Lib")

            intro &= "import sys" & vbCrLf
            intro &= "import System.IO" & vbCrLf
            intro &= "sys.path.append('" & ironpy & "')" & vbCrLf
            intro &= "import subprocess" & vbCrLf
            intro &= "import os" & vbCrLf
            intro &= "import shutil" & vbCrLf
            intro &= "import glob" & vbCrLf
            intro &= "import clr" & vbCrLf
            intro &= "clr.AddReferenceToFileAndPath('" & Application.StartupPath & "\RhinoCommon.dll')" & vbCrLf
            intro &= "import Rhino as rc" & vbCrLf & vbCrLf

            Return intro

        End Function

    End Module

End Namespace

