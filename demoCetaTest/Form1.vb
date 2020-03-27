Public Class Form1
    Private _oCeta815 As Ceta815

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        _oCeta815 = New Ceta815("COM1", 115200)
        If Not _oCeta815.Init() Then
            MsgBox("Initialization of Ceta815 failed!")
        End If

        Trace.Listeners.Add(new TextWriterTraceListener("trace.log") With {.TraceOutputOptions = TraceOptions.Timestamp})
        Trace.AutoFlush = true
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click

        Dim laufzeit as New Stopwatch
        Dim errorCode = 0
        Dim cetaBusy = True
        Dim result = ""
        Dim volume_ratio = ""
        Dim diff_pressure = ""

        If errorCode = 0 Then
            ' TEST START
            laufzeit.Restart()
            If _oCeta815.ConnectionTest() Then
                If _oCeta815.ExecuteTest() Then
                    ' test was executed properly, received result of Ceta815
                    If _oCeta815.Result = "PASS" Then
                        result = "PASS"
                        volume_ratio = _oCeta815.VolumeRatio
                        diff_pressure = _oCeta815.DifferentialPressure
                        errorCode = 0
                    Else If _oCeta815.Result = "Volume too low"
                        result = "FAIL"
                        volume_ratio = _oCeta815.VolumeRatio
                        errorCode = 1208
                    Else If _oCeta815.Result = "FAIL"
                        result = "FAIL"
                        volume_ratio = _oCeta815.VolumeRatio
                        diff_pressure = _oCeta815.DifferentialPressure
                        errorCode = 1209
                    End If
                Else
                    ' didnt get proper result
                    result = "FAIL"
                    errorCode = 1207
                End If
            Else
                ' ConnectionTest() failed, Ceta815 not responding
                result = "FAIL"
                errorCode = 1206
            End If
            

            laufzeit.Stop()
            Debug.WriteLine("doTestSt12_Ceta() Laufzeit =>" & laufzeit.ElapsedMilliseconds/1000 & "s")
            Debug.WriteLine("errorCode: " + errorCode.ToString() + ", result: " + result + ", volume_ratio: " + volume_ratio + ", diff_pressure: " + diff_pressure)


        End If


        tbDifferentialPressure.Text = diff_pressure
        tbVolumeRatio.Text = volume_ratio
        tbResult.Text = result
        If result = "PASS" Then
            tbResult.BackColor = Color.LightGreen
        Else
            tbResult.BackColor = Color.Red
        End If
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
