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
Imports System.Data.SqlClient
Imports System.Collections.Generic
Imports System.Data

Public Class Extractor
    Public Property ExtractorName As String

    Public Property object_filter As New List(Of String)
    Public Property type_filter As New List(Of String)

    Public Property where As String = ""

    Public Property DB_server As String
    Public Property DB_user As String
    Public Property DB_pwd As String
    Public Property DB_name As String

    Public Property folder_dest As String

    Public Event LogMessage(msg As String, isError As Boolean)

    Sub New(Optional ExtractorName As String = "")
        Me.ExtractorName = ExtractorName
    End Sub

    Private Sub log(msg As String, Optional isErr As Boolean = False)
        RaiseEvent LogMessage(msg, isErr)
    End Sub

    Private Function checkSettings() As Boolean
        If DB_server = "" Then
            log("Kein DB_server angegeben")
            Return False
        End If

        If DB_name = "" Then
            log("Kein DB_name angegeben")
            Return False
        End If

        Return True
    End Function

    Private Function getConnection() As SqlConnection
        Dim cs As New SqlConnectionStringBuilder()

        cs("Server") = DB_server
        If DB_user <> "" Then
            cs("user id") = DB_user
            cs.Password = DB_pwd
        Else
            cs("Integrated Security") = "true"
        End If
        cs("Initial catalog") = DB_name

        Dim cn As New SqlConnection(cs.ConnectionString)
        Return cn
    End Function

    Function StartExtract() As Integer

        If Not checkSettings() Then
            log("Fehler in den Settings", True)
            Return 0
        End If

        Dim cnt As Integer = 0
        Using cn As SqlConnection = getConnection(), cmd As New SqlCommand("", cn)

            Dim obj_names As String = ""
            Dim obj_types As String = ""
            Dim use_where As String = ""


            If object_filter.Count > 0 Then
                For Each t In object_filter
                    obj_names &= "OR name like '" & t & "' "
                Next
                obj_names = " AND ( 1=0 " & obj_names & ") "
            End If

            If type_filter.Count > 0 Then
                For Each t In type_filter
                    obj_types &= "OR type_desc like '" & t & "' "
                Next
                obj_types = " AND ( 1=0 " & obj_types & ") "
            End If

            If where <> "" Then
                use_where = " AND " & where
            End If



            cmd.CommandText = "select * FROM ( " & _
                    "SELECT OBJECT_NAME(sql_modules.object_id) as name, sql_modules.*, objects.type, objects.type_desc " & _
                    "FROM sys.sql_modules WITH (NOLOCK) " & _
                    "LEFT JOIN sys.objects WITH (NOLOCK) ON objects.object_id=sql_modules.object_id " & _
                    ") dt " & _
                    "WHERE " & _
                    "1=1 " & obj_names & obj_types & use_where

            cn.Open()

            Dim rd As SqlDataReader = cmd.ExecuteReader

            If rd.HasRows Then
                While rd.Read
                    If SaveObjectDef(rd) Then
                        cnt += 1
                    Else
                        log("Error while exporting")
                    End If
                End While
            Else
                log("No objects match current Filter")
                log("  Type: " & String.Join(", ", Me.type_filter))
                log("  Filter: " & String.Join(", ", Me.object_filter))
                log("  Where: " & Me.where)
            End If

        End Using

        Return cnt
    End Function

    Function getHeader() As String
        Dim hd As String = ""
        hd &= "--    /--------------------------------------------------- " & vbCrLf
        hd &= "--    |   Exported by SQL_Def Extractor                                   " & vbCrLf
        'hd &= "--    |      Date: " & Now.ToShortDateString & " " & Now.ToShortTimeString & vbCrLf
        hd &= "--    |      Database: " & DB_name & vbCrLf
        hd &= "--    \--------------------------------------------------- " & vbCrLf & vbCrLf

        Return hd
    End Function

    Function SaveObjectDef(def As IDataReader)
        Dim type As String = CStrD(def("type_desc"), "UNDEF_TYPE")
        Dim name As String = CStrD(def("name"), "UNDEF_NAME")
        Dim definition As String = CStrD(def("definition"))

        Dim basefolder = folder_dest

        If basefolder = "" Then
            basefolder = Directory.GetCurrentDirectory
            log(" Es wurde kein Ausgabeverzeichnis angegeben, benutze " & basefolder)
        End If

        If definition = "" Then
            log("   Defnition für " & type & "\" & name & " nicht gespeichert - leer")
        Else
            Dim fn As String = Path.Combine(folder_dest, type)
            If Not Directory.Exists(fn) Then Directory.CreateDirectory(fn)

            fn = Path.Combine(fn, name & ".sql")

            definition = getHeader() & definition

            File.WriteAllText(fn, definition)
            log("   Defnition für " & type & "\" & name & " gespeichert")
        End If


        Return True
    End Function


End Class

