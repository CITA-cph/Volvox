<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class InstallerWindow
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(InstallerWindow))
        Me.InstallBut = New System.Windows.Forms.Button()
        Me.PathBut = New System.Windows.Forms.Button()
        Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
        Me.PictureBox1 = New System.Windows.Forms.PictureBox()
        Me.InstallExamples = New System.Windows.Forms.CheckBox()
        Me.TableLayoutPanel1.SuspendLayout()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'InstallBut
        '
        Me.InstallBut.AutoSize = True
        Me.InstallBut.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.InstallBut.Dock = System.Windows.Forms.DockStyle.Fill
        Me.InstallBut.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.InstallBut.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.InstallBut.Location = New System.Drawing.Point(8, 82)
        Me.InstallBut.Name = "InstallBut"
        Me.InstallBut.Size = New System.Drawing.Size(618, 28)
        Me.InstallBut.TabIndex = 0
        Me.InstallBut.Text = "Install"
        Me.InstallBut.UseVisualStyleBackColor = True
        '
        'PathBut
        '
        Me.PathBut.AutoSize = True
        Me.PathBut.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.PathBut.Dock = System.Windows.Forms.DockStyle.Fill
        Me.PathBut.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.PathBut.Location = New System.Drawing.Point(8, 51)
        Me.PathBut.Name = "PathBut"
        Me.PathBut.Size = New System.Drawing.Size(618, 25)
        Me.PathBut.TabIndex = 1
        Me.PathBut.Text = "Select Folder"
        Me.PathBut.UseVisualStyleBackColor = True
        '
        'TableLayoutPanel1
        '
        Me.TableLayoutPanel1.AutoSize = True
        Me.TableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.TableLayoutPanel1.ColumnCount = 1
        Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
        Me.TableLayoutPanel1.Controls.Add(Me.InstallExamples, 0, 0)
        Me.TableLayoutPanel1.Controls.Add(Me.InstallBut, 0, 2)
        Me.TableLayoutPanel1.Controls.Add(Me.PathBut, 0, 1)
        Me.TableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.TableLayoutPanel1.Location = New System.Drawing.Point(0, 353)
        Me.TableLayoutPanel1.Margin = New System.Windows.Forms.Padding(12)
        Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
        Me.TableLayoutPanel1.Padding = New System.Windows.Forms.Padding(5)
        Me.TableLayoutPanel1.RowCount = 3
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle())
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle())
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle())
        Me.TableLayoutPanel1.Size = New System.Drawing.Size(634, 118)
        Me.TableLayoutPanel1.TabIndex = 3
        '
        'PictureBox1
        '
        Me.PictureBox1.Cursor = System.Windows.Forms.Cursors.Help
        Me.PictureBox1.Dock = System.Windows.Forms.DockStyle.Top
        Me.PictureBox1.Image = CType(resources.GetObject("PictureBox1.Image"), System.Drawing.Image)
        Me.PictureBox1.Location = New System.Drawing.Point(0, 0)
        Me.PictureBox1.Name = "PictureBox1"
        Me.PictureBox1.Size = New System.Drawing.Size(634, 348)
        Me.PictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage
        Me.PictureBox1.TabIndex = 4
        Me.PictureBox1.TabStop = False
        '
        'CheckBox1
        '
        Me.InstallExamples.AutoSize = True
        Me.InstallExamples.Checked = True
        Me.InstallExamples.CheckState = System.Windows.Forms.CheckState.Checked
        Me.InstallExamples.Dock = System.Windows.Forms.DockStyle.Fill
        Me.InstallExamples.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.InstallExamples.Location = New System.Drawing.Point(8, 8)
        Me.InstallExamples.Name = "CheckBox1"
        Me.InstallExamples.Padding = New System.Windows.Forms.Padding(40, 10, 10, 10)
        Me.InstallExamples.Size = New System.Drawing.Size(618, 37)
        Me.InstallExamples.TabIndex = 5
        Me.InstallExamples.Text = "Install examples folder on my desktop"
        Me.InstallExamples.UseVisualStyleBackColor = True
        '
        'InstallerWindow
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.AutoSize = True
        Me.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.BackColor = System.Drawing.Color.White
        Me.ClientSize = New System.Drawing.Size(634, 471)
        Me.Controls.Add(Me.PictureBox1)
        Me.Controls.Add(Me.TableLayoutPanel1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.KeyPreview = True
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.MinimumSize = New System.Drawing.Size(650, 460)
        Me.Name = "InstallerWindow"
        Me.ShowIcon = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Volvox Installer"
        Me.TableLayoutPanel1.ResumeLayout(False)
        Me.TableLayoutPanel1.PerformLayout()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents InstallBut As Button
    Friend WithEvents PathBut As Button
    Friend WithEvents TableLayoutPanel1 As TableLayoutPanel
    Friend WithEvents PictureBox1 As PictureBox
    Friend WithEvents InstallExamples As CheckBox
End Class
