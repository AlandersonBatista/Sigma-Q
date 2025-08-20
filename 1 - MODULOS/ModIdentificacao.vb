Imports System.Data.SQLite
Imports System.Text.RegularExpressions

Public Module ModIdentificacao

    ''' <summary>
    ''' Extrai o código base do folio, removendo o sufixo alfabético ao final (ex: "010001227680-0010CC" → "010001227680-0010").
    ''' </summary>
    Public Function ExtrairFolioSemSufixo(folio As String) As String
        If String.IsNullOrWhiteSpace(folio) Then Return ""

        Dim index As Integer = folio.IndexOf("-0010", StringComparison.OrdinalIgnoreCase)
        If index = -1 Then Return folio ' Se não encontrar "-0010", retorna o folio original

        ' Retorna até o final do "-0010", sem o sufixo que vem depois
        Return folio.Substring(0, index + 5)
    End Function


    ''' <summary>
    ''' Extrai o sufixo do folio, caso seja do tipo com prefixo numérico.
    ''' </summary>
    Public Function ExtrairSufixoTipo(folio As String) As String
        If String.IsNullOrWhiteSpace(folio) Then Return ""

        Dim index As Integer = folio.IndexOf("-0010", StringComparison.OrdinalIgnoreCase)
        If index = -1 OrElse index + 5 >= folio.Length Then Return ""

        Return folio.Substring(index + 5).ToUpper()
    End Function

    ''' <summary>
    ''' Extrai o prefixo do folio, caso seja do tipo com prefixo em letras (ex: ZETESBIOC25).
    ''' </summary>
    Public Function ExtrairPrefixoFolio(folio As String) As String
        Dim prefixo As String = folio.Split("-"c)(0)

        ' Remove final de ano se for ex: ZETESBIOC25 → remove "25"
        Dim anoAtual As Integer = Date.Now.Year
        Dim ultimosDoisDigitos As String = (anoAtual Mod 100).ToString("D2")

        If prefixo.EndsWith(ultimosDoisDigitos) Then
            ' Remove os dois últimos dígitos do ano
            prefixo = prefixo.Substring(0, prefixo.Length - 2)

            ' Verifica se os dois últimos caracteres restantes ainda são numéricos
            If prefixo.Length >= 2 AndAlso IsNumeric(prefixo.Substring(prefixo.Length - 2)) Then
                ' Remove mais dois caracteres numéricos
                prefixo = prefixo.Substring(0, prefixo.Length - 2)
            End If
        End If


        Return prefixo.ToUpper()
    End Function

    ''' <summary>
    ''' Identifica se o folio começa com número (ex: 010001227792...).
    ''' </summary>
    Public Function FolioComecaComNumero(folio As String) As Boolean
        Return Not String.IsNullOrEmpty(folio) AndAlso Char.IsDigit(folio(0))
    End Function

    ''' <summary>
    ''' Retorna o tipo de amostra e o valor da chave de identificação (sufixo ou prefixo).
    ''' </summary>
    Public Function ObterTipoAmostra(folio As String, conexao As SQLiteConnection) As ResultadoIdentificacao
        If String.IsNullOrWhiteSpace(folio) Then
            Return New ResultadoIdentificacao With {
                .TipoAmostra = "Folio Inválido",
                .ChaveIdentificacao = "",
                .FolioSemSufixo = folio,
                .Folio = folio
            }
        End If

        If FolioComecaComNumero(folio) Then
            Dim sufixo = ExtrairSufixoTipo(folio)
            If String.IsNullOrEmpty(sufixo) Then
                Return New ResultadoIdentificacao With {
                    .TipoAmostra = "Sufixo de Amostra Não Identificado",
                    .ChaveIdentificacao = "",
                    .Folio = folio
                }
            End If

            Using cmd As New SQLiteCommand("SELECT TipoAmostra FROM TipoAmostra WHERE UPPER(InicioFolio) = UPPER(@sufixo)", conexao)
                cmd.Parameters.AddWithValue("@sufixo", sufixo)
                Dim resultado = cmd.ExecuteScalar()

                Return New ResultadoIdentificacao With {
                    .TipoAmostra = If(resultado IsNot Nothing, resultado.ToString(), "Tipo de Amostra Não Cadastrado"),
                    .ChaveIdentificacao = sufixo,
                    .FolioSemSufixo = ExtrairFolioSemSufixo(folio),
                    .Folio = folio
                }
            End Using
        Else
            Dim prefixo = ExtrairPrefixoFolio(folio)

            Using cmd As New SQLiteCommand("SELECT TipoAmostra FROM TipoAmostra WHERE UPPER(InicioFolio) LIKE UPPER(@prefixo || '%')", conexao)
                cmd.Parameters.AddWithValue("@prefixo", prefixo)
                Dim resultado = cmd.ExecuteScalar()

                Return New ResultadoIdentificacao With {
                    .TipoAmostra = If(resultado IsNot Nothing, resultado.ToString(), "Tipo de Amostra Não Cadastrado"),
                    .ChaveIdentificacao = prefixo,
                    .FolioSemSufixo = folio,
                    .Folio = folio
                }
            End Using
        End If
    End Function


End Module
