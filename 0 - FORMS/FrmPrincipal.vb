Imports System.Data.SQLite
Imports System.IO

Public Class FrmPrincipal
    Dim processamentoEmAndamento As Boolean = False
    Private Shared ReadOnly dbLock As New Object()


    Private Const SenhaCorreta As String = "33954430"
    Private Sub FrmPrincipal_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        ProcessadorDeResultados.EnderecoPP = TxtEnderecoPP.Text


        Using conn As New SQLiteConnection("Data Source=C:\Sigma-Q\SigmaQ - TGA.db;Version=3;")
            conn.Open()
            Using cmd As New SQLiteCommand("PRAGMA journal_mode=WAL;", conn)
                cmd.ExecuteNonQuery()
            End Using
        End Using


        'ENDEREÇO DA PASTA E SUB PASTA
        '==========================================================================
        Dim basePath As String = "C:\Sigma-Q"
        Dim subDirs As String() = {"0 - Recebida", "1 - Processada", "2 - Backup"}
        '==========================================================================


        ' VERIFICAR SE EXISTE UMA PASTA ESPECIFICA E SE NÃO CRIA UMA
        '===========================================================
        If Not Directory.Exists(basePath) Then
            Directory.CreateDirectory(basePath)
        End If
        '===========================================================



        ' VERIFICAR SE EXISTEM AS SUB-PASTAS ESPECIFICAS 
        '===========================================================
        For Each dir As String In subDirs
            Dim fullPath As String = Path.Combine(basePath, dir)
            If Not Directory.Exists(fullPath) Then
                Directory.CreateDirectory(fullPath)
            End If
        Next
        '===========================================================

        ' Oculta o formulário e exibe apenas o ícone na bandeja
        Me.ShowInTaskbar = False
        Me.WindowState = FormWindowState.Minimized
        Me.Visible = False

        NotifyIcon1.Visible = True
        NotifyIcon1.Text = "Sigma-Q Monitor"
        NotifyIcon1.ContextMenuStrip = ContextMenuStrip1

        TimerMonitoramento.Start()


    End Sub

    'PEGAR O ARQUIVO DE TXT CASO ELE EXISTA
    '******************************************************************************************************************
    Private Sub TimerMonitoramento_Tick_1(sender As Object, e As EventArgs) Handles TimerMonitoramento.Tick
        Dim origemDir = "C:\Sigma-Q\0 - Recebida"
        Dim origemDir2 = "C:\Sigma-Q\1 - Processada"
        Dim destinoDir = "C:\Sigma-Q\2 - Backup"
        Dim destinoDir2 = TxtEnderecoPP.Text

        If processamentoEmAndamento Then Exit Sub


        ' Pega todos os arquivos .txt na pasta de origem
        Dim arquivos = Directory.GetFiles(origemDir, "*.csv")


        For Each arquivo In arquivos
            Try
                ProcessadorDeResultados.ENDERECOARQUIVO = arquivo

                processamentoEmAndamento = True
                EscreverLog("Arquivo encontrado: " & Path.GetFileName(arquivo))

                Dim dadosProcessados As List(Of ResultadoAnalise) = LerArquivoAnalise(arquivo)

                Dim dadosAnalise As List(Of ResultadoAnalise) = dadosProcessados.Select(Function(r) New ResultadoAnalise With {
                    .Folio = r.Folio,
                    .Elemento = r.Elemento,
                    .Valor = r.Valor}).ToList()
                '.Data = r.DataProcessamento ' ← COMENTA ESSA LINHA PARA COMPILAR

                Dim connString = "Data Source=C:\Sigma-Q\SigmaQ - TGA.db;Version=3;"
                SyncLock dbLock
                    Using c As New SQLiteConnection(connString)
                        c.Open()
                        Dim ProcessarDados As New ProcessadorDeResultados
                        ProcessarDados.Executar(dadosAnalise, c)
                        EscreverLog("Arquivo processado com sucesso: " & Path.GetFileName(arquivo))
                        '=========================================
                        '======
                    End Using
                End SyncLock


                ' Gera novo nome com data/hora atual
                Dim novoNome = "Resultados do TGA - " & Now.ToString("dd-MM-yyyy HH-mm-ss") & " - " & Guid.NewGuid.ToString("N") & ".csv"
                Dim destinoCompleto = Path.Combine(destinoDir, novoNome)

                ' Move o arquivo com novo nome
                File.Move(arquivo, destinoCompleto)


                EscreverLog("Arquivo movido para backup: " & novoNome)
            Catch ex As Exception
                EscreverLog("Erro ao processar o arquivo: " & ex.Message)
                My.Computer.Clipboard.SetText(ex.ToString)
            Finally
                processamentoEmAndamento = False
            End Try
        Next

        ' PEGAR ARQUIVOS PROCESSADOS
        '=========================================================================================================================================================
        Dim arquivos2 = Directory.GetFiles(origemDir2, "*.csv")

        For Each arquivo In arquivos2
            Try
                processamentoEmAndamento = True
                EscreverLog("Arquivo encontrado: " & Path.GetFileName(arquivo))

                ' Gera novo nome com data/hora atual
                Dim novoNome = "Resultados do TGA Processados- " & Now.ToString("dd-MM-yyyy HH-mm-ss") & " - " & Guid.NewGuid.ToString("N") & ".CSV"
                Dim destinoCompleto = Path.Combine(destinoDir2, novoNome)
                Dim destinoCompleto2 = Path.Combine("C:\Sigma-Q\1 - Processada\0 - Backup", novoNome)

                ' Move o arquivo com novo nome
                File.Copy(arquivo, destinoCompleto2)
                File.Move(arquivo, destinoCompleto)


                EscreverLog("Arquivo movido para Piso de Planta: " & novoNome)
            Catch ex As Exception
                EscreverLog("Erro ao processar o arquivo: " & ex.Message)
                My.Computer.Clipboard.SetText(ex.ToString)
            Finally
                processamentoEmAndamento = False
            End Try
        Next
        '=========================================================================================================================================================

    End Sub
    '******************************************************************************************************************

    Public Function LerArquivoAnalise(caminhoArquivo As String) As List(Of ResultadoAnalise)
        Dim linhas = File.ReadAllLines(caminhoArquivo)
        Dim resultados As New List(Of ResultadoAnalise)

        For Each linha In linhas
            If String.IsNullOrWhiteSpace(linha) Then Continue For

            Dim partes = linha.Split(";"c)
            If partes.Length <> 5 Then Continue For

            Dim folioCompleto = partes(0).Trim().ToUpper()
            Dim dataProcessamento = Date.Now

            ' Dicionário com os nomes dos elementos e valores correspondentes
            Dim elementos = New Dictionary(Of String, String) From {
            {"Umidade Higroscópica", partes(1)},
            {"Matéria Volátil", partes(2)},
            {"Teor de Cinzas", partes(3)},
            {"Carbono Fixo", partes(4)}
        }

            For Each par In elementos
                Dim valor As Double
                If Double.TryParse(par.Value, Globalization.NumberStyles.Any, Globalization.CultureInfo.InvariantCulture, valor) Then
                    resultados.Add(New ResultadoAnalise With {
                    .Folio = folioCompleto,
                    .Elemento = par.Key,
                    .Valor = valor,
                    .DataProcessamento = dataProcessamento
                })
                End If
            Next
        Next

        Return resultados
    End Function



    '******************************************************************************************************************
    Public Sub EscreverLog(mensagem As String)
        Dim dataHora As String = Now.ToString("HH:mm:ss")
        txtLog.AppendText($"[{dataHora}] {mensagem}{Environment.NewLine}")
    End Sub
    '******************************************************************************************************************

    Private Sub AbrirToolStripMenuItem_Click(sender As Object, e As EventArgs)
        Dim senha = InputBox("Digite a senha para abrir:", "Autenticação")

        If senha = SenhaCorreta Then
            Visible = True
            WindowState = FormWindowState.Normal
            ShowInTaskbar = True
        Else
            MessageBox.Show("Senha incorreta!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End If
    End Sub

    Private Sub SairToolStripMenuItem_Click(sender As Object, e As EventArgs)
        NotifyIcon1.Visible = False
        Application.Exit()
    End Sub

    Private Sub Form1_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        ' Se o fechamento for por clique no botão X ou ALT+F4
        If e.CloseReason = CloseReason.UserClosing Then
            e.Cancel = True ' Cancela o fechamento
            Me.Hide()       ' Oculta a janela
            NotifyIcon1.Visible = True
            NotifyIcon1.BalloonTipTitle = "Sigma-Q Monitor"
            NotifyIcon1.BalloonTipText = "O sistema continuará rodando em segundo plano."
            NotifyIcon1.ShowBalloonTip(3000)
        End If
    End Sub


End Class
