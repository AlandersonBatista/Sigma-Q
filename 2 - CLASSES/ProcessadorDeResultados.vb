Imports System.Data.SQLite
Imports System.Globalization
Imports System.IO
Imports System.Text

Public Class ProcessadorDeResultados
    Public Shared ENDERECOARQUIVO As String
    Public Shared EnderecoPP As String
    Private Shared ReadOnly dbLock As New Object()
    Public Sub Executar(dados As List(Of ResultadoAnalise), c As SQLiteConnection)
        Dim dadosFiltrados = dados

#Region "INICIO DOS TRATAMENTOS DE DADOS - PEGAR O TIPO DE AMOSTRA"
        Dim tipoAmostra As String
        Dim sufixoOuPrefixo As String
        Dim folioSemSufixo As String
        Dim foliooriginal As String

        Dim foliosBaseUnicos = dados.Select(Function(d)
                                                Dim f = d.Folio
                                                If f.Contains("[") Then
                                                    Return f.Substring(0, f.IndexOf("["))
                                                Else
                                                    Return f
                                                End If
                                            End Function).Distinct().ToList()

        For Each folioBase In foliosBaseUnicos
            Dim resultado = ModIdentificacao.ObterTipoAmostra(folioBase, c)

            tipoAmostra = resultado.TipoAmostra
            sufixoOuPrefixo = resultado.ChaveIdentificacao
            folioSemSufixo = resultado.FolioSemSufixo
            foliooriginal = resultado.Folio

            For Each item In dados
                ' Só altera se FolioSemSufixo for válido (não vazio e não nulo)
                If Not String.IsNullOrWhiteSpace(resultado.FolioSemSufixo) Then

                    If item.Folio.StartsWith(folioSemSufixo) Then
                        If item.Folio = folioSemSufixo & "[A]" Then
                            item.TipoAmostra = tipoAmostra
                        Else
                            item.Folio = folioSemSufixo
                            item.TipoAmostra = tipoAmostra
                        End If
                    End If
                Else
                    If item.Folio = foliooriginal & "[A]" Then
                        item.TipoAmostra = tipoAmostra
                    Else
                        item.Folio = foliooriginal
                        item.TipoAmostra = tipoAmostra
                    End If

                End If
            Next
        Next

#End Region

        ' Garante que há pelo menos um dado
        If dados Is Nothing OrElse dados.Count = 0 Then Exit Sub
        Dim TeveAlteracao As Boolean = False 'VARIAVEL PARA VERIFICAR SE TEVE ALTERAÇÃO 

#Region "1. VERIFICAR SE OS MÉTODOS ESTÃO DENTRO DO DESVIO ACEITAVEL PARA VOLATEIS E CINZAS"
        '****************************************************************************************************



        Dim reprovados = ObterResultadosReprovados(dados)

        If reprovados IsNot Nothing AndAlso reprovados.Count > 0 Then

            ' Zera os valores reprovados
            For Each r In reprovados
                r.Valor = Double.NaN ' ou use 0 se o resto do código tratar corretamente
            Next


            Dim caminhoPdf As String = ""
            Dim dataAtual = Date.Now
            Dim pastaRaiz = "C:\Sigma-Q\Resultado Fora do Limite"
            Dim ano = dataAtual.Year.ToString()
            Dim mes = dataAtual.ToString("MM-MMMM", New CultureInfo("pt-BR"))
            Dim dia = dataAtual.ToString("dd")
            Dim pastaDestino = Path.Combine(pastaRaiz, ano, mes, dia)
            Directory.CreateDirectory(pastaDestino)

            Dim timestamp = dataAtual.ToString("yyyyMMdd_HHmmss")
            caminhoPdf = Path.Combine(pastaDestino, $"RelatorioDesvios_{timestamp}.pdf")

            ' Monta um dicionário simulado de alertas, apenas com mensagem básica
            Dim alertasSimples As New Dictionary(Of ResultadoAnalise, List(Of ResultadoAnalise))

            For Each r In reprovados
                alertasSimples(r) = New List(Of ResultadoAnalise) From {
            New ResultadoAnalise With {
                .Folio = r.Folio,
                .Elemento = r.Elemento,
                .Valor = r.Valor,
                .Mensagem = "Valor fora da tolerância",
                .TipoAnalise = "Limite"
                    }
                }
            Next

            RelatorioAlertasGerais.Gerar(alertasSimples, caminhoPdf)

            dadosFiltrados = dados.Except(reprovados).ToList()


            Try
                Dim frm As New FrmVisualizadorPdf()
                frm.CarregarPDF(caminhoPdf)
                frm.ShowDialog()
            Catch ex As Exception
                MsgBox("ERRO: " & ex.Message)
            End Try

        End If

        '****************************************************************************************************
