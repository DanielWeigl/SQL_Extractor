Imports System.IO
Imports System.Collections.Generic
Imports System.Collections

Module Module1
    Dim WithEvents ex As New Extractor

    Sub Main()

        readParams()

        Try
            ex.StartExtract()
        Catch ex As Exception
            ex_LogMessage("Fehler: " & ex.Message, True)

        End Try
    End Sub

    Private Sub readParams()
        Dim args() As String = System.Environment.GetCommandLineArgs()

        If args.Length = 0 Then
            ex_LogMessage("Keine Settings angegeben", True)
            Environment.Exit(-1)
        End If

        For Each p In args
            readParam(p)

            If p.EndsWith(".sql_extract") Then readSettings(p)
        Next
    End Sub

    Private Sub readSettings(fn As String)
        Dim ln As String() = File.ReadAllLines(fn)

        For Each l In ln
            readParam(l)
        Next
    End Sub

    Private Sub readParam(p As String)
        If p.StartsWith("db=") Then ex.DB_name = split_para(p)
        If p.StartsWith("server=") Then ex.DB_server = split_para(p)
        If p.StartsWith("user=") Then ex.DB_user = split_para(p)
        If p.StartsWith("pwd=") Then ex.DB_pwd = split_para(p)

        If p.StartsWith("dest=") Then ex.folder_dest = split_para(p)

        If p.StartsWith("filter_name=") Then ex.object_filter.Add(split_para(p))
        If p.StartsWith("filter_type=") Then ex.type_filter.Add(split_para(p))
        If p.StartsWith("where=") Then ex.where = split_para(p)
    End Sub

    Private Function split_para(p As String)
        Dim x As String() = p.Split("=")
        If x.Length < 2 Then Throw New Exception("Param " & p & " has wrong format")
        Return x(1).Trim
    End Function

    Private Sub ex_LogMessage(msg As String, isError As Boolean) Handles ex.LogMessage
        Console.WriteLine(msg)

        If isError Then
            Console.WriteLine("Press return to countinue ...")
            Console.ReadLine()
        End If

    End Sub
End Module
