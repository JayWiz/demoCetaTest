<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Das Formular überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Wird vom Windows Form-Designer benötigt.
    Private components As System.ComponentModel.IContainer

    'Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
    'Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
    'Das Bearbeiten mit dem Code-Editor ist nicht möglich.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.tbResult = New System.Windows.Forms.TextBox()
        Me.tbDifferentialPressure = New System.Windows.Forms.TextBox()
        Me.tbVolumeRatio = New System.Windows.Forms.TextBox()
        Me.Button2 = New System.Windows.Forms.Button()
        Me.tbFail = New System.Windows.Forms.TextBox()
        Me.tbPass = New System.Windows.Forms.TextBox()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        Me.nudCount = New System.Windows.Forms.NumericUpDown()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Button3 = New System.Windows.Forms.Button()
        Me.GroupBox1.SuspendLayout
        Me.GroupBox2.SuspendLayout
        CType(Me.nudCount,System.ComponentModel.ISupportInitialize).BeginInit
        Me.SuspendLayout
        '
        'Button1
        '
        Me.Button1.Location = New System.Drawing.Point(6, 19)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(110, 23)
        Me.Button1.TabIndex = 0
        Me.Button1.Text = "ExecuteTest"
        Me.Button1.UseVisualStyleBackColor = true
        '
        'tbResult
        '
        Me.tbResult.Enabled = false
        Me.tbResult.Location = New System.Drawing.Point(6, 48)
        Me.tbResult.Name = "tbResult"
        Me.tbResult.Size = New System.Drawing.Size(110, 20)
        Me.tbResult.TabIndex = 1
        '
        'tbDifferentialPressure
        '
        Me.tbDifferentialPressure.Enabled = false
        Me.tbDifferentialPressure.Location = New System.Drawing.Point(6, 74)
        Me.tbDifferentialPressure.Name = "tbDifferentialPressure"
        Me.tbDifferentialPressure.Size = New System.Drawing.Size(110, 20)
        Me.tbDifferentialPressure.TabIndex = 2
        '
        'tbVolumeRatio
        '
        Me.tbVolumeRatio.Enabled = false
        Me.tbVolumeRatio.Location = New System.Drawing.Point(6, 100)
        Me.tbVolumeRatio.Name = "tbVolumeRatio"
        Me.tbVolumeRatio.Size = New System.Drawing.Size(110, 20)
        Me.tbVolumeRatio.TabIndex = 3
        '
        'Button2
        '
        Me.Button2.Location = New System.Drawing.Point(6, 19)
        Me.Button2.Name = "Button2"
        Me.Button2.Size = New System.Drawing.Size(110, 23)
        Me.Button2.TabIndex = 4
        Me.Button2.Text = "Trigger INIT"
        Me.Button2.UseVisualStyleBackColor = true
        '
        'tbFail
        '
        Me.tbFail.Enabled = false
        Me.tbFail.Location = New System.Drawing.Point(6, 129)
        Me.tbFail.Name = "tbFail"
        Me.tbFail.Size = New System.Drawing.Size(76, 20)
        Me.tbFail.TabIndex = 5
        '
        'tbPass
        '
        Me.tbPass.Enabled = false
        Me.tbPass.Location = New System.Drawing.Point(6, 103)
        Me.tbPass.Name = "tbPass"
        Me.tbPass.Size = New System.Drawing.Size(76, 20)
        Me.tbPass.TabIndex = 6
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.Button1)
        Me.GroupBox1.Controls.Add(Me.tbResult)
        Me.GroupBox1.Controls.Add(Me.tbDifferentialPressure)
        Me.GroupBox1.Controls.Add(Me.tbVolumeRatio)
        Me.GroupBox1.Location = New System.Drawing.Point(12, 12)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(122, 127)
        Me.GroupBox1.TabIndex = 8
        Me.GroupBox1.TabStop = false
        Me.GroupBox1.Text = "Test"
        '
        'GroupBox2
        '
        Me.GroupBox2.Controls.Add(Me.nudCount)
        Me.GroupBox2.Controls.Add(Me.Label2)
        Me.GroupBox2.Controls.Add(Me.Label1)
        Me.GroupBox2.Controls.Add(Me.Button3)
        Me.GroupBox2.Controls.Add(Me.tbFail)
        Me.GroupBox2.Controls.Add(Me.Button2)
        Me.GroupBox2.Controls.Add(Me.tbPass)
        Me.GroupBox2.Location = New System.Drawing.Point(140, 12)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.Size = New System.Drawing.Size(122, 155)
        Me.GroupBox2.TabIndex = 9
        Me.GroupBox2.TabStop = False
        Me.GroupBox2.Text = "Debug"
        '
        'nudCount
        '
        Me.nudCount.Location = New System.Drawing.Point(6, 77)
        Me.nudCount.Name = "nudCount"
        Me.nudCount.Size = New System.Drawing.Size(110, 20)
        Me.nudCount.TabIndex = 12
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(88, 132)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(23, 13)
        Me.Label2.TabIndex = 11
        Me.Label2.Text = "Fail"
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(88, 106)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(30, 13)
        Me.Label1.TabIndex = 10
        Me.Label1.Text = "Pass"
        '
        'Button3
        '
        Me.Button3.Location = New System.Drawing.Point(6, 48)
        Me.Button3.Name = "Button3"
        Me.Button3.Size = New System.Drawing.Size(110, 23)
        Me.Button3.TabIndex = 5
        Me.Button3.Text = "AutoTest"
        Me.Button3.UseVisualStyleBackColor = True
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(272, 177)
        Me.Controls.Add(Me.GroupBox2)
        Me.Controls.Add(Me.GroupBox1)
        Me.Name = "Form1"
        Me.Text = "Form1"
        Me.GroupBox1.ResumeLayout(false)
        Me.GroupBox1.PerformLayout
        Me.GroupBox2.ResumeLayout(false)
        Me.GroupBox2.PerformLayout
        CType(Me.nudCount,System.ComponentModel.ISupportInitialize).EndInit
        Me.ResumeLayout(false)

End Sub

    Friend WithEvents Button1 As Button
    Friend WithEvents tbResult As TextBox
    Friend WithEvents tbDifferentialPressure As TextBox
    Friend WithEvents tbVolumeRatio As TextBox
    Friend WithEvents Button2 As Button
    Friend WithEvents tbFail As TextBox
    Friend WithEvents tbPass As TextBox
    Friend WithEvents GroupBox1 As GroupBox
    Friend WithEvents GroupBox2 As GroupBox
    Friend WithEvents Label2 As Label
    Friend WithEvents Label1 As Label
    Friend WithEvents Button3 As Button
    Friend WithEvents nudCount As NumericUpDown
End Class
