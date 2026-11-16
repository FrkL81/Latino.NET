Imports System.Collections.Generic
Imports Latino.Core.Types
Imports Latino.Core.Runtime

Public Class SaludoVB
    <LatinoExtension("saludo_vb", Help:="Saluda desde Visual Basic", Args:="nombre")>
    Public Shared Function Saludar(args As List(Of LatinoObject)) As LatinoObject
        Dim nombre = args(0).ToString()
        Return New LatinoString("¡Hola " & nombre & " desde VB.NET!")
    End Function
End Class
