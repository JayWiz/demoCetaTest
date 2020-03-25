Imports System.Collections.ObjectModel
Imports System.Collections.Specialized
Imports System.IO.Ports
Imports System.Threading

Public Class Ceta815
    Private Shared _comPort As SerialPort
    Private ReadOnly _portName As String
    Private ReadOnly _baudRate As Integer
    Private Shared _receivedBytesQueue As ObservableCollection(Of Byte)
    Private Shared _receivedTelegramsQueue as ObservableCollection(Of Ceta815Telegram)
    Private ReadOnly _resultStopwatch as Stopwatch
    Private Shared _lastConnectionTestTelegram as Ceta815Telegram
    Private Shared _lastResultsHeaderTelegram as Ceta815Telegram
    Private Shared _lastDifferentialPressureTelegram as Ceta815Telegram
    Private Shared _lastVolumeRatioTelegram as Ceta815Telegram
    Private Shared _lastResultsEndTelegramTelegram as Ceta815Telegram
    Private Shared _lastSwitchEventsOnOffTelegram as Ceta815Telegram

    ' @todo: implement functionality of this timer
    ' use Windows.Forms.Timer instead of Timers.Timer! does guarantee timer raises its events
    ' on thread that created it
    Private Shared _connectionTimer As Windows.Forms.Timer
    ' doubts whether usage of mutex is necessary and correct, cross-thread or cross-process?
    ' Private Shared _comPortMutex As Mutex

    Public Property DifferentialPressure as Short
    Public Property VolumeRatio As Double
    Public Property Result as String

    ''' <summary>
    '''     Initializes a new instance of <c>Ceta815</c>-Class
    ''' </summary>
    ''' <param name="portName">Name of serialPort</param>
    ''' <param name="baudRate">Baud rate</param>
    Public Sub New(portName As String, baudRate As Integer)
        'init member
        _portName = portName
        _baudRate = baudRate
        _receivedBytesQueue = New ObservableCollection(Of Byte)
        _receivedTelegramsQueue = New ObservableCollection(Of Ceta815Telegram)
        _resultStopwatch = new Stopwatch()
        _connectionTimer = new Windows.Forms.Timer With {
            .Interval = 10000 ' 10 sec?
            }

        ' serialPort stuff
        _comPort = New SerialPort With {
            .BaudRate = _baudRate,
            .PortName = _portName,
            .DataBits = 8,
            .StopBits = StopBits.One,
            .Parity = Parity.None,
            .Handshake = Handshake.None,
            .DiscardNull = False,
            .ReceivedBytesThreshold = 1
            }
        _comPort.Open()

        ' attach event handler
        AddHandler _receivedBytesQueue.CollectionChanged, AddressOf ReceivedBytesQueueCollectionChangedHandler
        AddHandler _receivedTelegramsQueue.CollectionChanged, AddressOf ReceivedTelegramsQueueCollectionChangedHandler
        AddHandler _comPort.DataReceived, AddressOf ComPortDataReceivedHandler
        AddHandler _connectionTimer.Tick, AddressOf ConnectionTimerTickHandler

        ' timer stuff
        ' @todo: setup connection timer, mutex, watchdog?
        _connectionTimer.Start()

    End Sub

    Private Sub ConnectionTimerTickHandler(sender As Object, e As EventArgs)
        Debug.WriteLine("Shots fired")
        If Not ConnectionTest() Then
            _comPort.Close()
            Thread.Sleep(500)
            _comPort.Open()
            If Not ConnectionTest() Then
                Debug.WriteLine("alarm!")
            Else 
                Debug.WriteLine("Successfully reconnected")
            End If
        End If
        ' Throw New NotImplementedException
    End Sub

    ''' <summary>
    '''     Initializes Ceta815. Checks whether connection is established and sets correct result feedback.
    ''' </summary>
    ''' <returns></returns>
    Public Function Init() As Boolean
        Try
            If Not ConnectionTest() Then
                Debug.WriteLine("ConnectionTest failed!")
                Return False
            End If
            Debug.WriteLine("ConnectionTest succeeded!")
            If Not SwitchEventsOnOff() Then
                Debug.WriteLine("SwitchOnOff failed!")
                Return False
            End If
            Debug.WriteLine("SwitchOnOff succeeded!")
            Return True
        Catch ex As Exception
            DebugMessage(ex, "InitSerialConnection")
            Return False
        End Try
    End Function

    ''' <summary>
    '''     Starts Ceta815 test program and stores results in corresponding properties.
    ''' </summary>
    ''' <returns></returns>
    Public Function ExecuteTest()
        ' @todo: add more security checks
        Try
            If ConnectionTest() Then
                If SendCommand(&H5) Then
                    ' wait until we get a resultsEndTelegram, but don't wait too long
                    _resultStopwatch.Restart()
                    While _lastResultsEndTelegramTelegram Is Nothing
                        If _resultStopwatch.Elapsed.Seconds > 10 Then
                            Debug.WriteLine("ExecuteTest failed: time expired!")
                            Return False
                        End If
                    End While

                    ' @todo: check if this type of validation is correct
                    ' @todo: validate also age of differentialPressure and volumeRatio telegrams
                    ' @todo: case without device, only volumeRatio, but no differentialPressure telegram received!
                    If _lastResultsEndTelegramTelegram IsNot Nothing Then
                        Dim telegramAge As TimeSpan = Now - _lastResultsEndTelegramTelegram.TelegramTime
                        If telegramAge.TotalSeconds < 30 Then
                            _lastResultsEndTelegramTelegram = Nothing
                            Return True
                        Else
                            _lastResultsEndTelegramTelegram = Nothing
                            Return False
                        End If
                    Else
                        Debug.WriteLine("ExecuteTest failed: no resultsEndTelegram received!")
                        Return False
                    End If
                Else
                    Debug.WriteLine("ExecuteTest failed: SendCommand failed!")
                    Return False
                End If
            Else
                Debug.WriteLine("ExecuteTest failed: ConnectionTest failed!")
                Return False
            End If
        Catch ex As Exception
            DebugMessage(ex, "ExecuteTest")
            Return False
        End Try
    End Function

    Private Shared Function SendCommand(declarationByte As Byte, Optional eventByte As Byte = &H00)
        Try
            Dim command As Byte()

            If declarationByte >= &H01 And declarationByte <= &H07 Then
                ReDim command(4)
                command(0) = &H0D                   ' start byte
                command(1) = &H00                   ' length byte
                command(2) = declarationByte
                Dim crc As UShort = ComputeCrc16(command, 3)
                Dim crcBytes As Byte() = BitConverter.GetBytes(crc)
                command(3) = crcBytes(1)            ' CRC high byte
                command(4) = crcBytes(0)            ' CRC low byte

            ElseIf declarationByte = &H0E Then
                ReDim command(5)
                command(0) = &H0D
                command(1) = &H01
                command(2) = declarationByte
                command(3) = eventByte
                Dim crc As UShort = ComputeCrc16(command, 4)
                Dim crcBytes As Byte() = BitConverter.GetBytes(crc)
                command(4) = crcBytes(1)
                command(5) = crcBytes(0)

            Else
                Return False
            End If

            ' @todo: problem, comPort write still possible, even if not plugged in anymore!
            _comPort.DiscardOutBuffer()
            _comPort.Write(command, 0, command.Length)

            Return True


        Catch ex As Exception
            DebugMessage(ex, "SendCommand")
            Return False
        End Try
    End Function

    Private Shared Function ConnectionTest() As Boolean
        Try
            If SendCommand(&H01) Then
                Thread.Sleep(100)
                ' @todo: check if this type of validation is correct
                ' @todo: move telegram age to Ceta815Telegram
                If _lastConnectionTestTelegram IsNot Nothing Then
                    Dim telegramAge As TimeSpan = Now - _lastConnectionTestTelegram.TelegramTime
                    If telegramAge.TotalSeconds < 30 Then
                        _lastConnectionTestTelegram = Nothing
                        Return True
                    Else
                        _lastConnectionTestTelegram = Nothing
                        Return False
                    End If
                Else
                    Return False
                End If
            Else
                Return False
            End If
        Catch ex As Exception
            DebugMessage(ex, "ConnectionTest")
            Return False
        End Try
    End Function

    Private Shared Function SwitchEventsOnOff() As Boolean
        Try
            If SendCommand(&H0E, &H01) Then
                Thread.Sleep(100)
                ' @todo: check if this type of validation is correct
                If _lastSwitchEventsOnOffTelegram IsNot Nothing
                    Dim telegramAge As TimeSpan = Now - _lastSwitchEventsOnOffTelegram.TelegramTime
                    If telegramAge.TotalSeconds < 30 Then
                        _lastSwitchEventsOnOffTelegram = Nothing
                        Return True
                    Else
                        _lastSwitchEventsOnOffTelegram = Nothing
                        Return False
                    End If
                Else
                    Return False
                End If
            Else
                Return False
            End If
        Catch ex As Exception
            DebugMessage(ex, "SwitchEventsOnOff")
            Return False
        End Try
    End Function

    Private Shared Function ComputeCrc16(crcData As IEnumerable, crcLength As Integer) As UShort
        Dim crc16 As UShort = &HFFFF
        Const crcPolynomial As UShort = &HA001
        For i = 0 To crcLength - 1
            crc16 = crc16 Xor crcData(i)
            For j = 1 To 8
                If (crc16 Mod 2) > 0 Then
                    crc16 = (crc16 And &HFFFE)/2
                    crc16 = crc16 Xor crcPolynomial
                Else
                    crc16 = (crc16 And &HFFFE)/2
                End If
            Next
        Next
        Return crc16
    End Function

    Private Shared Sub DebugMessage(ex As Exception, message As String)
        If True Then
            Debug.WriteLine("Exception @ " + message + ": " + ex.ToString())
        End If
    End Sub

#Region "Handler"

    'Private Shared Sub ConnectionTimerElapsedHandler(sender As Object, e As Timer)
    '    Debug.WriteLine("Shots fired")
    '    ConnectionTest()
    'End Sub

    Private Shared Sub ComPortDataReceivedHandler(sender As Object, e As SerialDataReceivedEventArgs)
        Do Until _comPort.BytesToRead = 0
            _receivedBytesQueue.Add(_comPort.ReadByte())
        Loop
    End Sub

    Private Sub ReceivedTelegramsQueueCollectionChangedHandler(sender As Object,
                                                               e As NotifyCollectionChangedEventArgs)
        ' @todo: comment this out
        If _receivedTelegramsQueue.Count > 0 Then
            Debug.WriteLine("Received Telegram: " + _receivedTelegramsQueue.Last.TelegramDeclarationString)
        End If

        If _receivedTelegramsQueue.Count > 0 Then
            Select Case _receivedTelegramsQueue.Last().TelegramDeclarationString

                Case "ConnectionTest"
                    _lastConnectionTestTelegram = _receivedTelegramsQueue.Last()

                Case "SwitchEventsOnOff"
                    _lastSwitchEventsOnOffTelegram = _receivedTelegramsQueue.Last()

                Case "ResultsHeader"
                    _lastResultsHeaderTelegram = _receivedTelegramsQueue.Last()

                    Dim resultByte As Byte = _lastResultsHeaderTelegram.TelegramData(1)
                    Select Case resultByte
                        Case &H1
                            Result = "PASS"
                        Case &H21
                            Result = "Volume too low"
                        Case &H22
                            Result = "Volume too high"
                        Case Else
                            Result = "FAIL"
                    End Select
                    Debug.WriteLine("Result: " + Result.ToString())

                Case "DifferentialPressure"
                    _lastDifferentialPressureTelegram = _receivedTelegramsQueue.Last()

                    Dim differentialPressureBytes As Byte() =
                            {_lastDifferentialPressureTelegram.TelegramData(1),
                             _lastDifferentialPressureTelegram.TelegramData(0)}
                    DifferentialPressure = BitConverter.ToInt16(differentialPressureBytes, 0)
                    Debug.WriteLine("Differential Pressure: " + DifferentialPressure.ToString())

                Case "VolumeRatio"
                    _lastVolumeRatioTelegram = _receivedTelegramsQueue.Last()

                    Dim volumeRatioBytes As Byte() =
                            {_lastVolumeRatioTelegram.TelegramData(1), _lastVolumeRatioTelegram.TelegramData(0)}
                    VolumeRatio = BitConverter.ToInt16(volumeRatioBytes, 0)/100
                    Debug.WriteLine("Volume Ratio: " + VolumeRatio.ToString())

                Case "ResultsEndTelegram"
                    _lastResultsEndTelegramTelegram = _receivedTelegramsQueue.Last()

            End Select

            Try
                _receivedTelegramsQueue.RemoveAt(0)
            Catch ex As Exception
                DebugMessage(ex, "ReceivedTelegramsQueueCollectionChangedHandler, remove latest telegram")
            End Try
        End If
    End Sub

    ' @todo: optimize this method
    Private Shared Sub ReceivedBytesQueueCollectionChangedHandler(sender As Object,
                                                                  e As NotifyCollectionChangedEventArgs)

        If (e.Action = NotifyCollectionChangedAction.Add) Then
            ' remove old artifacts until start bit detected
            Try
                Do Until _receivedBytesQueue.First() = &HD
                    _receivedBytesQueue.RemoveAt(0)
                Loop
            Catch ex As Exception
                DebugMessage(ex, "ReceivedByteQueueCollectionChangedHandler, remove old artifacts")
                Exit Sub
            End Try

            ' check valid start bit and length of telegram at least 5 elements
            If _receivedBytesQueue(0) = &H0D And _receivedBytesQueue.Count >= 5 Then

                ' get start byte, length, declaration and data of telegram
                Dim telegramStartByte As Byte = _receivedBytesQueue.Item(0)
                Dim telegramLength As Byte = _receivedBytesQueue.Item(1)
                Dim telegramDeclaration As Byte = _receivedBytesQueue.Item(2)

                ' Check whether enough elements received
                If Not _receivedBytesQueue.Count = 5 + telegramLength Then
                    Exit Sub
                End If

                Dim telegramType As Ceta815Telegram.TelegramType
                Dim telegramData As Byte()

                ' Determine telegramType (with or without data block)
                If telegramLength = &H00 Then
                    telegramType = Ceta815Telegram.TelegramType.WithoutDataBlock
                    telegramData = Nothing
                Else
                    telegramType = Ceta815Telegram.TelegramType.WithDataBlock
                    ' Rearrange array for data and fill it
                    ReDim telegramData(telegramLength - 1)
                    For i = 0 To telegramLength - 1
                        telegramData(i) = _receivedBytesQueue.Item(3 + i)
                    Next
                End If

                ' get received crc checksum
                Dim receivedCrcBytes As Byte() =
                        {_receivedBytesQueue.Item(4 + telegramLength), _receivedBytesQueue.Item(3 + telegramLength)}

                ' build received telegram
                Dim receivedTelegram As Byte() = {telegramStartByte, telegramLength, telegramDeclaration}

                ' append telegramData to receivedTelegram if there's a dataBlock
                If (telegramType = Ceta815Telegram.TelegramType.WithDataBlock)
                    receivedTelegram = receivedTelegram.Concat(telegramData).ToArray()
                End If

                ' compute crc from received telegram
                Dim computedCrcBytes As Byte() = BitConverter.GetBytes(ComputeCrc16(receivedTelegram,
                                                                                    receivedTelegram.Count()))
                ' compare received and computed crc
                If computedCrcBytes.SequenceEqual(receivedCrcBytes) Then
                    ' Debug.WriteLine("crc for received message was valid")
                    ' Create new telegram and push it to _receivedTelegramsQueue
                    Dim newTelegram As _
                            New Ceta815Telegram(telegramLength, telegramDeclaration, telegramData, telegramType)
                    _receivedTelegramsQueue.Add(newTelegram)
                Else
                    ' no need to do something special, if crc is invalid, we get a short debug notice
                    ' afterwards, telegram bytes will be discarded
                    Debug.WriteLine("crc for received message was not valid!")
                End If

                ' remove old data
                ' @todo: optimize deletion of old data in _receivedBytesQueue, could cause problems
                For i = 5 + telegramLength - 1 To 0 step - 1
                    _receivedBytesQueue.RemoveAt(i)
                Next
            End If
        End If
    End Sub

#End Region
End Class


Public Class Ceta815Telegram
    Private ReadOnly _telegramLength As Byte
    Private ReadOnly _telegramType as TelegramType
    Private ReadOnly _telegramDeclaration as Byte

    Private ReadOnly _
        _controlTelegramsDeclarations =
            {"", "ConnectionTest", "LockFrontPanelKeys", "UnlockFrontPanelKeys",
             "Stop", "Start", "SystemInformation1", "CycleCount", "ChangeOfProgram",
             "RequestProgramParameters", "SystemInformation2", "DeviceStatus",
             "ReadStatistics", "ResetStatistics", "SwitchEventsOnOff", "LastResult",
             "RequestFirmwareVersion", "ChangeToOperationMode",
             "RequestProgramNumber"}

    Private ReadOnly _
        _resultsTelegramsDeclarations =
            {"ResultsHeader", "FillPressureSecondaryFillRefillPressure", "StabilisationSecondaryStabilisationPressure",
             "DynamicDifferentialPressureOffset", "DifferentialPressure", "VentPressure", "VolumeRatio",
             "PrimaryFillPressure", "PrimaryStabilisationPressure", "Pre-fillPressure", "Pre-ventPressure", "",
             "TemperatureCompensation", "JigPressure", "ReservoirFillPressure", "ReactionPressureOfPressureStages"}

    Private ReadOnly _
        _otherDeclarations =
            {"Start-up", "TestCurve", "PhaseTelegram", "DeviceStatus", "ErrorTelegram", "", "", "", "CRCError"}

    Public Enum TelegramType
        WithDataBlock
        WithoutDataBlock
    End Enum

    Public Sub New(telegramLength As Byte, telegramDeclaration As Byte, telegramData As Byte(),
                   telegramType As TelegramType)
        _telegramLength = telegramLength
        Me.TelegramData = telegramData
        _telegramDeclaration = telegramDeclaration
        _telegramType = telegramType
        TelegramTime = Now

        If _telegramDeclaration >= &H01 And _telegramDeclaration <= &H12 Then
            TelegramDeclarationString = _controlTelegramsDeclarations(_telegramDeclaration)
        ElseIf _telegramDeclaration >= &H80 and _telegramDeclaration <= &H8F Then
            TelegramDeclarationString = _resultsTelegramsDeclarations(_telegramDeclaration - &H80)
        ElseIf _telegramDeclaration = &H9F Then
            TelegramDeclarationString = "ResultsEndTelegram"
        ElseIf _telegramDeclaration >= &HC0 And _telegramDeclaration <= &HC8 Then
            TelegramDeclarationString = _otherDeclarations(_telegramDeclaration - &HC0)
        Else
            TelegramDeclarationString = ""
        End If
    End Sub

    Public Property TelegramDeclarationString As String
    Public Property TelegramData as Byte()
    Public Property TelegramTime as Date

    Public Overrides Function ToString() As String
        Dim outputString = "Telegram: " + TelegramTime.ToLongTimeString() + vbCrLf
        Dim stringDeclaration = "- Declaration: " + _telegramDeclaration.ToString("X2")
        Dim stringLength = "- Length: " + _telegramLength.ToString()
        Dim stringData = "- Data: "

        stringDeclaration += " => " + TelegramDeclarationString
        outputString += stringDeclaration + vbCrLf + stringLength + vbCrLf

        If _telegramType = TelegramType.WithDataBlock Then
            ' append every element of Telegram to stringData, converted to hex (2 digits) and insert space
            stringData += TelegramData.Aggregate("", Function(current, b) current + b.ToString("X2") + " ")
        Else
            stringData += "no data!"
        End If

        outputString += stringData + vbCrLf

        Return outputString
    End Function
End Class

