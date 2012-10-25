Module tools
    Function CStrD(ByVal value As Object, Optional ByVal OnConvError As String = "") As String

        If IsDBNull(value) OrElse value Is Nothing Then
            Return OnConvError
        Else
            Return value.ToString
        End If
    End Function
End Module
