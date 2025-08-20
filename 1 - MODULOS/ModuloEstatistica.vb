Imports System.IO
Imports System.Data.SQLite
Imports System.Globalization

Public Module ModuloEstatistica

    Public Function AplicarControleEstatisticoProcesso(
        ByRef dados As List(Of ResultadoAnalise),
        ByRef TeveAlteracao As Boolean,
        ByRef prefixoFolio As String,
        tipoAmostra As String) As Boolean

        Dim alertasPorResultado As New Dictionary(Of ResultadoAnalise, List(Of ResultadoAnalise))
        Dim dadosMedia As List(Of ResultadoAnalise) = GerarMediaResultadosDuplicados(dados)

        Using conn As New SQLiteConnection("Data Source=C:\Sigma-Q\SigmaQ - TGA.db;Version=3;")
            conn.Open()

            Dim analise As New AnaliseEstatistica(conn)



            For Each resultado In dadosMedia

                Dim anoAtual1 = Now.Year Mod 100
                Dim anoSufixo = anoAtual1.ToString("D2")

                If prefixoFolio.EndsWith(anoSufixo) Then
                    prefixoFolio = prefixoFolio.Substring(0, prefixoFolio.Length - 2)
                End If



                Dim resultadoTendencia = analise.AnalisarTendencia(prefixoFolio, resultado.Elemento, resultado.Valor, resultado.Folio, resultado.TipoAmostra)
                Dim resultadoDistribuicao = analise.AnalisarDistribuicao(prefixoFolio, resultado.Elemento, resultado.Valor, resultado.Folio, resultado.TipoAmostra)

                Dim listaAlertas As New List(Of ResultadoAnalise)

                If Not resultadoTendencia.Alerta.Contains("OK") Then
                    listaAlertas.Add(New ResultadoAnalise With {
                        .Mensagem = resultadoTendencia.Alerta,
                        .TipoAmostra = resultadoTendencia.TipoAmostra,
                        .TipoAnalise = "Tendência"
                    })
                End If

                If Not resultadoDistribuicao.Alerta.Contains("OK") Then
                    listaAlertas.Add(New ResultadoAnalise With {
                        .Mensagem = resultadoDistribuicao.Alerta,
                        .TipoAmostra = resultadoDistribuicao.TipoAmostra,
                        .TipoAnalise = "Distribuição"
                    })
                End If

                If listaAlertas.Count > 0 Then
                    alertasPorResultado(resultado) = listaAlertas
                    TeveAlteracao = True
                End If
            Next
        End Using

        If alertasPorResultado.Count > 0 Then
            Dim caminhoPdf As String = ""
            Dim dataAtual = Date.Now
            Dim pastaRaiz = "C:\Sigma-Q\Report de Desvios"
            Dim ano = dataAtual.Year.ToString()
            Dim mes = dataAtual.ToString("MM-MMMM", New CultureInfo("pt-BR"))
            Dim dia = dataAtual.ToString("dd")
            Dim pastaDestino = Path.Combine(pastaRaiz, ano, mes, dia)
            Directory.CreateDirectory(pastaDestino)

            Dim timestamp = dataAtual.ToString("yyyyMMdd_HHmmss")
            caminhoPdf = Path.Combine(pastaDestino, $"RelatorioDesvios_{timestamp}.pdf")



            RelatorioDesvios.Gerar(dadosMedia, alertasPorResultado, caminhoPdf)

            FrmPrincipal.TimerMonitoramento.Stop()
            Try
                Dim frm As New FrmVisualizadorPdf()
                frm.CarregarPDF(caminhoPdf)
                frm.ShowDialog()
            Catch ex As Exception
                MsgBox("ERRO: " & ex.Message)
            End Try
            FrmPrincipal.TimerMonitoramento.Start()

            ' ======================================
            ' DESCOMENTE PARA BLOQUEAR RESULTADOS CRÍTICOS
            ' ======================================
            'For Each resultadoCritico In alertasPorResultado.Keys.ToList()
            '    If alertasPorResultado(resultadoCritico).Any(Function(a) a.Mensagem.Contains("CRÍTICO")) Then
            '        dados.Remove(resultadoCritico)
            '    End If
            'Next
        End If

        Return True
    End Function

End Module
