Imports System.IO
Imports System.Threading
Imports E57LibReader
Imports E57LibCommon

Public Class E57TestApp

    Private FilePath As String = "C:\Mateusz\e57apptest.e57"

    Private xl As New List(Of Double)
    Private yl As New List(Of Double)
    Private zl As New List(Of Double)

    Private rl As New List(Of Int64)
    Private gl As New List(Of Int64)
    Private bl As New List(Of Int64)

    Private xo As New List(Of Double)
    Private yo As New List(Of Double)
    Private zo As New List(Of Double)

    Private Xmls As String = String.Empty
    Private PtsCount As Integer = 4773
    Private Checked As Integer = 0

    Private Report As String = String.Empty

    Private St As New System.Diagnostics.Stopwatch

    Private Sub E57TestApp_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        St.Start()
        Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture
        Dim rnd As New Random

        'create 100 random points

        For i As Integer = 0 To PtsCount - 1 Step 1
            xl.Add(rnd.NextDouble * 100)
            yl.Add(rnd.NextDouble * 100)
            zl.Add(rnd.NextDouble * 100)
            rl.Add(rnd.NextDouble * 255)
            bl.Add(rnd.NextDouble * 255)
            gl.Add(rnd.NextDouble * 255)
        Next

        CreateFile()
        Report &= "File created with " & PtsCount & " points" & vbCrLf
        LoadFile()
        Report &= "File loaded" & vbCrLf
        ComparePoints()
        Report &= "Data compared internally with " & Checked & " points consistent" & vbCrLf

        Report &= vbCrLf & "Evaluation tool result :" & vbCrLf
        RunEvalTool()
        Report &= vbCrLf

        Report &= Xmls
        LoadDebugInfo()

        RichTextBox1.Text = Report

    End Sub

    Private Sub CreateFile()
        'create document
        Dim nd As New E57LibWriter.e57Document()

        'add some metadata to the document
        nd.Guid = Guid.NewGuid.ToString 'thats not a guid
        nd.Date = DateTime.Now
        nd.LibraryVersion = "Volvox " & DateTime.Now.ToLongDateString
        nd.CoordinateMetadata = "TODO"
        'other values are set to default values so no need to change them
        'nd.LibraryVersion
        '...

        'create scan and add data to it
        Dim thiss As New E57LibWriter.Scan(nd)
        thiss.AppendData(ElementName.CartesianX, xl)
        thiss.AppendData(ElementName.CartesianY, yl)
        thiss.AppendData(ElementName.CartesianZ, zl)

        thiss.AppendData(ElementName.ColorRed, rl)
        thiss.AppendData(ElementName.ColorGreen, gl)
        thiss.AppendData(ElementName.ColorBlue, bl)

        'add some metadata to a scan
        thiss.Name = "Henrik's scan"
        thiss.Description = "Henrik's face 3d scanned"
        thiss.AtmosphericPressure = 1023
        thiss.Guid = Guid.NewGuid.ToString
        '... and other if needed
        thiss.Pose(0) = 1

        'add the scan to the document
        nd.Scans.Add(thiss)

        'save the file (this is the sub which you're generating xml part)
        If Not File.Exists(FilePath) Then
            Dim s As FileStream = File.Create(FilePath)
            s.Dispose()

        End If

        nd.Save(FilePath)
    End Sub

    Private Sub LoadFile()
        Dim nn As New e57Document(FilePath, True)
        Dim ns As E57LibReader.Scan = nn.Scans(0)
        ns.Data.GetList("cartesianX", xo)
        ns.Data.GetList("cartesianY", yo)
        ns.Data.GetList("cartesianZ", zo)

        Xmls = nn.XmlText
    End Sub

    Private Sub LoadDebugInfo()
        'Report &= vbCrLf & vbCrLf & "This is all we have from the glorious ''debugbin" & vbCrLf

        'For i As Integer = 0 To 'debugbin.Count - 1 Step 1
        '    Report &= 'debugbin.Item(i).ToString & vbCrLf
        'Next
    End Sub


    Private Sub ComparePoints()

        For i As Integer = 0 To PtsCount - 1 Step 1
            If xo(i) = xl(i) And yo(i) = yl(i) And zo(i) = zl(i) Then Checked += 1
        Next

    End Sub

    Private Sub RunEvalTool()

        Dim p As New System.Diagnostics.Process
        Dim ps As String = String.Empty
        p.StartInfo.UseShellExecute = False
        p.StartInfo.RedirectStandardOutput = True
        p.StartInfo.FileName = "C:\Mateusz\xerces-c-3.1.2\validate57.exe"
        p.StartInfo.CreateNoWindow = True
        p.StartInfo.Arguments = FilePath

        p.Start()

        Dim r As StreamReader = p.StandardOutput
        ps = r.ReadToEnd

        p.WaitForExit()
        p.Close()

        Report &= ps
    End Sub

    Dim p As System.Diagnostics.Process = Nothing

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        p = New System.Diagnostics.Process
        p.StartInfo.UseShellExecute = False
        p.StartInfo.FileName = "C:\Program Files\Rhinoceros 5.0 (64-bit)\System\Rhino.exe"
        p.StartInfo.Arguments = FilePath
        p.Start()
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        If p Is Nothing Then Return
        Try
            If p.HasExited Then Return
            p.CloseMainWindow()
            p.Close()
        Catch ex As Exception

        End Try
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        Dim op As New System.Windows.Forms.OpenFileDialog()
        op.FileName = FilePath
        If op.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
            FilePath = op.FileName
            LoadFile()
            Report = String.Empty
            RunEvalTool()
            Report &= vbCrLf & Xmls
            LoadDebugInfo()
            Me.RichTextBox1.Text = Report
            Me.RichTextBox1.Refresh()
        End If
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        Report = String.Empty
        FilePath = "C:\Mateusz\e57apptest.e57"

        xl = New List(Of Double)
        yl = New List(Of Double)
        zl = New List(Of Double)

        xo = New List(Of Double)
        yo = New List(Of Double)
        zo = New List(Of Double)

        Xmls = String.Empty
        Checked = 0

        Report = String.Empty


        Dim rnd As New Random

        'create 100 random points

        For i As Integer = 0 To PtsCount - 1 Step 1
            xl.Add(rnd.NextDouble * 100)
            yl.Add(rnd.NextDouble * 100)
            zl.Add(rnd.NextDouble * 100)
        Next

        CreateFile()
        Report &= "File created with " & PtsCount & " points" & vbCrLf
        LoadFile()
        Report &= "File loaded" & vbCrLf
        ComparePoints()
        Report &= "Data compared internally with " & Checked & " points consistent" & vbCrLf

        Report &= vbCrLf & "Evaluation tool result :" & vbCrLf
        RunEvalTool()
        Report &= vbCrLf

        Report &= Xmls
        LoadDebugInfo()
        RichTextBox1.Text = Report

    End Sub


End Class