Imports System

' У VB "Module" — це статичний аналог класу.
' Він зручний, якщо ви хочете викликати Log без створення екземпляра.
Public Module Logger

    Public Enum MessageType
        Usual = 0
        Attention = 1
        Warning = 2
    End Enum

    Public Sub Log(msg As String, type As MessageType)
        Select Case type
            Case MessageType.Usual
                Console.ForegroundColor = ConsoleColor.White
            Case MessageType.Attention
                Console.ForegroundColor = ConsoleColor.Yellow
            Case MessageType.Warning
                Console.ForegroundColor = ConsoleColor.Red
        End Select

        Console.WriteLine(msg)
        ' За потреби можна повернути колір у вихідний
        Console.ResetColor()
    End Sub

End Module
