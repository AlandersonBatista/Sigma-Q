Imports System.IO
Imports iTextSharp.text
Imports iTextSharp.text.pdf

Public Class RelatorioAlertasGerais
    Public Shared Sub Gerar(alertasPorResultado As Dictionary(Of ResultadoAnalise, List(Of ResultadoAnalise)), caminhoPDF As String)

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

            ' Fontes
            Dim fonteTitulo As New Font(Font.FontFamily.HELVETICA, 14, Font.BOLD)
            Dim fonteCabecalho As New Font(Font.FontFamily.HELVETICA, 10, Font.BOLD)
            Dim fonteNormal As New Font(Font.FontFamily.HELVETICA, 10, Font.NORMAL)

            ' Cabeçalho
            doc.Add(New Paragraph("RELATÓRIO DE ALERTAS DE RESULTADOS", fonteTitulo) With {.Alignment = Element.ALIGN_CENTER})
            doc.Add(New Paragraph("LABORATÓRIO CENTRAL", fonteTitulo) With {.Alignment = Element.ALIGN_CENTER})
            doc.Add(New Paragraph(" "))
            doc.Add(New Paragraph(" "))

            ' Agrupamento por Tipo de Amostra
            Dim grupos = alertasPorResultado.GroupBy(Function(a) a.Key.TipoAmostra)

            For Each grupo In grupos
                doc.Add(New Paragraph($"🧪 Tipo de Amostra: {grupo.Key}", fonteCabecalho))
                doc.Add(New Paragraph(" "))

                ' Tabela
                Dim tabela As New PdfPTable(4)
                tabela.WidthPercentage = 100
                tabela.SetWidths({20, 20, 20, 40})

                tabela.AddCell(New PdfPCell(New Phrase("FOLIO", fonteCabecalho)))
                tabela.AddCell(New PdfPCell(New Phrase("ELEMENTO", fonteCabecalho)))
                tabela.AddCell(New PdfPCell(New Phrase("VALOR", fonteCabecalho)))
                tabela.AddCell(New PdfPCell(New Phrase("ALERTA", fonteCabecalho)))

                For Each entrada In grupo
                    Dim resultado = entrada.Key
                    For Each alerta In entrada.Value
                        tabela.AddCell(New PdfPCell(New Phrase(resultado.Folio, fonteNormal)))
                        tabela.AddCell(New PdfPCell(New Phrase(resultado.Elemento, fonteNormal)))
                        tabela.AddCell(New PdfPCell(New Phrase(resultado.Valor.ToString("0.###"), fonteNormal)))

                        ' Formatação para alerta crítico
                        Dim mensagem = alerta.Mensagem
                        Dim celulaMensagem As New PdfPCell(New Phrase(mensagem, fonteNormal))

                        If mensagem.StartsWith("Alerta CRÍTICO:") Then
                            celulaMensagem.BackgroundColor = BaseColor.RED
                            celulaMensagem.Phrase = New Phrase(mensagem, New Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.WHITE))
                        End If

                        tabela.AddCell(celulaMensagem)
                    Next
                Next

                doc.Add(tabela)
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
