Module ModuloMediaDuplicados
    ''' <summary>
    ''' Agrupa resultados por folio base e elemento, e retorna uma nova lista com a média dos valores.
    ''' Se houver apenas um valor para um elemento, ele é mantido como está.
    ''' </summary>
    ''' <param name="dados">Lista original de resultados</param>
    ''' <returns>Lista com valores médios por folio base e elemento</returns>
    Public Function GerarMediaResultadosDuplicados(dados As List(Of ResultadoAnalise)) As List(Of ResultadoAnalise)
        Dim resultadosMediados As New List(Of ResultadoAnalise)

        If dados Is Nothing OrElse dados.Count = 0 Then Return resultadosMediados

        ' Agrupar por folio base (sem o sufixo [A], [B], etc.)
        Dim grupos = dados.GroupBy(Function(r)
                                       If r.Folio.Contains("[") Then
                                           Return r.Folio.Substring(0, r.Folio.IndexOf("["))
                                       Else
                                           Return r.Folio
                                       End If
                                   End Function)

        For Each grupo In grupos
            Dim folioBase = grupo.Key
            Dim registros = grupo.ToList()

            ' Agrupar por elemento
            Dim elementos = registros.GroupBy(Function(r) r.Elemento)

            For Each elemGrupo In elementos
                Dim elemento = elemGrupo.Key
                Dim valores = elemGrupo.Select(Function(r) r.Valor).ToList()

                Dim media As Double = Math.Round(valores.Average(), 4)
                Dim primeiro = elemGrupo.First()

                resultadosMediados.Add(New ResultadoAnalise With {
                .Folio = folioBase,
                .Elemento = elemento,
                .Valor = media,
                .DataProcessamento = primeiro.DataProcessamento,
                .TipoAmostra = primeiro.TipoAmostra
            })
            Next
        Next

        Return resultadosMediados
    End Function

End Module
