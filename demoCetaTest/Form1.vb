Imports System.IO

Public Class Form1
    Private _oCeta815 As Ceta815

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        _oCeta815 = New Ceta815("COM1", 115200)
        If Not _oCeta815.Init() Then
            MsgBox("Initialization of Ceta815 failed!")
        End If

        Trace.Listeners.Add(new TextWriterTraceListener("trace.log") With {.TraceOutputOptions = TraceOptions.Timestamp})
        Trace.AutoFlush = true
        
        

        'Dim  tr1 As TextWriterTraceListener = new TextWriterTraceListener(System.Console.Out)
        'Debug.Listeners.Add(tr1)
        
        'Dim tr2 As TextWriterTraceListener = new TextWriterTraceListener(System.IO.File.CreateText("Output.txt"))
        'Debug.Listeners.Add(tr2)


    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If _oCeta815.ExecuteTest() Then
            tbDifferentialPressure.Text = _oCeta815.DifferentialPressure
            tbVolumeRatio.Text = _oCeta815.VolumeRatio
            tbResult.Text = _oCeta815.Result
            If _oCeta815.Result = "PASS" Then
                tbResult.BackColor = Color.LightGreen
            Else
                tbResult.BackColor = Color.Red
            End If
            Debug.WriteLine("ExecuteTest succeeded")
        Else
            Debug.WriteLine("ExecuteTest failed")
        End IF
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        _oCeta815.Init()
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        Dim countPass, countFail as Integer
        Dim count = nudCount.Value
        While count > 0
            If _oCeta815.ExecuteTest() Then
                countPass += 1
                tbPass.Text = countPass.ToString()
            Else
                countFail += 1
                tbFail.Text = countFail.ToString()
            End If
            tbDifferentialPressure.Text = _oCeta815.DifferentialPressure
            tbVolumeRatio.Text = _oCeta815.VolumeRatio
            tbResult.Text = _oCeta815.Result
            If _oCeta815.Result = "PASS" Then
                tbResult.BackColor = Color.LightGreen
            Else
                tbResult.BackColor = Color.Red
            End If
            count -= 1
            Application.DoEvents()
        End While
    End Sub

    
End Class
