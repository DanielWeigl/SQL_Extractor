' SQL Extractor
'   CLI Tool to extract View/StoredProc/...-Definitions from a SQL-Server 
'   to track them in a versioning system
'
' Copyright (C) 2012  Daniel Weigl
'
' This program is free software: you can redistribute it and/or modify
' it under the terms of the GNU General Public License as published by
' the Free Software Foundation, either version 3 of the License, or
' (at your option) any later version.
' 
' This program is distributed in the hope that it will be useful,
' but WITHOUT ANY WARRANTY; without even the implied warranty of
' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
' GNU General Public License for more details.

' You should have received a copy of the GNU General Public License
' along with this program.  If not, see <http://www.gnu.org/licenses/>.

Imports System.IO
Imports System.Collections.Generic
Imports System.Collections

Module Main

    Dim extractors As New List(Of Extractor)
    Dim current_ex As Extractor

    Sub Main()
        newExtractor("Default")
        extractors.Add(current_ex)

        If Not readParams() Then
            ex_LogMessage("Error while reading settings.", True)
            Environment.Exit(-1)
        Else
            Try
                For Each ex In extractors
                    If extractors.Count > 0 Then
                        ex_LogMessage("Starting Extractor '" & ex.ExtractorName & "'")
                    End If
                    ex.StartExtract()
                    ex_LogMessage("")
                Next
            Catch ex As Exception
                ex_LogMessage("Fehler: " & ex.Message, True)
            End Try

        End If
        'Console.ReadLine()

    End Sub

    Private Sub newExtractor(name As String)
        current_ex = New Extractor(name)
        AddHandler current_ex.LogMessage, AddressOf ex_LogMessage
    End Sub


    Private Function readParams() As Boolean
        Dim args() As String = System.Environment.GetCommandLineArgs()

        If args.Length = 0 Then
            ex_LogMessage("Keine Settings angegeben")
            Return False
        End If

        Dim cnt As Integer = 0
        For Each p In args
            cnt += 1
            ' Skip first arg (=exe file)
            If cnt > 1 AndAlso Not readCfgLine(p, cnt) Then
                Return False
            End If


        Next
        Return True
    End Function

    Private Function readSettingsFile(fn As String) As Boolean
        Dim ln As String() = File.ReadAllLines(fn)

        Dim cnt As Integer = 0
        For Each l In ln
            cnt += 1
            If Not readCfgLine(l, cnt) Then
                Return False
            End If
        Next
        Return True
    End Function


    Private Function readCfgLine(p As String, lineNr As Integer) As Boolean
        Dim line As String = p.Trim


        If line.StartsWith("#") Or line = "" Or line.StartsWith("--") Then
            ' Comment or empty - ignore
        ElseIf line.StartsWith("[") And line.EndsWith("]") Then
            ' New config section - start a new extractor
            newExtractor(line.Trim(New Char() {"[", "]"}))
            extractors.Add(current_ex)
        ElseIf line.EndsWith(".sql_extract") Then
            If Not readSettingsFile(p) Then
                Return False
            End If
        Else

            Dim segs As New List(Of String)(line.Split(New Char() {"="}, 2))

            If segs.Count < 2 Then
                ex_LogMessage(String.Format("Unable to read config {0}: {1}", lineNr, line))
                Return False
            End If

            Dim para As String = segs(0)
            Dim arg As String = segs(1)

            If para = "connection" Then
                current_ex.DB_connection_string = arg
            ElseIf para = "db" Then
                current_ex.DB_name = arg
            ElseIf para = "server" Then
                current_ex.DB_server = arg
            ElseIf para = "user" Then
                current_ex.DB_user = arg
            ElseIf para = "pwd" Then
                current_ex.DB_pwd = arg
            ElseIf para = "dest" Then
                current_ex.folder_dest = arg
            ElseIf para = "filter" Then
                current_ex.object_filter.Add(arg)
            ElseIf para = "type" Then
                current_ex.type_filter.Add(arg)
            ElseIf para = "where" Then
                current_ex.where = arg
            ElseIf para = "header" Then
                current_ex.output_header = paraTrue(arg)
            ElseIf para = "header_date" Then
                current_ex.output_header_timestamp = paraTrue(arg)
            ElseIf para = "versions" Then
                current_ex.output_versions = CInt(arg)
            Else
                ex_LogMessage(String.Format("Unknown parameter {0}", line))
                Return False
            End If
        End If
        Return True

    End Function

    Private Function paraTrue(ByVal para As String)
        para = para.ToLower
        Return para = "1" OrElse para = "true" OrElse para = "yes"
    End Function


    Private Sub ex_LogMessage(msg As String, Optional isError As Boolean = False)
        Console.WriteLine(msg)

        If isError Then
            Console.WriteLine("Press return to countinue ...")
            Console.ReadLine()
        End If

    End Sub
End Module

