Imports System.Data.SQLite

Public Class RepositorioResultados
    Private ReadOnly _dbPath As String = "C:\Sigma-Q\SigmaQ.db"

    Public Sub SalvarResultado(resultado As ResultadoAnalise, conn As SQLiteConnection)
        ' Extrair prefixo do folio (ex: ZETAAICI25)
        Dim prefixoFolio As String = resultado.Folio.Split("-"c)(0)

        Dim anoAtual As Integer = Date.Now.Year
        Dim ultimosDoisDigitos As String = (anoAtual Mod 100).ToString("D2")

        If prefixoFolio.EndsWith(ultimosDoisDigitos) Then
            prefixoFolio = prefixoFolio.Substring(0, prefixoFolio.Length - 2)
        End If

        ' Buscar TipoAmostra correspondente
        Dim tipoAmostra As String = ""
        Dim cmdBusca As New SQLiteCommand("SELECT TipoAmostra FROM TipoAmostra WHERE UPPER(InicioFolio) = UPPER(@inicio)", conn)
        cmdBusca.Parameters.AddWithValue("@inicio", prefixoFolio)
        Dim resultadoBusca = cmdBusca.ExecuteScalar()

        If resultadoBusca IsNot Nothing Then
            tipoAmostra = resultadoBusca.ToString()
        Else
            tipoAmostra = "Tipo de Amostra Não Cadastrado"
        End If

        ' Inserir na tabela ResultadoAnalise
        Dim cmdInsert As New SQLiteCommand("
        INSERT INTO ResultadoProcessado 
        (TipoAmostra, Folio, Elemento, Valor, DataProcessamento, DataLeitura) 
        VALUES (@tipo, @folio, @elemento, @valor, @dataProc, @dataLeitura)", conn)

        cmdInsert.Parameters.AddWithValue("@tipo", tipoAmostra)
        cmdInsert.Parameters.AddWithValue("@folio", UCase(resultado.Folio))
        cmdInsert.Parameters.AddWithValue("@elemento", resultado.Elemento)

        Dim a = resultado.Valor
        If Double.IsNaN(a) Then
            cmdInsert.Parameters.AddWithValue("@valor", DBNull.Value)
        Else
            cmdInsert.Parameters.AddWithValue("@valor", a)
        End If


        cmdInsert.Parameters.AddWithValue("@dataProc", Now.ToString("yyyy-MM-dd HH:mm:ss"))
        cmdInsert.Parameters.AddWithValue("@dataLeitura", Now.ToString("yyyy-MM-dd HH:mm:ss"))

        cmdInsert.ExecuteNonQuery()
    End Sub
End Class
