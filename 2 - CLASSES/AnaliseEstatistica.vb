Imports System.Data.SQLite

Public Class AnaliseEstatistica
    Private ReadOnly _conn As SQLiteConnection

    Public Sub New(conn As SQLiteConnection)
        _conn = conn
    End Sub

    Public Function AnalisarTendencia(inicioFolio As String, elemento As String, valorAtual As Double, Folio As String, TipoAmostraRecebida As String) As ResultadoAnalise
        Try

            Dim folioTratado As String = Folio.Trim().ToUpper()

            Dim cmd As SQLiteCommand
            cmd = New SQLiteCommand("
            SELECT Folio, Valor, TipoAmostra, Elemento
            FROM ResultadoProcessado 
            WHERE Folio LIKE @inicio || '%' AND Elemento = @elemento 
            ORDER BY DataProcessamento DESC 
            LIMIT 10", _conn)
            cmd.Parameters.AddWithValue("@inicio", inicioFolio) 'ESSE FUNCIONA BEM
            cmd.Parameters.AddWithValue("@elemento", elemento)

            Dim analisados As New Dictionary(Of String, RegistroLido)
            Dim tipoAmostraFinal As String = ""

            Using reader = cmd.ExecuteReader()
                While reader.Read()
                    Dim foliolido As String = reader("Folio").ToString().Trim().ToUpper()
                    Dim elementoLido As String = reader("Elemento").ToString().Trim().ToUpper()
                    Dim chaveRegistro As String = foliolido & "|" & elementoLido

                    If Not analisados.ContainsKey(chaveRegistro) Then
                        Dim valor As Double
                        If Double.TryParse(reader("Valor").ToString(), valor) Then
                            Dim tipoAmostra As String = reader("TipoAmostra").ToString()
                            If foliolido.EndsWith(" S") Then
                                tipoAmostra &= " - SOLÚVEL"
                            End If

                            analisados.Add(chaveRegistro, New RegistroLido With {
                            .Folio = foliolido,
                            .Elemento = elementoLido,
                            .Valor = valor,
                            .TipoAmostra = tipoAmostra
                        })

                            If tipoAmostraFinal = "" Then tipoAmostraFinal = tipoAmostra
                        End If
                    End If
                End While
            End Using

            If analisados.Count = 0 Then
                Return New ResultadoAnalise With {
                .Alerta = "Sem dados anteriores",
                .TipoAmostra = TipoAmostraRecebida,
                .Valor = valorAtual,
                .Elemento = elemento,
                .Folio = Folio,
                .TipoAnalise = "Tendência"
            }
            End If

            ' Separa os valores em normais e solúveis
            Dim valoresNormais = analisados.Values.
            Where(Function(r) Not r.Folio.EndsWith(" S")).Select(Function(r) r.Valor).ToList()

            Dim valoresSoluveis = analisados.Values.
            Where(Function(r) r.Folio.EndsWith(" S")).Select(Function(r) r.Valor).ToList()

            ' Decide qual média usar com base no folio atual
            Dim media As Double
            Dim valoresUsados As List(Of Double)

            If Folio.Trim().ToUpper().EndsWith(" S") Then
                valoresUsados = valoresSoluveis
            Else
                valoresUsados = valoresNormais
            End If

            If valoresUsados.Count = 0 Then
                Return New ResultadoAnalise With {
            .Alerta = "Sem dados anteriores do mesmo tipo (normal/solúvel)",
            .TipoAmostra = tipoAmostraFinal,
            .Valor = valorAtual,
            .Elemento = elemento,
            .Folio = Folio,
            .TipoAnalise = "Tendência"
        }
            End If

            media = valoresUsados.Average()





            Dim atualFmt = valorAtual.ToString("F3")
            Dim mediaFmt = media.ToString("F3")
            Dim alerta As String


            If valorAtual > media * 1.2 Then
                alerta = $"Alerta: Valor acima da média (Atual: {atualFmt}, Média: {mediaFmt})"
            ElseIf valorAtual < media * 0.8 Then
                alerta = $"Alerta: Valor abaixo da média (Atual: {atualFmt}, Média: {mediaFmt})"
            Else
                alerta = $"OK: Valor dentro da média (Atual: {atualFmt}, Média: {mediaFmt})"
            End If

            Return New ResultadoAnalise With {
            .Alerta = alerta,
            .TipoAmostra = tipoAmostraFinal,
            .Valor = valorAtual,
            .Elemento = elemento,
            .Folio = Folio,
            .TipoAnalise = "Tendência"
        }

        Catch ex As Exception
            MsgBox("ERRO: " & ex.Message & vbCrLf & "Detalhes: " & ex.StackTrace)
            My.Computer.Clipboard.SetText(ex.ToString())
        End Try
    End Function
    Public Function AnalisarDistribuicao(inicioFolio As String, elemento As String, valorAtual As Double, Folio As String, TipoAmostraRecebida As String) As ResultadoAnalise
        Try

            ' Identifica se é solúvel
            Dim folioTratado As String = Folio.Trim().ToUpper()


            ' Extrai prefixo base do folio (ex: ZETESBIOC25)
            Dim folioBase As String = folioTratado.Split("-"c)(0)
            Dim grupoAmostra As String = folioBase

            ' Consulta os dados apenas do mesmo prefixo base (evita misturar grupos)
            Dim cmd As New SQLiteCommand("
        SELECT Folio, Valor, TipoAmostra 
        FROM ResultadoProcessado 
        WHERE Folio LIKE @prefixo || '%' AND Elemento = @elemento", _conn)

            cmd.Parameters.AddWithValue("@prefixo", folioBase)
            cmd.Parameters.AddWithValue("@elemento", elemento)

            ' Armazenar valores por grupo de tipo de amostra
            Dim registrosPorGrupo As New Dictionary(Of String, List(Of Double))
            Dim tipoAmostraFinal As String = ""

            Using reader = cmd.ExecuteReader()
                While reader.Read()
                    Dim foliolido As String = reader("Folio").ToString().Trim().ToUpper()
                    Dim valor As Double
                    If Not Double.TryParse(reader("Valor").ToString(), valor) Then Continue While

                    Dim tipoAmostra As String = reader("TipoAmostra").ToString()
                    If foliolido.EndsWith(" S") Then
                        tipoAmostra &= " - SOLÚVEL"
                    End If

                    Dim grupoLido As String = foliolido.Split("-"c)(0)
                    If foliolido.EndsWith(" S") Then grupoLido &= " - SOLÚVEL"

                    If tipoAmostraFinal = "" AndAlso grupoLido = grupoAmostra Then
                        tipoAmostraFinal = tipoAmostra
                    End If

                    If Not registrosPorGrupo.ContainsKey(grupoLido) Then
                        registrosPorGrupo(grupoLido) = New List(Of Double)
                    End If
                    registrosPorGrupo(grupoLido).Add(valor)
                End While
            End Using

            ' Verifica se há dados suficientes do grupo correto
            If Not registrosPorGrupo.ContainsKey(grupoAmostra) OrElse registrosPorGrupo(grupoAmostra).Count < 5 Then
                Return New ResultadoAnalise With {
                .Alerta = "Dados insuficientes para análise estatística robusta.",
                .TipoAmostra = tipoAmostraFinal,
                .Valor = valorAtual,
                .Elemento = elemento,
                .TipoAnalise = "Distibuição"
            }
            End If

            ' Ordenar os valores
            Dim valores = registrosPorGrupo(grupoAmostra)
            valores.Sort()

            ' Calcular Q1, Q3 e IQR
            Dim q1 = Percentil(valores, 0.25)
            Dim q3 = Percentil(valores, 0.75)
            Dim iqr = q3 - q1

            ' Remoção de outliers
            Dim filtrados = valores.Where(Function(x) x >= q1 - 1.5 * iqr AndAlso x <= q3 + 1.5 * iqr).ToList()

            If filtrados.Count < 3 Then
                Return New ResultadoAnalise With {
                .Alerta = "Após remoção de outliers, dados insuficientes para análise.",
                .TipoAmostra = tipoAmostraFinal,
                .Valor = valorAtual,
                .Elemento = elemento,
                .TipoAnalise = "Distibuição"
            }
            End If

            ' Mediana e MAD
            Dim mediana = Percentil(filtrados, 0.5)
            Dim mad = filtrados.Select(Function(x) Math.Abs(x - mediana)).Average()
            Dim diff = Math.Abs(valorAtual - mediana)

            Dim alerta As String
            If diff > 2 * mad Then
                alerta = $"Alerta CRÍTICO: Valor fora de 2×MAD (Atual: {valorAtual:F3}, Mediana: {mediana:F3}, MAD: {mad:F3})"
            ElseIf diff > mad Then
                alerta = $"Alerta: Valor entre 1 e 2×MAD (Atual: {valorAtual:F3}, Mediana: {mediana:F3}, MAD: {mad:F3})"
            Else
                alerta = $"OK: Valor dentro de 1×MAD (Atual: {valorAtual:F3}, Mediana: {mediana:F3}, MAD: {mad:F3})"
            End If

            Return New ResultadoAnalise With {
            .Alerta = alerta,
            .TipoAmostra = tipoAmostraFinal,
            .Valor = valorAtual,
            .Elemento = elemento,
            .TipoAnalise = "Distibuição"
        }
        Catch ex As Exception
            MsgBox("ERRO: " & ex.Message & vbCrLf & "Detalhes: " & ex.StackTrace)
        End Try
    End Function

    Private Function Percentil(valores As List(Of Double), p As Double) As Double
        If valores Is Nothing OrElse valores.Count = 0 Then Return Double.NaN

        valores.Sort()
        Dim n = (valores.Count - 1) * p
        Dim k = CInt(Math.Floor(n))
        Dim d = n - k

        If k + 1 < valores.Count Then
            Return valores(k) + d * (valores(k + 1) - valores(k))
        Else
            Return valores(k)
        End If
    End Function


End Class