#End Region

        ' Agrupar os resultados por Folio e Elemento
        Dim agrupados = dados.GroupBy(Function(r) r.Folio).ToDictionary(Function(g) g.Key, Function(g) g.ToList())


#Region "1ª REGRA DO SISTEMA - PERCORRER OS FOLIOS E TROCAR VALORES NEGATIVOS OU ZERADOS POR 0,1"
        '========================================================================================================

        TeveAlteracao = ModRegrasDeAjuste.AplicarRegraValoresNegativosOuZerados(agrupados) Or TeveAlteracao

        '========================================================================================================
#End Region

        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance)

#Region "3ª REGRA CONTROLE ESTATÍSTICO DO PROCESSO"
        '=============================================================================================================

        ModuloEstatistica.AplicarControleEstatisticoProcesso(dados, TeveAlteracao, sufixoOuPrefixo, tipoAmostra)

        '=============================================================================================================
#End Region


#Region "SALVAR OS RESULTADOS NA PASTA C:\Sigma-Q\1 - Processada "
        If TeveAlteracao Then
            ' Caminhos de saída
            Dim pastaSaida As String = "C:\Sigma-Q\1 - Processada"
            Dim pastaSaidaPisoPlanta As String = EnderecoPP

            If Not Directory.Exists(pastaSaida) Then Directory.CreateDirectory(pastaSaida)
            If Not Directory.Exists(pastaSaidaPisoPlanta) Then Directory.CreateDirectory(pastaSaidaPisoPlanta)

            Dim nomeArquivo As String = "Resultados do TGA Processados - " & Now.ToString("dd-MM-yyyy HH-mm-ss") & " - " & Guid.NewGuid().ToString("N") & ".csv"
            Dim caminhoCompleto As String = Path.Combine(pastaSaida, nomeArquivo)
            Dim caminhoCompletoPisoPlanta As String = Path.Combine(pastaSaidaPisoPlanta, nomeArquivo)

            ' Agrupar por folio
            Dim agrupadosPorFolio = dados.GroupBy(Function(r) r.Folio)

            Using writer As New StreamWriter(caminhoCompleto, False, Encoding.UTF8)

                For Each grupo In agrupadosPorFolio
                    Dim folio = grupo.Key

                    ' Cria um dicionário auxiliar para armazenar os valores dos 4 elementos fixos
                    Dim elementosDict As New Dictionary(Of String, String) From {
                        {"Umidade Higroscópica", ""},
                        {"Matéria Volátil", ""},
                        {"Teor de Cinzas", ""},
                        {"Carbono Fixo", ""}
                    }

                    ' Preenche com os valores encontrados no grupo
                    For Each resultado In grupo
                        If elementosDict.ContainsKey(resultado.Elemento) Then
                            elementosDict(resultado.Elemento) = resultado.Valor.ToString(Globalization.CultureInfo.InvariantCulture)
                        End If
                    Next

                    ' Monta a linha conforme o formato original
                    Dim linha As String = $"{folio};" &
                                          $"{elementosDict("Umidade Higroscópica")};" &
                                          $"{elementosDict("Matéria Volátil")};" &
                                          $"{elementosDict("Teor de Cinzas")};" &
                                          $"{elementosDict("Carbono Fixo")}"

                    writer.WriteLine(linha)
                Next

            End Using


            'File.Copy(caminhoCompleto, caminhoCompletoPisoPlanta, True)

        Else

            ' Caminhos de saída
            Dim pastaSaida As String = "C:\Sigma-Q\1 - Processada"
            Dim pastaSaidaPisoPlanta As String = EnderecoPP

            If Not Directory.Exists(pastaSaida) Then Directory.CreateDirectory(pastaSaida)
            If Not Directory.Exists(pastaSaidaPisoPlanta) Then Directory.CreateDirectory(pastaSaidaPisoPlanta)

            Dim nomeArquivo As String = "Resultados do TGA - " & Now.ToString("dd-MM-yyyy HH-mm-ss") & " - " & Guid.NewGuid().ToString("N") & ".csv"
            Dim caminhoCompleto As String = Path.Combine(pastaSaida, nomeArquivo)
            Dim caminhoCompletoPisoPlanta As String = Path.Combine(pastaSaidaPisoPlanta, nomeArquivo)

            ' Agrupar por folio
            Dim agrupadosPorFolio = dados.GroupBy(Function(r) r.Folio)

            ' Criar o arquivo apenas uma vez
            Using writer As New StreamWriter(caminhoCompleto, False, Encoding.UTF8)
                For Each grupo In agrupadosPorFolio
                    Dim folio = grupo.Key

                    ' Cria um dicionário auxiliar para armazenar os valores dos 4 elementos fixos
                    Dim elementosDict As New Dictionary(Of String, String) From {
                        {"Umidade Higroscópica", ""},
                        {"Matéria Volátil", ""},
                        {"Teor de Cinzas", ""},
                        {"Carbono Fixo", ""}
                    }

                    ' Preenche com os valores encontrados no grupo
                    For Each resultado In grupo
                        If elementosDict.ContainsKey(resultado.Elemento) Then
                            elementosDict(resultado.Elemento) = resultado.Valor.ToString(Globalization.CultureInfo.InvariantCulture)
                        End If
                    Next

                    ' Monta a linha conforme o formato original
                    Dim linha As String = $"{folio};" &
                                          $"{elementosDict("Umidade Higroscópica")};" &
                                          $"{elementosDict("Matéria Volátil")};" &
                                          $"{elementosDict("Teor de Cinzas")};" &
                                          $"{elementosDict("Carbono Fixo")}"

                    writer.WriteLine(linha)
                Next
            End Using



            'File.Copy(caminhoCompleto, caminhoCompletoPisoPlanta, True)
        End If
