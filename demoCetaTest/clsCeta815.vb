Imports System.Collections.ObjectModel
Imports System.Collections.Specialized
Imports System.IO.Ports
Imports System.Text

Public Class Ceta815
    Private Shared _comPort As SerialPort
    Private ReadOnly _portName As String
    Private ReadOnly _baudRate As Integer
    Private Shared _receivedBytesQueue As ObservableCollection(Of Byte)
    Private Shared _receivedTelegramsQueue as ObservableCollection(Of Ceta815Telegram)
    Public Shared Property DifferentialPressure as Int16
    Public Shared Property VolumeRatio As String

    Public Sub New(portName As String, baudRate As Integer)
        _portName = portName
        _baudRate = baudRate
        _receivedBytesQueue = New ObservableCollection(Of Byte)
        _receivedTelegramsQueue = New ObservableCollection(Of Ceta815Telegram)

        AddHandler _receivedBytesQueue.CollectionChanged, AddressOf ReceivedBytesQueueCollectionChangedHandler
        AddHandler _receivedTelegramsQueue.CollectionChanged, AddressOf ReceivedTelegramsQueueCollectionChangedHandler
    End Sub

    ''' <summary>
    '''     @todo add comments for this function
    ''' </summary>
    ''' <returns></returns>
    Public Function InitSerialConnection() As Boolean
        _comPort = New SerialPort
        Try
            _comPort.BaudRate = _baudRate
            _comPort.PortName = _portName
            _comPort.DataBits = 8
            _comPort.StopBits = StopBits.One
            _comPort.Parity = Parity.None
            _comPort.Handshake = Handshake.None
            _comPort.DiscardNull = False
            _comPort.ReceivedBytesThreshold = 1
            _comPort.Open()

            If _comPort.IsOpen Then

                ' @todo test connection with ceta
                Debug.WriteLine("comPort initialization succeeded!")

                AddHandler _comPort.DataReceived, AddressOf ComPortDataReceivedHandler
                Return True
            Else
                Return False
            End If
        Catch ex As Exception
            Debug.WriteLine(ex.ToString())
            Return False
        End Try
    End Function

    Public Function SendCommand(declarationByte As Byte)

        Try
            Dim telegramLengthByte As Byte
            Dim telegramType As Ceta815Telegram.TelegramType
            Dim command As Byte()

            If declarationByte >= &H01 And declarationByte <= &H07 Then
                telegramType = Ceta815Telegram.TelegramType.WithoutDataBlock
                telegramLengthByte = &H0
            End If

            If telegramType = Ceta815Telegram.TelegramType.WithoutDataBlock Then
                ReDim command(4)
                command(0) = &H0D                   ' start byte
                command(1) = telegramLengthByte
                command(2) = declarationByte

                Dim crc As UShort = ComputeCrc16(command, 3)
                Dim crcBytes As Byte() = BitConverter.GetBytes(crc)

                command(3) = crcBytes(1)            ' CRC high byte
                command(4) = crcBytes(0)            ' CRC low byte

                _comPort.DiscardOutBuffer()
                _comPort.Write(command, 0, command.Length)
            End If
            Return True
        Catch ex As Exception
            Debug.WriteLine("ex @ SendCommand: " + ex.ToString())
            Return False
        End Try
    End Function

    ''' <summary>
    '''     Computes crc16 for given data and length of data.
    ''' </summary>
    ''' <param name="crcData">Array of bytes with data which crc should be calculated for.</param>
    ''' <param name="crcLength">Count of bytes in data.</param>
    ''' <returns></returns>
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

    Private Shared Sub ComPortDataReceivedHandler(sender As Object, e As SerialDataReceivedEventArgs)
        Do Until _comPort.BytesToRead = 0
            _receivedBytesQueue.Add(_comPort.ReadByte())
        Loop
    End Sub

    Private Shared Sub ReceivedBytesQueueCollectionChangedHandler(sender As Object,
                                                                  e As NotifyCollectionChangedEventArgs)
        If (e.Action = NotifyCollectionChangedAction.Add) Then

            ' remove old artifacts until start bit detected
            Try
                Do Until _receivedBytesQueue.First() = &HD
                    _receivedBytesQueue.RemoveAt(0)
                Loop
            Catch ex As Exception
                Debug.WriteLine("ex @ remove old artifacts: " + ex.ToString())
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
                    Debug.WriteLine("crc for received message was valid")
                    ' Create new telegram and push it to _receivedTelegramsQueue
                    Dim newTelegram As _
                            new Ceta815Telegram(telegramLength, telegramDeclaration, telegramData, telegramType)
                    _receivedTelegramsQueue.Add(newTelegram)
                Else
                    Debug.WriteLine("crc for received message was not valid!")
                End If

                ' remove old data
                ' @todo optimize deletion of old data in _receivedBytesQueue, could cause problems
                For i = 5 + telegramLength - 1 To 0 step - 1
                    _receivedBytesQueue.RemoveAt(i)
                Next
            End If
        End If
    End Sub

    Private Shared Sub ReceivedTelegramsQueueCollectionChangedHandler(sender As Object,
                                                                      e As NotifyCollectionChangedEventArgs)
        Debug.WriteLine(_receivedTelegramsQueue.Last().ToString())

        ' @todo: discard all telegrams, which are not needed as response to a call or contain any results

        If _receivedTelegramsQueue.Last().TelegramDeclarationString = "DifferentialPressure" Then
            ' hey we have the differential pressure -> yay

            Dim differentialPressureBytes as Byte() =
                    {_receivedTelegramsQueue.Last().TelegramData(1), _receivedTelegramsQueue.Last().TelegramData(0)}

            DifferentialPressure = BitConverter.ToInt16(differentialPressureBytes, 0)
            Debug.WriteLine("Differential Pressure: " + DifferentialPressure.ToString())
        End If
    End Sub
End Class


Public Class Ceta815Telegram
    Private ReadOnly _telegramLength As Byte
    Private ReadOnly _telegramType as TelegramType
    Private ReadOnly _telegramTime As Date
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
        _telegramTime = Now

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

    Public Overrides Function ToString() As String
        Dim outputString = "Telegram: " + _telegramTime.ToLongTimeString() + vbCrLf
        Dim stringDeclaration = "- Declaration: " + _telegramDeclaration.ToString("X2")
        Dim stringLength = "- Length: " + _telegramLength.ToString()
        Dim stringData = "- Data: "

        stringDeclaration += " => " + TelegramDeclarationString

        outputString += stringDeclaration + vbCrLf + stringLength + vbCrLf

        ' @todo: write comment for lambda function in _telegramData.Aggregate( .. )
        If _telegramType = TelegramType.WithDataBlock Then
            stringData += TelegramData.Aggregate("", Function(current, b) current + b.ToString("X2") + " ")
        Else
            stringData += "no data!"
        End If

        outputString += stringData + vbCrLf

        Return outputString
    End Function
End Class

