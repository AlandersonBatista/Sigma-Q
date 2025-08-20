Imports System.IO
Imports iTextSharp.text
Imports iTextSharp.text.pdf

Public Class RelatorioDesvios
    Public Shared Sub Gerar(dados As List(Of ResultadoAnalise),
                        alertas As Dictionary(Of ResultadoAnalise, List(Of ResultadoAnalise)),
                        caminhoPDF As String)

        If String.IsNullOrEmpty(caminhoPDF) Then
            Throw New Exception("O caminho do PDF está vazio ou nulo.")
        End If

        Dim doc As Document = Nothing
        Dim fs As FileStream = Nothing

        Try
            doc = New Document(PageSize.A4)
            fs = New FileStream(caminhoPDF, FileMode.Create, FileAccess.Write, FileShare.None)
            Dim writer = PdfWriter.GetInstance(doc, fs)
            doc.Open()

            Dim fonteCabecalho As New Font(Font.FontFamily.HELVETICA, 14, Font.BOLD)
            Dim fonteTabela As New Font(Font.FontFamily.HELVETICA, 10, Font.NORMAL)
            Dim fonteCabecalhoTabela As New Font(Font.FontFamily.HELVETICA, 10, Font.BOLD)

            doc.Add(New Paragraph("LISTA DE DESVIOS OBSERVADOS", fonteCabecalho) With {.Alignment = Element.ALIGN_CENTER})
            doc.Add(New Paragraph("LABORATÓRIO CENTRAL", fonteCabecalho) With {.Alignment = Element.ALIGN_CENTER})
            doc.Add(New Paragraph(" "))
            doc.Add(New Paragraph(" "))

            Dim grupos = alertas.GroupBy(Function(a) a.Value(0).TipoAmostra)

            For Each grupo In grupos

                doc.Add(New Paragraph(" "))
                doc.Add(New Paragraph($"🧪 Tipo de Amostra: {grupo.Key}", fonteCabecalhoTabela))

                Dim tabelaTendencia As New PdfPTable(4)
                tabelaTendencia.WidthPercentage = 100
                tabelaTendencia.SetWidths({25, 25, 20, 30})
                tabelaTendencia.AddCell(New PdfPCell(New Phrase("FOLIO", fonteCabecalhoTabela)))
                Dim cellElemento As New PdfPCell(New Phrase("ELEMENTO", fonteCabecalhoTabela)) With {.HorizontalAlignment = Element.ALIGN_CENTER}
                tabelaTendencia.AddCell(cellElemento)
                Dim cellResultado As New PdfPCell(New Phrase("RESULTADO", fonteCabecalhoTabela)) With {.HorizontalAlignment = Element.ALIGN_CENTER}
                tabelaTendencia.AddCell(cellResultado)
                tabelaTendencia.AddCell(New PdfPCell(New Phrase("DESVIO OBSERVADO", fonteCabecalhoTabela)))

                Dim tabelaEstatistica As New PdfPTable(4)
                tabelaEstatistica.WidthPercentage = 100
                tabelaEstatistica.SetWidths({25, 25, 20, 30})
                tabelaEstatistica.AddCell(New PdfPCell(New Phrase("FOLIO", fonteCabecalhoTabela)))
                tabelaEstatistica.AddCell(New PdfPCell(New Phrase("ELEMENTO", fonteCabecalhoTabela)) With {.HorizontalAlignment = Element.ALIGN_CENTER})
                tabelaEstatistica.AddCell(New PdfPCell(New Phrase("RESULTADO", fonteCabecalhoTabela)) With {.HorizontalAlignment = Element.ALIGN_CENTER})
                tabelaEstatistica.AddCell(New PdfPCell(New Phrase("DESVIO OBSERVADO", fonteCabecalhoTabela)))

                Dim temTendencia As Boolean = False
                Dim temDistribuicao As Boolean = False

                For Each entrada In grupo
                    Dim r = entrada.Key
                    For Each alerta In entrada.Value
                        If alerta.TipoAnalise = "Tendência" Then
                            tabelaTendencia.AddCell(New Phrase(r.Folio, fonteTabela))
                            tabelaTendencia.AddCell(New Phrase(r.Elemento, fonteTabela))
                            tabelaTendencia.AddCell(New Phrase(r.Valor.ToString("0.###"), fonteTabela))
                            tabelaTendencia.AddCell(New Phrase(alerta.Mensagem, fonteTabela))
                            temTendencia = True
                        ElseIf alerta.TipoAnalise = "Distribuição" Then
                            tabelaEstatistica.AddCell(New Phrase(r.Folio, fonteTabela))
                            tabelaEstatistica.AddCell(New Phrase(r.Elemento, fonteTabela))
                            tabelaEstatistica.AddCell(New Phrase(r.Valor.ToString("0.###"), fonteTabela))

                            Dim mensagem = alerta.Mensagem
                            Dim celulaMensagem As New PdfPCell(New Phrase(mensagem, fonteTabela))

                            If mensagem.StartsWith("Alerta CRÍTICO:") Then
                                celulaMensagem.BackgroundColor = BaseColor.RED
                                celulaMensagem.Phrase = New Phrase(mensagem, New Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.WHITE))
                            End If

                            tabelaEstatistica.AddCell(celulaMensagem)


                            temDistribuicao = True
                        End If
                    Next
                Next

                doc.Add(New Paragraph(" "))
                doc.Add(New Paragraph("📊 Análise de Tendência (Últimos 3 Resultados)", fonteCabecalhoTabela))
                doc.Add(New Paragraph(" "))
                If temTendencia Then
                    doc.Add(tabelaTendencia)
                Else
                    doc.Add(New Paragraph("✔ Nenhum desvio observado na análise de tendência.", fonteTabela))
                End If

                doc.Add(New Paragraph(" "))
                doc.Add(New Paragraph("📈 Análise Estatística (Curva de Gauss)", fonteCabecalhoTabela))
                doc.Add(New Paragraph(" "))
                If temDistribuicao Then
                    doc.Add(tabelaEstatistica)
                ElseIf temTendencia Then
                    doc.Add(New Paragraph("📌 Os resultados não apresentaram desvio estatístico, mas demonstraram tendência fora da média nos últimos 3 resultados. Amostra considerada dentro do controle de normalidade, porém deve ser monitorada.", fonteTabela))
                Else
                    doc.Add(New Paragraph("✔ Nenhum desvio observado na análise estatística.", fonteTabela))
                End If

                doc.Add(New Paragraph(" "))
            Next
        Catch ex As Exception
            MessageBox.Show("Erro ao gerar o PDF: " & ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            If doc IsNot Nothing AndAlso doc.IsOpen Then doc.Close()
            If fs IsNot Nothing Then fs.Close()
        End Try
    End Sub
End Class