#End Region


#Region "SALVAR RESULTADOS NO BANCO DE DADOS"
        Dim repo As New RepositorioResultados()

        'SALVAR ARQUIVO NO BANCO DE DADOS!
        '=======================================
        Dim repor As New RepositorioResultados()
        Dim connString = "Data Source=C:\Sigma-Q\SigmaQ - TGA.db;Version=3;"
        SyncLock dbLock
            Using conn As New SQLiteConnection(connString)
                conn.Open()

                For Each r In dadosfiltrados
                    repor.SalvarResultado(r, conn)
                Next

            End Using
        End SyncLock
        '=======================================
#End Region


    End Sub

    Public Function ObterElementosEsperados(inicioFolio As String, diaSemana As String) As List(Of String)
        Dim connString = "Data Source=C:\Sigma-Q\SigmaQ.db;Version=3;"
        Dim elementos As New List(Of String)

        Dim anoAtual As Integer = Date.Now.Year
        Dim ultimosDoisDigitos As String = (anoAtual Mod 100).ToString("D2")

        If inicioFolio.EndsWith(ultimosDoisDigitos) Then
            inicioFolio = inicioFolio.Substring(0, inicioFolio.Length - 2)
        End If

        Using conn As New SQLiteConnection(connString)
            conn.Open()

            ' Força a comparação em UPPER para evitar qualquer conflito de case
            Dim cmd As New SQLiteCommand("
            SELECT Elemento 
            FROM ElementosPorDiaSemana 
            WHERE UPPER(InicioFolio) = UPPER(@folio) 
            AND UPPER(DiaSemana) = UPPER(@dia)", conn)

            cmd.Parameters.AddWithValue("@folio", inicioFolio)
            cmd.Parameters.AddWithValue("@dia", diaSemana)
            Dim reader = cmd.ExecuteReader()

            If reader.Read() Then
                Dim texto = reader("Elemento").ToString()
                elementos = texto.Split(","c).Select(Function(e) e.Trim()).ToList()
            End If
        End Using

        Return elementos
    End Function
    Public Sub EscreverLog(mensagem As String)
        Dim dataHora As String = Now.ToString("HH:mm:ss")
        FrmPrincipal.txtLog.AppendText($"[{dataHora}] {mensagem}{Environment.NewLine}")
    End Sub
End Class
