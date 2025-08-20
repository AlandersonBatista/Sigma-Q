Public Module ModRegrasDeAjuste
    ''' <summary>
    ''' Aplica a 2ª regra do sistema: substitui valores negativos ou zero por 0,001 e arredonda para 3 casas decimais.
    ''' </summary>
    ''' <param name="agrupados">Dicionário de listas agrupadas por folio</param>
    ''' <returns>True se houve alguma alteração</returns>
    Public Function AplicarRegraValoresNegativosOuZerados(agrupados As Dictionary(Of String, List(Of ResultadoAnalise))) As Boolean
        Dim teveAlteracao As Boolean = False

        For Each listaResultados In agrupados.Values
            For Each resultado In listaResultados
                If resultado.Valor <= 0 Then
                    resultado.Valor = 0.1
                    teveAlteracao = True
                End If

                ' Arredondar sempre para 3 casas
                resultado.Valor = Math.Round(resultado.Valor, 2)
            Next
        Next

        Return teveAlteracao
    End Function

End Module
