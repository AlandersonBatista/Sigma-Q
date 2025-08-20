Public Module ModComparadorDeResultados
    ''' <summary>
    ''' Compara valores entre pares de folios (original e [A]) e retorna apenas os que ultrapassam a tolerância
    ''' para tipos de amostra "CARVÃO" ou "COQUE". Outros tipos são ignorados.
    ''' </summary>
    ''' <param name="dados">Lista de resultados analisados</param>
    ''' <returns>Lista de resultados reprovados</returns>
    Public Function ObterResultadosReprovados(dados As List(Of ResultadoAnalise)) As List(Of ResultadoAnalise)
        Dim reprovados As New List(Of ResultadoAnalise)

        If dados Is Nothing OrElse dados.Count = 0 Then Return reprovados

        ' Agrupar por folio base (remove o [A])
        Dim grupos = dados.GroupBy(Function(r)
                                       If r.Folio.Contains("[") Then
                                           Return r.Folio.Substring(0, r.Folio.IndexOf("["))
                                       Else
                                           Return r.Folio
                                       End If
                                   End Function)

        For Each grupo In grupos
            Dim resultados = grupo.ToList()

            ' Separar original e [A]
            Dim originais = resultados.Where(Function(r) Not r.Folio.Contains("[")).ToList()
            Dim replicas = resultados.Where(Function(r) r.Folio.Contains("[A]")).ToList()

            If originais.Count = 0 OrElse replicas.Count = 0 Then Continue For

            ' Pega o tipo de amostra do original
            Dim tipoAmostra = originais.First().TipoAmostra.ToUpperInvariant()

            ' Se não for CARVÃO nem COQUE, ignora esse grupo
            If Not (tipoAmostra.Contains("CARV") OrElse tipoAmostra.Contains("COQUE")) Then Continue For

            ' Comparar elementos relevantes
            Dim elementosParaVerificar = {"Matéria Volátil", "Teor de Cinzas"}

            For Each elemento In elementosParaVerificar
                Dim v1 = originais.FirstOrDefault(Function(r) r.Elemento = elemento)
                Dim v2 = replicas.FirstOrDefault(Function(r) r.Elemento = elemento)

                If v1 Is Nothing OrElse v2 Is Nothing Then Continue For

                Dim diferenca = Math.Abs(Math.Round(v1.Valor - v2.Valor, 4))

                Dim tolerancia As Double = 0
                Select Case elemento
                    Case "Matéria Volátil"
                        tolerancia = 0.36
                    Case "Teor de Cinzas"
                        If tipoAmostra.Contains("CARV") Then
                            tolerancia = 0.19
                        ElseIf tipoAmostra.Contains("COQUE") Then
                            tolerancia = 0.09
                        End If
                End Select

                If diferenca > tolerancia Then
                    reprovados.Add(v1)
                    reprovados.Add(v2)
                End If
            Next
        Next

        Return reprovados
    End Function


End Module
